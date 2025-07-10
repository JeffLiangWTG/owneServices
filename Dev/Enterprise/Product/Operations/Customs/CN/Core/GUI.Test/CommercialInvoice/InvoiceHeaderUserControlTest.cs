using System.Linq;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.GUI.CommercialInvoice;

namespace Enterprise.Customs.CN.GUI.Testing
{
	sealed class InvoiceHeaderUserControlTest : Customs.GUI.Testing.InvoiceHeaderUserControlAbstractTest
	{
		public void TestPricingConfirmControls()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice = declaration.Invoices.FirstOrDefault() ?? declaration.Invoices.AddNew();

			using (var control = new InvoiceHeaderUserControl())
			{
				control.Invoice = invoice;
				AssertEquals("TemporaryPricingConfirmDropEdit binding.", "Invoices.JZ_Calc_TemporaryPricingConfirm", control.TemporaryPricingConfirmDropEdit.BindTo);
				AssertEquals("FormulaPricingConfirmDropEdit binding.", "Invoices.JZ_Calc_FormulaPricingConfirm", control.FormulaPricingConfirmDropEdit.BindTo);
			}
		}

		protected override Customs.GUI.CommonInvoiceHeaderUserControl GetNewInvoiceHeaderUserControl() => new InvoiceHeaderUserControl();
	}
}
