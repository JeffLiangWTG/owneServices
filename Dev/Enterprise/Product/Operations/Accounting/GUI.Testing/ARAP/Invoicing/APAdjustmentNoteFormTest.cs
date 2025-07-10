using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AdjustmentNoteForm))]
	public class APAdjustmentNoteFormTest : BaseInvoicingFormTest
	{
		#region Implementation

		protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
		{
			return new AdjustmentNoteForm(invoice) { ControllerID = ControllerIDs.APAdjustmentNote };
		}

		protected override InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null)
		{
			var invoice = factory != null ? factory.New<APAdjustmentNote>() : Factory.New<APAdjustmentNote>();
			if (fillTestData)
			{
				invoice.FillWithValidTestData();
			}

			return invoice;
		}

		protected override bool ShouldShowRelatedInvoicesTab
		{
			get { return false; }
		}

		protected override bool ShouldTestCashInvoiceOnCheckBoxControl => false;

		#endregion

		public void TestPromptToPrintCostConfirmationDocumentAdjustmentNote()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.PrintOptionWhenAPInvoicePosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			ZController controller = ZControllerFactory.Create(ControllerIDs.APAdjustmentNote);
			using (AdjustmentNoteForm form = (AdjustmentNoteForm)controller.ShowNewForm())
			{
				AssertEquals("IsPostOnly", false, form.IsPostOnly);
				APAdjustmentNote aPAdj = (APAdjustmentNote)form.BusinessEntity;
				aPAdj.FillWithValidTestData();
				aPAdj.AH_OH = testObjectCreator.AALSHI.PK;
				APAdjustmentNoteLine aPAdjLine = (APAdjustmentNoteLine)aPAdj.Lines.AddNew();
				var chargeList = aPAdjLine.ChargeList;
				chargeList.Load();
				aPAdjLine.GenericCharge = chargeList[0].PK;
				aPAdjLine.AL_OSExTaxAmount = 10m;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ValidateAndSave_ForTestOnly();

				Assert("User should be prompted", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("User should be prompted to print credit note", "Do you want to print a Cost Confirmation Document for this transaction?", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				aPAdjLine.AL_Desc = "Some Change";
				Assert("Should be in Database", aPAdj.IsInDatabase);
				form.ValidateAndSave_ForTestOnly();
				Assert("No questions asked", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			controller = ZControllerFactory.Create(ControllerIDs.ARAdjustmentNote);
			using (AdjustmentNoteForm form = (AdjustmentNoteForm)controller.ShowNewForm())
			{
				AssertEquals("IsPostOnly", false, form.IsPostOnly);
				ARAdjustmentNote aRAdj = (ARAdjustmentNote)form.BusinessEntity;
				aRAdj.FillWithValidTestData();
				aRAdj.AH_OH = testObjectCreator.AALSHI.PK;
				ARAdjustmentNoteLine aRAdjLine = (ARAdjustmentNoteLine)aRAdj.Lines.AddNew();
				var chargeList = aRAdjLine.ChargeList;
				chargeList.Load();
				aRAdjLine.GenericCharge = chargeList[0].PK;
				aRAdjLine.AL_OSExTaxAmount = 10m;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ValidateAndSave_ForTestOnly();

				AssertEquals("User should not be prompted to print credit note", false, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText("Do you want to print a Cost Confirmation Document for this transaction?"));
			}
		}

		public override void TestPreviewInvoiceMenuItem_OnOpeningFormWhenTaxFrameworkIsDisabled()
		{
			Assert("Not applicable here", true);
		}

		public override void TestPreviewInvoiceMenuItem_OnOpeningFormWhenTaxFrameworkIsEnabled()
		{
			Assert("Not applicable here", true);
		}

		public override void TestPreviewInvoiceMenuItem_WhenCalculateTaxTransactionsButtonDisabled()
		{
			Assert("Not applicable here", true);
		}

		public override void TestPreviewInvoiceMenuItem_WhenCalculateTaxTransactionsButtonDisabledAndEnabled()
		{
			Assert("Not applicable here", true);
		}

		public override void TestTotalsForOtherTaxesOnReversal()
		{
			Assert("Other taxes are not applicable here.", true);
		}

		public override void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithValidLedgerWhenInvoiceLedgerIsValid()
		{
			Assert("Test not applicable as tax framework configuration not applicable for adjustment note.", true);
		}

		public override void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithInvoiceCompany()
		{
			Assert("Test not applicable as tax framework configuration not applicable for adjustment note.", true);
		}

		public override void TestAutoAllocateDiscrepancy_Invokes_HasAnyActiveAccTaxConfiguration_WithInvoiceFactory()
		{
			Assert("Test not applicable as tax framework configuration not applicable for adjustment note.", true);
		}

		public override void TestAutoAllocateDiscrepancy_IsAllowed_WhenNoTaxConfigExistsForTheLedger()
		{
			Assert("Test not applicable as tax framework configuration not applicable for adjustment note.", true);
		}

		public override void TestAutoAllocateDiscrepancy_IsNotAllowed_WhenTaxConfigExistsForTheLedger()
		{
			Assert("Test not applicable as tax framework configuration not applicable for adjustment note.", true);
		}

		protected override bool ShouldTestForOtherTaxes => false;
	}
}
