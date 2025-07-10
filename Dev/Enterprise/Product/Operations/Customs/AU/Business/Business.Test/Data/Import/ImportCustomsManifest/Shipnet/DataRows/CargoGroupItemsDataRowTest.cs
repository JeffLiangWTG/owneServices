using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CargoGroupItemsDataRowTest : BaseDataRowTest
	{
		protected override void TestParseCore()
		{
			AssertEquals("PPP", dataRow[ShipnetConstants.Common.RecordID]);
			AssertEquals("CCCC", dataRow[ShipnetConstants.CargoGroupItems.CGRSequence]);
			AssertEquals("TTTTTTTTTT", dataRow[ShipnetConstants.CargoGroupItems.NumberOfPackages]);
			AssertEquals("KKKKK", dataRow[ShipnetConstants.CargoGroupItems.TypeOfPackages]);
			AssertEquals("BBBBBBBBBBBBBBB", dataRow[ShipnetConstants.CargoGroupItems.CargoCode]);
			AssertEquals("PPPPPPPPPPPPPPP", dataRow[ShipnetConstants.CargoGroupItems.CargoGroup]);
			AssertEquals("WWWWWWWWWWWWWWWWWWWWWWWWWWWWWW", dataRow[ShipnetConstants.CargoGroupItems.CargoDescription]);
			AssertEquals("MMMMMMMMMMMMMMM", dataRow[ShipnetConstants.CargoGroupItems.GrossWeightKGS]);
			AssertEquals("PPPPPPPPPPPPPPP", dataRow[ShipnetConstants.CargoGroupItems.NetWeightKGS]);
			AssertEquals("QQQQQQQQQQQQQQQ", dataRow[ShipnetConstants.CargoGroupItems.GrossCubeCBM]);
			AssertEquals("NNNNNNNNNNNNNNN", dataRow[ShipnetConstants.CargoGroupItems.NetCubeCBM]);
			AssertEquals("VVVVVVVVVVVVVVV", dataRow[ShipnetConstants.CargoGroupItems.ECNNumber]);
			AssertEquals("Y", dataRow[ShipnetConstants.CargoGroupItems.Hazardous]);
			AssertEquals("TTTTTTT", dataRow[ShipnetConstants.CargoGroupItems.HazardousClass]);
			AssertEquals("NNNN", dataRow[ShipnetConstants.CargoGroupItems.UNNumber]);
			AssertEquals("EEEEEEE", dataRow[ShipnetConstants.CargoGroupItems.Page]);
			AssertEquals("BBBBBBBBBBB", dataRow[ShipnetConstants.CargoGroupItems.Flashpoint]);
			AssertEquals("D", dataRow[ShipnetConstants.CargoGroupItems.TemperatureType]);
			AssertEquals("R", dataRow[ShipnetConstants.CargoGroupItems.Fumigated]);
			AssertEquals("Z", dataRow[ShipnetConstants.CargoGroupItems.ReportableDocument]);
			AssertEquals("P", dataRow[ShipnetConstants.CargoGroupItems.PersonalEffects]);
		}

		protected override BaseDataRow GetNewDataRow(ZString rawRow) => new CargoGroupItemsDataRow(rawRow);

		protected override ZString RawRow => "PPPCCCCTTTTTTTTTTKKKKKBBBBBBBBBBBBBBBPPPPPPPPPPPPPPPWWWWWWWWWWWWWWWWWWWWWWWWWWWWWWMMMMMMMMMMMMMMMPPPPPPPPPPPPPPPQQQQQQQQQQQQQQQNNNNNNNNNNNNNNNVVVVVVVVVVVVVVVYTTTTTTTNNNNEEEEEEEBBBBBBBBBBBDRZP";
	}
}
