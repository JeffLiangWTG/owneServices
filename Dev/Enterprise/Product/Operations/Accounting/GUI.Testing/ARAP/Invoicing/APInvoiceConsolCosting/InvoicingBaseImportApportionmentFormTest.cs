using System;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoicingBaseImportApportionmentForm))]
	public class InvoicingBaseImportApportionmentFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			APInvoice inv = TestObjectCreator.CreateInvoiceWithMinimumTestData<APInvoice>(Factory);
			JobConsolCost cost = inv.ConsolCosting.ConsolCosts.AddNew();
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			cost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, "JK");
			InvoicingBaseConsolCostImporter importer = new InvoicingBaseConsolCostImporter(Factory, cost, inv);
			return new InvoicingBaseImportApportionmentForm(importer);
		}

		public void TestTaxBranchVisibility()
		{
			AssertTaxBranchVisibility(true, true);
			AssertTaxBranchVisibility(true, false);
			AssertTaxBranchVisibility(false, true);
			AssertTaxBranchVisibility(false, false);

			void AssertTaxBranchVisibility(bool isEnableRegistry, bool isGSTRegistered)
			{
				var invoice = Factory.NewWithValidTestData<APInvoice>();
				var cost = Factory.NewWithValidTestData<JobConsolCost>();
				var businessEntity = new InvoicingBaseConsolCostImporter(Factory, cost, invoice);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnableRegistry))
				using (var form = new InvoicingBaseImportApportionmentForm(businessEntity))
				{
					form.Show();

					var expectedVisible = isEnableRegistry && isGSTRegistered;
					AssertEquals(expectedVisible, form.ConsolCostsToImportGrid.Columns.Contains(AutoJobConsolCost.Schema.E6_GB_CostTaxBranch));
				}
			}
		}
	}
}
