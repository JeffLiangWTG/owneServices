using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.MX.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		public void TestGetNewInvoiceLineUserControl()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				Application.DoEvents();
				AssertType<ImportInvoiceLineUserControl>(form.InvoiceLineUserControl);

				form.MainTabControl.SelectedTab = form.HeaderTabPage;
				Application.DoEvents();
				invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				Application.DoEvents();
				AssertType<ExportInvoiceLineUserControl>(form.InvoiceLineUserControl);

				form.MainTabControl.SelectedTab = form.HeaderTabPage;
				Application.DoEvents();
				invoice.JZ_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				Application.DoEvents();
				AssertType<BaseInvoiceLineUserControl>(form.InvoiceLineUserControl);
			}
		}

		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm() => new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());
	}
}
