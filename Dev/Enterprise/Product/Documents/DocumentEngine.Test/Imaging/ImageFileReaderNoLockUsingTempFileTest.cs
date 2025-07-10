using NUnit.Framework;

namespace Enterprise.DocumentEngine.Imaging.Testing
{
	sealed class ImageFileReaderNoLockUsingTempFileTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBasic()
		{
			var reader = new ImageFileReaderNoLockUsingTempFile(BaseSourcePath + @"Enterprise\Product\Documents\DocumentScanning\DocumentScanning.Business.Test\TestDocs\small_3pages.tif");
			try
			{
				Assert("Image", reader.PageSelector.CurrentImage != null);
				AssertEquals("Page Count", 3, reader.PageSelector.TotalPages);
			}
			finally
			{
				reader.Dispose();
			}
		}
	}
}
