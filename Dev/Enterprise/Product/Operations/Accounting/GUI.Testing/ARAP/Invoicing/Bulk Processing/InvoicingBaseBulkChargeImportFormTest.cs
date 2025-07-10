using System;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	public class InvoicingBaseBulkChargeImportFormTest : TestCaseWithFactory
	{
		public void TestFindWithOptionRecompile()
		{
			var invoice = Factory.New<APInvoice>();
			var importer = new InvoicingBaseBulkChargeImporter(invoice);

			using (var form = new InvoicingBaseBulkChargeImportForm(importer))
			{
				form.Show();
				ZFormTest.SetFilterStrip(form, 2, "cont\t", "s", "a");
				((InvoicingBaseBulkChargeImporter)form.BusinessEntity).LoadJobsCollection();

				var lastQuery = SqlEventTracker.Instance.LastSqlQuery;
				Assert("The query should have OPTION (RECOMPILE): " + lastQuery, lastQuery.Contains("OPTION (RECOMPILE)"));
			}
		}

		public void TestJobChargeDescriptionDisplay()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var forwardingShipment = testObjectCreator.CreateShipment("S00001234");
			var job = testObjectCreator.CreateJob(forwardingShipment);
			var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, 10m, 10m);
			AssertEquals("Charge Description is Desc", "Desc", charge.JR_Desc);
			Factory.Save();

			var aPInvoice = testObjectCreator.CreateAPInvoice<APInvoice>("ACE1", testObjectCreator.EUR, 1m, 100m, 0.0m, 0.0m, 100m, 0.0m, 0.0m);
			var importer = new InvoicingBaseBulkChargeImporter(aPInvoice);

			try
			{
				using (var form = new InvoicingBaseBulkChargeImportForm(importer))
				{
					form.Show();
					importer.LoadJobsCollection();
					AssertEquals(1, importer.Jobs.Count);

					var descriptionColumn = form.ChargesGrid_ForTestOnly.GetColumnStyle(JobChargeSchema.Constants.JR_Desc);

					AssertEquals("Job Charge Description is visible.", true, descriptionColumn.IsVisible);
					AssertEquals("Job Charge Description is readonly.", true, descriptionColumn.IsReadOnly);
					form.ChargesGrid_ForTestOnly.Select(0);

					var row = form.ChargesGrid_ForTestOnly.GetFirstSelectedRow();

					AssertNotNull(row);
					AssertEquals("Charge Description is Desc", "Desc", row["JR_Desc"]);
				}
			}
			finally
			{
				job.Dispose();
			}
		}

		public void TestGridImportDataIsDisabled()
		{
			var invoice = Factory.New<APInvoice>();
			var importer = new InvoicingBaseBulkChargeImporter(invoice);
			using (var form = new InvoicingBaseBulkChargeImportForm(importer))
			{
				var isJobsGridImportDisabled = form.JobsGrid_ForTestOnly.DisableImportDataMenuItem;
				var isChargesGridImportDisabled = form.ChargesGrid_ForTestOnly.DisableImportDataMenuItem;
				AssertEquals(true, isJobsGridImportDisabled);
				AssertEquals(true, isChargesGridImportDisabled);
			}
		}

		public void TestCostSupplyTypeColumnVisibility()
		{
			AssertCostSupplyTypeColumnVisibility(true);
			AssertCostSupplyTypeColumnVisibility(false);

			void AssertCostSupplyTypeColumnVisibility(bool isAvailable)
			{
				var invoice = Factory.New<APInvoice>();
				var importer = new InvoicingBaseBulkChargeImporter(invoice);

				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isAvailable))
				using (var form = new InvoicingBaseBulkChargeImportForm(importer))
				{
					form.Show();

					AssertEquals(isAvailable, form.ChargesGrid_ForTestOnly.Columns.Contains(JobChargeSchema.Constants.JR_CostSupplyType));
					if (isAvailable)
					{
						Assert(form.ChargesGrid_ForTestOnly.GetColumnStyle(JobChargeSchema.Constants.JR_CostSupplyType).IsReadOnly);
					}
				}
			}
		}

		public void TestTaxBranchColumnsVisibility()
		{
			AssertTaxBranchColumnsVisibility(true, true);
			AssertTaxBranchColumnsVisibility(true, false);
			AssertTaxBranchColumnsVisibility(false, true);
			AssertTaxBranchColumnsVisibility(false, false);

			void AssertTaxBranchColumnsVisibility(bool isEnaleRegistry, bool isGSTRegistered)
			{
				var invoice = Factory.New<APInvoice>();
				var importer = new InvoicingBaseBulkChargeImporter(invoice);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnaleRegistry))
				using (var form = new InvoicingBaseBulkChargeImportForm(importer))
				{
					form.Show();

					var expectedVisible = isEnaleRegistry && isGSTRegistered;

					AssertEquals(expectedVisible, form.JobsGrid_ForTestOnly.Columns.Contains(JobHeaderSchema.Constants.JH_GB_TaxBranch));
					AssertEquals(expectedVisible, form.ChargesGrid_ForTestOnly.Columns.Contains(JobChargeSchema.Constants.JR_GB_CostTaxBranch));
					if (expectedVisible)
					{
						Assert(form.JobsGrid_ForTestOnly.GetColumnStyle(JobHeaderSchema.Constants.JH_GB_TaxBranch).IsReadOnly);
						Assert(form.ChargesGrid_ForTestOnly.GetColumnStyle(JobChargeSchema.Constants.JR_GB_CostTaxBranch).IsReadOnly);
					}
				}
			}
		}
	}
}
