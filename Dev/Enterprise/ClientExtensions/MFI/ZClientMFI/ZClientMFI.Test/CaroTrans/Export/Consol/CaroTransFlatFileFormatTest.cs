using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.MFI.CaroTrans.Testing
{
	public class CaroTransFlatFileFormatTest : FlatFileFormatTestCase
	{
		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new CaroTransFlatFileFormat();
		}

		public void TestFileExtension()
		{
			CaroTransFlatFileFormat format = new CaroTransFlatFileFormat();
			AssertEquals("File Extension for export should be Client specific ", FileExtensionType.ClientSpecific, format.FileExtensionForExport);
			AssertEquals("Client Specific File Extension", Constants.CargoTransDefaultFileExtension, format.ClientSpecificExtensionForTesting);
		}
	}
}
