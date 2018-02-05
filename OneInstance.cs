using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace AnimatedWindow
{
    public class OneInstance
    {
        [DllImport("User32.dll", EntryPoint = "ShowWindow", CharSet = CharSet.Auto)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("User32.dll", EntryPoint = "UpdateWindow", CharSet = CharSet.Auto)]
        private static extern bool UpdateWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "IsIconic", CharSet = CharSet.Auto)]
        private static extern bool IsIconic(IntPtr hWnd);

        private const int WM_QUERYENDSESSION = 0x11;
        private const int IDANI_CAPTION = 3;
        private const int SW_HIDE = 0;
        private const int SW_MAX = 10;
        private const int SW_MAXIMIZE = 3;
        private const int SW_MINIMIZE = 6;
        private const int SW_NORMAL = 1;
        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;
        private const int SW_SHOWDEFAULT = 10;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMINNOACTIVE = 7;
        private const int SW_SHOWNA = 8;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_SHOWNORMAL = 1;

        static Mutex mutex = null;
        private static MemoryMappedFile sharedMemory;
        public static bool IsRunning()
        {
            // Used to check if we can create a new mutex
            bool newMutexCreated = false;
            // The name of the mutex is to be prefixed with Local\ to make sure that its is created in the per-session namespace, not in the global namespace.
            string mutexName = "Local\\" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;

                // Create a new mutex object with a unique name
            mutex = new Mutex(false, mutexName, out newMutexCreated);

            return !newMutexCreated;
        }

        //public static void KeepActive()
        //{
        //    GC.KeepAlive(mutex);
        //}
        public static void Release()
        {
            GC.KeepAlive(mutex);
            try
            {
                mutex.ReleaseMutex();
            }
            catch (ApplicationException ex)
            {
                GC.Collect();
            }
        }

        public static void CreateMMF(string fileName)
        {
            sharedMemory = MemoryMappedFile.CreateMMF(fileName, MemoryMappedFile.FileAccess.ReadWrite, 8);
        }

        public static void WriteHandle(IntPtr mainWindowHandle)
        {
            //Write the handle to the Shared Memory 
            sharedMemory.WriteHandle(mainWindowHandle);
        }

        public static IntPtr ReadHandle(string fileName)
        {
            return MemoryMappedFile.ReadHandle(fileName);
        }

        public static void ShowWindow(IntPtr mainWindowHandle)
        {
            if (mainWindowHandle != IntPtr.Zero)
            {
                if (IsIconic(mainWindowHandle))
                    ShowWindow(mainWindowHandle, SW_SHOWNORMAL); // Restore the Window 
                else
                    ShowWindow(mainWindowHandle, SW_RESTORE); // Restore the Window 

                UpdateWindow(mainWindowHandle);
            }
        }
    }
}
