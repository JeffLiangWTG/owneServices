using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AccQueryClaimForm))]
	public class AccQueryClaimFormARTest : AccQueryClaimFormTest
	{
		protected override Form GetFormToBashCore()
		{
			return new AccQueryClaimForm(Factory.New<ARAccQueryClaim>());
		}

		protected override InvoicingBase CreateValidatedInvoice()
		{
			TestObjectCreator.ABIGAS.OH_IsDebtor = true;
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "INV1", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice.AH_OC_InvoiceContactOverride = TestObjectCreator.ABIGAS.Contacts.First().PK;
			Factory.Save();

			return invoice;
		}
	}
}
