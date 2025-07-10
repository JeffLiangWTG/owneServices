using System;
using System.Windows.Forms;
using Enterprise.Billing.Integration;
using Enterprise.Client.Rohlig.HarleyDavidson;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.DataTransfer.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.Rohlig.GUI
{
	public class ROHMenuItem : EDIMenu
	{
		public static EDIMenu NewDelegate()
		{
			return new ROHMenuItem();
		}

		public static void Initialise()
		{
			OverridableNewDelegate.Value = new ConstructorDelegate(NewDelegate);
		}

		protected internal void SetupTopLevelMenuForTest() => SetupTopLevelMenu();

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();

			MenuItem harleyDavidsonMenuItem = new ZMenuItem(HarleyDavidsonImportMenuItemText, new EventHandler(ImportHarleyDavidsonCommercialInvoices));
			dataMenuItem.MenuItems.Add(harleyDavidsonMenuItem);
		}

		void ImportHarleyDavidsonCommercialInvoices(object sender, EventArgs e)
		{
			using (DataImporterForm form = DataImporterForm.Create(BillingInterfaceName.ClientSpecifiedImport))
			{
				form.Importer = new HarleyDavidsonDataImporter(Declaration);
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		internal const string HarleyDavidsonImportMenuItemText = "Import Invoices (Harley Davidson)";
		internal MenuItem InternaldataMenuItem => dataMenuItem;
	}
}
