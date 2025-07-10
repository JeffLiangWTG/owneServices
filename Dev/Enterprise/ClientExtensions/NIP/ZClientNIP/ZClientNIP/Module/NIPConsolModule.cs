#if DEBUG
using System;
using Enterprise.Billing.Integration;
using Enterprise.Client.NIP.Business.ConsolAndShipmentImport;
using Enterprise.DataTransfer.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.NIP.Module
{
	public class NIPJobConsolModule : JobConsolModule
	{
		public NIPJobConsolModule()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("Import Consol/Shipment <DEBUG ONLY>", new EventHandler(OnImportConsolAndShipment));
		}

		void OnImportConsolAndShipment(object sender, EventArgs e)
		{
			using (DataImporterForm form = DataImporterForm.Create(BillingInterfaceName.ClientSpecifiedImport))
			{
				form.Importer = new NIPConsolAndShipmentDataImporter();
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}
	}
}
#endif
