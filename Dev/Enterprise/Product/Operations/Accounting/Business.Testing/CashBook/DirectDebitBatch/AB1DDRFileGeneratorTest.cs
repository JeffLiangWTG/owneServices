using System.IO;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	public class AB1DDRFileGeneratorTest : ABADDRFileCreatorBaseTest
	{
		#region TestRecords

		protected override DDRFileGenerator GetNewFileGenerator(StringWriter writer, DirectDebitBatchHeader header)
		{
			return new ABAFileGeneratorWithDetailLine(writer, header);
		}

		protected override void AssertRecords(string[] records, decimal paymentAmount, decimal directPaymentAmount)
		{
			string prefix = "1385925 995839589 5000000";
			string directPaymentText = ((int)(directPaymentAmount * 100)).ToString("D");
			string paymentText = ((int)(paymentAmount * 100)).ToString("D");
			string directPaymentLine = "1385925 995839589 5000000" + directPaymentText + "Edward                          33                568090 480098029Eagle Datamation00000000";
			string paymentLine = "1348390 385029571 5000000" + paymentText + "George                          582305            568090 480098029Eagle Datamation00000000";
			string line2 = records[1].StartsWith(prefix) ? directPaymentLine : paymentLine;
			string line3 = records[2].StartsWith(prefix) ? directPaymentLine : paymentLine;
			string totalAmount = ((int)((directPaymentAmount + paymentAmount) * 100)).ToString("D");

			AssertEquals("Line 1", "0                 01ABC", records[0].Substring(0, 23));
			AssertEquals("Line 1", "       Eagle Datamation Internati00", records[0].Substring(23, 35));
			AssertEquals("Line 1", "PAYMENTS    230811                                        ", records[0].Substring(62));
			AssertEquals("Line 2", line2, records[1]);
			AssertEquals("Line 3", line3, records[2]);
			AssertEquals("Line 4", "1568090 480098029 1300000" + totalAmount + "Eagle Datamation International                    568090 480098029Eagle Datamation00000000", records[3]);
			AssertEquals("Line 5", "7999-999            000000000000000" + totalAmount + "00000" + totalAmount + "                        000003                                        ", records[4]);
		}
		#endregion
	}
}
