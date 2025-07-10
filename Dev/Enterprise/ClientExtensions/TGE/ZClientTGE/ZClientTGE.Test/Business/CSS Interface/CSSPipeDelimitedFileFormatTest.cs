using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.TGE.Business.Testing.CSSInterface
{
	public class CSSPipeDelimitedFileFormatTest : FlatFileFormatTestCase
	{
		public void TestConvertToLine()
		{
			string expected = "Hello|How|Are|You";
			FlatFileDataRow row = new FlatFileDataRow(4);
			row.SetField(0, "Hello");
			row.SetField(1, "How");
			row.SetField(2, "Are");
			row.SetField(3, "You");
			AssertEquals("The line did not format as expected", expected, FileFormat.ConvertToLine(row));
		}

		public virtual void TestConvertToRow()
		{
			CSSPipeDelimitedFileFormat format = new CSSPipeDelimitedFileFormat();
			FlatFileDataRow dataRow = format.ConvertToRow("hello|cello|test123");
			AssertEquals(3, dataRow.FieldCount);
			AssertEquals("hello", dataRow.GetField(0));
			AssertEquals("cello", dataRow.GetField(1));
			AssertEquals("test123", dataRow.GetField(2));
		}

		public void TestFileExtensionForExport()
		{
			AssertEquals("File Extension", FileExtensionType.ClientSpecific, FileFormat.FileExtensionForExport);
		}

		protected override FlatFileFormat GetFlatFileFormat()
		{
			return FileFormat;
		}

		CSSPipeDelimitedFileFormat FileFormat
		{
			get
			{
				return fileFormat ?? (fileFormat = new CSSPipeDelimitedFileFormat());
			}
		}

		CSSPipeDelimitedFileFormat fileFormat;
	}
}
