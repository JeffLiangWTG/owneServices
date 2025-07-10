using System;
using System.Linq;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public class CommercialInvoiceEDIMenu : Customs.GUI.CommercialInvoiceEDIMenu
	{
		public CommercialInvoiceEDIMenu()
		{
			importNfeMenuItem = new ZMenuItem(ResString.GetMultilingualString("8772E0D5-C3F6-4C04-B67E-56C5AB157FDB", "Import NF-e"), ImportNfeMenuItem_Click);
			MenuItems.Add(importNfeMenuItem);
		}

		readonly ZMenuItem importNfeMenuItem;

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			importNfeMenuItem.Visible = InvoiceHeader?.IsExport ?? false;
		}

		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		public JobComInvoiceHeader InvoiceHeader => Declaration?.Invoices.FirstOrDefault() as JobComInvoiceHeader;

		#region MenuItems

		protected void ImportNfeMenuItem_Click(object sender, EventArgs e)
		{
			var invoice = InvoiceHeader;
			if (invoice != null && invoice.IsExport && CustomsPlugIn.FormPreSaved(invoice, Form))
			{
				MenuHelper.ShowNFEImportForm(Declaration);
			}
		}

		#endregion
	}
}
