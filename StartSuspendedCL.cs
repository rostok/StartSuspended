/*
* just a simple conversion of http://www.codeproject.com/Articles/230005/Launch-a-process-suspended into a command line app
*/

using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StartSuspendedCL
{
    [Flags]
    public enum ProcessCreationFlags : uint
    {
        ZERO_FLAG = 0x00000000,
        CREATE_BREAKAWAY_FROM_JOB = 0x01000000,
        CREATE_DEFAULT_ERROR_MODE = 0x04000000,
        CREATE_NEW_CONSOLE = 0x00000010,
        CREATE_NEW_PROCESS_GROUP = 0x00000200,
        CREATE_NO_WINDOW = 0x08000000,
        CREATE_PROTECTED_PROCESS = 0x00040000,
        CREATE_PRESERVE_CODE_AUTHZ_LEVEL = 0x02000000,
        CREATE_SEPARATE_WOW_VDM = 0x00001000,
        CREATE_SHARED_WOW_VDM = 0x00001000,
        CREATE_SUSPENDED = 0x00000004,
        CREATE_UNICODE_ENVIRONMENT = 0x00000400,
        DEBUG_ONLY_THIS_PROCESS = 0x00000002,
        DEBUG_PROCESS = 0x00000001,
        DETACHED_PROCESS = 0x00000008,
        EXTENDED_STARTUPINFO_PRESENT = 0x00080000,
        INHERIT_PARENT_AFFINITY = 0x00010000
    }

    public struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }

    public struct STARTUPINFO
    {
        public uint cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public uint dwX;
        public uint dwY;
        public uint dwXSize;
        public uint dwYSize;
        public uint dwXCountChars;
        public uint dwYCountChars;
        public uint dwFillAttribute;
        public uint dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    public static class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
                                 bool bInheritHandles, ProcessCreationFlags dwCreationFlags, IntPtr lpEnvironment,
                                string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(IntPtr hThread);

        [DllImport("kernel32.dll")]
        public static extern uint SuspendThread(IntPtr hThread);
    }

    
    public class App
    {
        
        static IntPtr ThreadHandle = IntPtr.Zero;

        public static bool LaunchProcessSuspended(string processpath, int initialResumeTime, out uint PID)
        {
            STARTUPINFO si = new STARTUPINFO();
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            bool success = NativeMethods.CreateProcess(processpath, null, IntPtr.Zero, IntPtr.Zero, false, ProcessCreationFlags.CREATE_SUSPENDED, IntPtr.Zero, null, ref si, out pi);
            ThreadHandle = pi.hThread;
            PID = pi.dwProcessId;

            if (initialResumeTime > 0)
            {
                NativeMethods.ResumeThread(ThreadHandle);
                System.Threading.Thread.Sleep(initialResumeTime);
                NativeMethods.SuspendThread(ThreadHandle);
            }

            return success;
        }

        public static void ResumeProcess()
        {
            NativeMethods.ResumeThread(ThreadHandle);
        }


        [STAThread]
        static void Main(string[] args)
        {
        	if (args.Length==0)
        	{
        		Console.WriteLine("StartSuspendedCL.exe app.exe [suspension time in ms]");
				Environment.Exit(0);
        	} 
        	int wait = 100;
        	if (args.Length>=2) wait = Int32.Parse(args[1]);
   			Console.WriteLine(wait);

   			uint PID = 0;
   			string filename = args[0];
   			bool started = LaunchProcessSuspended(filename, wait, out PID);

            if (started)
                MessageBox.Show(
                    String.Format("The Process started with PID {0}.\r\nClose this dialog to resume it.", PID),
                    "Process started",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show(String.Format("Error launching file {0}", filename),
                    "Error launching file",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

            ResumeProcess();
        }
    }
}
