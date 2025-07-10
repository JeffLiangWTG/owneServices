using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;

namespace Enterprise.Accounting.GUI.Testing.ARAP.Invoicing.InterCompanyCostsApportionment
{
	internal class ApportionmentControlTest : TestCaseWithFactory
	{
		public void TestTaxBranchVisibility()
		{
			AssertTaxBranchVisibility(true, true);
			AssertTaxBranchVisibility(true, false);
			AssertTaxBranchVisibility(false, true);
			AssertTaxBranchVisibility(false, false);

			void AssertTaxBranchVisibility(bool isEnableRegistry, bool isGSTRegistered)
			{
				var businessEntity = new IntercompanyCostsApportionmentInvoice(Factory);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnableRegistry))
				using (var form = new IntercompanyCostsApportionmentForm(businessEntity))
				{
					form.Show();
					var control = form.GetControl<ApportionmentControl>("apportionmentDetails");
					AssertNotNull(control);

					var expectedVisible = isEnableRegistry && isGSTRegistered;

					AssertEquals(expectedVisible, control.apportionmentDetailsGrid.Columns.Contains(IntercompanyCostsApportionment.Schema.TaxBranch));
					AssertEquals(expectedVisible, control.apportionmentDetailsGrid.Columns.Contains(IntercompanyCostsApportionment.Schema.TaxBranchName));
				}
			}
		}
	}
}
