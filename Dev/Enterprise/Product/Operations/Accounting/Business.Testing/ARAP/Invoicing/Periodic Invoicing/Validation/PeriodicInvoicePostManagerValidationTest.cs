using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class PeriodicInvoicePostManagerValidationTest : PostManagerValidationTest
	{
		[TestDate(2015, 5, 1)]
		public void TestRunJobChargeTaxBranchValidation()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
				TestObjectCreator.CreateJobShipmentWithFIDCharge("S001", TestObjectCreator.LocalClient, TestObjectCreator.CC1, 100M, 1000M);

				var testJob = Factory.LoadTop1<Job>(new ZQuery(JobHeaderSchema.JH_JobNum, "S001"));
				var deferredCharge = testJob.Charges[0];
				deferredCharge.JR_GB_SellTaxBranch = GlbBranch.CurrentBranch.PK;

				testJob.RunPreSaveValidation();
				AssertNoErrors(testJob);

				Factory.Save();

				var periodicInvoice = new PeriodicInvoice(Factory);
				periodicInvoice.InvoiceDate = new ZDateTime(2015, 4, 30);
				periodicInvoice.PostDate = new ZDateTime(2015, 4, 29);
				periodicInvoice.TaxBranch = GlbBranch.CurrentBranch.PK;

				var validation = new PeriodicInvoicePostManagerValidation(testJob, new[] { deferredCharge }, periodicInvoice);

				AssertEquals(string.Empty, validation.RunJobChargeSellTaxBranchValidation_ForTestOnly());

				var newFactory = new BusinessObjectFactory();
				newFactory.RefreshEnabled = false;
				var chargeInNewFactory = newFactory.Load<Charge>(deferredCharge.PK);
				chargeInNewFactory.JR_GB_SellTaxBranch = TestObjectCreator.NonCurrentBranch.PK;

				newFactory.Save();

				AssertEquals("Tax Branch values on unposted charges in the billing tab conflict with the Periodic Invoice value.", validation.RunJobChargeSellTaxBranchValidation_ForTestOnly());
			}
		}

		public void TestDontLoadAllJobCharges()
		{
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupPeriods();

			SetupInvoiceStyles(TestObjectCreator.ABIGAS, InvoicePostingOptionsList.Codes.DisbursementAndFinal);
			SetupInvoiceStyles(TestObjectCreator.LocalClient, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			Job job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			Charge deferredCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100, 100);
			deferredCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;

			Charge deferredCharge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100, 100);
			deferredCharge2.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;

			Charge nonDeferredChargeWithError = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100, 100);
			nonDeferredChargeWithError.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
			nonDeferredChargeWithError.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;

			job.RunPreSaveValidation();
			AssertNoErrors(job);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			OrgHeader localClient = newFactory.Load<OrgHeader>(nonDeferredChargeWithError.JR_OH_SellAccount);
			localClient.CompanyData.OB_IsDebtor = false;
			newFactory.Save();

			Factory.Save();

			ReleaseFactory();

			var jobInNewFactory = Factory.Load<Job>(job.PK);
			var deferredChargeInNewFactory = Factory.Load<Charge>(deferredCharge.PK);
			PostManagerValidation validation = new PeriodicInvoicePostManagerValidation(jobInNewFactory, new[] { deferredChargeInNewFactory }, null);

			// Should be no errors because only charges passed in validation constructor should be validated
			AssertHasNoValidationErrors(validation);

			ZQuery cacheOnlyFilter = new ZQuery();
			cacheOnlyFilter.FetchOnlyFromLocalCache = true;
			var allChargesInFactory = Factory.Load<Charge>(cacheOnlyFilter);
			AssertEquals("Validation should load more charges the passed in its constructor", 1, allChargesInFactory.Length);
			AssertEquals(deferredChargeInNewFactory.PK, allChargesInFactory[0].PK);
		}

		public void TestOnlyValidateChargesThatAreDeferred()
		{
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupPeriods();

			SetupInvoiceStyles(TestObjectCreator.ABIGAS, InvoicePostingOptionsList.Codes.DisbursementAndFinal);
			SetupInvoiceStyles(TestObjectCreator.LocalClient, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			Factory.Save();

			IJobInvoicingPlugIn plugIn = TestObjectCreator.GetTestShipmentPlugIn();
			Job testJob = new Job.Loader(plugIn).TryCreateWithoutMutexForTestOnly();
			Charge deferredCharge = testJob.Charges.AddNew();
			deferredCharge.JR_AC = TestObjectCreator.CC1.PK;
			deferredCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			deferredCharge.JR_OSSellAmt = 100m;

			Charge nonDeferredChargeWithError = testJob.Charges.AddNew();
			nonDeferredChargeWithError.JR_AC = TestObjectCreator.CC1.PK;
			nonDeferredChargeWithError.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
			nonDeferredChargeWithError.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			nonDeferredChargeWithError.JR_OSSellAmt = 100m;

			testJob.RunPreSaveValidation();
			AssertNoErrors(testJob);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			OrgHeader localClient = newFactory.Load<OrgHeader>(nonDeferredChargeWithError.JR_OH_SellAccount);
			localClient.CompanyData.OB_IsDebtor = false;
			newFactory.Save();
			Factory.Save();

			PostManagerValidation validation = NewPostManagerValidation(new[] { testJob }, JobInvoicingPostingOption.Revenue);
			// Should be no errors because only non-deferred charge is invalid
			AssertHasNoValidationErrors(validation);
		}

		public void TestDontValidateRevenueRecognitionRequirementsOnNonDeferredCharges()
		{
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupPeriods();

			SetupInvoiceStyles(TestObjectCreator.ABIGAS, InvoicePostingOptionsList.Codes.DisbursementAndFinal);
			SetupInvoiceStyles(TestObjectCreator.LocalClient, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			AccChargeRevRecOverride revenueRecognitionOverride = TestObjectCreator.CC2.RevenueRecOverrides.AddNew();
			revenueRecognitionOverride.AE_JobType = JobInvoicingConsumerTypes.Shipment.Code;
			revenueRecognitionOverride.AE_Direction = "ALL";
			revenueRecognitionOverride.AE_Mode = "ALL";
			revenueRecognitionOverride.AE_RecognitionType = RevenueRecognitionLookups.RecognitionDateOptionCodes.CustomsClearanceDate;
			Factory.Save();

			Factory.Save();

			IJobInvoicingPlugIn plugIn = TestObjectCreator.GetTestShipmentPlugIn();
			Job testJob = new Job.Loader(plugIn).TryCreateWithoutMutexForTestOnly();
			Charge deferredCharge = testJob.Charges.AddNew();
			deferredCharge.JR_AC = TestObjectCreator.CC1.PK;
			deferredCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			deferredCharge.JR_OSSellAmt = 100m;

			Charge nonDeferredChargeWithError = testJob.Charges.AddNew();
			nonDeferredChargeWithError.JR_AC = TestObjectCreator.CC2.PK;
			nonDeferredChargeWithError.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			nonDeferredChargeWithError.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			nonDeferredChargeWithError.JR_OSSellAmt = 100m;

			testJob.RunPreSaveValidation();
			AssertNoErrors(testJob);

			Factory.Save();

			PostManagerValidation validation = NewPostManagerValidation(new[] { testJob }, JobInvoicingPostingOption.Revenue);
			// Should be no errors because only non-deferred charge is invalid
			AssertHasNoValidationErrors(validation);
		}

		[TestDate(2015, 5, 1)]
		public override void TestRunExchangeRateValidation_PostManagerValidation()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupPeriods();

			SetupInvoiceStyles(TestObjectCreator.ABIGAS, InvoicePostingOptionsList.Codes.DisbursementAndFinal);
			SetupInvoiceStyles(TestObjectCreator.LocalClient, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			Job job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var deferredCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100, 100);
			deferredCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			deferredCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			deferredCharge.JR_RX_NKSellCurrency = "USD";
			deferredCharge.JR_RX_NKSellInvoiceCurrency = "";
			job.UpdateBaseExchangeRate("USD", ExchangeRateValidLedgerEnum.AR, 0.81m);
			deferredCharge.JR_OSSellAmt = 100;

			job.RunPreSaveValidation();
			AssertNoErrors(job);
			Factory.Save();
			Assert(!deferredCharge.BillInInvoiceCurrency);

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.InvoiceDate = new ZDateTime(2015, 4, 30);
			periodicInvoice.PostDate = new ZDateTime(2015, 4, 29);

			var validation = new PeriodicInvoicePostManagerValidation(job, new[] { deferredCharge }, periodicInvoice);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			AssertHasNoValidationErrors(validation);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", validation);
			TestObjectCreator.CreateUSDBuyRate(0.951m, new DateTime(2015, 5, 1));
			AssertHasNoValidationErrors(validation);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the USD exchange rate is not set for the date 30-Apr-15. Please check your data and try again.", validation);
			TestObjectCreator.CreateUSDBuyRate(0.9430m, new DateTime(2015, 4, 30));
			AssertHasNoValidationErrors(validation);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 29-Apr-15. Please check your data and try again.", validation);
			TestObjectCreator.CreateUSDBuyRate(0.9429m, new DateTime(2015, 4, 29));
			AssertHasNoValidationErrors(validation);

			deferredCharge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.CNY.RX_Code;
			Factory.Save();
			Assert(deferredCharge.BillInInvoiceCurrency);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			AssertHasNoValidationErrors(validation);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the CNY exchange rate is not set for the date 01-May-15. Please check your data and try again.", validation);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 0.951m, new DateTime(2015, 5, 1), new DateTime(2015, 5, 1));
			AssertHasNoValidationErrors(validation);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the CNY exchange rate is not set for the date 30-Apr-15. Please check your data and try again.", validation);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 0.9430m, new DateTime(2015, 4, 30), new DateTime(2015, 4, 30));
			AssertHasNoValidationErrors(validation);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the CNY exchange rate is not set for the date 29-Apr-15. Please check your data and try again.", validation);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 0.9429m, new DateTime(2015, 4, 29), new DateTime(2015, 4, 29));
			AssertHasNoValidationErrors(validation);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		[TestDate(2015, 5, 1)]
		public override void TestRunExchangeRateValidation_ARPostingExRateRegistry()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();
			AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupPeriods();

			SetupInvoiceStyles(TestObjectCreator.ABIGAS, InvoicePostingOptionsList.Codes.DisbursementAndFinal);
			SetupInvoiceStyles(TestObjectCreator.LocalClient, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

			Factory.Save();

			var shipment = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
			Job job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var deferredCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100, 100);
			deferredCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			deferredCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
			deferredCharge.JR_RX_NKSellCurrency = "USD";
			deferredCharge.JR_RX_NKSellInvoiceCurrency = "";
			job.UpdateBaseExchangeRate("USD", ExchangeRateValidLedgerEnum.AR, 0.81m);
			deferredCharge.JR_OSSellAmt = 100;

			job.RunPreSaveValidation();
			AssertNoErrors(job);
			Factory.Save();
			Assert(!deferredCharge.BillInInvoiceCurrency);

			var periodicInvoice = new PeriodicInvoice(Factory);
			periodicInvoice.InvoiceDate = new ZDateTime(2015, 4, 30);
			periodicInvoice.PostDate = new ZDateTime(2015, 4, 29);

			var validation = new PeriodicInvoicePostManagerValidation(job, new[] { deferredCharge }, periodicInvoice);

			Assert("Charge Is In Local Invoice Currency For Posting because charge type FID is local", deferredCharge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
			SetPostingExRateRegistryAR("DEF", "INV");
			AssertHasNoValidationErrors(validation);

			SetPostingExRateRegistryAR("TOD", "DEF");
			AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", validation);
			TestObjectCreator.CreateUSDBuyRate(0.951m, new DateTime(2015, 5, 1));
			AssertHasNoValidationErrors(validation);

			SetPostingExRateRegistryAR("INV", "DEF");
			AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the USD exchange rate is not set for the date 30-Apr-15. Please check your data and try again.", validation);
			TestObjectCreator.CreateUSDBuyRate(0.9430m, new DateTime(2015, 4, 30));
			AssertHasNoValidationErrors(validation);

			SetPostingExRateRegistryAR("PST", "DEF");
			AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 29-Apr-15. Please check your data and try again.", validation);
			TestObjectCreator.CreateUSDBuyRate(0.9429m, new DateTime(2015, 4, 29));
			AssertHasNoValidationErrors(validation);

			deferredCharge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.CNY.RX_Code;
			Factory.Save();
			Assert(deferredCharge.BillInInvoiceCurrency);

			Assert("Charge Is In Foreign Invoice Currency For Posting because charge SellInvoiceCurrency is foreign", !deferredCharge.IsInLocalInvoiceCurrencyForPosting(ExchangeRateValidLedgerEnum.AR));
			SetPostingExRateRegistryAR("INV", "DEF");
			AssertHasNoValidationErrors(validation);

			SetPostingExRateRegistryAR("DEF", "TOD");
			AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the CNY exchange rate is not set for the date 01-May-15. Please check your data and try again.", validation);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 0.951m, new DateTime(2015, 5, 1), new DateTime(2015, 5, 1));
			AssertHasNoValidationErrors(validation);

			SetPostingExRateRegistryAR("DEF", "INV");
			AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the CNY exchange rate is not set for the date 30-Apr-15. Please check your data and try again.", validation);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 0.9430m, new DateTime(2015, 4, 30), new DateTime(2015, 4, 30));
			AssertHasNoValidationErrors(validation);

			SetPostingExRateRegistryAR("DEF", "PST");
			AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the CNY exchange rate is not set for the date 29-Apr-15. Please check your data and try again.", validation);
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, "BUY", 0.9429m, new DateTime(2015, 4, 29), new DateTime(2015, 4, 29));
			AssertHasNoValidationErrors(validation);

			ExchangeRateReader.GetReaderInstance().ClearCache();

			void SetPostingExRateRegistryAR(string localOption, string foreignOption)
			{
				var collectionAR = new InvoicePostingExRateOptionCollection();
				collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, foreignOption, 0));
				collectionAR.Add(new InvoicePostingExRateOption(Core.Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, localOption, 0));
				AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collectionAR);
			}
		}

		[TestDate(2015, 5, 1)]
		public override void TestRunExchangeRateValidation_PostManagerValidation_ARExchangeRateConfiguration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				ExchangeRateReader.GetReaderInstance().ClearCache();
				AccountingPeriodTestHelper helper = new AccountingPeriodTestHelper(Factory);
				helper.SetupPeriods();

				var debtor = TestObjectCreator.ABIGAS;
				debtor.CompanyData.OB_IsDebtor = true;
				debtor.CompanyData.SetARTaxApplicable(true);
				debtor.CompanyData.OB_ARWHTApplicable = false;
				debtor.OH_FullName = "Test debtor";

				debtor.CompanyData.AccARExchangeRateConfigurations.RemoveAndDeleteAll();
				debtor.CompanyData.AccARExchangeRateConfigurations.SetExRate("AR", "ALL", "ALL", "ALL", "SEL", "TDR", 0, false);

				AssertEquals("Pre-condition", 1, debtor.CompanyData.AccARExchangeRateConfigurations.Count);

				SetupInvoiceStyles(debtor, InvoicePostingOptionsList.Codes.DisbursementAndFinal);
				SetupInvoiceStyles(TestObjectCreator.LocalClient, InvoicePostingOptionsList.Codes.DisbursementAndFinal);

				Factory.Save();

				var shipment = TestObjectCreator.CreateShipment("S0001", "AUSYD", "USLAX");
				Job job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
				var deferredCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100, 100);
				deferredCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
				deferredCharge.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice;
				deferredCharge.JR_RX_NKSellCurrency = "USD";
				deferredCharge.JR_RX_NKSellInvoiceCurrency = "";
				deferredCharge.JR_OSSellAmt = 100;

				job.RunPreSaveValidation();
				AssertNoErrors(job);
				Factory.Save();
				Assert(!deferredCharge.BillInInvoiceCurrency);

				var today = new ZDateTime(2015, 5, 1);
				var invoiceDate = new ZDateTime(2015, 4, 30);
				var postDate = new ZDateTime(2015, 4, 29);

				var periodicInvoice = new PeriodicInvoice(Factory);
				periodicInvoice.InvoiceDate = invoiceDate;
				periodicInvoice.PostDate = postDate;
				periodicInvoice.DebtorPK = debtor.PK;

				var validation = new PeriodicInvoicePostManagerValidation(job, new[] { deferredCharge }, periodicInvoice);

				using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF"))
				{
					AssertHasNoValidationErrors(validation);
				}

				using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD"))
				{
					TestObjectCreator.CreateUSDBuyRate(6.71m, today);
					AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.", validation);
					TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 6.71m, today, today);
					AssertHasNoValidationErrors(validation);
				}

				using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV"))
				{
					TestObjectCreator.CreateUSDBuyRate(6.72m, invoiceDate);
					AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the USD exchange rate is not set for the date 30-Apr-15. Please check your data and try again.", validation);
					TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 6.72m, invoiceDate, invoiceDate);
					AssertHasNoValidationErrors(validation);
				}

				using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST"))
				{
					TestObjectCreator.CreateUSDBuyRate(6.73m, postDate);
					AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 29-Apr-15. Please check your data and try again.", validation);
					TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, Core.Constants.ExchangeRateTypes.Code.SellRate, 6.73m, postDate, postDate);
					AssertHasNoValidationErrors(validation);
				}

				deferredCharge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.CNY.RX_Code;
				Factory.Save();
				Assert(deferredCharge.BillInInvoiceCurrency);

				using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF"))
				{
					AssertHasNoValidationErrors(validation);
				}

				using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD"))
				{
					TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, Core.Constants.ExchangeRateTypes.Code.BuyRate, 6.11m, today, today);
					AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the CNY exchange rate is not set for the date 01-May-15. Please check your data and try again.", validation);
					TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, Core.Constants.ExchangeRateTypes.Code.SellRate, 6.11m, today, today);
					AssertHasNoValidationErrors(validation);
				}

				using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV"))
				{
					TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, Core.Constants.ExchangeRateTypes.Code.BuyRate, 6.12m, invoiceDate, invoiceDate);
					AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the CNY exchange rate is not set for the date 30-Apr-15. Please check your data and try again.", validation);
					TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, Core.Constants.ExchangeRateTypes.Code.SellRate, 6.12m, invoiceDate, invoiceDate);
					AssertHasNoValidationErrors(validation);
				}

				using (PostingExRateRegistryAR.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST"))
				{
					TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, Core.Constants.ExchangeRateTypes.Code.BuyRate, 6.13m, postDate, postDate);
					AssertHasValidationError(@"This job cannot be posted. The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the CNY exchange rate is not set for the date 29-Apr-15. Please check your data and try again.", validation);
					TestObjectCreator.CreateExchangeRate(TestObjectCreator.CNY, Core.Constants.ExchangeRateTypes.Code.SellRate, 6.13m, postDate, postDate);
					AssertHasNoValidationErrors(validation);
				}

				ExchangeRateReader.GetReaderInstance().ClearCache();
			}
		}

		public override void TestRunJobChargeSupplyTypeValidation()
		{
			var helper = new AccountingPeriodTestHelper(Factory);
			helper.SetupPeriods();

			SetupInvoiceStyles(TestObjectCreator.ABIGAS, InvoicePostingOptionsList.Codes.DisbursementFreightAndFinal);

			Factory.Save();

			var plugIn = TestObjectCreator.GetTestShipmentPlugIn();
			var testJob = new Job.Loader(plugIn).TryCreateWithoutMutexForTestOnly();
			var deferredCharge = testJob.Charges.AddNew();
			deferredCharge.JR_AC = TestObjectCreator.CC1.PK;
			deferredCharge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			deferredCharge.JR_OSSellAmt = 100m;
			deferredCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var deferredCharge2 = testJob.Charges.AddNew();
			deferredCharge2.JR_AC = TestObjectCreator.CC1.PK;
			deferredCharge2.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			deferredCharge2.JR_OSSellAmt = 100m;
			deferredCharge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			testJob.RunPreSaveValidation();
			AssertNoErrors(testJob);

			Factory.Save();

			var validation = NewPostManagerValidation(new[] { testJob }, JobInvoicingPostingOption.Revenue);

			AssertEquals("Precondition", true, validation.ShouldMandatorySellSupplyType_ForTestOnly(deferredCharge.JR_InvoiceType));
			AssertEquals("Precondition", true, validation.ShouldMandatorySellSupplyType_ForTestOnly(deferredCharge2.JR_InvoiceType));

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Charges without Supply Type specified cannot be included in the invoice.", validation.RunJobChargeSupplyTypeValidation_ForTestOnly());
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertNullOrEmpty(validation.RunJobChargeSupplyTypeValidation_ForTestOnly());
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				AssertNullOrEmpty(validation.RunJobChargeSupplyTypeValidation_ForTestOnly());
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (AccountingMasterFilesRegistry.Instance.SupplyTypeClassificationCodesIsMandatory.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				AssertNullOrEmpty(validation.RunJobChargeSupplyTypeValidation_ForTestOnly());
			}
		}

		protected override PostManagerValidation NewPostManagerValidation(IEnumerable<Job> jobs, JobInvoicingPostingOption postingOption, IEnumerable<Job> originalJobs)
		{
			if (postingOption == JobInvoicingPostingOption.Revenue)
			{
				return new PeriodicInvoicePostManagerValidation(jobs.First(), jobs.First().Charges.Cast<Charge>(), null);
			}
			else
			{
				return base.NewPostManagerValidation(jobs, postingOption, originalJobs);
			}
		}

		protected override string DisbursementInvoiceType
		{
			get { return InvoiceTypesList.Codes.DisbursementInvoice_Batching; }
		}

		protected override string FinalInvoiceType
		{
			get { return InvoiceTypesList.Codes.FinalInvoice_Batching; }
		}

		protected override bool ShouldErrorWhenInvoiceTypeIsEmpty
		{
			get { return false; }
		}

		protected override void SetupInvoiceStyles(OrgHeader debtor, string invoicePostingOption)
		{
			base.SetupInvoiceStyles(debtor, invoicePostingOption);

			OrgInvoiceType type = debtor.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
			type.PI_RS_NKServiceLevel = "STD";
		}

		protected override void AssertIsCostEligibleToPost(Job job, Charge charge, JobInvoicingPostingOption postingOption)
		{
			var validator = NewPostManagerValidation(new Job[] { job }, postingOption);
			AssertEquals(false, validator.IsCostEligibleToPost_ForTestOnly(charge));
		}
	}
}
