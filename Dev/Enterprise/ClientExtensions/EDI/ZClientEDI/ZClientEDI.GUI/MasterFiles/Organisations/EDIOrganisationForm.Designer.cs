
namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	partial class EDIOrganisationForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.OrganisationsTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// OrganisationsTabControl
			//
			this.OrganisationsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 592, true);
			//
			// ContactsTabPage
			//
			this.ContactsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 565, true);
			//
			// StmNoteTabPage
			//
			this.StmNoteTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 565, true);
			//
			// DetailsTabPage
			//
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 565, true);
			this.DetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DetailsTabPage_InitializeTab));
			//
			// AddressesTabPage
			//
			this.AddressesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 565, true);
			//
			// ButtonsUserControl
			//
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(692, 617, true);
			//
			// MainStatusBar
			//
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 646, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 24, true);
			//
			// EDIOrganisationForm
			//
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 670, true);
			this.Name = "EDIOrganisationForm";
			this.OrganisationsTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.TransportTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.TransportTabPage_InitializeTab));
			this.ConsigneeTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ConsigneeTabPage_InitializeTab));
			this.ReceivablesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ReceivablesTabPage_InitializeTab));
		}

		void ReceivablesTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.ReceivablesTabPage.SuspendLayout();
			this.ReceivablesTabPage.Controls.SetChildIndex(this.ReceivablesControl, 0);
			this.ReceivablesTabPage.ResumeLayout(true);
		}

		void ConsigneeTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.ConsigneeTabPage.SuspendLayout();
			this.ConsigneeTabPage.Controls.SetChildIndex(this.ConsigneeControl, 0);
			this.ConsigneeTabPage.ResumeLayout(true);
		}

		void TransportTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.TransportTabPage.SuspendLayout();
			this.TransportTabPage.Controls.SetChildIndex(this.TransportControl, 0);
			this.TransportTabPage.ResumeLayout(true);
		}

		void DetailsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.DetailsTabPage.SuspendLayout();
			//
			// DetailsControl
			//
			this.DetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 565);
			this.DetailsTabPage.ResumeLayout(true);
		}

		#endregion
	}
}
