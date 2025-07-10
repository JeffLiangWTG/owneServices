using System.Drawing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Imaging.Testing
{
	sealed class DuplicatePageTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBasic()
		{
			string testDoc = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.tif";
			DuplicatePage dupPage = null;
			try
			{
				int origWidth = 0;
				int origHeight = 0;
				Image origImage = Image.FromFile(testDoc);
				try
				{
					origWidth = origImage.Width;
					origHeight = origImage.Height;

					dupPage = new DuplicatePage((Bitmap)origImage);
				}
				finally
				{
					origImage.Dispose();
				}

				AssertEquals("Width", origWidth, dupPage.DuplicatedImage.Width);
				AssertEquals("Width", origHeight, dupPage.DuplicatedImage.Height);
			}
			finally
			{
				dupPage.Dispose();
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultiPage()
		{
			string testDoc = BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\5pages.tif";
			DuplicatePage dupPage = null;
			try
			{
				int origWidth = 0;
				int origHeight = 0;
				Image origImage = Image.FromFile(testDoc);
				try
				{
					origWidth = origImage.Width;
					origHeight = origImage.Height;

					origImage.SelectActiveFrame(System.Drawing.Imaging.FrameDimension.Page, 0);

					AssertEquals("Num pages", 5, origImage.GetFrameCount(System.Drawing.Imaging.FrameDimension.Page));

					dupPage = new DuplicatePage((Bitmap)origImage);
				}
				finally
				{
					origImage.Dispose();
				}

				AssertEquals("Width", origWidth, dupPage.DuplicatedImage.Width);
				AssertEquals("Height", origHeight, dupPage.DuplicatedImage.Height);
				AssertEquals("Num pages", 1, dupPage.DuplicatedImage.GetFrameCount(System.Drawing.Imaging.FrameDimension.Page));
			}
			finally
			{
				dupPage.Dispose();
			}
		}
	}
}
