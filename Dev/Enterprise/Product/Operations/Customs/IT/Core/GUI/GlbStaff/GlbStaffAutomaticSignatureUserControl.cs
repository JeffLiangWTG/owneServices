using System;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI;

public partial class GlbStaffAutomaticSignatureUserControl : ZUserControl
{
	public GlbStaffAutomaticSignatureUserControl()
	{
		InitializeComponent();
	}

	GlbStaffWrapper StaffWrapper => (GlbStaffWrapper)CurrentDataItem;

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);

		AddAutomaticSignatureButton.Click += AddAutomaticSignatureButton_Click;
		ClearAutomaticSignatureButton.Click += ClearAutomaticSignatureButton_Click;
		ToggleAutomaticSignatureButtonsStatus();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			AddAutomaticSignatureButton.Click -= AddAutomaticSignatureButton_Click;
			ClearAutomaticSignatureButton.Click -= ClearAutomaticSignatureButton_Click;
		}
		base.Dispose(disposing);
	}

	void ToggleAutomaticSignatureButtonsStatus()
	{
		var isSignatureEntered = StaffWrapper.AutomaticSignaturePasswordCollection.Count > 0;
		AddAutomaticSignatureButton.Enabled = !isSignatureEntered;
		ClearAutomaticSignatureButton.Enabled = isSignatureEntered;
	}

	void ClearAutomaticSignatureButton_Click(object sender, EventArgs e)
	{
		StaffWrapper.AutomaticSignaturePasswordCollection.RemoveAndDeleteAll();
		ToggleAutomaticSignatureButtonsStatus();
	}

	void AddAutomaticSignatureButton_Click(object sender, EventArgs e)
	{
		StaffWrapper.AutomaticSignaturePasswordCollection.AddNew();
		ToggleAutomaticSignatureButtonsStatus();
	}
}
