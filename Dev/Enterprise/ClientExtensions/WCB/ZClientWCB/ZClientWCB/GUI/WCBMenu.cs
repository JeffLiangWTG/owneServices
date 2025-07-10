using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.WCB.GUI
{
	internal class WCBMenu : EDIMenu
	{
		internal static EDIMenu NewDelegate()
		{
			return new WCBMenu();
		}

		public static void Initialise()
		{
			OverridableNewDelegate.Value = new ConstructorDelegate(NewDelegate);
		}
		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			MenuItem importFlatFileMenuItem = new ZMenuItem(ImportWCBInvoiceFormatText, new EventHandler(ImportFromTextFile));
			dataMenuItem.MenuItems.Add(importFlatFileMenuItem);
		}
		static internal bool IsOverridableNewDelegateNull => OverridableNewDelegate.Value == null;
		protected internal void SetupTopLevelMenuInternal() => SetupTopLevelMenu();
		protected internal MenuItem dataMenuItemInternal => base.dataMenuItem;
		internal const string ImportWCBInvoiceFormatText = "Import WCB Invoice Format";

		internal void ImportFromTextFile(object sender, EventArgs e)
		{
			using (WCBDataImporterForm form = new WCBDataImporterForm(new DataImporterBusinessObject(new BusinessObjectFactory()), "Importing Files"))
			{
				form.Importer = new WCBDataImporter(Declaration);
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}
			}
}
