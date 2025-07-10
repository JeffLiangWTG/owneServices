using System;
using System.IO;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	public class BCSDDRFileGeneratorTest : DDRFileCreatorBaseTest
	{
		public void TestWriteTransactionRecordForDirectPayment()
		{
			DirectPayment.DirectPayment testDirectPayment = Factory.New<DirectPayment.DirectPayment>();
			testDirectPayment.AH_AB = TestBank.PK;
			testDirectPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			testDirectPayment.AH_DrawerBranch = apBankBSB;
			testDirectPayment.AH_DrawerBank = apBankAccountNum;
			testDirectPayment.AH_ChequeDrawer = accountTitle;
			testDirectPayment.AH_ChequeOrReference = lodgementRef;

			testDirectPayment.Lines.AddNew();
			DirectPayment.DirectPaymentLine testLine = (DirectPayment.DirectPaymentLine)testDirectPayment.Lines[0];
			testLine.AL_OSExTaxAmount = 500m;

			DirectDebitBatchHeader header = Factory.New<DirectDebitBatchHeader>();
			header.AH_AB = TestBank.PK;

			Factory.Save();

			StringWriter writer = new StringWriter();

			BCSDDRFileGenerator fileCreator = new BCSDDRFileGenerator(writer, header);
			fileCreator.WriteDetailRecord_ForTestOnly(header.Lines[0] as TransactionHeader, "1");
			string detailRecord = writer.ToString();

			AssertResult(detailRecord);
		}

		public void TestWriteTransactionRecordForPayment()
		{
			OrgHeader testOrg = Factory.New<OrgHeader>();
			testOrg.OH_Code = "FHUEYR";

			APPayment apPayment = Factory.New<APPayment>();
			apPayment.AH_AB = TestBank.PK;
			apPayment.AH_OH = testOrg.PK;
			apPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			apPayment.AH_RX_NKTransactionCurrency = Constants.CurrencyCodes.Cuba;
			apPayment.AH_ExchangeRate = 1m;

			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = apBankBSB;
			accountDetails.A1_BankAccount = apBankAccountNum;
			accountDetails.A1_AccountName = accountTitle;
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_RX_NKAccountCurrency = Constants.CurrencyCodes.Cuba;
			accountDetails.A1_IsDefaultAccount = true;

			apPayment.AH_OSExTaxAmount = 500m;
			apPayment.AH_ChequeOrReference = lodgementRef;

			DirectDebitBatchHeader header = Factory.New<DirectDebitBatchHeader>();
			TestBank.AB_RX_NKAccountCurrency = Core.Constants.CurrencyCodes.Cuba;
			header.AH_AB = TestBank.PK;
			header.AH_RX_NKTransactionCurrency = Core.Constants.CurrencyCodes.Cuba;
			header.AH_ExchangeRate = 1m;

			Factory.Save();

			StringWriter writer = new StringWriter();

			BCSDDRFileGenerator fileCreator = new BCSDDRFileGenerator(writer, header);
			fileCreator.WriteDetailRecord_ForTestOnly(header.Lines[0] as TransactionHeader, "1");
			string detailRecord = writer.ToString();
			AssertResult(detailRecord);
		}

		#region TestRecords

		protected override DDRFileGenerator GetNewFileGenerator(StringWriter writer, DirectDebitBatchHeader header)
		{
			return new BCSDDRFileGenerator(writer, header);
		}

		protected override void AssertRecords(string[] records, decimal paymentAmount, decimal directPaymentAmount)
		{
			string prefix = "01,568090,480098029,385925";
			string directPaymentText = directPaymentAmount.ToString("N2");
			string paymentText = paymentAmount.ToString("N2");
			string directPaymentLine = "01,568090,480098029,385925,995839589,Edward," + directPaymentText + ",,33";
			string paymentLine = "01,568090,480098029,348390,385029571,George," + paymentText + ",,582305";
			string line1 = records[0].StartsWith(prefix) ? directPaymentLine : paymentLine;
			string line2 = records[1].StartsWith(prefix) ? directPaymentLine : paymentLine;

			AssertEquals("Line 1", line1, records[0]);
			AssertEquals("Line 2", line2, records[1]);
		}

		#endregion

		#region Implementation

		AccBankAccount fTestBank;
		AccBankAccount TestBank
		{
			get
			{
				if (fTestBank == null)
				{
					string userID = TestObjectCreator.GetRandomString(6);
					string bankCode = TestObjectCreator.GetRandomString(3);

					fTestBank = SetBank(userID, bankCode);
					fTestBank.AB_AccountNum = accountNum;
					fTestBank.AB_BSB = accountBSB;
				}
				return fTestBank;
			}
		}

		void AssertResult(string detailRecord)
		{
			AssertEquals("Transaction Code", "01" + separator, detailRecord.Substring(0, 3));
			AssertEquals("Originating Sort Code", accountBSB + separator, detailRecord.Substring(3, 7));
			AssertEquals("Originating Account Number", accountNum + separator, detailRecord.Substring(10, 9));
			AssertEquals("Destination Sort Code", apBankBSB + separator, detailRecord.Substring(19, 7));
			AssertEquals("Destination Account Number", apBankAccountNum + separator, detailRecord.Substring(26, 9));
			AssertEquals("Destination Account Name", accountTitle.Substring(0, 18) + separator, detailRecord.Substring(35, 19));
			AssertEquals("Amount", "500.00" + separator, detailRecord.Substring(54, 7));
			AssertEquals("User's Name", GlbStaff.CurrentUser.GS_FullName.SubstringSafe(0, 18) + separator, detailRecord.Substring(61, Math.Min(GlbStaff.CurrentUser.GS_FullName.Length, 18) + 1));
			AssertEquals("User's Reference", lodgementRef + System.Environment.NewLine, detailRecord.Substring(61 + Math.Min(GlbStaff.CurrentUser.GS_FullName.Length, 18) + 1, 14));
		}

		const string separator = ",";
		const string accountNum = "87654321";
		const string accountBSB = "654321";
		const string lodgementRef = "LodgementRef";
		const string apBankBSB = "123456";
		const string apBankAccountNum = "12345678";
		const string accountTitle = "Account Title Longer than 20 chars";

		#endregion
	}
}
