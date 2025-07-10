using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(UAInvoiceForm))]
	public class UAInvoiceFormTest : APInvoiceFormTest
	{
		public void TestPromptToPrintCostConfirmationDocument()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			AccountingConfigurationRegistry.Instance.PrintOptionWhenUnapprovedAPInvoicePosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			ZController controller = ZControllerFactory.Create(ControllerIDs.UAInvoice);
			using (UAInvoiceForm form = (UAInvoiceForm)controller.ShowNewForm())
			{
				UAInvoice invoice = (UAInvoice)form.BusinessEntity;
				invoice.FillWithValidTestData();
				invoice.AH_OH = testObjectCreator.AALSHI.PK;
				UAInvoiceLine invoiceLine = (UAInvoiceLine)invoice.Lines.AddNew();
				var chargeList = invoiceLine.ChargeList;
				chargeList.Load();
				invoiceLine.GenericCharge = chargeList[0].PK;
				invoiceLine.AL_OSExTaxAmount = 10m;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ValidateAndSave_ForTestOnly();

				Assert("User should be prompted", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("User should be prompted to print credit note", "Do you want to print a Cost Confirmation Document for this transaction?", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			AccountingConfigurationRegistry.Instance.PrintOptionWhenUnapprovedAPInvoicePosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			using (UAInvoiceForm form = (UAInvoiceForm)controller.ShowNewForm())
			{
				UAInvoice invoice = (UAInvoice)form.BusinessEntity;
				invoice.FillWithValidTestData();
				invoice.AH_OH = testObjectCreator.AALSHI.PK;
				UAInvoiceLine invoiceLine = (UAInvoiceLine)invoice.Lines.AddNew();
				var chargeList = invoiceLine.ChargeList;
				chargeList.Load();
				invoiceLine.GenericCharge = chargeList[0].PK;
				invoiceLine.AL_OSExTaxAmount = 10m;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ValidateAndSave_ForTestOnly();

				Assert("User should not be prompted any message", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}
		}

		public void TestInitialiseForm()
		{
			UAInvoice invoice = Factory.New<UAInvoice>();
			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Assert("CashInvoiceCheckboxPanel must be not visible", !form.ReceiptPaymentOuterPanel.Visible);
			}

			invoice = Factory.New<UAInvoice>();
			invoice.AH_Ledger = "AP";
			invoice.AH_TransactionType = "INV";

			using (InvoiceForm form = new InvoiceForm(invoice))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();
				Assert("CashInvoiceCheckboxPanel must be visible", form.ReceiptPaymentOuterPanel.Visible);
			}
		}

		public void TestFormDoesntAskForConfirmationOnRejecting()
		{
			UAInvoice invoice = (UAInvoice)GetInvoiceWithValidTestData();
			Factory.Save();
			ZController controller = ZControllerFactory.Create(ControllerIDs.UAInvoice);
			using (UAInvoiceForm form = (UAInvoiceForm)controller.ShowDeleteForm(invoice))
			{
				form.Show();
				form.PostingButtonsUserControl.SaveAndCloseButton.PerformClick();
				Assert("There should be no prompts", !UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}
		}

		public void TestPostButtonText_ForConvertedUAInvoice()
		{
			UAInvoice invoice = (UAInvoice)GetInvoiceWithValidTestData();
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
			var invoice = factory != null ? factory.New<UAInvoice>() : Factory.New<UAInvoice>();
			if (fillTestData)
			{
				invoice.FillWithValidTestData();
			}

			return invoice;
		}

		protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
		{
			var form = invoice is Invoice ? (BaseInvoicingForm)new UAInvoiceForm(invoice) : new UACreditNoteForm(invoice);
			form.ControllerID = ControllerIDs.UAInvoice;
			return form;
		}

		protected override bool ShouldShowRelatedInvoicesTab
		{
			get { return false; }
		}

		#endregion

		protected override bool ShouldTestForOtherTaxes => false;
	}
}
