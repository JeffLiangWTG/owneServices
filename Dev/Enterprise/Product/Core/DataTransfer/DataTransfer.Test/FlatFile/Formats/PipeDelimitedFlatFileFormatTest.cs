namespace Enterprise.DataTransfer.Business.Testing
{
	class PipeDelimitedFlatFileFormatTest : FlatFileFormatTestCase
	{
		public void TestConvertToLine()
		{
			string expected = "Hello|How|Are|You";
			FlatFileDataRow row = new FlatFileDataRow(4);
			row.SetField(0, "Hello");
			row.SetField(1, "How");
			row.SetField(2, "Are");
			row.SetField(3, "You");

			PipeDelimitedFlatFileFormat format = new PipeDelimitedFlatFileFormat();
			AssertEquals("The line did not format as expected", expected, format.ConvertToLine(row));
		}

		public virtual void TestConvertToRow()
		{
			PipeDelimitedFlatFileFormat format = new PipeDelimitedFlatFileFormat();
			FlatFileDataRow dataRow = format.ConvertToRow("hello|ce,llo|te\tst123");
			AssertEquals(3, dataRow.FieldCount);
			AssertEquals("hello", dataRow.GetField(0));
			AssertEquals("ce,llo", dataRow.GetField(1));
			AssertEquals("te\tst123", dataRow.GetField(2));
		}

		public void TestFileExtensionForExport()
		{
			PipeDelimitedFlatFileFormat format = new PipeDelimitedFlatFileFormat();
			AssertEquals("File Extension", FileExtensionType.Txt, format.FileExtensionForExport);
		}

		public virtual void TestFileExtensionForImport()
		{
			PipeDelimitedFlatFileFormat format = new PipeDelimitedFlatFileFormat();
			AssertEquals("File Extension", FileExtensionType.Txt, format.FileExtensionForImport);
		}

		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new PipeDelimitedFlatFileFormat();
		}
	}
}
