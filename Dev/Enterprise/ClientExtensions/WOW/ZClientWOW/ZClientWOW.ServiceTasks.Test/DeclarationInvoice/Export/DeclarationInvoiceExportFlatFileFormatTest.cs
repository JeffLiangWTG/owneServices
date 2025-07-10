using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice.Export.Testing
{
	public class DeclarationInvoiceExportFlatFileFormatTest : FlatFileFormatTestCase
	{
		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new DeclarationInvoiceExportFlatFileFormat();
		}

		public void TestFileExtensionForExport()
		{
			AssertEquals("File Extension For Export", FileExtensionType.Txt, new DeclarationInvoiceExportFlatFileFormat().FileExtensionForExport);
		}

		public void TestConvertToLine()
		{
			FlatFileDataRow dataRow = new FlatFileDataRow(new string[] { "value1", "value2" });
			AssertEquals("ConvertToLine", dataRow.ToString(), new DeclarationInvoiceExportFlatFileFormat().ConvertToLine(dataRow));
		}

		public void TestFileExtensionForImport()
		{
			AssertEquals("File Extension For Export", FileExtensionType.None, new DeclarationInvoiceExportFlatFileFormat().FileExtensionForImport);
		}
	}
}
