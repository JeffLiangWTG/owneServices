using System;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.OpeningReceipt.Testing
{
	[TestedType(typeof(OpeningReceipt))]
	public class OpeningReceiptTest : TransactionHeaderTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<OpeningReceipt>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(OpeningReceiptValidation); }
		}

		OpeningReceipt OpeningReceipt
		{
			get { return Header as OpeningReceipt; }
		}

		public void TestDebitCredit()
		{
			OpeningReceipt.AH_OSTotal = -80m;
			AssertEquals("Debit should be 80", 80m, OpeningReceipt.Debit);
			AssertEquals("Credit should be 0", 0m, OpeningReceipt.Credit);

			OpeningReceipt.AH_OSTotal = 90m;
			AssertEquals("Debit should be 0", 0m, OpeningReceipt.Debit);
			AssertEquals("Credit should be 90", 90m, OpeningReceipt.Credit);
		}

		public override void TestLocalCredit()
		{
			AssertLocalCreditValueForNormalAndOpeningRecPay();
		}

		public override void TestLocalDebit()
		{
			AssertLocalDebitValueForNormalAndOpeningRecPay();
		}

		public void TestDocManagerCode()
		{
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "ORC", ((IDocManagerSupport)Factory.New<OpeningReceipt>()).DocManagerInfo.DocManagerCode);
		}

		public void TestCashAccount()
		{
			OpeningReceipt.AH_ReceiptType = ReceiptTypes.Cheque;
			OpeningReceipt.SubmittedFromForm = true;
			var cashAccount = TestObjectCreator.CreateBankAccount("TST", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			cashAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;

			OpeningReceipt.AH_AB = cashAccount.PK;
			AssertEquals("Setting a cash account should set receipt type to cash.", ReceiptTypes.Cash, OpeningReceipt.AH_ReceiptType);
		}

		#region Defaults

		public void TestDefaultDescription()
		{
			AssertEquals("Default description should be 'Opening Receipt'", "Opening Receipt", OpeningReceipt.AH_Desc);
		}

		public void TestDefaultReceiptType()
		{
			AssertEquals("Default receipt type should be 'Cheque'", ZArchitecture.Core.ReceiptTypes.Cheque, OpeningReceipt.AH_ReceiptType);

			AccountingConfigurationRegistry.Instance.DefaultCashBookReceiptType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
			Header = PrepareTransactionHeaderForTest() as TransactionHeader;
			AssertEquals(ReceiptTypes.Cash, OpeningReceipt.AH_ReceiptType);
		}

		public void TestDefaultReceiptTypeReferenceNumber()
		{
			AccountingConfigurationRegistry.Instance.DefaultCashBookReceiptType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.CreditCard);
			var list = AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.Value;
			var element = list.Cast<PaymentReceiptTypeReferenceNumber>().First(x => x.Type == ReceiptTypes.CreditCard);
			element.ReferenceNumber = "Test1";
			AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			Header = PrepareTransactionHeaderForTest() as TransactionHeader;
			AssertEquals("Default receipt type should be 'Credit Card'", ZArchitecture.Core.ReceiptTypes.CreditCard, OpeningReceipt.AH_ReceiptType);
			AssertEquals("Test1", OpeningReceipt.AH_ChequeOrReference);

			OpeningReceipt.AH_ReceiptType = ReceiptTypes.Cash;
			AssertEquals(ReceiptTypes.Cash, OpeningReceipt.AH_ReceiptType);
			AssertEquals("CSH", OpeningReceipt.AH_ChequeOrReference);
		}

		#endregion

		#region Readonly

		public void TestSettingReceiptTypeToCheque()
		{
			OpeningReceipt.AH_ReceiptType = ZString.Empty;
			OpeningReceipt.AH_ChequeOrReference = "Clear this";
			//OpeningReceipt.AH_ChequeDrawerInfo.BizObj.SetPropertyReadOnlynessForTest("AH_ChequeDrawer", true);
			//OpeningReceipt.AH_DrawerBankInfo.BizObj.SetPropertyReadOnlynessForTest("AH_DrawerBank", true);
			//OpeningReceipt.AH_DrawerBranchInfo.BizObj.SetPropertyReadOnlynessForTest("AH_DrawerBranch", true);

			OpeningReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Assert("Cheque Or Reference should be empty", OpeningReceipt.AH_ChequeOrReference.IsEmpty);
			Assert("Cheque Drawer should be editable", !OpeningReceipt.AH_ChequeDrawerInfo.ReadOnly);
			Assert("Drawer Bank should be editable", !OpeningReceipt.AH_DrawerBankInfo.ReadOnly);
			Assert("Drawer Branch should be editable", !OpeningReceipt.AH_DrawerBranchInfo.ReadOnly);

			OpeningReceipt.IsReverseTransaction = true;
			Assert("Cheque Drawer should be readonly", OpeningReceipt.AH_ChequeDrawerInfo.ReadOnly);
			Assert("Drawer Bank should be readonly", OpeningReceipt.AH_DrawerBankInfo.ReadOnly);
			Assert("Drawer Branch should be readonly", OpeningReceipt.AH_DrawerBranchInfo.ReadOnly);
		}

		public void TestChequeDrawerReadonly()
		{
			OpeningReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			Assert("ChequeDrawer should be readonly", OpeningReceipt.AH_ChequeDrawerInfo.ReadOnly);
			OpeningReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Assert("ChequeDrawer should not be readonly", !OpeningReceipt.AH_ChequeDrawerInfo.ReadOnly);
			OpeningReceipt.IsReverseTransaction = true;
			Assert("ChequeDrawer should be readonly", OpeningReceipt.AH_ChequeDrawerInfo.ReadOnly);
		}

		public void TestIsDrawerBankReadonly()
		{
			OpeningReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.DirectCredit;
			Assert("DrawerBank should be readonly ", OpeningReceipt.AH_DrawerBankInfo.ReadOnly);
			OpeningReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Assert("DrawerBank should not be readonly ", !OpeningReceipt.AH_DrawerBankInfo.ReadOnly);
			OpeningReceipt.IsReverseTransaction = true;
			Assert("DrawerBank should be readonly ", OpeningReceipt.AH_DrawerBankInfo.ReadOnly);
		}

		public void TestIsDrawerBranchReadonly()
		{
			OpeningReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.CreditCard;
			Assert("DrawerBranch should be readonly ", OpeningReceipt.AH_DrawerBranchInfo.ReadOnly);
			OpeningReceipt.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cheque;
			Assert("DrawerBranch should not be readonly ", !OpeningReceipt.AH_DrawerBranchInfo.ReadOnly);
			OpeningReceipt.IsReverseTransaction = true;
			Assert("DrawerBranch should be readonly ", OpeningReceipt.AH_DrawerBranchInfo.ReadOnly);
		}

		public void TestCurrencyIsReadonly()
		{
			Assert("Currency should be readonly", OpeningReceipt.AH_RX_NKTransactionCurrencyInfo.ReadOnly);
		}

		public void TestTransactionNumberIsReadonly()
		{
			Assert("Transaction number should be read only", OpeningReceipt.AH_TransactionNumInfo.ReadOnly);
		}

		public void TestSaveSuccessfulSetsAllPropertiesReadonly()
		{
			Factory.Save();
			Assert("AH_OH should be read only", OpeningReceipt.AH_OHInfo.ReadOnly);
			Assert("AH_OSExTaxAmount should be read only", OpeningReceipt.AH_OSExTaxAmountInfo.ReadOnly);
			Assert("AH_ChequeDrawerInfo should be read only", OpeningReceipt.AH_ChequeDrawerInfo.ReadOnly);
			Assert("AH_DrawerBankInfo should be read only", OpeningReceipt.AH_DrawerBankInfo.ReadOnly);
			Assert("AH_DrawerBranchInfo should be read only", OpeningReceipt.AH_DrawerBranchInfo.ReadOnly);
		}

		#endregion

		#region Setting in Setters

		public void TestSettingOrganisationPopulatesChequeDetails()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_ARPreviousChequeDrawer = "Monday";
			org.CompanyData.OB_ARPreviousChequeDrawerBank = "Tuesday";
			org.CompanyData.OB_ARPreviousChequeDrawerBankBranch = "Wednesday";

			OpeningReceipt.AH_OH = org.PK;
			AssertEquals("ChequeDrawer should be Monday", "Monday", OpeningReceipt.AH_ChequeDrawer);
			AssertEquals("DrawerBank should be Tuesday", "Tuesday", OpeningReceipt.AH_DrawerBank);
			AssertEquals("DrawerBranch should be Wednesday", "Wednesday", OpeningReceipt.AH_DrawerBranch);
		}

		public void TestSettingBankSetsCurrency()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = currency.RX_Code;

			OpeningReceipt.SubmittedFromForm = false;
			OpeningReceipt.AH_RX_NKTransactionCurrency = ZString.Empty;
			OpeningReceipt.AH_AB = bank.PK;
			Assert("Currency should be empty", OpeningReceipt.AH_RX_NKTransactionCurrency.IsEmpty);

			OpeningReceipt.SubmittedFromForm = true;
			OpeningReceipt.AH_RX_NKTransactionCurrency = ZString.Empty;
			OpeningReceipt.AH_AB = bank.PK;
			AssertEquals("Receipt currency should be the bank currency", currency.RX_Code, OpeningReceipt.AH_RX_NKTransactionCurrency);
		}

		public void TestSettingOrgSetsDefaultBank()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = currency.RX_Code;
			bank.AB_GC = GlbCompany.CurrentCompany.PK;
			bank.AB_GB = GlbBranch.CurrentBranch.PK;
			bank.AB_IsDefaultReceiptBankAccount = true;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_RX_NKARDDefltCurrency = currency.RX_Code;

			OpeningReceipt.AH_AB = ZGuid.Empty;
			OpeningReceipt.AH_OH = org.PK;
			AssertEquals("Bank should default to the test bank", bank.PK, OpeningReceipt.AH_AB);
		}

		public void TestSettingLocalExTaxSetsLocalOutstanding()
		{
			OpeningReceipt.AH_LocalOutstandingAmount = 0m;
			OpeningReceipt.AH_LocalExTaxAmount = 10m;
			AssertEquals("Local OutstandingAmount should be 10", 10m, OpeningReceipt.AH_LocalOutstandingAmount);
		}

		public void TestSettingForeignSetsLocalOnly()
		{
			OpeningReceipt.AH_ExchangeRate = 2m;
			OpeningReceipt.AH_OSExTaxAmount = 10m;
			AssertEquals("LocalExTax should be 5", 5m, OpeningReceipt.AH_LocalExTaxAmount);
			OpeningReceipt.AH_OSExTaxAmount = 5m;
			AssertEquals("LocalExTax should be 2.5", 2.5m, OpeningReceipt.AH_LocalExTaxAmount);
			AssertEquals("ExchangeRate should be 2", 2m, OpeningReceipt.AH_ExchangeRate);
		}

		#endregion
	}
}
