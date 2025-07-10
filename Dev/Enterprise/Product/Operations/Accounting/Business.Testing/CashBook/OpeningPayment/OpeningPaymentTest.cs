using System;
using System.Data;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.CashBook.OpeningPayment.Testing
{
	[TestedType(typeof(OpeningPayment))]
	public class OpeningPaymentTest : TransactionHeaderTest
	{
		public void TestIEdocsParsingSupportProvider()
		{
			var bo = Factory.NewWithValidTestData<OpeningPayment>();
			var eDocsParsingSupport = bo as IEDocsParsingSupport;
			AssertNotNull("IEDocsParsingSupport must be implemented", eDocsParsingSupport);
			Assert(eDocsParsingSupport.DenySendForParsing(new Guid(), "PIN", "testfile.pdf"));
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(OpeningPaymentValidation); }
		}

		OpeningPayment OpeningPayment
		{
			get { return Header as OpeningPayment; }
		}

		public void TestDebitCredit()
		{
			OpeningPayment.AH_OSTotal = 30m;
			AssertEquals("Debit should = 0", 0m, OpeningPayment.Debit);
			AssertEquals("Credit should = 30", 30m, OpeningPayment.Credit);

			OpeningPayment.AH_OSTotal = -40m;
			AssertEquals("Debit should = 40", 40m, OpeningPayment.Debit);
			AssertEquals("Credit should = 0", 0m, OpeningPayment.Credit);
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
			AssertEquals("Wrong DocManagerCode. Any change to the IDocManagerSupport interface must also be changed in document scanning lookup", "OPY", ((IDocManagerSupport)Factory.New<OpeningPayment>()).DocManagerInfo.DocManagerCode);
		}

		public void TestCashAccount()
		{
			var openingPayment = Factory.New<OpeningPayment>();
			openingPayment.SubmittedFromForm = true;
			openingPayment.AH_ReceiptType = ReceiptTypes.Cheque;

			var bank = TestObjectCreator.CreateBankAccount("TST", "Test Bank", TestObjectCreator.AUD, TestObjectCreator.GLHeader1);
			bank.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			openingPayment.AH_AB = bank.PK;
			AssertEquals("Setting a cash account should set receipt type to cash.", ReceiptTypes.Cash, openingPayment.AH_ReceiptType);
		}

		#region Defaults

		public override void TestExchangeRateTypeDependsOnLedgerAndType()
		{
			AssertEquals("ExchangeRateType should be Buy", ExchangeRateType.Buy, OpeningPayment.RateType);
		}

		public void TestDescriptionDefaulting()
		{
			OpeningPayment opy = Factory.New<OpeningPayment>();
			AssertEquals("Default description should be 'opening payment'", "Opening payment", opy.AH_Desc);
		}

		public void TestPaymentTypeDefaulting()
		{
			OpeningPayment opy = Factory.New<OpeningPayment>();
			AssertEquals("Default payment type should be 'cheque'", ZArchitecture.Core.ReceiptTypes.Cheque, opy.AH_ReceiptType);

			AccountingConfigurationRegistry.Instance.DefaultCashBookPaymentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.Cash);
			opy = Factory.New<OpeningPayment>();
			AssertEquals(ReceiptTypes.Cash, opy.AH_ReceiptType);
		}

		public void TestPaymentTypeReferenceNumberDefaulting()
		{
			AccountingConfigurationRegistry.Instance.DefaultCashBookPaymentType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ReceiptTypes.CreditCard);
			var list = AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.Value;
			var element = list.Cast<PaymentReceiptTypeReferenceNumber>().First(x => x.Type == ReceiptTypes.CreditCard);
			element.ReferenceNumber = "Test1";
			AccountingConfigurationRegistry.Instance.PaymentReceiptTypeReferenceNumberRegistryDefaults.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			OpeningPayment opy = Factory.New<OpeningPayment>();
			AssertEquals("Default payment type should be 'Credit Card'", ZArchitecture.Core.ReceiptTypes.CreditCard, opy.AH_ReceiptType);
			AssertEquals("Test1", opy.AH_ChequeOrReference);

			opy.AH_ReceiptType = ZArchitecture.Core.ReceiptTypes.Cash;
			AssertEquals(ReceiptTypes.Cash, opy.AH_ReceiptType);
			AssertEquals("", opy.AH_ChequeOrReference);
		}

		#endregion

		#region Setting in Setters

		public void TestSettingBankSetsCurrency()
		{
			AccBankAccount uSDBank = Factory.NewWithValidTestData<AccBankAccount>();
			uSDBank.AB_RX_NKAccountCurrency = "USD";

			AccBankAccount cNYBank = Factory.NewWithValidTestData<AccBankAccount>();
			cNYBank.AB_RX_NKAccountCurrency = "CNY";

			OpeningPayment.SubmittedFromForm = true;
			OpeningPayment.AH_AB = uSDBank.PK;
			AssertEquals("Payment currency should be USD", "USD", OpeningPayment.AH_RX_NKTransactionCurrency);

			OpeningPayment.AH_AB = cNYBank.PK;
			AssertEquals("Payment currency should be CNY", "CNY", OpeningPayment.AH_RX_NKTransactionCurrency);

			OpeningPayment.SubmittedFromForm = false;
			OpeningPayment.AH_AB = uSDBank.PK;
			AssertEquals("Payment currency should still be CNY since not submitted from form", "CNY", OpeningPayment.AH_RX_NKTransactionCurrency);
		}

		public void TestSettingOrgSetsDefaultBank()
		{
			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_IsDefaultReceiptBankAccount = true;
			bank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bank.AB_GC = GlbCompany.CurrentCompany.PK;
			bank.AB_GB = GlbBranch.CurrentBranch.PK;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_RX_NKARDDefltCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			OpeningPayment.AH_AB = ZGuid.Empty;
			OpeningPayment.AH_OH = org.PK;
			AssertEquals("Bank should be the test bank", bank.PK, OpeningPayment.AH_AB);
		}

		public void TestSettingForeignCurrencyMakesExchangeRateEditable()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();

			RefExchangeRate exRate = Factory.NewWithValidTestData<RefExchangeRate>();
			exRate.RE_RX_NKExCurrency = currency.RX_Code;
			exRate.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
			exRate.RE_SellRate = 0.1234m;
			exRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			AccBankAccount bank = Factory.NewWithValidTestData<AccBankAccount>();
			bank.AB_RX_NKAccountCurrency = currency.RX_Code;
			OpeningPayment.SubmittedFromForm = true;
			AssertEquals("Exchange rate field should be readonly before AH_AB is set", true, OpeningPayment.AH_ExchangeRateInfo.ReadOnly);
			OpeningPayment.AH_AB = bank.PK;
			AssertEquals("Payment currency should be Currency", currency.RX_Code, OpeningPayment.AH_RX_NKTransactionCurrency);
			AssertEquals("ExchangeRate should be 0.1234", 0.1234m, OpeningPayment.AH_ExchangeRate);
		}

		public void TestSettingLocalExTaxSetsLocalOutstanding()
		{
			OpeningPayment.AH_LocalOutstandingAmount = 0m;
			OpeningPayment.AH_LocalExTaxAmount = 10m;
			AssertEquals("Local OutstandingAmount should be 10", 10m, OpeningPayment.AH_LocalOutstandingAmount);
		}

		public void TestSettingForeignSetsLocalOnly()
		{
			OpeningPayment.AH_ExchangeRate = 2m;
			OpeningPayment.AH_OSExTaxAmount = 10m;
			AssertEquals("LocalExTax should be 5", 5m, OpeningPayment.AH_LocalExTaxAmount);
			OpeningPayment.AH_OSExTaxAmount = 5m;
			AssertEquals("LocalExTax should be 2.5", 2.5m, OpeningPayment.AH_LocalExTaxAmount);
			AssertEquals("ExchangeRate should be 2", 2m, OpeningPayment.AH_ExchangeRate);
		}

		#endregion

		#region Read Only

		public void TestCurrencyReadonly()
		{
			Assert("Currency should be readonly", OpeningPayment.AH_RX_NKTransactionCurrencyInfo.ReadOnly);
		}

		public void TestTransactionNumberReadonly()
		{
			Assert("Transaction Number should be readonly", OpeningPayment.AH_TransactionNumInfo.ReadOnly);
		}

		public void TestSaveSuccessfulSetsAllPropertiesReadonly()
		{
			Assert("AH_Desc is not Readonly", !OpeningPayment.AH_DescInfo.ReadOnly);
			Assert("AH_AB is not Readonly", !OpeningPayment.AH_ABInfo.ReadOnly);
			OpeningPayment.OnSaved(true);
			Assert("AH_Desc should be readonly", OpeningPayment.AH_DescInfo.ReadOnly);
			Assert("AH_AB should be readonly", OpeningPayment.AH_ABInfo.ReadOnly);
		}

		#endregion
	}
}
