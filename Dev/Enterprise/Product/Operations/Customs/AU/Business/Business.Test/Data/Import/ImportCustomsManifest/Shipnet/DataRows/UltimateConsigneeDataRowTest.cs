using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UltimateConsigneeDataRowTest : BaseDataRowTest
	{
		protected override void TestParseCore()
		{
			AssertEquals("GGG", dataRow[ShipnetConstants.Common.RecordID]);
			AssertEquals("SSSSSSSSSSSSSSS", dataRow[ShipnetConstants.PartyDetails.PartyCode]);
			AssertEquals("NNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNN", dataRow[ShipnetConstants.PartyDetails.PartyName]);
			AssertEquals("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", dataRow[ShipnetConstants.PartyDetails.PartyAddress1]);
			AssertEquals("TTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT", dataRow[ShipnetConstants.PartyDetails.PartyAddress2]);
			AssertEquals("GGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGG", dataRow[ShipnetConstants.PartyDetails.PartyAddress3]);
			AssertEquals("VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV", dataRow[ShipnetConstants.PartyDetails.PartyAddress4]);
			AssertEquals("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD", dataRow[ShipnetConstants.PartyDetails.PartyAddress5]);
			AssertEquals("GGGGGGGGGG", dataRow[ShipnetConstants.PartyDetails.PartyACNNumber]);
			AssertEquals("OOOOOOOOOOOOOOOOOOOO", dataRow[ShipnetConstants.PartyDetails.PartyFwdrRegNo]);
			AssertEquals("HHHHHHHHHHHHHHHHHHHH", dataRow[ShipnetConstants.PartyDetails.PartyReference]);
		}

		protected override BaseDataRow GetNewDataRow(ZString rawRow) => new UltimateConsigneeDataRow(rawRow);

		protected override ZString RawRow => "GGGSSSSSSSSSSSSSSSNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNNCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGGVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDGGGGGGGGGGOOOOOOOOOOOOOOOOOOOOHHHHHHHHHHHHHHHHHHHH";
	}
}
