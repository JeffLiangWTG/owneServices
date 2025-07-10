using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class RefundProcessingForm : ZChildForm
	{
		public RefundProcessingForm(ClientRefund refund)
			: base(refund)
		{
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (Refund != null)
			{
				Refund.T10_IsRefundRejectedInfo.ValueChanged -= T10_IsRefundRejectedInfo_ValueChanged;
			}
			base.SetDataBinding(dataSource, dataMember);
			if (Refund != null)
			{
				Refund.T10_IsRefundRejectedInfo.ValueChanged += T10_IsRefundRejectedInfo_ValueChanged;
			}
		}

		#region Override

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			RefundRejectedDetailsGroupBox.Enabled = Refund.T10_IsRefundRejected;
			if (Refund.T10_RefundRejectedDetails.IsEmpty)
			{
				Refund.T10_RefundRejectedDetails = GlbStaff.CurrentUser.GS_FullName;
			}
			StoreRefundValues(Refund);
		}

		protected override void InitialiseForm()
		{
			InitializeComponent();
			base.InitializeComponent();
		}

		public override string FormHeading
		{
			get { return "Refund Processing"; }
		}

		#endregion

		#region Store/Restore

		void StoreRefundValues(ClientRefund refund)
		{
			T10_IsRefundRejected = refund.T10_IsRefundRejected;
			T10_GS_NKAtFaultUser = refund.T10_GS_NKAtFaultUser;
			T10_RefundAmount = refund.T10_RefundAmount;
			T10_RefundProcessingFee = refund.T10_RefundProcessingFee;
			T10_RefundReason = refund.T10_RefundReason;
			T10_RefundRejectedDetails = refund.T10_RefundRejectedDetails;
			T10_WriteOffAmount = refund.T10_WriteOffAmount;
			T10_AdditionalCharges = refund.T10_AdditionalCharges;
			Remarks = refund.Remarks;
		}

		void RestoreRefundValues(ClientRefund refund)
		{
			refund.T10_IsRefundRejected = T10_IsRefundRejected;
			refund.T10_GS_NKAtFaultUser = T10_GS_NKAtFaultUser;
			refund.T10_RefundAmount = T10_RefundAmount;
			refund.T10_RefundProcessingFee = T10_RefundProcessingFee;
			refund.T10_RefundReason = T10_RefundReason;
			refund.T10_RefundRejectedDetails = T10_RefundRejectedDetails;
			refund.T10_WriteOffAmount = T10_WriteOffAmount;
			refund.T10_AdditionalCharges = T10_AdditionalCharges;
			refund.Remarks = Remarks;
		}

		ZBool T10_IsRefundRejected;
		ZString T10_GS_NKAtFaultUser;
		ZDecimal T10_RefundAmount;
		ZDecimal T10_RefundProcessingFee;
		ZDecimal T10_WriteOffAmount;
		ZDecimal T10_AdditionalCharges;
		ZString T10_RefundReason;
		ZString T10_RefundRejectedDetails;
		ZString Remarks;

		#endregion

		ClientRefund Refund
		{
			get { return (ClientRefund)DataSource; }
		}

		#region Event Handlers

		void T10_IsRefundRejectedInfo_ValueChanged(object sender, EventArgs e)
		{
			RefundRejectedDetailsGroupBox.Enabled = Refund.T10_IsRefundRejected;
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			Refund.RunPreSaveValidation();
			if (Refund.ProcessRefund = !Refund.HasErrors)
			{
				DialogResult = DialogResult.OK;
			}
		}

		void OnCancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			if (Refund.HasChanges)
			{
				RestoreRefundValues(Refund);
			}
		}

		#endregion

		void RefundRejectedRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			zPanelRefundRejected.Visible = true;
			zPanelRefundApproved.Visible = false;
		}

		void RefundApprovedRadioButton_CheckedChanged(object sender, EventArgs e)
		{
			zPanelRefundRejected.Visible = false;
			zPanelRefundApproved.Visible = true;
		}
	}
}
