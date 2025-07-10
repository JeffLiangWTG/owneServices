using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.DocumentEngine.Imaging.Testing;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	sealed class MagnifyFormTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWhenMagnifyFormIsDisposed()
		{
			using (var imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (var magManager = new MagnifyManager(imageSource))
			{
				Image img = null;
				using (var form = new MagnifyForm(magManager))
				{
					form.Show();
					img = form.PictureBox.Image;
				}

				AssertNoExceptionThrown("MagnifyManager Image should NOT have been disposed", () => {
					_ = magManager.LatestBitmap.Width;
					_ = magManager.LatestBitmap.Height;
				});

				AssertExceptionThrown<ArgumentException>("MagnifyForm Image should have been disposed", () => { _ = img.Height; });
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetOpenHandCursor()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager magManager = new MagnifyManager(imageSource))
			using (MagnifyForm form = new MagnifyForm(magManager))
			{
				AssertNotNull("Cursor shouldn't be null", form.GetOpenHand());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetClosedHandCursor()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager magManager = new MagnifyManager(imageSource))
			using (MagnifyForm form = new MagnifyForm(magManager))
			{
				AssertNotNull("Cursor shouldn't be null", form.GetClosedHand());
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestPictureBoxHasDockNone()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager magManager = new MagnifyManager(imageSource))
			using (MagnifyForm form = new MagnifyForm(magManager))
			{
				AssertEquals("PictureBox has Dock = None", DockStyle.None, form.PictureBox.Dock);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateLatestImage_InvalidNewImage()
		{
			var imageSource = new DummyIImagePageSelector(BaseSourcePath);
			var manager = new Mock<MagnifyManager>(imageSource, null) { CallBase = true };
			using (MagnifyForm form = new MagnifyForm(manager.Object))
			{
				Bitmap flag = new Bitmap(200, 100);
				form.PictureBox.Image = flag;
				manager.Setup(m => m.CreateLatestImageSection(It.IsAny<Size>(), It.IsAny<float>(), It.IsAny<float>())).Returns((Image)null);
				form.UpdateLatestImage(1f, 1f, true);
				AssertEquals("Update Lastest Image should keep previous behavior when invalid new image generating", flag, form.PictureBox.Image);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCreateOCRImage_WhenExtractNullImage()
		{
			var imageSource = new DummyIImagePageSelector(BaseSourcePath);
			var manager = new Mock<MagnifyManager>(imageSource, null) { CallBase = true };
			using (MagnifyForm form = new MagnifyForm(manager.Object))
			{
				form.PictureBox.Image = new Bitmap(200, 100);
				form.fSelectionBoxEnd = new Point(10, 10);
				AssertNoExceptionThrown("ExtractImageSection null image", form.PerformOCR);
			}
		}

#if !WINZOR
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestHandleKey()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager magManager = new MagnifyManager(imageSource))
			using (MagnifyForm form = new MagnifyForm(magManager))
			{
				magManager.MagForm = form;
				form.Show();

				AssertEquals("A minus key - should be handled", true, form.HandleKey(107));
				AssertEquals("An alphanumeric key (not t or h)- ignored)", false, form.HandleKey(87));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestKeyDown()
		{
			using (DummyIImagePageSelector imageSource = new DummyIImagePageSelector(BaseSourcePath))
			using (MagnifyManager magManager = new MagnifyManager(imageSource))
			using (MagnifyForm form = new MagnifyForm(magManager))
			{
				magManager.MagForm = form;
				form.Show();

				AssertEquals(true, form.IsKeyDown(1));
				AssertEquals(false, form.IsKeyDown(-1));
			}
		}
#endif
	}

	[TestedType(typeof(MagnifyForm))]
	[DatCapabilityRequirement("SOURCE_CODE")]
	sealed class MagnifyFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			control = new GraphicalDisplayControl();
			imageSource = new DummyIImagePageSelector(BaseSourcePath);
			manager = new MagnifyManager(imageSource, control.DocumentPreviewPictureBox);

			var sender = control.DocumentPreviewPictureBox;
			var mouseArgs = new MouseEventArgs(MouseButtons.Left, 1, 20, 20, 0);
			manager.PictureBox_MouseDown(sender, mouseArgs);

			TypeDescriptor.AddAttributes(manager.MagForm.ExpandCollapseButton, new SuppressFormsLocalizedTestAttribute());

			return manager.MagForm;
		}

		protected override void TearDown()
		{
			imageSource?.Dispose();
			manager?.Dispose();
			control?.Dispose();
			base.TearDown();
		}

		GraphicalDisplayControl control;
		MagnifyManager manager;
		DummyIImagePageSelector imageSource;
	}
}
