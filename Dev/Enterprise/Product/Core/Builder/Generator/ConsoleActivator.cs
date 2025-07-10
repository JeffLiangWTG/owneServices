using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Enterprise.Builder.Generator
{
	public static class ConsoleActivator
	{
		public static void EnsureConsole()
		{
			if (NativeMethods.AttachConsole(NativeMethods.ATTACH_PARENT_PROCESS))
			{
				// We've attached to the existing console.
				// Nothing more to do here.
				return;
			}

			var errorCode = Marshal.GetLastWin32Error();

			switch (errorCode)
			{
				case NativeMethods.ERROR_ACCESS_DENIED:
					// The calling process is already attached to a console.
					return;

				case NativeMethods.ERROR_INVALID_HANDLE:
					// The calling process does not have a console.
					// Let's create a new one:
					if (NativeMethods.AllocConsole())
					{
						// Nothing more to do here.
						break;
					}

					// We failed to create our own console.
					throw new Win32Exception();

				default:
					throw new Win32Exception(errorCode);
			}
		}

		static class NativeMethods
		{
			// BOOL WINAPI AllocConsole(void);
			[DllImport("kernel32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool AllocConsole();

			// BOOL WINAPI AttachConsole(
			//   _In_ DWORD dwProcessId
			// );
			[DllImport("kernel32.dll", SetLastError = true)]
			[return: MarshalAs(UnmanagedType.Bool)]
			public static extern bool AttachConsole(uint dwProcessId);

			public const uint ATTACH_PARENT_PROCESS = 0xFFFFFFFF;
			public const int ERROR_ACCESS_DENIED = 5;
			public const int ERROR_INVALID_HANDLE = 6;
		}
	}
}
