using System;
using Enterprise.Billing.Integration;
using Enterprise.Client.MFI.CaroTrans;
using Enterprise.DataTransfer.GUI;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client
{
	public class MFIOrderModuleOverride : OrdersModule
	{
		public MFIOrderModuleOverride()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("From CaroTrans File", new EventHandler(OnCTIFileImport));
		}

		void OnCTIFileImport(object sender, EventArgs e)
		{
			using (DataImporterForm form = DataImporterForm.Create(BillingInterfaceName.ClientSpecifiedImport))
			{
				form.Importer = new CaroTransOrderDataImporter();
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}
	}
}
