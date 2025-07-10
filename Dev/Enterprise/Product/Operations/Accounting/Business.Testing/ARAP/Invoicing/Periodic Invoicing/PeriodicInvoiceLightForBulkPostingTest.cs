using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Accounting.Business.ARAP.Invoicing.PeriodicInvoiceBulkPoster;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(PeriodicInvoiceLightForBulkPosting))]
	public class PeriodicInvoiceLightForBulkPostingTest : PeriodicInvoiceLightTest
	{
		public void TestSellReferenceByInvoiceInfo()
		{
			SetUpPeriodicInvoiceLightForBulkPosting();
			TestInvoiceInfo.TaxBranch = TestObjectCreator.NonCurrentBranch.PK;
			var invoice = (PeriodicInvoiceLightForBulkPosting)GetNewBusinessObject();
			invoice.InitalizeAndValidate(TestInvoiceInfo);
			AssertEquals(TestObjectCreator.NonCurrentBranch.PK, invoice.TaxBranch);
		}

		public void TestInvoiceTermFieldsByInvoiceInfo()
		{
			var expectDueDate = ZDateTime.Now;
			SetUpPeriodicInvoiceLightForBulkPosting();
			TestInvoiceInfo.InvoiceTerm = "MIC";
			TestInvoiceInfo.InvoiceTermDays = 90;
			TestInvoiceInfo.DueDate = expectDueDate;
			var invoice = (PeriodicInvoiceLightForBulkPosting)GetNewBusinessObject();
			invoice.InitalizeAndValidate(TestInvoiceInfo);
			AssertEquals("MIC", invoice.InvoiceTerm);
			AssertEquals(new ZByte(90), invoice.InvoiceTermDays);
			AssertEquals(expectDueDate, invoice.DueDate);
		}

		public void TestTaxBranchByInvoiceInfo()
		{
			SetUpPeriodicInvoiceLightForBulkPosting();
			TestInvoiceInfo.SellReference = "Test Sell Reference";
			var invoice = (PeriodicInvoiceLightForBulkPosting)GetNewBusinessObject();
			invoice.InitalizeAndValidate(TestInvoiceInfo);
			AssertEquals("Test Sell Reference", invoice.SellReference);
		}

		public override void TestRunPreSaveValidationCore()
		{
			SetUpPeriodicInvoiceLightForBulkPosting();

			var invoice = (PeriodicInvoiceLightForBulkPosting)GetNewBusinessObject();
			invoice.InitalizeAndValidate(TestInvoiceInfo);
			((IBusiness)invoice).ValidateIfQuickAndImprovesPreSaveValidationPerformance();
			AssertNoErrors(invoice);

			TestInvoiceInfo.LocalExTaxAmount = 100M;
			TestInvoiceInfo.LocalTaxAmount = 5M;

			TestCharge.JR_OSCostAmt = TestCharge.JR_OSSellAmt = 0M;
			TestCharge.Delete();
			TestCharge.Factory.Save();

			ReleaseFactory();
			invoice = (PeriodicInvoiceLightForBulkPosting)GetNewBusinessObject();
			invoice.InitalizeAndValidate(TestInvoiceInfo);
			((IBusiness)invoice).ValidateIfQuickAndImprovesPreSaveValidationPerformance();
			AssertHasRowError(invoice, "Charge count for the invoice was changed from 1 to 0.");
			AssertHasRowError(invoice, "Total Local Ex Tax Amount of this invoice has changed from $100.00 to $50.00.");
			AssertHasRowError(invoice, "Total Local Tax Amount of this invoice has changed from $5.00 to $0.00.");
		}

		void SetUpPeriodicInvoiceLightForBulkPosting()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-1));

			var type = TestObjectCreator.Agent.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;

			var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001"));
			job.LocalChargesPK = TestObjectCreator.LocalClient.PK;
			TestCharge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, TestObjectCreator.Agent);
			TestCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

			var glAccount = TestObjectCreator.CreateARSuspenseControlAccount();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			InvoicingBase miscInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1M);
			miscInvoice.AH_OH = TestObjectCreator.Agent.PK;
			miscInvoice.AH_PostDate = ZDateTime.Today.AddDays(-10);
			miscInvoice.IsDisbursementOrFinal = false;
			TestObjectCreator.CreateInvoiceLine(miscInvoice, TestObjectCreator.AUD, 1M, 50M, 0M, 0M, glAccount.PK);

			Factory.Save();

			TestInvoiceInfo = new InvoiceInfo();
			TestInvoiceInfo.DebtorPK = TestObjectCreator.Agent.PK;
			TestInvoiceInfo.InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			TestInvoiceInfo.CurrencyNK = TestObjectCreator.AUD.RX_Code;
			TestInvoiceInfo.InvoiceDate = ZDateTime.Today;
			TestInvoiceInfo.PostDate = ZDateTime.Today;
			TestInvoiceInfo.SelectedJobs = new[] { job.PK };
			TestInvoiceInfo.Charges = new[] { TestCharge.PK };
			TestInvoiceInfo.MiscInvoices = new[] { miscInvoice.PK };
			TestInvoiceInfo.LocalExTaxAmount = 150M;
			TestInvoiceInfo.LocalTaxAmount = 10M;

			ReleaseFactory();
		}

		public override void TestLoadMiscInvoicesWithOtherTaxes()
		{
			Assert("This is not applicable for PeriodicInvoiceLightForBulkPosting", true);
		}

		public override void TestPeriodicInvoiceDoesNotLoadMiscInvoicesWithOtherTaxes()
		{
			Assert("This is not applicable for PeriodicInvoiceLightForBulkPosting", true);
		}

		public override void TestRemoveJobsWhichAllChargesShouldPostAutoJRJ_AllChargesShouldNotPost()
		{
			Assert("This is not applicable for PeriodicInvoiceLightForBulkPosting", true);
		}

		public override void TestRemoveJobsWhichAllChargesShouldPostAutoJRJ_PartOfChargesShouldPost()
		{
			Assert("This is not applicable for PeriodicInvoiceLightForBulkPosting", true);
		}

		public override void TestReloadChargesByJob_ExcludeAutoJRJ()
		{
			Assert("This is not applicable for PeriodicInvoiceLightForBulkPosting", true);
		}

		public override void TestRemoveJobsWhichAllChargesShouldPostAutoJRJ_AllOfChargesShouldPost()
		{
			Assert("This is not applicable for PeriodicInvoiceLightForBulkPosting", true);
		}

		InvoiceInfo TestInvoiceInfo;
		Charge TestCharge;

		protected override void SetupPeriodicInvoiceWithJobsAndMiscInvoices(PeriodicInvoiceBase invoice, ZGuid[] jobPks, ZGuid[] chargePks, ZGuid[] invoicePks)
		{
			var invoiceInfo = PrepareInvoiceInfo(invoice, jobPks, chargePks, invoicePks);
			((PeriodicInvoiceLightForBulkPosting)invoice).InitalizeAndValidate(invoiceInfo);
		}

		protected override void Initialize(InvoiceInfo invoiceInfo)
		{
			(testPeriodicInvoice_internalValue as PeriodicInvoiceLightForBulkPosting)?.InitalizeAndValidate(invoiceInfo);
		}
	}
}
