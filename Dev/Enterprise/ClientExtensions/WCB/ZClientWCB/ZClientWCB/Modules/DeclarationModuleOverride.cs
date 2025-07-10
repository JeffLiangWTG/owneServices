using System;
using System.Windows.Forms;
using Enterprise.Billing.Integration;
using Enterprise.Client.WCB.DaimlerChrysler;
using Enterprise.Client.WCB.DaimlerChrysler.GUI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Module;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.WCB.Module
{
	public class DeclarationModuleOverride : JobDeclarationModule
	{
		public DeclarationModuleOverride()
		{
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("From Mercedes File", new EventHandler(HandleDaimlerFileImport));
			AddImportDataMenuItem("From Chrysler File", new EventHandler(HandleChryslerFileImport));
		}

		void HandleDaimlerFileImport(object sender, EventArgs e)
		{
			ShowImportForm(true);
		}

		void HandleChryslerFileImport(object sender, EventArgs e)
		{
			ShowImportForm(false);
		}

		void ShowImportForm(bool isMercedes)
		{
			using (DCDataImporterForm form = DCDataImporterForm.Create(BillingInterfaceName.ClientSpecifiedImport))
			{
				form.Importer = new DCFlatFileDataImporter(isMercedes, IsOkToImportData);
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		bool IsOkToImportData(JobDeclarationCollection fixedJobDecs, BaseJobDeclarationCollection jobDecs)
		{
			return ZFormModaliser.ShowDialogAndDispose(
				new DCImportDataEditorForm((JobDeclarationFixedCollection)fixedJobDecs)) == DialogResult.OK;
		}
	}
}
