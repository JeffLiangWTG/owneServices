using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.GUI;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI.PlugIns
{
	class RNSMFMenu : MessageManagementMenu
	{
		public RNSMFMenu(IRNSPlugInSupport plugInSupport)
			: base(plugInSupport.GetRNSMultiMessageManager())
		{
			Argument.NotNull(plugInSupport, "plugInSupport");
			this.master = plugInSupport.Master;

			this.Text = "RNS/MF";
		}

		void AttachAndCreateShipments_Click(object sender, EventArgs e)
		{
			if (master.HasChanges)
			{
				Globals.Message.ShowError(Res.GetString("7d6aac45-2d2b-44fb-b3bc-db61bd5f90d5", "Please save {0} before attach/create shipments from Forwarded Manifests.",
					master.HumanReadableName));

				return;
			}

			var support = new LoadListForwardManifestSupport(this.master as CFSLoadListConsol);
			if (support.UnlinkedAndExistingShipmentMessages.Length == 0
				&& support.UnlinkedAndNoShipmentMessages.Length == 0)
			{
				if (support.LinkedInconsistentMesasges.Length == 0)
				{
					Globals.Message.ShowInformation(Res.GetString("2733ef99-82da-435e-8ddc-8fbe323d4739", "There are no new matching Forwarded Manifests."));
				}
				else
				{
					string resultMessage = LoadListForwardManifestSupport.AttachOrCreateShipmentsFromForwardedManifests(support);

					Globals.Message.ShowInformation(resultMessage,
						Res.GetString("224f7c88-7a03-4927-be42-ed95a71aaccf", "Attach/Create Shipments from Forwarded Manifests"));
				}
			}
			else
			{
				var dialogResult = Globals.Message.Show(
					Res.GetString("d656a6ec-7cbb-4fe1-824e-fc8119854c32", @"Matching Forwarded Manifests founded:
    {0} new shipment(s) will be created.
    {1} new House Bill Manifest(s) will be linked to existing shipments.
Do you want to continue?", support.UnlinkedAndNoShipmentMessages.Length, support.UnlinkedAndExistingShipmentMessages.Length),
					Res.GetString("7d0994e2-468b-4da0-b4b9-965fe83db354", "Attach/Create Shipments from Forwarded Manifests"),
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Question);

				if (dialogResult == DialogResult.Yes)
				{
					string resultMessage = LoadListForwardManifestSupport.AttachOrCreateShipmentsFromForwardedManifests(support);

					Globals.Message.ShowInformation(resultMessage,
						Res.GetString("224f7c88-7a03-4927-be42-ed95a71aaccf", "Attach/Create Shipments from Forwarded Manifests"));
				}
			}
		}

		#region MessageManagementMenu

		protected override void InitializeMenu()
		{
			base.InitializeMenu();
			if (master != null)
			{
				SetupMenuItems(master);
			}
		}

		void SetupMenuItems(BusinessObject topLevelObject)
		{
			sendMessages.Visible = true;
			withdrawMessages.Visible = false;
			resetToOriginal.Visible = false;

			sendMessages.Text = ResString.GetMultilingualString("71050798-e72c-40d1-9288-a814a82beca5", "Send RNS Status Request");

			if (this.master is CFSLoadListConsol)
			{
				MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("59a9f078-aeae-4d82-be47-ec6c19acd50f", "Attach/Create Shipments from Forwarded Manifests"), AttachAndCreateShipments_Click));
			}
			else if (this.master is TallyContainer)
			{
				sendArrivalCertificationMessages = new ZMenuItem(ResString.GetMultilingualString("9a4499c0-0579-494a-82d9-bdbcc32ba71d", "Send Arrival Certification for Arrived Cargo"), SendMessages_Click);
				MenuItems.Add(sendArrivalCertificationMessages);
			}
		}

		MenuItem sendArrivalCertificationMessages;

		protected override ISendsMessagesToCustoms Sender
		{
			get { return new RNSSendsMessagesToCustomsGUI(this.Manager.RNSRequestParent); }
		}

		protected override bool SendMessagesClickCore(object sender)
		{
			this.Manager.RNSRequestParent.Notification = new MessageInstructionUserNotification();

			if (sender == sendMessages)
			{
				this.Manager.RNSRequestParent.IsStatusQuery = true;
			}
			else if (sender == sendArrivalCertificationMessages)
			{
				this.Manager.RNSRequestParent.IsStatusQuery = false;
			}

			return base.SendMessagesClickCore(sender);
		}
		#endregion

		#region Implementation

		readonly BusinessObject master;

		RNSMultiMessageManager Manager
		{
			get { return (RNSMultiMessageManager)base.manager; }
		}

		#endregion
	}
}
