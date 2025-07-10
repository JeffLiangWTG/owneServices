using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.DocumentScanning.GUI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.DocumentScanning.GUI.Res;

namespace Enterprise.DocumentScanning.Business
{
	public class MagnifyManager : NonPersistentBusinessObject, IObsoleteValidation, IDisposable
	{
		readonly DisposableList registeredEventHandlers = new DisposableList(1);

		public MagnifyManager(IImagePageSelectorProvider imageSource, PictureBox sourceBox = null)
		{
			fImageSource = imageSource;
			LoadRegistrySettings();
			ZoomInfo.ValueChanged += new EventHandler(ZoomInfo_ValueChanged);

			EnablePictureBox(sourceBox);
		}

		public void Dispose()
		{
			registeredEventHandlers.Dispose();

			ZoomInfo.ValueChanged -= new EventHandler(ZoomInfo_ValueChanged);
			SaveRegistrySettings();

			if (MagForm != null)
			{
				MagForm.Dispose();
				MagForm = null;
			}
		}

		#region PictureBox

		public void EnablePictureBox(PictureBox pictureBox)
		{
			if (pictureBox != null)
			{
				pictureBox.Cursor = MagnifyingGlassCursor;
				pictureBox.MouseDown += new MouseEventHandler(PictureBox_MouseDown);
				pictureBox.MouseMove += new MouseEventHandler(PictureBox_MouseMove);
				pictureBox.MouseUp += new MouseEventHandler(PictureBox_MouseUp);

				registeredEventHandlers.Add(new DisposableAction(() => DisablePictureBox(pictureBox)));
			}
		}

		void DisablePictureBox(PictureBox pictureBox)
		{
			if (pictureBox != null && !pictureBox.IsDisposed)
			{
				pictureBox.MouseDown -= new MouseEventHandler(PictureBox_MouseDown);
				pictureBox.MouseMove -= new MouseEventHandler(PictureBox_MouseMove);
				pictureBox.MouseUp -= new MouseEventHandler(PictureBox_MouseUp);
				pictureBox.Cursor = Cursors.Default;
			}
		}

		#region Cursor

		Cursor MagnifyingGlassCursor
		{
			get
			{
				if (fMagnifyingGlassCursor == null)
				{
					Assembly assembly = this.GetType().Assembly;
					System.IO.Stream cursorStream = assembly.GetManifestResourceStream("Enterprise.DocumentScanning.GUI.Cursors.MagnifyingGlass1.cur");
					fMagnifyingGlassCursor = new Cursor(cursorStream);
				}
				return fMagnifyingGlassCursor;
			}
		}

		internal Cursor MagnifyingGlassCursorInternal => MagnifyingGlassCursor;

		Cursor fMagnifyingGlassCursor;

		#endregion

		#endregion

		public void UpdatePictureInMagnificationWindow()
		{
			if (MagForm != null && MagForm.Visible)
			{
				MagForm.UpdateLatestImage(0.0F, 0.0F, false);
			}
		}

		/// <summary>
		/// Searches through the present list of zooms and returns the next fZoom level
		/// (either increase or decrease magnification)
		/// </summary>
		public void ChangeZoom(bool increaseZoom)
		{
			int newZoom = 0;

			if (Zoom == "0")
			{
				newZoom = GetNewZoomFromFitToWidth(increaseZoom);
			}
			else
			{
				for (int i = 0; i < ZoomValues.Count; i++)
				{
					if (ZoomValues[i] == int.Parse(Zoom))
					{
						if (increaseZoom && i < (ZoomValues.Count - 1))
						{
							newZoom = ZoomValues[i + 1];
						}
						else if (!increaseZoom && i > 0)
						{
							newZoom = ZoomValues[i - 1];
						}
						else
						{
							newZoom = int.Parse(Zoom);
						}
					}
				}
			}

			Zoom = newZoom.ToString();
		}

		public Bitmap ExtractImageSection(RectangleF intersection)
		{
			RectangleF adjustedRectangleForResolution = new DocumentImageSizer(LatestBitmap).ScaleIntersectionRectangleForResolution(intersection);
			adjustedRectangleForResolution = AdjustRectangleToBeWhollyWithinAvailableSpace(adjustedRectangleForResolution, new SizeF(LatestBitmap.Width, LatestBitmap.Height), 0.001F);
			return adjustedRectangleForResolution == RectangleF.Empty ? null : LatestBitmap.Clone(adjustedRectangleForResolution, LatestBitmap.PixelFormat);
		}

		/// <summary>
		/// The dimensions of the portion of the image being displayed, IN PROPORTION to 
		/// the size of the whole image.  Values will always be between 0-1.
		/// </summary>
		public SizeF? DisplaySizeInProportions
		{
			get
			{
				if (LatestBitmap == null)
				{
					return null;
				}

				// Cannot cache the AdjustedSize - calculate it individually when you need it.
				// Because if page count > 0 and there are different size pages in your document, the cached size 
				// will be incorrect for landscape/portrait/unusual size images, etc.
				var adjustedSize = new DocumentImageSizer(LatestBitmap).AdjustSizeForResolution();
				return new SizeF(DisplayRectangleInPixels.Width / adjustedSize.Width, DisplayRectangleInPixels.Height / adjustedSize.Height);
			}
		}

		/// <summary>
		/// The portion of the image being shown in the magnifying window. e.g. if image size is 850 x 600,
		/// rectangle might be showing starting point (400,300) length 100 width 100 pixels.
		/// </summary>
		public RectangleF DisplayRectangleInPixels
		{
			get { return fDisplayRectangleInPixels; }
		}

		public virtual Image CreateLatestImageSection(Size availableScreenSize, float extraMovementProportionX, float extraMovementProportionY)
		{
			drawableAreaSize = availableScreenSize;

			if (LatestBitmap == null || availableScreenSize.Width <= 0 || availableScreenSize.Height <= 0)
			{
				return null;
			}

			PointF startingPoint = CalculateDisplayStartPoint(extraMovementProportionX, extraMovementProportionY);
			return GetSectionToMagnify(startingPoint, availableScreenSize);
		}

		#region TitleText

		/// <summary>
		/// Text to appear in the title window of the Magnifying Glass window.
		/// </summary>
		public ZString TitleText
		{
			get { return " " + Res.GetString("5131334b-5fa8-449f-96ef-19e7782fb04f", "- Page") + " " + (CurrentPageIndex + 1) + " " + Res.GetString("3f3ce2d9-82a7-4ee5-bfb7-937862ff7db0", "of {0}", fImageSource.PageSelector.TotalPages); }
		}

		#endregion

		#region CurrentPageIndex

		/// <summary>
		/// Zero based index of current page being displayed in the magnifying window. 
		/// </summary>
		public ZInt CurrentPageIndex
		{
			get { return fImageSource.PageSelector.CurrentPageIndex; }
			set
			{
				if (value < fImageSource.PageSelector.TotalPages && value >= 0)
				{
					fImageSource.PageSelector.CurrentPageIndex = value;
				}
				CurrentPageIndexInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo CurrentPageIndexInfo
		{
			get { return GetZPropertyInfo(nameof(CurrentPageIndex)); }
		}

		#endregion

		#region Zoom

		[BusinessObjectTestExclude()]
		[List("Zoom_List")]
		[MaxLength(3)]
		public ZString Zoom
		{
			get { return fZoom; }
			set
			{
				if (!value.IsEmpty && value.IsNumbersOnlyOrEmpty)
				{
					CheckMaximumLength(ZoomInfo, value);
					fZoom = value;
					MagnificationValue = Convert.ToInt32(fZoom) / 100f;
				}
				ZoomInfo.RefreshBinding();
			}
		}

		protected ZString fZoom;

		public ZPropertyInfo ZoomInfo
		{
			get
			{
				return GetZPropertyInfo(nameof(Zoom));
			}
		}

		#endregion

		#region Zoom_List

		public CodeDescriptionPairList Zoom_List
		{
			get
			{
				if (fZoom_List == null)
				{
					fZoom_List = new CodeDescriptionPairList();
					foreach (int zoomValue in ZoomValues)
					{
						fZoom_List.AddPair(zoomValue.ToString(), zoomValue.ToString() + "%");
					}
					fZoom_List.AddPair("0", Res.GetString("48d80a26-756a-425d-8a4c-92ce8bf2e6bf", "Fit to Width"));
				}
				return fZoom_List;
			}
		}

		CodeDescriptionPairList fZoom_List;

		#endregion

		#region Zoom functions

		internal List<int> ZoomValues
		{
			get
			{
				if (fZoomValues == null)
				{
					fZoomValues = new List<int>();
					fZoomValues.Add(25);
					fZoomValues.Add(38);
					fZoomValues.Add(50);
					fZoomValues.Add(62);
					fZoomValues.Add(75);
					fZoomValues.Add(88);
					fZoomValues.Add(100);
					fZoomValues.Add(125);
					fZoomValues.Add(150);
					fZoomValues.Add(175);
					fZoomValues.Add(200);
					fZoomValues.Add(225);
					fZoomValues.Add(250);
					fZoomValues.Add(275);
					fZoomValues.Add(300);
					fZoomValues.Add(350);
					fZoomValues.Add(400);
					fZoomValues.Add(450);
					fZoomValues.Add(500);
				}
				return fZoomValues;
			}
		}

		List<int> fZoomValues;

		/// <summary>
		/// Calculates what the new fZoom should be (increased/decreased) if currently on Fit To Width of screen. 
		/// </summary>
		internal int GetNewZoomFromFitToWidth(bool increaseZoom)
		{
			SizeF screenSize = CachedScreenInfo.Instance.FromControl(MagForm).Size;
			float currentScale = drawableAreaSize.Width / screenSize.Width;

			int valueBelow = Convert.ToInt32(ZoomValues[0]);
			int valueAbove = Convert.ToInt32(ZoomValues[ZoomValues.Count - 1]);

			foreach (int value in ZoomValues)
			{
				float zoomValue = value / 100f;

				if (currentScale > zoomValue)
				{
					if (valueBelow < value) { valueBelow = value; }
				}
				else
				{
					if (valueAbove >= value) { valueAbove = value; }
				}
			}

			return (increaseZoom) ? valueAbove : valueBelow;
		}

		#endregion

		#region Registry Settings

		DMMagnifyingGlassSettingsStruct registrySettings;

		void LoadRegistrySettings()
		{
			registrySettings = Env.Registry.DMMagnifyingGlassSettings;
			if (ZoomValues.Contains(registrySettings.MagnificationPercentage))
			{
				Zoom = registrySettings.MagnificationPercentage.ToString();
			}
			else
			{
				Zoom = "0";
			}
		}

		internal void SaveRegistrySettings()
		{
			var magnificationPercentage = int.TryParse(Zoom, out var result) ? result : 100;
			registrySettings.MagnificationPercentage = magnificationPercentage;

			if (Env.Registry.DMMagnifyingGlassSettings.MagnificationPercentage != magnificationPercentage)
			{
				Env.Registry.DMMagnifyingGlassSettings = registrySettings;
			}
		}

		#endregion

		#region LatestBitmap

		internal Bitmap LatestBitmap => (Bitmap)fImageSource.PageSelector?.CurrentImage;

		#endregion

		void PictureBox_MouseUp(object sender, MouseEventArgs e)
		{
			if (fIsMovingMagnifyingGlass)
			{
				fIsMovingMagnifyingGlass = false;
			}
		}

		internal void PictureBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (IsMouseEventAcceptable(e))
			{
				if (LatestBitmap != null)
				{
					fLatestSourcePictureBoxX = e.X;
					fLatestSourcePictureBoxY = e.Y;

					UpdateImageForMousePosition((PictureBox)sender, e.X, e.Y);
					fIsMovingMagnifyingGlass = true;
				}
			}
		}

		void PictureBox_MouseMove(object sender, MouseEventArgs e)
		{
			if (fIsMovingMagnifyingGlass)
			{
				PictureBox curPictureBox = (PictureBox)sender;
				if (ControlDpiScalingHelper.NewScaledRectangle(ControlDpiScalingHelper.NewScaledPoint(0, 0), curPictureBox.Size).Contains(e.X, e.Y))
				{
					if (IsMouseEventAcceptable(e))
					{
						if (!((fLatestSourcePictureBoxX == e.X) &&
							(fLatestSourcePictureBoxY == e.Y)))
						{
							fLatestSourcePictureBoxX = e.X;
							fLatestSourcePictureBoxY = e.Y;

							UpdateImageForMousePosition((PictureBox)sender, e.X, e.Y);
						}
					}
				}
			}
		}

		bool IsMouseEventAcceptable(MouseEventArgs e)
		{
			if (e.Button != System.Windows.Forms.MouseButtons.Left)
			{
				return false;
			}

			if (Control.ModifierKeys == Keys.Control || Control.ModifierKeys == Keys.Shift)
			{
				return false;
			}

			return true;
		}

		void UpdateImageForMousePosition(PictureBox senderPictureBox, int reqX, int reqY)
		{
			var curPictureBox = senderPictureBox;
			DisplayStartPointInProportions = new PointF(reqX / (float)curPictureBox.ClientSize.Width, reqY / (float)curPictureBox.ClientSize.Height);

			if (MagForm == null)
			{
				MagForm = new MagnifyForm(this);
				MagForm.Closing += MagForm_Closing;
				MagForm.Show();
			}

#if !WINZOR
			MagForm.UpdateLatestImage(0.0F, 0.0F, true);
#endif
		}

		internal bool OnLastPage
		{
			get { return CurrentPageIndex == fImageSource.PageSelector.TotalPages - 1; }
		}

		internal bool OnFirstPage
		{
			get { return CurrentPageIndex == 0; }
		}

		internal PointF CalculateDisplayStartPoint(float extraXMovement, float extraYMovement)
		{
			float pX = DisplayStartPointInProportions.X + extraXMovement;
			float pY = DisplayStartPointInProportions.Y + extraYMovement;
			PointF startingPoint = new PointF(pX, pY);

			// horizontal movement changes
			if (startingPoint.X < 0.0F)
			{
				ControlDpiScalingHelper.SetX(ref startingPoint, (int)0.0F, true);
			}
			else if (startingPoint.X > ControlDpiScalingHelper.ScaleToCurrentDpiX((int)1.0F))
			{
				ControlDpiScalingHelper.SetX(ref startingPoint, (int)1.0F, true);
			}

			// vertical movement changes
			// Cannot cache the AdjustedSize - calculate it individually when you need it.
			// Because if page count > 0 and there are different size pages in your document, the cached size 
			// will be incorrect for landscape/portrait/unusual size images, etc.
			Size adjustedSize = new DocumentImageSizer(LatestBitmap).AdjustSizeForResolution();
			var lastPercentRegion = adjustedSize.Height / 33f;

			var currentlyAtBottomOfPage = Math.Ceiling(DisplayRectangleInPixels.Y + DisplayRectangleInPixels.Height + lastPercentRegion) >= adjustedSize.Height;
			bool currentlyAtTopOfPage = Math.Floor(DisplayRectangleInPixels.Y - lastPercentRegion) <= 0.0f;

			bool bottomOfPageCommandSent = startingPoint.Y > 1.0F;
			bool topOfPageCommandSent = startingPoint.Y < 0.0F;

			if (topOfPageCommandSent)
			{
				if (currentlyAtTopOfPage && !OnFirstPage)
				{
					CurrentPageIndex--;
					ControlDpiScalingHelper.SetY(ref startingPoint, 1, true);
				}
				else
				{
					ControlDpiScalingHelper.SetY(ref startingPoint, 0, true);
				}
			}
			else if (bottomOfPageCommandSent)
			{
				if (currentlyAtBottomOfPage && !OnLastPage)
				{
					CurrentPageIndex++;
					ControlDpiScalingHelper.SetY(ref startingPoint, 0, true);
				}
				else
				{
					ControlDpiScalingHelper.SetY(ref startingPoint, 1, true);
				}
			}

			return startingPoint;
		}

		float AdjustPointWithinBounds(float proposed, float maxAllowed, float sectionLength)
		{
			float adjustedPoint = 0.0F;

			if (proposed + sectionLength > maxAllowed)
			{
				adjustedPoint = maxAllowed - sectionLength;
			}
			else if (proposed > 0.0F)
			{
				adjustedPoint = proposed;
			}

			return adjustedPoint;
		}

		float RoundFloatDown(float srcFloat)
		{
			return Convert.ToSingle(Math.Floor(Convert.ToDouble(srcFloat)));
		}

		float GetMagnificationValueForFitToWidth(float availWidth, float imageWidth)
		{
			return availWidth / imageWidth;
		}

		float ExtraMovementPixelsToProportion(float movementPixels, float imageLength)
		{
			return movementPixels / imageLength;
		}

		protected Bitmap GetSectionToMagnify(PointF startingPointPortion, Size availScreenSize)
		{
			// Cannot cache the AdjustedSize - calculate it individually when you need it.
			// Because if page count > 0 and there are different size pages in your document, the cached size 
			// will be incorrect for landscape/portrait/unusual size images, etc.
			Size imageDimensions = new DocumentImageSizer(LatestBitmap).AdjustSizeForResolution();

			var spaceAroundX = RoundFloatDown(availScreenSize.Width / 2.0F);
			var spaceAroundY = RoundFloatDown(availScreenSize.Height / 2.0F);
			float scaledMagnifyFactor;

			if (MagnificationValue == 0.0F) // i.e. means fit to width
			{
				scaledMagnifyFactor = GetMagnificationValueForFitToWidth(availScreenSize.Width, imageDimensions.Width);
			}
			else
			{
				Size availMagnify100Size = LatestBitmap.Size;
				scaledMagnifyFactor = GetMagnificationValueForFitToWidth(availMagnify100Size.Width, imageDimensions.Width);
				scaledMagnifyFactor *= MagnificationValue;
			}

			spaceAroundX /= scaledMagnifyFactor;
			spaceAroundY /= scaledMagnifyFactor;

			if (imageDimensions.Width < (spaceAroundX * 2))
			{
				// Source image is not wide enough, so display less stuff.
				spaceAroundX = RoundFloatDown(imageDimensions.Width / 2.0F);
			}

			if (imageDimensions.Height < (spaceAroundY * 2))
			{
				// Source image is not tall enough, so display less stuff.
				spaceAroundY = RoundFloatDown(imageDimensions.Height / 2.0F);
			}

			// Starting Point of the section we're magnifying on the image
			PointF startingPointPixels = new PointF(startingPointPortion.X * imageDimensions.Width,
				startingPointPortion.Y * imageDimensions.Height);

			PointF newPointF = new PointF(startingPointPixels.X - spaceAroundX, startingPointPixels.Y - spaceAroundY);

			float adjustedX2 = AdjustPointWithinBounds(newPointF.X, imageDimensions.Width, spaceAroundX * 2);
			float adjustedY2 = AdjustPointWithinBounds(newPointF.Y, imageDimensions.Height, spaceAroundY * 2);

			ControlDpiScalingHelper.SetX(ref newPointF, (int)adjustedX2, false);
			ControlDpiScalingHelper.SetY(ref newPointF, (int)adjustedY2, false);
			DisplayStartPointInProportions = new PointF(ExtraMovementPixelsToProportion(adjustedX2 + spaceAroundX, imageDimensions.Width),
									ExtraMovementPixelsToProportion(adjustedY2 + spaceAroundY, imageDimensions.Height));

			SizeF newSizeF = new SizeF(spaceAroundX * 2.0F, spaceAroundY * 2.0F);
			RectangleF srcRect = new RectangleF(newPointF, newSizeF);
			Bitmap destBitMap = ExtractImageSection(srcRect);

			if (MagForm != null && destBitMap != null)
			{
				PictureBox destPictureBox = this.MagForm.GetImagePictureBox();
				if (destPictureBox != null)
				{
					SizeF actualImageSize = new SizeF(srcRect.Size.Width * scaledMagnifyFactor,
						srcRect.Size.Height * scaledMagnifyFactor);

					destPictureBox.ClientSize = Size.Truncate(actualImageSize);
				}
			}

			fDisplayRectangleInPixels = srcRect;
			return destBitMap;
		}

		/// <summary>
		/// Adjusts source rectangle so that it is wholly within the physical available size. 
		/// If no intersection, returns an empty rectangle.
		/// </summary>
		internal RectangleF AdjustRectangleToBeWhollyWithinAvailableSpace(RectangleF srcRect, SizeF physicalSizeAvailable, float smallDelta)
		{
			RectangleF adjusted = RectangleF.Intersect(srcRect, new RectangleF(new PointF(0, 0), physicalSizeAvailable));

			// Subtract a small delta to make sure the whole dimension is not used, as
			// dotnet Image support can sometimes have problems if the amounts are not subtracted.
			if ((adjusted.X + adjusted.Width) >= physicalSizeAvailable.Width)
			{
				ControlDpiScalingHelper.SetWidth(ref adjusted, (int)(adjusted.Width - smallDelta), false);
			}

			if ((adjusted.Y + adjusted.Height) >= physicalSizeAvailable.Height)
			{
				ControlDpiScalingHelper.SetHeight(ref adjusted, (int)(adjusted.Height - smallDelta), false);
			}

			if (adjusted.Width <= 0 || adjusted.Height <= 0)
			{
				adjusted = RectangleF.Empty;
			}

			return adjusted;
		}

		internal PointF DisplayStartPointInProportions
		{
			get { return fDisplayStartPointInProportions; }
			set { fDisplayStartPointInProportions = value; }
		}

		internal MagnifyForm MagForm;
		PointF fDisplayStartPointInProportions = PointF.Empty;
		internal RectangleF fDisplayRectangleInPixels = RectangleF.Empty;
		bool fIsMovingMagnifyingGlass;
		int fLatestSourcePictureBoxX;
		int fLatestSourcePictureBoxY;
		internal float MagnificationValue;  // default to fit to width. 1 -> 100%, 2 -> 200% etc.
		readonly IImagePageSelectorProvider fImageSource;
		Size drawableAreaSize;

		void ZoomInfo_ValueChanged(object sender, EventArgs e)
		{
			if (MagForm != null)
			{
				MagForm.UpdateLatestImage(0.0F, 0.0F, true);
			}
		}

		void MagForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			MagForm = null;
		}
	}
}
