using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class RefundEnquiryForm : ZChildForm
	{
		public RefundEnquiryForm(ClientRefundWrapper refundWrapper) : base(refundWrapper) { }

		public ClientRefundWrapper RefundWrapper
		{
			get { return (ClientRefundWrapper)BusinessEntity; }
		}

		public override string FormHeading
		{
			get { return "Enter Refund Enquiry Details"; }
		}

		void zButtonSave_Click(object sender, EventArgs e)
		{
			if (RefundWrapper.EnquiryDetailsValid)
			{
				ClientRefund refund = RefundWrapper.RefundOwner.Refund;
				if (refund == null && RefundWrapper.RefundOwner.RefundManager != null)
				{
					refund = RefundWrapper.RefundOwner.RefundManager.CreateClientRefund();
				}
				if (refund != null && RefundWrapper.HasChanges)
				{
					RefundWrapper.Syncronise(refund);
					refund.PostRefund = true;
				}
				DialogResult = DialogResult.OK;
			}
		}

		void zButtonClose_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
		}
	}
}
