using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(TransactionPendingAllocationApprovalModule))]
	class TransactionPendingAllocationApprovalModuleTest : TransactionApprovalModuleTest<TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails>
	{
		protected override TransactionPendingAllocationApprovalRequest GetNewApprovalRequest(string jobNumber)
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation(jobNumber, TestObjectCreator.Creditor1, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction);
			request.XP_ReasonDescription = "Desc";
			request.XP_ReasonCode = "DAM";
			return request;
		}

		TransactionPendingAllocationApprovalRequest GetNewApprovalRequestForPostedTransaction(string jobNumber)
		{
			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var newObjectCreator = new TestObjectCreator(newFactory);
			var transaction = newObjectCreator.CreateTransactionPendingAllocation(jobNumber, TestObjectCreator.Creditor1, 100);
			var request = newFactory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction);
			request.XP_ReasonDescription = "Linked is Posted";
			request.XP_ReasonCode = "DAM";
			newFactory.Save();

			var (invoice, error) = TransactionAllocationConverter.ConvertUnallocatedToAP(request.LinkedTransaction);
			AssertNullOrEmpty(error);
			invoice.Factory.Save();

			return Factory.Load<TransactionPendingAllocationApprovalRequest>(request.PK);
		}

		[TestDate(2016, 10, 20)]
		public override void TestRejectWithConcurrencySenario()
		{
			var moduleAction = "Reject";
			// Pending Allocated transaction has more allowed status for rejection.
			var expectedErrorMsg = "Can't Reject - These approvals are NOT in allowed (REQ, ERR) status: S001";
			TestBehaviorWithConcurrencySenarioCore(moduleAction, expectedErrorMsg);
		}

		[TestDate(2016, 10, 20)]
		public void TestReject_ShowsValidationMessage_ForRequestedStatus_WithPostedTransaction()
			=> TestReject_ShowsValidationMessage_WithPostedTransaction(GenApprovalRequestApprovalStatus.Requested);

		[TestDate(2016, 10, 20)]
		public void TestReject_ShowsValidationMessage_ForErrorStatus_WithPostedTransaction()
			=> TestReject_ShowsValidationMessage_WithPostedTransaction(GenApprovalRequestApprovalStatus.Error);

		void TestReject_ShowsValidationMessage_WithPostedTransaction(string approvalStatus)
		{
			var request = GetNewApprovalRequestForPostedTransaction("1");
			request.XP_ApprovalStatus = approvalStatus;
			AssertEquals("Precondition: transaction must be posted.", true, request.LinkedTransaction.IsPosted);
			request.RunPreSaveValidation();
			AssertNoErrors(request);
			Factory.Save();

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (ZForm form = new ZForm())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: one approval request should be found", 1, module.GridCollection.Count);

				var grid = (ZGrid)module.EmbeddedControl.Controls.Find("FilteredGrid", true)[0];
				grid.SelectAllElements();

				var menuItem = module.FormActionMenu.FindByText("Reject", true);
				menuItem.PerformClick();

				AssertStartsWith("Rejecting an approval request for posted transaction should show an error popup message", "Can't Reject - Transactions for these approvals have already been posted. Please Cancel these approval requests:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval status should not change", approvalStatus, request.XP_ApprovalStatus);
				AssertNull("No form should be shown which might allow the user to reject the request", ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestCancel_ForRequestedStatus_WithPostedTransaction()
			=> TestCancel_WithPostedTransaction(GenApprovalRequestApprovalStatus.Requested);

		[TestDate(2016, 10, 20)]
		public void TestCancel_ForErrorStatus_WithPostedTransaction()
			=> TestCancel_WithPostedTransaction(GenApprovalRequestApprovalStatus.Error);

		void TestCancel_WithPostedTransaction(string approvalStatus)
		{
			var request = GetNewApprovalRequestForPostedTransaction("1");
			request.XP_ApprovalStatus = approvalStatus;
			AssertEquals("Precondition: transaction must be posted.", true, request.LinkedTransaction.IsPosted);
			request.RunPreSaveValidation();
			AssertNoErrors(request);
			Factory.Save();

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (ZForm form = new ZForm())
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals("Precondition: one approval request should be found", 1, module.GridCollection.Count);

				var grid = (ZGrid)module.EmbeddedControl.Controls.Find("FilteredGrid", true)[0];
				grid.SelectAllElements();

				var menuItem = module.FormActionMenu.FindByText("Cancel", true);
				menuItem.PerformClick();

				var lastForm = (ZForm)ZFormModaliser.LastFormShownForTest;
				AssertNotNull("Approval confirmation form should be shown when cancel action is clicked", lastForm);
				AssertType(ApprovalBulklastFormType, lastForm);
				AssertEquals("Cancel", lastForm.FormVerb);

				lastForm.FireSaveButton();
				lastForm.Dispose();

				AssertEquals("Approval status should be canceled", GenApprovalRequestApprovalStatus.Cancelled, request.XP_ApprovalStatus);
				AssertEquals("Transaction should not be canceled as it's already posted", false, request.LinkedTransaction.AH_IsCancelled);
			}
		}

		protected override void SetupSecurity(bool allowSecurity)
		{
			Env.Security.TransactionsPendingAllocationAllocate.IsAllowed = allowSecurity;
		}

		protected override Type ApprovalBulklastFormType
		{
			get { return typeof(TransactionPendingAllocationApprovalBulkForm); }
		}

		protected override string ExpectedUserDoesntHaveCancelRightMessage
		{
			get { return null; }
		}

		protected override SecurityCheckpoint CheckpointForCancel
		{
			get { return null; }
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.TransactionsPendingAllocationApproval;
		}

		protected override bool IsErrorStatusSupported
		{
			get { return true; }
		}

		protected override bool IsRelatedTransactionEditSupported
		{
			get { return true; }
		}

		protected override void AssertEditFormForRelatedTransaction(ZFilterModule module)
		{
			var moduleCasted = module as TransactionPendingAllocationApprovalModule;
			AssertNotNull("Precondition: module has correct type", moduleCasted);
			AssertNotNull("Controller should be created", moduleCasted.LastController_ForTestOnly);
			var lastForm = (ZForm)moduleCasted.LastController_ForTestOnly.LastShownForm;
			AssertNotNull(lastForm);
			AssertType<TransactionPendingAllocationForm>(lastForm);
			AssertEquals("Form is shown to Edit transaction", "Edit", lastForm.FormVerb);
			lastForm.Close();
		}

		protected override string ExpectedApprovalStatus => GenApprovalRequestApprovalStatus.ApprovalRequested;
		protected override string ExpectedRejectionStatus => GenApprovalRequestApprovalStatus.RejectionRequested;
	}
}
