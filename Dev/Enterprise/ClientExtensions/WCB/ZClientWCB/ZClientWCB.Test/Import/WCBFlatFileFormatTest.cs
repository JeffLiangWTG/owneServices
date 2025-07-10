using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.WCB.Testing
{
	public class WCBFlatFileFormatTest : CsvFlatFileFormatTest
	{
		public static void TestFileExtensionType()
		{
			WCBFlatFileFormat flatFileFormat = new WCBFlatFileFormat();
			AssertEquals("File Extension Type Should be for a Text File", FileExtensionType.Txt, flatFileFormat.FileExtensionForImport);
		}
	}
}
