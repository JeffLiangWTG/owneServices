using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.NZP.CMS.Testing
{
	public class CMSFlatFileFormatTest : FlatFileFormatTestCase
	{
		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new CMSFlatFileFormat();
		}

		public void TestFileExtension()
		{
			CMSFlatFileFormat format = new CMSFlatFileFormat();
			AssertEquals("File Extension for export should be ClientSpecific", FileExtensionType.ClientSpecific, format.FileExtensionForExport);
			AssertEquals("File Extension for import should be None", FileExtensionType.None, format.FileExtensionForImport);
		}

		public void TestConvertToLine()
		{
			CMSFlatFileFormat format = new CMSFlatFileFormat();
			ZString result = format.ConvertToLine(new CMSFlatFileDataRow());
			AssertEquals("Number of pipes should be 36", 36, result.Occurrences("|"));
		}
	}
}
