using System.Windows.Forms;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideInvoiceAddressContactForm))]
	public class OverrideInvoiceAddressContactFormBasherTest : OverrideInvoiceDetailsFormTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new OverrideInvoiceAddressContactForm(new OverrideInvoiceAddressContactHelper(Factory, Factory.New<ARInvoice>().PK));
		}

		#endregion
	}
}
