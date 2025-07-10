using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EquipmentDetailsDataRowTest : BaseDataRowTest
	{
		protected override void TestParseCore()
		{
			AssertEquals("LLL", dataRow[ShipnetConstants.Common.RecordID]);
			AssertEquals("BBBB", dataRow[ShipnetConstants.EquipmentDetails.CGRSequence]);
			AssertEquals("CCCCCCCCCCCC", dataRow[ShipnetConstants.EquipmentDetails.EquipmentNumber]);
			AssertEquals("UUUUU", dataRow[ShipnetConstants.EquipmentDetails.SizeType]);
			AssertEquals("J", dataRow[ShipnetConstants.EquipmentDetails.EmptyFull]);
			AssertEquals("PPPPPPPPPPPPPPP", dataRow[ShipnetConstants.EquipmentDetails.TotalGrossWeightKGS]);
			AssertEquals("NNNNNNNNNNNNNNN", dataRow[ShipnetConstants.EquipmentDetails.TotalNetWeightKGS]);
			AssertEquals("AAAAAAAAAAAAAAA", dataRow[ShipnetConstants.EquipmentDetails.TotalGrossCubeCBM]);
			AssertEquals("FFFFFFFFFFFFFFF", dataRow[ShipnetConstants.EquipmentDetails.TotalNetCubeCBM]);
			AssertEquals("HHHHHHHHHHHHHHH", dataRow[ShipnetConstants.EquipmentDetails.TotalTareWeight]);
			AssertEquals("IIIIIIIIII", dataRow[ShipnetConstants.EquipmentDetails.NoOfPackages]);
			AssertEquals("OOOOO", dataRow[ShipnetConstants.EquipmentDetails.Packages]);
			AssertEquals("PPPPPPPPPPPPPPP", dataRow[ShipnetConstants.EquipmentDetails.SealNumber1]);
			AssertEquals("FFFFFFFFFFFFFFF", dataRow[ShipnetConstants.EquipmentDetails.SealNumber2]);
			AssertEquals("X", dataRow[ShipnetConstants.EquipmentDetails.Hazardous]);
			AssertEquals("D", dataRow[ShipnetConstants.EquipmentDetails.ShippersOwn]);
		}

		protected override BaseDataRow GetNewDataRow(ZString rawRow) => new EquipmentDetailsDataRow(rawRow);

		protected override ZString RawRow => "LLLBBBBCCCCCCCCCCCCUUUUUJPPPPPPPPPPPPPPPNNNNNNNNNNNNNNNAAAAAAAAAAAAAAAFFFFFFFFFFFFFFFHHHHHHHHHHHHHHHIIIIIIIIIIOOOOOPPPPPPPPPPPPPPPFFFFFFFFFFFFFFFXD";
	}
}
