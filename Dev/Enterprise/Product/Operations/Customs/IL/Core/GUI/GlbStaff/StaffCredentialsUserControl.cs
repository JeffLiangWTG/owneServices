using System;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.GUI
{
	public partial class StaffCredentialsUserControl : MasterFiles.GUI.StaffCredentialsUserControl
	{
		public StaffCredentialsUserControl()
		{
			InitializeComponent();
		}

		GlbStaffWrapper StaffWrapper => (GlbStaffWrapper)CurrentDataItem;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			AddSignatureButton.Click += AddSignatureButton_Click;
			ClearSignatureButton.Click += ClearSignatureButton_Click;
			ToggleSignatureButtonsStatus();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				AddSignatureButton.Click -= AddSignatureButton_Click;
				ClearSignatureButton.Click -= ClearSignatureButton_Click;
			}
			base.Dispose(disposing);
		}

		void ToggleSignatureButtonsStatus()
		{
			var isSignatureEntered = StaffWrapper.PasswordCollection.Count > 0;
			AddSignatureButton.Enabled = !isSignatureEntered;
			ClearSignatureButton.Enabled = isSignatureEntered;
		}

		void ClearSignatureButton_Click(object sender, EventArgs e)
		{
			StaffWrapper.PasswordCollection.RemoveAndDeleteAll();
			ToggleSignatureButtonsStatus();
		}

		void AddSignatureButton_Click(object sender, EventArgs e)
		{
			StaffWrapper.PasswordCollection.AddNew();
			StaffWrapper.HasChanges = true;
			ToggleSignatureButtonsStatus();
		}
	}
}
