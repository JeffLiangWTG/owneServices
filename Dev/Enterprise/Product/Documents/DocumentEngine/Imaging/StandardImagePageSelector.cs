using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using CargoWise.Common;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.Imaging
{
	public class StandardImagePageSelector : Disposable, IImagePageSelector
	{
		public StandardImagePageSelector()
			: base()
		{
		}

		public StandardImagePageSelector(Image viewableImage)
		{
			SetImage(viewableImage);
		}

		public StandardImagePageSelector(Image viewableImage, string fileName)
		{
			SetImage(viewableImage);
			this.FileName = fileName;
		}

		public IImagePageSelector PageSelector
		{
			get { return this; }
		}

		#region Total Pages

		public int TotalPages
		{
			get
			{
				if (CurrentImage == null)
				{
					return 0;
				}

				Guid[] supportedDimensions = CurrentImage.FrameDimensionsList;
				foreach (Guid dimension in supportedDimensions)
				{
					if (dimension.Equals(FrameDimension.Page.Guid))
					{
						return CurrentImage.GetFrameCount(FrameDimension.Page);
					}
				}
				return 1;
			}
		}

		#endregion

		#region FileName

		public string FileName { get; set; }

		#endregion

		#region CurrentPageIndex

		public int CurrentPageIndex
		{
			get
			{
				return fCurrentPageIndex;
			}
			set
			{
				if (CurrentImage == null)
				{
					fCurrentPageIndex = -1;
				}
				else
				{
					if ((value >= 0) && (value < TotalPages))
					{
						try
						{
							CurrentImage.SelectActiveFrame(FrameDimension.Page, value);
						}
						catch (ExternalException ex)
						{
							throw new UnsupportedImagePageCompressionException(ex, FileName);
						}

						fCurrentPageIndex = value;
					}
				}
			}
		}

		protected int fCurrentPageIndex = -1;

		#endregion

		#region CurrentImage

		public Image CurrentImage
		{
			get { return fCurrentImage; }
		}

		Image fCurrentImage;

		#endregion

		#region Dispose

		protected override void Dispose(bool isDisposing)
		{
			if (isDisposing)
			{
				fCurrentImage = null; // image currently disposed by parent of this object
			}
		}

		#endregion

		#region Implementation

		protected void SetImage(Image reqImage)
		{
			fCurrentPageIndex = -1;

			if (reqImage != null)
			{
				fCurrentImage = reqImage;
				fCurrentPageIndex = 0;
			}
			else
			{
				fCurrentImage = null;
			}
		}

		#endregion
	}
}
