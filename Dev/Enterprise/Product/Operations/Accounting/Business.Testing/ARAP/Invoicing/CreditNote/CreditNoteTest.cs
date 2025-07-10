using System;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class CreditNoteTest : InvoicingBaseTest
	{
		protected override Type GetExpectedBusinessObjectLineType()
		{
			return typeof(CreditNoteLine);
		}

		protected override Type TypeOfValidation
		{
			get { return typeof(CreditNoteValidation); }
		}

		public void TestCopyValuesFrom()
		{
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();

			RefExchangeRate exRate = Factory.NewWithValidTestData<RefExchangeRate>();
			exRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.BuyRate;
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_SellRate = 0.9m;
			exRate.RE_RX_NKExCurrency = currency.RX_Code;
			exRate.RE_StartDate = ZDateTime.Today.AddDays(-2);
			exRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			Factory.Save();

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.CompanyData.OB_RX_NKAPDefltCurrency = currency.RX_Code;
			org.CompanyData.OB_RX_NKARDDefltCurrency = currency.RX_Code;

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			aPInv.AH_OH = org.PK;
			aPInv.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			aPInv.AH_ExchangeRate = 0.5m;
			aPInv.AH_GB_TaxBranch = TestObjectCreator.NonCurrentBranch.PK;

			CreditNote crd = Factory.NewWithValidTestData(GetExpectedBusinessObjectType()) as CreditNote;
			crd.CopyValuesFrom(aPInv);
			AssertEquals("Credit Note Currency should be the local currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, crd.AH_RX_NKTransactionCurrency);
			AssertEquals("Credit Note ExchangeRate should be 0.5", 0.5m, crd.AH_ExchangeRate);
			AssertEquals("AH_GB_TaxBranch", aPInv.AH_GB_TaxBranch, crd.AH_GB_TaxBranch);
		}

		public void TestCreditNoteWithOriginalReference()
		{
			var postingExRateRegistry = PostingExRateRegistryAR;

			ARInvoice invoice = null;
			using (postingExRateRegistry.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF"))
			{
				invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("ARINV1", TestObjectCreator.USD, 0.78m, TestObjectCreator.AALSHI);
				invoice.AH_PostDate = ZDateTime.Today.AddDays(-1);
				invoice.IsDisbursementOrFinal = false;
				TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC1, TestObjectCreator.USD, 0.78m, "", 1000m);
				TestObjectCreator.CreateARInvoiceLine(invoice, null, TestObjectCreator.CC2, TestObjectCreator.USD, 0.78m, "", 1000m);
				Factory.Save();

				var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD1", TestObjectCreator.AALSHI, TestObjectCreator.USD, 0.58m);
				creditNote.AH_PostDate = ZDateTime.Today.AddDays(0);
				creditNote.OriginalTransactionReference = invoice.PK;

				AssertNoErrors(creditNote.AH_ExchangeRateInfo);
				AssertNoWarnings(creditNote.AH_ExchangeRateInfo);
				AssertEquals(invoice.AH_ExchangeRate, creditNote.AH_ExchangeRate);
			}

			using (postingExRateRegistry.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST"))
			{
				var uSDCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				TestObjectCreator.CreateExchangeRate(uSDCurrency, "SEL", 0.71m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));
				TestObjectCreator.CreateExchangeRate(uSDCurrency, "SEL", 0.698m, ZDateTime.Today.AddDays(0), ZDateTime.Today.AddDays(0));

				TestObjectCreator.CreateExchangeRate(uSDCurrency, "BUY", 0.71m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(-1));
				TestObjectCreator.CreateExchangeRate(uSDCurrency, "BUY", 0.698m, ZDateTime.Today.AddDays(0), ZDateTime.Today.AddDays(0));

				Factory.Save();

				var creditNote = TestObjectCreator.CreateARCreditNote("ARCRD1", TestObjectCreator.AALSHI, TestObjectCreator.USD, 0.58m);
				creditNote.AH_PostDate = ZDateTime.Today.AddDays(0);
				creditNote.OriginalTransactionReference = invoice.PK;
				Assert(creditNote.IsAmendingTransaction);

				AssertEquals(invoice.AH_ExchangeRate, creditNote.AH_ExchangeRate);
				AssertEquals(0.78m, creditNote.AH_ExchangeRate);
				AssertEquals(0.78m, invoice.AH_ExchangeRate);
				AssertHasWarning(creditNote.AH_ExchangeRateInfo, @"The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
With this option, the exchange rate should not be changed manually. System expected 0.698000 rate but 0.780000 was entered.");

				AssertEquals("Exchange rates of the header and line match ?", creditNote.Lines[0].AL_ExchangeRate, creditNote.AH_ExchangeRate);

				Assert(creditNote.IsAmendingOrReversal);
				creditNote.AH_ExchangeRate = 0.698m;
				AssertNoErrors(creditNote.AH_ExchangeRateInfo);
				AssertNoWarnings(creditNote.AH_ExchangeRateInfo);
			}
		}
	}
}
