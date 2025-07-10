using System;
using System.Collections.Generic;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.GUI.ARAP.UnapprovedAPTransaction
{
	public partial class UnapprovedTransactionAuthorisationForm
	{
		public void CandidatesGrid_ApproveInvoices_Click_ForTestOnly(object sender, EventArgs e)
		{
			CandidatesGrid_ApproveInvoices_Click(sender, e);
		}

		public void CandidatesGrid_ApproveInvoicesWithClaim_Click_ForTestOnly(object sender, EventArgs e)
		{
			CandidatesGrid_ApproveInvoicesWithClaim_Click(sender, e);
		}

		public void CadidatesGrid_EditApproveTransactionsWithClaim_Click_ForTestOnly(object sender, EventArgs e)
		{
			CadidatesGrid_EditApproveTransactionsWithClaim_Click(sender, e);
		}
		public void CadidatesGrid_EditApproveTransactions_Click_ForTestOnly(object sender, EventArgs e)
		{
			CadidatesGrid_EditApproveTransactions_Click(sender, e);
		}

		public void ZButtonApproveAll_Click_ForTestOnly(object sender, EventArgs e)
		{
			zButtonApproveAll_Click(sender, e);
		}

		public void HandleInvalidTransactions_ForTestOnly(List<InvoicingBase> approvedSelfBillingInvoices, bool withGui, bool withClaim, bool areThereAnyInvalidTransactions, string additionalErrorInfo)
		{
			HandleInvalidTransactions(approvedSelfBillingInvoices, withGui, withClaim, areThereAnyInvalidTransactions, additionalErrorInfo);
		}

		public ZArchitecture.ZGrid CandidatesGrid_ForTestOnly => CandidatesGrid;

		public bool ShouldMakeFormReadOnly_ForTestOnly(InvoicingBase invoice)
		{
			return ShouldMakeFormReadOnly(invoice);
		}

		public void CandidatesGridContextMenu_Popup_ForTestOnly(object sender, EventArgs e)
		{
			CandidatesGridContextMenu_Popup(sender, e);
		}

		public ZArchitecture.ZTextBox MaxAuthorisationLevelTextBox_ForTestOnly => MaxAuthorisationLevelTextBox;
		public ZArchitecture.GUI.ZButton ZButtonApproveAll_ForTestOnly => zButtonApproveAll;

		public UnapprovedTransactionConverter BusinessEntity_ForTestOnly => BusinessEntity;
	}
}
