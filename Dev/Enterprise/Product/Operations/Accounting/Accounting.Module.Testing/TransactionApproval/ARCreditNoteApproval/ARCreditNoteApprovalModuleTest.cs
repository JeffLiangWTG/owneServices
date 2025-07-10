using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.TransactionApproval.Testing
{
	[TestedType(typeof(ARCreditNoteApprovalModule))]
	class ARCreditNoteApprovalModuleTest : TransactionApprovalModuleTest<InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails>
	{
		public override void TestApprove_UnderAnotherUser() => AssertHandleAction("Approve", true, "You don't have rights to approve selected requests.");
		public override void TestReject_UnderAnotherUser() => AssertHandleAction("Reject", true, "You don't have rights to reject selected requests.");
		public override void TestCancel_UnderAnotherUser() => AssertHandleAction("Cancel", true, @"You can't cancel selected requests. 
A request can be canceled only by the user who either created it or has rights to approve it. Neither of those conditions were satisfied.");

		public override void TestApprove() => AssertHandleAction("Approve", false, "You don't have rights to approve selected requests.");
		public override void TestApprove_IfCountryComplianceIsImplementedToEInvoicingRequestProvider() =>
			Assert("Since ARCreditNoteApprovalModuleTest has its own independent unit tests, this test is overridden to prevent it from failing the virtual one.", true);
		public override void TestApprove_IfCountryComplianceIsImplementedToEInvoicingRequestProvider_UnderAnotherUser() =>
			Assert("Since ARCreditNoteApprovalModuleTest has its own independent unit tests, this test is overridden to prevent it from failing the virtual one.", true);
		public override void TestReject() => AssertHandleAction("Reject", false, "You don't have rights to reject selected requests.");
		public override void TestReject_IfCountryComplianceIsImplementedToEInvoicingRequestProvider() =>
			Assert("Since ARCreditNoteApprovalModuleTest has its own independent unit tests, this test is overridden to prevent it from failing the virtual one.", true);
		public override void TestReject_IfCountryComplianceIsImplementedToEInvoicingRequestProvider_UnderAnotherUser() =>
			Assert("Since ARCreditNoteApprovalModuleTest has its own independent unit tests, this test is overridden to prevent it from failing the virtual one.", true);
		public override void TestCancel() => AssertHandleAction("Cancel", false, @"You can't cancel selected requests. 
A request can be canceled only by the user who either created it or has rights to approve it. Neither of those conditions were satisfied.");

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		void AssertHandleAction(string actionName, bool requestsCreatedByOtherUser, string noValidApprovalsMessage)
		{
			SetupSecurity(false);
			var newStaff = TestObjectCreator.CreateStaffWithSecurityRights("NewStaff", "NST", Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code);

			if (!requestsCreatedByOtherUser)
			{
				Env.SetUserContext(new UserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK));
			}

			var level1Request1 = GetNewApprovalRequest("01");
			level1Request1.PostingDetails.MaxAmountToApprove = 150;
			var level1Request2 = GetNewApprovalRequest("02");
			level1Request2.PostingDetails.MaxAmountToApprove = 150;
			var level2Request1 = GetNewApprovalRequest("03");
			level2Request1.PostingDetails.MaxAmountToApprove = 300;
			var level2Request2 = GetNewApprovalRequest("04");
			level2Request2.PostingDetails.MaxAmountToApprove = 300;

			Factory.Save();

			Env.SetUserContext(new UserContext(newStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK));
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (ZForm form = new ZForm())
			{
				try
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];

					var menuItem = module.FormActionMenu.FindByText(actionName, true);
					AssertNotNull(menuItem);
					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(4, module.GridCollection.Count);
					grid.SelectAllElements();

					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					menuItem.PerformClick();
					ARCreditNoteApprovalBulk approvalBulk;
					if (actionName == "Cancel" && !requestsCreatedByOtherUser)
					{
						approvalBulk = (ZFormModaliser.LastFormShownForTest as ARCreditNoteApprovalBulkForm).BusinessEntity as ARCreditNoteApprovalBulk;
						AssertNotNull(approvalBulk);
						AssertEquals(4, approvalBulk.Approvals.Count);
						var pks = approvalBulk.Approvals.Select(x => x.PK);
						AssertContainsExactElementsInAnyOrder(new ZGuid[] { level1Request1.PK, level1Request2.PK, level2Request1.PK, level2Request2.PK }, pks);
					}
					else
					{
						AssertCheckDeniedMessage();
						AssertNull("Bulk form not shown when no is selected", ZFormModaliser.LastFormShownForTest);
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

						UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
						menuItem.PerformClick();
						AssertCheckDeniedMessage();
						AssertEquals(typeof(ARCreditNoteApprovalBulkForm), ZFormModaliser.LastFormShownForTest.GetType());
						approvalBulk = (ZFormModaliser.LastFormShownForTest as ARCreditNoteApprovalBulkForm).BusinessEntity as ARCreditNoteApprovalBulk;
						AssertNotNull(approvalBulk);
						AssertEquals(2, approvalBulk.Approvals.Count);
						var pks = approvalBulk.Approvals.Select(x => x.PK);
						AssertContainsExactElementsInAnyOrder(new ZGuid[] { level1Request1.PK, level1Request2.PK }, pks);
					}
					ZFormModaliser.LastFormShownForTest.Dispose();
					ZFormModaliser.LastFormShownForTest = null;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					grid.SelectSingleElementByPK(level1Request1.PK);
					menuItem.PerformClick();
					AssertNull("No prompt is shown when only approvals user has rights for are selected", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(typeof(ARCreditNoteApprovalBulkForm), ZFormModaliser.LastFormShownForTest.GetType());
					approvalBulk = (ZFormModaliser.LastFormShownForTest as ARCreditNoteApprovalBulkForm).BusinessEntity as ARCreditNoteApprovalBulk;
					AssertNotNull(approvalBulk);
					AssertEquals(1, approvalBulk.Approvals.Count);
					AssertEquals(level1Request1.PK, approvalBulk.Approvals[0].PK);
					ZFormModaliser.LastFormShownForTest.Dispose();
					ZFormModaliser.LastFormShownForTest = null;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					if (actionName == "Cancel" && !requestsCreatedByOtherUser)
					{
						grid.SelectSingleElementByPK(level2Request1.PK);
						menuItem.PerformClick();
						Assert("Cancelling own request does not result in an error", UnitTestUserNotification.Instance.LastMessage.WasNone);
						AssertEquals(typeof(ARCreditNoteApprovalBulkForm), ZFormModaliser.LastFormShownForTest.GetType());
						approvalBulk = (ZFormModaliser.LastFormShownForTest as ARCreditNoteApprovalBulkForm).BusinessEntity as ARCreditNoteApprovalBulk;
						AssertNotNull(approvalBulk);
						AssertEquals(1, approvalBulk.Approvals.Count);
						AssertEquals(level2Request1.PK, approvalBulk.Approvals[0].PK);
					}
					else
					{
						grid.SelectSingleElementByPK(level2Request1.PK);
						menuItem.PerformClick();
						Assert("Module's error Message not used when login form is opened", UnitTestUserNotification.Instance.LastMessage.WasNone);
						AssertEquals("User should be prompted for access when only one request is selected and user does not have security rights", "Security Override Login", ZFormModaliser.LastFormShownDialogForTest.Text);
						AssertNull(ZFormModaliser.LastFormShownForTest);
					}
					ZFormModaliser.LastFormShownForTest?.Dispose();
					ZFormModaliser.LastFormShownForTest = null;
					ZFormModaliser.LastFormShownDialogForTest = null;

					var filter = (ARCreditNoteApprovalFilterBusinessObject)module.FilterBusinessObject;
					var selectParentFilter = (ModuleSQLFilter)filter["Custom SQL Filter"];
					selectParentFilter.IsActive = true;
					selectParentFilter.Property1 = $"{GenApprovalRequestSchema.PK.Name} in ('{level2Request1.PK}', '{level2Request2.PK}')";

					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(2, module.GridCollection.Count);
					grid.SelectAllElements();

					menuItem.PerformClick();
					if (actionName == "Cancel" && !requestsCreatedByOtherUser)
					{
						Assert("Expect no error message when cancelling own requests", UnitTestUserNotification.Instance.LastMessage.WasNone);
						AssertEquals("User should not be prompted for access when more than one request is selected", null, ZFormModaliser.LastFormShownDialogForTest);
						AssertEquals(typeof(ARCreditNoteApprovalBulkForm), ZFormModaliser.LastFormShownForTest.GetType());
					}
					else
					{
						AssertEquals("Error message is shown when only approvals user has no rights for are selected", noValidApprovalsMessage, UnitTestUserNotification.Instance.LastMessage.Text);
						AssertEquals("User should not be prompted for access when more than one request is selected", null, ZFormModaliser.LastFormShownDialogForTest);
						AssertNull(ZFormModaliser.LastFormShownForTest);
					}
				}
				finally
				{
					ZFormModaliser.LastFormShownForTest?.Dispose();
				}
			}

			void AssertCheckDeniedMessage()
			{
				AssertContains($"You do not have the required authorization level to {actionName.ToLower()} the following requests:", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"03 {Env.CurrentBranch.Code} {Env.CurrentDepartment.Code}", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"04 {Env.CurrentBranch.Code} {Env.CurrentDepartment.Code}", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains($"Would you like to {actionName.ToLower()} the remaining requests?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestNotAllowedToApproveMessageWithSingleApprovalSelection()
		{
			AssertNotAllowedToApproveMessage(true);
		}

		public void TestNotAllowedToApproveMessageWithMultiApprovalsSelection()
		{
			AssertNotAllowedToApproveMessage(false);
		}

		void AssertNotAllowedToApproveMessage(bool singleSelection)
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GetAuthorisationConfigSetting("TWO", true));
			var moduleAction = "Approve";

			var approval = CreateApprovalWithChild();

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];

					var expectedItemsCount = 2;
					if (singleSelection)
					{
						expectedItemsCount = 1;
						var filter = (ARCreditNoteApprovalFilterBusinessObject)module.FilterBusinessObject;
						var selectParentFilter = (ModuleSQLFilter)filter["Custom SQL Filter"];
						selectParentFilter.IsActive = true;
						selectParentFilter.Property1 = string.Format("{0} = '{1}'", GenApprovalRequestSchema.PK.Name, approval.PK);
					}

					var menuItem = module.FormActionMenu.FindByText(moduleAction, true);
					AssertNotNull(menuItem);
					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(expectedItemsCount, module.GridCollection.Count);
					grid.SelectAllElements();
					menuItem.PerformClick();

					var expectedErrorMsg = singleSelection ?
						@"This request cannot be approved until all related requests are in Approved status.
Please approve the related requests first." :
						@"One of the selected requests is a parent request and it cannot be approved until all related requests are in Approved status.
Please approve the related requests first.";

					AssertEquals(expectedErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		public void TestNotAllowedToCancelMessageWithSingleApprovalSelection()
		{
			AssertNotAllowedToCancelMessage(true);
		}

		public void TestNotAllowedToCancelMessageWithMultiApprovalsSelection()
		{
			AssertNotAllowedToCancelMessage(false);
		}

		void AssertNotAllowedToCancelMessage(bool singleSelection)
		{
			AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var moduleAction = "Cancel";

			var approval = CreateApprovalWithChild();

			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					var grids = module.EmbeddedControl.Controls.Find("FilteredGrid", true);
					AssertEquals(1, grids.Length);
					var grid = (ZGrid)grids[0];

					var expectedItemsCount = 2;
					if (singleSelection)
					{
						expectedItemsCount = 1;
						var filter = (ARCreditNoteApprovalFilterBusinessObject)module.FilterBusinessObject;
						var selectChildFilter = (ModuleSQLFilter)filter["Custom SQL Filter"];
						selectChildFilter.IsActive = true;
						selectChildFilter.Property1 = string.Format("{0} = '{1}'", GenApprovalRequestSchema.XP_ParentID.Name, approval.PK);
					}

					var menuItem = module.FormActionMenu.FindByText(moduleAction, true);
					AssertNotNull(menuItem);
					((IFilterModuleInternalsForTesting)module).PerformSearch();
					AssertEquals(expectedItemsCount, module.GridCollection.Count);
					grid.SelectAllElements();
					menuItem.PerformClick();

					var expectedErrorMsg = singleSelection ?
						@"This request cannot be canceled as it relates to another request. You can either reject or approve this request.
To cancel all requests in this group, cancel the parent request." :
						@"One of the selected requests is related to another request and it cannot be canceled directly. You can either reject or approve this request.
To cancel all requests in this group, cancel the parent request.";

					AssertEquals(expectedErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		ARCreditNoteApprovalRequest CreateApprovalWithChild()
		{
			var job = TestObjectCreator.CreateJob("S001", TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var approval = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			var creditNote = TestObjectCreator.CreateARCreditNoteWithLine("01", TestObjectCreator.LocalClient, GlbCompany.CurrentCompany.LocalCurrency, 1m, "desc", job, TestObjectCreator.FRT, 150m, ZDateTime.Today, false);
			TestObjectCreator.CreateJobCharge(creditNote.Lines[0], job, TestObjectCreator.FRT, GlbCompany.CurrentCompany.LocalCurrency);
			creditNote.AH_JH = job.PK;

			approval.Initialize(new ARCreditNote[] { creditNote }, job.PK, job.TablePrefix, JobInvoicingPostingOption.All);
			approval.PostingDetails.MaxAmountToApprove = 100;
			approval.XP_ReasonDescription = "Desc";
			approval.XP_ReasonCode = "DAM";

			approval.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			approval.RunPreSaveValidation();
			AssertNoErrors(approval);
			Factory.Save();
			AssertEquals("In CW1 instance1 there is a approval request with status REQ", Constants.GenApprovalRequestApprovalStatus.Requested, approval.XP_ApprovalStatus);

			return approval;
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ARCreditNoteApproval;
		}

		protected override Type ExpectedLoginFormType()
		{
			return typeof(GUI.LoginFormForARCreditNoteApprovalOverride);
		}

		protected override ARCreditNoteApprovalRequest GetNewApprovalRequest(string jobNumber)
		{
			var job = TestObjectCreator.CreateJob(jobNumber, TestObjectCreator.LocalClient, 0, TestObjectCreator.Agent, 0);
			var approval = Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>();
			approval.Initialize(Array.Empty<InvoicingBase>(), job.PK, job.TablePrefix, JobInvoicingPostingOption.All);
			approval.PostingDetails.MaxAmountToApprove = 100;
			approval.XP_ReasonDescription = "Desc";
			approval.XP_ReasonCode = "DAM";

			return approval;
		}

		protected override void SetupSecurity(bool allowSecurity)
		{
			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = allowSecurity;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = allowSecurity;

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

		AuthorizationModeAndSettings GetAuthorisationConfigSetting(string mode = "", bool includeLevelNone = false)
		{
			var result = new AuthorizationModeAndSettings();
			var collection = result.AuthorisationSettings;
			var upToPaymentAuthorisationSettings = collection.AddNew();
			upToPaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			upToPaymentAuthorisationSettings.Amount = 10;
			upToPaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			var abovePaymentAuthorisationSettings = collection.AddNew();
			abovePaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			abovePaymentAuthorisationSettings.Amount = 10;
			abovePaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			if (includeLevelNone)
			{
				upToPaymentAuthorisationSettings = collection.AddNew();
				upToPaymentAuthorisationSettings.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
				upToPaymentAuthorisationSettings.Amount = 5;
				upToPaymentAuthorisationSettings.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			}

			if (!string.IsNullOrWhiteSpace(mode))
			{
				result.AuthorizationMode = mode;
			}

			return result;
		}

		protected override Type ApprovalBulklastFormType
		{
			get { return typeof(ARCreditNoteApprovalBulkForm); }
		}

		protected override string ExpectedUserDoesntHaveCancelRightMessage
		{
			get { return null; }
		}

		protected override SecurityCheckpoint CheckpointForCancel
		{
			get { return null; }
		}
	}
}
