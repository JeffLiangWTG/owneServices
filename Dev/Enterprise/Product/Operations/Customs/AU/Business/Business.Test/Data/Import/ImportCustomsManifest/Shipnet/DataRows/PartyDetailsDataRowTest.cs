using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class PartyDetailsDataRowTest : BaseDataRowTest
	{
		protected override void TestParseCore()
		{
			AssertEquals("III", dataRow[ShipnetConstants.Common.RecordID]);
			AssertEquals("JJJJJJJJJJJJJJJ", dataRow[ShipnetConstants.PartyDetails.PartyCode]);
			AssertEquals("MMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM", dataRow[ShipnetConstants.PartyDetails.PartyName]);
			AssertEquals("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE", dataRow[ShipnetConstants.PartyDetails.PartyAddress1]);
			AssertEquals("UUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUU", dataRow[ShipnetConstants.PartyDetails.PartyAddress2]);
			AssertEquals("YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY", dataRow[ShipnetConstants.PartyDetails.PartyAddress3]);
			AssertEquals("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", dataRow[ShipnetConstants.PartyDetails.PartyAddress4]);
			AssertEquals("FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF", dataRow[ShipnetConstants.PartyDetails.PartyAddress5]);
			AssertEquals("VVVVVVVVVVVVVVVVVVVV", dataRow[ShipnetConstants.PartyDetails.PartyACNNumber]);
			AssertEquals("TTTTTTTTTTTTTTTTTTTT", dataRow[ShipnetConstants.PartyDetails.PartyFwdrRegNo]);
			AssertEquals("QQQQQQQQQQQQQQQQQQQQ", dataRow[ShipnetConstants.PartyDetails.PartyReference]);
		}

		protected override BaseDataRow GetNewDataRow(ZString rawRow) => new PartyDetailsDataRow(rawRow);

		protected override ZString RawRow => "IIIJJJJJJJJJJJJJJJMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUUYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFVVVVVVVVVVVVVVVVVVVVTTTTTTTTTTTTTTTTTTTTQQQQQQQQQQQQQQQQQQQQ";
	}
}
