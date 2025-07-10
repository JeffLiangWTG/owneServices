using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Core.Forms;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.GUI.Testing
{
	abstract class TransactionApprovalBulkFormBasherTest<FormType, TransactionType, RequestType, DetailsType> : ZFormBasherTest
		where FormType : TransactionApprovalBulkForm<TransactionType, RequestType, DetailsType>
		where TransactionType : TransactionHeader
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		public virtual void TestFormParameters()
		{
			using (FormType testForm = (FormType)GetFormToBash())
			{
				testForm.Show();

				var postingButtonsUserControl = testForm.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl");

				AssertEquals(this.ShouldSaveButtonVisibleAtFirst, postingButtonsUserControl.SaveButton.Visible);
				AssertEquals(ODisplayMode.Edit, testForm.DisplayMode);
				AssertEquals("Continue", postingButtonsUserControl.SaveAndCloseButton.Text);
				AssertEquals("TopGridPanel.Visible", false, testForm.GetControl<ZPanel>("TopGridPanel").Visible);
				AssertEquals("TopSingleRequestPanel.Visible", true, testForm.GetControl<ZPanel>("TopSingleRequestPanel").Visible);
				AssertEquals("ReasonDescriptionTextBox.ReadOnly", false, testForm.GetControl<ZTextBox>("ReasonDescriptionTextBox").ReadOnly);
				AssertEquals("FormVerb", "", testForm.FormVerb);

				var formBizo = (TransactionApprovalBulk<TransactionType, RequestType, DetailsType>)testForm.BusinessEntity;
				formBizo.RunPreSaveValidation();
				AssertNoErrors("Precondition: ", formBizo);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				postingButtonsUserControl.SaveAndCloseButton.PerformClick();
				AssertNull("User shouldn't be asked about changes before closing a form.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("testForm.DialogResult", DialogResult.OK, testForm.DialogResult);
				AssertEquals("Factory must not be saved.", false, formBizo.Approvals[0].IsInDatabase);
			}
		}

		protected virtual bool ShouldSaveButtonVisibleAtFirst
		{
			get
			{
				return false;
			}
		}

		public virtual void TestDontAskToSaveDataOnCanceling()
		{
			using (FormType testForm = (FormType)GetFormToBash())
			{
				testForm.Show();
				var formBizo = (TransactionApprovalBulk<TransactionType, RequestType, DetailsType>)testForm.BusinessEntity;
				formBizo.Approvals[0].XP_ReasonDescription = "other desc";
				formBizo.RunPreSaveValidation();
				AssertNoErrors("Precondition: ", formBizo);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var postingButtonsUserControl = testForm.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl");
				postingButtonsUserControl.CloseButton.PerformClick();
				AssertNull("User shouldn't be asked about changes before closing a form.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDontSaveFactoryInViewMode()
		{
			ApprovalFormMode = TransactionApprovalFormModes.View;
			using (FormType testForm = (FormType)GetFormToBash())
			{
				testForm.Show();
				var formBizo = (TransactionApprovalBulk<TransactionType, RequestType, DetailsType>)testForm.BusinessEntity;
				formBizo.RunPreSaveValidation();
				AssertNoErrors("Precondition: ", formBizo);
				var postingButtonsUserControl = testForm.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl");
				postingButtonsUserControl.SaveAndCloseButton.PerformClick();
				AssertEquals("Factory must not be saved.", false, formBizo.Approvals[0].IsInDatabase);
			}
		}

		public void TestFormVerb()
		{
			ApprovalFormMode = TransactionApprovalFormModes.Approve;
			using (FormType testForm = (FormType)GetFormToBash())
			{
				AssertEquals("FormVerb", "Approve", testForm.FormVerb);
			}

			ApprovalFormMode = TransactionApprovalFormModes.Reject;
			using (FormType testForm = (FormType)GetFormToBash())
			{
				AssertEquals("FormVerb", "Reject", testForm.FormVerb);
			}

			ApprovalFormMode = TransactionApprovalFormModes.Cancel;
			using (FormType testForm = (FormType)GetFormToBash())
			{
				AssertEquals("FormVerb", "Cancel", testForm.FormVerb);
			}

			ApprovalFormMode = TransactionApprovalFormModes.SetDescription;
			using (FormType testForm = (FormType)GetFormToBash())
			{
				AssertEquals("FormVerb", "", testForm.FormVerb);
			}
		}

		public void TestReasonDescriptionTextBoxReadOnly()
		{
			foreach (var mode in Enum.GetValues(typeof(TransactionApprovalFormModes)))
			{
				ApprovalFormMode = (TransactionApprovalFormModes)mode;
				using (FormType testForm = (FormType)GetFormToBash())
				{
					testForm.Show();

					var reasonDescriptionTextBox = testForm.GetControl<ZTextBox>("ReasonDescriptionTextBox");
					bool isReadOnly = !ModesWhenReasonDescriptionShouldNotBeReadOnly.Contains(ApprovalFormMode);
					AssertEquals("ReasonDescriptionTextBox ReadOnly", isReadOnly, reasonDescriptionTextBox.ReadOnly);
				}
			}
		}

		#region Implementation

		protected virtual TransactionApprovalFormModes[] ModesWhenReasonDescriptionShouldNotBeReadOnly
		{
			get { return new TransactionApprovalFormModes[] { TransactionApprovalFormModes.SetDescription }; }
		}

		protected override bool AllowHasChangesOnFormOpen
		{
			get { return true; }
		}

		protected override Form GetFormToBashCore()
		{
			var formBizo = GetNewApprovalRequest();
			formBizo.XP_ParentID = ZGuid.NewZGuid();
			formBizo.XP_ReasonDescription = "Desc";
			formBizo.XP_ReasonCode = "IOB";
			return GetRequestForm(formBizo);
		}

		protected abstract RequestType GetNewApprovalRequest();
		protected abstract FormType GetRequestForm(params RequestType[] bizos);

		protected override void SetUp()
		{
			base.SetUp();

			ApprovalFormMode = TransactionApprovalFormModes.SetDescription;
		}

		protected TransactionApprovalFormModes ApprovalFormMode;

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion
	}

	abstract class TransactionApprovalBulkFormWithMultipleRequestsBasherTest<FormType, TransactionType, RequestType, DetailsType> :
		TransactionApprovalBulkFormBasherTest<FormType, TransactionType, RequestType, DetailsType>
		where FormType : TransactionApprovalBulkForm<TransactionType, RequestType, DetailsType>
		where TransactionType : TransactionHeader
		where RequestType : TransactionApprovalRequest<DetailsType>
		where DetailsType : ApprovalRequestDetails
	{
		public void TestEditFormsHaveDifferentFactories()
		{
			if (!ShouldSaveButtonVisibleAtFirst)
			{
				Assert("Testing button is not visible", true);
				return;
			}

			var testApprovals = 3;
			ApprovalFormMode = TransactionApprovalFormModes.Approve;
			var bulkApprovalForm = GetRequestForm(GetApprovalRequests(testApprovals, testApprovals));

			var editFormFactoryHashes = new HashSet<int>();

			bulkApprovalForm.OnEditFormShown_ForTestOnly += (sender, e) =>
			{
				var form = (ZForm)sender;
				editFormFactoryHashes.Add(form.BusinessEntity.Factory.GetHashCode());
				form.Close();
			};

			bulkApprovalForm.Show();

			var saveAndPostButton = bulkApprovalForm
				.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
				.SaveButton;

			saveAndPostButton.PerformClick();

			var approvalGrid = bulkApprovalForm
				.GetControl<ZGrid>("TopGrid");
			var editOption = approvalGrid.ContextMenu.MenuItems.FindByText("Edit", true);

			approvalGrid.SelectAllElements();
			editOption.PerformClick();

			AssertEquals("Factories should be as many as failed posting approvals, because they should be all different.", testApprovals, editFormFactoryHashes.Count);

			bulkApprovalForm.Close();
		}

		public void TestErrorsOfApprovalWillPrevendEditForm()
		{
			if (!ShouldSaveButtonVisibleAtFirst)
			{
				Assert("Testing button is not visible", true);
				return;
			}

			var requests = GetApprovalRequests(5, 5);
			ReleaseFactory();

			requests = Factory.Load<RequestType>(new ZQuery(GenApprovalRequestSchema.PK, requests.Select(x => x.PK)));
			ApprovalFormMode = TransactionApprovalFormModes.Approve;
			var bulkApprovalForm = GetRequestForm(requests);
			var approvalsInForm = ((TransactionApprovalBulk<TransactionType, RequestType, DetailsType>)bulkApprovalForm.BusinessEntity).Approvals;

			var editFormsShown = 0;

			bulkApprovalForm.OnEditFormShown_ForTestOnly += (sender, e) =>
			{
				editFormsShown++;
				((ZForm)sender).Close();
			};

			var saveAndPostButton = bulkApprovalForm
				.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
				.SaveButton;

			bulkApprovalForm.Show();
			saveAndPostButton.PerformClick();

			var approvalGrid = bulkApprovalForm
				.GetControl<ZGrid>("TopGrid");
			var editOption = approvalGrid.ContextMenu.MenuItems.FindByText("Edit", true);

			approvalsInForm[0].XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Rejected;
			Factory.Save();

			approvalGrid.UnSelectAll();
			approvalGrid.Select(0);
			editOption.PerformClick();

			var requestID = approvalsInForm[0].ReferenceID;
			Assert("Precondition: ReferenceID", !requestID.IsEmpty);
			var approvalStatus = approvalsInForm[0].XP_ApprovalStatus;
			Assert("Precondition: XP_ApprovalStatus", !approvalStatus.IsEmpty);
			AssertEquals($"Can’t Edit request ({requestID}) - only approved request can be edited here. Its status now is ‘{approvalStatus}’.",
				UnitTestUserNotification.Instance.LastMessage.Text);

			approvalsInForm[1].XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Cancelled;
			approvalsInForm[1].SetContext(BusinessContext.CancelApprovalRequestByUser);
			approvalsInForm[2].XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Posted;
			Factory.Save();

			var requestID1 = approvalsInForm[1].ReferenceID;
			Assert("Precondition: ReferenceID", !requestID1.IsEmpty);
			var requestID2 = approvalsInForm[2].ReferenceID;
			Assert("Precondition: ReferenceID", !requestID2.IsEmpty);
			var noticeInformation = ZString.Format("Request ({0}) is already Canceled.\r\nRequest ({1}) is already Posted.", requestID1, requestID2);

			approvalGrid.UnSelectAll();
			approvalGrid.Select(1);
			approvalGrid.Select(2);
			editOption.PerformClick();

			AssertEquals(noticeInformation, UnitTestUserNotification.Instance.LastMessage.Text);

			approvalsInForm[1].XP_ParentID = ZGuid.NewZGuid();
			Factory.Save();

			approvalGrid.UnSelectAll();
			approvalGrid.Select(1);
			editOption.PerformClick();

			requestID = approvalsInForm[1].ReferenceID;
			if (!IsRequestIDDependsOnXP_ParentID)
			{
				Assert("Precondition: ReferenceID", !requestID.IsEmpty);
			}
			AssertEquals($"A {ParentType} can’t be found for request ({requestID}).", UnitTestUserNotification.Instance.LastMessage.Text);

			var newFactory = new BusinessObjectFactory();
			newFactory.RefreshEnabled = false;
			var approval = newFactory.Load<RequestType>(approvalsInForm[2].PK);
			approval.Delete();
			newFactory.Save();

			approvalGrid.UnSelectAll();
			approvalGrid.Select(2);
			editOption.PerformClick();

			requestID = approvalsInForm[2].ReferenceID;
			Assert("Precondition: ReferenceID", !requestID.IsEmpty);
			AssertEquals($"Request ({requestID}) cannot be found.", UnitTestUserNotification.Instance.LastMessage.Text);

			AssertEquals("No edit form should be shown because of all editing should be stopped by errors.", 0, editFormsShown);

			bulkApprovalForm.Close();
		}

		public void TestSaveAndPostButtonShowsOnlyWhenApproving()
		{
			if (!ShouldSaveButtonVisibleAtFirst)
			{
				Assert("Testing button is not visible", true);
				return;
			}

			var requests = GetApprovalRequests(1, 0);
			ApprovalFormMode = TransactionApprovalFormModes.Approve;
			var bulkApprovalForm = GetRequestForm(requests);

			var saveAndPostButton = bulkApprovalForm
			.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
			.SaveButton;

			bulkApprovalForm.Show();

			Assert("Save button should be shown in Approve mode.", saveAndPostButton.Visible);

			bulkApprovalForm.Close();

			ApprovalFormMode = TransactionApprovalFormModes.SetDescription;
			bulkApprovalForm = GetRequestForm(requests);

			saveAndPostButton = bulkApprovalForm
				.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
				.SaveButton;

			bulkApprovalForm.Show();

			Assert("Save button should not be shown in SetDescription mode.", !saveAndPostButton.Visible);

			bulkApprovalForm.Close();

			ApprovalFormMode = TransactionApprovalFormModes.View;
			bulkApprovalForm = GetRequestForm(requests);

			saveAndPostButton = bulkApprovalForm
			.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
			.SaveButton;

			bulkApprovalForm.Show();

			Assert("Save button should not be shown in View mode.", !saveAndPostButton.Visible);

			bulkApprovalForm.Close();
		}

		public void TestSaveAndPostButton()
		{
			if (!ShouldSaveButtonVisibleAtFirst)
			{
				Assert("Testing button is not visible", true);
				return;
			}

			var requests = GetApprovalRequests(2, 1);
			ApprovalFormMode = TransactionApprovalFormModes.Approve;
			var bulkApprovalForm = GetRequestForm(requests);

			bulkApprovalForm.Show();
			var saveAndPostButton = bulkApprovalForm
				.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
				.SaveButton;

			Assert(saveAndPostButton.Visible);
			AssertEquals("Save && Post", saveAndPostButton.Text);

			saveAndPostButton.PerformClick();
			Assert("Save & Post button shouldn't be able to click again.", !saveAndPostButton.Enabled);

			bulkApprovalForm.Close();

			SetPostingSecurityRight(false);

			var bulkApprovalForm2 = GetRequestForm(requests);

			bulkApprovalForm2.Show();
			var saveAndPostButton2 = bulkApprovalForm2
				.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
				.SaveButton;

			Assert(!saveAndPostButton2.Visible);

			bulkApprovalForm2.Close();
		}

		#region TestSaveAndPostButtonAndEditOperationsProgress

		public void TestSaveAndPostButtonAndEditOperationsProgress()
		{
			if (!ShouldSaveButtonVisibleAtFirst)
			{
				Assert("Testing button is not visible", true);
				return;
			}

			AssertApprovalFormClosedAtLastWithOperations(1, 0, 0);
			AssertApprovalFormClosedAtLastWithOperations(1, 1, 0);
			AssertApprovalFormClosedAtLastWithOperations(1, 1, 1);

			AssertApprovalFormClosedAtLastWithOperations(2, 0, 0);
			AssertApprovalFormClosedAtLastWithOperations(2, 2, 0);

			AssertApprovalFormClosedAtLastWithOperations(5, 3, 0);
			AssertApprovalFormClosedAtLastWithOperations(5, 3, 1);
			AssertApprovalFormClosedAtLastWithOperations(5, 3, 2);
			AssertApprovalFormClosedAtLastWithOperations(5, 3, 3);
		}

		void AssertApprovalFormClosedAtLastWithOperations(int numberOfRequests, int numberOfRequestsWithExceptions, int numberOfEditedRequests)
		{
			Assert("Test should at least has one approval.", numberOfRequests > 0);
			Assert("Number of edited approvals should be no less than those with error.", numberOfEditedRequests <= numberOfRequestsWithExceptions);

			var requests = GetApprovalRequests(numberOfRequests, numberOfRequestsWithExceptions);
			ApprovalFormMode = TransactionApprovalFormModes.Approve;
			var bulkApprovalForm = GetRequestForm(requests);

			var originalApprovals = requests;

			bulkApprovalForm.Show();

			var saveAndPostButton = bulkApprovalForm
				.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
				.SaveButton;
			var editFormsCount = 0;

			var transactionRequestSpecificErroMessage = IsTransactionRelatedRequest ? $"This request is approved, but related {ParentType} has validation errors.\r\n" : "";

			if (numberOfRequestsWithExceptions > 0)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				if (numberOfEditedRequests > 0)
				{
					bulkApprovalForm.OnEditFormShown_ForTestOnly += (sender, e) =>
					{
						var form = (ZForm)sender;
						editFormsCount++;
						AssertEquals("Should use correct form to edit parent of error request.", EditFormTypeName, form.GetType().Name);

						if (IsTransactionRelatedRequest)
						{
							var formControl = form.GetControl<DataGridTextBox>("AL_AG");
							Assert("Any control for AL_AG should be available in the Edit form, it's in a editing mode.", !formControl.ReadOnly);
						}

						if (numberOfRequests == 1)
						{
							AssertEquals(
$@"Error(s) have been found during posting. They can be fixed possibly by editing the related {ParentType}. Do you want to edit it now?
Errors:
{transactionRequestSpecificErroMessage}{ErrorMessageForParentWithValidationError}",
									UnitTestUserNotification.Instance.LastMessage.Text);
						}

						form.BusinessEntity.HasChanges = true;

						form.Close();
					};
				}

				if (numberOfRequests > 1)
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				}
				else
				{
					UnitTestUserNotification.Instance.AddAnswer(
						numberOfEditedRequests > 0
							? DialogResult.Yes
							: DialogResult.No);
				}
			}

			saveAndPostButton.PerformClick();

			if (numberOfRequestsWithExceptions > 0)
			{
				if (numberOfRequests > 1)
				{
					AssertEquals("All approvals left should be those posted with error.",
						((TransactionApprovalBulk<TransactionType, RequestType, DetailsType>)bulkApprovalForm.BusinessEntity).Approvals.Count,
						((TransactionApprovalBulk<TransactionType, RequestType, DetailsType>)bulkApprovalForm.BusinessEntity).Approvals.Where(approval => approval.HasRowErrors).Count());
					Assert("Form should not be closed when any approval with errors after posting.", bulkApprovalForm.Visible);

					AssertEquals(BulkPostingMessageAboutErrors, UnitTestUserNotification.Instance.LastMessage.Text);

					var approvalGrid = bulkApprovalForm
						.GetControl<ZGrid>("TopGrid");
					var editOption = approvalGrid.ContextMenu.MenuItems.FindByText("Edit", true);
					AssertNotNull(editOption);

					AssertEquals("All approvals with error should still be on the grid, others should be removed from.", numberOfRequestsWithExceptions, approvalGrid.VisibleRowCount);

					editOption.PerformClick();
					AssertEquals("Please select at least one approval.", UnitTestUserNotification.Instance.LastMessage.Text);

					if (numberOfEditedRequests > 0)
					{
						for (var rowIndex = 0; rowIndex < numberOfEditedRequests; rowIndex++)
						{
							approvalGrid.Select(rowIndex);
						}

						editOption.PerformClick();

						AssertEquals("Number of left approvals should be the same as those have not been selected to edit.", numberOfRequestsWithExceptions - numberOfEditedRequests, approvalGrid.VisibleRowCount);

						if (numberOfRequestsWithExceptions > numberOfEditedRequests)
						{
							bulkApprovalForm.Close();
						}
					}
					else
					{
						bulkApprovalForm.Close();
					}
				}
				else
				{
					bool isEditingAfterSingleFailedPostingSupported = IsTransactionRelatedRequest;
					if (numberOfEditedRequests == 0 || !isEditingAfterSingleFailedPostingSupported)
					{
						numberOfEditedRequests = 0;
						var messageHeader = !IsTransactionRelatedRequest ? "" :
$@"Error(s) have been found during posting. They can be fixed possibly by editing the related {ParentType}. Do you want to edit it now?
Errors:
";
						AssertEquals(messageHeader + transactionRequestSpecificErroMessage + ErrorMessageForParentWithValidationError, UnitTestUserNotification.Instance.LastMessage.Text);
					}
					else
					{
						AssertEquals(
@"This record has been modified.
Would you like to save the changes?",
						UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
			}

			AssertEquals("Approval that triggered exception should be approved but posted.",
				numberOfRequestsWithExceptions,
				originalApprovals.Where(approval => approval.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Approved).Count());

			AssertEquals("Approval that wasn't triggered exception should be posted.",
				numberOfRequests - numberOfRequestsWithExceptions,
				originalApprovals.Where(approval => approval.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Posted).Count());

			AssertEquals("Edit forms should be as much as to be edited exception approvals.", numberOfEditedRequests, editFormsCount);

			Assert("Bulk approval form should be closed after whole progress.", !bulkApprovalForm.Visible);
		}

		protected string BulkPostingMessageAboutErrors => $"Requests that remained in the Approval Requests grid are not posted due to errors found. They can be fixed possibly by editing the related {ParentType}. Right click > select Edit on the Approval Requests grid to edit the {ParentType}.";

		#endregion

		public override void TestFormParameters()
		{
			using (FormType testForm = (FormType)GetFormToBash())
			{
				testForm.Show();
				var postingButtonsUserControl = testForm.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl");

				AssertEquals(this.ShouldSaveButtonVisibleAtFirst, postingButtonsUserControl.SaveButton.Visible);
				AssertEquals(ODisplayMode.Edit, testForm.DisplayMode);
				AssertEquals("S&ave && Close", postingButtonsUserControl.SaveAndCloseButton.Text);
				AssertEquals("TopGridPanel.Visible", true, testForm.GetControl<ZPanel>("TopGridPanel").Visible);
				AssertEquals("TopSingleRequestPanel.Visible", false, testForm.GetControl<ZPanel>("TopSingleRequestPanel").Visible);
				AssertEquals("ReasonDescriptionTextBox.ReadOnly", true, testForm.GetControl<ZTextBox>("ReasonDescriptionTextBox").ReadOnly);
				AssertEquals("FormVerb", "Approve", testForm.FormVerb);

				var formBizo = (TransactionApprovalBulk<TransactionType, RequestType, DetailsType>)testForm.BusinessEntity;
				formBizo.RunPreSaveValidation();
				AssertNoErrors("Precondition: ", formBizo);
				postingButtonsUserControl.SaveAndCloseButton.PerformClick();
				AssertEquals("Factory must not be saved.", true, formBizo.Approvals[0].IsInDatabase);
				AssertEquals("Factory must not be saved.", true, formBizo.Approvals[1].IsInDatabase);
			}
		}

		#region Implementation

		protected RequestType[] GetApprovalRequests(int numberOfRequests, int numberOfRequestsWithExceptions)
		{
			Assert("testApprovalsCount should be no less than numberOfTriggeredExceptionApprovals.", numberOfRequests >= numberOfRequestsWithExceptions);

			var testApprovals = new RequestType[numberOfRequests];
			for (var index = 0; index < numberOfRequests; index++)
			{
				testApprovals[index] = GetNewApprovalRequest(numberOfRequestsWithExceptions-- <= 0);
				testApprovals[index].XP_ReasonDescription = "Test";
				testApprovals[index].XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
			}

			Factory.Save();

			return testApprovals;
		}

		protected sealed override RequestType GetNewApprovalRequest()
		{
			return GetNewApprovalRequest(true);
		}

		protected virtual RequestType GetNewApprovalRequest(bool isValidParent)
		{
			throw new NotImplementedException();
		}

		protected virtual string ErrorMessageForParentWithValidationError
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		protected virtual void SetPostingSecurityRight(bool isAllowed)
		{
			throw new NotImplementedException();
		}

		protected virtual string EditFormTypeName
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		protected override Form GetFormToBashCore()
		{
			var formBizo1 = GetNewApprovalRequest();
			formBizo1.XP_ParentID = ZGuid.NewZGuid();
			formBizo1.XP_ReasonDescription = "Desc";
			formBizo1.XP_ReasonCode = "IOB";
			var formBizo2 = GetNewApprovalRequest();
			formBizo2.XP_ParentID = ZGuid.NewZGuid();
			formBizo2.XP_ReasonDescription = "Desc";
			formBizo2.XP_ReasonCode = "IOB";
			return GetRequestForm(formBizo1, formBizo2);
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));

			ApprovalFormMode = TransactionApprovalFormModes.Approve;
		}

		protected AccGLHeader InactiveTestGLAccount
		{
			get
			{
				if (inactiveTestGLAccount == null)
				{
					inactiveTestGLAccount = TestObjectCreator.CreateAccGLHeader("1100.90.00", "TS", "Test PNL 0", "P&L", Core.Constants.DebitCredit.Debit);
					inactiveTestGLAccount.AG_IsActive = false;
				}

				return inactiveTestGLAccount;
			}
		}
		AccGLHeader inactiveTestGLAccount;

		protected virtual ZString ParentType => ZString.Empty;

		protected virtual bool IsRequestIDDependsOnXP_ParentID => false;

		protected virtual bool IsTransactionRelatedRequest => true;

		#endregion
	}
}
