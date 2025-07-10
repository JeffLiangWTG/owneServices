using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseBulkChargeImporterDependentJob))]
	public class InvoicingBaseBulkChargeImporterDependentJobTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesInvoicingBaseBulkChargeImporterDependentJob()
		{
			var consol = TestObjectCreator.CreateConsol("ABC", "DEF", "C0001");
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			var jobB = TestObjectCreator.CreateJob(shipment);
			TestObjectCreator.CreateCharge(jobB);
			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			InvoicingBaseBulkChargeImporter importer = new InvoicingBaseBulkChargeImporter(invoice);
			importer.LoadJobsCollection();
			var job = importer.Jobs.FirstOrDefault() as InvoicingBaseBulkChargeImporterDependentJob;
			AssertNotNull(job);
			AssertNotNull("Company should not be null", job.Company);

			var localList = new List<string>
			{
				nameof(job.SelectedChargesLocalCostAmount)
			};

			var osList = new List<string>
			{
				nameof(job.SelectedChargesInvoiceCurrencyAmount)
			};

			var exList = new List<string>
			{
				nameof(job.InvoiceCurrencyJobExchangeRate)
			};

			var tester = new DecimalPlacesAttributeTester(job, job.Company);
			tester.CheckLocalCurrency(localList, nameof(job.LocalDecimals));
			tester.CheckNonLocalCurrency(osList, nameof(job.OSDecimals), nameof(invoice.AH_RX_NKTransactionCurrency), invoice);
			tester.CheckExchangeRate(exList, nameof(job.ExchangeRateDecimals));
		}

		public void TestSelectedChargesInvoiceCurrencyAmount_UseJobExRate()
		{
			var consol = TestObjectCreator.CreateConsol("ABC", "DEF", "C0001");
			consol.JK_MasterBillNum = "12345678901";
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			shipment.JS_HouseBill = "10987654321";
			var job = TestObjectCreator.CreateJob(shipment);

			var rateUSD = job.ExchangeRates.AddNew();
			rateUSD.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			rateUSD.JF_BaseRate = 1.2m;
			var rateGBP = job.ExchangeRates.AddNew();
			rateGBP.JF_RX_NKRateCurrency = TestObjectCreator.GBP.RX_Code;
			rateGBP.JF_BaseRate = 1.33m;

			var charge1 = TestObjectCreator.CreateCharge(job);
			charge1.IsSelectedForImport = true;
			charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge1.JR_OSCostAmt = 2m;

			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			invoice.UseJobExchangeRate = true;

			InvoicingBaseBulkChargeImporter importer = new InvoicingBaseBulkChargeImporter(invoice);
			importer.LoadJobsCollection();
			var importedJob = importer.Jobs.FirstOrDefault() as InvoicingBaseBulkChargeImporterDependentJob;
			AssertNotNull(importedJob);

			var charge2 = importedJob.Charges.AddNew();
			charge2.IsSelectedForImport = true;
			charge2.JR_RX_NKCostCurrency = TestObjectCreator.GBP.RX_Code;
			charge2.JR_OSCostAmt = 4m;

			AssertEquals(5.61m, importedJob.SelectedChargesInvoiceCurrencyAmount);
		}

		public void TestSelectedChargesInvoiceCurrencyAmount_FallbackToInvoiceExRateWhenUseJobExRateIsNotSet()
		{
			var consol = TestObjectCreator.CreateConsol("ABC", "DEF", "C0001");
			consol.JK_MasterBillNum = "12345678901";
			var shipment = TestObjectCreator.CreateShipment("S0001", consol);
			shipment.JS_HouseBill = "10987654321";
			var job = TestObjectCreator.CreateJob(shipment);

			var rateUSD = job.ExchangeRates.AddNew();
			rateUSD.JF_RX_NKRateCurrency = TestObjectCreator.USD.RX_Code;
			rateUSD.JF_BaseRate = 1.2m;
			var rateGBP = job.ExchangeRates.AddNew();
			rateGBP.JF_RX_NKRateCurrency = TestObjectCreator.GBP.RX_Code;
			rateGBP.JF_BaseRate = 1.33m;

			var charge1 = TestObjectCreator.CreateCharge(job);
			charge1.IsSelectedForImport = true;
			charge1.JR_RX_NKCostCurrency = TestObjectCreator.USD.RX_Code;
			charge1.JR_OSCostAmt = 2m;

			Factory.Save();

			APInvoice invoice = Factory.New<APInvoice>();
			invoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			Assert("Precondition", !invoice.UseJobExchangeRate);
			invoice.AH_ExchangeRate = 1.5m;

			InvoicingBaseBulkChargeImporter importer = new InvoicingBaseBulkChargeImporter(invoice);
			importer.LoadJobsCollection();
			var importedJob = importer.Jobs.FirstOrDefault() as InvoicingBaseBulkChargeImporterDependentJob;
			AssertNotNull(importedJob);

			var charge2 = importedJob.Charges.AddNew();
			charge2.IsSelectedForImport = true;
			charge2.JR_RX_NKCostCurrency = TestObjectCreator.GBP.RX_Code;
			charge2.JR_OSCostAmt = 4m;

			AssertEquals(6.51m, importedJob.SelectedChargesInvoiceCurrencyAmount);
		}

		public void TestIsSelectedForImportIsAlwaysWritable()
		{
			bool isAllowed = Env.Security.ModifyChargesOfCompleteJobs.IsAllowed;
			try
			{
				InvoicingBase invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1);
				InvoicingBaseBulkChargeImporter importer = new InvoicingBaseBulkChargeImporter(invoice);
				var job = Factory.NewJobWithValidTestDataForTesting<InvoicingBaseBulkChargeImporterDependentJob>();

				Env.Security.ModifyChargesOfCompleteJobs.IsAllowed = false;
				job.JH_Status = JobHeaderStatus.Complete.Code;

				AssertEquals("This property should always be writable", false, job.IsSelectedForImportInfo.ReadOnly);
			}
			finally
			{
				Env.Security.ModifyChargesOfCompleteJobs.IsAllowed = isAllowed;
			}
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Shouldn't be fired on this business object", true);
		}

		public void TestInvoiceCurrencyJobExchangeRate_ShouldSetOneAsLocalCurrency()
		{
			var exRate = Factory.New<RefExchangeRate>();
			exRate.RE_GC = GlbCompany.CurrentCompany.PK;
			exRate.RE_ExRateType = nameof(ExchangeRateType.Buy);
			exRate.RE_RX_NKExCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			exRate.RE_StartDate = new ZDateTime(ZDateTime.Today.Year, 1, 1);
			exRate.RE_ExpiryDate = new ZDateTime(ZDateTime.Today.Year, 12, 31);
			exRate.RE_SellRate = 0.8m;
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "I0001", TestObjectCreator.AUD, 4m, TestObjectCreator.ABIGAS);
			invoice.AH_PostDate = ZDateTime.Today;
			invoice.AH_InvoiceDate = ZDateTime.Today;
			invoice.AH_PostedToEFT = true;
			invoice.UseJobExchangeRate = true;

			var importer = new InvoicingBaseBulkChargeImporter(invoice);
			var job = Factory.NewJobWithValidTestDataForTesting<InvoicingBaseBulkChargeImporterDependentJob>();
			job.Master = importer;

			AssertEquals("Local currency exchange rate should equal 1", 1.0m, job.InvoiceCurrencyJobExchangeRate);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewJobForTesting<InvoicingBaseBulkChargeImporterDependentJob>();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			factory.SetContext(BusinessContext.JobCreatedFromJobLoader);
			var result = base.GetNewBusinessObjectForDeleteTest(factory);
			factory.RemoveContext(BusinessContext.JobCreatedFromJobLoader);
			return result;
		}

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
	}
}
