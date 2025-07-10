using System;
using System.Runtime.InteropServices;

namespace Enterprise.ReflectionTest
{
	static class NativeMethods
	{
		#region CreateSymbolicLink p/invoke

		[Flags]
		internal enum SYMBOLIC_LINK_FLAG
		{
			File = 0,
			Directory = 1,
			AllowUnprivilegedCreate = 2
		}

		/// <summary>
		/// see: https://learn.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-createsymboliclinka
		/// </summary>
		/// <param name="lpSymlinkFileName">The symbolic link to be created.</param>
		/// <param name="lpTargetFileName">The name of the target for the symbolic link to be created.</param>
		/// <param name="dwFlags">Indicates whether the link target, lpTargetFileName, is a directory.</param>
		/// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.To get extended error information, call GetLastError.</returns>
		/// <remarks>
		///	pinvoke definition from: https://www.pinvoke.net/default.aspx/kernel32/CreateSymbolicLink.html
		/// </remarks>
		[DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		[return: MarshalAs(UnmanagedType.I1)]
		internal static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SYMBOLIC_LINK_FLAG dwFlags);

		#endregion CreateSymbolicLink p/invoke
	}
}
