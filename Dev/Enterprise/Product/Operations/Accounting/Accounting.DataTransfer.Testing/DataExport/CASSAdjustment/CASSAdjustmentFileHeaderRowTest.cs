using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class CASSAdjustmentFileHeaderRowTest : FlatFileDataRowTest
	{
		public void TestRecordType()
		{
			var row = new CASSAdjustmentFileHeaderRow();
			AssertEquals("RecordType", "AA", row.RecordType);
		}

		public void TestPublicFields()
		{
			var row = new CASSAdjustmentFileHeaderRow();
			row.Agent = "12345678910";
			row.InvoicePeriod = 201508;

			AssertEquals("Agent", "12345678910", row.GetField(CASSAdjustmentFileHeaderRow.Schema.Agent));
			AssertEquals("InvoicePeriod", "201508", row.GetField(CASSAdjustmentFileHeaderRow.Schema.InvoicePeriod));
		}
	}
}
