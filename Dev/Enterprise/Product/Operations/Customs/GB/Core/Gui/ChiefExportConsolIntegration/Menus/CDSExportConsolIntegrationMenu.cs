using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.CDS.Messaging;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public class CDSExportConsolIntegrationMenu : EDIMenu
	{
		readonly CustomsExportConsolIntegrationWrapper wrapper;
		readonly ZMenuItem eaaMenu;
		readonly ZMenuItem ealMenu;
		readonly ZMenuItem edlMenu;

		public CDSExportConsolIntegrationMenu(CustomsExportConsolIntegrationWrapper wrapper)
		{
			this.wrapper = Argument.NotNull(wrapper, nameof(wrapper));
			Text = "CDS";

			eaaMenu = new ZMenuItem("Anticipate Goods' Arrival", SendCDSAnticipateGoodsArrival);
			ealMenu = new ZMenuItem("Arrive Goods", SendCDSArriveGoods);
			edlMenu = new ZMenuItem("Depart Goods", SendCDSDepartGoods);

			MenuItems.Clear();
			MenuItems.AddRange(new MenuItem[]
			{
				new ZMenuItem("Close Master", SendCDSCloseMaster),
				new ZMenuItem("Query Master", SendCDSQueryMaster),
				eaaMenu,
				ealMenu,
				edlMenu
			});
		}

		public override void RefreshMenu()
		{
			var ccsukIsEnabled = wrapper.IsChiefCcsukEnabled
								&& !(wrapper.ForwardingConsol?.JK_MasterBillNum ?? ZString.Empty).IsEmpty;
			var isDep = wrapper.CredentialIsDEP || wrapper.CredentialIsLoader;
			var isDEPOrLoaderAllowedbyRegistry = GBCustomsDataRegistry.Instance.CcsukShowDEPProfiles.Value;
			eaaMenu.Enabled = ccsukIsEnabled && isDep && isDEPOrLoaderAllowedbyRegistry;
			ealMenu.Enabled = ccsukIsEnabled && isDep && isDEPOrLoaderAllowedbyRegistry;
			edlMenu.Enabled = ccsukIsEnabled && isDep && isDEPOrLoaderAllowedbyRegistry;
		}

		void SendToCDS(GbDes242MessageFunction how)
		{
			if (wrapper.IsCDSFunctionalityEnabled)
			{
				var gateway = wrapper.GetCredentialForPima()?.CSP ?? ZString.Empty;
				var provider = GBCustomsRequestFactory.GetProviderType(gateway);
				if (provider == ProviderType.Direct && (wrapper.ForwardingConsol.SendingForwarder == null || wrapper.ForwardingConsol.SendingForwarder.GetEuIdentificationNumber().IsEmpty))
				{
					Globals.Message.Show("Messaging directly to CDS is not possible when the Sending Forwarder is missing or when this organisation lacks an EORI.");
				}
				else
				{
					var cdsConsolMessageSender = new CDSConsolMessageSender(wrapper);
					cdsConsolMessageSender.SendToRecipient(wrapper, new SendsMessagesToCustomsGUI(), how, false);
					wrapper.MawbExportHelper.Messages.Load();
					wrapper.ForwardingConsol.Messages.RefreshBinding();
				}
			}
			else
			{
				Globals.Message.Show(GUI.CDSEDIMenu.SendToCDSNotEnabled);
			}
		}

		void SendCDSCloseMaster(object sender, EventArgs args)
		{
			SendToCDS(new GbDes242MessageFunction.MucrClose());
		}

		void SendCDSQueryMaster(object sender, EventArgs args)
		{
			SendToCDS(new GbDes242MessageFunction.QueryMasterDEC());
		}

		void SendCDSAnticipateGoodsArrival(object sender, EventArgs args)
		{
			SendToCDS(new GbInventoryManagementMessageFunction.ArrivalAnticipated(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master));
		}

		void SendCDSArriveGoods(object sender, EventArgs args)
		{
			SendToCDS(new GbInventoryManagementMessageFunction.ArrivalActual(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master));
		}

		void SendCDSDepartGoods(object sender, EventArgs args)
		{
			SendToCDS(new GbInventoryManagementMessageFunction.Departure(GbInventoryManagementMessageFunction.MasterOrDeclaration.Master));
		}
	}
}
