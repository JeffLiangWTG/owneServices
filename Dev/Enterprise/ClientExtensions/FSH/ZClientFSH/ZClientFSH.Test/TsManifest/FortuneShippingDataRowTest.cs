using CargoWise.Types;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.FSH.TsManifest.Testing
{
	public class FortuneShippingDataRowTest : FlatFileDataRowTest
	{
		public void TestZStringArray()
		{
			ZString[] test = { "foo", "bar" };
			FortuneShippingDataRow row = new FortuneShippingDataRow(test, 5);
			AssertEquals(5, row.FieldCount);
			AssertEquals("foo", row[0]);
			AssertEquals("bar", row[1]);
			AssertEquals("", row[2]);
			AssertEquals("", row[3]);
			AssertEquals("", row[4]);
			row = new FortuneShippingDataRow(test, 0);
			AssertEquals(2, row.FieldCount);
			AssertEquals("foo", row[0]);
			AssertEquals("bar", row[1]);
		}

		public void TestRowType()
		{
			ZString[] test = { "5", "bar" };
			FortuneShippingDataRow row = new FortuneShippingDataRow(test, 2);
			AssertEquals(5, row.RowType);
		}
	}
}
