using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals.Testing
{
	[TestedType(typeof(GLJournalApprovalBulkForm))]
	class GLJournalApprovalBulkFormBasherTest :
		TransactionApprovalBulkFormBasherTest<GLJournalApprovalBulkForm, GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails>
	{
		public void TestGLJournalUserControl()
		{
			using (var testForm = GetFormToBash())
			{
				var journalTabControl = testForm.GetControl<ZTabControl>("JournalTabControl");
				var journalTabPage = testForm.GetControl<ZTabPage>("JournalTabPage");
				journalTabControl.SelectedTab = journalTabPage;
				testForm.Show();

				var glJournalUserControl = testForm.GetControl<GLJournalUserControl>("glJournalUserControl");
				Assert("ShowApprovalRequestControls", !glJournalUserControl.ShowApprovalRequestControls);
			}
		}

		protected override GLJournalApprovalRequest GetNewApprovalRequest()
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);

			var request = Factory.New<GLJournalApprovalRequest>();
			request.Initialize(journal);

			return request;
		}

		protected override GLJournalApprovalBulkForm GetRequestForm(params GLJournalApprovalRequest[] bizos)
		{
			return new GLJournalApprovalBulkForm(new GLJournalApprovalBulk(Factory, new InteractiveSecurityOverrideProvider(), bizos), ApprovalFormMode);
		}

		protected override TransactionApprovalFormModes[] ModesWhenReasonDescriptionShouldNotBeReadOnly
		{
			get { return new TransactionApprovalFormModes[] { TransactionApprovalFormModes.SetDescription, TransactionApprovalFormModes.Reject }; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
		}
	}

	[TestedType(typeof(GLJournalApprovalBulkForm))]
	class GLJournalApprovalBulkFormWithMultipleRequestsBasherTest :
		TransactionApprovalBulkFormWithMultipleRequestsBasherTest<GLJournalApprovalBulkForm, GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails>
	{
		public void TestJournalApproveAndPostPerformance_RequestXMLDeserializationCount()
		{
			GLJournalApprovalRequest.ReadXMLFromBlobAndDeserialize_CallsCount_ForTestOnly = 0;

			var requests = GetApprovalRequests(1, 0);
			AssertEquals("Precondition: Approvals.Count", 1, requests.Length);
			var originalApproval = requests[0];

			ApprovalFormMode = TransactionApprovalFormModes.Approve;
			using (var bulkApprovalForm = GetRequestForm(requests))
			{
				bulkApprovalForm.Show();
				Application.DoEvents();

				var saveAndPostButton = bulkApprovalForm
					.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
					.SaveButton;

				saveAndPostButton.PerformClick();

				AssertEquals("Postcondition: XP_ApprovalStatus.", Constants.GenApprovalRequestApprovalStatus.Posted,
					originalApproval.XP_ApprovalStatus);

				AssertEquals("ReadXMLFromBlobAndDeserialize_CallsCount_ForTestOnly", 2, GLJournalApprovalRequest.ReadXMLFromBlobAndDeserialize_CallsCount_ForTestOnly);
			}
		}

		public void TestEDocsTabPage()
		{
			foreach (TransactionApprovalFormModes mode in Enum.GetValues(typeof(TransactionApprovalFormModes)))
			{
				ApprovalFormMode = mode;
				using (var testForm = GetRequestForm(GetApprovalRequests(2, 1)))
				{
					try
					{
						var eDocsTabPage = testForm.GetControl<ZTabPage>("EDocsTabPage");
						Assert("eDocs tab page is visible", eDocsTabPage.TabVisible);
					}
					catch
					{
						AssertEquals("eDocs tab page is not visible when mode is SetDescription", TransactionApprovalFormModes.SetDescription, mode);
					}
				}
			}
		}

		public void TestInitializeEDocsTab()
		{
			ApprovalFormMode = TransactionApprovalFormModes.View;
			using (var testForm = GetRequestForm(GetApprovalRequests(2, 1)))
			{
				var journalTabControl = testForm.GetControl<ZTabControl>("JournalTabControl");
				var eDocsTabPage = testForm.GetControl<ZTabPage>("EDocsTabPage");
				journalTabControl.SelectedTab = eDocsTabPage;
				testForm.Show();
				AssertNotNull(testForm.eDocPlugIn);
				Assert(testForm.eDocPlugIn.HostBusinessObject is GLJournalApprovalRequest);
				Assert("eDoc user control should be readonly", testForm.edocUserControl.ReadOnly);
			}
		}

		public void TestLogsWillBeAddedForJournalOfApproval()
		{
			var requests = GetApprovalRequests(1, 0);
			ApprovalFormMode = TransactionApprovalFormModes.Approve;
			var bulkApprovalForm = GetRequestForm(requests);

			var approval = requests[0];

			var editFormsShownTimes = 0;

			bulkApprovalForm.OnEditFormShown_ForTestOnly += (sender, e) =>
			{
				editFormsShownTimes++;
				(sender as GLJournalForm).Close();
			};

			var saveAndPostButton = bulkApprovalForm
				.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
				.SaveButton;

			bulkApprovalForm.Show();

			saveAndPostButton.PerformClick();

			var journal = approval.GetLinkedJournal().journal;
			var logsCount = journal.Logs.DatabaseCount;

			journal = new BusinessObjectFactory().Load<GLJournal>(journal.PK);

			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);

			approval = Factory.New<GLJournalApprovalRequest>();
			approval.Initialize(journal);
			approval.XP_ReasonDescription = "Test";
			approval.XP_ApprovalStatus = Enterprise.Core.Constants.GenApprovalRequestApprovalStatus.Approved;

			journal = Factory.Load<GLJournal>(journal.PK);
			AssertEquals("Journal in database should have origin line numbers.", 2, journal.Lines.Count);

			Factory.Save();

			bulkApprovalForm = GetRequestForm(approval);

			saveAndPostButton = bulkApprovalForm
				.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
				.SaveButton;

			bulkApprovalForm.Show();

			saveAndPostButton.PerformClick();

			journal = approval.GetLinkedJournal().journal;
			var currentLogsCount = journal.Logs.DatabaseCount;

			Assert("New logs should be added after posting approval for existing journal.", currentLogsCount > logsCount);
		}

		public void TestApprovalRequestIsNotPostedWhenAnotherUserCurrentlyAccessingThisJournal()
		{
			var initiator = Factory.NewWithValidTestData<GlbStaff>();
			initiator.GS_LoginName = "David Park";
			initiator.GS_Code = "DP";
			Factory.Save();

			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);

			var approvalsRequest = Factory.New<GLJournalApprovalRequest>();
			approvalsRequest.Initialize(journal);
			approvalsRequest.XP_ReasonDescription = "Test";
			approvalsRequest.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Approved;
			approvalsRequest.XP_ParentID = journal.PK;
			Factory.Save();

			using (var form1 = new GLJournalForm(journal))
			{
				form1.DisplayMode = ODisplayMode.Edit;
				form1.Show();
				Application.DoEvents();
				using (Env.SetTemporaryUserContext(initiator.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					Env.Security.GLJournalApprovalPost.IsAllowed = true;
					ApprovalFormMode = TransactionApprovalFormModes.Approve;
					using (var form2 = GetRequestForm(approvalsRequest))
					{
						form2.Show();
						var saveAndPostButton = form2.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl").SaveButton;
						saveAndPostButton.PerformClick();

						Assert("Another user is editing", UnitTestUserNotification.Instance.LastMessage.Text.Contains("The GL Journal is currently being changed by user 'CargoWise Support'"));
					}
				}
			}
		}

		protected override bool ShouldSaveButtonVisibleAtFirst => true;

		protected override GLJournalApprovalRequest GetNewApprovalRequest(bool isValidParent)
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			if (!isValidParent)
			{
				TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, InactiveTestGLAccount.PK);
				TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, InactiveTestGLAccount.PK);
			}

			var request = Factory.New<GLJournalApprovalRequest>();
			request.Initialize(journal);

			return request;
		}

		protected override string ErrorMessageForParentWithValidationError =>
@"GL Post To Account: This GL Post To Account is inactive - it may not be used.
GL Post To Account: This account is currently marked as inactive";

		protected override void SetPostingSecurityRight(bool isAllowed)
		{
			Env.Security.GLJournalApprovalPost.IsAllowed = isAllowed;
		}

		protected override string EditFormTypeName => "GLJournalForm";

		protected override GLJournalApprovalBulkForm GetRequestForm(params GLJournalApprovalRequest[] bizos)
		{
			return new GLJournalApprovalBulkForm(new GLJournalApprovalBulk(Factory, new InteractiveSecurityOverrideProvider(), bizos), ApprovalFormMode);
		}

		protected override TransactionApprovalFormModes[] ModesWhenReasonDescriptionShouldNotBeReadOnly
		{
			get { return new TransactionApprovalFormModes[] { TransactionApprovalFormModes.SetDescription, TransactionApprovalFormModes.Reject }; }
		}

		protected override ZString ParentType => "journal";
	}
}
