using System.IO;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	public class DDRFileGeneratorWithDetailLineTest : DDRFileCreatorBaseTest
	{
		public void TestBalancingDetailRecordInsertedOnFooterWithPayment()
		{
			string aPBankBSB = "1234-56";
			string aPBankAccountNum = "123456789";
			string accountTitle = "Account Title";
			string lodgementRef = "Lodgement Ref";
			string bSB = "65-4321";
			string bankAccountNum = "987654321";

			string userID = TestObjectCreator.GetRandomString(6);
			string bankCode = TestObjectCreator.GetRandomString(3);

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_AccountNum = bankAccountNum;
			testBank.AB_BSB = bSB;
			testBank.AB_BankAccountName = "TEST ACCOUNT NAME";
			string expectedCompanyName = testBank.AB_BankAccountName.Left(16);
			string expectedCompanyNameForDebitLine = testBank.AB_BankAccountName.PadRight(32);

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			aPPayment.AH_AB = testBank.PK;

			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			directPayment.AH_AB = testBank.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;
			header.AH_ChequeOrReference = lodgementRef;

			OrgHeader testOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;
			aPPayment.AH_OH = testOrg.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			aPPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Cuba;

			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = aPBankBSB;
			accountDetails.A1_BankAccount = aPBankAccountNum;
			accountDetails.A1_AccountName = accountTitle;
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Cuba;

			aPPayment.AH_OSExTaxAmount = 500m;

			directPayment.AH_DrawerBranch = aPBankBSB;
			directPayment.AH_DrawerBank = aPBankAccountNum;
			directPayment.AH_ChequeDrawer = accountTitle;
			directPayment.AH_OSExTaxAmount = 500m;

			StringWriter writer = new StringWriter();

			DDRFileGeneratorWithDetailLine fileCreator = new DDRFileGeneratorWithDetailLine(writer, header);
			fileCreator.WriteDescriptiveHeaderRecord_ForTestOnly(header);
			fileCreator.WriteFileTotalRecord_ForTestOnly(1000.00m, "2", "");

			string balancingLine = writer.ToString().Substring(122, 122);
			AssertEquals("Balancing line + Detail line Length", 122 + 122 + 122, writer.ToString().Length);
			AssertEquals("Final character should be newline", balancingLine.Substring(120, 2), System.Environment.NewLine);

			AssertEquals("Record Type", "1", balancingLine.Substring(0, 1));
			AssertEquals("Payee BSB", bSB, balancingLine.Substring(1, 7));
			AssertEquals("Payee Bank Account Number", bankAccountNum, balancingLine.Substring(8, 9));
			AssertEquals("Indicator", " ", balancingLine.Substring(17, 1));
			AssertEquals("Transaction Code", "13", balancingLine.Substring(18, 2));
			AssertEquals("Amount", "0000100000", balancingLine.Substring(20, 10));
			AssertEquals("Title Of Account to be Debited", expectedCompanyNameForDebitLine, balancingLine.Substring(30, 32));
			AssertEquals("Lodgement Reference", lodgementRef.PadRight(18), balancingLine.Substring(62, 18));
			AssertEquals("Trace Record BSB", bSB, balancingLine.Substring(80, 7));
			AssertEquals("Trace Record Account Number", bankAccountNum, balancingLine.Substring(87, 9));
			AssertEquals("Name of Remitter", expectedCompanyName, balancingLine.Substring(96, 16));
			AssertEquals("Zero Filled Reserved", "00000000", balancingLine.Substring(112, 8));
			AssertEquals("NewLine", System.Environment.NewLine, balancingLine.Substring(120, 2));

			string fileTotalRecord = writer.ToString().Substring(244, 122);
			AssertEquals("Final character should be newline", fileTotalRecord.Substring(120, 2), System.Environment.NewLine);
			AssertEquals("String Length", 122, fileTotalRecord.Length);

			AssertEquals("Record Type", "7", fileTotalRecord.Substring(0, 1));
			AssertEquals("Reserved Field", "999-999", fileTotalRecord.Substring(1, 7));
			AssertEquals("Reserved Field", "".PadRight(12), fileTotalRecord.Substring(8, 12));
			AssertEquals("File Net Total Amount", "0000000000", fileTotalRecord.Substring(20, 10));
			AssertEquals("File Credit Total Amount", "0000100000", fileTotalRecord.Substring(30, 10));
			AssertEquals("Reserved Field - File Debit Total", "0000100000", fileTotalRecord.Substring(40, 10));
			AssertEquals("Reserved Field", "".PadRight(24), fileTotalRecord.Substring(50, 24));
			AssertEquals("File Total Count", "000003", fileTotalRecord.Substring(74, 6));
			AssertEquals("Reserved Field", "".PadRight(40), fileTotalRecord.Substring(80, 40));
		}

		#region TestRecords

		protected override DDRFileGenerator GetNewFileGenerator(StringWriter writer, DirectDebitBatchHeader header)
		{
			return new DDRFileGeneratorWithDetailLine(writer, header);
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
			AssertEquals("Line 1", "PAYMENTS    230811                                        ", records[0].Substring(62));
			AssertEquals("Line 2", line2, records[1]);
			AssertEquals("Line 3", line3, records[2]);
			AssertEquals("Line 4", "1568090 480098029 1300000" + totalAmount + "Eagle Datamation International                    568090 480098029Eagle Datamation00000000", records[3]);
			AssertEquals("Line 5", "7999-999            000000000000000" + totalAmount + "00000" + totalAmount + "                        000003                                        ", records[4]);
		}

		#endregion
	}
}
