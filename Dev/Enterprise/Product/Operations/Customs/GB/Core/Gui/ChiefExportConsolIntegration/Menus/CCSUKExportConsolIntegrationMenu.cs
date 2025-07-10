using System;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	public class CCSUKExportConsolIntegrationMenu : EDIMenu
	{
		readonly CustomsExportConsolIntegrationWrapper wrapper;
		readonly ZMenuItem sendGood2GOMenu;

		public CCSUKExportConsolIntegrationMenu(CustomsExportConsolIntegrationWrapper wrapper)
		{
			this.wrapper = Argument.NotNull(wrapper, nameof(wrapper));
			Text = "CCS-UK";

			sendGood2GOMenu = new ZMenuItem("Send 'Good to Go' message", SendGood2Go);

			MenuItems.Clear();
			MenuItems.AddRange(new MenuItem[]
			{
				new ZMenuItem("Query Master with FSR", SendCcsukExportFsr),
				new ZMenuItem("Query Master with FSR (omit shed)", SendCcsukExportFsrNoShed),
				sendGood2GOMenu
			});
		}

		public override void RefreshMenu()
		{
			sendGood2GOMenu.Enabled = GBCustomsDataRegistry.Instance.ChiefFallbackExports.Value;
		}

		void SendGood2Go(object sender, EventArgs args)
		{
			SendToCcsuk(new CcsukTransmissionMessageFunction.CUKG2G());
		}

		void SendCcsukExportFsr(object sender, EventArgs args)
		{
			SendToCcsuk(new CcsukTransmissionMessageFunction.CUKFSR.FsaForExport());
		}

		void SendCcsukExportFsrNoShed(object sender, EventArgs args)
		{
			SendToCcsuk(new CcsukTransmissionMessageFunction.CUKFSR.FsaForExportWithoutShed());
		}

		void SendToCcsuk(CcsukTransmissionMessageFunction function)
		{
			var consolSender = new CcsukConsolMessageSender();
			consolSender.SendToCCSUK(wrapper, new SendsMessagesToCustomsGUI(), function);
			wrapper.MawbExportHelper.Messages.Load();
			wrapper.ForwardingConsol.Messages.RefreshBinding();
		}
	}
}
