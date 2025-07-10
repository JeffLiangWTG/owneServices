using System;

using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class UPECurrentQueueUserControl : CurrentQueueUserControl
	{
		public UPECurrentQueueUserControl()
		{
			InitializeComponent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				bool showEIRRaised = CurrentDataItem is UPEJobDeclaration || CurrentDataItem is UPECusHAWB;
				EIRRaisedDateDateEdit.Visible = showEIRRaised;
				EIRRaisedDateLabel.Visible = showEIRRaised;

				bool showAccountNumber = CurrentDataItem is UPECusHAWB && (((UPECusHAWB)CurrentDataItem).CurrentQueue is UPECalloutQueue);
				AccountNumberTextBox.Visible = showAccountNumber;
				AccountNumberLabel.Visible = showAccountNumber;
				EIRRaisedDateDateEdit.AllowOutsideOfParent();
			}
		}

		protected override void SetBindToPrefixes(IBusiness dataSource)
		{
			base.SetBindToPrefixes(dataSource);

			if (!string.IsNullOrEmpty(BindToPrefix))
			{
				AccountNumberTextBox.BindTo = BindToPrefix + AccountNumberTextBox.BindTo;
				EIRRaisedDateDateEdit.BindTo = BindToPrefix + EIRRaisedDateDateEdit.BindTo;
			}
		}
	}
}
