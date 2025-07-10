using System;
using System.Runtime.InteropServices;

namespace Enterprise.FaxRouter.Processor
{
	public static class TIFFSDKWrapper
	{
		public const int TCOMP_JPEG = 207;

		[DllImport("Tiff32.dll")]
		public static extern int GetNumberOfImagesInTiffFile(string aFileName);

		[DllImport("Tiff32.dll")]
		public static extern bool LoadTiffFileIntoClipboard(string aFileName, int aImageNumber, bool aDialog);

		[DllImport("Tiff32.dll")]
		public static extern IntPtr LoadTiffFileIntoBitmap(string aFileName, int aImageNumber, bool aDialog);

		[DllImport("Tiff32.dll")]
		public static extern IntPtr LoadTiffIntoDIB(string aFileName, int aImageNumber, bool aDialog);

		[DllImport("Tiff32.dll")]
		public static extern bool SaveDIBInTiffFile(string aFileName, IntPtr aDIB, uint aCompression, bool aDialog);
	}
}
