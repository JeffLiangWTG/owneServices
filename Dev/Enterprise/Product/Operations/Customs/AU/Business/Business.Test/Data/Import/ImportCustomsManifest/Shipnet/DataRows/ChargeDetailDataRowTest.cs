using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ChargeDetailDataRowTest : BaseDataRowTest
	{
		protected override void TestParseCore()
		{
			AssertEquals("CHL", dataRow[ShipnetConstants.Common.RecordID]);
			AssertEquals("AAAAAAAAAA", dataRow[ShipnetConstants.ChargeDetail.ChargeCode]);
			AssertEquals("P", dataRow[ShipnetConstants.ChargeDetail.PrepaidCollect]);
			AssertEquals("AUD", dataRow[ShipnetConstants.ChargeDetail.Currency]);
			AssertEquals("1230.000", dataRow[ShipnetConstants.ChargeDetail.Amount]);
		}

		protected override BaseDataRow GetNewDataRow(ZString rawRow) => new ChargeDetailDataRow(rawRow);

		protected override ZString RawRow => "CHLAAAAAAAAAAPAUD             1230.000";
	}
}
