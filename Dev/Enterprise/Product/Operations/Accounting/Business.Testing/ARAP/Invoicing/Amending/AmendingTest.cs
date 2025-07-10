using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing.Amending
{
	public class AmendingTest : TestCaseWithFactory
	{
		[TestDate(2070, 2, 2)]
		public void TestAmendingRespectsExchangeRateConfiguration()
		{
			AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "INV");

			var testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, "BUY", 1.15m, ZDateTime.Today, ZDateTime.Today);
			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, "BUY", 1.25m, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(1));
			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, "SEL", 1.10m, ZDateTime.Today, ZDateTime.Today);
			testObjectCreator.CreateExchangeRate(testObjectCreator.USD, "SEL", 1.20m, ZDateTime.Today.AddDays(1), ZDateTime.Today.AddDays(1));

			var shipment = testObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = testObjectCreator.CreateJob(shipment, false);

			var charge = testObjectCreator.CreateCharge(job, sellCurrency: testObjectCreator.USD, osSellAmt: 200m, debtor: testObjectCreator.Debtor, chargeCode: testObjectCreator.CC1);
			charge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;

			var invoice = testObjectCreator.PostJobAsBillingTab(job, JobInvoicingPostingOption.Revenue).GetAllARInvoicesAndCreditNotes().Single();
			Factory.Save();

			Assert(charge.IsInDatabase);
			Assert(charge.IsRevenuePosted);
			AssertEquals(1.15m, invoice.AH_ExchangeRate);

			var amendment = (invoice as IAmending)?.GenerateAmendingTransaction(TransactionTypes.Invoice) as InvoicingBase;
			amendment.AH_InvoiceDate = ZDateTime.Today.AddDays(1);

			AssertEquals("Invoice Date BUY rate should to be used according to default ex rate configuration on the Debtor and the registry", 1.25m, amendment.AH_ExchangeRate);

			AssertNoErrors(amendment.AH_ExchangeRateInfo);
			amendment.AH_ExchangeRate = 1.3;
			AssertHasWarning(amendment.AH_ExchangeRateInfo, @"The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
With this option, the exchange rate should not be changed manually. System expected 1.250000 rate but 1.300000 was entered.");
		}

		public void TestAmendingAPInvoiceAfertChargeGSTRateChangedAndTaxBranchEnabled_ShouldApplyTheSameChargeAsAPInvoice()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			Assert("Pre-condition: Company is GST registered", GlbCompany.CurrentCompany.GC_IsGSTRegistered);
			Assert("Pre-condition: Tax branch should be applicable", AccountingMasterFilesUtils.IsTaxBranchApplicable);

			var testObjectCreator = new TestObjectCreator(Factory);

			var shipment = testObjectCreator.CreateShipment("S00001", "AUBNE", "JPTYO");
			var job = testObjectCreator.CreateJob(shipment, false);

			var gst1 = testObjectCreator.CreateTaxRate("TSTGST1", "", 10);
			var gst2 = testObjectCreator.CreateTaxRate("TSTGST2", "", 20);
			var chargeCode = testObjectCreator.CreateChargeCode("TSTCC", "Test", testObjectCreator.FRT.AC_ChargeType, 0m, gst1, null);
			Factory.Save();

			var charge = testObjectCreator.CreateCharge(job, costCurrency: testObjectCreator.AUD, osCostAmt: 200m, creditor: testObjectCreator.Creditor1, chargeCode: chargeCode);
			charge.JR_APInvoiceNum = "TST0001";
			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			charge.JR_APInvoiceDate = ZDateTime.Today;

			var invoice = testObjectCreator.PostJobAsBillingTab(job, JobInvoicingPostingOption.Costs).GetAllAPInvoicesAndCreditNotes().Single();
			Factory.Save();

			chargeCode.AC_AT_GSTRate = gst2.PK;
			chargeCode.ClearGSTRateCacheForTesting();
			Factory.Save();

			var amendedCreditNotes = (invoice as IAmending)?.GenerateAmendingTransaction(TransactionTypes.CreditNote) as InvoicingBase;

			Assert("GST Tax ID on amended credit note line should be the same as it on the invoice line", amendedCreditNotes.Lines[0].AL_AT == gst1.PK);
		}

		public void TestAmendWithCreditNoteFromARInvoiceWillNotThrowCriticalValidationError()
		{
			var newFactory = new BusinessObjectFactory();
			var newCompany = newFactory.New<GlbCompany>();
			newCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			newCompany.GC_Code = "DCN";
			newCompany.GC_IsReciprocal = true;
			var newBranch = newCompany.Branches.AddNew();
			newBranch.GB_Code = "BJN";
			newFactory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, newBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var testObjectCreator = new TestObjectCreator(newFactory);
				testObjectCreator.CreateExchangeRate(testObjectCreator.AED, "BUY", 0.909547329m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(10));
				testObjectCreator.CreateExchangeRate(testObjectCreator.AED, "SEL", 0.909547329m, ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(10));

				var shipment = testObjectCreator.CreateShipment("S0001");
				var job = testObjectCreator.CreateJob(shipment, false);

				var charge1 = testObjectCreator.CreateCharge(job, testObjectCreator.CC3, osCostAmt: 0m, costCurrency: testObjectCreator.CNY, sellCurrency: testObjectCreator.AED, osSellAmt: 20017.42m, debtor: testObjectCreator.AALSHI);
				var invoice = testObjectCreator.CreateInvoice(typeof(ARInvoice), "AR0001", testObjectCreator.AED, 0.909547329m, testObjectCreator.AALSHI);
				invoice.AH_TransactionCategory = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				var line = testObjectCreator.CreateInvoiceLine(invoice, testObjectCreator.AED, 0.909547329m, 20017.42m, 0m, testObjectCreator.CC3.PK);

				line.AL_JH = job.PK;
				charge1.JR_AL_ARLine = line.PK;

				//// WI00638269 - Critical Validation Error: This issue is not able to reproduce so manually changing the AL_ExchangeRate to verify the fix.
				//// While changing the AL_ExchangeRate the AL_LineAmount (LocalAmount) is getting updated so using suspender to ignore updating the Local Amount.
				using (line.GetLocalAmountCalculationSuspender())
				{
					line.AL_ExchangeRate = 0.909547329m;
				}

				var reloadInvoice = newFactory.Load<ARInvoice>(invoice.PK);
				((IAmending)reloadInvoice).GenerateAmendingTransaction(TransactionTypes.CreditNote);
				AssertNoExceptionThrown(@"Should not be getting 'Related REV amount is not the same as charge amount' - Critical Validation Exception because of the exchange rate rounding issue", () => newFactory.Save());
			}
		}
	}
}
