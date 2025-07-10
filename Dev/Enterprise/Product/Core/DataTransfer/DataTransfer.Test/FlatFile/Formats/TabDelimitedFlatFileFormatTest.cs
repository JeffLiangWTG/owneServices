namespace Enterprise.DataTransfer.Business.Testing
{
	public class TabDelimitedFlatFileFormatTest : FlatFileFormatTestCase
	{
		public void TestConvertToLine()
		{
			string expected = "Hello\tHow\tAre\tYou";
			FlatFileDataRow row = new FlatFileDataRow(4);
			row.SetField(0, "Hello");
			row.SetField(1, "How");
			row.SetField(2, "Are");
			row.SetField(3, "You");

			TabDelimitedFlatFileFormat format = new TabDelimitedFlatFileFormat();
			AssertEquals("The line did not format as expected", expected, format.ConvertToLine(row));
		}

		public virtual void TestConvertToRow()
		{
			TabDelimitedFlatFileFormat format = new TabDelimitedFlatFileFormat();
			FlatFileDataRow dataRow = format.ConvertToRow("hello\tcello\ttest123");
			AssertEquals(3, dataRow.FieldCount);
			AssertEquals("hello", dataRow.GetField(0));
			AssertEquals("cello", dataRow.GetField(1));
			AssertEquals("test123", dataRow.GetField(2));
		}

		public void TestFileExtensionForExport()
		{
			TabDelimitedFlatFileFormat format = new TabDelimitedFlatFileFormat();
			AssertEquals("File Extension", FileExtensionType.Txt, format.FileExtensionForExport);
		}

		public void TestFileExtensionForImport()
		{
			TabDelimitedFlatFileFormat format = new TabDelimitedFlatFileFormat();
			AssertEquals("File Extension", FileExtensionType.Txt, format.FileExtensionForImport);
		}

		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new TabDelimitedFlatFileFormat();
		}
	}
}
