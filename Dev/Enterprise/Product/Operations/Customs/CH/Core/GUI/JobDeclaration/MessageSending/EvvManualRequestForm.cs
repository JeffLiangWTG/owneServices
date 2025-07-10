using System;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class EvvManualRequestForm : ZChildForm
{
	public EvvManualRequestForm(EvvRequestSendingObjectParent sendingObjectParent) : base(sendingObjectParent)
	{
		InitializeComponent();
	}

	public new EvvRequestSendingObjectParent BusinessEntity => (EvvRequestSendingObjectParent)base.BusinessEntity;

	public override string FormHeading => Res.GetString("7061532D-7776-41CA-B1F7-E052900D9F27", "EVV Documents Manual Request");

	#region Event Handlers

	void SendButton_Click(object sender, EventArgs e)
	{
		DialogResult = System.Windows.Forms.DialogResult.OK;
		Close();
	}

	internal void ChangeSendButtonAvailability()
	{
		if (BusinessEntity != null)
		{
			SendButton.Enabled = !BusinessEntity.HasMessageErrors
				&& (VatCheckBox.Checked || CustomsDutiesCheckBox.Checked || ReimbursementCustomsDutiesCheckBox.Checked || ReimbursementVatCheckBox.Checked);
		}
	}

	void VersionTextBox_TextChanged(object sender, EventArgs e)
	{
		ChangeSendButtonAvailability();
	}

	void CustomsDutiesCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		ChangeSendButtonAvailability();
	}

	void VatCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		ChangeSendButtonAvailability();
	}

	void ReimbursementCustomsDutiesCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		ChangeSendButtonAvailability();
	}

	void ReimbursementVatCheckBox_CheckedChanged(object sender, EventArgs e)
	{
		ChangeSendButtonAvailability();
	}

	#endregion
}
