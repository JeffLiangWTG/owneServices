using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UACreditNoteForm))]
	public class UACreditNoteFormTest : APCreditNoteFormTest
	{
		public override void TestOriginalInvoiceReferenceNumberForAmendVisibility()
		{
			using (var form = GetFormByInvoice(GetInvoiceWithValidTestData()))
			{
				AssertOriginalInvoiceReferenceGUIVisibilty(form, false, false);
			}
		}

		public void TestFormDoesntAskForConfirmationOnRejecting()
		{
			UACreditNote creditNote = (UACreditNote)GetInvoiceWithValidTestData();
			Factory.Save();
			ZController controller = ZControllerFactory.Create(ControllerIDs.UACreditNote);
			using (UACreditNoteForm form = (UACreditNoteForm)controller.ShowDeleteForm(creditNote))
			{
				form.Show();
				form.PostingButtonsUserControl.SaveAndCloseButton.PerformClick();
				Assert("There should be no prompts", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}
		}

		public void TestPostButtonText_ForConvertedUACreditNote()
		{
			UACreditNote invoice = (UACreditNote)GetInvoiceWithValidTestData();
			Factory.Save();

			var converter = new UnapprovedTransactionConverter(Factory);

			var convertedInvoice = converter.ConvertToAP(invoice, false);

			using (var form = new BaseInvoicingForm(convertedInvoice))
			{
				form.Show();
				AssertEquals("Post button text", "&Post", form.PostingButtonsUserControl.SaveButton.Text);
				AssertEquals("Post and close button text", "P&ost && Close", form.PostingButtonsUserControl.SaveAndCloseButton.Text);
			}
		}

		public override void TestSavingWhenDisplayModeIsDelete()
		{
			Assert("Reversing doesn't not apply here.", true);
		}

		public override void TestTotalsForOtherTaxesOnReversal()
		{
			Assert("Reversing doesn't not apply here.", true);
		}

		public override void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithValidLedgerWhenInvoiceLedgerIsValid()
		{
			Assert("Test not applicable as tax framework configuration not applicable for UA Type transaction", true);
		}

		public override void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithInvoiceCompany()
		{
			Assert("Test not applicable as tax framework configuration not applicable for UA Type transaction", true);
		}

		public override void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithInvoiceFactory()
		{
			Assert("Test not applicable as tax framework configuration not applicable for UA Type transaction", true);
		}

		public override void TestAutoAllocateDiscrepancy_IsAllowed_WhenNoTaxConfigExistsForTheLedger()
		{
			Assert("Test not applicable as tax framework configuration not applicable for UA Type transaction", true);
		}

		public override void TestAutoAllocateDiscrepancy_IsNotAllowed_WhenTaxConfigExistsForTheLedger()
		{
			Assert("Test not applicable as tax framework configuration not applicable for UA Type transaction", true);
		}

		#region Implementation

		protected override InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null)
		{
			var invoice = factory != null ? factory.New<UACreditNote>() : Factory.New<UACreditNote>();
			if (fillTestData)
			{
				invoice.FillWithValidTestData();
			}

			return invoice;
		}

		protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
		{
			var form = invoice is Invoice ? (BaseInvoicingForm)new UAInvoiceForm(invoice) : new UACreditNoteForm(invoice);
			form.ControllerID = ControllerIDs.UACreditNote;
			return form;
		}

		protected override bool ShouldTestCashInvoiceOnCheckBoxControl => false;

		protected override bool ShouldTestForOtherTaxes => false;

		protected override bool ShouldSupportOverrideExRateCheckbox => true;
		#endregion
	}
}
