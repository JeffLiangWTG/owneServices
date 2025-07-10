using System;
using Enterprise.Customs.AU.Module.AirCargo;

namespace Enterprise.Client.TNT.AirCargo
{
	public class TNTAirCargoModuleOverride : AUCustomsAirCargoModule
	{
		public TNTAirCargoModuleOverride()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("I&QDown", new EventHandler(OnIQDownImport_Click));
			AddImportDataMenuItem("&XXX", new EventHandler(OnXXXImport_Click));
		}

		void OnIQDownImport_Click(object sender, EventArgs e)
		{
			IQDownAirCargoFileImporter importer = new IQDownAirCargoFileImporter(EmbeddedControl.FindForm());
			importer.Import();
		}

		void OnXXXImport_Click(object sender, EventArgs e)
		{
			XXXAirCargoFileImporter importer = new XXXAirCargoFileImporter(EmbeddedControl.FindForm());
			importer.Import();
		}
	}
}
