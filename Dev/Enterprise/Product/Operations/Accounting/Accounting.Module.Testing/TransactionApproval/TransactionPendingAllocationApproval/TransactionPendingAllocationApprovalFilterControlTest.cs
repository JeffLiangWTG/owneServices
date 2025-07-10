using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	public class TransactionPendingAllocationApprovalFilterControlTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			var approvals = new TransactionPendingAllocationApprovalRequestCollection(Factory);
			var filterBO = new TransactionApprovalFilterBusinessObject();

			using (var form = new ZForm())
			{
				var filterControl = new TransactionPendingAllocationApprovalFilterControl(approvals, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}

		public void TestTransactionPendingAllocationApprovalColumns() => AssertColumnStyles(eInvoicingEnabled: false, ColumnDefinitions_Default);

		public void TestTransactionPendingAllocationApprovalColumnsWhenEInvoicingEnabled() => AssertColumnStyles(eInvoicingEnabled: true, ColumnDefinitions_EInvoicingEnabled);

		void AssertColumnStyles(bool eInvoicingEnabled, params string[] expectedListOfColumns)
		{
			AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, eInvoicingEnabled);

			using (var filterControl = new TransactionPendingAllocationApprovalFilterControl())
			{
				var grid = filterControl.FilteredGrid;

				var realListOfColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x.ColumnName}, {(x.IsVisible ? "visible" : "hidden")}, {(x.IsUnavailable ? "unavailable" : "available")}").ToArray();

				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);
			}
		}

		string[] ColumnDefinitions_Default => new[] {
			"JobNumber, visible, available",
			"XP_GB_RequestingBranch, visible, available",
			"XP_ApprovalStatus, visible, available",
			"XP_GS_NKApprovingUser1, visible, available",
			"XP_ApprovalDate, visible, available",
			"XP_ReasonDescription, visible, available",
			"LinkedTransaction+AH_ComplianceSubType, hidden, unavailable",
			"LinkedTransaction+AH_OH, hidden, unavailable",
			"LinkedTransaction+InvoiceDate, hidden, unavailable",
			"LinkedTransaction+EInvoicingStatus, hidden, unavailable",
			"LinkedTransaction+EInvoicingError, hidden, unavailable",
		};

		string[] ColumnDefinitions_EInvoicingEnabled => new[] {
			"JobNumber, visible, available",
			"XP_GB_RequestingBranch, visible, available",
			"XP_ApprovalStatus, visible, available",
			"XP_GS_NKApprovingUser1, visible, available",
			"XP_ApprovalDate, visible, available",
			"XP_ReasonDescription, visible, available",
			"LinkedTransaction+AH_ComplianceSubType, hidden, available",
			"LinkedTransaction+AH_OH, hidden, available",
			"LinkedTransaction+InvoiceDate, hidden, available",
			"LinkedTransaction+EInvoicingStatus, hidden, available",
			"LinkedTransaction+EInvoicingError, hidden, available",
		};
	}
}
