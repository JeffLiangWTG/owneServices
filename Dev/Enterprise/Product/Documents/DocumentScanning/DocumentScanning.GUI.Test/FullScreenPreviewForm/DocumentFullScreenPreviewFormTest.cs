using System;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.GUI.Testing
{
	[TestedType(typeof(DocumentFullScreenPreviewForm))]
	sealed class DocumentFullScreenPreviewFormTest : ZFormBasherTest
	{
		public void TestDoPreview_OnLoad_WhenImageHeightGreaterThanWidth()
		{
			Form.Show();
			Application.DoEvents();
			AssertEquals(true, Form.DocumentImage.Height > Form.DocumentImage.Width);

			AssertEquals("Expected FitToHeight", true, Form.FitToHeightMenuItem.Checked);
			AssertEquals("Expected FitToHeight", false, Form.FitToWidthMenuItem.Checked);
		}

		[RequiresSTA]
		public void TestDoPreview_OnLoad_WhenImageWidthGreaterThanHeight()
		{
			using (fDocumentImage = new Bitmap(200, 100))
			{
				Form.Show();
				Application.DoEvents();
				AssertEquals(true, Form.DocumentImage.Width > Form.DocumentImage.Height);

				AssertEquals("Expected FitToWidth", false, Form.FitToHeightMenuItem.Checked);
				AssertEquals("Expected FitToWidth", true, Form.FitToWidthMenuItem.Checked);
			}
		}

		public void TestDoPreview_FitToHeight_ImageHeightGreaterThanWidth()
		{
			using (fDocumentImage = new Bitmap(100, 200))
			{
				Form.Show();
				Application.DoEvents();
				Form.DoPreview(ResizeOption.FitToHeight);

				int expectedImageWidth = (int)(Form.Height * ImageAspectRatio);
				Rectangle expectedPictureBounds = new Rectangle(
					(Form.Width - expectedImageWidth) / 2,
					0,
					expectedImageWidth,
					Form.Height);
				AssertEquals(expectedPictureBounds, Form.DocumentPreviewPictureBox.Bounds);
			}
		}

		public void TestDoPreview_FitToHeight_ImageWidthGreaterThanHeight()
		{
			using (fDocumentImage = new Bitmap(200, 100))
			{
				Form.Show();
				Application.DoEvents();
				Form.DoPreview(ResizeOption.FitToHeight);

				int expectedImageWidth = (int)(Form.Height * ImageAspectRatio);
				Rectangle expectedPictureBounds = new Rectangle(
					0,
					0,
					expectedImageWidth,
					Form.Height);
				AssertEquals(expectedPictureBounds, Form.DocumentPreviewPictureBox.Bounds);
			}
		}

		public void TestDoPreview_FitToWidth_ImageWidthGreaterThanHeight()
		{
			using (fDocumentImage = new Bitmap(200, 100))
			{
				Form.Show();
				Application.DoEvents();
				Form.DoPreview(ResizeOption.FitToWidth);

				int expectedImageHeight = (int)(Form.Width / ImageAspectRatio);
				Rectangle expectedPictureBounds = new Rectangle(
					0,
					(Form.Height - expectedImageHeight) / 2,
					Form.Width,
					expectedImageHeight);
				AssertEquals(expectedPictureBounds, Form.DocumentPreviewPictureBox.Bounds);
			}
		}

		public void TestDoPreview_FitToWidth_ImageHeightGreaterThanWidth()
		{
			using (fDocumentImage = new Bitmap(100, 200))
			{
				Form.Show();
				Application.DoEvents();
				Form.DoPreview(ResizeOption.FitToWidth);

				int expectedImageHeight = (int)(Form.Width / ImageAspectRatio);
				Rectangle expectedPictureBounds = new Rectangle(
					0,
					0,
					Form.Width,
					expectedImageHeight);
				AssertEquals(expectedPictureBounds, Form.DocumentPreviewPictureBox.Bounds);
			}
		}

		[ExpectNoExceptions]
		public void TestDoPreview_WithInvalidImage_WhenSettingImageBeforeShowingForm()
		{
			Form.DocumentImage = InvalidImage;
			Form.Show();
			Application.DoEvents();
		}

		[ExpectNoExceptions]
		public void TestDoPreview_WithInvalidImage_WhenSettingImageAfterShowingForm()
		{
			Form.Show();
			Application.DoEvents();
			Form.DocumentImage = InvalidImage;
			Application.DoEvents();
		}

		#region Test Classes

		class TestDocumentFullScreenPreviewForm : DocumentFullScreenPreviewForm
		{
			public new void DoPreview(ResizeOption selectedOption)
			{
				base.DoPreview(selectedOption);
			}

			public new PictureBox DocumentPreviewPictureBox
			{
				get { return base.DocumentPreviewPictureBox; }
			}

			public new ZPanel ImagePanel
			{
				get { return base.ImagePanel; }
			}

			public new ZButton CloseUnboundButton
			{
				get { return base.CloseUnboundButton; }
			}

			public new MenuItem FitToHeightMenuItem
			{
				get { return base.FitToHeightMenuItem; }
			}

			public new MenuItem FitToWidthMenuItem
			{
				get { return base.FitToWidthMenuItem; }
			}
		}

		#endregion

		#region Implementation

		double ImageAspectRatio
		{
			get { return DocumentImage.Width / (double)DocumentImage.Height; }
		}

		protected override Form GetFormToBashCore()
		{
			return Form;
		}

		TestDocumentFullScreenPreviewForm Form
		{
			get
			{
				if (fForm == null)
				{
					fForm = new TestDocumentFullScreenPreviewForm();
					fForm.DocumentImage = DocumentImage;
				}
				return fForm;
			}
		}
		TestDocumentFullScreenPreviewForm fForm;

		Image DocumentImage
		{
			get
			{
				if (fDocumentImage == null)
				{
					fDocumentImage = Image.FromFile(Image100DpiPath);
				}
				return fDocumentImage;
			}
		}
		Image fDocumentImage;

		Image InvalidImage
		{
			get
			{
				if (invalidImage == null)
				{
					invalidImage = new Bitmap(10, 10);
					invalidImage.Dispose();
				}
				return invalidImage;
			}
		}
		Image invalidImage;

		protected override void TearDown()
		{
			base.TearDown();
			fDocumentImage?.Dispose();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
			if (fForm != null)
			{
				fForm.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(StorageDocsBaseTest).Assembly));

		string Image100DpiPath
		{
			get
			{
				if (string.IsNullOrEmpty(image100DpiPath))
				{
					image100DpiPath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentScanning.Business.Test.TestDocs.Image100dpi.tif");
				}
				return image100DpiPath;
			}
		}
		string image100DpiPath;

		#endregion
	}
}
