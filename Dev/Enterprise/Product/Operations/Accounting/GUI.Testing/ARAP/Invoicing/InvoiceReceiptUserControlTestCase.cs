using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class InvoiceReceiptUserControlTestCase : TestCaseWithFactory
	{
		public void TestReceiptPaymentAH_DescTextbox()
		{
			using (var form = new ZForm())
			{
				var control = new InvoiceReceiptUserControl();
				form.Controls.Add(control);
				control.SetDataBinding(Factory.NewWithValidTestData<ARInvoice>(), null);
				form.Show();

				Assert("Description text box should be enabled.", control.ReceiptPaymentAH_DescTextbox.Enabled);
				Assert("Description text box should be visible.", control.ReceiptPaymentAH_DescTextbox.Visible);
				Assert("Description text box shoule not be readonly.", !control.ReceiptPaymentAH_DescTextbox.ReadOnly);
			}
		}
	}
}
