using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.Client.AUS.Declaration;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.AUS.GUI
{
	public class CommercialInvoiceMenu : EDIMenu
	{
		static EDIMenu NewDelegate()
		{
			return new CommercialInvoiceMenu();
		}

		public static void Initialise()
		{
			OverridableNewDelegate.Value = new ConstructorDelegate(NewDelegate);
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			MenuItem importFlatFileMenuItem = new ZMenuItem(ImportAUSInvoiceFormatText, new EventHandler(ImportFromTextFile));
			dataMenuItem.MenuItems.Add(importFlatFileMenuItem);
		}

		internal const string ImportAUSInvoiceFormatText = "Import Invoices in Austin Format";

		public void ImportFromTextFile(object sender, EventArgs e)
		{
			if (Declaration.HasChanges)
			{
				Globals.Message.ShowError("Please save before importing commercial invoice data", "Please Save Before Importing");
			}
			else if (!Declaration.JE_OH_Importer.IsValid || !Declaration.JE_OH_Supplier.IsValid)
			{
				Globals.Message.ShowError("Please create and save declaration details before importing commercial invoice data", "Please Create Declaration First");
			}
			else
			{
				using (DataImporterForm form = new DataImporterForm(new DataImporterBusinessObject(new BusinessObjectFactory()), "Importing Commercial Invoice Data", BillingInterfaceName.ClientSpecifiedImport))
				{
					form.Importer = new AUSDataImporter(Declaration);
					ZFormModaliser.ShowDialogWithoutDispose(form);
				}
			}
		}

		// expose for test
		internal MenuItem InternalDataMenuItem
		{
			get { return dataMenuItem; }
			set { dataMenuItem = value; }
		}
		internal void InternalSetupTopLevelMenu()
		{
			SetupTopLevelMenu();
		}

		static internal bool IsOverridableNewDelegateNull => OverridableNewDelegate.Value == null;
	}
}
