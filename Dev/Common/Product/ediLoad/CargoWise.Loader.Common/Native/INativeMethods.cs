using System;
using System.Text;

namespace CargoWise.Loader.Common.Native
{
	public interface INativeMethods
	{
		void CloseHandle(IntPtr hObject);
		bool FreeLibrary(IntPtr hLibModule);
		IntPtr GetCurrentProcess();
		bool GetDiskFreeSpaceEx(string lpDirectoryName, out ulong lpFreeBytesAvailable, out ulong lpTotalNumberOfBytes, out ulong lpTotalNumberOfFreeBytes);
		int GetFileVersion(string szFilename, StringBuilder szBuffer, int cchBuffer, out int dwLength);
		IntPtr GetModuleHandle(string lpModuleName);
		int GetSystemWindowsDirectory(StringBuilder lpBuffer, int uSize);
		bool GetTokenInformation(IntPtr tokenHandle, TOKEN_INFORMATION_CLASS tokenInformationClass, IntPtr tokenInformation, uint tokenInformationLength, out uint returnLength);
		IntPtr LoadLibrary(string lpLibFileName);
		bool OpenProcessToken(IntPtr processHandle, uint desiredAccess, out int tokenHandle);
		int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken, int dwFlags, StringBuilder lpszPath);
		void StrongNameFreeBuffer(IntPtr pbMemory);
		bool StrongNameSignatureVerificationEx(string wszFilePath, bool fForceVerification, out bool pfWasVerified);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		bool StrongNameTokenFromAssembly(string wszFilePath, out IntPtr ppbStrongNameToken, out int pcbStrongNameToken);

		int AddFontResource(string lpszFilename);
	}
}
