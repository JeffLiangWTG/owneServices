using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.YAS.GUI
{
	public class YASMenu : EDIMenu
	{
		static EDIMenu NewDelegate()
		{
			return new YASMenu();
		}

		public static void Initialise()
		{
			OverridableNewDelegate.Value = new ConstructorDelegate(NewDelegate);
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Australia)
			{
				MenuItem importYamahaInvoicesMenuItem = new ZMenuItem("Import Yamaha Invoices", new EventHandler(ImportYamahaFile));
				dataMenuItem.MenuItems.Add(importYamahaInvoicesMenuItem);
			}
		}

		public void ImportYamahaFile(object sender, EventArgs e)
		{
			using (DataImporterForm form = new DataImporterForm(new DataImporterBusinessObject(new BusinessObjectFactory()), "Import Yamaha Invoices", BillingInterfaceName.ClientSpecifiedImport))
			{
				form.Importer = new YASInvoiceImporter.YASFlatFileDataImporter(Declaration);
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}
	}
}
