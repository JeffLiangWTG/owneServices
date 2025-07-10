using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace Enterprise.DocumentEngine.Imaging
{
	public enum ResizeOption
	{
		FitShorterSide,
		FitCompletely,
		FitToWidth,
		FitToHeight
	}

	public class DocumentImageSizer
	{
		public DocumentImageSizer(Image imageToResize, ResizeOption resizeOption)
		{
			this.resizeOption = resizeOption;
			this.imageToResize = imageToResize;
		}

		public DocumentImageSizer(Image imageToResize)
			: this(imageToResize, ResizeOption.FitShorterSide)
		{
		}

		public Size AdjustImageSize(Size available)
		{
			ResizeOption unused;
			return AdjustImageSize(available, out unused);
		}

		[SuppressMessage("Microsoft.Design", "CA1021")]
		public Size AdjustImageSize(Size available, out ResizeOption resizingOptionApplied)
		{
			Size resolutionAdjustedSize = AdjustSizeForResolution();
			return AdjustImageSize(resizeOption, available, resolutionAdjustedSize.Width / (double)resolutionAdjustedSize.Height, out resizingOptionApplied);
		}

		public static Size AdjustImageSize(ResizeOption resizingOption, Size available, double imageWidthHeightRatio)
		{
			ResizeOption unused;
			return AdjustImageSize(resizingOption, available, imageWidthHeightRatio, out unused);
		}

		static Size AdjustImageSize(ResizeOption resizingOption, Size available, double imageWidthHeightRatio, out ResizeOption resizingOptionApplied)
		{
			var containerRatio = available.Width / (double)available.Height;
			int x, y;

			if (FitToXAxis(resizingOption, containerRatio, imageWidthHeightRatio))
			{
				// need to fit X flush, scroll Y.
				x = available.Width;
				y = (int)(available.Width / imageWidthHeightRatio);
				resizingOptionApplied = ResizeOption.FitToWidth;
			}
			else
			{
				// need to fit Y flush, scroll X.
				y = available.Height;
				x = (int)(available.Height * imageWidthHeightRatio);
				resizingOptionApplied = ResizeOption.FitToHeight;
			}
			return ControlDpiScalingHelper.NewScaledSize(x, y, false);
		}

		/// <summary>
		/// Makes width and height proportional to the image if it had the same resolution for vertical and horizontal axes.
		/// IT MAY RETURN A WIDTH AND HEIGHT LARGER THAN YOUR ACTUAL IMAGE. 
		/// </summary>
		public Size AdjustSizeForResolution()
		{
			Size adjustedSize = imageToResize.Size;

			if (imageToResize.HorizontalResolution > imageToResize.VerticalResolution) // image is wider than tall
			{
				ControlDpiScalingHelper.SetHeight(ref adjustedSize, (int)((double)imageToResize.HorizontalResolution / (double)imageToResize.VerticalResolution * adjustedSize.Height), false);
			}
			else if (imageToResize.HorizontalResolution < imageToResize.VerticalResolution)
			{
				ControlDpiScalingHelper.SetWidth(ref adjustedSize, (int)((double)imageToResize.VerticalResolution / (double)imageToResize.HorizontalResolution * adjustedSize.Width), false);
			}
			return adjustedSize;
		}

		public RectangleF ScaleIntersectionRectangleForResolution(RectangleF rectangleToScale)
		{
			RectangleF scaledRectangle = rectangleToScale;

			if (imageToResize.HorizontalResolution > imageToResize.VerticalResolution)
			{
				float ratio = imageToResize.VerticalResolution / imageToResize.HorizontalResolution;

				ControlDpiScalingHelper.SetHeight(ref scaledRectangle, (int)(scaledRectangle.Height * ratio), false);
				ControlDpiScalingHelper.SetY(ref scaledRectangle, (int)(scaledRectangle.Y * ratio), false);
			}
			else if (imageToResize.VerticalResolution > imageToResize.HorizontalResolution)
			{
				float ratio = imageToResize.HorizontalResolution / imageToResize.VerticalResolution;

				ControlDpiScalingHelper.SetWidth(ref scaledRectangle, (int)(scaledRectangle.Width * ratio), false);
				ControlDpiScalingHelper.SetX(ref scaledRectangle, (int)(scaledRectangle.X * ratio), false);
			}
			return scaledRectangle;
		}

		public Bitmap CreateThumbnail(Size boundingSize)
		{
			Size newSize = AdjustImageSize(boundingSize);
			Bitmap clonedImage = new Bitmap(newSize.Width, newSize.Height);

			// use Graphics.draw instead of image.Clone because you can adjust for uneven resolutions where x is different to y
			using (Graphics g = Graphics.FromImage(clonedImage))
			{
				g.DrawImage(imageToResize, 0, 0, newSize.Width, newSize.Height);
			}
			return clonedImage;
		}

		static bool FitToXAxis(ResizeOption resizingOption, double containerRatio, double imageRatio)
		{
			bool returnValue;
			switch (resizingOption)
			{
				case ResizeOption.FitToHeight:
					returnValue = false;
					break;

				case ResizeOption.FitToWidth:
					returnValue = true;
					break;

				case ResizeOption.FitCompletely:
					returnValue = containerRatio <= imageRatio;
					break;

				case ResizeOption.FitShorterSide:
				default:
					returnValue = containerRatio > imageRatio;
					break;
			}
			return returnValue;
		}

		readonly Image imageToResize;
		readonly ResizeOption resizeOption;
	}
}
