using System.Drawing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Imaging.Testing
{
	sealed class StandardImagePageSelectorTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBasic()
		{
			using (var testImage = Image.FromFile(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small_3pages.tif"))
			{
				using (var selector = new StandardImagePageSelector(testImage))
				{
					AssertNotNull("Image", selector.CurrentImage);
					AssertEquals("Total pages", 3, selector.TotalPages);
					AssertEquals("Current index", 0, selector.CurrentPageIndex);
				}
			}
		}

		public void TestNoImage()
		{
			using (var selector = new StandardImagePageSelector(null))
			{
				AssertNull("Image", selector.CurrentImage);
				AssertEquals("Total pages", 0, selector.TotalPages);
				AssertEquals("CurIndex", -1, selector.CurrentPageIndex);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalPagesWithGif()
		{
			using (var testImage = Image.FromFile(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small.gif"))
			{
				using (var selector = new StandardImagePageSelector(testImage))
				{
					AssertNotNull("Image", selector.CurrentImage);
					AssertEquals("Total pages", 1, selector.TotalPages);
					AssertEquals("Current index", 0, selector.CurrentPageIndex);
				}
			}
		}
	}
}
