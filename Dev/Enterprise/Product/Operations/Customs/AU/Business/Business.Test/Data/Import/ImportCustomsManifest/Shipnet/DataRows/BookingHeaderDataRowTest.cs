using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BookingHeaderDataRowTest : BaseDataRowTest
	{
		protected override void TestParseCore()
		{
			AssertEquals("TTT", dataRow[ShipnetConstants.Common.RecordID]);
			AssertEquals("LLLLLLLLLLLLLLLLLLLL", dataRow[ShipnetConstants.BookingHeader.ReferenceNo]);
			AssertEquals("HHHHHHHHHHHHHHHHHHHH", dataRow[ShipnetConstants.BookingHeader.SecondaryReference]);
			AssertEquals("GGGGGGGGGG", dataRow[ShipnetConstants.BookingHeader.AcceptanceLocationCode]);
			AssertEquals("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD", dataRow[ShipnetConstants.BookingHeader.AcceptanceLocationName]);
			AssertEquals("ZZZZZ", dataRow[ShipnetConstants.BookingHeader.AcceptancePortCode]);
			AssertEquals("RRRRR", dataRow[ShipnetConstants.BookingHeader.LoadPortCode]);
			AssertEquals("LLLLL", dataRow[ShipnetConstants.BookingHeader.TranshipmentPortCode]);
			AssertEquals("DDDDD", dataRow[ShipnetConstants.BookingHeader.DischargePortCode]);
			AssertEquals("GGGGG", dataRow[ShipnetConstants.BookingHeader.DeliveryPortCode]);
			AssertEquals("EEEEEEEEEE", dataRow[ShipnetConstants.BookingHeader.DeliveryLocationCode]);
			AssertEquals("QQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQ", dataRow[ShipnetConstants.BookingHeader.DeliveryLocationName]);
			AssertEquals("HHHHH", dataRow[ShipnetConstants.BookingHeader.OriginCountryCode]);
			AssertEquals("ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ", dataRow[ShipnetConstants.BookingHeader.OriginCountryName]);
			AssertEquals("KK", dataRow[ShipnetConstants.BookingHeader.DestinationCountryCode]);
			AssertEquals("IIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII", dataRow[ShipnetConstants.BookingHeader.DestinationCountryName]);
			AssertEquals("WWWWW", dataRow[ShipnetConstants.BookingHeader.OriginTerms]);
			AssertEquals("JJJJJ", dataRow[ShipnetConstants.BookingHeader.DestinationTerms]);
			AssertEquals("I", dataRow[ShipnetConstants.BookingHeader.PaymentTerms]);
			AssertEquals("C", dataRow[ShipnetConstants.BookingHeader.EDIFACTCargoType]);
			AssertEquals("BBBBBBBBBB", dataRow[ShipnetConstants.BookingHeader.FAKIndicator]);
			AssertEquals("SSSSSSSS", dataRow[ShipnetConstants.BookingHeader.ETA]);
			AssertEquals("CCCCCCCC", dataRow[ShipnetConstants.BookingHeader.ETD]);
			AssertEquals("H", dataRow[ShipnetConstants.BookingHeader.UnderBond]);
			AssertEquals("QQQQQQQQQQ", dataRow[ShipnetConstants.BookingHeader.OriginPremiseCode]);
			AssertEquals("SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS", dataRow[ShipnetConstants.BookingHeader.OriginPremiseName]);
			AssertEquals("NNNNNNNNNN", dataRow[ShipnetConstants.BookingHeader.DestPremiseCode]);
			AssertEquals("QQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQ", dataRow[ShipnetConstants.BookingHeader.DestPremiseName]);
			AssertEquals("GGGGGGGGGGGGGGG", dataRow[ShipnetConstants.BookingHeader.Berth]);
			AssertEquals("JJJJJJJJJJJJJJJ", dataRow[ShipnetConstants.BookingHeader.CAN]);
		}

		protected override BaseDataRow GetNewDataRow(ZString rawRow) => new BookingHeaderDataRow(rawRow);

		protected override ZString RawRow => "TTTLLLLLLLLLLLLLLLLLLLLHHHHHHHHHHHHHHHHHHHHGGGGGGGGGGDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDZZZZZRRRRRLLLLLDDDDDGGGGGEEEEEEEEEEQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQHHHHHZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZKKIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIWWWWWJJJJJICBBBBBBBBBBSSSSSSSSCCCCCCCCHQQQQQQQQQQSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSNNNNNNNNNNQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQQGGGGGGGGGGGGGGGJJJJJJJJJJJJJJJ";
	}
}
