using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	public class BBLDDRFileGenerator_InnerTest : DDRFileCreatorBaseTest
	{
		protected const char Padding = ' ';

		#region Header

		public void TestWriteDescriptiveHeaderRecordForBBL()
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_AccountNum = "328930225";
			bank.AB_BSB = "449027";
			bank.AB_AutoDDRFormat = Core.Constants.DDRFileFormat.BBL;
			bank.AB_AccountEFTUserID = "346594";

			StringWriter writer = new StringWriter();

			DirectDebitBatchHeader batchHeader = Factory.New<DirectDebitBatchHeader>();
			batchHeader.AH_AB = bank.PK;

			string expectedUser = "ExpectedUser".PadRight(26);
			bank.AB_BankAccountName = expectedUser;

			BBLDDRFileGenerator generator = new BBLDDRFileGenerator(writer, batchHeader);
			generator.WriteDescriptiveHeaderRecord_ForTestOnly(batchHeader);

			string headerString = writer.ToString();

			AssertEquals("Record Type", "0", headerString.Substring(0, 1));
			AssertEquals("Padding", string.Empty.PadLeft(17, ' '), headerString.Substring(1, 17));
			AssertEquals("Reel sequence", "01", headerString.Substring(18, 2));
			AssertEquals("Name of User's bank", Core.Constants.DDRFileFormat.BBL, headerString.Substring(20, 3));
			AssertEquals("Blank", string.Empty.PadLeft(7, ' '), headerString.Substring(23, 7));
			AssertEquals("Name of user supplying tape", expectedUser, headerString.Substring(30, 26));
			AssertEquals("Number of user supplying tape", "346594", headerString.Substring(56, 6));
			AssertEquals("Description of entries on tape", DDRFileGenerator.EntryDescription_ForTestONly.PadRight(12, ' '), headerString.Substring(62, 12));
			AssertEquals("Date to be processed", Env.Time.CurrentLocalDateTime.ToString(DDRFileGenerator.DateFormat_ForTestOnly), headerString.Substring(74, 6));
			AssertEquals("Blank", string.Empty.PadLeft(40, ' '), headerString.Substring(80, 40));
		}

		#endregion

		#region Detail

		public void TestWriteDetailRecord()
		{
			AccBankAccount payerBank = Factory.NewWithValidTestData<AccBankAccount>();
			payerBank.AB_BSB = "345-678";
			payerBank.AB_AccountNum = "089739455";
			payerBank.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Cuba;

			DirectDebitBatchHeader dDRBatchHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			dDRBatchHeader.AH_AB = payerBank.PK;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccAPAccountDetails accountDetails = org.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "888-235";
			accountDetails.A1_BankAccount = "438568403";
			accountDetails.A1_AccountName = "Conference in Darwin";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Cuba;
			accountDetails.A1_IsDefaultAccount = true;

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			aPPay.AH_ExchangeRate = 2m;
			aPPay.AH_OSExTaxAmount = 483.23m;
			aPPay.AH_ChequeOrReference = "005835";
			aPPay.AH_OSWHTAmount = 23.56m;
			aPPay.AH_OH = org.PK;
			aPPay.AH_AB = payerBank.PK;
			aPPay.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Cuba;
			aPPay.AH_ExchangeRate = 1m;

			Factory.Save();

			StringWriter writer = new StringWriter();

			string expectedRemitterName = "ExpectedRemitter";
			payerBank.AB_BankAccountName = expectedRemitterName;

			BBLDDRFileGenerator generator = new BBLDDRFileGenerator(writer, dDRBatchHeader);
			generator.WriteDetailRecord_ForTestOnly(aPPay, "1");

			string headerString = writer.ToString();

			AssertEquals("Record Type", "1", headerString.Substring(0, 1));
			AssertEquals("Institution Number", "888-235", headerString.Substring(1, 7));
			AssertEquals("Account Number", "438568403", headerString.Substring(8, 9));
			AssertEquals("Indicator", " ", headerString.Substring(17, 1));
			AssertEquals("Transaction Code", "50", headerString.Substring(18, 2));
			AssertEquals("Amount in Cents", "0000048323", headerString.Substring(20, 10));
			AssertEquals("Title of Account", "Conference in Darwin            ", headerString.Substring(30, 32));
			AssertEquals("Lodgement Reference", "005835            ", headerString.Substring(62, 18));
			AssertEquals("Trace Record Institution Number", "345-678", headerString.Substring(80, 7));
			AssertEquals("Payee Account Number", "089739455", headerString.Substring(87, 9));
			AssertEquals("Name of Remitter", expectedRemitterName, headerString.Substring(96, 16));
			AssertEquals("Withholding Tax", "00002356", headerString.Substring(112, 8));
		}

		#endregion

		#region Trailer

		public void TestWriteTrailerRecord()
		{
			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();

			StringWriter writer = new StringWriter();
			BBLDDRFileGenerator generator = new BBLDDRFileGenerator(writer, dDRHeader);
			generator.WriteFileTotalRecord_ForTestOnly(358776.34m, "4", "ASDF");

			string trailerRecord = writer.ToString();
			AssertEquals("Record Type", "7", trailerRecord.Substring(0, 1));
			AssertEquals("Institution Number", generator.NineFilledReservedField_ForTestOnly, trailerRecord.Substring(1, 7));
			AssertEquals("Blank", string.Empty.PadLeft(12, Padding), trailerRecord.Substring(8, 12));
			AssertEquals("Net Total Amount", "0035877634", trailerRecord.Substring(20, 10));
			AssertEquals("Credit Total Amount", "0035877634", trailerRecord.Substring(30, 10));
			AssertEquals("Debit Total Amount", generator.ZeroFilledReservedField_ForTestOnly, trailerRecord.Substring(40, 10));
			AssertEquals("Blank", string.Empty.PadLeft(24, Padding), trailerRecord.Substring(50, 24));
			AssertEquals("Count of Records", "000004", trailerRecord.Substring(74, 6));
			AssertEquals("Blank", string.Empty.PadLeft(40, Padding), trailerRecord.Substring(80, 40));
		}

		#endregion

		#region All

		public void TestAllWriteAllRecords()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccAPAccountDetails accountDetails = org.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "385-359";
			accountDetails.A1_BankAccount = "483479009";
			accountDetails.A1_AccountName = "Commonwealth";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Cuba;
			accountDetails.A1_IsDefaultAccount = true;
			Factory.Save();

			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Cuba;

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_AB = bank.PK;
			aPPay.AH_OH = org.PK;
			aPPay.AH_ExchangeRate = 1m;
			aPPay.AH_OSExTaxAmount = 10m;
			aPPay.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			aPPay.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Cuba;
			aPPay.AH_ExchangeRate = 1m;

			DirectPayment.DirectPayment directPay = Factory.NewWithValidTestData<DirectPayment.DirectPayment>();
			directPay.AH_AB = bank.PK;
			directPay.AH_ExchangeRate = 0.5m;
			directPay.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;

			directPay.Lines.AddNew();
			DirectPayment.DirectPaymentLine testLine = (DirectPayment.DirectPaymentLine)directPay.Lines[0];
			testLine.AL_OSExTaxAmount = 40m;

			Factory.Save();

			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			dDRHeader.AH_TransactionNum = ZString.Empty;
			dDRHeader.AH_AB = bank.PK;
			AssertEquals("There should be 2 lines", 2, dDRHeader.Lines.Count);

			StringWriter writer = new StringWriter();
			BBLDDRFileGenerator generator = new BBLDDRFileGenerator(writer, dDRHeader);
			generator.Create();
			string[] lines = Regex.Split(writer.ToString(), System.Environment.NewLine);
			AssertEquals("There should be 4 lines in the DDR File", 4, lines.Length - 1);

			Assert("First line should start with 0", lines[0].StartsWith("0"));
			Assert("Second line should start with 1", lines[1].StartsWith("1"));
			Assert("Third line should start with 1", lines[2].StartsWith("1"));
			Assert("Fourth line should start with 7", lines[3].StartsWith("7"));

			AssertEquals("Net Total Amount", "0000005000", lines[3].Substring(20, 10));
			AssertEquals("Net Credit Amount", "0000005000", lines[3].Substring(30, 10));
			AssertEquals("Count of Records", "000002", lines[3].Substring(74, 6));
		}

		#endregion

		#region TestRecords

		protected override DDRFileGenerator GetNewFileGenerator(StringWriter writer, DirectDebitBatchHeader header)
		{
			return new BBLDDRFileGenerator(writer, header);
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
			AssertEquals("Line 4", "7999-999            00000" + totalAmount + "00000" + totalAmount + "0000000000                        000002                                        ", records[3]);
		}

		#endregion
	}
}
