using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module.Testing
{
	public class JobManagementFilterControlBaseTest : TestCaseWithFactory
	{
		public void TestHoldReasonColumn()
		{
			using (var filterControl = new JobManagementFilterControlBase(new JobManagementCollection(Factory), new JobManagementFilterBusinessObject()))
			{
				bool columnExistsAndNotVisible = filterControl.FilteredGrid.ColumnStyles
										.Cast<ZGridColumnInfo>()
										.Any(col => col.ColumnName == nameof(JobHeader.JH_HoldReason) && !col.IsVisible);

				Assert("Hold Reason column should exist and should NOT be visible.", columnExistsAndNotVisible);
			}
		}

		public void TestTaxBranchColumnAvailability()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("IsTaxBranchApplicable: false", !AccountingMasterFilesUtils.IsTaxBranchApplicable);
			var colTaxBranch = GetColumnByName(AutoJobHeader.Schema.JH_GB_TaxBranch);
			AssertNull("When currentCompany's GC_IsGSTRegistered is false, column 'JH_GB_TaxBranch' should not be added.", colTaxBranch);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Assert("IsTaxBranchApplicable: false", !AccountingMasterFilesUtils.IsTaxBranchApplicable);
			colTaxBranch = GetColumnByName(AutoJobHeader.Schema.JH_GB_TaxBranch);
			AssertNull("When registry EnableTaxBranchReporting is false, column 'JH_GB_TaxBranch' should not be added.", colTaxBranch);

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Assert("IsTaxBranchApplicable: true", AccountingMasterFilesUtils.IsTaxBranchApplicable);
			colTaxBranch = GetColumnByName(AutoJobHeader.Schema.JH_GB_TaxBranch);
			AssertNotNull("IsTaxBranchApplicable: true, column 'JH_GB_TaxBranch' should be added.", colTaxBranch);
		}

		public void TestParentJobNumberColumnAvailability()
		{
			var parentJobNumCol = GetColumnByName("ParentJobNumber");
			AssertNotNull("The parent Job Number column should always be there.", parentJobNumCol);
		}

		ZGridColumn GetColumnByName(string columnName)
		{
			using (var form = new ZForm())
			using (var moduleToTest = (JobManagementModule)ZArchitecture.Modules.ZModuleFactory.Instance.Create(ZArchitecture.Modules.ModuleIDs.JobManagement))
			{
				var control = (JobManagementFilterControl)moduleToTest.EmbeddedControl;
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				return control.FilteredGrid.Columns[columnName];
			}
		}
	}
}
