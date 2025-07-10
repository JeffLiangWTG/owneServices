using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocDirectPayment))]
	sealed class DocDirectPaymentTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocDirectPayment.New(DirectPayment, Factory)
			};
		}

		public void TestMICRNumber()
		{
			var bank = Factory.New<AccBankAccount>();
			bank.AB_BankName = "Test Bank Account";
			bank.AB_BSB = "123456789-";
			bank.AB_AccountNum = "010203040506-";
			bank.AB_BankAddress = "123 Address Test";
			bank.AB_GC = GlbCompany.CurrentCompany.PK;
			bank.AB_RX_NKAccountCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			bank.AB_GB = GlbBranch.CurrentBranch.PK;
			DirectPayment.AH_AB = bank.PK;
			DirectPayment.AH_ChequeOrReference = "1357924680-";

			AssertEquals("MICRNumber", "C1357924680DC A123456789DA 010203040506DC", DirectPaymentWrapper.MICRNumber);
		}

		public void TestInvoiceAmountAndTax()
		{
			AssertEquals(0M, DirectPaymentWrapper.InvoiceAmountAndTax);

			DirectPayment.AH_LocalExTaxAmount = 200M;
			DirectPayment.AH_LocalTaxAmount = 20M;

			AssertEquals(220M, DirectPaymentWrapper.InvoiceAmountAndTax);
		}

		public void TestOSTotalForRemittanceAdvice()
		{
			DirectPaymentLine line1 = (DirectPaymentLine)DirectPayment.Lines.AddNew();
			DirectPaymentLine line2 = (DirectPaymentLine)DirectPayment.Lines.AddNew();
			DirectPaymentLine line3 = (DirectPaymentLine)DirectPayment.Lines.AddNew();
			line1.AL_OSExTaxAmount = 200M;
			line2.AL_OSExTaxAmount = 100M;
			line3.AL_OSExTaxAmount = 220M;
			Factory.Save();

			DirectPaymentWrapper = DocDirectPayment.New(DirectPayment, Factory);
			AssertEquals(520M, DirectPaymentWrapper.OSTotalForRemittanceAdvice);
			AssertEquals(520M, DirectPaymentWrapper.Cheque.ChequeAmount);
		}

		public void TestInvoiceAmountForPaymentVoucher()
		{
			AssertEquals(0M, DirectPaymentWrapper.InvoiceAmountForPaymentVoucher);

			DirectPayment.AH_LocalExTaxAmount = -200M;
			DirectPayment.AH_LocalTaxAmount = -20M;
			AssertEquals(220M, DirectPaymentWrapper.InvoiceAmountForPaymentVoucher);
		}

		public void TestNumberOfDifferentCurrenciesOnDPY()
		{
			DirectPayment.AH_TransactionNum = "S0003999";

			DirectPaymentLine line1 = (DirectPaymentLine)DirectPayment.Lines.AddNew();
			DirectPaymentLine line2 = (DirectPaymentLine)DirectPayment.Lines.AddNew();
			Factory.Save();
			AssertEquals(1, DirectPaymentWrapper.NumberOfDifferentCurrencies);

			line1.AL_RX_NKTransactionCurrency = "AUD";
			line2.AL_RX_NKTransactionCurrency = "USD";
			Factory.Save();
			DirectPaymentWrapper = DocDirectPayment.New(DirectPayment, Factory);
			AssertEquals(2, DirectPaymentWrapper.NumberOfDifferentCurrencies);
		}

		public void TestRemittanceAdviceContactDPYType()
		{
			AssertEquals("", DirectPaymentWrapper.RemittanceAdviceContact);
			DirectPayment.AH_OH = (Factory.LoadTop1<OrgHeader>(new ZQuery())).PK;
			DirectPayment.AH_ChequeDrawer = "Jamie";
			AssertEquals("Jamie", DirectPaymentWrapper.RemittanceAdviceContact);
		}

		public void TestChequePayTo()
		{
			AssertEquals("", DirectPaymentWrapper.ChequePayTo);
			AssertEquals("", DirectPaymentWrapper.Cheque.ChequePayTo);

			var aPOrg = Factory.New<OrgHeader>();
			aPOrg.OH_Code = "AP_ORG TEST";
			aPOrg.MainAddress.OA_Address1 = "AP_ORG Address";
			aPOrg.OH_FullName = "AP ORG";

			DirectPayment.AH_ChequeDrawer = "Cheque Drawer";
			DirectPayment.AH_OH = aPOrg.PK;
			DirectPaymentWrapper = DocDirectPayment.New(DirectPayment, Factory);
			AssertEquals("Cheque Drawer", DirectPaymentWrapper.ChequePayTo);
			AssertEquals("Cheque Drawer", DirectPaymentWrapper.Cheque.ChequePayTo);
		}

		public void TestNotIsReversalDPY()
		{
			AssertEquals("Should not be a reversal transaction", ZBool.False, DirectPaymentWrapper.IsReversal);
		}

		public void TestIsReversalDPY()
		{
			DirectPayment.AH_IsCancelled = ZBool.True;
			DirectPayment.AH_InvoiceAmount = 1000M;
			DirectPayment.AH_OSTotal = 1000M;
			DirectPayment.Lines.AddNew(DirectPayment.DependentTransactionLineType);
			DirectPayment.Lines[0].AL_LineAmount = 500m;
			DirectPayment.Lines[0].AL_OSAmount = 500m;
			DirectPayment.Lines.AddNew(DirectPayment.DependentTransactionLineType);
			DirectPayment.Lines[1].AL_LineAmount = 500m;
			DirectPayment.Lines[1].AL_OSAmount = 500m;
			Factory.Save();
			AssertEquals("Should be a reversal transaction", ZBool.True, DirectPaymentWrapper.IsReversal);
		}

		public void TestShowOriginalAmountForDPY()
		{
			DirectPayment.AH_TransactionNum = "S0003999";

			RefCurrency localCurrency = GlbCompany.CurrentCompany.LocalCurrency;
			RefCurrency uSCurr = GetRefCurrency("USD");

			DirectPaymentLine line1 = (DirectPaymentLine)DirectPayment.Lines.AddNew();
			line1.AL_RX_NKTransactionCurrency = localCurrency.RX_Code;
			DirectPaymentLine line2 = (DirectPaymentLine)DirectPayment.Lines.AddNew();
			line2.AL_RX_NKTransactionCurrency = uSCurr.RX_Code;
			Factory.Save();
			AssertEquals("Y", DirectPaymentWrapper.ShowOriginalAmountForDPY.ToString());

			line2.AL_RX_NKTransactionCurrency = localCurrency.RX_Code;
			Factory.Save();
			AssertEquals("N", DirectPaymentWrapper.ShowOriginalAmountForDPY.ToString());
		}

		public void TestPrintRemittanceOnChequeDPY()
		{
			DirectPayment.AH_TransactionNum = "S0003999";

			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();

			Factory.Save();
			DirectPaymentWrapper = DocDirectPayment.New(DirectPayment, Factory);
			AssertEquals("Cheque should fit this, as it's 22 lines with 1X2 rows currency grouping = 24 lines.", "Y", DirectPaymentWrapper.PrintRemittanceOnCheque.ToString());
			AssertEquals(22, DirectPaymentWrapper.FirstPageDPYLines.Count);
		}

		public void TestNotToPrintRemittanceAdviceOnChequeDPY()
		{
			DirectPayment.AH_TransactionNum = "S0003999";

			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();

			Factory.Save();
			DirectPaymentWrapper = DocDirectPayment.New(DirectPayment, Factory);
			AssertEquals("Cheque should not fit this, as it's 23 lines with 1X2 rows currency grouping = 25 lines.", "N", DirectPaymentWrapper.PrintRemittanceOnCheque.ToString());
			AssertEquals(22, DirectPaymentWrapper.FirstPageDPYLines.Count);
		}

		public void TestPaymentCurrency()
		{
			DirectPaymentLine line1 = (DirectPaymentLine)DirectPayment.Lines.AddNew();
			line1.AL_RX_NKTransactionCurrency = "AUD";
			DirectPaymentLine line2 = (DirectPaymentLine)DirectPayment.Lines.AddNew();
			line2.AL_RX_NKTransactionCurrency = "AUD";
			Factory.Save();
			AssertEquals("AUD", DirectPaymentWrapper.PaymentCurrency.Code);
			AssertEquals("AUD", DirectPaymentWrapper.Cheque.PaymentCurrency.Code);
		}

		public void TestDirectPayLines()
		{
			DirectPayment.AH_TransactionNum = "S0003999";

			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();

			Factory.Save();
			DirectPaymentWrapper = DocDirectPayment.New(DirectPayment, Factory);
			AssertEquals(3, DirectPaymentWrapper.DirectPayLines.Count);
		}

		public void TestNumberOfTransactionsAndTotalsOnCheque()
		{
			DirectPayment.AH_TransactionNum = "S0003999";
			DirectPayment.Lines.AddNew();

			Factory.Save();
			DirectPaymentWrapper = DocDirectPayment.New(DirectPayment, Factory);
			AssertEquals(3m, DirectPaymentWrapper.NumberOfTransactionsAndTotalsOnCheque);

			DirectPayment.Lines.AddNew();
			DirectPayment.Lines.AddNew();

			Factory.Save();
			DirectPaymentWrapper = DocDirectPayment.New(DirectPayment, Factory);
			AssertEquals(5m, DirectPaymentWrapper.NumberOfTransactionsAndTotalsOnCheque);
		}

		public void TestCheque()
		{
			DirectPayment.AH_TransactionNum = "S0003999";
			DirectPayment.AH_RX_NKTransactionCurrency = "AUD";

			DirectPaymentLine line1 = (DirectPaymentLine)DirectPayment.Lines.AddNew();
			DirectPaymentLine line2 = (DirectPaymentLine)DirectPayment.Lines.AddNew();

			line1.AL_RX_NKTransactionCurrency = "AUD";
			line2.AL_RX_NKTransactionCurrency = "AUD";

			line1.AL_OSExTaxAmount = 400M;
			line2.AL_OSExTaxAmount = 600M;

			AssertNotNull(DirectPaymentWrapper.Cheque);
			AssertEquals("ONE THOUSAND DOLLARS ONLY", DirectPaymentWrapper.Cheque.ChequeAmountInWords);
		}

		public void TestPaymentTransactionSummary()
		{
			AssertEquals("Should always return remittance referral because remittance stub lacks space for text descriptions.", "PLEASE REFER TO REMITTANCE ADVICE FOR DETAILED LIST OF TRANSACTIONS", DirectPaymentWrapper.PaymentTransactionSummary);
		}

		public void TestChequePayToWithAddress()
		{
			DirectPayment.AH_ChequeDrawer = "AH_ChequeDrawer";
			Factory.Save();
			AssertEquals("AH_ChequeDrawer", DirectPayment.AH_ChequeDrawer);
			AssertEquals(DirectPaymentWrapper.ChequePayToWithAddress, DirectPayment.AH_ChequeDrawer);
		}

		#region Implementation

		DirectPayment DirectPayment => directPayment ?? (directPayment = Factory.New<DirectPayment>());
		DirectPayment directPayment;
		DocDirectPayment DirectPaymentWrapper
		{
			get
			{
				return directPaymentWrapper ?? (directPaymentWrapper = DocDirectPayment.New(DirectPayment, Factory));
			}
			set
			{
				directPaymentWrapper = value;
			}
		}

		DocDirectPayment directPaymentWrapper;

		RefCurrency GetRefCurrency(ZString currencyCode)
		{
			return Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, currencyCode);
		}

		#endregion
	}
}
