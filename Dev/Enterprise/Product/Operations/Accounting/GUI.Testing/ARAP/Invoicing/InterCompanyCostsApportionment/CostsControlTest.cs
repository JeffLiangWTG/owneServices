using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;

namespace Enterprise.Accounting.GUI.Testing.ARAP.Invoicing.InterCompanyCostsApportionment
{
	class CostsControlTest : TestCaseWithFactory
	{
		public void TestAL_TaxDateColumn()
		{
			var businessEntity = new IntercompanyCostsApportionmentInvoice(Factory);
			using (var form = new IntercompanyCostsApportionmentForm(businessEntity))
			{
				form.Show();
				var control = form.GetControl<CostsControl>("costsControl");
				AssertNotNull(control);
				var taxDateColumn = control.LineSummaryGrid.GetColumnStyle("AL_TaxDate");
				AssertNotNull(taxDateColumn);
				AssertEquals(false, taxDateColumn.IsVisible);
			}
		}

		public void TestSupplyTypeColumnVisibility()
		{
			var businessEntity = new IntercompanyCostsApportionmentInvoice(Factory);

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (var form = new IntercompanyCostsApportionmentForm(businessEntity))
			{
				form.Show();
				var control = form.GetControl<CostsControl>("costsControl");
				AssertNotNull(control);

				AssertEquals("Supply Type column should be available.", true, control.LineSummaryGrid.Columns.Contains("AL_SupplyType"));
			}

			using (AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (var form = new IntercompanyCostsApportionmentForm(businessEntity))
			{
				form.Show();
				var control = form.GetControl<CostsControl>("costsControl");
				AssertNotNull(control);

				AssertEquals("Supply Type column shouldn't be available.", false, control.LineSummaryGrid.Columns.Contains("AL_SupplyType"));
			}
		}

		public void TestTaxBranchVisibility()
		{
			AssertTaxBranchVisibility(true, true);
			AssertTaxBranchVisibility(true, false);
			AssertTaxBranchVisibility(false, true);
			AssertTaxBranchVisibility(false, false);

			void AssertTaxBranchVisibility(bool isEnaleRegistry, bool isGSTRegistered)
			{
				var businessEntity = new IntercompanyCostsApportionmentInvoice(Factory);

				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnaleRegistry))
				using (var form = new IntercompanyCostsApportionmentForm(businessEntity))
				{
					form.Show();
					var control = form.GetControl<CostsControl>("costsControl");
					AssertNotNull(control);

					var expectedVisible = isEnaleRegistry && isGSTRegistered;

					AssertEquals(expectedVisible, control.LineSummaryGrid.Columns.Contains(IntercompanyCostsApportionmentInvoiceLine.Schema.TaxBranch));
					AssertEquals(expectedVisible, control.LineSummaryGrid.Columns.Contains(IntercompanyCostsApportionmentInvoiceLine.Schema.TaxBranchName));
					AssertEquals(expectedVisible, control.TaxBranchGuidFindBox_ForTestOnly.Visible);
				}
			}
		}
	}
}
