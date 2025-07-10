using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SummaryDataRowTest : BaseDataRowTest
	{
		protected override void TestParseCore()
		{
			AssertEquals("OOO", dataRow[ShipnetConstants.Common.RecordID]);
			AssertEquals("UUUUUUUUUU", dataRow[ShipnetConstants.Summary.NumberOfMTContainers]);
			AssertEquals("EEEEEEEEEE", dataRow[ShipnetConstants.Summary.NumberOfFullContainers]);
			AssertEquals("JJJJJJJJJJJJJJJ", dataRow[ShipnetConstants.Summary.TotalGrossWeightKGS]);
			AssertEquals("WWWWWWWWWWWWWWW", dataRow[ShipnetConstants.Summary.TotalNetWeightKGS]);
			AssertEquals("JJJJJJJJJJJJJJJ", dataRow[ShipnetConstants.Summary.FileTotalTareWeight]);
			AssertEquals("OOOOOOOOOOOOOOO", dataRow[ShipnetConstants.Summary.TotalGrossCubeCBM]);
			AssertEquals("VVVVVVVVVVVVVVV", dataRow[ShipnetConstants.Summary.TotalNetCubeCBM]);
			AssertEquals("PPPPPPPPPP", dataRow[ShipnetConstants.Summary.TotalNoOfPackages]);
			AssertEquals("AAAAAAAAAA", dataRow[ShipnetConstants.Summary.TotalNumberOfRecords]);
		}

		protected override BaseDataRow GetNewDataRow(ZString rawRow) => new SummaryDataRow(rawRow);

		protected override ZString RawRow => "OOOUUUUUUUUUUEEEEEEEEEEJJJJJJJJJJJJJJJWWWWWWWWWWWWWWWJJJJJJJJJJJJJJJOOOOOOOOOOOOOOOVVVVVVVVVVVVVVVPPPPPPPPPPAAAAAAAAAA";
	}
}
