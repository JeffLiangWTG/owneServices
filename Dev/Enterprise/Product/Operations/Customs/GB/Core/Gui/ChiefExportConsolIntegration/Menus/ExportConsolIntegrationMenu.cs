using CargoWise.Types;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.GB.GUI.ChiefExportConsolIntegration
{
	public class ExportConsolIntegrationMenu : EDIMenu
	{
		readonly ForwardingConsol consol;
		readonly CustomsExportConsolIntegrationWrapper wrapper;

		public ExportConsolIntegrationMenu(CustomsExportConsolIntegrationWrapper wrapper)
		{
			this.wrapper = wrapper;
			consol = wrapper?.ForwardingConsol;

			Text = "Customs && CCS-UK";
		}

		public override void RefreshMenu()
		{
			WipeAll();

			Enabled = (wrapper?.IsChiefCcsukEnabled ?? false) && !(consol?.JK_MasterBillNum ?? ZString.Empty).IsEmpty;
			if (Enabled)
			{
				var chiefMenu = new ChiefExportConsolIntegrationMenu(wrapper);
				var ccsukMenu = new CCSUKExportConsolIntegrationMenu(wrapper);
				var cdsMenu = new CDSExportConsolIntegrationMenu(wrapper);
				MenuItems.Add(chiefMenu);
				MenuItems.Add(ccsukMenu);
				MenuItems.Add(cdsMenu);
			}
		}

		void WipeAll()
		{
			MenuItems.Clear();
		}
	}
}
