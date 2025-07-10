using Enterprise.DataTransfer.Business;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business.JXC.Testing
{
	public class JXCFlatFileFormatTest : TestCase
	{
		public void TestConvertToRow()
		{
			FlatFileDataRow row = Format.ConvertToRow("abc;bcd");
			AssertEquals(2, row.FieldCount);
			AssertEquals("abc", row[0]);
			AssertEquals("bcd", row[1]);
			row = Format.ConvertToRow("abc|bcd");
			AssertEquals(1, row.FieldCount);
			AssertEquals("abc|bcd", row[0]);
		}

		public void TestConvertToLine()
		{
			FlatFileDataRow row = new FlatFileDataRow(new string[] { "abc", "bcd" });
			AssertEquals("abc;bcd", Format.ConvertToLine(row));
		}

		readonly JXCFlatFileFormat Format = new JXCFlatFileFormat();
	}
}
