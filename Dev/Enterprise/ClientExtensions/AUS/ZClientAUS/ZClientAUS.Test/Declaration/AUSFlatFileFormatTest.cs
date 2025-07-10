using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.AUS.Testing
{
	public class AUSFlatFileFormatTest : CsvFlatFileFormatTest
	{
		public void TestFileExtensionType()
		{
			AUSFlatFileFormat flatFileFormat = new AUSFlatFileFormat();
			AssertEquals("File Extension Type Should be for a Text File", FileExtensionType.Txt, flatFileFormat.FileExtensionForImport);
		}
	}
}
