using System;

namespace Enterprise.Client.ZClientCCP.Module
{
	public class AUCommercialInvoiceModuleOverride : Customs.AU.Module.CommercialInvoiceModule
	{
		public AUCommercialInvoiceModuleOverride() { }

		protected override void AddInterfaceConnectorMenuItems()
		{
			base.AddInterfaceConnectorMenuItems();
			AddImportDataMenuItem("Kawasaki Invoices", new EventHandler(ImportKawasakiInvoices_OnClick));
		}

		protected void ImportKawasakiInvoices_OnClick(object sender, EventArgs e)
		{
			KawasakiImportController kawasakiController = new KawasakiImportController();
			kawasakiController.ShowDialog();
			kawasakiController.SaveInvoices();
		}
	}

	public class SGCommercialInvoiceModuleOverride : Customs.SG.V4.Module.CommercialInvoiceModule
	{
		public SGCommercialInvoiceModuleOverride() { }
	}

	public class USCommercialInvoiceModuleOverride : Customs.US.Module.CommercialInvoiceModule
	{
		public USCommercialInvoiceModuleOverride() { }
	}
}
