using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	public class WNZDDRFileGenerator_InnerTest : DDRFileCreatorBaseTest
	{
		#region Header

		public void TestWriteDescriptiveHeaderRecord()
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_BSB = "038-304";
			bank.AB_AccountNum = "273482130";

			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			dDRHeader.AH_AB = bank.PK;

			StringWriter writer = new StringWriter();
			WNZDDRFileGenerator generator = new WNZDDRFileGenerator(writer, dDRHeader);
			generator.WriteDescriptiveHeaderRecord_ForTestOnly(dDRHeader);

			string headerString = writer.ToString();

			AssertEquals("Record Type", WNZDDRFileGenerator.HeaderRecordType_ForTestOnly + WNZDDRFileGenerator.Separator_ForTestOnly, headerString.Substring(0, 2));
			AssertEquals("Sequence Number", "1" + WNZDDRFileGenerator.Separator_ForTestOnly, headerString.Substring(2, 2));
			AssertEquals("Origin Bank", "03" + WNZDDRFileGenerator.Separator_ForTestOnly, headerString.Substring(4, 3));
			AssertEquals("Origin Branch", "8304" + WNZDDRFileGenerator.Separator_ForTestOnly, headerString.Substring(7, 5));
			AssertEquals("CustomerName", WNZDDRFileGenerator.Separator_ForTestOnly, headerString.Substring(12, 1));
			AssertEquals("CustomerNumber", WNZDDRFileGenerator.Separator_ForTestOnly, headerString.Substring(13, 1));
			AssertEquals("Description", "REMITTANCE" + WNZDDRFileGenerator.Separator_ForTestOnly, headerString.Substring(14, 11));
			AssertEquals("Due Date", Env.Time.CurrentLocalDate.ToString(WNZDDRFileGenerator.DateFormat_ForTestOnly) + WNZDDRFileGenerator.Separator_ForTestOnly, headerString.Substring(25, 7));
		}

		#endregion

		#region Detail

		public void TestWriteDetailRecord()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccAPAccountDetails accountDetails = org.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "138-345";
			accountDetails.A1_BankAccount = "309932005";
			accountDetails.A1_AccountName = "Bank of China";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Cuba;
			accountDetails.A1_IsDefaultAccount = true;

			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_BSB = "385-999";
			bank.AB_AccountNum = "5988662002";
			bank.AB_BankAccountName = "Goldblatt Resources Ltd";
			bank.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Cuba;

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_OH = org.PK;
			aPPay.AH_AB = bank.PK;
			aPPay.AH_OSExTaxAmount = 388.33m;
			aPPay.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Cuba;
			aPPay.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			aPPay.AH_ChequeOrReference = "284908";

			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			dDRHeader.AH_AB = bank.PK;

			StringWriter writer = new StringWriter();
			WNZDDRFileGenerator generator = new WNZDDRFileGenerator(writer, dDRHeader);
			generator.WriteDetailRecord_ForTestOnly(aPPay, "1");

			string detailRecord = writer.ToString();

			AssertEquals("Record type", "D" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(0, 2));
			AssertEquals("Sequence Number", "1" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(2, 2));
			AssertEquals("Bank number", "13" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(4, 3));
			AssertEquals("Branch number", "8345" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(7, 5));
			AssertEquals("Account number", "309932" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(12, 7));
			AssertEquals("Suffix", "005" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(19, 4));
			AssertEquals("Transaction Code", "50" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(23, 3));
			AssertEquals("MTS Source", "DC" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(26, 3));
			AssertEquals("Amount", "38833" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(29, 6));
			AssertEquals("Payee Name", "Bank of China" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(35, 14));
			AssertEquals("Payee Particulars", "REMITTANCE" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(49, 11));
			AssertEquals("Payee Analysis Code", "284908" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(60, 7));
			AssertEquals("Payee Reference", WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(67, 1));
			AssertEquals("Bank Number", "38" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(68, 3));
			AssertEquals("Branch number", "5999" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(71, 5));
			AssertEquals("Account number", "5988662" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(76, 8));
			AssertEquals("Suffix", "002" + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(84, 4));
			AssertEquals("Payee Name", "Goldblatt Resources " + WNZDDRFileGenerator.Separator_ForTestOnly, detailRecord.Substring(88, 21));
		}

		#endregion

		#region Trailer

		public void TestWriteTrailerRecord()
		{
			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			StringWriter writer = new StringWriter();

			WNZDDRFileGenerator generator = new WNZDDRFileGenerator(writer, dDRHeader);
			generator.WriteFileTotalRecord_ForTestOnly(99.33m, "0", "blah");
			string trailer = writer.ToString();
			AssertEquals("Write Total should not do anything", string.Empty, trailer);
		}

		#endregion

		#region All

		public void TestWriteAllRecords()
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_BSB = "957-145";
			bank.AB_AccountNum = "389933227";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			AccAPAccountDetails accountDetails = org.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = "000-284";
			accountDetails.A1_BankAccount = "400573095";
			accountDetails.A1_AccountName = "Bank of China";
			accountDetails.A1_PaymentMethod = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Cuba;

			APPayment aPPay = Factory.NewWithValidTestData<APPayment>();
			aPPay.AH_OH = org.PK;
			aPPay.AH_AB = bank.PK;
			aPPay.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			aPPay.AH_OSExTaxAmount = 38.98m;
			aPPay.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Cuba;
			aPPay.AH_ExchangeRate = 1m;

			DirectPayment.DirectPayment dPY = Factory.NewWithValidTestData<DirectPayment.DirectPayment>();
			dPY.AH_AB = bank.PK;
			dPY.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectDebit;
			dPY.AH_ChequeDrawer = "Bank of America";
			dPY.AH_DrawerBranch = "232-670";
			dPY.AH_DrawerBank = "586088008";
			dPY.AH_ExchangeRate = 1m;

			Factory.Save();

			DirectDebitBatchHeader dDRHeader = Factory.NewWithValidTestData<DirectDebitBatchHeader>();
			dDRHeader.AH_TransactionNum = ZString.Empty;
			dDRHeader.AH_AB = bank.PK;

			StringWriter writer = new StringWriter();
			WNZDDRFileGenerator generator = new WNZDDRFileGenerator(writer, dDRHeader);
			generator.Create();

			string[] lines = Regex.Split(writer.ToString(), System.Environment.NewLine);
			AssertEquals("Three rows should be created", 3, lines.Length - 1);

			AssertEquals("First row should have record type 'A'", "A", lines[0].Substring(0, 1));
			AssertEquals("First row should have sequence number 1", "1", lines[0].Substring(2, 1));

			AssertEquals("Second row should have record type 'D'", "D", lines[1].Substring(0, 1));
			AssertEquals("Second row should have sequence number 2", "2", lines[1].Substring(2, 1));

			AssertEquals("Third row should have record type 'D'", "D", lines[2].Substring(0, 1));
			AssertEquals("Third row should have sequence number 3", "3", lines[2].Substring(2, 1));
		}

		#endregion

		#region TestRecords

		protected override DDRFileGenerator GetNewFileGenerator(StringWriter writer, DirectDebitBatchHeader header)
		{
			return new WNZDDRFileGenerator(writer, header);
		}

		protected override void AssertRecords(string[] records, decimal paymentAmount, decimal directPaymentAmount)
		{
			string prefix = "38,5925,995839,589,50,DC,";
			string directPaymentText = ((int)(directPaymentAmount * 100)).ToString("D");
			string paymentText = ((int)(paymentAmount * 100)).ToString("D");
			string directPaymentLine = "38,5925,995839,589,50,DC," + directPaymentText + ",Edward,REMITTANCE,33,,56,8090,480098,029,Eagle Datamation Int,";
			string paymentLine = "34,8390,385029,571,50,DC," + paymentText + ",George,REMITTANCE,582305,,56,8090,480098,029,Eagle Datamation Int,";
			string line2 = "D,2," + (records[1].StartsWith("D,2," + prefix) ? directPaymentLine : paymentLine);
			string line3 = "D,3," + (records[2].StartsWith("D,3," + prefix) ? directPaymentLine : paymentLine);

			AssertEquals("Line 1", "A,1,03,8090,,,REMITTANCE,230811,", records[0]);
			AssertEquals("Line 2", line2, records[1]);
			AssertEquals("Line 3", line3, records[2]);
		}

		#endregion
	}
}
