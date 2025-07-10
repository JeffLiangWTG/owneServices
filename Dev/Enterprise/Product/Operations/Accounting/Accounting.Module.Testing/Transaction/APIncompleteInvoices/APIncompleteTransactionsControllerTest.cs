using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Accounting.Module.Transaction.Base;
using Enterprise.Accounting.Module.Transaction.Base.Testing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.Transaction.Testing
{
	public abstract class APIncompleteTransactionsControllerTest<TBusinessObject, TForm> : TransactionControllerWithReadOnlyBehaviourControlledBySourceTest
			where TBusinessObject : InvoicingBase
			where TForm : BaseInvoicingForm
	{
		public override void TestNewForm()
		{
			Assert("Creating new transactions is not supported.", true);
		}

		public override void TestTemplateCopyForm()
		{
			Assert("Copying incomplete transactions is not supported.", true);
		}

		public void TestGetForm()
		{
			using (IZForm form = Controller.ShowEditForm(GetBusinessObjectThatIsInTheDatabase()))
			{
				AssertEquals(typeof(TForm), form.GetType());
				var bizo = (InvoicingBase)((ZForm)form).BusinessEntity;
				AssertEquals(true, bizo.SubmittedFromInvoicingForm);
				AssertGetFormBisoFactory(bizo);
			}
		}

		public virtual void TestGetForm_ReturnsNullForm_WhenRestoreSavedDataReturnsFailed()
		{
			var invoice = GetBusinessObjectThatIsInTheDatabase();

			var invoiceInNewFactory = new BusinessObjectFactory().Load<InvoicingBase>(invoice.PK);

			Factory.LoadTop1<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, invoice.PK).AddToFilter(StmNoteSchema.ST_Table, invoice.TableName)).Delete();
			Factory.Save();

			using (IZForm form = Controller.ShowEditForm(invoiceInNewFactory))
			{
				AssertNull(form);
				AssertEquals("Post-condition", "There is a problem with the transaction and this transaction can no longer be used. You will need to delete this transaction from your system", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGetForm_ReportForTransactionWithoutLineError()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			Factory.Save();

			var incompleteInvoiceWithoutLine = Factory.NewWithValidTestData<TBusinessObject>();
			incompleteInvoiceWithoutLine.AH_InvoiceAmount = 100M;
			incompleteInvoiceWithoutLine.AH_OSTotalAmount = 100M;
			incompleteInvoiceWithoutLine.AH_OH = TestObjectCreator.Creditor1.PK;
			SaveAsIncomplete(incompleteInvoiceWithoutLine);

			AssertEquals(0, incompleteInvoiceWithoutLine.Lines.Count);
			AssertEquals(0, ErrorReporter.TotalErrorCount);

			using (IZForm form = Controller.ShowEditForm(incompleteInvoiceWithoutLine))
			{
				AssertEquals(typeof(TForm), form.GetType());
				AssertEquals("Incomplete transaction must have transaction lines", ErrorReporter.LastMessageReported);
			}

			ErrorReporter.Clear();
		}

		public void TestGetForm_TransactionRestoreSavedDataError()
		{
			ErrorReporter.Clear();

			var shipment = TestObjectCreator.CreateShipment("S00001", true);
			var job = TestObjectCreator.CreateJob(shipment, createWithMutex: false);
			var apInvoice = TestObjectCreator.CreateInvoice(typeof(APInvoice), "AP001");
			var line1 = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1, 100m);
			var line2 = TestObjectCreator.CreateInvoiceLine(apInvoice, TestObjectCreator.AUD, 1, 200m);
			line2.AL_JH = job.PK;
			line2.GenericCharge = TestObjectCreator.FREEVAT.PK;
			apInvoice.SaveAsIncomplete();

			var newShipment = NewFactory().Load<ForwardingShipment>(shipment.PK);
			var newJob = TestObjectCreator.CreateJob(newShipment, createWithMutex: true, newFactory: newShipment.Factory);

			var incompleteInvoice = NewFactory().Load<InvoicingBase>(apInvoice.PK);
			AssertNotNull(incompleteInvoice);

			var form = Controller.ShowEditForm(incompleteInvoice);
			AssertNull(form);
			AssertEquals(0, ErrorReporter.TotalErrorCount);
			AssertEquals(0, incompleteInvoice.Lines.Count);

			newJob.Dispose();
		}

		public void TestResetPostDateWhenGetForm()
		{
			var invoice = (InvoicingBase)GetBusinessObjectThatIsInTheDatabase();
			invoice.AH_PostDate = ZDateTime.BrettsBirthday;
			invoice.SaveAsIncomplete();

			var newFactory = new BusinessObjectFactory();
			var loadedInvoice = newFactory.Load<APInvoice>(invoice.PK);

			if (!loadedInvoice.HasApprovalRequest)
			{
				Assert(!loadedInvoice.HasApprovalRequest);
				AssertEquals(ZDateTime.BrettsBirthday, loadedInvoice.AH_PostDate);

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				loadedInvoice.AH_PostDate = ZDateTime.BrettsBirthday;
				using (var form = Controller.ShowEditForm(loadedInvoice))
				{
					var bizo = (InvoicingBase)((ZForm)form).BusinessEntity;
					AssertEquals(ZDateTime.BrettsBirthday, bizo.AH_PostDate);
				}

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				loadedInvoice.AH_PostDate = ZDateTime.BrettsBirthday;
				using (var form = Controller.ShowEditForm(loadedInvoice))
				{
					var bizo = (InvoicingBase)((ZForm)form).BusinessEntity;
					AssertEquals(ZDateTime.Today, bizo.AH_PostDate);
				}

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
				loadedInvoice.AH_PostDate = ZDateTime.BrettsBirthday;
				using (var form = Controller.ShowEditForm(loadedInvoice))
				{
					var bizo = (InvoicingBase)((ZForm)form).BusinessEntity;
					AssertEquals(ZDateTime.Today, bizo.AH_PostDate);
				}

				AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				Env.Security.PayablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
				loadedInvoice.AH_PostDate = ZDateTime.BrettsBirthday;
				using (var form = Controller.ShowEditForm(loadedInvoice))
				{
					var bizo = (InvoicingBase)((ZForm)form).BusinessEntity;
					AssertEquals(ZDateTime.Today, bizo.AH_PostDate);
				}
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual void AssertGetFormBisoFactory(InvoicingBase bizo)
		{
			AssertNotEquals("A form should open bizo in new factory", Factory._Instance, bizo.Factory._Instance);
		}

		#region TestRestoreSavedDataHasShowErrorHandler

		public void TestRestoreSavedDataHasShowErrorHandler()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			JobHeader testJob = newFactory.NewJobWithValidTestDataForTesting<JobHeader>();
			newFactory.Save();

			var invoice = GetBusinessObject();
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AH = invoice.PK;
			line.AL_JH = testJob.PK;
			SaveAsIncomplete(invoice);
			AssertEquals("AH_JH is not set", ZGuid.Empty, invoice.AH_JH);
			string jobNumber = testJob.JH_JobNum;
			testJob.MarkAsInactive();
			newFactory.Save();

			using (IZForm form = Controller.ShowEditForm(invoice))
			{
				AssertEquals("The form should be shown", form, Controller.LastShownForm);
				AssertJobErrorInTestRestoreSavedDataHasShowErrorHandler(jobNumber);
			}
		}

		public void TestIncompleteTransactionDataWithInactiveJob()
		{
			var newFactory = new BusinessObjectFactory();
			var testJob = newFactory.NewJobWithValidTestDataForTesting<JobHeader>();
			newFactory.Save();

			var invoice = GetBusinessObject();
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AH = invoice.PK;
			line.AL_JH = testJob.PK;
			SaveAsIncomplete(invoice);
			AssertEquals("AH_JH is not set", ZGuid.Empty, invoice.AH_JH);
			var jobNumber = testJob.JH_JobNum;
			testJob.MarkAsInactive();
			newFactory.Save();

			using (IZForm form = Controller.ShowEditForm(invoice))
			{
				AssertEquals("The form should be shown", form, Controller.LastShownForm);
				AssertJobErrorInTestRestoreSavedDataHasShowErrorHandler(jobNumber);
			}
		}

		protected virtual void AssertJobErrorInTestRestoreSavedDataHasShowErrorHandler(string jobNumber)
		{
			AssertContains("Error notification", string.Format("Could not find job {0}", jobNumber), UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		public void TestShowEditForm()
		{
			InvoicingBase invoice = (InvoicingBase)GetBusinessObjectThatIsInTheDatabase();

			using (IZForm form = Controller.ShowEditForm(invoice))
			{
				AssertEquals("The form should be shown", form, Controller.LastShownForm);
			}

			invoice.IsCancelled = true;
			Factory.Save();

			AssertNull("No form should be shown", Controller.ShowEditForm(invoice));
			AssertEquals("Information message", "This transaction is canceled and cannot be modified.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestIncompleteInvoiceShowEditForm_ShouldPreventCreateCreditNote_TurnedOff()
		{
			using (AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				InvoicingBase invoice = (InvoicingBase)GetBusinessObjectThatIsInTheDatabase();

				using (IZForm form = Controller.ShowEditForm(invoice))
				{
					AssertNotNull(form);
				}
			}
		}

		public void TestIncompleteInvoiceShowEditForm_ShouldPreventCreateCreditNote_TurnedOn()
		{
			InvoicingBase invoice = (InvoicingBase)GetBusinessObjectThatIsInTheDatabase();
			if (invoice.AH_Ledger == LedgerTypes.IncompleteTransactions && invoice.AH_TransactionType == TransactionTypes.IncompleteCreditNote)
			{
				using (AccountingMasterFilesRegistry.Instance.PayablePreventCreationOfCreditNotes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					using (IZForm form = Controller.ShowEditForm(invoice))
					{
						AssertNull("No form should be shown", form);
						string preventCreationOfCreditNoteMessage = "This transaction cannot be modified as Posting of Credit Notes is prevented. This is controlled by the registry setting Accounting -> Payable Defaults -> Default Settings -> Prevent Creation of Credit Notes.";
						AssertEquals("Information message", preventCreationOfCreditNoteMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}
			else
			{
				Assert("Not for this ledger and transaction type", true);
			}
		}

		protected override InvoicingBase TransactionImportedFromUniversalXML
		{
			get
			{
				var isInvoiceTesting = typeof(TBusinessObject) == typeof(APInvoice);
				var multiplier = isInvoiceTesting ? 1 : -1;
				var transactionPendingAllocation = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 60 * multiplier);
				var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
				request.Initialize(transactionPendingAllocation, "some XML", false);
				Factory.Save();
				var convertedInvoice = TransactionAllocationConverter.ConvertUnallocatedToAP(transactionPendingAllocation).Invoice;
				var line = (InvoicingLineBase)convertedInvoice.Lines.AddNew();
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.FillWithValidTestData();
				convertedInvoice.SaveAsIncomplete();
				AssertType("Precondition: we are testing expected invoice type.", typeof(TBusinessObject), convertedInvoice);
				Assert("Precondition: HasUniversalTransaction", convertedInvoice.IsImportedFromUniversalXML);
				return convertedInvoice;
			}
		}

		public virtual void TestShowCancelForm()
		{
			InvoicingBase invoice = (InvoicingBase)GetBusinessObjectThatIsInTheDatabase();

			APIncompleteTransactionsController controller = Controller as APIncompleteTransactionsController;
			AssertNotNull("Should be APIncompleteTransactionsController", controller);

			using (IZForm form = controller.ShowCancelForm(invoice))
			{
				AssertEquals("The form should be shown", form, Controller.LastShownForm);
				AccountingZForm accForm = form as AccountingZForm;
				AssertNotNull("Should be AccountingZForm", accForm);

				Assert("CancelInsteadOfDelete", accForm.CancelInsteadOfDelete);
				Assert("Form Text should start with Cancel", accForm.Text.StartsWith("Cancel"));

				IPostingButtonsProvider btnForm = form as IPostingButtonsProvider;
				AssertNotNull("Should be IPostingButtonsProvider", btnForm);
				AssertEquals("CommandButtonPost.Text", "&Cancel", btnForm.CommandButtonPost.Text);
				AssertEquals("CommandButtonCancel.Text", "C&lose", btnForm.CommandButtonCancel.Text);
			}

			Env.Security.APIncompleteInvoicesCancel.IsAllowed = false;
			using (IZForm form = controller.ShowCancelForm(invoice))
			{
				AssertNull("No form should be shown", form);
				AssertContains("Error Message", "You do not have the appropriate security rights to run this function.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Env.Security.APIncompleteInvoicesCancel.IsAllowed = true;
			using (IZForm form = controller.ShowCancelForm(invoice))
			{
				AssertNotNull("The form should be shown", form);
			}

			invoice.IsCancelled = true;
			Factory.Save();

			AssertNull("No form should be shown", controller.ShowCancelForm(invoice));
			AssertEquals("Information message", "This transaction is already canceled.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowDeleteForm()
		{
			InvoicingBase invoice = (InvoicingBase)GetBusinessObjectThatIsInTheDatabase();

			invoice.IsCancelled = true;
			Factory.Save();
			APIncompleteTransactionsController controller = Controller as APIncompleteTransactionsController;
			AssertNotNull("Should be APIncompleteTransactionsController", controller);

			controller.ShowDeleteForm(invoice);

			AssertNull("No form should be shown", controller.ShowDeleteForm(invoice));
			AssertEquals("Error Message", "Canceled incomplete invoices cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);

			InvoicingBase invoice2 = (InvoicingBase)GetBusinessObjectThatIsInTheDatabase();

			controller.ShowDeleteForm(invoice2);
			AssertNotNull("Form Should be shown", controller.ShowDeleteForm(invoice2));
		}

		public void TestShowDeleteForm_ActivatesCurrentViewFrom_WhenDeleteActionIsTriggered()
		{
			AssertSwitchingToCurrentAPIncompleteTransactionFormDoesNotChangeDisplayMode((ctrl1, inv) => ctrl1.ShowViewForm(inv), (ctrl1, inv) => ctrl1.ShowDeleteForm(inv));
		}

		public void TestShowDeleteForm_ActivatesCurrentEditForm_WhenDeleteActionTriggered()
		{
			AssertSwitchingToCurrentAPIncompleteTransactionFormDoesNotChangeDisplayMode((ctrl1, inv) => ctrl1.ShowEditForm(inv), (ctrl1, inv) => ctrl1.ShowDeleteForm(inv));		
		}

		public void TestShowDeleteForm_ActivatesCurrentCancelForm_WhenDeleteActionTriggered()
		{
			AssertSwitchingToCurrentAPIncompleteTransactionFormDoesNotChangeDisplayMode((ctrl1, inv) => ctrl1.ShowCancelForm(inv), (ctrl1, inv) => ctrl1.ShowDeleteForm(inv));
		}

		public void TestShowCancelForm_ActivatesCurrentViewForm_WhenCancelActionIsTriggered()
		{
			AssertSwitchingToCurrentAPIncompleteTransactionFormDoesNotChangeDisplayMode((ctrl1, inv) => ctrl1.ShowViewForm(inv), (ctrl1, inv) => ctrl1.ShowCancelForm(inv));
		}

		public void TestShowCancelForm_ActivatesCurrentEditForm_WhenCancelActionTriggered()
		{
			AssertSwitchingToCurrentAPIncompleteTransactionFormDoesNotChangeDisplayMode((ctrl1, inv) => ctrl1.ShowEditForm(inv), (ctrl1, inv) => ctrl1.ShowCancelForm(inv));
		}

		public void TestShowCancelForm_ActivatesCurrentDeleteForm_WhenCancelActionTriggered()
		{
			AssertSwitchingToCurrentAPIncompleteTransactionFormDoesNotChangeDisplayMode((ctrl1, inv) => ctrl1.ShowDeleteForm(inv), (ctrl1, inv) => ctrl1.ShowCancelForm(inv));
		}

		public void TestShowDeleteForm_ActivatesCurrentAPForm_WhenDeleteActionTriggered()
		{
			AssertSwitchingToOpenNewAPInvoiceFormSavedAsIncomplete((ctrl1, inv) => ctrl1.ShowDeleteForm(inv));
		}

		public void TestShowCancelForm_ActivatesCurrentAPForm_WhenCancelActionTriggered()
		{
			AssertSwitchingToOpenNewAPInvoiceFormSavedAsIncomplete((ctrl1, inv) => ctrl1.ShowCancelForm(inv));
		}

		public void TestActivateCurrentEditFormInsteadOfCreateNewOne()
		{
			var dataSource = (InvoicingBase)GetBusinessObjectThatIsInTheDatabase();
			var controller1 = GetController();

			if (controller1 != null)
			{
				foreach (var controllerIdToTest in EditFormControllerIDs)
				{
					var controller2 = ZControllerFactory.Create(controllerIdToTest);
					AssertNotNull(controller2);

					AssertActivateCurrentEditFormInsteadOfCreateNewOne(dataSource, controller1, controller2);
					AssertActivateCurrentEditFormInsteadOfCreateNewOne(dataSource, controller2, controller1);
				}

				var newTransactionController = ZControllerFactory.Create(NewFormControllerID);
				AssertNotNull(newTransactionController);
				AssertActivateCurrentEditFormInsteadOfCreateNewOne(dataSource, newTransactionController, controller1); //new approval controller can only be the first controller
			}
			else
			{
				Assert("Not supported", true);
			}
		}

		protected virtual void AssertSwitchingToCurrentAPIncompleteTransactionFormDoesNotChangeDisplayMode(Func<APIncompleteTransactionsController, InvoicingBase, IZForm> showForm1, Func<APIncompleteTransactionsController, InvoicingBase, IZForm> showForm2)
		{
			if (Controller != null)
			{
				var invoice = GetBusinessObject();
				invoice.SaveAsIncomplete();

				var controller2 = ZControllerFactory.Create(Controller.ID) as APIncompleteTransactionsController;

				AssertNotNull(controller2);

				using (var form1 = showForm1(Controller as APIncompleteTransactionsController, invoice))
				{
					AssertEquals("The form should be shown", form1, Controller.LastShownForm);
					var displayMode = form1.DisplayMode;

					// Display form2 and verify it has the same display mode as form1
					using (var form2 = showForm2(controller2, invoice))
					{
						AssertNotNull(form2);
						AssertEquals(form1, form2);
						AssertEquals(displayMode, form2.DisplayMode);
					}
				}
			}
		}

		protected virtual void AssertSwitchingToOpenNewAPInvoiceFormSavedAsIncomplete(Func<APIncompleteTransactionsController, InvoicingBase, IZForm> showForm)
		{
			if (Controller != null)
			{
				var relativeId = GetRelativeControllerID();
				var relativeController = ZControllerFactory.Create(relativeId);

				AssertNotNull(relativeController);

				using (var invoiceForm = (ZForm)relativeController.ShowNewForm())
				{
					var apInvoice = (InvoicingBase)invoiceForm.BusinessEntity;
					apInvoice.FillWithValidTestData();
					apInvoice.SaveAsIncomplete();

					AssertEquals(ODisplayMode.New, invoiceForm.DisplayMode);

					using (var form = showForm(Controller as APIncompleteTransactionsController, apInvoice))
					{
						AssertNotNull(form);
						AssertEquals(ODisplayMode.New, form.DisplayMode);
						AssertEquals(invoiceForm, form);
						AssertEquals("There should be only one form!", 1, OpenedFormCache.GetInstance().Count);
					}
				}
			}
		}

		protected virtual ZController GetController() => null;

		protected virtual IEnumerable<ControllerID> EditFormControllerIDs => Enumerable.Empty<ControllerID>();

		protected virtual ControllerID NewFormControllerID => null;

		void AssertActivateCurrentEditFormInsteadOfCreateNewOne(InvoicingBase dataSource, ZController controller1, ZController controller2)
		{
			using (var form1 = controller1.ShowEditForm(dataSource))
			{
				AssertNotNull("Edit form should be created successfully!");
				using (var form2 = controller2.ShowEditForm(dataSource))
				{
					AssertEquals("No duplicated edit form created.", form1, form2);
					AssertEquals("There should be only one form!", 1, OpenedFormCache.GetInstance().Count);
				}
			}
		}

		public virtual void TestGetFormCoreThroughProcessTask_NoException()
		{
			try
			{
				var invoice = (InvoicingBase)GetBusinessObjectThatIsInTheDatabase();
				((IWorkflowProvider)invoice).WorkflowItems.Tasks.AddNew();
				invoice.SaveAsIncomplete();
				using (var processTasks = new ProcessTasksModule())
				{
					processTasks.ModuleDecisionProvider.HandleDefaultAction(((IWorkflowProvider)invoice).WorkflowItems.ToArray());
					var form = ZApplication.GetOpenForms().OfType<TForm>().FirstOrDefault();
					AssertNotNull(form);
				}
			}
			finally
			{
				foreach (Form form in ZApplication.GetOpenForms().OfType<TForm>())
				{
					form.Dispose();
				}
			}
		}

		public virtual void TestGetFormCoreThroughProcessTask_NoEditSecurity()
		{
			Env.Security.APIncompleteInvoicesEdit_DirectEntered.IsAllowed = false;
			Env.Security.APIncompleteInvoicesEdit.IsAllowed = false;

			try
			{
				var invoice = (InvoicingBase)GetBusinessObjectThatIsInTheDatabase();
				((IWorkflowProvider)invoice).WorkflowItems.Tasks.AddNew();
				invoice.SaveAsIncomplete();
				using (var processTasks = new ProcessTasksModule())
				{
					processTasks.ModuleDecisionProvider.HandleDefaultAction(((IWorkflowProvider)invoice).WorkflowItems.ToArray());
					var form = ZApplication.GetOpenForms().OfType<TForm>().FirstOrDefault();
					AssertNotNull(form);
					AssertEquals(ODisplayMode.ReadOnly, form.DisplayMode);
				}
			}
			finally
			{
				foreach (Form form in ZApplication.GetOpenForms().OfType<TForm>())
				{
					form.Dispose();
				}
			}
		}

		#region INavigationControllerIDProvider

		public virtual void TestGetValidControllerID()
		{
			var controllerId = GetControllerID();
			var relativeId = GetRelativeControllerID();
			var provider = ZControllerFactory.Create(controllerId) as INavigationControllerIDProvider;

			var invoice = GetBusinessObjectThatIsInTheDatabase();
			var relativeInvoice = GetBusinessObject();
			var invoicingBase = invoice as InvoicingBase;
			var relativeLedger = invoicingBase != null ? invoicingBase.AH_Ledger.ToString() : invoice.GetType().Name;
			var nonRelatedBizO = Factory.NewWithValidTestData<ARInvoice>();

			AssertEquals(string.Format(CultureInfo.InvariantCulture, "ControllerID for non-related bizO should be {0}.", controllerId.Name), controllerId, provider.GetValidControllerID(nonRelatedBizO));
			AssertEquals(string.Format(CultureInfo.InvariantCulture, "ControllerID for {0} transaction should be {1}.", relativeInvoice.AH_Ledger, relativeId.Name), relativeId, provider.GetValidControllerID(relativeInvoice));
			AssertEquals(string.Format(CultureInfo.InvariantCulture, "ControllerID for {0} transaction should be {1}.", relativeLedger, controllerId.Name), controllerId, provider.GetValidControllerID(invoice));

			AssertOtherGetValidControllerIDCase(provider);
		}

		protected virtual void AssertOtherGetValidControllerIDCase(INavigationControllerIDProvider controller)
		{
		}

		protected abstract ControllerID GetRelativeControllerID();

		public virtual void TestReportInvalidDataSource()
		{
			var dataSource = (InvoicingBase)GetBusinessObject();
			Factory.Save();

			var controller = Controller as APIncompleteTransactionsController;
			using (var form = controller.GetForm_ForTest(dataSource))
			{
				var expectedErrorMessage = string.Format(CultureInfo.InvariantCulture, @"Message: ZController {5} is handling an invalid bizO {0}. The correct controller ID should be {1}
AH_TransactionType: {2}
AH_Ledger: {3}
Details:
Header: PK = {4}", dataSource.GetType().ToString(), GetRelativeControllerID(), dataSource.AH_TransactionType, dataSource.AH_Ledger, dataSource.PK.ToString(), GetControllerID());
				AssertEquals("Should have a developer notification exception.", 1, ExceptionReporterTestListener.Instance.Count);
				Assert("The developer notification exception should be about using invalid controller.", ExceptionReporterTestListener.Instance[0].InnerException.Message.Contains(expectedErrorMessage));
				ExceptionReporterTestListener.Instance.Clear();
			}
		}

		public void TestShouldLoadBusinessObject()
		{
			var controller = Controller as APIncompleteTransactionsController;
			AssertEquals(true, controller.ShouldLoadBusinessObject);
		}

		#endregion

		#region Implementation

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get
			{
				if (transaction == null)
				{
					transaction = (InvoicingBase)GetBusinessObjectThatIsInTheDatabase();
				}

				return transaction;
			}
		}

		InvoicingBase transaction;

		protected override void TestShowForm_TransactionMustMatchCurrentLoginCompany(Action<TransactionControllerWithLoginCompanyCheck, BusinessObject> showFormDelegate)
		{
			transaction = GetBusinessObjectThatIsInTheDatabase() as InvoicingBase;
			base.TestShowForm_TransactionMustMatchCurrentLoginCompany(showFormDelegate);
		}

		protected virtual TBusinessObject GetBusinessObject()
		{
			var invoice = Factory.NewWithValidTestData<TBusinessObject>();
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.FillWithValidTestData();

			return invoice;
		}

		protected virtual void SaveAsIncomplete(TBusinessObject invoice)
		{
			invoice.SaveAsIncomplete();
		}

		protected override InvoicingBase ChangeStateOfParentTransactionHeaderRow()
		{
			var transaction = ParentTransactionHeaderRow as InvoicingBase;
			transaction.MoveFromIncompleteToPayableLedger();
			Factory.Save();
			AssertEquals(LedgerTypes.AccountsPayable, transaction.AH_Ledger);

			return transaction;
		}

		protected override string MessageToShowWhenControllerMismatchForBusinessEntity => "The AP Incomplete invoice cannot be displayed because another user has changed, deleted or posted the record. Closing and re-opening this window will refresh your data.";

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var invoice = GetBusinessObject();
			SaveAsIncomplete(invoice);

			return invoice;
		}

		#endregion
	}
}
