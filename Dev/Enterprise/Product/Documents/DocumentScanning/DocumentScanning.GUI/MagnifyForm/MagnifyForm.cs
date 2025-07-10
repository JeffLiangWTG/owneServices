using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Interop;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.OCR;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public sealed partial class MagnifyForm : ZChildForm
	{
		[SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "WINZOR condition causing issues")]
		bool NavigationMode = true;
		Point fSelectionBoxStart = Point.Empty;
		internal Point fSelectionBoxEnd = Point.Empty;
		readonly ArrayList fSelectionBoxColumnOffsets = new ArrayList();
		bool fFormWasLoaded;

		public MagnifyForm(MagnifyManager magManager)
			: base(magManager)
		{
			InitializeComponent();
			MainStatusBar.Visible = false;
			SetupToolTips();
#if !WINZOR
			SetupMainContextMenu();
#endif
			ChangeNavigationMode();

#if !WINZOR
			SetKeyboardHook();
#else
			InitializeFormForWinzor();
#endif

			ExpandCollapseButton.AllowOverlap(ToolbarPanel);
		}

		void SetupToolTips()
		{
			this.LeftButton.ToolTipText = Res.GetString("2c9507e7-29f8-4fc3-bf4b-60dc40388d78", "Click to scroll left");
			this.UpButton.ToolTipText = Res.GetString("f66f3368-b567-4758-9b62-c5172104869e", "Click to scroll up");
			this.DownButton.ToolTipText = Res.GetString("5a91ef38-d59c-48d2-994f-70c3a973197a", "Click to scroll down");
			this.RightButton.ToolTipText = Res.GetString("cbbccd90-38ff-4d44-be1b-1e0836677e2a", "Click to scroll right");
			this.SelectTextButton.ToolTipText = Res.GetString("021c2fef-f6e6-43d2-840e-e85fbc1a128e", "Select Text");
			this.OCRButton.ToolTipText = Res.GetString("eDocsMagnify|ConvertToText", "Convert to Text");
			this.ClearSelectionButton.ToolTipText = Res.GetString("eDocsMagnify|EraseSelectionBox", "Erase Selection Box");
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			PercentLabel.Location = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(ZoomDropEdit.Location.X + ZoomDropEdit.Size.Width), 8);
			UpdateLatestImage(0.0F, 0.0F, false);
		}

		MagnifyManager MagnifyManager
		{
			get { return (MagnifyManager)BusinessEntity; }
		}

		#region Cursors

		internal Cursor GetOpenHand()
		{
			return new Cursor(this.GetType().Assembly.GetManifestResourceStream("Enterprise.DocumentScanning.GUI.Cursors.handflat.cur"));
		}

		internal Cursor GetClosedHand()
		{
			return new Cursor(this.GetType().Assembly.GetManifestResourceStream("Enterprise.DocumentScanning.GUI.Cursors.handgrab.cur"));
		}

		#endregion

		#region Keyboard Hook
#if !WINZOR

		NativeMethods.HookProc KeyboardHookProcEventHandler;
		IntPtr KeyboardHook;

		void SetKeyboardHook()
		{
			if (KeyboardHook == IntPtr.Zero)
			{
				if (KeyboardHookProcEventHandler == null)
				{
					KeyboardHookProcEventHandler = new NativeMethods.HookProc(KeyboardHookProc);
				}

				KeyboardHook = UnsafeNativeMethods.SetWindowsHookEx(WindowsMessage.WH_KEYBOARD, KeyboardHookProcEventHandler, IntPtr.Zero, SafeNativeMethods.GetCurrentThreadId());
			}
		}

		void RemoveKeyboardHook()
		{
			if (KeyboardHook != IntPtr.Zero)
			{
				UnsafeNativeMethods.UnhookWindowsHookEx(new HandleRef(this, KeyboardHook));
				KeyboardHook = IntPtr.Zero;
			}
		}

		[SuppressMessage("Style", "IDE0004:Remove Unnecessary Cast", Justification = "wParam & lParam require a redundant cast")]
		IntPtr KeyboardHookProc(int nCode, int wParam, int lParam)
		{
			bool isHandled = false;

			if (nCode == SafeNativeMethods.HC_ACTION)
			{
				if (Form.ModifierKeys == (Keys.Alt | Keys.Control) && IsKeyDown(lParam))
				{
					isHandled = HandleKey(wParam);
				}
			}

			if (!isHandled)
			{
				return UnsafeNativeMethods.CallNextHookEx(new HandleRef(this, KeyboardHook), nCode, (IntPtr)wParam, (IntPtr)lParam);
			}
			else
			{
				return new IntPtr(1);
			}
		}

		/// <summary>
		/// Returns true if the key is handled, false otherwise
		/// </summary>
		internal bool HandleKey(int wParam)
		{
			bool isHandled = false;

			switch ((Keys)wParam)
			{
				case Keys.Up:
				case Keys.NumPad8:
					MoveUp();
					isHandled = true;
					break;

				case Keys.Down:
				case Keys.NumPad2:
					MoveDown();
					isHandled = true;
					break;

				case Keys.Left:
				case Keys.NumPad4:
					MoveLeft();
					isHandled = true;
					break;

				case Keys.Right:
				case Keys.NumPad6:
					MoveRight();
					isHandled = true;
					break;

				case Keys.Add:
				case Keys.Oemplus:
					MagnifyManager.ChangeZoom(true);
					isHandled = true;
					break;

				case Keys.Subtract:
				case Keys.OemMinus:
					MagnifyManager.ChangeZoom(false);
					isHandled = true;
					break;

				case Keys.H:
				case Keys.NumPad5:
					WindowState = (WindowState == FormWindowState.Minimized) ? FormWindowState.Normal : FormWindowState.Minimized;
					isHandled = true;
					break;

				case Keys.PageUp:
				case Keys.NumPad9:
					MovePageTop();
					isHandled = true;
					break;

				case Keys.PageDown:
				case Keys.NumPad3:
					MovePageBottom();
					isHandled = true;
					break;

				case Keys.T:
				case Keys.NumPad0:
					ChangeToolBarVisibility();
					isHandled = true;
					break;

				case Keys.F:
				case Keys.Multiply:
					MagnifyManager.Zoom = "0";
					isHandled = true;
					break;

				case Keys.Home:
				case Keys.NumPad7:
					MoveWindowTop();
					isHandled = true;
					break;

				case Keys.End:
				case Keys.NumPad1:
					MoveWindowBottom();
					isHandled = true;
					break;
			}
			return isHandled;
		}

		internal bool IsKeyDown(int lParam)
		{
			return (unchecked((uint)lParam) >> 31 == 0);
		}

#endif
		#endregion

		#region Context Menu

#if !WINZOR
		void SetupMainContextMenu()
		{
			ContextMenu mainMenu = new ContextMenu();

			MenuItem curItem;

			curItem = new ZMenuItem(ResString.GetMultilingualString("eDocsMagnify|EraseSelectionBox", "Erase Selection Box"), new EventHandler(RemoveSelectionBox));
			mainMenu.MenuItems.Add(curItem);

			curItem = new ZMenuItem(ResString.GetMultilingualString("eDocsMagnify|ConvertToText", "Convert to Text"), new EventHandler(DoPerformOCR));
			mainMenu.MenuItems.Add(curItem);

			curItem = new ZMenuItem("-");
			mainMenu.MenuItems.Add(curItem);

			curItem = new ZMenuItem(ResString.GetMultilingualString("eDocsMagnify|EraseSelectionColumns", "Erase Selection Box Columns"), new EventHandler(ClearColumns));
			mainMenu.MenuItems.Add(curItem);

			curItem = new ZMenuItem(ResString.GetMultilingualString("eDocsMagnify|ConvertToTable", "Convert to Table"), new EventHandler(ConvertToTable));
			mainMenu.MenuItems.Add(curItem);

			ContextMenu = mainMenu;
		}
#endif

		#endregion

		#region Zoom Magnification DropEdit methods

#if !WINZOR
		void ZoomDropEdit_KeyDown(object sender, KeyEventArgs e)
		{
			if (!ZoomDropEdit.IsDroppedDown)
			{
				switch (e.KeyCode)
				{
					case Keys.Down:
						MoveDown();
						e.Handled = true;
						break;
					case Keys.Up:
						MoveUp();
						e.Handled = true;
						break;
					case Keys.Right:
						MoveRight();
						e.Handled = true;
						break;
					case Keys.Left:
						MoveRight();
						e.Handled = true;
						break;
				}
			}
		}
#endif
		#endregion

		#region Dispose
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>

		void CleanUp()
		{
			if (!DesignMode && fFormWasLoaded)
			{
				if (PictureBox.Image != null)
				{
					PictureBox.Image.Dispose();
				}
			}
		}
		#endregion

		void ChangeNavigationMode()
		{
			PictureBox.Cursor = NavigationMode ? GetOpenHand() : Cursors.Default;
			RemoveSelectionBox(this, EventArgs.Empty);
		}

#if !WINZOR
		public void UpdateLatestImage(float extraMovementProportionX, float extraMovementProportionY, bool updateImmediately)
		{
			if (MagnifyManager != null)
			{
				if ((ImagePanel.ClientSize.Width <= 0) && (ImagePanel.ClientSize.Height <= 0))
				{
					return;
				}

				try
				{
					Image newImage = MagnifyManager.CreateLatestImageSection(ImagePanel.ClientSize, extraMovementProportionX, extraMovementProportionY);

					if (newImage != null)
					{
						if (PictureBox.Image != null)
						{
							PictureBox.Image.Dispose();
						}

						PictureBox.Image = newImage;
						UpdateTitle();

						if (updateImmediately)
						{
							Update();
						}
					}
				}
				catch (OutOfMemoryException) //I00201638 Exception ID E00028282-AHN-GLO. Not a real OoM, thrown by GDI+ to indicate generic failure
				{
					Globals.Message.ShowError(Res.GetString("57e08fb3-e24b-4ea4-9d5d-5e8592edae30",
						"A GDI+ error occurred while attempting to render this image, most likely due to it being in a format that GDI+ (Windows graphics library) cannot read. Try loading and exporting the image from a graphics program."));
				}
			}
		}
#endif

		#region Form Events

		void MagnifyForm_Load(object sender, EventArgs e)
		{
			fFormWasLoaded = true;
		}

		void MagnifyForm_Resize(object sender, EventArgs e)
		{
			BeginInvoke(new EventHandler(AfterResize), new object[] { this, EventArgs.Empty });
		}

		void AfterResize(object sender, EventArgs e)
		{
			UpdateLatestImage(0.0F, 0.0F, false);
		}
		#endregion

#if !WINZOR
		#region Toolbar Events

		void MagnifyToolBar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
		{
			object curTag = e.Button.Tag;

			switch ((string)curTag)
			{
				case "LeftButton":
					MoveLeft();
					break;
				case "UpButton":
					MoveUp();
					break;
				case "DownButton":
					MoveDown();
					break;
				case "RightButton":
					MoveRight();
					break;
				case "OCRButton":
					PerformOCR();
					break;
				case "SelectTextButton":
					NavigationMode = !(e.Button.Pushed);
					ChangeNavigationMode();
					break;
				case "ClearSelectionButton":
					RemoveSelectionBox(null, EventArgs.Empty);
					break;
			}
		}
		#endregion

		#region Image Navigation (move up, down, left, right)

		void MoveLeft()
		{
			UpdateLatestImage(-0.02F, 0.0F, true);
		}

		void MoveUp()
		{
			UpdateLatestImage(0.0F, -0.02F, true);
		}

		void MoveDown()
		{
			UpdateLatestImage(0.0F, 0.02F, true);
		}

		void MoveRight()
		{
			UpdateLatestImage(0.02F, 0.0F, true);
		}

		void MovePageTop()
		{
			UpdateLatestImage(0.0F, -1F, true);
		}

		void MovePageBottom()
		{
			UpdateLatestImage(0.0F, 1F, true);
		}

		void MoveWindowTop()
		{
			DesktopBounds = ControlDpiScalingHelper.NewScaledRectangle(0, 0, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(CachedScreenInfo.Instance.PrimaryScreenInfo.Width), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Height));
		}

		void MoveWindowBottom()
		{
			DesktopBounds = ControlDpiScalingHelper.NewScaledRectangle(
				0,
				ControlDpiScalingHelper.UnscaleFromCurrentDpiY(CachedScreenInfo.Instance.PrimaryScreenInfo.Height - Height),
				ControlDpiScalingHelper.UnscaleFromCurrentDpiX(CachedScreenInfo.Instance.PrimaryScreenInfo.Width),
				ControlDpiScalingHelper.UnscaleFromCurrentDpiY(Height));
		}
		#endregion
#endif

		#region OCR functions

		void DoPerformOCR(object sender, EventArgs e)
		{
			PerformOCR();
		}

		internal void PerformOCR()
		{
			// check for image first
			// get rectangle from the points clicked by the user on MouseDown events
			// if no rectangle tell them 
			// check the box is larger than 0
			// get an intersection of the image from the box 
			// OCR the image
			// show the results, replacing \n with \r\n

			if (PictureBox.Image == null)
			{
				Globals.Message.ShowInformation(Res.GetString("d111c180-4cce-4d51-9568-84c1203e1c4f", "Invalid request - there is no current image."));
				return;
			}

			if (fSelectionBoxStart != fSelectionBoxEnd)
			{
				Rectangle croppedBox = GetRectangleFromPoints(fSelectionBoxStart, fSelectionBoxEnd);
				croppedBox = IntersectionBox(croppedBox);

				if (croppedBox.Width > 0 && croppedBox.Height > 0)
				{
					// CroppedBox is in Client co-ordinates.
					string filename = DocumentUtilities.GetTempFilename();

					try
					{
						CreateOCRImage(croppedBox, filename);

						string resultText;
						string errMsg = DoOCR(filename, out resultText);

						if (!string.IsNullOrEmpty(errMsg))
						{
							Globals.Message.ShowInformation(errMsg);
							return;
						}

						OCRResultForm resultForm = new OCRResultForm();
						resultForm.DisplayText = resultText.Replace("\n", "\r\n");
						resultForm.ShowDialog(this);
					}
					finally
					{
						File.Delete(filename);
					}
				}
				return;
			}

			Globals.Message.ShowInformation(Res.GetString("5bb05016-8346-489d-8ad7-94323ddcf5f2", "Use the mouse to select a portion of the image to convert to text first."), Res.GetString("f650a74f-f2d4-4aab-a7cc-5abc8ce9404b", "Select Portion"));
		}

		void CreateOCRImage(Rectangle croppedBox, string filename)
		{
			float proportionX;
			float proportionY;

			float proportionWidth;
			float proportionHeight;

			proportionWidth = croppedBox.Width / (float)PictureBox.ClientSize.Width;
			proportionHeight = croppedBox.Height / (float)PictureBox.ClientSize.Height;

			proportionX = croppedBox.Left / (float)PictureBox.ClientSize.Width;
			proportionY = croppedBox.Top / (float)PictureBox.ClientSize.Height;

			float aLeft;
			float aTop;
			float aHeight;
			float aWidth;

			aWidth = proportionWidth * MagnifyManager.DisplayRectangleInPixels.Width;
			aHeight = proportionHeight * MagnifyManager.DisplayRectangleInPixels.Height;

			aLeft = MagnifyManager.DisplayRectangleInPixels.Left + proportionX * MagnifyManager.DisplayRectangleInPixels.Width;
			aTop = MagnifyManager.DisplayRectangleInPixels.Top + proportionY * MagnifyManager.DisplayRectangleInPixels.Height;

			RectangleF srcRect = new RectangleF(aLeft, aTop, aWidth, aHeight);

			using (Bitmap wholePicture = MagnifyManager.ExtractImageSection(srcRect))
			{
				if (wholePicture != null)
				{
					wholePicture.Save(filename, System.Drawing.Imaging.ImageFormat.Tiff);
				}
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1079:DoNotCompareOnExceptionMessage", Justification = "The message below pretty much says the same thing so if it fails we dont care")]
		string DoOCR(string fileName, out string resultText)
		{
			string errMsg = "";
			resultText = "";

			TiffToText tiffer = new TiffToText();
			if (tiffer.IsOCRAvailable())
			{
				tiffer.UseExistingTextInFile = true;

				try
				{
					resultText = tiffer.Convert(fileName);
				}
				catch (OCRError e)
				{
					if (e.Message == Res.GetString("ac6f80f0-4b30-45a4-8ac4-c5467745032c", "Cannot recognize text in this document. OCR was not successful."))
					{
						errMsg = Res.GetString("869e7a56-4ca7-4203-96c7-f0b5f9f93a08", "Cannot convert the selected image to text as no text was recognized.");
					}
					else
					{
						errMsg = Res.GetString("5795fd7c-fc7c-4d46-83f5-d613a391b37c", "Could not perform conversion to text. Reason :\r\n{0}", e.Message);
					}
				}
			}
			else if (tiffer.IsMODI2003Available())
			{
				errMsg = Res.GetString("c32310c2-9d2a-4e30-b380-f56be8b6bac8", "This feature requires Microsoft Office XP Document Imaging.\r\nDocument Imaging with Office 2003 is currently not supported.");
			}
			else
			{
				errMsg = Res.GetString("5118347c-67ca-49d1-bfd1-4d6758774a5c", "This feature requires Microsoft Office XP Document Imaging.\r\nPlease check that it has been installed correctly on this PC.");
			}

			return errMsg;
		}

		#endregion

		Rectangle GetRectangleFromPoints(Point point1, Point point2)
		{
			int aLeft;
			int aWidth;

			if (point1.X < point2.X)
			{
				aLeft = point1.X;
				aWidth = point2.X - point1.X;
			}
			else
			{
				aLeft = point2.X;
				aWidth = point1.X - point2.X;
			}

			int aTop;
			int aHeight;

			if (point1.Y < point2.Y)
			{
				aTop = point1.Y;
				aHeight = point2.Y - point1.Y;
			}
			else
			{
				aTop = point2.Y;
				aHeight = point1.Y - point2.Y;
			}

			return ControlDpiScalingHelper.NewScaledRectangle(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(aLeft), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(aTop), ControlDpiScalingHelper.UnscaleFromCurrentDpiX(aWidth), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(aHeight));
		}

		Rectangle IntersectionBox(Rectangle subSetBox)
		{
			Rectangle imageRect = ControlDpiScalingHelper.NewScaledRectangle(0, 0, ControlDpiScalingHelper.UnscaleFromCurrentDpiX(PictureBox.Size.Width) - 1, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(PictureBox.Size.Height) - 1);
			imageRect.Intersect(subSetBox);

			return imageRect;
		}

		#region Picture Box Events & Methods

#if !WINZOR
		Point fCroppingColumnsStart = Point.Empty;
		Point fCroppingColumnsEnd = Point.Empty;
		Point fSavedCroppingColumnsStart = Point.Empty;
#endif

		public PictureBox GetImagePictureBox()
		{
			return PictureBox;
		}

		void InvalidatePictureBox(object sender, EventArgs e)
		{
			PictureBox.Refresh();
		}

#if !WINZOR
		bool fCroppingActive;
		bool fCroppingColumnsActive;
		int fCroppingColumnsXPos;

		#region Picture Box MouseDown
		internal void PictureBox_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button != System.Windows.Forms.MouseButtons.Left)
			{
				return;
			}

			if (PictureBox.Image != null)
			{
				if (NavigationMode)
				{
					fSelectionBoxStart = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y));
					fSelectionBoxEnd = fSelectionBoxStart;
					fCroppingActive = true;
					PictureBox.Cursor = GetClosedHand();
				}
				else
				{
					if (!fCroppingActive && !fCroppingColumnsActive)
					{
						Point justClickedPoint = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y));
						bool boxAlreadyDrawn = fSelectionBoxStart != fSelectionBoxEnd;
						bool newPointOutsideExistingBox = IsPointInsideRegion(justClickedPoint, fSelectionBoxStart, fSelectionBoxEnd);
						bool columnsDrawn = fSelectionBoxColumnOffsets.Count > 0;

						if (!boxAlreadyDrawn || (newPointOutsideExistingBox && !columnsDrawn))
						{
							RemoveSelectionBox(this, EventArgs.Empty);
							Cursor.Clip = PictureBox.RectangleToScreen(PictureBox.ClientRectangle);

							fSelectionBoxStart = justClickedPoint;
							fSelectionBoxEnd = fSelectionBoxStart;
							fCroppingActive = true;

							BeginInvoke(new EventHandler(InvalidatePictureBox), new object[] { this, EventArgs.Empty });
						}
						else
						{
							Rectangle curRectangle = GetRectangleFromPoints(fSelectionBoxStart, fSelectionBoxEnd);
							if (curRectangle.Contains(justClickedPoint))
							{
								// Possibly starting a column draw.
								int curX = justClickedPoint.X;

								Point useStart = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(curX), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(curRectangle.Y));

								fCroppingColumnsActive = true;
								fCroppingColumnsStart = justClickedPoint;
								fCroppingColumnsEnd = justClickedPoint;
								fSavedCroppingColumnsStart = justClickedPoint;
								return;
							}
						}
					}
				}
			}
		}

		bool IsPointInsideRegion(Point testPoint, Point regionStart, Point regionEnd)
		{
			return (testPoint.X < regionStart.X && testPoint.X < regionEnd.X) ||
				(testPoint.X > regionStart.X && testPoint.X > regionEnd.X) ||
				(testPoint.Y < regionStart.Y && testPoint.Y < regionEnd.Y) ||
				(testPoint.Y > regionStart.Y && testPoint.Y > regionEnd.Y);
		}

		#endregion

		#region Picture Box MouseUp

		void PictureBox_MouseUp(object sender, MouseEventArgs e)
		{
			if (fCroppingActive && NavigationMode)
			{
				fCroppingActive = false;
				PictureBox.Cursor = GetOpenHand();
			}
			else if (fCroppingActive)
			{
				fCroppingActive = false;
				Cursor.Clip = Rectangle.Empty;  // turn off cropping

				PictureBox.Invalidate();
			}
			else if (fCroppingColumnsActive)
			{
				fCroppingColumnsActive = false;
				Cursor.Clip = Rectangle.Empty;

				Rectangle rect = GetRectangleFromPoints(fSelectionBoxStart, fSelectionBoxEnd);

				int curOffset = fCroppingColumnsXPos - rect.Left;

				if ((curOffset > 0) && (curOffset < rect.Width))
				{
					if (!fSelectionBoxColumnOffsets.Contains(curOffset))
					{
						fSelectionBoxColumnOffsets.Add(curOffset);
					}
				}

				PictureBox.Invalidate();
			}
		}

		#endregion

		#region Picture Box MouseMove

		void PictureBox_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				if (fCroppingActive && NavigationMode)
				{
					int xChange = fSelectionBoxStart.X - e.X;
					int yChange = fSelectionBoxStart.Y - e.Y;

					var foundPortion = PixelDeltaToProportion(xChange, yChange);
					if (foundPortion.HasValue)
					{
						UpdateLatestImage(foundPortion.Value.X, foundPortion.Value.Y, true);
						fSelectionBoxStart = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y));
					}
				}
				else if (fCroppingActive || fCroppingColumnsActive)
				{
					var newPoint = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(e.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(e.Y));

					if (fCroppingActive)
					{
						if (newPoint != fSelectionBoxStart)
						{
							if (fSelectionBoxStart != fSelectionBoxEnd)
							{
								// rub out previous box.
								DrawRubberBand(fSelectionBoxStart, fSelectionBoxEnd);
							}

							DrawRubberBand(fSelectionBoxStart, newPoint);
							fSelectionBoxEnd = newPoint;
						}
					}
					else if (fCroppingColumnsActive)
					{
						fCroppingColumnsXPos = 0;

						if (fCroppingColumnsStart != fCroppingColumnsEnd)
						{
							// rub out previous line.
							DrawReversibleLine(fCroppingColumnsStart, fCroppingColumnsEnd);
						}

						var rect = GetRectangleFromPoints(fSelectionBoxStart, fSelectionBoxEnd);

						if (rect.Contains(newPoint))
						{
							if (IsValidColumnLine(fSavedCroppingColumnsStart, newPoint, rect))
							{
								fCroppingColumnsXPos = fSavedCroppingColumnsStart.X;
								fCroppingColumnsStart = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(fCroppingColumnsXPos), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(rect.Y));
								fCroppingColumnsEnd = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(fCroppingColumnsXPos), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(rect.Y + rect.Height));
							}
							else
							{
								fCroppingColumnsStart = fSavedCroppingColumnsStart;
								fCroppingColumnsEnd = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(fSavedCroppingColumnsStart.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(newPoint.Y));
							}

							DrawReversibleLine(fCroppingColumnsStart, fCroppingColumnsEnd);
						}
					}
				}
			}
		}

		PointF? PixelDeltaToProportion(int deltaX, int deltaY)
		{
			var displaySizeInProportions = MagnifyManager.DisplaySizeInProportions;
			if (!displaySizeInProportions.HasValue)
			{
				return null;
			}

			var proportionBoxWidth = deltaX / (float)PictureBox.ClientSize.Width;
			var proportionBoxHeight = deltaY / (float)PictureBox.ClientSize.Height;

			var portionOverallWidth = proportionBoxWidth * displaySizeInProportions.Value.Width;
			var portionOverallHeight = proportionBoxHeight * displaySizeInProportions.Value.Height;

			return new PointF(portionOverallWidth, portionOverallHeight);
		}

		void DrawReversibleLine(Point point1, Point point2)
		{
			Point pointScreen1 = this.PictureBox.PointToScreen(point1);
			Point pointScreen2 = this.PictureBox.PointToScreen(point2);
			// PointScreen2 = new Point(PointScreen2.X + 1, PointScreen2.Y); 

			Rectangle rectScreen = GetRectangleFromPoints(pointScreen1, pointScreen2);
			//ControlPaint.DrawReversibleFrame(RectScreen, Color.Aqua /*this.PictureBox.BackColor */, FrameStyle.Dashed);

			ControlPaint.DrawReversibleLine(pointScreen1, pointScreen2, Color.Aqua);
		}

		bool IsValidColumnLine(Point p1, Point p2, Rectangle boxRect)
		{
			bool isValid = false;

			int fullHeight = boxRect.Height;

			Rectangle lineRect = GetRectangleFromPoints(p1, p2);
			if (lineRect.Width < 20)
			{
				if (boxRect.Height > 0)
				{
					if ((lineRect.Height / (float)boxRect.Height) >= 0.3333F)
					{
						isValid = true;
					}
				}
			}
			return isValid;
		}

		// Points in picturebox co-ordinates.
		void DrawRubberBand(Point point1, Point point2)
		{
			Point pointScreen1 = this.PictureBox.PointToScreen(point1);
			Point pointScreen2 = this.PictureBox.PointToScreen(point2);

			Rectangle rectScreen = GetRectangleFromPoints(pointScreen1, pointScreen2);

			ControlPaint.DrawReversibleFrame(rectScreen, Color.Aqua /*this.PictureBox.BackColor */, FrameStyle.Dashed);
		}

		#endregion

		#region PictureBox Paint

		void PictureBox_Paint(object sender, PaintEventArgs e)
		{
			if (PictureBox.Image != null && !NavigationMode)
			{
				if (fSelectionBoxStart != fSelectionBoxEnd)
				{
					var rect = GetRectangleFromPoints(fSelectionBoxStart, fSelectionBoxEnd);

					rect = IntersectionBox(rect);

					e.Graphics.DrawRectangle(Pens.Red, rect);

					foreach (int curColumnXOffset in fSelectionBoxColumnOffsets)
					{
						int curColumnX = curColumnXOffset + rect.Left;

						var topColumnPoint = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(curColumnX), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(rect.Y));
						var bottomColumnPoint = ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(curColumnX), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(rect.Y + rect.Height));

						e.Graphics.DrawLine(Pens.Red, topColumnPoint, bottomColumnPoint);
					}
				}
			}
		}

		#endregion
#endif
		#endregion

		#region Selection Box methods

		void RemoveSelectionBox(object sender, EventArgs e)
		{
			if (fFormWasLoaded)
			{
				fSelectionBoxStart = Point.Empty;
				fSelectionBoxEnd = fSelectionBoxStart;
				fSelectionBoxColumnOffsets.Clear();

				BeginInvoke(new EventHandler(InvalidatePictureBox), new object[] { this, EventArgs.Empty });
			}
		}

		void ClearColumns(object sender, EventArgs e)
		{
			fSelectionBoxColumnOffsets.Clear();
			BeginInvoke(new EventHandler(InvalidatePictureBox), new object[] { this, EventArgs.Empty });
		}

		void ConvertToTable(object sender, EventArgs e)
		{
			if (PictureBox.Image == null)
			{
				Globals.Message.ShowInformation(Res.GetString("d111c180-4cce-4d51-9568-84c1203e1c4f", "Invalid request - there is no current image."));
				return;
			}

			if (fSelectionBoxStart != fSelectionBoxEnd)
			{
				Rectangle croppedBox = GetRectangleFromPoints(fSelectionBoxStart, fSelectionBoxEnd);
				croppedBox = IntersectionBox(croppedBox);

				if (croppedBox.Width > 0 && croppedBox.Height > 0)
				{
					// Here, CroppedBox is the whole box.
					// Need to OCR for each column.
					fSelectionBoxColumnOffsets.Sort();
					Rectangle[] columnRectangles = GetColumnRectangles(croppedBox, fSelectionBoxColumnOffsets);

					OCRResultGridData resultData = new OCRResultGridData(columnRectangles.Length);

					for (int columnIndex = 0; columnIndex < columnRectangles.Length; columnIndex++)
					{
						Rectangle curColumnBox = columnRectangles[columnIndex];

						// CurColumnBox is in Client co-ordinates.
						string filename = DocumentUtilities.GetTempFilename();

						try
						{
							CreateOCRImage(curColumnBox, filename);

							string resultText;
							string errMsg = DoOCR(filename, out resultText);

							if (!string.IsNullOrEmpty(errMsg))
							{
								int oneBasedColumnIndex = columnIndex + 1;
								errMsg += "\n" + Res.GetString("be21e826-df10-4b2e-b94f-7ac73466f6a2", "Column with problem : {0}", oneBasedColumnIndex.ToString());
								Globals.Message.ShowInformation(errMsg);
								resultText = "";
							}

							StringCollection curLines = new StringCollection();

							if (!string.IsNullOrEmpty(resultText))
							{
								curLines = OCRResultGridData.ConvertStringToCollection(resultText);
							}

							resultData.SetColumnData(curLines, columnIndex);
						}
						finally
						{
							File.Delete(filename);
						}
					}

					// At this point, ResultData has been set up.
					OCRResultGridForm resultForm = new OCRResultGridForm();
					resultForm.SynchData(resultData);
					resultForm.ShowDialog(this);
				}
				return;
			}

			Globals.Message.ShowInformation(Res.GetString("5bb05016-8346-489d-8ad7-94323ddcf5f2", "Use the mouse to select a portion of the image to convert to text first."), Res.GetString("f650a74f-f2d4-4aab-a7cc-5abc8ce9404b", "Select Portion"));
		}

		Rectangle[] GetColumnRectangles(Rectangle croppedBox, ArrayList aColumnOffsets)
		{
			fSelectionBoxColumnOffsets.Sort();
			int previousOffset = 0;

			Rectangle columnRect;

			ArrayList al = new ArrayList();

			for (int ii = 0; ii < aColumnOffsets.Count; ii++)
			{
				int curColumnOffset = (int)aColumnOffsets[ii];

				if ((curColumnOffset > 0) && (curColumnOffset < croppedBox.Width))
				{
					columnRect = GetColumnRectangle(croppedBox, previousOffset, curColumnOffset);
					if (columnRect.Width != 0)
					{
						al.Add(columnRect);
					}
					previousOffset = curColumnOffset;
				}
				else
				{
					break;
				}
			}

			// do the last column.
			columnRect = GetColumnRectangle(croppedBox, previousOffset, croppedBox.Width);
			if (columnRect.Width != 0)
			{
				al.Add(columnRect);
			}

			Rectangle[] result = new Rectangle[al.Count];
			for (int i = 0; i < al.Count; i++)
			{
				result[i] = (Rectangle)al[i];
			}
			return result;
		}

		Rectangle GetColumnRectangle(Rectangle croppedBox, int offsetStart, int offsetEnd)
		{
			if (!((croppedBox.Width > 0) && (croppedBox.Height > 0) &&
				(offsetStart >= 0) && (offsetStart < croppedBox.Width) &&
				(offsetStart < offsetEnd) &&
				(offsetEnd > 0) && (offsetEnd <= croppedBox.Width)))
			{
				return Rectangle.Empty;
			}

			int aX, aY, aWidth, aHeight;

			aY = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(croppedBox.Y);
			aX = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(croppedBox.X + offsetStart);
			aHeight = ControlDpiScalingHelper.UnscaleFromCurrentDpiY(croppedBox.Height);
			aWidth = ControlDpiScalingHelper.UnscaleFromCurrentDpiX(offsetEnd - offsetStart);

			return ControlDpiScalingHelper.NewScaledRectangle(aX, aY, aWidth, aHeight);
		}
		#endregion

		void ExpandCollapseButton_Click(object sender, EventArgs e)
		{
			ChangeToolBarVisibility();
		}

		void ChangeToolBarVisibility()
		{
			ToolbarPanel.Visible = !ToolbarPanel.Visible;
			ExpandCollapseButton.ImageIndex = ToolbarPanel.Visible ? 1 : 0;
			UpdateLatestImage(0.0f, 0.0f, true);
		}

		void UpdateTitle()
		{
			Text = Res.GetString("bbb86a14-33c6-4f98-acc0-73aea94e97fe", "Magnifying Window");
			if (!MagnifyManager.TitleText.IsEmpty)
			{
				Text += MagnifyManager.TitleText;
			}
		}

		#region Dispose

		System.ComponentModel.IContainer components;
		protected override void Dispose(bool isNotFinalizing)
		{
#if !WINZOR
			RemoveKeyboardHook();
#endif

			if (isNotFinalizing)
			{
				CleanUp();

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
