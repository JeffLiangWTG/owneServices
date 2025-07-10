using System;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.DataExportBatch;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(TransactionPendingAllocationForm))]
	public class TransactionPendingAllocationFormTest : ZFormBasherTest
	{
		#region Plug-ins

		public void TestDataExportBatchPluginIsAdded()
		{
			using (var form = (TransactionPendingAllocationForm)GetFormToBashCore())
			{
				var source = form.BusinessEntity as IDataExportBatchSource;
				AssertNotNull("Precondition: entity is IDataExportBatchSource", source);
				AssertNotNull("Precondition: IDataExportBatchSource entity has support", source.IsDataExportBatchSupported);
				AssertNotNull("IDataExportBatchSource entity should have plugin", form.PlugIns.GetPlugIn(ControllerIDs.DataExportBatchPlugin));
			}
		}

		public void TesteDocsPluginIsAdded()
		{
			using (var form = (TransactionPendingAllocationForm)GetFormToBashCore())
			{
				AssertNotNull("Should have eDocsPlugIn", form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			}
		}

		#endregion

		public void TestLoadAndReloadForWorkflowTab()
		{
			var transactionByCreditor1 = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			var transactionByCreditor2 = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor2, 50);
			Factory.Save();

			using (var form = new TransactionPendingAllocationForm(transactionByCreditor1))
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Show();
				Assert("Should have workFlow tab", form.MainTabControl.TabPages.ContainsKey("WorkflowTabPage"));
				form.MainTabControl.SelectTab("WorkflowTabPage");
				var workflowTabPage = form.GetControl<ZWorkflowTabPage>("WorkflowTabPage");
				AssertNotNull(workflowTabPage);
			}

			using (var form = new TransactionPendingAllocationForm(transactionByCreditor2))
			{
				form.ControllerID = DummyControllerIDs.Dummy1;
				var transactionDetailsTabPage = form.GetControl<ZTabPage>("TransactionDetailsTabPage");
				transactionDetailsTabPage.NotifyBindingOrShowing();
				Assert("Should have workFlow tab", form.MainTabControl.TabPages.ContainsKey("WorkflowTabPage"));
				form.MainTabControl.SelectTab("WorkflowTabPage");
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("ErrorReporter's exception count", 0, ErrorReporter.TotalErrorCount);
					AssertEquals("ErrorReporter's exception content", "", ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				});
			}
		}

		public void TestNotesTab()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			var note = transaction.Notes.AddNew(true, "Good Description", "Good Text");
			Factory.Save();

			using (var form = new TransactionPendingAllocationForm(transaction))
			{
				form.Show();
				Assert("Should have notes tab",form.MainTabControl.TabPages.ContainsKey("NotesTabPage"));
				form.MainTabControl.SelectTab("NotesTabPage");
				var notesTabPage = form.GetControl<ZStmNoteTabPage>("NotesTabPage");
				Assert("Notes tab should be read only",notesTabPage.ReadOnly);
				AssertEquals("Should have notes", note, notesTabPage.SelectedNote);
			}
		}

		public void TestAH_NumberOfSupportingDocumentsCalcEditVisiblity()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction, "some xml", true);
			Assert("Precondition: HasUniversalTransaction", transaction.IsImportedFromUniversalXML);

			using (var form = new TransactionPendingAllocationForm(transaction))
			{
				form.Show();
				var ah_NumberOfSupportingDocumentsCalcEdit = form.GetControl<ZCalcEdit>("AH_NumberOfSupportingDocumentsCalcEdit"); // Unit testing code
				AssertEquals("NumberOfSupportingDocumentsCalcEdit: Not china, so not visible", false, ah_NumberOfSupportingDocumentsCalcEdit.Visible);
				var xmlTabControl = form.GetControl<ZTemplateTabControl>("MainTabControl", false);
				xmlTabControl.SelectTab("sourceXmlTabPage");
				var xmlNumberOfDocsCalcEdit = form.GetControl<ZCalcEdit>("xmlNumberOfDocsCalcEdit");
				AssertEquals("xmlNumberOfDocsCalcEdit: Not china, so not visible", false, xmlNumberOfDocsCalcEdit.Visible);
			}

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.China);
			using (var form = new TransactionPendingAllocationForm(transaction))
			{
				form.Show();
				var ah_NumberOfSupportingDocumentsCalcEdit = form.GetControl<ZCalcEdit>("AH_NumberOfSupportingDocumentsCalcEdit"); // Unit testing code
				AssertEquals("NumberOfSupportingDocumentsCalcEdit: China, so visible", true, ah_NumberOfSupportingDocumentsCalcEdit.Visible);
				var xmlTabControl = form.GetControl<ZTemplateTabControl>("MainTabControl", false);
				xmlTabControl.SelectTab("sourceXmlTabPage");
				var xmlNumberOfDocsCalcEdit = form.GetControl<ZCalcEdit>("xmlNumberOfDocsCalcEdit");
				AssertEquals("xmlNumberOfDocsCalcEdit: China, so visible", true, xmlNumberOfDocsCalcEdit.Visible);
			}
		}

		public void TestWHTColumnsVisiblity()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction, "some xml", true);
			Assert("Precondition: HasUniversalTransaction", transaction.IsImportedFromUniversalXML);

			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;
			using (var form = new TransactionPendingAllocationForm(transaction))
			{
				form.Show();
				var xmlTabControl = form.GetControl<ZTemplateTabControl>("MainTabControl", false);
				xmlTabControl.SelectTab("sourceXmlTabPage");
				var xmlLinesGrid = form.GetControl<ZGrid>("xmlLinesGrid");
				AssertEquals("WithholdingTaxID", false, xmlLinesGrid.Columns.Contains("WithholdingTaxID"));
				AssertEquals("OSWHTAmount", false, xmlLinesGrid.Columns.Contains("OSWHTAmount"));
				AssertEquals("LocalWHTAmount", false, xmlLinesGrid.Columns.Contains("LocalWHTAmount"));
			}

			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
			using (var form = new TransactionPendingAllocationForm(transaction))
			{
				form.Show();
				var xmlTabControl = form.GetControl<ZTemplateTabControl>("MainTabControl", false);
				xmlTabControl.SelectTab("sourceXmlTabPage");
				var xmlLinesGrid = form.GetControl<ZGrid>("xmlLinesGrid");
				AssertEquals("WithholdingTaxID", true, xmlLinesGrid.Columns.Contains("WithholdingTaxID"));
				AssertEquals("WithholdingTaxID default visibility", false, xmlLinesGrid.GetColumnStyle("WithholdingTaxID").IsVisible);
				AssertEquals("OSWHTAmount", true, xmlLinesGrid.Columns.Contains("OSWHTAmount"));
				AssertEquals("OSWHTAmount default visibility", false, xmlLinesGrid.GetColumnStyle("OSWHTAmount").IsVisible);
				AssertEquals("LocalWHTAmount", true, xmlLinesGrid.Columns.Contains("LocalWHTAmount"));
				AssertEquals("LocalWHTAmount default visibility", false, xmlLinesGrid.GetColumnStyle("LocalWHTAmount").IsVisible);
			}
		}

		public void TestTaxBranchVisibility()
		{
			AssertTaxBranchVisibility(true, true);
			AssertTaxBranchVisibility(true, false);
			AssertTaxBranchVisibility(false, true);
			AssertTaxBranchVisibility(false, false);

			void AssertTaxBranchVisibility(bool isEnableRegistry, bool isGSTRegistered)
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isEnableRegistry))
				using (var testForm = GetFormToBash())
				{
					testForm.Show();
					Application.DoEvents();

					var expectedVisible = isEnableRegistry && isGSTRegistered;
					var taxBranchFindBox = testForm.GetControl<ZGuidFindBox>("TaxBranchFindBox");

					AssertEquals(expectedVisible, taxBranchFindBox.Visible);
				}
			}
		}

		public void TestDefaultColumnsVisiblity()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction, "some xml", true);
			Assert("Precondition: HasUniversalTransaction", transaction.IsImportedFromUniversalXML);

			using (var form = new TransactionPendingAllocationForm(transaction))
			{
				form.Show();
				var xmlTabControl = form.GetControl<ZTemplateTabControl>("MainTabControl", false);
				xmlTabControl.SelectTab("sourceXmlTabPage");
				var xmlLinesGrid = form.GetControl<ZGrid>("xmlLinesGrid");
				AssertEquals("TaxMessageID", false, xmlLinesGrid.GetColumnStyle("TaxMessageID").IsVisible);
				AssertEquals("RecoverableGSTVATPercentage", false, xmlLinesGrid.GetColumnStyle("RecoverableGSTVATPercentage").IsVisible);
				AssertEquals("SubAccounts", false, xmlLinesGrid.GetColumnStyle("SubAccounts").IsVisible);
			}
		}

		public void TestSourceXmlTabVisibility()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			using (var form = new TransactionPendingAllocationForm(transaction))
			{
				form.Show();

				var xmlTabControl = form.GetControl<ZTemplateTabControl>("MainTabControl", false);
				Assert("sourceXmlTabPage.TabVisible", !xmlTabControl.TabPages.ContainsKey("sourceXmlTabPage"));
			}

			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction);
			Assert("Precondition: HasUniversalTransaction", !transaction.IsImportedFromUniversalXML);
			using (var form = new TransactionPendingAllocationForm(transaction))
			{
				form.Show();

				var xmlTabControl = form.GetControl<ZTemplateTabControl>("MainTabControl", false);
				Assert("sourceXmlTabPage.TabVisible", !xmlTabControl.TabPages.ContainsKey("sourceXmlTabPage"));
			}

			request.Initialize(transaction, "some xml", true);
			Assert("Precondition: HasUniversalTransaction", transaction.IsImportedFromUniversalXML);
			using (var form = new TransactionPendingAllocationForm(transaction))
			{
				form.Show();

				var xmlTabControl = form.GetControl<ZTemplateTabControl>("MainTabControl", false);
				Assert("sourceXmlTabPage.TabVisible", xmlTabControl.TabPages.ContainsKey("sourceXmlTabPage"));
				var sourceXmlTabPage = form.GetControl<ZTabPage>("sourceXmlTabPage");
				Assert("sourceXmlTabPage", sourceXmlTabPage.TabVisible);
			}
		}

		public void TestApprovalRequestCreation()
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.TransactionsPendingAllocationAllocate.IsAllowed = false;
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			transaction.RunPreSaveValidation();
			AssertNoErrors("Precondition", transaction);

			using (var form = new TransactionPendingAllocationForm(transaction))
			{
				form.Show();

				Env.Instance.Registry.ShowSaveProgressBox = false;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
					{
						if (dialog is LoginForm)
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore;
						}
						else
						{
							ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
						}
					});
				form.FireSaveButton();
				Assert("Transaction is posted", transaction.IsInDatabase);
				Assert("Transaction Has ApprovalRequest. An approval request should exist.", transaction.HasApprovalRequest);
			}
		}

		public void TestEditingTransactionWithApprovalRequest()
		{
			AccountingMasterFilesRegistry.Instance.EnableTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.TransactionsPendingAllocationAllocate.IsAllowed = true;
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			transaction.RunPreSaveValidation();
			AssertNoErrors("Precondition", transaction);

			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction);
			Factory.Save();
			Assert("Precondition: HasApprovalRequest", transaction.HasApprovalRequest);
			var previousRequest = transaction.TransactionApprovalRequest;
			AssertNotNull("previousRequest", previousRequest);

			using (var form = new TransactionPendingAllocationForm(transaction))
			{
				form.Show();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.FireSaveButton();

				Assert("Transaction HasApprovalRequest", transaction.HasApprovalRequest);
				AssertNotNull("TransactionApprovalRequest", transaction.TransactionApprovalRequest);
				Assert("TransactionApprovalRequest is saved", transaction.TransactionApprovalRequest.IsInDatabase);
				AssertEquals("TransactionApprovalRequest.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, transaction.TransactionApprovalRequest.XP_ApprovalStatus);
				AssertNotEquals("New TransactionApprovalRequest is created", previousRequest.PK, transaction.TransactionApprovalRequest.PK);
				AssertEquals("previousRequest.XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, previousRequest.XP_ApprovalStatus);
				var expectedMessage = @"There is another request for this transaction. Only one request is permitted.

Do you want to cancel previous request and queue this one for approval?";
				AssertEquals("Cancel request", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConvertingUnallocatedTransactionWithGovernmentAllocatedID()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
				Factory.Save();

				using (var form = new TransactionPendingAllocationForm(transaction))
				{
					form.Show();
					Application.DoEvents();

					var governmentAllocatedIDTextBox = form.GetControl<ZTextBox>("AH_GovernmentAllocatedIDTextBox");
					governmentAllocatedIDTextBox.Text = "111-GOV-ID";

					form.FireSaveButton();

					var invoiceTransaction = (APInvoice)TransactionAllocationConverter.ConvertUnallocatedToAP(transaction).Invoice;

					AssertEquals(invoiceTransaction.AH_GovernmentAllocatedID, governmentAllocatedIDTextBox.Text);
				}
			}
		}

		public void TestDeleteAllocatedTransaction()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			Factory.Save();

			using (var form = new TransactionPendingAllocationForm(transaction))
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.Delete;
				form.Show();
				Application.DoEvents();
				AssertEquals("Delete", form.FormVerb);

				APInvoice invoiceTransaction = (APInvoice)TransactionAllocationConverter.ConvertUnallocatedToAP(transaction).Invoice;
				InvoicingLineBase line = (InvoicingLineBase)invoiceTransaction.Lines.AddNew();
				line.AL_OSExTaxAmount = invoiceTransaction.AH_OSExTaxAmount;
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				invoiceTransaction.Factory.Save();

				var expectedErrorMessage = "This transaction has been allocated and cannot be deleted.";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var postingButtonsUserControl = form.GetControl<ZPostingButtonsUserControl>("postingButtonsUserControl");
				AssertEquals("Should show delete button", true, postingButtonsUserControl.SaveAndCloseButton.Visible);
				AssertEquals("&Delete", postingButtonsUserControl.SaveAndCloseButton.Text);
				postingButtonsUserControl.SaveAndCloseButton.PerformClick();
				AssertEquals(expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2015, 1, 5)]
		public void TestExchangeRateINV()
		{
			ZArchitecture.Core.ExchangeRateReader.GetReaderInstance().ClearCache();

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 2m, new DateTime(2015, 1, 2), new DateTime(2015, 1, 2));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", 2m, new DateTime(2015, 1, 2), new DateTime(2015, 1, 2));

			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "BUY", 4m, new DateTime(2015, 1, 4), new DateTime(2015, 1, 4));
			TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, "SEL", 4m, new DateTime(2015, 1, 4), new DateTime(2015, 1, 4));

			PostingExRateRegistryAP.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			Registry.Business.AccountingConfigurationRegistry.Instance.FallBackToPreviousExchangeRate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			var transaction = Factory.New<TransactionPendingAllocation>();
			using (var form = new TransactionPendingAllocationForm(transaction))
			{
				form.DisplayMode = ZArchitecture.Core.ODisplayMode.New;
				form.Show();
				transaction.AH_InvoiceDate = new DateTime(2015, 1, 3);
				transaction.ExchangeRate.Currency = "USD";
				AssertEquals(2m, transaction.ExchangeRate.Rate);
			}
		}

		public void TestPlaceOfSupplyDropEditVisiblity()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				using (var testForm = GetFormToBash())
				{
					testForm.Show();
					var placeOfSupplyDropEdit = testForm.GetControl<ZDropEdit>("PlaceOfSupplyDropEdit");
					Assert(placeOfSupplyDropEdit.Visible);
				}
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				using (var testForm = GetFormToBash())
				{
					testForm.Show();
					var placeOfSupplyDropEdit = testForm.GetControl<ZDropEdit>("PlaceOfSupplyDropEdit");
					Assert(!placeOfSupplyDropEdit.Visible);
				}
			}
		}

		public void TestAH_GovernmentAllocatedIDTextBoxVisibility()
		{
			var allocatedNumberRegistry = AccountingMasterFilesRegistry.Instance.EnableGovernmentAllocatedNumberBehavior;
			using (allocatedNumberRegistry.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				using (var testForm = GetFormToBash())
				{
					testForm.Show();
					var governmentAllocatedIDTextBox = testForm.GetControl<ZTextBox>("AH_GovernmentAllocatedIDTextBox");
					Assert(governmentAllocatedIDTextBox.Visible);
				}
			}
			using (allocatedNumberRegistry.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				using (var testForm = GetFormToBash())
				{
					testForm.Show();
					var governmentAllocatedIDTextBox = testForm.GetControl<ZTextBox>("AH_GovernmentAllocatedIDTextBox");
					Assert(!governmentAllocatedIDTextBox.Visible);
				}
			}
		}

		protected override Form GetFormToBashCore()
		{
			TransactionPendingAllocation transaction = Factory.New<TransactionPendingAllocation>();
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction, "some xml", true);
			Assert("Precondition: HasUniversalTransaction", transaction.IsImportedFromUniversalXML);
			var form = new TransactionPendingAllocationForm(transaction) { ControllerID = ControllerIDs.TransactionsPendingAllocation };

			return form;
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;
	}
}
