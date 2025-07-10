using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class FileHeaderDataRowTest : BaseDataRowTest
	{
		protected override void TestParseCore()
		{
			AssertEquals("MMM", dataRow[ShipnetConstants.Common.RecordID]);
			AssertEquals("GGGGGGGGGG", dataRow[ShipnetConstants.FileHeader.Context]);
			AssertEquals("SSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSS", dataRow[ShipnetConstants.FileHeader.SenderID]);
			AssertEquals("VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV", dataRow[ShipnetConstants.FileHeader.RecipientID]);
			AssertEquals("YYYYYYYYYY", dataRow[ShipnetConstants.FileHeader.MessageType]);
			AssertEquals("HHHHH", dataRow[ShipnetConstants.FileHeader.MessageVersion]);
			AssertEquals("BBBBBBBBBB", dataRow[ShipnetConstants.FileHeader.VesselCode]);
			AssertEquals("PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP", dataRow[ShipnetConstants.FileHeader.VesselName]);
			AssertEquals("CCCCCCCCCC", dataRow[ShipnetConstants.FileHeader.VesselLloyds]);
			AssertEquals("MMMMMMMMMM", dataRow[ShipnetConstants.FileHeader.VesselOperator]);
			AssertEquals("IIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIII", dataRow[ShipnetConstants.FileHeader.Carrier]);
			AssertEquals("EE", dataRow[ShipnetConstants.FileHeader.ModeOfTransport]);
			AssertEquals("XXXXXXXXXX", dataRow[ShipnetConstants.FileHeader.VoyageNumber]);
			AssertEquals("N", dataRow[ShipnetConstants.FileHeader.AddTareWeight]);
			AssertEquals("OOOOO", dataRow[ShipnetConstants.FileHeader.LastPortOfLoadCode]);
			AssertEquals("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC", dataRow[ShipnetConstants.FileHeader.LastPortOfLoadName]);
			AssertEquals("VVVVVVVV", dataRow[ShipnetConstants.FileHeader.SailDate]);
			AssertEquals("WWWWWW", dataRow[ShipnetConstants.FileHeader.SailTime]);
			AssertEquals("PPPPP", dataRow[ShipnetConstants.FileHeader.FirstPODCode]);
			AssertEquals("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA", dataRow[ShipnetConstants.FileHeader.FirstPODName]);
			AssertEquals("WWWWWWWW", dataRow[ShipnetConstants.FileHeader.ArrivalDate]);
			AssertEquals("ZZZZZZ", dataRow[ShipnetConstants.FileHeader.ArrivalTime]);
			AssertEquals("YYYYYYYYYYYYYYY", dataRow[ShipnetConstants.FileHeader.BerthCTO]);
			AssertEquals("FFFFFFFFFFF", dataRow[ShipnetConstants.FileHeader.SendersABN]);
		}

		protected override BaseDataRow GetNewDataRow(ZString rawRow) => new FileHeaderDataRow(rawRow);

		protected override ZString RawRow => "MMMGGGGGGGGGGSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSSVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVYYYYYYYYYYHHHHHBBBBBBBBBBPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPCCCCCCCCCCMMMMMMMMMMIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIIEEXXXXXXXXXXNOOOOOCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCVVVVVVVVWWWWWWPPPPPAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAWWWWWWWWZZZZZZYYYYYYYYYYYYYYYFFFFFFFFFFF";
	}
}
