using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module.Transaction.Testing
{
	public abstract class APIncompleteTransactionsWithApprovalRequestsControllerTest<TBusinessObject, TForm> : APIncompleteTransactionsControllerTest<TBusinessObject, TForm>
			where TBusinessObject : InvoicingBase
			where TForm : BaseInvoicingForm
	{
		protected abstract string ExpectedTransactionType { get; }

		public void TestCheckPointForEdit_TransactionWithApprovalRequest()
		{
			var transaction = TransactionImportedFromUniversalXML_WithApprovalRequest;
			AssertCheckPointForEdit(transaction, Env.Security.APInvoiceApproval_Edit_ImportSourced);

			transaction = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<TBusinessObject>(TestObjectCreator.LocalClient, 100);
			Assert("Precondition: HasApprovalRequest", transaction.HasApprovalRequest);
			AssertCheckPointForEdit(transaction, Env.Security.APInvoiceApproval_Edit_DirectEntered);
		}

		public void TestShowLoadedForm_EditAction_TransactionWithApproval()
		{
			var transaction = TransactionImportedFromUniversalXML_WithApprovalRequest;
			AssertForShowLoadedForm_EditAction(transaction, Env.Security.APInvoiceApproval_Edit_ImportSourced_EditInvoiceHeader);

			transaction = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<TBusinessObject>(TestObjectCreator.LocalClient, 100);
			Assert("Precondition: HasApprovalRequest", transaction.HasApprovalRequest);
			AssertForShowLoadedForm_EditAction(transaction, Env.Security.APInvoiceApproval_Edit_DirectEntered_EditInvoiceHeader);
		}

		InvoicingBase TransactionImportedFromUniversalXML_WithApprovalRequest
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
				TestObjectCreator.CreateAndSaveApprovalRequestForInvoiceInNewFactoryAsInProduction(convertedInvoice);
				var line = (InvoicingLineBase)convertedInvoice.Lines.AddNew();
				line.AL_AG = TestObjectCreator.GLHeader1.PK;
				line.FillWithValidTestData();
				convertedInvoice.SaveAsIncomplete();
				AssertType("Precondition: we are testing expected invoice type.", typeof(TBusinessObject), convertedInvoice);
				Assert("Precondition: HasUniversalTransaction", convertedInvoice.IsImportedFromUniversalXML);
				Assert("Precondition: HasApprovalRequest", convertedInvoice.HasApprovalRequest);
				return convertedInvoice;
			}
		}

		public void TestShowEditFormForInvoiceWithApprovalRequest()
		{
			var invoice = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<TBusinessObject>(TestObjectCreator.LocalClient, 100);
			Env.Security.APInvoiceApproval_Edit_DirectEntered.IsAllowed = false;
			using (IZForm form = Controller.ShowEditForm(invoice))
			{
				AssertEquals("The form should be shown", form, Controller.LastShownForm);
				AssertEquals("form Text", "View Unapproved " + ExpectedTransactionType, ((ZForm)form).Text);
			}
			Env.Security.APInvoiceApproval_Edit_DirectEntered.IsAllowed = true;
			using (IZForm form = Controller.ShowEditForm(invoice))
			{
				AssertEquals("The form should be shown", form, Controller.LastShownForm);
				AssertEquals("form Text", "Edit Unapproved " + ExpectedTransactionType, ((ZForm)form).Text);
				Assert("Factory context is Request Editing", form.BusinessEntityForPersistingForm.Factory.HasContext(APInvoiceChargesApprovalRequest.Context.Editing));
				var invoiceInForm = form.BusinessEntityForPersistingForm as InvoicingBase;
				Assert("Restored invoice is shown", invoiceInForm.Lines.Count > 0);
			}

			var request = invoice.TransactionRelatedApprovalRequest;
			request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			request.Factory.Save();
			Env.Security.APInvoiceApproval_Post.IsAllowed = false;
			using (IZForm form = Controller.ShowEditForm(invoice))
			{
				AssertEquals("The form should be shown", form, Controller.LastShownForm);
				AssertEquals("form Text", "View Unapproved " + ExpectedTransactionType, ((ZForm)form).Text);
			}
			Env.Security.APInvoiceApproval_Post.IsAllowed = true;
			using (IZForm form = Controller.ShowEditForm(invoice))
			{
				AssertEquals("The form should be shown", form, Controller.LastShownForm);
				AssertEquals("form Text", "Post " + ExpectedTransactionType, ((ZForm)form).Text);
				Assert("Factory context is Request Posting", form.BusinessEntityForPersistingForm.Factory.HasContext(APInvoiceChargesApprovalRequest.Context.Posting));
				var invoiceInForm = form.BusinessEntityForPersistingForm as InvoicingBase;
				Assert("Restored invoice is shown", invoiceInForm.Lines.Count > 0);
			}
		}

		public void TestShowViewFormForInvoiceWithApprovalRequest()
		{
			var invoice = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<TBusinessObject>(TestObjectCreator.LocalClient, 100);
			using (IZForm form = Controller.ShowViewForm(invoice))
			{
				AssertEquals("The form should be shown", form, Controller.LastShownForm);
				var invoiceInForm = form.BusinessEntityForPersistingForm as InvoicingBase;
				Assert("Restored invoice is shown", invoiceInForm.Lines.Count > 0);
			}
		}

		public void TestShowCancelFormForInvoiceWithApprovalRequest()
		{
			var invoice = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<TBusinessObject>(TestObjectCreator.LocalClient, 100);
			APIncompleteTransactionsController incompleteInvoiceController = Controller as APIncompleteTransactionsController;
			AssertNull("No form should be shown", incompleteInvoiceController.ShowCancelForm(invoice));
			AssertEquals("Information message", "To cancel invoice with approval request you should cancel its approval request in Invoice Approval module.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestShowDeleteFormForInvoiceWithApprovalRequest()
		{
			var invoice = TestObjectCreator.CreateAPInvoiceWithApprovalRequest<TBusinessObject>(TestObjectCreator.LocalClient, 100);
			AssertNull("No form should be shown", Controller.ShowDeleteForm(invoice));
			AssertEquals("Information message", "Invoice with approval request can't be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestRestoreSavedDataWithNotFindGenericChargeError()
		{
			var incompleteInvoice = PrepareTestDataForDeleteOrCancelIncompleteInvoiceWhenContainsErrors();

			using (IZForm form = Controller.ShowViewForm(incompleteInvoice))
			{
				AssertEquals(@"This transaction cannot be viewed. Please Delete or Cancel this record.
This transaction contains obsolete data that cannot be successfully restored.
Could not find generic charge ZZABC", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (IZForm form = Controller.ShowEditForm(incompleteInvoice))
			{
				AssertEquals(@"This transaction cannot be edited. Please Delete or Cancel this record.
This transaction contains obsolete data that cannot be successfully restored.
Could not find generic charge ZZABC", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowDeleteFormWithNotFindGenericChargeError()
		{
			var incompleteInvoice = PrepareTestDataForDeleteOrCancelIncompleteInvoiceWhenContainsErrors();

			Assert(!incompleteInvoice.IsDeleted);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			using (IZForm form = Controller.ShowDeleteForm(incompleteInvoice))
			{
				Assert(@"This transaction cannot be viewed before deletion.
This transaction contains obsolete data that cannot be successfully restored.
Do you want to proceed with the deletion of this transaction?
Could not find generic charge ZZABC", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}

			Assert(incompleteInvoice.IsDeleted);
		}

		public void TestShowCancelFormWithNotFindGenericChargeError()
		{
			var incompleteInvoice = PrepareTestDataForDeleteOrCancelIncompleteInvoiceWhenContainsErrors();

			Assert(!incompleteInvoice.IsCancelled);

			APIncompleteTransactionsController controller = Controller as APIncompleteTransactionsController;
			AssertNotNull("Should be APIncompleteTransactionsController", controller);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			using (IZForm form = controller.ShowCancelForm(incompleteInvoice))
			{
				Assert(@"This transaction cannot be viewed before cancellation.
This transaction contains obsolete data that cannot be successfully restored.
Do you want to proceed with the cancellation of this transaction?
Could not find generic charge ZZABC", UnitTestUserNotification.Instance.LastMessage.WasQuestion);
			}

			Assert(incompleteInvoice.IsCancelled);
		}

		public virtual InvoicingBase PrepareTestDataForDeleteOrCancelIncompleteInvoiceWhenContainsErrors()
		{
			var businessObjectFactory = new BusinessObjectFactory();
			var job = businessObjectFactory.NewJobWithValidTestDataForTesting<JobHeader>();
			businessObjectFactory.Save();

			var invoice = GetBusinessObject();
			var line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AH = invoice.PK;
			line.AL_JH = job.PK;
			var chargeCode = TestObjectCreator.CreateChargeCode("ABC");
			line.GenericCharge = chargeCode.PK;
			SaveAsIncomplete(invoice);

			var incompleteInvoice = NewFactory().Load<InvoicingBase>(invoice.PK);
			AssertNotNull(incompleteInvoice);

			var chargeCode2 = businessObjectFactory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode.AC_Code));
			chargeCode2.Delete();
			businessObjectFactory.Save();

			return incompleteInvoice;
		}

		public void TestRestoreSavedDataWithJobCreationException()
		{
			var anotherFactory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(anotherFactory);
			var consol = creator.CreateConsol("KRSEL", "AUSYD", "C1");
			var shipment1 = creator.CreateShipment("S1", consol);
			var job1 = creator.CreateJob(shipment1, false);
			anotherFactory.Save();

			var invoice = GetBusinessObject();
			var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_OSCostAmount = 200m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.SetIsUsedForApportionment();
			invoice.ImportAllApportionmentsFromCosting();
			SaveAsIncomplete(invoice);

			var shipment2 = creator.CreateShipment("S2", consol);
			anotherFactory.Save();

			var lockingFactory = new BusinessObjectFactory();
			var loader = new Job.Loader(lockingFactory, lockingFactory.Load<ForwardingShipment>(shipment2.PK));
			var job2 = loader.TryCreateWithMutex();

			var errorMessage = @"You have created the job S2 on another form, but haven't saved it yet.
Please close or save other forms that use job S2 to continue.";

			APIncompleteTransactionsController controller = Controller as APIncompleteTransactionsController;
			AssertNotNull("Should be APIncompleteTransactionsController", controller);

			try
			{
				using (IZForm form = controller.ShowViewForm(invoice))
				{
					AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (IZForm form = controller.ShowEditForm(invoice))
				{
					AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (IZForm form = controller.ShowDeleteForm(invoice))
				{
					AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}

				using (IZForm form = controller.ShowCancelForm(invoice))
				{
					AssertEquals(errorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				job2.Dispose();
			}
		}
	}
}
