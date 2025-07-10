using CargoWise.Types;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class DelimitedFlatFileFormatTest : FlatFileFormatTestCase
	{
		public void TestConvertToLine()
		{
			FlatFileDataRow dataRow = new FlatFileDataRow(3);
			dataRow.SetField(0, "ab");
			dataRow.SetField(1, "cd");
			dataRow.SetField(2, "ef");

			DelimitedFlatFileFormat format = new DelimitedFlatFileFormatForTest(';');
			AssertEquals("\"ab\";\"cd\";\"ef\"", format.ConvertToLine(dataRow));

			format = new DelimitedFlatFileFormatForTest('|');
			AssertEquals("\"ab\"|\"cd\"|\"ef\"", format.ConvertToLine(dataRow));
		}

		public void TestConvertToRow()
		{
			ZString testRow = "ab,cd,ef";
			DelimitedFlatFileFormat format = new DelimitedFlatFileFormatForTest(';');
			FlatFileDataRow resultRow = format.ConvertToRow(testRow);
			AssertEquals(1, resultRow.FieldCount);
			AssertEquals("ab,cd,ef", resultRow.GetField(0));

			format = new DelimitedFlatFileFormatForTest(',');
			resultRow = format.ConvertToRow(testRow);
			AssertEquals("ab", resultRow.GetField(0));
			AssertEquals("cd", resultRow.GetField(1));
			AssertEquals("ef", resultRow.GetField(2));

			ZString testRow2 = "1\t2\t3\t4";
			format = new DelimitedFlatFileFormatForTest('\t');
			resultRow = format.ConvertToRow(testRow2);
			AssertEquals("1", resultRow.GetField(0));
			AssertEquals("2", resultRow.GetField(1));
			AssertEquals("3", resultRow.GetField(2));
			AssertEquals("4", resultRow.GetField(3));
		}

		public void TestFileExtensionForExport()
		{
			DelimitedFlatFileFormat format = new DelimitedFlatFileFormatForTest(';');
			AssertEquals("Default should be TXT file extension type", FileExtensionType.Txt, format.FileExtensionForExport);
		}

		public void TestFileExtensionForImport()
		{
			DelimitedFlatFileFormat format = new DelimitedFlatFileFormatForTest(';');
			AssertEquals("Default should be TXT file extension type", FileExtensionType.Txt, format.FileExtensionForImport);
		}

		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new DelimitedFlatFileFormatForTest(',');
		}

		#region DelimitedFlatFileFormatForTest

		class DelimitedFlatFileFormatForTest : DelimitedFlatFileFormat
		{
			public DelimitedFlatFileFormatForTest(char delimiter)
			{
				this.delimiter = delimiter;
			}

			protected override char Delimiter
			{
				get { return delimiter; }
			}
			readonly char delimiter;

			protected override bool IncludeQuotesWhenExporting
			{
				get { return true; }
			}
		}

		#endregion
	}
}
