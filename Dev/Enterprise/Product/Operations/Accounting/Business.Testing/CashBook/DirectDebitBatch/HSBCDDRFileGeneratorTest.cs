using System.IO;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	public class HSBCDDRFileGeneratorTest : DDRFileCreatorTest
	{
		public void TestWriteDescriptiveHeaderRecordForHSBC()
		{
			string userID = TestObjectCreator.GetRandomString(4);
			string bankCode = TestObjectCreator.GetRandomString(3);

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_BankAccountName = "TEST ACCOUNT NAME";

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;
			Factory.Save();

			StringWriter writer = new StringWriter();

			string expectedCompanyName = testBank.AB_BankAccountName.PadRight(26);

			HSBCDDRFileGenerator fileCreator = new HSBCDDRFileGenerator(writer, header);
			fileCreator.WriteDescriptiveHeaderRecord_ForTestOnly(header);
			string entireHeaderRecordString = writer.ToString();

			AssertEquals("String Length", 122, entireHeaderRecordString.Length);

			AssertEquals("Record Type", "0", entireHeaderRecordString.Substring(0, 1));
			AssertEquals("Reserved Field", "".PadRight(17), entireHeaderRecordString.Substring(1, 17));
			AssertEquals("Sequence Number", "01", entireHeaderRecordString.Substring(18, 2));
			AssertEquals("Name Of Financial Institution", bankCode, entireHeaderRecordString.Substring(20, 3));
			AssertEquals("Reserved Field", "".PadRight(7), entireHeaderRecordString.Substring(23, 7));
			AssertEquals("CurrentCompanyName", expectedCompanyName, entireHeaderRecordString.Substring(30, 26));
			AssertEquals("User ID Number", userID.PadLeft(6, '0'), entireHeaderRecordString.Substring(56, 6));
			AssertEquals("Description of Entries on file", "PAY000001000", entireHeaderRecordString.Substring(62, 12));
			AssertEquals("Today's Date", Env.Time.CurrentLocalDate.ToString("ddMMyy"), entireHeaderRecordString.Substring(74, 6));
			AssertEquals("Reserved Space", "".PadRight(40), entireHeaderRecordString.Substring(80, 40));
			AssertEquals("NewLine", System.Environment.NewLine, entireHeaderRecordString.Substring(120));

			header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;
			Factory.Save();

			writer = new StringWriter();

			expectedCompanyName = testBank.AB_BankAccountName.PadRight(26);

			fileCreator = new HSBCDDRFileGenerator(writer, header);
			fileCreator.WriteDescriptiveHeaderRecord_ForTestOnly(header);
			entireHeaderRecordString = writer.ToString();

			AssertEquals("String Length", 122, entireHeaderRecordString.Length);

			AssertEquals("Record Type", "0", entireHeaderRecordString.Substring(0, 1));
			AssertEquals("Reserved Field", "".PadRight(17), entireHeaderRecordString.Substring(1, 17));
			AssertEquals("Sequence Number", "01", entireHeaderRecordString.Substring(18, 2));
			AssertEquals("Name Of Financial Institution", bankCode, entireHeaderRecordString.Substring(20, 3));
			AssertEquals("Reserved Field", "".PadRight(7), entireHeaderRecordString.Substring(23, 7));
			AssertEquals("CurrentCompanyName", expectedCompanyName, entireHeaderRecordString.Substring(30, 26));
			AssertEquals("User ID Number", userID.PadLeft(6, '0'), entireHeaderRecordString.Substring(56, 6));
			AssertEquals("Description of Entries on file", "PAY000001001", entireHeaderRecordString.Substring(62, 12));
			AssertEquals("Today's Date", Env.Time.CurrentLocalDate.ToString("ddMMyy"), entireHeaderRecordString.Substring(74, 6));
			AssertEquals("Reserved Space", "".PadRight(40), entireHeaderRecordString.Substring(80, 40));
			AssertEquals("NewLine", System.Environment.NewLine, entireHeaderRecordString.Substring(120));
		}

		#region TestRecords

		protected override DDRFileGenerator GetNewFileGenerator(StringWriter writer, DirectDebitBatchHeader header)
		{
			return new HSBCDDRFileGenerator(writer, header);
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

			AssertEquals("Line 1", "0                 01", records[0].Substring(0, 20));
			AssertEquals("Line 1", "       Eagle Datamation Internati00", records[0].Substring(23, 35));
			AssertEquals("Line 1", "PAY000000000230811                                        ", records[0].Substring(62));
			AssertEquals("Line 2", line2, records[1]);
			AssertEquals("Line 3", line3, records[2]);
			AssertEquals("Line 4", "7999-999            00000" + totalAmount + "00000" + totalAmount + "0000000000                        000002                                        ", records[3]);
		}

		#endregion
	}
}
