using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	class InvoicingBaseBulkConsolCostImportFormTest : TestCaseWithFactory
	{
		public void TestColumnsAddCorrectly()
		{
			using (var form = new InvoicingBaseBulkConsolCostImportForm(null))
			{
				var columns = form.ConsolCostsGrid_ForTestOnly.ColumnStyles.Cast<ZGridColumnInfo>();
				AssertColumnsAddCorrectly("E6_GS_NKConsolCostOwner", columns);
			}

			void AssertColumnsAddCorrectly(string columnName, IEnumerable<ZGridColumnInfo> columns, bool isColumnsAvailable = true, bool isVisible = true)
			{
				var expectedColumn = columnName;
				AssertEquals("New column should be added.", isColumnsAvailable, columns.Any(x => x.ColumnName == expectedColumn));
				if (isColumnsAvailable)
				{
					AssertEquals("Assert column is Visible", isVisible, columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible);
					Assert("New column should be read only", columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsReadOnly);
				}
			}
		}

		public void TestSupplyTypeColumnVisibility()
		{
			AssertSupplyTypeColumnVisibility(true);
			AssertSupplyTypeColumnVisibility(false);

			void AssertSupplyTypeColumnVisibility(bool isAvailable)
			{
				var invoice = Factory.New<APInvoice>();
				var costing = new APInvoiceConsolCosting(Factory, invoice);
				var importer = new InvoicingBaseBulkConsolCostImporter(costing);

				using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isAvailable))
				using (var form = new InvoicingBaseBulkConsolCostImportForm(importer))
				{
					form.Show();
					AssertEquals(isAvailable, form.ConsolCostsGrid_ForTestOnly.Columns.Contains("E6_SupplyType"));
				}
			}
		}

		public void TestTaxBranchColumnsVisibility()
		{
			AssertTaxBranchColumnVisibility(true, true);
			AssertTaxBranchColumnVisibility(true, false);
			AssertTaxBranchColumnVisibility(false, true);
			AssertTaxBranchColumnVisibility(false, false);

			void AssertTaxBranchColumnVisibility(bool isEnaleRegistry, bool isGSTRegistered)
			{
				var invoice = Factory.New<APInvoice>();
				var costing = new APInvoiceConsolCosting(Factory, invoice);
				var importer = new InvoicingBaseBulkConsolCostImporter(costing);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnaleRegistry))
				using (var form = new InvoicingBaseBulkConsolCostImportForm(importer))
				{
					form.Show();

					var expectedVisible = isEnaleRegistry && isGSTRegistered;
					AssertEquals(expectedVisible, form.ConsolCostsGrid_ForTestOnly.Columns.Contains(JobConsolCostSchema.Constants.E6_GB_CostTaxBranch));
					AssertEquals(expectedVisible, form.ConsolCostsGrid_ForTestOnly.Columns.Contains(JobConsolCost.Schema.CostTaxBranchName));
				}
			}
		}
	}
}
