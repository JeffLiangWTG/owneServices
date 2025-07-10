using System;
using Enterprise.Client.CLE.OrdersDataImport;
using Enterprise.DataTransfer.Business;
using Enterprise.Freight.Forwarding.Orders.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.CLE.Modules
{
	public class CLEOrdersModule : OrdersModule
	{
		public CLEOrdersModule()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("CLE Orders", new EventHandler(OnImportOrders));
		}

		void OnImportOrders(object sender, EventArgs e)
		{
			using (CLEDataImporterForm form = new CLEDataImporterForm(new DataImporterBusinessObject(Factory), null))
			{
				form.Importer = new CLEOrderDataImporter();
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}
	}
}
