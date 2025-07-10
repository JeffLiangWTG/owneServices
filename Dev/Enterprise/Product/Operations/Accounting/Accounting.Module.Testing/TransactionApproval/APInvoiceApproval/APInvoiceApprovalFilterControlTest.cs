using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	public class APInvoiceApprovalFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			var approvals = new APInvoiceChargesApprovalRequestCollection(Factory);
			var filterBO = new TransactionApprovalFilterBusinessObject();

			using (ZForm form = new ZForm())
			{
				APInvoiceApprovalFilterControl filterControl = new APInvoiceApprovalFilterControl(approvals, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}

		ZBool ColumnExistsInTheGrid(ZDisplayGrid grid, ZString columnName)
		{
			foreach (ZGridColumnInfo columnStyle in grid.ColumnStyles)
			{
				if (columnStyle.ColumnName == columnName)
				{
					return ZBool.True;
				}
			}
			return ZBool.False;
		}

		public void TestAPInvoiceApproval_TaxBranchColumn()
		{
			var approvals = new APInvoiceChargesApprovalRequestCollection(Factory);
			var filterBO = new TransactionApprovalFilterBusinessObject();

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (var filterControl = new APInvoiceApprovalFilterControl(approvals, filterBO))
			{
				Assert("Tax Branch column should not exists for AP Invoice approval", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "TaxBranch"));
			}

			AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			using (var filterControl = new APInvoiceApprovalFilterControl(approvals, filterBO))
			{
				Assert("Tax Branch column should exists for AP Invoice approval", ColumnExistsInTheGrid(filterControl.FilteredGrid, "TaxBranch"));
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			using (var filterControl = new APInvoiceApprovalFilterControl(approvals, filterBO))
			{
				Assert("Tax Branch column should not exists for AP Invoice approval", !ColumnExistsInTheGrid(filterControl.FilteredGrid, "TaxBranch"));
			}
		}
	}
}
