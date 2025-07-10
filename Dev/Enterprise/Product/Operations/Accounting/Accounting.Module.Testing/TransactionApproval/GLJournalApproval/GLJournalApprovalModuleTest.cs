using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.GUI.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.DocumentScanning.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(GLJournalApprovalModule))]
	class GLJournalApprovalModuleTest : TransactionApprovalModuleTest<GLJournal, GLJournalApprovalRequest, GLJournalApprovalRequestDetails>
	{
		public void TestApproveAndPostPerformance_RequestXMLDeserializationCount()
		{
			var bizo1 = GetNewApprovalRequest("1");
			bizo1.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			bizo1.RunPreSaveValidation();
			AssertNoErrors(bizo1);
			Factory.Save();

			GLJournalApprovalRequest.ReadXMLFromBlobAndDeserialize_CallsCount_ForTestOnly = 0;

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals("Precondition: GridCollection.Count", 1, module.GridCollection.Count);

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals("Precondition: FilteredGrid is found.", 1, grids.Length);
					var grid = (ZGrid)grids[0];
					grid.SelectAllElements();

					var menuItem = module.FormActionMenu.FindByText("Approve", true);
					AssertNotNull("Precondition: Approve menu item is found.", menuItem);
					menuItem.PerformClick();

					var lastForm = (ZForm)ZFormModaliser.LastFormShownForTest;
					AssertNotNull(lastForm);
					AssertType("Postcondition: LastFormShownForTest", ApprovalBulklastFormType, lastForm);
					var saveAndPostButton = lastForm
						.GetControl<ZPostingButtonsUserControl>("PostingButtonsUserControl")
						.SaveButton;
					saveAndPostButton.PerformClick();
					lastForm.Dispose();
					AssertEquals("Postcondition: XP_ApprovalStatus.", Constants.GenApprovalRequestApprovalStatus.Posted, bizo1.XP_ApprovalStatus);

					AssertEquals("ReadXMLFromBlobAndDeserialize_CallsCount_ForTestOnly", 2, GLJournalApprovalRequest.ReadXMLFromBlobAndDeserialize_CallsCount_ForTestOnly);
				}
			}
		}

		public void TestApproveWhenOwnJournalApprovalIsNotAllowed()
		{
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertStatusAction("Approve", Core.Constants.GenApprovalRequestApprovalStatus.Approved, allowActionForOwnRequest: false, loginUnderAnotherUser: false);
		}

		public void TestApproveWhenOwnJournalApprovalIsNotAllowed_UnderAnotherUser()
		{
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertStatusAction("Approve", Core.Constants.GenApprovalRequestApprovalStatus.Approved, allowActionForOwnRequest: false, loginUnderAnotherUser: true);
		}

		public void TestRejectWhenOwnJournalApprovalIsNotAllowed()
		{
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertStatusAction("Reject", Core.Constants.GenApprovalRequestApprovalStatus.Rejected, allowActionForOwnRequest: false, loginUnderAnotherUser: false);
		}

		public void TestRejectWhenOwnJournalApprovalIsNotAllowed_UnderAnotherUser()
		{
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertStatusAction("Reject", Core.Constants.GenApprovalRequestApprovalStatus.Rejected, allowActionForOwnRequest: false, loginUnderAnotherUser: true);
		}

		public void TestCancelWhenOwnJournalApprovalIsNotAllowed()
		{
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertStatusAction("Cancel", Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, allowActionForOwnRequest: false, loginUnderAnotherUser: false);
		}

		public void TestCancelWhenOwnJournalApprovalIsNotAllowed_UnderAnotherUser()
		{
			AccountingConfigurationRegistry.Instance.AllowUsersToApproveOwnGLJournals.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertStatusAction("Cancel", Core.Constants.GenApprovalRequestApprovalStatus.Cancelled, allowActionForOwnRequest: false, loginUnderAnotherUser: true);
		}

		public void TestMenuItems()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals("&View", module.FormActionMenu[0].Text);
				AssertEquals("&Edit", module.FormActionMenu[1].Text);
				AssertEquals("&Post", module.FormActionMenu[2].Text);
			}
		}

		public void TestEdit_InvalidType()
		{
			using (var module = (GLJournalApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
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

					var menuItem = module.FormActionMenu.FindByText("Edit", true);
					AssertNotNull(menuItem);

					menuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Posted;
					bizo1.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);
					grid.SelectAllElements();

					menuItem.PerformClick();
					AssertEquals(string.Format("Can't Edit request (00000001) - Only requests with status 'Requested' can be edited."), UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("Controller should not be created", module.LastController_ForTestOnly);
				}
			}
		}

		public void TestEdit()
		{
			using (var module = (GLJournalApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
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

					var menuItem = module.FormActionMenu.FindByText("Edit", true);
					AssertNotNull(menuItem);

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Posted;
					bizo1.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);
					grid.SelectAllElements();

					var allStatusesButRequested = typeof(Constants.GenApprovalRequestApprovalStatus).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).
						Where(x => x != Core.Constants.GenApprovalRequestApprovalStatus.Requested);

					foreach (var status in allStatusesButRequested)
					{
						bizo1.XP_ApprovalStatus = status;
						Factory.Save();
						menuItem.PerformClick();
						AssertEquals(string.Format("Can't Edit request (00000001) - Only requests with status 'Requested' can be edited."), UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}

					bizo1.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Requested;
					Factory.Save();
					Env.Security.GLJournalApprovalEdit.IsAllowed = false;
					menuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var lastForm = (ZForm)module.LastController_ForTestOnly.LastShownForm;
					AssertStartsWith("When user don't have rights View form should be shown", "View Waiting Approval GL Journal", lastForm.Text);
					lastForm.Close();

					Env.Security.GLJournalApprovalEdit.IsAllowed = true;
					menuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					lastForm = (ZForm)module.LastController_ForTestOnly.LastShownForm;
					AssertStartsWith("Form caption", "New Waiting Approval GL Journal", lastForm.Text);
					var formBizo = (GLJournal)lastForm.BusinessEntity;
					Assert("Bizo in Editing context", formBizo.HasContext(GLJournalApprovalRequest.Context.Editing));
					AssertEquals("GLJournal is restored", 2, formBizo.Lines.Count);
					lastForm.Close();
				}
			}
		}

		public void TestPost_InvalidType()
		{
			using (var module = (GLJournalApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
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

					var menuItem = module.FormActionMenu.FindByText("Post", true);
					AssertNotNull(menuItem);

					menuItem.PerformClick();
					AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Posted;
					var bizo2 = GetNewApprovalRequest("2");
					bizo2.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Posted;
					bizo1.RunPreSaveValidation();
					bizo2.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					AssertNoErrors(bizo2);
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(2, module.GridCollection.Count);
					grid.SelectAllElements();

					menuItem.PerformClick();
					AssertStartsWith("LastMessage", "The following request(s) cannot be posted/reversed.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("LastMessage", "Request (00000001) - Only requests with status 'Approved' can be posted/reversed.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertContains("LastMessage", "Request (00000002) - Only requests with status 'Approved' can be posted/reversed.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("Controller should not be created", module.LastController_ForTestOnly);
				}
			}
		}

		public void TestPost()
		{
			using (var module = (GLJournalApprovalModule)ZModuleFactory.Instance.Create(GetModuleID()))
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

					var menuItem = module.FormActionMenu.FindByText("Post", true);
					AssertNotNull(menuItem);

					var bizo1 = GetNewApprovalRequest("1");
					bizo1.RunPreSaveValidation();
					AssertNoErrors(bizo1);
					((IDocManagerSupport)bizo1).DocManagerInfo.AddFileOrDocument(new ZBlob(new byte[] { 1, 2, 3 }), "TestFile1", "INV");
					Factory.Save();

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(1, module.GridCollection.Count);
					grid.SelectAllElements();

					var allStatusesButApproved = typeof(Constants.GenApprovalRequestApprovalStatus).GetFields(BindingFlags.Static | BindingFlags.Public).Select(x => (string)x.GetValue(null)).
						Where(x => x != Core.Constants.GenApprovalRequestApprovalStatus.Approved);

					foreach (var status in allStatusesButApproved)
					{
						bizo1.XP_ApprovalStatus = status;
						Factory.Save();
						grid.SelectAllElements();
						menuItem.PerformClick();
						AssertEquals("LastMessage",
@"The following request(s) cannot be posted/reversed.
Request (00000001) - Only requests with status 'Approved' can be posted/reversed.
", UnitTestUserNotification.Instance.LastMessage.Text);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					}

					bizo1.XP_ApprovalStatus = Core.Constants.GenApprovalRequestApprovalStatus.Approved;
					Factory.Save();
					Env.Security.GLJournalApprovalPost.IsAllowed = false;
					grid.SelectAllElements();
					menuItem.PerformClick();
					AssertEquals("LastMessage", Env.Security.GLJournalApprovalPost.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					AssertNull("When user don't have rights form should not be shown", module.LastController_ForTestOnly);

					Env.Security.GLJournalApprovalPost.IsAllowed = true;
					grid.SelectAllElements();
					menuItem.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					var lastForm = (ZForm)module.LastController_ForTestOnly.LastShownForm;
					AssertStartsWith("Form caption", "Post GL Journal", lastForm.Text);
					var formBizo = (GLJournal)lastForm.BusinessEntity;
					Assert("Bizo in Posting context", formBizo.HasContext(GLJournalApprovalRequest.Context.Posting));
					AssertEquals("GLJournal is restored", 2, formBizo.Lines.Count);
					AssertNotEquals("GLJournal should not be in the module factory.", module.GridCollection.Factory._Instance, formBizo.Factory._Instance);
					lastForm.FireSaveButton();
					lastForm.Close();

					DocumentFactory newDocumentFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
					var storageMain = newDocumentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, bizo1.PK));
					AssertNull("eDocs should not link to request", storageMain);
					storageMain = newDocumentFactory.LoadTop1<StorageMain>(new ZQuery(StorageMainSchema.SM_ParentFK, formBizo.PK));
					AssertNotNull("eDocs should link to journal", storageMain);
					AssertEquals("journal should have 1 eDocs now", 1, storageMain.eDocs.Count);
					AssertEquals("GLJ", storageMain.SM_Type);
				}
			}
		}

		public override void TestRejectWithConcurrencySenario()
		{
			var moduleAction = "Reject";
			// GLJournal has own number generator.
			var expectedErrorMsg = "Can't Reject - These approvals are NOT in allowed REQ status: 00000001";
			TestBehaviorWithConcurrencySenarioCore(moduleAction, expectedErrorMsg);
		}

		public override void TestCancelWithConcurrencySenario()
		{
			var moduleAction = "Cancel";
			// GLJournal has own number generator.
			var expectedErrorMsg = "Can't Cancel - These approvals are already posted, canceled, or have unfinished Electronic Invoicing process: 00000001";
			TestBehaviorWithConcurrencySenarioCore(moduleAction, expectedErrorMsg);
		}

		public override void TestApproveWithConcurrencySenario()
		{
			var moduleAction = "Approve";
			// GLJournal has own number generator.
			var expectedErrorMsg = "Can't Approve - These approvals are NOT in allowed REQ status: 00000001";
			TestBehaviorWithConcurrencySenarioCore(moduleAction, expectedErrorMsg);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GLJournalApproval;
		}

		protected override GLJournalApprovalRequest GetNewApprovalRequest(string jobNumber)
		{
			var journal = TestObjectCreator.CreateGLJournal<GLJournal>(TransactionTypes.GLStandardJournal, ZDateTime.Today, ZDateTime.Today.AddMonths(-1));
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.CR, TestObjectCreator.ExchangeGainLossControlAccount.PK);
			TestObjectCreator.CreateGLJournalLine(journal, 10, DebitCredit.DR, TestObjectCreator.ExchangeGainLossAdjustmentAccount.PK);
			var approval = Factory.NewWithValidTestData<GLJournalApprovalRequest>();
			approval.Initialize(journal);
			approval.XP_ReasonDescription = "Desc";
			approval.XP_ReasonCode = "DAM";
			return approval;
		}

		protected override void SetupSecurity(bool allowSecurity)
		{
			Env.Security.GeneralLedgerJournal_FirstApproval.IsAllowed = allowSecurity;
			Env.Security.GeneralLedgerJournal_SecondApproval.IsAllowed = allowSecurity;

			var newValue = new GLJournalApprovalThresholdCollection();
			var threshold = newValue.AddNew();
			threshold.Type = GLJournalApprovalThreshold.TypeCodes.All;
			var settings = threshold.AuthorisationSettings.AddNew();
			settings.Range = AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.UpTo;
			settings.Amount = 200;
			settings.AuthorisationRequirement = AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			settings = threshold.AuthorisationSettings.AddNew();
			settings.Range = AmountBasedThreeLevelAuthorisationRequirement.RangeCodes.Above;
			settings.Amount = 200;
			settings.AuthorisationRequirement = AmountBasedThreeLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.NewGLJournalApprovalThresholdSetup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, newValue);
		}

		protected override Type ApprovalBulklastFormType
		{
			get { return typeof(GLJournalApprovalBulkForm); }
		}

		protected override string ExpectedUserDoesntHaveCancelRightMessage
		{
			get { return null; }
		}

		protected override SecurityCheckpoint CheckpointForCancel
		{
			get { return null; }
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			GetNewApprovalRequest("1");
			Factory.Save();
		}

		protected override bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Edit => true;

		protected override void SetUp()
		{
			base.SetUp();

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today.AddMonths(-3));
		}
	}
}
