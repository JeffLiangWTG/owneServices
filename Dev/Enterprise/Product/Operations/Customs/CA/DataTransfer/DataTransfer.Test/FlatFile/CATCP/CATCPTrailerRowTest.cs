using NUnit.Framework;

namespace Enterprise.Customs.CA.DataTransfer.Testing
{
	[TestedType(typeof(CATCPTrailerRow))]
	sealed class CATCPTrailerRowTest : CATCPRowTest
	{
		protected override CATCPRow GetFullyPopulatedDataRow()
		{
			var row = new CATCPTrailerRow();
			row[CATCPTrailerRow.Schema.RecordIdentifier] = "99";
			row[CATCPTrailerRow.Schema.NumberOfRecords] = "4";
			return row;
		}

		protected override string ExpectedData => "99000000004";
	}
}
