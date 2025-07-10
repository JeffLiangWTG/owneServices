using System;
using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.Common.AU;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.AU.GUI.CommercialInvoice.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		public void TestQuarantineTabPage()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				Application.DoEvents();
				Assert("Default to false.", !invoice.IsQuarantine);
				AssertNull(form.MainTabControl.TabPages["QuarantineTabPage"]);
				invoice.JZ_MessageType = AUJobMessageTypeList.Codes.Quarantine;
				Assert("Should be true when the message type is AQS.", invoice.IsQuarantine);
				var tabPage = (ZTabPage)form.MainTabControl.TabPages["QuarantineTabPage"];
				Assert("Should be false for prevent any visible brodcsting.", !tabPage.CheckForChildrenControlsVisibilityChange);
				Assert("Should be false for prevent any visible brodcsting.", !tabPage.CheckForNotifications);
				Assert("Should be true when the invoice is Quarantine.", tabPage.TabVisible);
				invoice.JZ_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				Assert("Should be false when the invoice is not Quarantine.", !tabPage.TabVisible);
			}
		}

		public void TestDifferntInvoiceLineUserControlForDifferentMessageType()
		{
			using (CommercialInvoiceForm form = new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>()))
			{
				form.Show();
				AssertInvoiceLineUserControl(form, Common.Shared.SharedJobMessageTypeList.Codes.Import, typeof(AUCMRImportInvoiceLineUserControl));
				AssertInvoiceLineUserControl(form, Common.Shared.SharedJobMessageTypeList.Codes.Export, typeof(AUExportInvoiceLineUserControl));
				AssertInvoiceLineUserControl(form, AUJobMessageTypeList.Codes.Quarantine, typeof(AUQuarantineInvoiceLineUserControl));
			}
		}

		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm() => new CommercialInvoiceForm(Factory.New<JobComInvoiceHeader>());

		void AssertInvoiceLineUserControl(CommercialInvoiceForm form, string messageType, Type expectedInvoiceLineUserControlType)
		{
			form.MainTabControl.SelectedTab = form.HeaderTabPage;
			form.Invoice.JZ_MessageType = messageType;
			form.MainTabControl.SelectedTab = form.LinesTabPage;
			AssertEquals("InvoiceLineUserControl type", expectedInvoiceLineUserControlType, form.InvoiceLineUserControl.GetType());
		}
	}
}
