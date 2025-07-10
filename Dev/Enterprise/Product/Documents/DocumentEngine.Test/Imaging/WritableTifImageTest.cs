using System.Drawing;
using System.Drawing.Imaging;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Imaging.Testing
{
	sealed class WritableTifImageTest : TransactionedTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddPageWithOnePage()
		{
			using (var tempFile = TempFile.NewWithExtension("tif"))
			{
				using (var page = Image.FromFile(OnePageImageFile))
				{
					using (var tifImage = new WritableTifImage(tempFile.Filename))
					{
						tifImage.AddPage(page);
					}
				}

				using (var outputImage = Image.FromFile(tempFile.Filename))
				{
					AssertEquals("There should be one page in the output file", 1, outputImage.GetFrameCount(FrameDimension.Page));
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddPageWithTwoPages()
		{
			using (var tempFile = TempFile.NewWithExtension("tif"))
			{
				using (var page = Image.FromFile(OnePageImageFile))
				{
					using (var tifImage = new WritableTifImage(tempFile.Filename))
					{
						tifImage.AddPage(page);
						tifImage.AddPage(page);
					}
				}

				using (var outputImage = Image.FromFile(tempFile.Filename))
				{
					AssertEquals("There should be two pages in the output file", 2, outputImage.GetFrameCount(FrameDimension.Page));
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			OnePageImageFile = PrintProcessingConstants.TestWorkingDirectory + PrintProcessingConstants.TestSingleLandscapeFileName;
		}

		string OnePageImageFile;
	}
}
