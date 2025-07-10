using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	abstract class TransactionApprovalModuleTest<TransactionType, RequestType, DetailsType> : ZModuleBasherTest
		where TransactionType : TransactionHeader
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		public void TestApprovingERRRequestShowsEditForm()
		{
			var action = "Approve";
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var request = GetNewApprovalRequest("1");
					request.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Error;
					request.RunPreSaveValidation();
					AssertNoErrors(request);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];
					grid.SelectAllElements();

					var menuItem = module.FormActionMenu.FindByText(action, true);
					AssertNotNull(menuItem);
					menuItem.PerformClick();

					var expectedCantApproveErrorMessage = string.Format("Can't {0} - These approvals are NOT in allowed REQ status: {1}", action, request.ReferenceID);
					var expectedEditRequest = @"One or more of the selected requests have a status of 'ERR'. This means the transaction pending allocation has errors that must be corrected before the transaction can be approved for allocation. You must edit the related transaction pending allocation and save it. This will cancel the current request and create a new request with a valid approval status.

Do you want to edit the related transaction pending allocation now?";
					var cantApproveErrorMessageShown = UnitTestUserNotification.Instance.LastMessage.Text;
					if (IsRelatedTransactionEditSupported)
					{
						AssertEquals(expectedEditRequest, UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertEquals(expectedCantApproveErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					}
					AssertEquals(Constants.GenApprovalRequestApprovalStatus.Error, request.XP_ApprovalStatus);
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);

					if (IsRelatedTransactionEditSupported)
					{
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						menuItem.PerformClick();
						AssertEquals(expectedEditRequest, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals(Constants.GenApprovalRequestApprovalStatus.Error, request.XP_ApprovalStatus);
						AssertEditFormForRelatedTransaction(module);
					}
				}
			}
		}

		public virtual void TestApprove()
		{
			AssertStatusAction("Approve", Constants.GenApprovalRequestApprovalStatus.Approved, loginUnderAnotherUser: false);
		}

		public virtual void TestApprove_UnderAnotherUser()
		{
			AssertStatusAction("Approve", Constants.GenApprovalRequestApprovalStatus.Approved, loginUnderAnotherUser: true);
		}

		public virtual void TestApprove_IfCountryComplianceIsImplementedToEInvoicingRequestProvider()
		{
			AssertStatusAction("Approve", ExpectedApprovalStatus, loginUnderAnotherUser: false);
		}

		public virtual void TestApprove_IfCountryComplianceIsImplementedToEInvoicingRequestProvider_UnderAnotherUser()
		{
			AssertStatusAction("Approve", ExpectedApprovalStatus, loginUnderAnotherUser: true);
		}

		protected virtual string ExpectedApprovalStatus => Constants.GenApprovalRequestApprovalStatus.Approved;

		[TestDate(2016, 10, 20)]
		public virtual void TestApproveWithConcurrencySenario()
		{
			var moduleAction = "Approve";
			var expectedErrorMsgPart = "Can't Approve - These approvals are NOT in allowed REQ status: S001";
			TestBehaviorWithConcurrencySenarioCore(moduleAction, expectedErrorMsgPart);
		}

		public virtual void TestReject()
		{
			AssertStatusAction("Reject", Constants.GenApprovalRequestApprovalStatus.Rejected, loginUnderAnotherUser: false);
		}

		public virtual void TestReject_UnderAnotherUser()
		{
			AssertStatusAction("Reject", Constants.GenApprovalRequestApprovalStatus.Rejected, loginUnderAnotherUser: true);
		}

		public virtual void TestReject_IfCountryComplianceIsImplementedToEInvoicingRequestProvider()
		{
			AssertStatusAction("Reject", ExpectedRejectionStatus, loginUnderAnotherUser: false);
		}

		public virtual void TestReject_IfCountryComplianceIsImplementedToEInvoicingRequestProvider_UnderAnotherUser()
		{
			AssertStatusAction("Reject", ExpectedRejectionStatus, loginUnderAnotherUser: true);
		}

		protected virtual string ExpectedRejectionStatus => Constants.GenApprovalRequestApprovalStatus.Rejected;

		[TestDate(2016, 10, 20)]
		public virtual void TestRejectWithConcurrencySenario()
		{
			var moduleAction = "Reject";
			var expectedErrorMsg = "Can't Reject - These approvals are NOT in allowed REQ status: S001";
			TestBehaviorWithConcurrencySenarioCore(moduleAction, expectedErrorMsg);
		}

		public virtual void TestCancel()
		{
			AssertStatusAction("Cancel", Constants.GenApprovalRequestApprovalStatus.Cancelled, loginUnderAnotherUser: false);
		}

		public virtual void TestCancel_UnderAnotherUser()
		{
			AssertStatusAction("Cancel", Constants.GenApprovalRequestApprovalStatus.Cancelled, loginUnderAnotherUser: true);
		}

		[TestDate(2016, 10, 20)]
		public virtual void TestCancelWithConcurrencySenario()
		{
			var moduleAction = "Cancel";
			var expectedErrorMsg = "Can't Cancel - These approvals are already posted, canceled, or have unfinished Electronic Invoicing process: S001";
			TestBehaviorWithConcurrencySenarioCore(moduleAction, expectedErrorMsg);
		}

		protected void TestBehaviorWithConcurrencySenarioCore(string moduleAction, string expectedErrorMsg)
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);

			var approvalToAct = GetNewApprovalRequest("S001");
			approvalToAct.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			approvalToAct.RunPreSaveValidation();
			AssertNoErrors(approvalToAct);
			Factory.Save();
			AssertEquals("In CW1 instance1 there is a approval request with status REQ", Constants.GenApprovalRequestApprovalStatus.Requested, approvalToAct.XP_ApprovalStatus);

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];

					var menuItem = module.FormActionMenu.FindByText(moduleAction, true);
					AssertNotNull(menuItem);
					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);

					var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
					var approval = newFactory.Load<RequestType>(approvalToAct.PK);
					if (approval == null)
					{
						Assert("UT does not support this ApprovalRequest type.", true);
						return;
					}
					approval.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Posted;
					newFactory.Save();
					AssertEquals("In CW1 instance2, the transaction is successfully approved and post", Core.Constants.GenApprovalRequestApprovalStatus.Posted, approval.XP_ApprovalStatus);

					AssertEquals("In CW1 instance1, the transaction is still with REQ status", Core.Constants.GenApprovalRequestApprovalStatus.Requested, ((RequestType)module.GridCollection[0]).XP_ApprovalStatus);
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

					grid.SelectAllElements();
					menuItem.PerformClick();

					AssertEquals("Should not allow to " + moduleAction, expectedErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		protected void AssertStatusAction(string action, string expectedStatus, bool allowActionForOwnRequest = true, bool loginUnderAnotherUser = false)
		{
			var isCancelAction = expectedStatus == Constants.GenApprovalRequestApprovalStatus.Cancelled;
			var isApprovalRequestedAction = expectedStatus == Constants.GenApprovalRequestApprovalStatus.ApprovalRequested;
			var isApproveAction = expectedStatus == Constants.GenApprovalRequestApprovalStatus.Approved || isApprovalRequestedAction;
			var isRejectionRequestedAction = expectedStatus == Constants.GenApprovalRequestApprovalStatus.RejectionRequested;
			var isRejectAction = expectedStatus == Constants.GenApprovalRequestApprovalStatus.Rejected || isRejectionRequestedAction;

			AccountingMasterFilesRegistry.Instance.EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, isApprovalRequestedAction || isRejectionRequestedAction);
			var (eRequestProviderMock, _) = TestObjectCreator.MockCountryFactoryForTPAAeInvoicing(isTPAAeInvoicingProviderImplemented: isApprovalRequestedAction || isRejectionRequestedAction);

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(0, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];
					grid.SelectAllElements();

					var menuItem = module.FormActionMenu.FindByText(action, true);
					AssertNotNull(menuItem);

					menuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var bizo1 = GetNewApprovalRequest("1");
					var bizo2 = GetNewApprovalRequest("2");
					var bizo3 = GetNewApprovalRequest("3");
					var bizo4 = GetNewApprovalRequest("4");
					var bizo5 = GetNewApprovalRequest("5");
					var bizo6 = GetNewApprovalRequest("6");
					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
					bizo2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
					bizo3.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Posted;
					bizo4.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Error;
					bizo5.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.ApprovalRequested;
					bizo6.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.RejectionRequested;
					bizo1.RunPreSaveValidation();
					bizo2.RunPreSaveValidation();
					bizo3.RunPreSaveValidation();
					bizo4.RunPreSaveValidation();
					bizo5.RunPreSaveValidation();
					bizo6.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					AssertNoErrors(bizo2);
					AssertNoErrors(bizo3);
					AssertNoErrors(bizo4);
					AssertNoErrors(bizo5);
					AssertNoErrors(bizo6);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(6, module.GridCollection.Count);
					grid.SelectAllElements();
					menuItem.PerformClick();
					var expectedNotAllowedStatusErrorMessage = IsErrorStatusSupported && !isApproveAction ? "NOT in allowed (REQ, ERR) status" : "NOT in allowed REQ status";
					if (isCancelAction || (isRejectAction && IsErrorStatusSupported))
					{
						AssertContains(bizo1.ReferenceID, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertContains(bizo3.ReferenceID, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertContains(bizo5.ReferenceID, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertContains(bizo6.ReferenceID, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertContains(string.Format("Can't {0} - These approvals are {1}: ", action, isCancelAction ? "already posted, canceled, or have unfinished Electronic Invoicing process" : expectedNotAllowedStatusErrorMessage), UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						if (isApproveAction && IsRelatedTransactionEditSupported)
						{
							AssertEquals(@"One or more of the selected requests have a status of 'ERR'. This means the transaction pending allocation has errors that must be corrected before the transaction can be approved for allocation. You must edit the related transaction pending allocation and save it. This will cancel the current request and create a new request with a valid approval status.

Do you want to edit the related transaction pending allocation now?", UnitTestUserNotification.Instance.LastMessage.Text);
						}
						else
						{
							AssertContains(bizo1.ReferenceID, UnitTestUserNotification.Instance.LastMessage.Text);
							AssertContains(bizo3.ReferenceID, UnitTestUserNotification.Instance.LastMessage.Text);
							AssertContains(bizo4.ReferenceID, UnitTestUserNotification.Instance.LastMessage.Text);
							AssertContains(string.Format("Can't {0} - These approvals are {1}: ", action, expectedNotAllowedStatusErrorMessage), UnitTestUserNotification.Instance.LastMessage.Text);
						}
					}
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
					bizo3.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
					bizo4.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
					bizo5.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
					bizo6.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
					Factory.Save();

					if (isRejectionRequestedAction)
					{
						eRequestProviderMock.Setup(x => x.IsTransactionEligibleToCreateRejectionRequest(It.IsAny<AccTransactionHeader>())).Returns(false);

						grid.SelectAllElements();
						menuItem.PerformClick();
						expectedNotAllowedStatusErrorMessage = "Can't Reject - Transactions for these approvals are not eligible for rejecting or they have already created rejection request. You can reject only commercial invoices. Please Cancel these approval requests:";
						AssertContains(bizo1.ReferenceID, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertContains(bizo2.ReferenceID, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertContains(bizo3.ReferenceID, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertContains(bizo4.ReferenceID, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertContains(bizo5.ReferenceID, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertContains(bizo6.ReferenceID, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertContains(expectedNotAllowedStatusErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);

						eRequestProviderMock.Setup(x => x.IsTransactionEligibleToCreateRejectionRequest(It.IsAny<AccTransactionHeader>())).Returns(true);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}

					var user = loginUnderAnotherUser ? Factory.NewWithValidTestData<GlbStaff>() : GlbStaff.CurrentUser;
					Factory.Save();
					using (Env.SetTemporaryUserContext(user.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
					{
						SetupSecurity(allowSecurity: false);
						if (CheckpointForCancel != null)
						{
							CheckpointForCancel.IsAllowed = false;
						}
						grid.SelectAllElements();
						menuItem.PerformClick();
						var expectedMessage = string.Format("You don't have rights to {0} selected requests.", action.ToLower());
						if (isCancelAction)
						{
							if (!string.IsNullOrEmpty(ExpectedUserDoesntHaveCancelRightMessage))
							{
								AssertEquals(ExpectedUserDoesntHaveCancelRightMessage, UnitTestUserNotification.Instance.LastMessage.Text);
								AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo1.XP_ApprovalStatus);
								AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo2.XP_ApprovalStatus);
								AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo3.XP_ApprovalStatus);
								AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo4.XP_ApprovalStatus);
								AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo5.XP_ApprovalStatus);
								AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo6.XP_ApprovalStatus);
								UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

								CheckpointForCancel.IsAllowed = true;
								grid.SelectAllElements();
								menuItem.PerformClick();
							}
							expectedMessage = @"You can't cancel selected requests. 
A request can be canceled only by the user who either created it or has rights to approve it. Neither of those conditions were satisfied.";
						}

						if (!isCancelAction || loginUnderAnotherUser)
						{
							if (allowActionForOwnRequest)
							{
								AssertType(ExpectedLoginFormType(), ZFormModaliser.LastFormShownDialogForTest);
							}
							else
							{
								AssertNull(ZFormModaliser.LastFormShownDialogForTest);
							}
							AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo1.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo2.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo3.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo4.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo5.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo6.XP_ApprovalStatus);
							UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

							SetupSecurity(allowSecurity: true);
							grid.SelectAllElements();
							menuItem.PerformClick();
						}

						if (allowActionForOwnRequest || loginUnderAnotherUser || isCancelAction)
						{
							var lastForm = (ZForm)ZFormModaliser.LastFormShownForTest;
							AssertNotNull(lastForm);

							var approvalsInForm = ((TransactionApprovalBulk<TransactionType, RequestType, DetailsType>)lastForm.BusinessEntity).Approvals;
							if (isCancelAction)
							{
								foreach (var request in approvalsInForm)
								{
									Assert("Request should have CancelApprovalRequestByUser context set", request.HasContext(BusinessContext.CancelApprovalRequestByUser));
								}
							}
							AssertType(ApprovalBulklastFormType, lastForm);
							AssertEquals(action, lastForm.FormVerb);
							lastForm.FireSaveButton();
							lastForm.Dispose();
							AssertEquals(expectedStatus, bizo1.XP_ApprovalStatus);
							AssertEquals(expectedStatus, bizo2.XP_ApprovalStatus);
							AssertEquals(expectedStatus, bizo3.XP_ApprovalStatus);
							AssertEquals(expectedStatus, bizo4.XP_ApprovalStatus);
							AssertEquals(expectedStatus, bizo5.XP_ApprovalStatus);
							AssertEquals(expectedStatus, bizo6.XP_ApprovalStatus);

							if (isCancelAction)
							{
								foreach (var request in approvalsInForm)
								{
									Assert("Request should not have CancelApprovalRequestByUser context set anymore, since its already saved", !request.HasContext(BusinessContext.CancelApprovalRequestByUser));
								}
							}
						}
						else
						{
							AssertNull(ZFormModaliser.LastFormShownDialogForTest);
							AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo1.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo2.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo3.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo4.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo5.XP_ApprovalStatus);
							AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, bizo6.XP_ApprovalStatus);
						}
					}
				}
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		protected virtual Type ExpectedLoginFormType()
		{
			return typeof(MasterFiles.GUI.LoginForm);
		}

		protected abstract RequestType GetNewApprovalRequest(string jobNumber);

		protected abstract void SetupSecurity(bool allowSecurity);

		protected abstract Type ApprovalBulklastFormType { get; }

		protected abstract string ExpectedUserDoesntHaveCancelRightMessage { get; }
		protected abstract SecurityCheckpoint CheckpointForCancel { get; }

		protected virtual bool IsErrorStatusSupported
		{
			get { return false; }
		}

		protected virtual bool IsRelatedTransactionEditSupported
		{
			get { return false; }
		}

		protected virtual void AssertEditFormForRelatedTransaction(ZFilterModule module)
		{
			Assert(false);
		}
	}
}
