using System.Reflection;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(InvoiceForm))]
	public class APIncompleteInvoiceFormTest : APInvoiceFormTest
	{
		protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
		{
			return invoice is Invoice ? new InvoiceForm(invoice) { ControllerID = ControllerIDs.APIncompleteInvoice } :
														new CreditNoteForm(invoice) { ControllerID = ControllerIDs.APIncompleteCreditNote };
		}

		protected override InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null)
		{
			var invoice = base.GetInvoiceWithValidTestData(true, factory);
			invoice.SaveAsIncomplete();
			AssertEquals("Postcondition: invoice is incomplete now", LedgerTypes.IncompleteTransactions, invoice.AH_Ledger);

			return invoice;
		}

		public void TestShouldReopenWithCorrectControllerID()
		{
			var invoice1 = GetInvoiceWithValidTestData(true);
			var invoice2 = base.GetInvoiceWithValidTestData(true);

			using (var form1 = GetFormByInvoice(invoice1))
			using (var form2 = GetFormByInvoice(invoice2))
			{
				AssertEquals("Postcondition: invoice1 is Incomplete.", LedgerTypes.IncompleteTransactions, invoice1.AH_Ledger);
				AssertEquals("Postcondition: form1 is APIncompleteInvoice.", ControllerIDs.APIncompleteInvoice, form1.ControllerID);

				AssertEquals("Postcondition: invoice2 is AP.", LedgerTypes.AccountsPayable, invoice2.AH_Ledger);
				AssertEquals("Postcondition: form2 is APIncompleteInvoice.", ControllerIDs.APIncompleteInvoice, form2.ControllerID);

				var methodInfo = typeof(BaseInvoicingForm).GetMethod("TryReopenWithCorrectControllerID", BindingFlags.NonPublic | BindingFlags.Instance);

				var result1 = methodInfo.Invoke(form1, null);
				AssertEquals("form1: Should NOT create new ZController.", null, result1);

				var result2 = (ZController)methodInfo.Invoke(form2, null);
				AssertEquals("form2: Should create an APInvoice ZController.", ControllerIDs.APInvoice, result2.ID);
				AssertEquals("form2 new controller: IsUseBusinessEntityFactory should be true.", true, ((IBusinessEntityFactorySettings)result2).ShouldUseSourceEntityFactory);

				result2.LastShownForm.Dispose();
			}
		}

		protected override bool ShouldTestForOtherTaxes => true;

		protected override bool ShouldSupportOverrideExRateCheckbox => true;

		protected override bool IsTrackHasChangesCalledWhenDeleteingAnInvoice => false;
	}
}
