using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CargoGroupDataRowTest : BaseDataRowTest
	{
		protected override void TestParseCore()
		{
			AssertEquals("III", dataRow[ShipnetConstants.Common.RecordID]);
			AssertEquals("KKKK", dataRow[ShipnetConstants.CargoGroup.CGRSequence]);
			AssertEquals("SSSSSSSSSS", dataRow[ShipnetConstants.CargoGroup.NumberOfContainers]);
			AssertEquals("CCCCC", dataRow[ShipnetConstants.CargoGroup.SizeType]);
			AssertEquals("A", dataRow[ShipnetConstants.CargoGroup.EmptyFull]);
			AssertEquals("PPPPPPPPPPPPPPP", dataRow[ShipnetConstants.CargoGroup.TotalGrossWeightKGS]);
			AssertEquals("AAAAAAAAAAAAAAA", dataRow[ShipnetConstants.CargoGroup.TotalNetWeightKGS]);
			AssertEquals("NNNNNNNNNNNNNNN", dataRow[ShipnetConstants.CargoGroup.TotalGrossCubeCBM]);
			AssertEquals("MMMMMMMMMMMMMMM", dataRow[ShipnetConstants.CargoGroup.TotalNetCubeCBM]);
			AssertEquals("BBBBBBBBBB", dataRow[ShipnetConstants.CargoGroup.TotalNoOfPackages]);
			AssertEquals("TTTTTTTTTTTTTTT", dataRow[ShipnetConstants.CargoGroup.TotalContainerTare]);
			AssertEquals("JJJJJJJJJJJJJJJ", dataRow[ShipnetConstants.CargoGroup.CAN]);
		}

		protected override BaseDataRow GetNewDataRow(ZString rawRow) => new CargoGroupDataRow(rawRow);

		protected override ZString RawRow => "IIIKKKKSSSSSSSSSSCCCCCAPPPPPPPPPPPPPPPAAAAAAAAAAAAAAANNNNNNNNNNNNNNNMMMMMMMMMMMMMMMBBBBBBBBBBTTTTTTTTTTTTTTTJJJJJJJJJJJJJJJ";
	}
}
