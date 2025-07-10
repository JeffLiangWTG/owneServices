namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class AutoDetectFlatFileFormatTest : FlatFileFormatTestCase
	{
		public void TestConvertToRowWhenCommaDelimited()
		{
			FlatFileDataRow row = Format.ConvertToRow("abc,cde,efg");

			AssertEquals(3, row.FieldCount);
			AssertEquals("abc", row[0]);
			AssertEquals("cde", row[1]);
			AssertEquals("efg", row[2]);
		}

		public void TestConvertToRowWhenPipeDelimited()
		{
			FlatFileDataRow row = Format.ConvertToRow("a|b,c,d,e|f,g|h,i|j");

			AssertEquals(5, row.FieldCount);
			AssertEquals("a", row[0]);
			AssertEquals("b,c,d,e", row[1]);
			AssertEquals("f,g", row[2]);
			AssertEquals("h,i", row[3]);
			AssertEquals("j", row[4]);
		}

		public void TestFileExtensionForImport()
		{
			AssertEquals("FileExtensionForImport", FileExtensionType.All, Format.FileExtensionForImport);
		}

		AutoDetectFlatFileFormat Format
		{
			get
			{
				if (fFormat == null)
				{
					fFormat = (AutoDetectFlatFileFormat)GetFlatFileFormat();
				}
				return fFormat;
			}
		}
		AutoDetectFlatFileFormat fFormat;

		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new AutoDetectFlatFileFormat();
		}
	}
}
