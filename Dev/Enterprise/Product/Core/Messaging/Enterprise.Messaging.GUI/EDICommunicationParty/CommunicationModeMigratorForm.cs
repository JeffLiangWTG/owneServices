using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	public partial class CommunicationModeMigratorForm : ZChildForm
	{
		public override string FormCaption
		{
			get
			{
				return Res.GetString("385E29FE-E6F6-4AC1-8C33-B04F0B913893", $@"Run Update Organizations EDI Client");
			}
		}

		public CommunicationModeMigratorForm(BusinessObjectFactory factory, CommunicationModeMigratorDataModel migrator) : base(migrator)
		{
			userDataInput = migrator;
			InitializeComponent();
		}
		internal readonly CommunicationModeMigratorDataModel userDataInput;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log string")]
		public void SendAndCloseButton_Click(object sender, EventArgs e)
		{
			var party = userDataInput.Factory.Load<EDICommunicationParty>(userDataInput.ECP_PK);
			if (party != null && (party.ECP_IsActive == false || party.CurrentOutboundConfig == null || party.CurrentOutboundConfig.ECC_IsActive == false))
			{
				Globals.Message.ShowError(Res.GetString("C2704F23-4778-4862-8D1D-FC43FD6DAB1B", "You should select an active Outbound config"), "Error");
			} else
			{
				var organizations = CommunicationModeMigrator.OrganizationCount(userDataInput.Factory);
				if (Globals.Message.ShowConfirmation(Res.GetString("D7148FFF-5589-4114-8710-D59962C2362B", "You are about to update {0} organizations to point to this config", organizations), "Confirmation", Res.GetString("EF3C3735-89BC-438A-B5EB-9DAEE8136606", "yes"), MessageBoxIcon.Asterisk) == DialogResult.OK)
				{
					runnerTabControl.SelectTab(progressTabPage);
					if (!CommunicationModeMigrator.Run(progressControl, party?.CurrentOutboundConfig?.PK, userDataInput.Factory))
					{
						okButton.Text = Res.GetString("1CE5C628-A468-448E-A074-AFF858F45854", "Run Again");
					}
					else if (userDataInput.CloseOnCompletion)
					{
						Close();
					}
					else if (okButton.Text == Res.GetString("1CE5C628-A468-448E-A074-AFF858F45854", "Run Again"))
					{
						okButton.Text = Res.GetString("AFDA5DC7-AFC3-491B-BA89-AB1976F103CA", "OK");
					}
				}
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}
	}
}
