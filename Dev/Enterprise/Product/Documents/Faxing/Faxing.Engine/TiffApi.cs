using System;
using System.Runtime.InteropServices;

namespace Enterprise.Faxing.Engine
{
	public static class TiffApi
	{
		[DllImport("tiff32.dll", CharSet = CharSet.Ansi)]
		public static extern IntPtr OpenTiffFile(string fileName, int mode);

		[DllImport("tiff32.dll", CharSet = CharSet.Ansi)]
		public static extern bool CloseTiffFile(IntPtr tiffHandle);

		[DllImport("tiff32.dll", CharSet = CharSet.Ansi)]
		public static extern bool GetTiffImage(IntPtr tiffHandle, int imageIndex);

		[DllImport("tiff32.dll", CharSet = CharSet.Ansi)]
		public static extern bool DropTiffImage(IntPtr tiffHandle, int imageIndex);

		[DllImport("tiff32.dll", CharSet = CharSet.Ansi)]
		public static extern int GetTiffDimensions(IntPtr tiffHandle, int imageIndex, out BITMAPINFOHEADER bih);

		[DllImport("Tiff32.dll", CharSet = CharSet.Ansi)]
		public static extern int GetNumberOfImagesInTiffFile(string fileName);
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct BITMAPINFOHEADER
	{
		public uint biSize;
		public int biWidth;
		public int biHeight;
		public ushort biPlanes;
		public ushort biBitCount;
		public uint biCompression;
		public uint biSizeImage;
		public int biXPelsPerMeter;
		public int biYPelsPerMeter;
		public uint biClrUsed;
		public uint biClrImportant;
	}
}
