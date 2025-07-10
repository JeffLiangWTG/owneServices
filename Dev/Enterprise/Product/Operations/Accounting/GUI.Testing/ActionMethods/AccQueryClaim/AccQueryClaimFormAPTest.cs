using System.Linq;
using System.Windows.Forms;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.AccQueryClaims;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AccQueryClaimForm))]
	public class AccQueryClaimFormAPTest : AccQueryClaimFormTest
	{
		public override void TestInvoiceNumberControlUseCorrectCodeProperty()
		{
			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1, 10, 0, 10, 0);
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			Factory.Save();
			AssertNotEquals("Precondition: AH_TransactionNum not equal to AH_ConsolidatedInvoiceRef.", invoice.AH_TransactionNum, invoice.AH_ConsolidatedInvoiceRef);
			using (var form = (AccQueryClaimForm)GetFormToBash())
			{
				form.Claim_ForTestOnly.AY_OH_Debtor = invoice.AH_OH;
				form.Show();

				var accQueryClaimUserControl = form.Controls.Find("accQueryClaimUserControl", true)[0];
				var invoiceFindBox = (ZGuidFindBox)accQueryClaimUserControl.Controls.Find("InvoiceGuidFindBox", true)[0];
				var contactGuidDropEdit = accQueryClaimUserControl.Controls.Find("ContactGuidDropEdit", true)[0];

				invoiceFindBox.Focus();
				invoiceFindBox.CodeBox.Text = invoice.AH_ConsolidatedInvoiceRef;
				contactGuidDropEdit.Focus();
				AssertEquals("AY_AH", invoice.PK, form.Claim_ForTestOnly.AY_AH);
			}
		}

		protected override InvoicingBase CreateValidatedInvoice()
		{
			TestObjectCreator.ABIGAS.OH_IsCreditor = true;
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "INV1", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m);
			invoice.AH_OH = TestObjectCreator.ABIGAS.PK;
			invoice.AH_OC_InvoiceContactOverride = TestObjectCreator.ABIGAS.Contacts.First().PK;
			Factory.Save();

			return invoice;
		}

		protected override Form GetFormToBashCore()
		{
			return new AccQueryClaimForm(Factory.New<APAccQueryClaim>());
		}
	}
}
