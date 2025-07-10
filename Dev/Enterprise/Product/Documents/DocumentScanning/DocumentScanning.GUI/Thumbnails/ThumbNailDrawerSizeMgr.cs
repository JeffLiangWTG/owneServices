using System.Drawing;
using CargoWise.Windows.UI;

namespace Enterprise.DocumentScanning.Thumbnails
{
	public class ThumbNailDrawerSizeMgr
	{
		public ThumbNailDrawerSizeMgr()
		{
			TotalBoxSizeForOneThumbnail = ControlDpiScalingHelper.NewScaledSize(100, 120); // setup default values
		}

		#region TotalBoxSizeForOneThumbnail

		/// <summary>
		/// Area for one thumbnail - includes the drawable area for the label, image, and spacing around.
		/// </summary>
		[DpiState(DpiState.ScaledVariant)]
		public Size TotalBoxSizeForOneThumbnail
		{
			get
			{
				return fTotalBoxSizeForOneThumbnail;
			}
			set
			{
				fTotalBoxSizeForOneThumbnail = value;
				AdjustThumbnailMeasurements();
			}
		}

		Size fTotalBoxSizeForOneThumbnail;

		#endregion

		#region ImagePanelSize

		/// <summary>
		/// Total area of the image portion of the thumbnail (doesn't include Text panel above)
		/// </summary>
		[DpiState(DpiState.ScaledVariant)]
		public Size ImagePanelSize
		{
			get { return fCalcImagePanelSize; }
		}
		Size fCalcImagePanelSize;

		#endregion

		#region ImagePanelOffset

		/// <summary>
		/// Relative offset point to start drawing the ImagePanelSize
		/// </summary>
		[DpiState(DpiState.ScaledVariant)]
		public Point ImagePanelOffset
		{
			get { return fCalcImagePanelOffset; }
		}
		Point fCalcImagePanelOffset;

		#endregion

		#region DrawableAreaSize

		/// <summary>
		/// Total area taken up by one thumbnail. Includes Label & Image - does not include whitespace around the thumbnail.
		/// </summary>
		[DpiState(DpiState.ScaledVariant)]
		public Size DrawableAreaSize
		{
			get { return fCalcDrawableAreaSize; }
		}
		Size fCalcDrawableAreaSize;

		#endregion

		#region DrawableAreaOffset

		/// <summary>
		/// Relative Offset point to start drawing the DrawableArea from.
		/// </summary>
		[DpiState(DpiState.ScaledVariant)]
		public Point DrawableAreaOffset
		{
			get { return fCalcDrawableAreaOffset; }
		}
		Point fCalcDrawableAreaOffset;

		#endregion

		#region LabelSize

		/// <summary>
		/// Size of the label above each thumbnail with the thumb's page number
		/// </summary>
		[DpiState(DpiState.ScaledVariant)]
		public Size LabelSize
		{
			get { return fCalcLabelSize; }
		}
		Size fCalcLabelSize;

		#endregion

		#region LabelOffset

		/// <summary>
		/// Relative offset to start painting the label
		/// </summary>
		[DpiState(DpiState.ScaledVariant)]
		public Point LabelOffset
		{
			get { return fCalcLabelOffset; }
		}
		Point fCalcLabelOffset;

		#endregion

		#region DropPanelSize

		/// <summary>
		/// The area where Drag-Drop is allowed (the whitespace between thumbnails)
		/// </summary>
		[DpiState(DpiState.ScaledVariant)]
		public Size DropPanelSize
		{
			get { return fCalcDropPanelSize; }
		}

		Size fCalcDropPanelSize;

		#endregion

		#region DropPanelLastInRowSize

		/// <summary>
		/// The area where Drag-Drop is allowed - specifically for the last thumbnail in a row
		/// </summary>
		[DpiState(DpiState.ScaledVariant)]
		public Size DropPanelLastInRowSize
		{
			get { return fCalcDropPanelLastInRowSize; }
		}

		Size fCalcDropPanelLastInRowSize;

		#endregion

		#region DropPanelFirstInRowSize

		/// <summary>
		/// The area where Drag-Drop is allowed - specifically for the first thumbnail in a row
		/// </summary>
		[DpiState(DpiState.ScaledVariant)]
		public Size DropPanelFirstInRowSize
		{
			get { return fCalcDropPanelFirstInRowSize; }
		}
		Size fCalcDropPanelFirstInRowSize;
		#endregion

		[DpiState(DpiState.ScaledVariant)]
		public Size MinimumTotalBoxSizeForOneThumbnail
		{
			get { return ControlDpiScalingHelper.NewScaledSize(MinimumWidth, (int)(MinimumWidth * 1.33)); }
		}

		public readonly int WhitespaceAtRowStart = 8;
		public readonly int WhitespaceAtRowEnd = 8;

		#region Implementation

		readonly int PaddingLeft = 10;
		readonly int PaddingRight = 10;
		readonly int PaddingTop = 8;
		readonly int PaddingBottom = 6;
		readonly int TextBoxHeight = 22;
		readonly int MinimumWidth = 80;

		protected void AdjustThumbnailMeasurements()
		{
			int drawableAreaWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(TotalBoxSizeForOneThumbnail.Width) - PaddingLeft - PaddingRight;
			int drawableAreaHeight = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(TotalBoxSizeForOneThumbnail.Height) - PaddingTop - PaddingBottom;

			// Thumbnail calculations
			fCalcDrawableAreaSize = ControlDpiScalingHelper.NewScaledSize(drawableAreaWidth, drawableAreaHeight);
			fCalcDrawableAreaOffset = ControlDpiScalingHelper.NewScaledPoint(PaddingLeft, PaddingTop);

			// Calculations for Drag & Drop areas
			fCalcDropPanelSize = ControlDpiScalingHelper.NewScaledSize((PaddingLeft + PaddingRight), drawableAreaHeight);
			fCalcDropPanelFirstInRowSize = ControlDpiScalingHelper.NewScaledSize(PaddingLeft, drawableAreaHeight);
			fCalcDropPanelLastInRowSize = ControlDpiScalingHelper.NewScaledSize(PaddingRight, drawableAreaHeight);

			// Size of the thumbnail image (no label)
			int imagePanelHeight = drawableAreaHeight - TextBoxHeight;
			fCalcImagePanelSize = ControlDpiScalingHelper.NewScaledSize(drawableAreaWidth, imagePanelHeight);
			fCalcImagePanelOffset = ControlDpiScalingHelper.NewScaledPoint(0, TextBoxHeight);

			// size of the label you use to select the thumb
			fCalcLabelSize = ControlDpiScalingHelper.NewScaledSize(drawableAreaWidth, TextBoxHeight);
			fCalcLabelOffset = ControlDpiScalingHelper.NewScaledPoint(0, 0);
		}

		#endregion
	}
}
