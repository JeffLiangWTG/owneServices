using System;
using System.Runtime.InteropServices;
using System.Text;

namespace CargoWise.Loader.Common.Native
{
	public sealed class NativeMethods : INativeMethods
	{
		public const int CSIDL_COMMON_DESKTOPDIRECTORY = 0x0019; // All Users\Desktop
		public const int CSIDL_COMMON_PROGRAMS = 0x0017;         // All Users\Start Menu\Programs
		public const int CSIDL_DESKTOPDIRECTORY = 0x0010;        // <user name>\Desktop
		public const int CSIDL_FLAG_CREATE = 0x8000;
		public const int CSIDL_FONTS = 0x0014;
		public const int CSIDL_PROGRAMS = 0x0002;                // <user name>\Start Menu\Programs
		public const int CSIDL_WINDOWS = 0x0024;
		public static readonly IntPtr HWND_BROADCAST = new IntPtr(0xFFFF);
		public const int MAX_PATH = 260;
		public const int SHGFP_TYPE_CURRENT = 0;
		public const uint TOKEN_QUERY = 0x0008;
		public const int WM_FONTCHANGE = 0x001D;

		public NativeMethods()
		{
		}

		[DllImport("gdi32.dll")]
		static extern int AddFontResource(
			[MarshalAs(UnmanagedType.LPWStr)] string lpszFilename);

		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool CloseHandle(IntPtr hObject);

		[DllImport("kernel32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		extern static bool FreeLibrary(IntPtr hLibModule);

		[DllImport("kernel32.dll", SetLastError = true)]
		static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);

		[DllImport("mscoree.dll")]
		static extern int GetFileVersion(
			[MarshalAs(UnmanagedType.LPWStr)] string szFilename,
			[MarshalAs(UnmanagedType.LPWStr)] StringBuilder szBuffer,
			int cchBuffer,
			out int dwLength);

		[DllImport("kernel32")]
		extern static IntPtr GetModuleHandle(string lpModuleName);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern int GetSystemWindowsDirectory(StringBuilder lpBuffer, int uSize);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetTokenInformation(
			IntPtr TokenHandle,
			TOKEN_INFORMATION_CLASS TokenInformationClass,
			IntPtr TokenInformation,
			uint TokenInformationLength,
			out uint ReturnLength);

		[DllImport("kernel32")]
		extern static IntPtr LoadLibrary(string lpLibFileName);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool OpenProcessToken(IntPtr ProcessHandle, UInt32 DesiredAccess, out int TokenHandle);

		[DllImport("shfolder.dll", CharSet = CharSet.Auto)]
		static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);

		[DllImport("mscoree.dll")]
		static extern void StrongNameFreeBuffer(IntPtr pbMemory);

		[DllImport("mscoree.dll")]
		[return: MarshalAs(UnmanagedType.U1)]
		static extern bool StrongNameSignatureVerificationEx(
			[MarshalAs(UnmanagedType.LPWStr)] string wszFilePath,
			[MarshalAs(UnmanagedType.U1)] bool fForceVerification,
			[MarshalAs(UnmanagedType.U1)] out bool pfWasVerified);

		[DllImport("mscoree.dll")]
		[return: MarshalAs(UnmanagedType.U1)]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return")]
		static extern bool StrongNameTokenFromAssembly(
			[MarshalAs(UnmanagedType.LPWStr)] string wszFilePath,
			out IntPtr ppbStrongNameToken,
			out int pcbStrongNameToken);

		#region INativeMethods Members

		int INativeMethods.AddFontResource(string lpszFilename)
		{
			return AddFontResource(lpszFilename);
		}

		void INativeMethods.CloseHandle(IntPtr hObject)
		{
			CloseHandle(hObject);
		}

		bool INativeMethods.FreeLibrary(IntPtr hLibModule)
		{
			return FreeLibrary(hLibModule);
		}

		IntPtr INativeMethods.GetCurrentProcess()
		{
			return GetCurrentProcess();
		}

		bool INativeMethods.GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes)
		{
			return GetDiskFreeSpaceEx(lpDirectoryName, out lpFreeBytesAvailable, out lpTotalNumberOfBytes, out lpTotalNumberOfFreeBytes);
		}

		int INativeMethods.GetFileVersion(string szFilename, StringBuilder szBuffer, int cchBuffer, out int dwLength)
		{
			return GetFileVersion(szFilename, szBuffer, cchBuffer, out dwLength);
		}

		IntPtr INativeMethods.GetModuleHandle(string lpModuleName)
		{
			return GetModuleHandle(lpModuleName);
		}

		int INativeMethods.GetSystemWindowsDirectory(StringBuilder lpBuffer, int uSize)
		{
			return GetSystemWindowsDirectory(lpBuffer, uSize);
		}

		bool INativeMethods.GetTokenInformation(IntPtr TokenHandle, TOKEN_INFORMATION_CLASS TokenInformationClass, IntPtr TokenInformation, uint TokenInformationLength, out uint ReturnLength)
		{
			return GetTokenInformation(TokenHandle, TokenInformationClass, TokenInformation, TokenInformationLength, out ReturnLength);
		}

		IntPtr INativeMethods.LoadLibrary(string lpLibFileName)
		{
			return LoadLibrary(lpLibFileName);
		}

		bool INativeMethods.OpenProcessToken(IntPtr ProcessHandle, uint DesiredAccess, out int TokenHandle)
		{
			return OpenProcessToken(ProcessHandle, DesiredAccess, out TokenHandle);
		}

		int INativeMethods.SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath)
		{
			return SHGetFolderPath(hwndOwner, nFolder, hToken, dwFlags, lpszPath);
		}

		void INativeMethods.StrongNameFreeBuffer(IntPtr pbMemory)
		{
			StrongNameFreeBuffer(pbMemory);
		}

		bool INativeMethods.StrongNameSignatureVerificationEx(string wszFilePath, bool fForceVerification, out bool pfWasVerified)
		{
			return StrongNameSignatureVerificationEx(wszFilePath, fForceVerification, out pfWasVerified);
		}

		bool INativeMethods.StrongNameTokenFromAssembly(string wszFilePath, out IntPtr ppbStrongNameToken, out int pcbStrongNameToken)
		{
			return StrongNameTokenFromAssembly(wszFilePath, out ppbStrongNameToken, out pcbStrongNameToken);
		}

		#endregion
	}
}
