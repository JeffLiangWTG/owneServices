using System;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(CreditNoteForm))]
	public class APCreditNoteFormTest : CreditNoteFormTest
	{
		public class NonTransactionedAPCreditNoteFormTest : NonTransactionedBaseInvoicingFormTest
		{
			protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
			{
				return APCreditNoteFormTest.GetFormByInvoice(invoice);
			}

			protected override InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true)
			{
				return APCreditNoteFormTest.GetInvoiceWithValidTestData();
			}

			APCreditNoteFormTest APCreditNoteFormTest
			{
				get
				{
					if (apCreditNoteFormTest == null)
					{
						apCreditNoteFormTest = new APCreditNoteFormTest();
					}

					return apCreditNoteFormTest;
				}
			}
			APCreditNoteFormTest apCreditNoteFormTest;
		}

		public override void TestOriginalInvoiceReferenceNumberForAmendVisibility()
		{
			using (var form = GetFormByInvoice(GetInvoiceWithValidTestData()))
			{
				AssertOriginalInvoiceReferenceGUIVisibilty(form, true, false);
			}
		}

		public override void TestOnLoad_FinalFlagVisibility()
		{
			APCreditNote creditNote = Factory.NewWithValidTestData<APCreditNote>();
			using (CreditNoteForm form = new CreditNoteForm(creditNote))
			{
				form.Show();
				foreach (ZGridColumnInfo columnStyle in form.InvoiceDetails.TransactionLinesGrid.ColumnStyles)
				{
					if (columnStyle.ColumnName == InvoiceLine.Schema.AL_IsFinalCharge)
					{
						AssertEquals("IsVisible", true, columnStyle.IsVisible);
						break;
					}
				}
			}

			creditNote.IsReverseTransaction = true;
			using (CreditNoteForm form = new CreditNoteForm(creditNote))
			{
				form.Show();
				Application.DoEvents();
				Assert("Button should be disabled for reversing transaction", form.InvoiceDetails.ApportionChargesButton.ReadOnly);
				foreach (ZGridColumnInfo columnStyle in form.InvoiceDetails.TransactionLinesGrid.ColumnStyles)
				{
					if (columnStyle.ColumnName == InvoiceLine.Schema.AL_IsFinalCharge)
					{
						AssertEquals("IsVisible", false, columnStyle.IsVisible);
						break;
					}
				}
			}

			creditNote.IsReverseTransaction = false;
			Factory.Save();
			using (CreditNoteForm form = new CreditNoteForm(creditNote))
			{
				form.Show();
				Application.DoEvents();
				Assert("Button should be enabled", !form.InvoiceDetails.ApportionChargesButton.ReadOnly);
				foreach (ZGridColumnInfo columnStyle in form.InvoiceDetails.TransactionLinesGrid.ColumnStyles)
				{
					if (columnStyle.ColumnName == InvoiceLine.Schema.AL_IsFinalCharge)
					{
						AssertEquals("IsVisible", false, columnStyle.IsVisible);
						break;
					}
				}
			}
		}

		public void TestDebtorReadonlyWhenReversing()
		{
			var creditNote = Factory.NewWithValidTestData<ARCreditNote>();

			creditNote.IsReverseTransaction = false;
			using (var form = new CreditNoteForm(creditNote))
			{
				form.Show();
				Application.DoEvents();
				Assert("Should not be readonly", !form.InvoiceDetails.AddressWithContactControl.ReadOnly);
			}

			creditNote.IsReverseTransaction = true;
			using (var form = new CreditNoteForm(creditNote))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				Application.DoEvents();
				Assert("Should be readonly", form.InvoiceDetails.AddressWithContactControl.ReadOnly);
			}
		}

		public void TestPromptToPrintSelfBillingCreditNote()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();

			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);

			OrgHeader organisation = testObjectCreator.AALSHI;
			organisation.CompanyData.OB_IsCreditor = true;
			organisation.CompanyData.SetAPTaxApplicable(false);
			organisation.CompanyData.OB_APCostsSelfBilled = true;
			Factory.Save();

			ZController controller = ZControllerFactory.Create(ControllerIDs.APCreditNote);
			using (CreditNoteForm form = (CreditNoteForm)controller.ShowNewForm())
			{
				AssertEquals("IsPostOnly", false, form.IsPostOnly);
				APCreditNote aPCrd = (APCreditNote)form.BusinessEntity;
				aPCrd.FillWithValidTestData();
				aPCrd.AH_OH = organisation.PK;
				APCreditNoteLine aPCrdLine = (APCreditNoteLine)aPCrd.Lines.AddNew();
				var chargeList = aPCrdLine.ChargeList;
				chargeList.Load();
				aPCrdLine.GenericCharge = chargeList[0].PK;
				aPCrdLine.AL_OSExTaxAmount = 10m;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ValidateAndSave_ForTestOnly();

				Assert("User should be prompted", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("User should be prompted to print credit note", string.Format("Do you want to print Self Billing Credit Note {0}?", aPCrd.AH_TransactionNum), UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestPromptToPrintCostConfirmationDocumentInvoice()
		{
			AccountingPeriodTestHelper periodHelper = new AccountingPeriodTestHelper(new BusinessObjectFactory());
			periodHelper.SetupPeriods();
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccountingConfigurationRegistry.Instance.PrintOptionWhenAPInvoicePosted.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			ZController controller = ZControllerFactory.Create(ControllerIDs.APCreditNote);
			using (CreditNoteForm form = (CreditNoteForm)controller.ShowNewForm())
			{
				AssertEquals("IsPostOnly", false, form.IsPostOnly);
				APCreditNote aPCrd = (APCreditNote)form.BusinessEntity;
				aPCrd.FillWithValidTestData();
				aPCrd.AH_OH = testObjectCreator.AALSHI.PK;
				APCreditNoteLine aPCrdLine = (APCreditNoteLine)aPCrd.Lines.AddNew();
				var chargeList = aPCrdLine.ChargeList;
				chargeList.Load();
				aPCrdLine.GenericCharge = chargeList[0].PK;
				aPCrdLine.AL_OSExTaxAmount = 10m;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.ValidateAndSave_ForTestOnly();

				Assert("User should be prompted", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("User should be prompted to print credit note", "Do you want to print a Cost Confirmation Document for this transaction?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeleteConsolApportionmentRowsFromGrid()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			Factory.Save();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			var creator = new TestObjectCreator(Factory);

			var aPCreditNote = Factory.New<APCreditNote>();

			try
			{
				using (var form = new CreditNoteForm(aPCreditNote))
				{
					form.Show();

					var cost = aPCreditNote.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
					cost.E6_AC_ChargeCode = creator.CC1.PK;
					cost.E6_OSCostAmount = 100m;
					cost.E6_ApportionmentMethod = "SHP";
					cost.SetIsUsedForApportionment();
					aPCreditNote.ImportAllApportionmentsFromCosting();

					AssertEquals("Should be 2 lines", 2, aPCreditNote.Lines.Count);

					var handleDeleteInfo = typeof(ZGrid).GetMethod("HandleDelete", BindingFlags.Instance | BindingFlags.NonPublic);
					AssertNotNull(handleDeleteInfo);
					form.InvoiceDetails.TransactionLinesGrid.Select(0);
					handleDeleteInfo.Invoke(form.InvoiceDetails.TransactionLinesGrid, new object[] { 1 });

					AssertEquals("Should show message saying that all apportionment related lines must be selected",
						"This line relates to an apportionment of cost. All lines relating to this apportionment must also be deleted. Would you like to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Should show question to user with yes and no buttons", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				}
			}
			finally
			{
				aPCreditNote.ClearApportionmentJobMutexes();
			}
		}

		public override void TestApportionChargesButtonEnabled()
		{
			APCreditNote aPCreditNote = Factory.New<APCreditNote>();
			using (CreditNoteForm form = new CreditNoteForm(aPCreditNote))
			{
				form.Show();
				Assert("Should have apportion button visible", form.InvoiceDetails.ApportionChargesButton.Visible);
				Assert("Should have apportion button enabled", form.InvoiceDetails.ApportionChargesButton.Enabled);
			}
		}

		public void TestBusinessContextAPCreditNoteForm()
		{
			var creditNote = Factory.NewWithValidTestData<APCreditNote>();

			using (var form = (CreditNoteForm)GetFormByInvoice(creditNote))
			{
				form.DisplayMode = ODisplayMode.New;
				form.Show();
				Application.DoEvents();

				AssertEquals(true, creditNote.Factory.HasContext(BusinessContext.APCreditNoteForm));
				form.Close();

				AssertEquals(false, creditNote.Factory.HasContext(BusinessContext.APCreditNoteForm));
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

		#region Implementation

		protected override BaseInvoicingForm GetFormByInvoice(InvoicingBase invoice)
		{
			return invoice is Invoice ? new InvoiceForm(invoice) { ControllerID = ControllerIDs.APInvoice } :
														new CreditNoteForm(invoice) { ControllerID = ControllerIDs.APCreditNote };
		}

		protected override InvoicingBase GetInvoiceWithValidTestData(bool fillTestData = true, BusinessObjectFactory factory = null)
		{
			var invoice = factory != null ? factory.New<APCreditNote>() : Factory.New<APCreditNote>();
			if (fillTestData)
			{
				invoice.FillWithValidTestData();
			}

			return invoice;
		}

		protected override bool ShouldShowRelatedInvoicesTab
		{
			get { return true; }
		}

		protected override bool ShouldTestCashInvoiceOnCheckBoxControl => false;

		public override void TestPromptToPrintComplianceDocumentWhenEnablePrompt()
		{
			TestPromptToPrintComplianceDocumentCore(true, AssertForNotPromptToPrintComplianceDocument);
		}

		public override void TestPromptToPrintComplianceDocumentWhenDisablePrompt()
		{
			TestPromptToPrintComplianceDocumentCore(false, AssertForNotPromptToPrintComplianceDocument);
		}

		protected override bool ShouldSupportOverrideExRateCheckbox => true;
		#endregion
	}
}
