using NUnit.Framework;

namespace Enterprise.Customs.CA.DataTransfer.Testing
{
	[TestedType(typeof(CATCPHeaderRow))]
	sealed class CATCPHeaderRowTest : CATCPRowTest
	{
		protected override CATCPRow GetFullyPopulatedDataRow()
		{
			var row = new CATCPHeaderRow();
			row[CATCPHeaderRow.Schema.RecordIdentifier] = "00";
			row[CATCPHeaderRow.Schema.BusinessNumber] = "100035922";
			return row;
		}

		protected override string ExpectedData => "00100035922";
	}
}
