using CargoWise.Types;

namespace Enterprise.DataTransfer.Business.Testing
{
	public class CsvFlatFileFormatTest : FlatFileFormatTestCase
	{
		public void TestConvertToLineWithoutQuotes()
		{
			CsvFlatFileFormat format = new CsvFlatFileFormat(false);
			FlatFileDataRow dataRow = new FlatFileDataRow(3);
			dataRow.SetField(0, "ab");
			dataRow.SetField(1, "cd");
			dataRow.SetField(2, "ef");

			ZString expected = "ab,cd,ef";

			AssertEquals("Did not format a correct CSV line from FlatFileDataRow", expected, format.ConvertToLine(dataRow));
		}

		public void TestConvertToLine()
		{
			CsvFlatFileFormat format = new CsvFlatFileFormat();
			FlatFileDataRow dataRow = new FlatFileDataRow(3);
			dataRow.SetField(0, "ab");
			dataRow.SetField(1, "cd");
			dataRow.SetField(2, "ef");

			ZString expected = "\"ab\",\"cd\",\"ef\"";

			AssertEquals("Did not format a correct CSV line from FlatFileDataRow", expected, format.ConvertToLine(dataRow));
		}

		public void TestConvertToRow()
		{
			ZString testRow = "\"ab\",\"cd\",\"ef\"";

			CsvFlatFileFormat format = new CsvFlatFileFormat();
			FlatFileDataRow resultRow = format.ConvertToRow(testRow);

			AssertEquals("ab", resultRow.GetField(0));
			AssertEquals("cd", resultRow.GetField(1));
			AssertEquals("ef", resultRow.GetField(2));
		}

		public void TestFileExtensionForExport()
		{
			CsvFlatFileFormat format = new CsvFlatFileFormat();
			AssertEquals("Should always be a CSV file extension type", FileExtensionType.Csv, format.FileExtensionForExport);
		}

		public void TestFileExtensionForImport()
		{
			CsvFlatFileFormat format = new CsvFlatFileFormat();
			AssertEquals("Should always be a CSV file extension type", FileExtensionType.Csv, format.FileExtensionForImport);
		}

		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new CsvFlatFileFormat();
		}
	}
}
