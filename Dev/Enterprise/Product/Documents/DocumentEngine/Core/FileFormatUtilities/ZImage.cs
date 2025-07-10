using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.Common.Testing;

namespace Enterprise.DocumentEngine.FileFormatUtilities
{
	public class ZImage : IDisposable
	{
		readonly string SourceFile = string.Empty;
		readonly Stream SourceStream;

		ZImage(string sourceFileFullPath)
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			SourceFile = sourceFileFullPath;
		}

		ZImage(Stream stream)
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			SourceStream = stream;
		}

		ZImage(Bitmap image)
		{
			DisposableLeakListener.Instance.RegisterDisposable(this);
			fInternalImage = image;
		}

		public static ZImage FromStream(Stream stream)
		{
			return new ZImage(stream);
		}

		public static ZImage FromFile(string sourceFileFullPath)
		{
			return new ZImage(sourceFileFullPath);
		}

		public static implicit operator Image(ZImage image)
		{
			return image.InternalImage;
		}

		public bool IsA4LandscapePage
		{
			get { return IsA4Page && InternalImage.Width > InternalImage.Height; }
		}

		public bool IsA4Page
		{
			get
			{
				var widthToHeightRatio = InternalImage.Width / (double)InternalImage.Height;
				return widthToHeightRatio > 0.6 && widthToHeightRatio < 0.8 || widthToHeightRatio > 1.3 && widthToHeightRatio < 1.5;
			}
		}

		public int PageCount
		{
			get { return InternalImage.GetFrameCount(FrameDimension.Page); }
		}

		public int CurrentPageIndex
		{
			get
			{
				return fCurrentPageIndex;
			}
			set
			{
				if (value >= 0 && value < PageCount)
				{
					InternalImage.SelectActiveFrame(FrameDimension.Page, value);
					fCurrentPageIndex = value;
				}
			}
		}

		int fCurrentPageIndex;

		public int Width
		{
			get { return InternalImage.Width; }
		}

		public int Height
		{
			get { return InternalImage.Height; }
		}

		public float VerticalResolution
		{
			get { return InternalImage.VerticalResolution; }
		}

		public float HorizontalResolution
		{
			get { return InternalImage.HorizontalResolution; }
		}

		public Size Size
		{
			get { return InternalImage.Size; }
		}

		public SizeF PhysicalDimension
		{
			get { return InternalImage.PhysicalDimension; }
		}

		public PixelFormat PixelFormat
		{
			get { return InternalImage.PixelFormat; }
		}

		public ImageFormat RawFormat
		{
			get { return InternalImage.RawFormat; }
		}

		/// <summary>
		/// Clones the specified page and rotates it. This will leave the original document as-is. The frame index is 0-based.
		/// </summary>
		public ZImage RotateAndClonePage(int frameIndex, RotateFlipType rotation)
		{
			int oldPageIndex = CurrentPageIndex;
			ZImage imageToRotate = null;

			try
			{
				CurrentPageIndex = frameIndex;
				imageToRotate = CloneCurrentPage();

				if (imageToRotate.InternalImage != null)
				{
					imageToRotate.InternalImage.RotateFlip(rotation);
				}
			}
			finally
			{
				CurrentPageIndex = oldPageIndex;
			}

			return imageToRotate;
		}

		public ZImage CloneCurrentPage()
		{
			return new ZImage((Bitmap)InternalImage.Clone());
		}

		Bitmap InternalImage
		{
			get
			{
				if (fInternalImage == null)
				{
					if (!string.IsNullOrEmpty(SourceFile))
					{
						fInternalImage = new Bitmap(SourceFile);
					}
					else if (SourceStream != null)
					{
						fInternalImage = new Bitmap(SourceStream);
					}
				}
				return fInternalImage;
			}
		}

#if DEBUG
		internal Bitmap InternalImage_Exposed
		{
			get
			{
				return InternalImage;
			}
		}
#endif

		Bitmap fInternalImage;

		#region IDisposable Members

		public void Dispose()
		{
			if (fInternalImage != null)
			{
				fInternalImage.Dispose();
			}
			DisposableLeakListener.Instance.UnRegisterDisposable(this);
		}

		#endregion
	}
}
