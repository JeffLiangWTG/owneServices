#if DEBUG

using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Accounting.GUI.ARAP;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class PaymentProcessingModule
	{
		public SecurityCheckpoint PrintCheckpoint_ForTestOnly => PrintCheckpoint;

		public BusinessObjectFactory Factory_ForTestOnly => Factory;

		public BusinessObject[] SelectedBusinessObjects_ForTestOnly => SelectedBusinessObjects;

		public List<Business.ARAP.PaymentApproval.PaymentApprovalBase> ReloadApprovalsInTheNewFactory_ForTestOnly(BusinessObject[] selectedApprovals, BusinessObjectFactory factory)
		{
			return PaymentProcessingGUIHelper.ReloadApprovalsInTheNewFactory(selectedApprovals, factory);
		}

		public void PopulateChequeNoForPaymentApprovals_ForTestOnly(object sender, EventArgs e)
		{
			PopulateChequeNoForPaymentApprovals(sender, e);
		}

		public void PopulateChequeNoAndPostPaymentApprovals_ForTestOnly(object sender, EventArgs e)
		{
			PopulateChequeNoAndPostPaymentApprovals(sender, e);
		}

		public IZForm ShowDeleteForm_ForTestOnly(BusinessObject selectedBusinessObject)
		{
			return ShowDeleteForm(selectedBusinessObject);
		}

		public void AuthorisePaymentApprovals_ForTestOnly(object sender, EventArgs e)
		{
			AuthorisePaymentApprovals(sender, e);
		}

		public void UnAuthorisePaymentApprovals_ForTestOnly(object sender, EventArgs e)
		{
			UnAuthorisePaymentApprovals(sender, e);
		}

		public void CancelPaymentApprovals_ForTestOnly(object sender, EventArgs e)
		{
			CancelPaymentApprovals(sender, e);
		}

		public void RejectPaymentApprovals_ForTestOnly(object sender, EventArgs e)
		{
			RejectPaymentApprovals(sender, e);
		}

		public void PrintPaymentApproval_ForTestOnly(object sender, EventArgs e)
		{
			PrintPaymentApproval(sender, e);
		}
	}
}

#endif
