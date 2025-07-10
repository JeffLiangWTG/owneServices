using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class CASSAdjustmentFileTrailerRowTest : FlatFileDataRowTest
	{
		public void TestRecordType()
		{
			var row = new CASSAdjustmentFileTrailerRow();
			AssertEquals("RecordType", "TT", row.RecordType);
		}

		public void TestPublicFields()
		{
			var line = new CASSAdjustmentFileTrailerRow();
			line.NumberOfRecords = 2;

			AssertEquals("NumberOfRecords", "2", line.GetField(CASSAdjustmentFileTrailerRow.Schema.NumberOfRecords));
		}
	}
}
