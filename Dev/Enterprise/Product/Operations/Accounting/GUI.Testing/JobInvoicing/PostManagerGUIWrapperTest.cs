using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.GUI.JobInvoicing.AgentPostingOptionSelection;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestsSubclassesOf(typeof(PostManagerGUIWrapper))]
	public abstract class PostManagerGUIWrapper_PostingTransactionApprovalTest : TestCaseWithFactory
	{
		[TestDate(2005, 9, 10)]
		public virtual void TestApprovalRequestForPreviewActionShouldNotBeCreated()
		{
			SetupSecurity();
			var job = SetupJobData();
			var charge1 = SetupChargeData(job, 150m, TestObjectCreator.CC3);
			var charge2 = SetupChargeData(job, 150m, TestObjectCreator.CC7);
			var charge3 = SetupChargeData(job, 150m, TestObjectCreator.CC3, useAnotherOrg: true);
			Factory.Save();

			ZFormModaliser.LastFormShownDialogForTest = null;
			var guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Preview();
			Assert("IsAnyChargePosted", !IsAnyChargePosted(charge1, charge2, charge3));
			AssertType("LastFormShownDialogForTest", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest);
			var approvalCollection = LoadAllApprovalRequests();
			AssertEquals("Approval request.", 0, approvalCollection.Length);
			AssertNull("InvoicePreviewForm should not be shown as user doesn't have right to create invoices and so to print", ZFormModaliser.LastFormShownForTest);
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestApprovalRequestForPostingAction()
		{
			SetupSecurity();
			var job = SetupJobData();
			var charge1 = SetupChargeData(job, 150m, TestObjectCreator.CC3);
			var charge2 = SetupChargeData(job, 150m, TestObjectCreator.CC7);
			var charge3 = SetupChargeData(job, 150m, TestObjectCreator.CC3, useAnotherOrg: true);
			Factory.Save();

			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();
			Assert("IsAnyChargePosted", !IsAnyChargePosted(charge1, charge2, charge3));
			AssertType("LastFormShownDialogForTest", typeof(LoginFormWithRequest), ZFormModaliser.LastFormShownDialogForTest);
			Assert("No errors", !UnitTestUserNotification.Instance.LastMessage.WasError);
			var approvalCollection = LoadAllApprovalRequests();
			AssertEquals("Approval request.", 0, approvalCollection.Length);

			bool isLoginFormShown = false;
			ZFormModaliser.PreShowInvoker setLoginScreenToAskToCreateRequest = (form) =>
			{
				isLoginFormShown |= form.GetType() == typeof(LoginFormWithRequest);
				ZFormModaliser.ResultToReturnFromShowDialog = form.GetType() == typeof(LoginFormWithRequest) ? DialogResult.Ignore : DialogResult.OK;
			};
			ZFormModaliser.PreShowInvoker setLoginScreenToAskToCreateRequestAndCancelSummaryScreen = (form) =>
			{
				if (form.GetType() == typeof(APInvoicePostingWithApprovalRequestSummaryForm))
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				}
				else
				{
					setLoginScreenToAskToCreateRequest(form);
				}
			};
			ZFormModaliser.PreShowInvoker setLoginScreenToAskToCreateRequestAndContinueSummaryScreen = (form) =>
			{
				if (form.GetType() == typeof(APInvoicePostingWithApprovalRequestSummaryForm))
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				}
				else
				{
					setLoginScreenToAskToCreateRequest(form);
				}
			};
			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(setLoginScreenToAskToCreateRequest);
			ZFormModaliser.LastFormShownDialogForTest = null;
			guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();
			Assert("IsAnyChargePosted", !IsAnyChargePosted(charge1, charge2, charge3));
			AssertType("LastFormShownDialogForTest", LoginForType, ZFormModaliser.LastFormShownDialogForTest);
			Assert("No errors", !UnitTestUserNotification.Instance.LastMessage.WasError);
			approvalCollection = LoadAllApprovalRequests();
			GenApprovalRequest approvalRequest = null;
			GenApprovalRequest approvalRequest2 = null;
			ZGuid expectedParentID;
			ZString expectedParentTableCode;
			if (IsARCreditNoteTesting)
			{
				AssertEquals("Approval request.", 1, approvalCollection.Length);
				approvalRequest = approvalCollection[0];
				GetParentForPostingAction(job, out expectedParentID, out expectedParentTableCode);
			}
			else
			{
				AssertEquals("Approval request.", 2, approvalCollection.Length);
				approvalRequest = approvalCollection[0];
				approvalRequest2 = approvalCollection[1];
				expectedParentID = job.PK;
				expectedParentTableCode = job.TablePrefix;
			}
			string expectedPostingOption = nameof(JobInvoicingPostingOption.All);
			var expectedNumerOfChargesForRequest = IsARCreditNoteTesting ? new[] { 3 } : new[] { 2, 1 };
			var expectedAmounts = IsARCreditNoteTesting ? new[] { 300m } : new[] { 300m, 150m };
			var expectedCreditors = new[] { "ZCreditor1", "ZCreditor2" };
			AssertApprovalRequest(approvalCollection, expectedParentID, expectedParentTableCode, Constants.GenApprovalRequestApprovalStatus.Requested, expectedPostingOption, expectedAmounts, expectedNumerOfChargesForRequest, expectedCreditors);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			isLoginFormShown = false;
			if (IsARCreditNoteTesting)
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			}
			else
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(setLoginScreenToAskToCreateRequestAndCancelSummaryScreen);
			}
			ZFormModaliser.LastFormShownDialogForTest = null;
			guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();
			Assert("IsAnyChargePosted", !IsAnyChargePosted(charge1, charge2, charge3));
			string expectedMessage = string.Format(@"There is another request for this {0}. Only one request is permitted.

Do you want to cancel previous request and queue this one for approval?", expectedParentTableCode == JobConsolSchema.Constants.Prefix ? "consol" : "job");
			approvalCollection = LoadAllApprovalRequests();
			AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest.XP_ApprovalStatus);
			if (IsARCreditNoteTesting)
			{
				AssertType("LastFormShownDialogForTest", typeof(LoginFormWithRequest), ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval request.", 1, approvalCollection.Length);
			}
			else
			{
				Assert("LoginFormWithRequest is shown.", isLoginFormShown);
				AssertType("LastFormShownDialogForTest", typeof(APInvoicePostingWithApprovalRequestSummaryForm), ZFormModaliser.LastFormShownDialogForTest);
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval request.", 2, approvalCollection.Length);
				AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Requested, approvalRequest2.XP_ApprovalStatus);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			isLoginFormShown = false;
			if (IsARCreditNoteTesting)
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			}
			else
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(setLoginScreenToAskToCreateRequest);
			}
			ZFormModaliser.LastFormShownDialogForTest = null;
			guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();
			Assert("IsAnyChargePosted", !IsAnyChargePosted(charge1, charge2, charge3));
			Assert("LoginFormWithRequest is shown.", isLoginFormShown);
			var expectedLastMessage = "Posting is canceled. Only AR Credit Note requests will be created. To Post other transaction types post them separately from unauthorized AR Credit Notes.";
			approvalCollection = LoadAllApprovalRequests();
			AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest.XP_ApprovalStatus);
			GenApprovalRequest[] newApprovalRequests = null;
			if (IsARCreditNoteTesting)
			{
				AssertType("LastFormShownDialogForTest", ApprovalBulkFormType, ZFormModaliser.LastFormShownDialogForTest);
				if (IsShowWarningsSupported)
				{
					AssertEquals("PreviousMessage", expectedMessage, UnitTestUserNotification.Instance.PreviousMessages[UnitTestUserNotification.Instance.PreviousMessages.Length - 2].Text);
					AssertEquals("LastMessage", expectedLastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				AssertEquals("Approval request.", 2, approvalCollection.Length);
				newApprovalRequests = approvalCollection.Where(x => x.PK != approvalRequest.PK).ToArray();
				AssertEquals("newApprovalRequests.Length", 1, newApprovalRequests.Length);
			}
			else
			{
				AssertType("LastFormShownDialogForTest", typeof(APInvoicePostingWithApprovalRequestSummaryForm), ZFormModaliser.LastFormShownDialogForTest);
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval request.", 4, approvalCollection.Length);
				newApprovalRequests = approvalCollection.Where(x => x.PK != approvalRequest.PK && x.PK != approvalRequest2.PK).ToArray();
				AssertEquals("newApprovalRequests.Length", 2, newApprovalRequests.Length);
			}
			AssertApprovalRequest(newApprovalRequests, expectedParentID, expectedParentTableCode, Constants.GenApprovalRequestApprovalStatus.Requested, expectedPostingOption, expectedAmounts, expectedNumerOfChargesForRequest, expectedCreditors);

			newApprovalRequests[0].XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			AlterApprovalRequest(newApprovalRequests[0], 10); //to make this request different for current posting
			if (!IsARCreditNoteTesting)
			{
				newApprovalRequests[1].XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
				AlterApprovalRequest(newApprovalRequests[1], 20); //to make this request different for current posting
			}
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			isLoginFormShown = false;
			if (IsARCreditNoteTesting)
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			}
			else
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(setLoginScreenToAskToCreateRequestAndCancelSummaryScreen);
			}
			ZFormModaliser.LastFormShownDialogForTest = null;
			guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();
			Assert("IsAnyChargePosted", !IsAnyChargePosted(charge1, charge2, charge3));
			expectedMessage = "There is an approved request for this posting action, but data for approval is different. This approved request must be canceled to continue posting.";
			approvalCollection = LoadAllApprovalRequests();
			AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest.XP_ApprovalStatus);
			AssertEquals("current request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, newApprovalRequests[0].XP_ApprovalStatus);
			if (IsARCreditNoteTesting)
			{
				var expectedFormType = expectedParentTableCode == JobConsolSchema.Constants.Prefix ? typeof(AgentPostingOptionSelectionForm) : null;
				AssertType("LastFormShownDialogForTest", expectedFormType, ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval request.", 2, approvalCollection.Length);
			}
			else
			{
				Assert("LastFormShownDialogForTest should be shown always as we go all way and just show summary at the end", isLoginFormShown);
				AssertType("LastFormShownDialogForTest", typeof(APInvoicePostingWithApprovalRequestSummaryForm), ZFormModaliser.LastFormShownDialogForTest);
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval request.", 4, approvalCollection.Length);
				AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest2.XP_ApprovalStatus);
				AssertEquals("current request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Approved, newApprovalRequests[1].XP_ApprovalStatus);
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			if (IsARCreditNoteTesting)
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			}
			else
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(setLoginScreenToAskToCreateRequestAndContinueSummaryScreen);
			}
			ZFormModaliser.LastFormShownDialogForTest = null;
			guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();
			Assert("IsAnyChargePosted", !IsAnyChargePosted(charge1, charge2, charge3));
			approvalCollection = LoadAllApprovalRequests();
			AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest.XP_ApprovalStatus);
			AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, newApprovalRequests[0].XP_ApprovalStatus);
			GenApprovalRequest[] newApprovalRequests2 = null;
			if (IsARCreditNoteTesting)
			{
				AssertType("LastFormShownDialogForTest", ApprovalBulkFormType, ZFormModaliser.LastFormShownDialogForTest);
				if (IsShowWarningsSupported)
				{
					AssertEquals("PreviousMessage", expectedMessage, UnitTestUserNotification.Instance.PreviousMessages[UnitTestUserNotification.Instance.PreviousMessages.Length - 2].Text);
					AssertEquals("LastMessage", expectedLastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				else
				{
					AssertEquals("LastMessage", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				}
				AssertEquals("Approval request.", 3, approvalCollection.Length);
				newApprovalRequests2 = approvalCollection.Where(x => x.PK != approvalRequest.PK && x.PK != newApprovalRequests[0].PK).ToArray();
				AssertEquals("newApprovalRequests2.Length", 1, newApprovalRequests2.Length);
			}
			else
			{
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Approval request.", 6, approvalCollection.Length);
				var pksToExclude = new[] { approvalRequest.PK, approvalRequest2.PK, newApprovalRequests[0].PK, newApprovalRequests[1].PK };
				newApprovalRequests2 = approvalCollection.Where(x => !pksToExclude.Contains(x.PK)).ToArray();
				AssertEquals("newApprovalRequests2.Length", 2, newApprovalRequests2.Length);
			}
			AssertApprovalRequest(newApprovalRequests2, expectedParentID, expectedParentTableCode, Constants.GenApprovalRequestApprovalStatus.Requested, expectedPostingOption, expectedAmounts, expectedNumerOfChargesForRequest, expectedCreditors);

			guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.ShowLoginFormForTest = false;
			guiWrapper.SecurityItemForTest = IsARCreditNoteTesting ? Env.Security.ReceivablesTransactions.Code : Env.Security.APInvoiceApproval.Code;
			guiWrapper.Post();
			Assert("IsAnyChargePosted", IsAnyChargePosted(charge1, charge2, charge3));
			AssertEquals("Should create invoices", 2, GetNumerOfPostedInvoices(charge1, charge2, charge3));
			approvalCollection = LoadAllApprovalRequests();
			if (IsARCreditNoteTesting)
			{
				AssertEquals("Approval request.", 3, approvalCollection.Length);
				AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest.XP_ApprovalStatus);
				AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, newApprovalRequests[0].XP_ApprovalStatus);
				AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, newApprovalRequests2[0].XP_ApprovalStatus);
			}
			else
			{
				AssertEquals("Approval request.", 6, approvalCollection.Length);
				AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest.XP_ApprovalStatus);
				AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, approvalRequest2.XP_ApprovalStatus);
				AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, newApprovalRequests[0].XP_ApprovalStatus);
				AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, newApprovalRequests[1].XP_ApprovalStatus);
				AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, newApprovalRequests2[0].XP_ApprovalStatus);
				AssertEquals("previous request ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Cancelled, newApprovalRequests2[1].XP_ApprovalStatus);
			}
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestApprovalRequestIncludeChargesForInvoicesNeedAuthorizationOnly()
		{
			SetupSecurity(true);
			var job = SetupJobData();
			var charge1 = SetupChargeData(job, 150m, TestObjectCreator.CC3);
			var charge2 = SetupChargeData(job, 150m, TestObjectCreator.CC7);
			var charge3 = SetupChargeData(job, 150m, TestObjectCreator.CC3, useAnotherOrg: true);
			Factory.Save();

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					ZFormModaliser.ResultToReturnFromShowDialog = form.GetType() == typeof(LoginFormWithRequest) ? DialogResult.Ignore : DialogResult.OK;
				});
			var guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();

			if (IsARCreditNoteTesting)
			{
				Assert("IsAnyChargePosted", !IsAnyChargePosted(charge1, charge2, charge3));
				AssertType("Should prompt login form", ApprovalBulkFormType, ZFormModaliser.LastFormShownDialogForTest);
			}
			else
			{
				Assert("charge 1,2 - IsAnyChargePosted", !IsAnyChargePosted(charge1, charge2));
				Assert("charge 3 - IsAnyChargePosted", IsAnyChargePosted(charge3));
				AssertType("Should prompt login form", typeof(APInvoicePostingWithApprovalRequestSummaryForm), ZFormModaliser.LastFormShownDialogForTest);
			}
			Assert("No errors", !UnitTestUserNotification.Instance.LastMessage.WasError);
			var approvalCollection = LoadAllApprovalRequests();
			AssertEquals("Approval request should be created.", 1, approvalCollection.Length);
			ZGuid expectedParentID;
			ZString expectedParentTableCode;
			if (IsARCreditNoteTesting)
			{
				GetParentForPostingAction(job, out expectedParentID, out expectedParentTableCode);
			}
			else
			{
				expectedParentID = job.PK;
				expectedParentTableCode = job.TablePrefix;
			}
			var approvalRequest = approvalCollection[0];
			string expectedPostingOption = nameof(JobInvoicingPostingOption.All);
			AssertApprovalRequest(new[] { approvalRequest }, expectedParentID, expectedParentTableCode, Constants.GenApprovalRequestApprovalStatus.Requested, expectedPostingOption, new[] { 300m }, new[] { 2 }, new[] { "ZCreditor1" });
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestPostApprovedRequestOnNextPosting()
		{
			SetupSecurity();
			var job = SetupJobData();
			var charge1 = SetupChargeData(job, 150m, TestObjectCreator.CC3);
			var charge2 = SetupChargeData(job, 150m, TestObjectCreator.CC7);
			var charge3 = SetupChargeData(job, 150m, TestObjectCreator.CC3, useAnotherOrg: true);
			Factory.Save();

			ZFormModaliser.LastFormShownDialogForTest = null;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				ZFormModaliser.ResultToReturnFromShowDialog = form.GetType() == typeof(LoginFormWithRequest) ? DialogResult.Ignore : DialogResult.OK);
			var guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();

			Assert("IsAnyChargePosted", !IsAnyChargePosted(charge1, charge2, charge3));
			AssertType("LastFormShownDialogForTest", ApprovalBulkFormType, ZFormModaliser.LastFormShownDialogForTest);
			Assert("No errors", !UnitTestUserNotification.Instance.LastMessage.WasError);
			var approvalCollection = LoadAllApprovalRequests();
			if (IsARCreditNoteTesting)
			{
				AssertEquals("Approval request should be created.", 1, approvalCollection.Length);
			}
			else
			{
				AssertEquals("Approval request should be created.", 2, approvalCollection.Length);
			}

			var approvalRequest = approvalCollection[0];
			approvalRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			GenApprovalRequest approvalRequest2 = null;
			if (!IsARCreditNoteTesting)
			{
				approvalRequest2 = approvalCollection[1];
				approvalRequest2.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			}
			Factory.Save();
			guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.ShowLoginFormForTest = false;
			guiWrapper.SecurityItemForTest = IsARCreditNoteTesting ? Env.Security.ReceivablesTransactions.Code : Env.Security.APInvoiceApproval.Code;
			guiWrapper.Post();
			Assert("IsAnyChargePosted", IsAnyChargePosted(charge1, charge2, charge3));
			AssertEquals("Should create invoices", 2, GetNumerOfPostedInvoices(charge1, charge2, charge3));
			AssertEquals("previous request XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequest.XP_ApprovalStatus);
			if (!IsARCreditNoteTesting)
			{
				AssertEquals("previous request 2 XP_ApprovalStatus", Constants.GenApprovalRequestApprovalStatus.Posted, approvalRequest2.XP_ApprovalStatus);
			}
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestApprovalRequestForARCreditNoteAndAPInvoiceCreatedInOneGo()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetupSecurity(isARCreditNoteTestingForced: true);
			SetupSecurity(isARCreditNoteTestingForced: false);
			var job = SetupJobData();
			var charge1 = SetupChargeData(job, 300m, TestObjectCreator.CC3, isARCreditNoteTestingForced: true);
			var charge2 = SetupChargeData(job, 300m, TestObjectCreator.CC3, isARCreditNoteTestingForced: false);
			Factory.Save();

			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();
			Assert("IsAnyChargePosted for ARCreditNote", !IsAnyChargePosted(true, charge1, charge2));
			Assert("IsAnyChargePosted for APInvoice", !IsAnyChargePosted(false, charge1, charge2));
			Assert("No errors", !UnitTestUserNotification.Instance.LastMessage.WasError);
			var approvalCollection = LoadAllApprovalRequests(isARCreditNoteTestingForced: true);
			AssertEquals("Approval request for ARCreditNote", 0, approvalCollection.Length);
			approvalCollection = LoadAllApprovalRequests(isARCreditNoteTestingForced: false);
			AssertEquals("Approval request for APInvoice", 0, approvalCollection.Length);

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
							ZFormModaliser.ResultToReturnFromShowDialog = form.GetType() == typeof(LoginFormWithRequest) ? DialogResult.Ignore : DialogResult.OK);
			ZFormModaliser.LastFormShownDialogForTest = null;
			guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();
			Assert("IsAnyChargePosted for ARCreditNote", !IsAnyChargePosted(true, charge1, charge2));
			Assert("IsAnyChargePosted for APInvoice", !IsAnyChargePosted(false, charge1, charge2));
			Assert("No errors", !UnitTestUserNotification.Instance.LastMessage.WasError);
			approvalCollection = LoadAllApprovalRequests(isARCreditNoteTestingForced: true);
			AssertEquals("Approval request for ARCreditNote", 1, approvalCollection.Length);
			approvalCollection = LoadAllApprovalRequests(isARCreditNoteTestingForced: false);
			AssertEquals("Approval request for APInvoice can't be created in the same time with AR Credit Note request as saving only AP Invoice request factory can cause saving posted request without AP Invoice", 0, approvalCollection.Length);
			if (IsShowWarningsSupported)
			{
				var expectedLastMessage = "Posting is canceled. Only AR Credit Note requests will be created. To Post other transaction types post them separately from unauthorized AR Credit Notes.";
				AssertEquals("LastMessage", expectedLastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestApprovalRequestForARCreditNoteCancelledAndAPInvoiceCreatedInOneGo()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetupSecurity(isARCreditNoteTestingForced: true);
			SetupSecurity(isARCreditNoteTestingForced: false);
			var job = SetupJobData();
			var charge1 = SetupChargeData(job, 300m, TestObjectCreator.CC3, isARCreditNoteTestingForced: true);
			var charge2 = SetupChargeData(job, 300m, TestObjectCreator.CC3, isARCreditNoteTestingForced: false);
			Factory.Save();

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				if (form.GetType() == typeof(LoginFormWithRequest))
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore;
				}
				else
				{
					ZFormModaliser.ResultToReturnFromShowDialog = ((ZForm)form).BusinessEntity is ARCreditNoteApprovalBulk ? DialogResult.Cancel : DialogResult.OK;
				}
			});
			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();
			Assert("IsAnyChargePosted for ARCreditNote", !IsAnyChargePosted(true, charge1, charge2));
			Assert("IsAnyChargePosted for APInvoice", !IsAnyChargePosted(false, charge1, charge2));
			Assert("No errors", !UnitTestUserNotification.Instance.LastMessage.WasError);
			var approvalCollection = LoadAllApprovalRequests(isARCreditNoteTestingForced: true);
			AssertEquals("Approval request for ARCreditNote", 0, approvalCollection.Length);
			approvalCollection = LoadAllApprovalRequests(isARCreditNoteTestingForced: false);
			AssertEquals("Approval request for APInvoice can't be created in the same time with AR Credit Note request as saving only AP Invoice request factory can cause saving posted request without AP Invoice", 0, approvalCollection.Length);
			AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[TestDate(2005, 9, 10)]
		public virtual void TestApprovalRequestForARCreditNoteCreatedAndAPInvoiceCancelledInOneGo()
		{
			AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			SetupSecurity(isARCreditNoteTestingForced: true);
			SetupSecurity(isARCreditNoteTestingForced: false);
			var job = SetupJobData();
			var charge1 = SetupChargeData(job, 300m, TestObjectCreator.CC3, isARCreditNoteTestingForced: true);
			var charge2 = SetupChargeData(job, 300m, TestObjectCreator.CC3, isARCreditNoteTestingForced: false);
			Factory.Save();

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				if (form.GetType() == typeof(LoginFormWithRequest))
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore;
				}
				else
				{
					ZFormModaliser.ResultToReturnFromShowDialog = ((ZForm)form).BusinessEntity is APInvoiceChargesApprovalBulk ? DialogResult.Cancel : DialogResult.OK;
				}
			});
			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var guiWrapper = GetNewGUIWrapper(job);
			guiWrapper.Post();
			Assert("IsAnyChargePosted for ARCreditNote", !IsAnyChargePosted(true, charge1, charge2));
			Assert("IsAnyChargePosted for APInvoice", !IsAnyChargePosted(false, charge1, charge2));
			Assert("No errors", !UnitTestUserNotification.Instance.LastMessage.WasError);
			var approvalCollection = LoadAllApprovalRequests(isARCreditNoteTestingForced: true);
			AssertEquals("Approval request for ARCreditNote", 1, approvalCollection.Length);
			approvalCollection = LoadAllApprovalRequests(isARCreditNoteTestingForced: false);
			AssertEquals("Approval request for APInvoice", 0, approvalCollection.Length);
			if (IsShowWarningsSupported)
			{
				var expectedLastMessage = "Posting is canceled. Only AR Credit Note requests will be created. To Post other transaction types post them separately from unauthorized AR Credit Notes.";
				AssertEquals("LastMessage", expectedLastMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				AssertNull("LastMessage", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		protected virtual Job SetupJobData(string jobNumber = "S001")
		{
			var shipment = TestObjectCreator.CreateShipment(jobNumber);
			var job = TestObjectCreator.CreateJob(shipment, false);
			job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;

			return job;
		}

		protected virtual Charge SetupChargeData(Job job, ZDecimal amount, AccChargeCode chargeCode, bool useAnotherOrg = false, bool? isARCreditNoteTestingForced = null)
		{
			Charge charge;
			bool isARCreditNoteTesting = isARCreditNoteTestingForced ?? IsARCreditNoteTesting;
			if (isARCreditNoteTesting)
			{
				charge = TestObjectCreator.CreateCharge(job, chargeCode, "", TestObjectCreator.AUD, 0, null, TestObjectCreator.AUD, -amount, useAnotherOrg ? TestObjectCreator.Agent : TestObjectCreator.LocalClient);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
			}
			else
			{
				charge = TestObjectCreator.CreateCharge(job, chargeCode, "", TestObjectCreator.AUD, amount, useAnotherOrg ? TestObjectCreator.Creditor2 : TestObjectCreator.Creditor1, TestObjectCreator.AUD, 0, null);
				charge.JR_APInvoiceNum = job.JH_JobNum + "_123";
				charge.JR_APInvoiceDate = ZDateTime.Today;
			}

			return charge;
		}

		protected bool IsAnyChargePosted(params Charge[] charges)
		{
			return IsAnyChargePosted(null, charges);
		}

		bool IsAnyChargePosted(bool? isARCreditNoteTestingForced = null, params Charge[] charges)
		{
			return GetNumerOfPostedInvoices(isARCreditNoteTestingForced, charges) > 0;
		}

		protected int GetNumerOfPostedInvoices(params Charge[] charges)
		{
			return GetNumerOfPostedInvoices(null, charges);
		}

		int GetNumerOfPostedInvoices(bool? isARCreditNoteTestingForced = null, params Charge[] charges)
		{
			bool isARCreditNoteTesting = isARCreditNoteTestingForced ?? IsARCreditNoteTesting;
			var postedCharges = from charge in charges
								where isARCreditNoteTesting ? charge.JR_IsRevenuePosted : charge.JR_IsCostPosted
								select isARCreditNoteTesting ? charge.ARLine.AL_AH : charge.APLine.AL_AH;

			return postedCharges.Distinct().Count();
		}

		protected abstract bool IsARCreditNoteTesting { get; }

		Type LoginForType
		{
			get
			{
				return IsARCreditNoteTesting ? ApprovalBulkFormType : typeof(APInvoicePostingWithApprovalRequestSummaryForm);
			}
		}

		protected PostManagerGUIWrapper GetNewGUIWrapper(params Job[] jobs)
		{
			var guiWrapper = GetNewGUIWrapperCore(jobs);
			guiWrapper.DoTestPostTransactions = true;
			guiWrapper.ShowLoginFormForTest = true;

			return guiWrapper;
		}

		protected void SetupSecurity(bool setOnlySecondLevel = false, bool? isARCreditNoteTestingForced = null)
		{
			bool isARCreditNoteTesting = isARCreditNoteTestingForced ?? IsARCreditNoteTesting;
			if (isARCreditNoteTesting)
			{
				if (!setOnlySecondLevel)
				{
					Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				}
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;

				var setting = new AuthorizationModeAndSettings();
				var valuesForTest = setting.AuthorisationSettings;
				var newSetting = valuesForTest.AddNew();
				newSetting.Amount = 200;
				newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
				newSetting.Range = PaymentAuthorisationSettings.RangeCodes.UpTo;
				newSetting = valuesForTest.AddNew();
				newSetting.Amount = 200;
				newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
				newSetting.Range = PaymentAuthorisationSettings.RangeCodes.Above;

				AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), setting);
			}
			else
			{
				if (!setOnlySecondLevel)
				{
					Env.Security.APInvoiceApproval_FirstApproval.IsAllowed = false;
				}
				Env.Security.APInvoiceApproval_SecondApproval.IsAllowed = false;

				var valuesForTest = new PaymentTwelveLevelAuthorisationSettingsCollection();
				var newSetting = valuesForTest.AddNew();
				newSetting.Amount = 200;
				newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
				newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
				newSetting = valuesForTest.AddNew();
				newSetting.Amount = 200;
				newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
				newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;

				AccountingConfigurationRegistry.Instance.UnapprovedInvoicesAuthorizationSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
			}
		}

		protected GenApprovalRequest[] LoadAllApprovalRequests(bool? isARCreditNoteTestingForced = null)
		{
			bool isARCreditNoteTesting = isARCreditNoteTestingForced ?? IsARCreditNoteTesting;
			if (isARCreditNoteTesting)
			{
				return Factory.Load<ARCreditNoteApprovalRequest>(new ARCreditNoteApprovalRequestCollection(Factory).CompleteFilter);
			}
			else
			{
				return Factory.Load<APInvoiceChargesApprovalRequest>(new APInvoiceChargesApprovalRequestCollection(Factory).CompleteFilter);
			}
		}

		protected void AssertApprovalRequest(GenApprovalRequest[] approvalRequests, ZGuid expectedParentID, string expectedParentTableCode, string expectedStatus, string postingOption, decimal[] maxAmounts, int[] chargeCounts, string[] creditors = null)
		{
			if (IsARCreditNoteTesting)
			{
				AssertEquals("One request for job", 1, approvalRequests.Length);
				var approvalRequest_Cast = approvalRequests[0] as ARCreditNoteApprovalRequest;
				AssertEquals("ParentID", expectedParentID, approvalRequests[0].XP_ParentID);
				AssertEquals("ParentID", expectedParentTableCode, approvalRequests[0].XP_ParentTableCode);
				AssertEquals("ApprovalStatus", expectedStatus, approvalRequests[0].XP_ApprovalStatus);
				AssertEquals("PostingDetails.PostingOption", postingOption, approvalRequest_Cast.PostingDetails.PostingOption);
				AssertEquals("PostingDetails.MaxAmountToApprove", maxAmounts[0], approvalRequest_Cast.PostingDetails.MaxAmountToApprove);
				AssertEquals("PostingDetails.Charges.Count", chargeCounts[0], approvalRequest_Cast.PostingDetails.Charges.Count);
			}
			else
			{
				for (int i = 0; i < approvalRequests.Length; i++)
				{
					var approvalRequest_Cast = approvalRequests[i] as APInvoiceChargesApprovalRequest;
					AssertEquals("ParentID", expectedParentID, approvalRequests[i].XP_ParentID);
					AssertEquals("ParentID", expectedParentTableCode, approvalRequests[i].XP_ParentTableCode);
					AssertEquals("ApprovalStatus", expectedStatus, approvalRequests[i].XP_ApprovalStatus);
					AssertEquals("PostingDetails.Creditor", creditors[i], approvalRequest_Cast.PostingDetails.Creditor);
					AssertEquals("PostingDetails.TransactionNumber", "S001_123", approvalRequest_Cast.PostingDetails.TransactionNumber);
					AssertEquals("PostingDetails.MaxAmountToApprove", maxAmounts[i], approvalRequest_Cast.PostingDetails.MaxAmountToApprove);
					AssertEquals("PostingDetails.Charges.Count", chargeCounts[i], approvalRequest_Cast.PostingDetails.Charges.Count);
				}
			}
		}

		void AlterApprovalRequest(GenApprovalRequest approvalRequest, int maxAmount)
		{
			if (IsARCreditNoteTesting)
			{
				var approvalRequestCasted = approvalRequest as ARCreditNoteApprovalRequest;
				approvalRequestCasted.PostingDetails.MaxAmountToApprove = maxAmount;
			}
			else
			{
				var approvalRequestCasted = approvalRequest as APInvoiceChargesApprovalRequest;
				approvalRequestCasted.PostingDetails.MaxAmountToApprove = maxAmount;
			}
		}

		protected abstract PostManagerGUIWrapper GetNewGUIWrapperCore(params Job[] jobs);

		protected virtual void GetParentForPostingAction(Job job, out ZGuid expectedParentID, out ZString expectedParentTableCode)
		{
			expectedParentID = job.PK;
			expectedParentTableCode = job.TablePrefix;
		}

		protected override void SetUp()
		{
			base.SetUp();

			if (!IsARCreditNoteTesting)
			{
				AccountingMasterFilesRegistry.Instance.EnableAPInvoiceApproval.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			}
			TestObjectCreator.CreateTestPeriodsForEntireYear(ZDateTime.Today.Year);
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}

		TestObjectCreator testObjectCreator;

		protected virtual bool IsShowWarningsSupported
		{
			get { return true; }
		}

		#endregion

		public Type ApprovalBulkFormType
		{
			get { return IsARCreditNoteTesting ? typeof(ARCreditNoteApprovalBulkForm) : typeof(APInvoicePostingWithApprovalRequestSummaryForm); }
		}

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;
	}
}
