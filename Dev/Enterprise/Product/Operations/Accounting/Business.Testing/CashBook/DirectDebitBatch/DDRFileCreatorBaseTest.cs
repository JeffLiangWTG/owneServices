using System.IO;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.DirectDebitBatch.Testing
{
	public abstract class DDRFileCreatorBaseTest : TestCaseWithFactory
	{
		#region TestRecords

		[TestDate(2011, 08, 23)]
		public void TestRecords_LocalBankCurrency()
		{
			string[] records = createRecords(Core.Constants.CurrencyCodes.Australia);
			AssertRecords(records, 123.45m, 440m);
		}

		[TestDate(2011, 08, 23)]
		public void TestRecords_ForeignBankCurrency()
		{
			string[] records = createRecords(Core.Constants.CurrencyCodes.UnitedStates);
			AssertRecords(records, 239.42m, 550m);
		}

		string[] createRecords(string bankCurrency)
		{
			string bankBSB = "568090";
			string bankAccountNum = "480098029";
			string userID = TestObjectCreator.GetRandomString(4);
			string bankCode = TestObjectCreator.GetRandomString(3);

			AccBankAccount testBank = SetBank(userID, bankCode);
			testBank.AB_AccountNum = bankAccountNum;
			testBank.AB_BSB = bankBSB;
			testBank.AB_RX_NKAccountCurrency = bankCurrency;

			string payeeBSB_Detail1 = "348390";
			string payeeAccountNum_Detail1 = "385029571";
			string accountTitle_Detail1 = "George";
			string payeeCode_Detail1 = "GFF";
			string lodgementReference_Detail1 = "582305";

			string payeeBSB_Detail2 = "385925";
			string payeeAccountNum_Detail2 = "995839589";
			string accountTitle_Detail2 = "Edward";
			string lodgementReference_Detail2 = "33";

			OrgHeader testOrg = Factory.New<OrgHeader>();
			testOrg.OH_Code = "RGY$*E";

			APPayment aPPayment = Factory.New<APPayment>();
			aPPayment.AH_AB = testBank.PK;
			aPPayment.AH_OH = testOrg.PK;
			aPPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;
			aPPayment.AH_ExchangeRate = 1m;

			AccAPAccountDetails accountDetails = testOrg.CompanyData.AccountDetailsCollection.AddNew();
			accountDetails.A1_BankBsb = payeeBSB_Detail1;
			accountDetails.A1_BankAccount = payeeAccountNum_Detail1;
			accountDetails.A1_AccountName = accountTitle_Detail1;
			accountDetails.A1_RX_NKAccountCurrency = bankCurrency;
			accountDetails.A1_PaymentMethod = ReceiptTypes.DirectDebit;
			accountDetails.A1_IsDefaultAccount = true;

			bool isLocalCurrency = bankCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			if (isLocalCurrency)
			{
				aPPayment.AH_LocalExTaxAmount = 123.45m;
				aPPayment.AH_OSExTaxAmount = 123.45m;
			}
			else
			{
				aPPayment.AH_OSExTaxAmount = 239.42m;
			}
			testOrg.OH_FullName = payeeCode_Detail1;
			aPPayment.AH_ChequeOrReference = lodgementReference_Detail1;

			DirectPayment.DirectPayment testDirectPayment = Factory.New<DirectPayment.DirectPayment>();
			testDirectPayment.AH_AB = testBank.PK;
			testDirectPayment.AH_ReceiptType = ReceiptTypes.DirectDebit;

			testDirectPayment.AH_DrawerBranch = payeeBSB_Detail2;
			testDirectPayment.AH_DrawerBank = payeeAccountNum_Detail2;
			testDirectPayment.AH_ChequeDrawer = accountTitle_Detail2;
			testDirectPayment.AH_ChequeOrReference = lodgementReference_Detail2;
			testDirectPayment.AH_OH = testOrg.PK;
			testDirectPayment.AH_ExchangeRate = 1m;

			testDirectPayment.Lines.AddNew();
			DirectPayment.DirectPaymentLine testLine = (DirectPayment.DirectPaymentLine)testDirectPayment.Lines[0];

			if (isLocalCurrency)
			{
				testLine.AL_LocalExTaxAmount = 400.00m;
				testLine.AL_LocalTaxAmount = 40.00m;
			}
			else
			{
				testLine.AL_OSExTaxAmount = 500.00m;
				testLine.AL_OSTaxAmount = 50.00m;
			}

			Factory.Save();

			StringWriter writer = new StringWriter();

			DirectDebitBatchHeader header = Factory.New<DirectDebitBatchHeader>();
			header.AH_AB = testBank.PK;

			DDRFileGenerator fileCreator = GetNewFileGenerator(writer, header);
			fileCreator.Create();

			return Regex.Split(writer.ToString(), System.Environment.NewLine);
		}

		protected abstract DDRFileGenerator GetNewFileGenerator(StringWriter writer, DirectDebitBatchHeader header);

		protected abstract void AssertRecords(string[] records, decimal paymentAmount, decimal directPaymentAmount);

		#endregion

		#region Implementation

		protected AccBankAccount SetBank(string userID, string bankCode)
		{
			AccBankAccount testBank = Factory.New(typeof(AccBankAccount)) as AccBankAccount;
			testBank.AB_AccountEFTUserID = userID;
			testBank.AB_Code = bankCode;
			testBank.AB_AutoDDRFormat = bankCode;
			testBank.AB_AG = TestObjectCreator.CreateGLHeader().PK;
			return testBank;
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		#endregion
	}
}