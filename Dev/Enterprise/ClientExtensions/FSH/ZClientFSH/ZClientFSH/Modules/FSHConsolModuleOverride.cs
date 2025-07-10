using System;
using Enterprise.Billing.Integration;
using Enterprise.Client.FSH.TsManifest;
using Enterprise.DataTransfer.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.FSH
{
	public class FSHConsolModuleOverride : JobConsolModule
	{
		public FSHConsolModuleOverride()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("From TsManifest File", new EventHandler(TsFileImport));
		}

		void TsFileImport(object sender, EventArgs e)
		{
			using (DataImporterForm form = DataImporterForm.Create(BillingInterfaceName.ClientSpecifiedImport))
			{
				form.Importer = new FortuneShippingFlatFileDataImporter();
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}
	}
}
