using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Module;
using Enterprise.Billing.Integration;
using Enterprise.Client.MFI.Data;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.MFI
{
	public class MFIPayableTransactionModuleOverride : APTransactionModuleStrip
	{
		public MFIPayableTransactionModuleOverride()
			: base()
		{
		}

		protected override void AddExtraImportExportMenuItems()
		{
			AddImportDataMenuItem("&CSV EStatement", new EventHandler(ImportEStatementCSVEventHandler));
		}

		protected void ImportEStatementCSVEventHandler(object sender, EventArgs args)
		{
			using (var dialog = new ZOpenFileDialog())
			{
				dialog.Title = "Import CSV Statement";
				dialog.CheckFileExists = true;
				if (ZFormModaliser.ShowCommonDialogWithoutDispose(dialog) == DialogResult.OK)
				{
					ImportEStatement(dialog.ForceLocalFile());
				}
			}
		}

		protected void ImportEStatement(ZString fileName)
		{
			NotificationBuffer notificationBuffer = new NotificationBuffer();
			EStatementImporter flatFileDataImporter = new EStatementImporter();
			flatFileDataImporter.RunExtraValidation = true;
			flatFileDataImporter.ImportData(fileName, notificationBuffer, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.ClientSpecifiedImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, fileName));

			if (!notificationBuffer.HasErrors)
			{
				if (flatFileDataImporter.ImportedInvoice != null)
				{
					DisplayImportedTransactionForm(flatFileDataImporter.ImportedInvoice);
				}
			}
			else
			{
				Globals.Message.ShowError(notificationBuffer.AsString);
			}
		}
	}
}
