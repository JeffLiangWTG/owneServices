using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.DocumentEngine.Imaging.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	[TestedType(typeof(MagnifyManager))]
	[GuiTest]
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class MagnifyManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestUpdatePictureInMagnificationWindow()
		{
			using (Image testImage100dpi = DocumentUtilities.GetImageFromFile(TestingConstants.TIF_Squares_100dpi))
			using (StandardImagePageSelectorForTesting pageSelector = new StandardImagePageSelectorForTesting())
			using (MagnifyManager manager = new MagnifyManager(pageSelector))
			using (MagnifyForm magForm = new MagnifyForm(manager))
			{
				pageSelector.SetImageForTesting(testImage100dpi);
				manager.MagForm = magForm;
				magForm.Show();
				magForm.PictureBox.Image = null;
				AssertNull("Magnification window contents null", magForm.PictureBox.Image);

				manager.UpdatePictureInMagnificationWindow();
				AssertNotNull("magnification window contents should not be null any more", magForm.PictureBox.Image);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions()]
		public void TestSaveRegistrySettingsWithInvalidZoomStillWorks()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			{
				MagnifyManager manager = new MagnifyManager(imageSource);
				manager.Zoom = "-"; // set to something invalid - should theoretically never happen
				manager.SaveRegistrySettings();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDispose()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				manager.Zoom = "57";
				manager.Zoom = "32";
			}

			AssertEquals("registry item should update with the latest fZoom chosen", 32, Env.Registry.DMMagnifyingGlassSettings.MagnificationPercentage);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDBHitsForSaveDMMagnifyingGlassSettings()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			{
				MagnifyManager manager = new MagnifyManager(imageSource);

				var magnificationPercentage = Env.Registry.DMMagnifyingGlassSettings.MagnificationPercentage;
				manager.Zoom = magnificationPercentage.ToString();

				var commandCount = Db.Connection.ExecutedCommandCount;
				manager.SaveRegistrySettings();
				var newCommandCount = Db.Connection.ExecutedCommandCount;

				AssertEquals("Should not execute update DMMagnifyingGlassSettings command", commandCount, newCommandCount);
			}
		}

		public void TestDisplaySizeInProportions()
		{
			using (var testImage = Image.FromFile(TestingConstants.TIF_Resolution200x100))
			using (var imageSource = new StandardImagePageSelectorForTesting())
			{
				imageSource.SetImageForTesting(testImage);

				using (var manager = new MagnifyManager(imageSource))
				{
					manager.fDisplayRectangleInPixels = new RectangleF(10, 10, 100, 100);
					AssertEquals("Manager.DisplaySizeInProportions Width", 100 / (float)testImage.Width, manager.DisplaySizeInProportions.Value.Width);
					AssertEquals("Manager.DisplaySizeInProportions Height", 100 / (float)(testImage.Height * 2), manager.DisplaySizeInProportions.Value.Height);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDisplayRectangleInPixels()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				RectangleF testRectangle = new RectangleF(10, 10, 100, 100);
				manager.fDisplayRectangleInPixels = testRectangle;
				AssertEquals(testRectangle, manager.DisplayRectangleInPixels);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAdjustRectangle()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				SizeF availableSize = new SizeF(20, 20);

				RectangleF srcRect = new RectangleF(10, 10, 10, 10);
				RectangleF foundRect = manager.AdjustRectangleToBeWhollyWithinAvailableSpace(srcRect, availableSize, 0.0F);
				RectangleF expectedRect = new RectangleF(10, 10, 10, 10);
				AssertEquals("Complete overlap", expectedRect, foundRect);

				// X, Y, Width, Height.
				srcRect = new RectangleF(50, 50, 20, 20);
				foundRect = manager.AdjustRectangleToBeWhollyWithinAvailableSpace(srcRect, availableSize, 0.0F);
				expectedRect = new RectangleF(0, 0, 0, 0);
				AssertEquals("No overlap", expectedRect, foundRect);

				// X, Y, Width, Height.
				srcRect = new RectangleF(5, 2, 30, 30);
				foundRect = manager.AdjustRectangleToBeWhollyWithinAvailableSpace(srcRect, availableSize, 0.0F);
				expectedRect = new RectangleF(5, 2, 15, 18);
				AssertEquals("Some overlap", expectedRect, foundRect);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAdjustRectangleWithOverflowDelta()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				SizeF availableSize = new SizeF(20, 20);

				RectangleF srcRect = new RectangleF(10, 10, 10, 10);
				RectangleF foundRect = manager.AdjustRectangleToBeWhollyWithinAvailableSpace(srcRect, availableSize, 2.0F);
				RectangleF expectedRect = new RectangleF(10, 10, 8, 8);
				AssertEquals("Complete overlap", expectedRect, foundRect);

				// X, Y, Width, Height.
				srcRect = new RectangleF(50, 50, 20, 20);
				foundRect = manager.AdjustRectangleToBeWhollyWithinAvailableSpace(srcRect, availableSize, 2.0F);
				expectedRect = new RectangleF(0, 0, 0, 0);
				AssertEquals("No overlap", expectedRect, foundRect);

				// X, Y, Width, Height.
				srcRect = new RectangleF(5, 2, 30, 30);
				foundRect = manager.AdjustRectangleToBeWhollyWithinAvailableSpace(srcRect, availableSize, 1.0F);
				expectedRect = new RectangleF(5, 2, 14, 17);
				AssertEquals("Some overlap", expectedRect, foundRect);
			}
		}

		public void TestNullPageSelector()
		{
			var imageProvider = new Mock<IImagePageSelectorProvider>();
			imageProvider.Setup(m => m.PageSelector).Returns((IImagePageSelector)null);

			using (var manager = new MagnifyManager(imageProvider.Object))
			using (var form = new MagnifyForm(manager))
			{
				AssertNoExceptionThrown("The form should be able to handle no image being selected", () =>
				{
					form.Show();

					form.Size = form.MaximumSize;
					manager.MagForm = form;
					manager.MagnificationValue = 5;
				});

				AssertNull("If no file is selected, the created image should be null", manager.CreateLatestImageSection(new Size(200, 200), 0, 0));
			}
			imageProvider.VerifyAll();
		}

		public void TestNullPageSelector_DisplaySizeInProportions()
		{
			var imageProvider = new Mock<IImagePageSelectorProvider>();
			imageProvider.Setup(m => m.PageSelector).Returns((IImagePageSelector)null);

			using (var manager = new MagnifyManager(imageProvider.Object))
			using (var form = new MagnifyForm(manager))
			{
				AssertNoExceptionThrown("Shouldn't throw null exception", () =>
				{
					form.Show();
					var temp = manager.DisplaySizeInProportions;
				});

				AssertNull("DisplaySizeInProportions should return null", manager.DisplaySizeInProportions);
			}
			imageProvider.VerifyAll();
		}

		/// <summary>
		/// This test can fail on unusual screen resolutions - it is dependent on the desktop resolution to calculate magnification.
		/// </summary>
		[GuiTest]
		public void TestCreateLatestImageSection()
		{
			using (Image testImage100dpi = DocumentUtilities.GetImageFromFile(TestingConstants.TIF_Squares_100dpi))
			using (StandardImagePageSelectorForTesting pageSelector = new StandardImagePageSelectorForTesting())
			using (MagnifyManager manager = new MagnifyManager(pageSelector))
			using (MagnifyForm magForm = new MagnifyForm(manager))
			{
				magForm.Show();
				magForm.Size = magForm.MaximumSize;
				manager.MagForm = magForm;
				manager.MagnificationValue = 5;
				pageSelector.SetImageForTesting(testImage100dpi);

				Bitmap magnifiedImage500Percent = (Bitmap)manager.CreateLatestImageSection(new Size(200, 200), 0, 0);
				AssertOnlyOneColour(magnifiedImage500Percent, Color.White);
			}

			using (Image testImage100dpi = DocumentUtilities.GetImageFromFile(TestingConstants.TIF_Squares_100dpi))
			using (StandardImagePageSelectorForTesting pageSelector = new StandardImagePageSelectorForTesting())
			using (MagnifyManager manager = new MagnifyManager(pageSelector))
			using (MagnifyForm magForm = new MagnifyForm(manager))
			{
				magForm.Show();
				magForm.Size = magForm.MaximumSize;
				manager.MagForm = magForm;
				manager.MagnificationValue = 0.5F;
				pageSelector.SetImageForTesting(testImage100dpi);

				Bitmap magnifiedImage50percent = (Bitmap)manager.CreateLatestImageSection(new Size(200, 200), 0, 0);
				Assert("the 50% magnified image should contain black", ContainsColour(magnifiedImage50percent, Color.Black));
			}
		}

		[GuiTest]
		public void TestExtractImageSection_WithEvenResolution()
		{
			using (Image testImage100dpi = DocumentUtilities.GetImageFromFile(TestingConstants.TIF_Squares_100dpi))
			using (StandardImagePageSelectorForTesting pageSelector = new StandardImagePageSelectorForTesting())
			using (MagnifyManager manager = new MagnifyManager(pageSelector))
			using (MagnifyForm magForm = new MagnifyForm(manager))
			{
				pageSelector.SetImageForTesting(testImage100dpi);
				var extractedImage = manager.ExtractImageSection(new RectangleF(0, 0, 40, 40));
				AssertOnlyOneColour(extractedImage, Color.White);

				extractedImage = manager.ExtractImageSection(new Rectangle(51, 51, 40, 40));
				AssertOnlyOneColour(extractedImage, Color.Black);
			}
		}

		[GuiTest]
		public void TestExtractImageSection_WithUnevenResolution()
		{
			using (Image testImageUnevenResolution = DocumentUtilities.GetImageFromFile(TestingConstants.TIF_Resolution200x100))
			using (StandardImagePageSelectorForTesting pageSelector = new StandardImagePageSelectorForTesting())
			using (MagnifyManager manager = new MagnifyManager(pageSelector))
			using (MagnifyForm magForm = new MagnifyForm(manager))
			{
				pageSelector.SetImageForTesting(testImageUnevenResolution);
				var extractedImage = manager.ExtractImageSection(new RectangleF(0, 500, 100, 60));
				// there is a black pixel in the square but due to the resolution the rectangle will be adjusted and 
				// the black should not be included in the crop. 
				AssertOnlyOneColour(extractedImage, Color.White);
			}
		}

		void AssertOnlyOneColour(Bitmap image, Color expectedColour)
		{
			for (int x = 0; x < image.Width; x++)
			{
				for (int y = 0; y < image.Height; y++)
				{
					AssertEquals(String.Format("Image should be all one colour ({0}). Current coord ({1}, {2}) Colour: {3} Screen resolution(s): {4}", expectedColour.Name, x, y, image.GetPixel(x, y).Name, GetScreenResolutionInformation()),
						expectedColour.ToArgb(), image.GetPixel(x, y).ToArgb());
				}
			}
		}

		string GetScreenResolutionInformation()
		{
			string infos = string.Empty;
			foreach (Rectangle screenInfo in CachedScreenInfo.Instance.ScreenInfos)
			{
				infos += string.Format("({0}x{1}) ", screenInfo.Width, screenInfo.Height);
			}
			return infos;
		}

		bool ContainsColour(Bitmap image, Color expectedColour)
		{
			bool containsColour = false;
			for (int x = 0; x < image.Width; x++)
			{
				for (int y = 0; y < image.Height; y++)
				{
					if (image.GetPixel(x, y).ToArgb() == expectedColour.ToArgb())
					{
						containsColour = true;
						break;
					}
				}
			}
			return containsColour;
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestZoomValueList()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				AssertEquals("Combo dropdown has one more than the list dropdown accounting for FitToWidth option", manager.ZoomValues.Count + 1, manager.Zoom_List.Count);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestChangeZoom()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				AssertEquals("Load up with registry stettings by default", manager.MagnificationValue, Env.Registry.DMMagnifyingGlassSettings.MagnificationPercentage / 100f);

				manager.Zoom = "125";
				manager.ChangeZoom(true);
				AssertEquals("Zoom factor should have increased to next step", "150", manager.Zoom);

				manager.ChangeZoom(false);
				manager.ChangeZoom(false);
				AssertEquals("Zoom factor should have decreased two steps", "100", manager.Zoom);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestZoomCannotBeBlankString()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				ZString before = manager.Zoom;
				manager.Zoom = "";
				AssertEquals("Trying to set fZoom to an empty string shouldn't do anything - will revert to previous value", before, manager.Zoom);

				manager.Zoom = "50";
				AssertEquals("Trying with values will work", "50", manager.Zoom);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestZoom()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				manager.Zoom = "25";
				AssertEquals(0.25f, manager.MagnificationValue);

				manager.Zoom = "150";
				AssertEquals(1.5f, manager.MagnificationValue);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCalculateDisplayStartPointRegularChange()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				PointF oldPoint = manager.DisplayStartPointInProportions;
				PointF newPoint = manager.CalculateDisplayStartPoint(0.00f, 0.02f);
				Assert("Sent Down movement", newPoint != oldPoint);
				AssertEquals("Movement changed by 0.02", oldPoint.Y + 0.02f, newPoint.Y);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCalculateDisplayStartPointPageUp()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				manager.fDisplayRectangleInPixels = new RectangleF(new PointF(200f, 200f), new SizeF(100, 100));
				manager.CurrentPageIndex = 3;
				manager.DisplayStartPointInProportions = new PointF(0.5f, 0.5f);

				PointF oldPoint = manager.DisplayStartPointInProportions;
				PointF newPoint = manager.CalculateDisplayStartPoint(0.00f, -1.0f);
				Assert("Sent PageUp movement", newPoint != oldPoint);
				AssertEquals("Movement changed to top", 0.0f, newPoint.Y);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCalculateDisplayStartPointPageDown()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				manager.CurrentPageIndex = 3;

				PointF oldPoint = manager.DisplayStartPointInProportions;
				PointF newPoint = manager.CalculateDisplayStartPoint(0.00f, 1.0f);
				Assert("Sent PageDown movement", newPoint != oldPoint);
				AssertEquals("Movement changed to bottom", 1.0f, newPoint.Y);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCalculateDisplayStartPointPageUpAtPageTop()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				manager.CurrentPageIndex = 3;
				manager.DisplayStartPointInProportions = new PointF(0.5f, 0.05f);

				PointF oldPoint = manager.DisplayStartPointInProportions;
				PointF newPoint = manager.CalculateDisplayStartPoint(0.00f, -1.0f);
				Assert("Sent PageUp movement while at top of page", newPoint != oldPoint);
				AssertEquals("Movement changed to bottom of page ", ControlDpiScalingHelper.ScaleToCurrentDpiY(1), (int)newPoint.Y);
				AssertEquals("CurrentPageNumber changed to prevous page", 2, manager.CurrentPageIndex);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCalculateDisplayStartPointPageDownAtPageBottom()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				PointF startPoint = new PointF(imageSource.CurrentImage.Width - 100, imageSource.CurrentImage.Height - 100);
				manager.fDisplayRectangleInPixels = new RectangleF(startPoint, new SizeF(100, 100));
				manager.CurrentPageIndex = 3;

				manager.DisplayStartPointInProportions = new PointF(0.5f, 0.98f);

				PointF oldPoint = manager.DisplayStartPointInProportions;
				PointF newPoint = manager.CalculateDisplayStartPoint(0.00f, 1.0f);
				Assert("Sent PageUp movement while at top of page", newPoint != oldPoint);
				AssertEquals("Movement changed to top of page ", 0.0f, newPoint.Y);
				AssertEquals("CurrentPageNumber changed to next page", 4, manager.CurrentPageIndex);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCurrentPageIndex()
		{
			// the dummy selector has 5 pages, currentpageindex is 0-based indexing
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				AssertEquals("OnLoad, currentpageindex should be at first page", 0, manager.CurrentPageIndex);

				manager.CurrentPageIndex = 2;
				AssertEquals("CurrentPageIndex valid, so it should have been set", 2, manager.CurrentPageIndex);

				manager.CurrentPageIndex = 5;
				AssertEquals("CurrentPageIndex over the bounds of the page count, should still be previous value", 2, manager.CurrentPageIndex);

				manager.CurrentPageIndex = 0;
				AssertEquals("CurrentPageIndex should be set to 0 (first page)", 0, manager.CurrentPageIndex);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestExtractImageSection_CauseWidthOrHeightEqualZero()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				SizeF availableSize = new SizeF(21, 20);
				var bitmap = new Bitmap(imageSource.CurrentImage, new Size(20, 20));
				RectangleF srcRect = new RectangleF(20, 10, 10, 10);
				RectangleF foundRect = manager.AdjustRectangleToBeWhollyWithinAvailableSpace(srcRect, availableSize, 1.0F);
				AssertEquals(RectangleF.Empty, foundRect);
				AssertExceptionThrown(typeof(ArgumentException), "Rectangle '{X=0,Y=0,Width=0,Height=0}' cannot have a width or height equal to 0.", () => bitmap.Clone(foundRect, bitmap.PixelFormat));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest]
		public void TestGetNewZoomFromFitToWidth()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				manager.MagForm = new MagnifyForm(manager);
				manager.MagForm.Show();

				int maxScreenWith = 1920;// Use 1920 as the default max screen width.
				SizeF screenSize = CachedScreenInfo.Instance.FromControl(manager.MagForm).Size;
				if (screenSize.Width > maxScreenWith)
				{
					Size currentClientSize = manager.MagForm.ClientSize;
					var scaleFactor = screenSize.Width * 1.0 / maxScreenWith;
					manager.MagForm.ClientSize = new Size((int)(currentClientSize.Width * scaleFactor), currentClientSize.Height);
				}

				manager.Zoom = "0";
				manager.Zoom = manager.GetNewZoomFromFitToWidth(true).ToString();

				string aboveZoom = manager.Zoom;
				Assert("Zoom no longer fit to width", manager.Zoom != "0");
				Assert("Zoom should be a member of fZoom list", manager.Zoom_List.ContainsCode(manager.Zoom));

				manager.Zoom = "0";
				manager.Zoom = manager.GetNewZoomFromFitToWidth(false).ToString();
				string belowZoom = manager.Zoom;
				Assert("Zoom no longer fit to width", manager.Zoom != "0");
				Assert("Zoom should be a member of fZoom list", manager.Zoom_List.ContainsCode(manager.Zoom));
				Assert("Zoom different", aboveZoom != belowZoom);

				bool testedNext = false;
				for (int i = 0; i < manager.Zoom_List.Count; i++)
				{
					if (manager.Zoom_List[i].Code == belowZoom)
					{
						testedNext = true;
						AssertEquals("Both values are next to each other on the scale", aboveZoom, manager.Zoom_List[i + 1].Code);
					}
				}

				Assert("DIdn't test that the two values are next to each other", testedNext);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTitleText()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				AssertEquals("Title Text", " - Page 1 of 5", manager.TitleText);

				manager.CurrentPageIndex = 2;
				AssertEquals("Title text", " - Page 3 of 5", manager.TitleText);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCurrentPageNumber()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				AssertEquals("Current page number default", 0, manager.CurrentPageIndex);

				Image oldImage = manager.LatestBitmap;

				manager.CurrentPageIndex = 2;
				AssertEquals("Current page number updated", 2, manager.CurrentPageIndex);
				Image newImage = manager.LatestBitmap;

				Assert("Two current images are different - changing page changes the image", !DocumentUtilities.IsCurrentPageEqual(oldImage, newImage));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOnLastPage()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				Assert("first page", !manager.OnLastPage);

				manager.CurrentPageIndex = 2;
				Assert("middle page", !manager.OnLastPage);

				manager.CurrentPageIndex = 4;
				Assert("Last page", manager.OnLastPage);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOnFirstPage()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				Assert("first page", manager.OnFirstPage);

				manager.CurrentPageIndex = 2;
				Assert("middle page", !manager.OnFirstPage);

				manager.CurrentPageIndex = 4;
				Assert("Last page", !manager.OnFirstPage);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions()]
		public void TestCanOpenAndCloseMagnifyingFormMultipleTimes()
		{
			using (GraphicalDisplayControl control = new GraphicalDisplayControl())
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				PictureBox sender = control.DocumentPreviewPictureBox;
				MouseEventArgs mouseArgs = new MouseEventArgs(MouseButtons.Left, 1, 20, 20, 0);
				manager.PictureBox_MouseDown(sender, mouseArgs);
				AssertNotNull("Magnify form is not null", manager.MagForm);
				manager.MagForm.Close();

				mouseArgs = new MouseEventArgs(MouseButtons.Left, 1, 20, 40, 0);
				manager.PictureBox_MouseDown(sender, mouseArgs);

				AssertNotNull("Magnify form is not null", manager.MagForm);
				manager.MagForm.Close();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions()]
		public void TestCanChangeSectionMagnifiedMultipleTimesWithoutClosingWindow()
		{
			using (GraphicalDisplayControl control = new GraphicalDisplayControl())
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				PictureBox sender = control.DocumentPreviewPictureBox;
				MouseEventArgs mouseArgs = new MouseEventArgs(MouseButtons.Left, 1, 20, 20, 0);
				manager.PictureBox_MouseDown(sender, mouseArgs);
				AssertNotNull("Magnify form is not null", manager.MagForm);

				mouseArgs = new MouseEventArgs(MouseButtons.Left, 1, 20, 40, 0);
				manager.PictureBox_MouseDown(sender, mouseArgs);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMagnifyingGlassCursor()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager manager = new MagnifyManager(imageSource))
			{
				AssertNotNull("Magnifying glass cursor should have been retrieved", manager.MagnifyingGlassCursorInternal);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MagnifyManager
			(
				Mock.Of<IImagePageSelector>(o => o.PageSelector == o && o.TotalPages == 2)
			);
		}
	}
}
