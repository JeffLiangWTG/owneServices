using System;
using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Client.UPE.GUI
{
	[SuppressBindingMemberBashingTest] // for ShipperMatchingErrorCheckBox
	public partial class UPSUserControl : BaseCustomsEntryUserControl
	{
		public UPSUserControl()
		{
			InitializeComponent();
			MissingResourceStringChecker.ExcludeFromTest(zLabelControlNumber);
		}

		public override BaseJobDeclaration JobDeclaration
		{
			get { return base.JobDeclaration; }
			set
			{
				base.JobDeclaration = value;
				if (UPEJobDeclaration != null && UPEJobDeclaration.CurrentQueue.P4_CustomFlag3)
				{
					ShipperMatchingErrorCheckBox.CheckState = CheckState.Checked;
					ShipperMatchingErrorCheckBox.Enabled = false;
				}
			}
		}

		void ProcessRefundButton_Click(object sender, EventArgs e)
		{
			if (UPEJobDeclaration.Refund != null && !UPEJobDeclaration.IsRefundProcessed)
			{
				if (DialogResult.OK == ZFormModaliser.ShowDialogAndDispose(new RefundProcessingForm(UPEJobDeclaration.Refund)))
				{
					UPEJobDeclaration.HasChanges = true;
				}
			}
		}

		UPEJobDeclaration UPEJobDeclaration
		{
			get { return (UPEJobDeclaration)JobDeclaration; }
		}

		protected virtual void ShipperMatchingError_Clicked(object sender, EventArgs e)
		{
			if (ConfirmShipperMatchingError())
			{
				UPEJobDeclaration.ShipperMatchingError();
				ShipperMatchingErrorCheckBox.CheckState = CheckState.Checked;
				ShipperMatchingErrorCheckBox.Enabled = false;
			}
			else
			{
				ShipperMatchingErrorCheckBox.CheckState = CheckState.Unchecked;
			}
		}

		bool ConfirmShipperMatchingError()
		{
			string confirmationMessage = "Are you sure you want to flag shipment?";
			return Globals.Message.Show(confirmationMessage, "Shipper Matching Error", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
		}
	}
}
