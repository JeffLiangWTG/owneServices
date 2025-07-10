using System;
using Enterprise.Billing.Integration;
using Enterprise.Client.MFI.CaroTrans.Export;
using Enterprise.Client.MFI.Data;
using Enterprise.Client.MFI.GUI;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.MFI
{
	public class MFIConsolModuleOverride : JobConsolModule
	{
		public MFIConsolModuleOverride()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("From Carotrans File", new EventHandler(CTIFileImport));
			AddExportDataMenuItem("To Carotrans File", new EventHandler(CTIFileExport));
		}

		void CTIFileImport(object sender, EventArgs e)
		{
			using (MFIDataImporterForm form = MFIDataImporterForm.Create(BillingInterfaceName.ClientSpecifiedImport))
			{
				form.Importer = new CTIDataImporter();
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		void CTIFileExport(object sender, EventArgs e)
		{
			CaroTransFlatFileDataExporter exporter = new CaroTransFlatFileDataExporter(Factory);
			ShowExportForm(exporter);
		}

		void ShowExportForm(FlatFileDataExporter exporter)
		{
			ZFormModaliser.ShowDialogAndDispose(new DataExportForm(exporter, CollectionForExport));
		}
	}
}
