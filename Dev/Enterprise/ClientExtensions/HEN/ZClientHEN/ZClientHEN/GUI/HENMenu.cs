using System;
using System.Windows.Forms;
using Enterprise.Billing.Integration;
using Enterprise.Client.HEN.Nissan;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.DataTransfer.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.HEN.GUI
{
	public class HENMenu : EDIMenu
	{
		static EDIMenu NewDelegate()
		{
			return new HENMenu();
		}

		public static void Initialise()
		{
			OverridableNewDelegate.Value = NewDelegate;
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();

			MenuItem harleyDavidsonMenuItem = new ZMenuItem(NissanImportMenuItemText, new EventHandler(ImportNissanCommercialInvoices));
			dataMenuItem.MenuItems.Add(harleyDavidsonMenuItem);
		}

		void ImportNissanCommercialInvoices(object sender, EventArgs e)
		{
			using (DataImporterForm form = DataImporterForm.Create(BillingInterfaceName.ClientSpecifiedImport))
			{
				form.Importer = new NissanInvoiceDataImporter(Declaration);
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		internal const string NissanImportMenuItemText = "Import Nissan Invoice Data";

		internal MenuItem InternalDataMenuItem => dataMenuItem;
		internal void InternalSetupTopLevelMenu() => SetupTopLevelMenu();
	}
}
