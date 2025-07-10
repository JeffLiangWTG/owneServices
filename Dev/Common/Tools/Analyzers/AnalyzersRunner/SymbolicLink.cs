using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace AnalyzersRunner
{
	/*
	 *
	 * contents of this region was adapted from: https://stackoverflow.com/a/9882819
	 *	and https://web.archive.org/web/20130213213634/http://troyparsons.com/blog/2012/03/symbolic-links-in-c-sharp/
	 *
	 */
	static class SymbolicLink
	{
		/// 
		/// Refer to http://msdn.microsoft.com/en-us/library/windows/hardware/ff552012%28v=vs.85%29.aspx
		/// 
		[StructLayout(LayoutKind.Sequential)]
		public struct SymbolicLinkReparseData
		{
			// Not certain about this!
			const int maxUnicodePathLength = 260 * 2;

			public uint ReparseTag;
			public ushort ReparseDataLength;
			public ushort Reserved;
			public ushort SubstituteNameOffset;
			public ushort SubstituteNameLength;
			public ushort PrintNameOffset;
			public ushort PrintNameLength;
			public uint Flags;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = maxUnicodePathLength)]
			public byte[] PathBuffer;
		}

		const uint genericReadAccess = 0x80000000;

		const uint fileFlagsForOpenReparsePointAndBackupSemantics = 0x02200000;

		const int ioctlCommandGetReparsePoint = 0x000900A8;

		const uint openExisting = 0x3;

		const uint pathNotAReparsePointError = 0x80071126;

		const uint shareModeAll = 0x7; // Read, Write, Delete

		const uint symLinkTag = 0xA000000C;

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern SafeFileHandle CreateFile(
			string lpFileName,
			uint dwDesiredAccess,
			uint dwShareMode,
			IntPtr lpSecurityAttributes,
			uint dwCreationDisposition,
			uint dwFlagsAndAttributes,
			IntPtr hTemplateFile);

		[Flags]
		public enum SYMBOLIC_LINK_FLAG
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
		public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SYMBOLIC_LINK_FLAG dwFlags);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool DeviceIoControl(
			IntPtr hDevice,
			uint dwIoControlCode,
			IntPtr lpInBuffer,
			int nInBufferSize,
			IntPtr lpOutBuffer,
			int nOutBufferSize,
			out int lpBytesReturned,
			IntPtr lpOverlapped);

		public static void CreateDirectoryLink(string linkPath, string targetPath)
		{
			if (!CreateSymbolicLink(linkPath, targetPath, SYMBOLIC_LINK_FLAG.Directory) || Marshal.GetLastWin32Error() != 0)
			{
				try
				{
					Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
				}
				catch (COMException exception)
				{
					throw new IOException(exception.Message, exception);
				}
			}
		}

		public static void CreateFileLink(string linkPath, string targetPath)
		{
			if (!CreateSymbolicLink(linkPath, targetPath, SYMBOLIC_LINK_FLAG.File))
			{
				Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}

		public static bool Exists(string path)
		{
			string target = GetTarget(path);
			return target != null;
		}

		static SafeFileHandle getFileHandle(string path)
		{
			return CreateFile(path, genericReadAccess, shareModeAll, IntPtr.Zero, openExisting,
				fileFlagsForOpenReparsePointAndBackupSemantics, IntPtr.Zero);
		}

		public static bool TryGetTarget(string path, out string linkTarget)
		{
			linkTarget = GetTarget(path);

			return linkTarget != null;
		}

		public static string GetTarget(string path)
		{
			if (!Directory.Exists(path) && !File.Exists(path))
			{
				return null;
			}

			SymbolicLinkReparseData reparseDataBuffer;

			using (SafeFileHandle fileHandle = getFileHandle(path))
			{
				if (fileHandle.IsInvalid)
				{
					Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
				}

				int outBufferSize = Marshal.SizeOf(typeof(SymbolicLinkReparseData));
				IntPtr outBuffer = IntPtr.Zero;
				try
				{
					outBuffer = Marshal.AllocHGlobal(outBufferSize);
					int bytesReturned;
					bool success = DeviceIoControl(
						fileHandle.DangerousGetHandle(), ioctlCommandGetReparsePoint, IntPtr.Zero, 0,
						outBuffer, outBufferSize, out bytesReturned, IntPtr.Zero);

					fileHandle.Close();

					if (!success)
					{
						unchecked
						{
							if (((uint)Marshal.GetHRForLastWin32Error()) == pathNotAReparsePointError)
							{
								return null;
							}
						}
						Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
					}

					reparseDataBuffer = (SymbolicLinkReparseData)Marshal.PtrToStructure(
						outBuffer, typeof(SymbolicLinkReparseData));
				}
				finally
				{
					Marshal.FreeHGlobal(outBuffer);
				}
			}
			if (reparseDataBuffer.ReparseTag != symLinkTag)
			{
				return null;
			}

			string target = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer,
				reparseDataBuffer.PrintNameOffset, reparseDataBuffer.PrintNameLength);

			return target;
		}
	}
}
