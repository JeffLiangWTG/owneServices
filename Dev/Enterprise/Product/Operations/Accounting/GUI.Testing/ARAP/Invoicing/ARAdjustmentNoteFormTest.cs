using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(AdjustmentNoteForm))]
	public class ARAdjustmentNoteFormTest : BaseInvoicingFormTest
	{
		#region Implementation

		protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
		{
			return new AdjustmentNoteForm(invoice) { ControllerID = ControllerIDs.ARAdjustmentNote };
		}

		protected override InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null)
		{
			var invoice = factory != null ? factory.New<ARAdjustmentNote>() : Factory.New<ARAdjustmentNote>();
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

		public void TestPromptToPrintSelfBillingAdjustmentNote_OnPosting()
		{
			RunTestPromptToPrintSelfBillingAdjustmentNote_OnPosting(true);
		}

		public void TestPromptToPrintSelfBillingAdjustmentNote_OnPosting_UnsuccessfulSave()
		{
			RunTestPromptToPrintSelfBillingAdjustmentNote_OnPosting(false);
		}

		void RunTestPromptToPrintSelfBillingAdjustmentNote_OnPosting(bool testSuccessfulSave)
		{
			CargoWise.Common.Testing.DisposableLeakListener.Instance.StackTraceEnabled = true;
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			OrgHeader organisation = testObjectCreator.AALSHI;
			organisation.CompanyData.OB_IsCreditor = true;
			organisation.CompanyData.SetAPTaxApplicable(false);
			organisation.CompanyData.OB_APCostsSelfBilled = true;
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.APAdjustmentNote);
			using (AdjustmentNoteForm form = (AdjustmentNoteForm)controller.ShowNewForm())
			{
				if (!testSuccessfulSave)
				{
					form.BusinessEntity.Factory.Saving += f =>
					{
						throw new JobCreationException("Test Exception");
					};
				}

				AssertEquals("IsPostOnly", false, form.IsPostOnly);
				APAdjustmentNote aPAdj = (APAdjustmentNote)form.BusinessEntity;
				aPAdj.FillWithValidTestData();
				aPAdj.AH_OH = organisation.PK;
				APAdjustmentNoteLine aPAdjLine = (APAdjustmentNoteLine)aPAdj.Lines.AddNew();
				var chargeList = aPAdjLine.ChargeList;
				chargeList.Load();
				aPAdjLine.GenericCharge = chargeList[0].PK;
				aPAdjLine.AL_OSExTaxAmount = 10m;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.ValidateAndSave_ForTestOnly();

				if (testSuccessfulSave)
				{
					Assert("User should be prompted", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals("User should be prompted to print credit note", "Do you want to print Self Billing Adjustment Note SB00001000?", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					aPAdjLine.AL_Desc = "Some change";
					Assert("Should be in Database", aPAdj.IsInDatabase);
					form.ValidateAndSave_ForTestOnly();
					Assert("No questions asked", UnitTestUserNotification.Instance.LastMessage.WasNone);
				}
				else
				{
					AssertEquals("Mutex error shown", "Test Exception", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		public void TestPromptToPrintSelfBillingAdjustmentNote_OnReversing()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			testObjectCreator.AALSHI.CompanyData.OB_IsDebtor = true;

			ARAdjustmentNote adjustmentNote = Factory.NewWithValidTestData<ARAdjustmentNote>();
			adjustmentNote.AH_OH = testObjectCreator.AALSHI.PK;
			InvoicingLineBase line = (InvoicingLineBase)adjustmentNote.Lines.AddNew();
			line.FillWithValidTestData();
			line.AL_AC = TestObjectCreator.CC1.PK;
			line.AL_OSExTaxAmount = 10m;

			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.ARAdjustmentNote);
			using (AdjustmentNoteForm form = (AdjustmentNoteForm)controller.ShowDeleteForm(adjustmentNote))
			{
				AssertEquals("IsPostOnly", true, form.IsPostOnly);
				form.FReversingReason_ForTestOnly = "Because we want to reverse";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Delete_ForTestOnly();
				Assert("There should be a question", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("There should be a prompt for user to print the adjustment note", "Do you want to print adjustment note 00001001?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public override void TestPreviewInvoiceMenuItem_OnOpeningFormWhenTaxFrameworkIsEnabled()
		{
			var invoice = GetInvoiceWithValidTestData();

			using (var form = GetFormByInvoice(invoice))
			{
				form.DisplayMode = ODisplayMode.Edit;
				form.Show();

				var previewInvoiceMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName].MenuItems.FindByText("Preview Invoice");
				Assert("Preview Menu item accessibilty", previewInvoiceMenuItem.Enabled);
			}
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
