using System.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	public class BNZDDRFileCreatorTest : DDRFileCreatorBaseTest
	{
		public void TestGetFormattedAmountString()
		{
			StringWriter writer = new StringWriter();
			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;

			header.AH_OSExTaxAmount = 230m;
			BNZDDRFileGenerator fileCreator = new BNZDDRFileGenerator(writer, header);
			AssertEquals("23000", fileCreator.GetFormattedAmountString_ForTestOnly(header.AH_OSExTaxAmount.ToString()));

			header.AH_OSExTaxAmount = 70.456m;
			AssertEquals("7046", fileCreator.GetFormattedAmountString_ForTestOnly(header.AH_OSExTaxAmount.ToString()));
		}

		public void TestWriteDescriptiveHeaderRecordForBNZ()
		{
			string userID = TestObjectCreator.GetRandomString(6);
			string bankCode = TestObjectCreator.GetRandomString(3);
			string accountNum = TestObjectCreator.GetRandomString(9);
			string accountBSB = TestObjectCreator.GetRandomString(6);
			string separator = ",";

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_AccountNum = accountNum;
			testBank.AB_BSB = accountBSB;

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			StringWriter writer = new StringWriter();

			BNZDDRFileGenerator fileCreator = new BNZDDRFileGenerator(writer, header);
			fileCreator.WriteDescriptiveHeaderRecord_ForTestOnly(header);

			string entireHeaderRecordString = writer.ToString();

			AssertEquals("Record Type", "1" + separator, entireHeaderRecordString.Substring(0, 2));
			AssertEquals("Blank field", separator, entireHeaderRecordString.Substring(2, 1));
			AssertEquals("Optional field", separator, entireHeaderRecordString.Substring(3, 1));
			AssertEquals("Optional field", separator, entireHeaderRecordString.Substring(4, 1));
			AssertEquals("Account Number", accountBSB + accountNum + separator, entireHeaderRecordString.Substring(5, 16));
			AssertEquals("Batch Type", "7" + separator, entireHeaderRecordString.Substring(21, 2));
			AssertEquals("Batch Due Date", Env.Time.CurrentLocalDate.ToString("yyMMdd") + separator, entireHeaderRecordString.Substring(23, 7));
			AssertEquals("Today's Date", Env.Time.CurrentLocalDateTime.ToString("yyMMdd") + separator, entireHeaderRecordString.Substring(30, 7));
			AssertEquals("Indicator", "I", entireHeaderRecordString.Substring(37, 1));
			AssertEquals("Carriage Return Line Feed", System.Environment.NewLine, entireHeaderRecordString.Substring(38, 2));
		}

		public void TestWriteTransactionRecordForDirectPayment()
		{
			string userID = TestObjectCreator.GetRandomString(6);
			string bankCode = TestObjectCreator.GetRandomString(3);
			string accountNum = TestObjectCreator.GetRandomString(9);
			string accountBSB = TestObjectCreator.GetRandomString(6);
			string separator = ",";

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_AccountNum = accountNum;
			testBank.AB_BSB = accountBSB;

			string aPBankBSB = "1234-56";
			string aPBankAccountNum = "123456789";
			string accountTitle = "Account Title";
			string lodgementRef = "Lodgement Ref";
			string bSB = "65-4321";
			string bankAccountNum = "987654321";
			string expectedCompanyName = "Remitter";
			testBank.AB_BankAccountName = expectedCompanyName;
			int companyNameLength = expectedCompanyName.Length;

			DirectPayment.DirectPayment testDirectPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			testDirectPayment.AH_AB = testBank.PK;
			testDirectPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			testDirectPayment.AH_DrawerBranch = aPBankBSB;
			testDirectPayment.AH_DrawerBank = aPBankAccountNum;
			testDirectPayment.AH_ChequeDrawer = accountTitle;
			testDirectPayment.AH_ChequeOrReference = lodgementRef;

			testDirectPayment.Lines.AddNew();
			DirectPayment.DirectPaymentLine testLine = (DirectPayment.DirectPaymentLine)testDirectPayment.Lines[0];
			testLine.AL_OSExTaxAmount = 500m;

			testBank.AB_AccountNum = bankAccountNum;
			testBank.AB_BSB = bSB;

			Factory.Save();

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			StringWriter writer = new StringWriter();

			BNZDDRFileGenerator fileCreator = new BNZDDRFileGenerator(writer, header);
			fileCreator.WriteDetailRecord_ForTestOnly(header.Lines[0] as TransactionHeader, "1");
			string detailRecord = writer.ToString();

			AssertEquals("Record Type", "2" + separator, detailRecord.Substring(0, 2));
			AssertEquals("Account Num", aPBankBSB + aPBankAccountNum + separator, detailRecord.Substring(2, 17));
			AssertEquals("Transaction Code", "50" + separator, detailRecord.Substring(19, 3));
			AssertEquals("Amount", "50000" + separator, detailRecord.Substring(22, 6));
			AssertEquals("Other Party Name", accountTitle + separator, detailRecord.Substring(28, 14));
			AssertEquals("Optional Field", separator, detailRecord.Substring(42, 1));
			AssertEquals("Optional Field", separator, detailRecord.Substring(43, 1));
			AssertEquals("Intentionally Blank Field", separator, detailRecord.Substring(44, 1));
			AssertEquals("Optional Field", separator, detailRecord.Substring(45, 1));
			AssertEquals("Subscriber Name", expectedCompanyName + separator, detailRecord.Substring(46, companyNameLength + 1));
			int previousIndex = 46 + companyNameLength + 1;
			AssertEquals("Optional Field", separator, detailRecord.Substring(previousIndex, 1));
			previousIndex++;
			AssertEquals("Optional Field", separator, detailRecord.Substring(previousIndex, 1));
			previousIndex++;
			AssertEquals("Carriage Return Line Feed", System.Environment.NewLine, detailRecord.Substring(previousIndex, 2));
		}

		public void TestWriteTransactionRecordForPayment()
		{
			string userID = TestObjectCreator.GetRandomString(6);
			string bankCode = TestObjectCreator.GetRandomString(3);
			string accountNum = TestObjectCreator.GetRandomString(9);
			string accountBSB = TestObjectCreator.GetRandomString(6);
			string separator = ",";

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_AccountNum = accountNum;
			testBank.AB_BSB = accountBSB;
			testBank.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Cuba;

			string aPBankBSB = "1234-56";
			string aPBankAccountNum = "123456789";
			string accountTitle = "Account Title";
			string lodgementRef = "Lodgement Ref";
			string bSB = "65-4321";
			string bankAccountNum = "987654321";
			string expectedCompanyName = "ExpectedName";
			int companyNameLength = expectedCompanyName.Length;
			testBank.AB_BankAccountName = expectedCompanyName;

			OrgHeader testOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;
			testOrg.OH_Code = "FHUEYR";

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			aPPayment.AH_AB = testBank.PK;
			aPPayment.AH_OH = testOrg.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			aPPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Cuba;
			aPPayment.AH_ExchangeRate = 1m;

			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = aPBankBSB;
			accountDetails.A1_BankAccount = aPBankAccountNum;
			accountDetails.A1_AccountName = accountTitle;
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Cuba;
			accountDetails.A1_IsDefaultAccount = true;

			aPPayment.AH_OSExTaxAmount = 500m;
			aPPayment.AH_ChequeOrReference = lodgementRef;
			testBank.AB_AccountNum = bankAccountNum;
			testBank.AB_BSB = bSB;

			Factory.Save();

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			StringWriter writer = new StringWriter();

			BNZDDRFileGenerator fileCreator = new BNZDDRFileGenerator(writer, header);
			fileCreator.WriteDetailRecord_ForTestOnly(header.Lines[0] as TransactionHeader, "1");
			string detailRecord = writer.ToString();

			AssertEquals("Record Type", "2" + separator, detailRecord.Substring(0, 2));
			AssertEquals("Account Num", aPBankBSB + aPBankAccountNum + separator, detailRecord.Substring(2, 17));
			AssertEquals("Transaction Code", "50" + separator, detailRecord.Substring(19, 3));
			AssertEquals("Amount", "50000" + separator, detailRecord.Substring(22, 6));
			AssertEquals("Other Party Name", accountTitle + separator, detailRecord.Substring(28, 14));
			AssertEquals("Optional Field", separator, detailRecord.Substring(42, 1));
			AssertEquals("Optional Field", separator, detailRecord.Substring(43, 1));
			AssertEquals("Intentionally Blank Field", separator, detailRecord.Substring(44, 1));
			AssertEquals("Optional Field", separator, detailRecord.Substring(45, 1));
			AssertEquals("Subscriber Name", expectedCompanyName + separator, detailRecord.Substring(46, companyNameLength + 1));
			int previousIndex = 46 + companyNameLength + 1;
			AssertEquals("Optional Field", separator, detailRecord.Substring(previousIndex, 1));
			previousIndex++;
			AssertEquals("Optional Field", separator, detailRecord.Substring(previousIndex, 1));
			previousIndex++;
			AssertEquals("Carriage Return Line Feed", System.Environment.NewLine, detailRecord.Substring(previousIndex, 2));
		}

		public void TestWriteControlRecord()
		{
			string separator = ",";
			string userID = TestObjectCreator.GetRandomString(6);
			string bankCode = TestObjectCreator.GetRandomString(3);
			string aPBankBSB = "123456";
			string aPBankAccountNum = "123456789";
			string accountTitle = "Account Title";
			string lodgementRef = "Lodgement Ref";
			string bSB = "65-4321";
			string bankAccountNum = "987654321";

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_BSB = bSB;
			testBank.AB_AccountNum = bankAccountNum;

			DirectPayment.DirectPayment directPayment = Factory.New(typeof(DirectPayment.DirectPayment)) as DirectPayment.DirectPayment;
			directPayment.AH_AB = testBank.PK;
			directPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			APPayment aPPayment = Factory.New(typeof(APPayment)) as APPayment;
			aPPayment.AH_AB = testBank.PK;

			OrgHeader testOrg = Factory.New(typeof(OrgHeader)) as OrgHeader;
			aPPayment.AH_OH = testOrg.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			DirectDebitBatchHeader header = Factory.New(typeof(DirectDebitBatchHeader)) as DirectDebitBatchHeader;
			header.AH_AB = testBank.PK;

			directPayment.AH_DrawerBranch = aPBankBSB;
			directPayment.AH_DrawerBank = aPBankAccountNum;
			directPayment.AH_ChequeDrawer = accountTitle;
			directPayment.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Cuba;
			directPayment.AH_OSExTaxAmount = 500m;
			directPayment.AH_ChequeOrReference = lodgementRef;
			testBank.AB_AccountNum = bankAccountNum;
			testBank.AB_BSB = bSB;

			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = aPBankBSB;
			accountDetails.A1_BankAccount = aPBankAccountNum;
			accountDetails.A1_AccountName = accountTitle;
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Cuba;

			aPPayment.AH_OSExTaxAmount = 500m;
			aPPayment.AH_ChequeOrReference = lodgementRef;

			StringWriter writer = new StringWriter();

			BNZDDRFileGenerator fileCreator = new BNZDDRFileGenerator(writer, header);

			fileCreator.WriteFileTotalRecord_ForTestOnly(1000.00m, "2", "69122469134");

			string fileTotalRecord = writer.ToString();

			AssertEquals("Record Type", "3" + separator, fileTotalRecord.Substring(0, 2));
			AssertEquals("Total Amount", "100000" + separator, fileTotalRecord.Substring(2, 7));
			AssertEquals("Transaction Amount", "2" + separator, fileTotalRecord.Substring(9, 2));
			AssertEquals("Hash Total", "69122469134", fileTotalRecord.Substring(11, 11));
		}

		public void TestGetFileHashingTotal()
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();

			DirectPayment.DirectPayment dPY = Factory.NewWithValidTestData<DirectPayment.DirectPayment>();
			dPY.AH_AB = bank.PK;
			dPY.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			dPY.AH_DrawerBranch = "010527";
			dPY.AH_DrawerBank = "0135884083";

			Factory.Save();

			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			dDRHeader.AH_TransactionNum = ZString.Empty;
			dDRHeader.AH_AB = bank.PK;

			StringWriter writer = new StringWriter();
			BNZDDRFileGenerator generator = new BNZDDRFileGenerator(writer, dDRHeader);
			AssertEquals("Hash total should be 05270135884", "05270135884", generator.GetFileHashingTotal_ForTestOnly(dDRHeader));
		}

		#region TestRecords

		protected override DDRFileGenerator GetNewFileGenerator(StringWriter writer, DirectDebitBatchHeader header)
		{
			return new BNZDDRFileGenerator(writer, header);
		}

		protected override void AssertRecords(string[] records, decimal paymentAmount, decimal directPaymentAmount)
		{
			string prefix = "2,385925995839589,";
			string directPaymentText = ((int)(directPaymentAmount * 100)).ToString("D");
			string paymentText = ((int)(paymentAmount * 100)).ToString("D");
			string directPaymentLine = "2,385925995839589,50," + directPaymentText + ",Edward,,,,,Eagle Datamation Int,,,";
			string paymentLine = "2,348390385029571,50," + paymentText + ",George,,,,,Eagle Datamation Int,,,";
			string line2 = records[1].StartsWith(prefix) ? directPaymentLine : paymentLine;
			string line3 = records[2].StartsWith(prefix) ? directPaymentLine : paymentLine;

			AssertEquals("Line 1", "1,,,,568090480098029,7,110823,110823,I", records[0]);
			AssertEquals("Line 2", line2, records[1]);
			AssertEquals("Line 3", line3, records[2]);
			AssertEquals("Line 4", "3," + ((int)((directPaymentAmount + paymentAmount) * 100)).ToString("D") + ",2,43163808690", records[3]);
		}

		#endregion
	}
}
