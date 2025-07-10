using System;
using Enterprise.Billing.Integration;
using Enterprise.Freight.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.CLE.Modules
{
	public class CLEContainerModule : ContainersModule
	{
		public CLEContainerModule()
			: base()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("&GMC Dehire from CSV", new EventHandler(OnImportContainerDates));
		}

		void OnImportContainerDates(object sender, EventArgs e)
		{
			using (CLEDataImporterForm form = CLEDataImporterForm.Create(BillingInterfaceName.ClientSpecifiedImport))
			{
				form.Importer = new ContainerDatesImporter();
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}
	}
}
