using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using CargoWise.Common;
using CargoWise.Interop;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI.RichEdit
{
	internal class ZMetaFileUtil
	{
		/// <summary>
		/// Get a .NET Metafile object from the METAFILEPICT. The underlying metafile handle is released by this method.
		/// </summary>
		internal static Metafile ConvertMetaPictToDNetMetaFile(NativeMethods.METAFILEPICT metaPict)
		{
			var pBuffer = Marshal.AllocCoTaskMem(10000);
			var error = UnsafeNativeMethods.GetMetaFileBitsEx(metaPict.hEmf, 10000, pBuffer);
			if (error != 0)
			{
				var ex = new Win32Exception(int.Parse(error.ToString(CultureInfo.InvariantCulture),CultureInfo.InvariantCulture), "ZMetaFileUtil.cs and UnsafeNativeMethods.GetMetaFileBitsEx() have returned an error");
				ErrorReporter.ReportOnce("", ex);
			}

			var pMetaPict = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(NativeMethods.METAFILEPICT)));
			IntPtr result;
			try
			{
				Marshal.StructureToPtr(metaPict, pMetaPict, false);
				result = UnsafeNativeMethods.SetWinMetaFileBits(10000, pBuffer, IntPtr.Zero, pMetaPict);
			}
			finally
			{
				Marshal.FreeCoTaskMem(pMetaPict);
			}
			return new Metafile(result, true);
		}

		internal static Bitmap GetIconFromIconicMetafile(Metafile meta, Size metaFileDimensions)
		{
			var scaledSize = GetMetafilePixelDimensions(meta);

			var bmp = new Bitmap(scaledSize.Width, scaledSize.Height, PixelFormat.Format32bppArgb);
			var g = Graphics.FromImage(bmp);

			g.FillRectangle(Brushes.Transparent, 0, 0, bmp.Width, bmp.Height);
			g.DrawImage(meta, ControlDpiScalingHelper.NewScaledRectangle(0, 0, bmp.Width, bmp.Height, false));

			// carve out the top, centre section of the metafile which will be the icon
			var result = bmp.Clone(ControlDpiScalingHelper.NewScaledRectangle((bmp.Width - ControlDpiScalingHelper.ScaleToCurrentDpiX(32)) / 2, 0, ControlDpiScalingHelper.ScaleToCurrentDpiX(32), ControlDpiScalingHelper.ScaleToCurrentDpiY(32), false), bmp.PixelFormat);

			return result;
		}

		internal static Icon GetIconSuitableOleIconApiCallsLike(Icon ico)
		{
			var iconStream = new MemoryStream();
			ico.Save(iconStream);
			var buffer = iconStream.ToArray();
			var result = new Icon(new MemoryStream(buffer));

			return result;
		}

		#region Implementation

		#region Constants

		protected const float IconicMetafileScalingFactor = 26.4583333333f;

		#endregion

		protected static Size GetMetafilePixelDimensions(Metafile meta)
		{
			return ControlDpiScalingHelper.NewScaledSize((int)(meta.PhysicalDimension.Width / IconicMetafileScalingFactor + 0.5), (int)(meta.PhysicalDimension.Height / IconicMetafileScalingFactor + 0.5), false);
		}

		#endregion
	}
}
