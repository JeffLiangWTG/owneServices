using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing.TestBase;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Base.Trasaction.Approval.Testing
{
	[TestedType(typeof(ARCreditNoteApprovalBulk))]
	public class ARCreditNoteApprovalBulkTest : TransactionApprovalBulkTestBase<ARCreditNoteApprovalBulk, InvoicingBase, ARCreditNoteApprovalRequest, ARCreditNoteApprovalRequestDetails, InvoicingBase>
	{
		[GuiTest]
		public void TestUsersAllowedToCancelOwnRequests()
		{
			var branch = TestObjectCreator.CreateBranch("BRD", GlbCompany.CurrentCompany);
			var dept = TestObjectCreator.CreateDepartment("DPD");
			SetupThreeLevelSecurity(branch, dept, Constants.AuthorizationMode.Codes.TwoApprovers);

			var staff = TestObjectCreator.CreateStaff("US1");
			var otherStaff = TestObjectCreator.CreateStaff("US2");
			var request1 = CreateRequestWithTestData(Constants.AuthorizationMode.Codes.Default, 500, branch.PK, dept.PK, staff);
			var request2 = CreateRequestWithTestData(Constants.AuthorizationMode.Codes.Default, 600, branch.PK, dept.PK, staff);
			var request3 = CreateRequestWithTestData(Constants.AuthorizationMode.Codes.Default, 700, branch.PK, dept.PK, staff);
			var bulkSingleApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request1 }), request1);
			var bulkMultipleApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request2, request3 }), request2, request3);

			using (Env.SetTemporaryUserContext(new UserContext(otherStaff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				DoActionAndAssertFailed(Constants.GenApprovalRequestApprovalStatus.Cancelled, bulkSingleApproval);
				DoActionAndAssertFailed(Constants.GenApprovalRequestApprovalStatus.Cancelled, bulkMultipleApproval);
			}

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Cancelled, true, bulkSingleApproval);
				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Cancelled, true, bulkMultipleApproval);
			}
		}

		[GuiTest]
		public void TestSetARCreditNoteApprovalsStatus_Default_Approve() => AssertSetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Approved, Constants.AuthorizationMode.Codes.Default);
		[GuiTest]
		public void TestSetARCreditNoteApprovalsStatus_Default_Reject() => AssertSetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Rejected, Constants.AuthorizationMode.Codes.Default);
		[GuiTest]
		public void TestSetARCreditNoteApprovalsStatus_Default_Cancel() => AssertSetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Cancelled, Constants.AuthorizationMode.Codes.Default);

		[GuiTest]
		public void TestSetARCreditNoteApprovalsStatus_TwoApprover_Approve() => AssertSetARCreditNoteApprovalsStatus_TwoApprover();
		[GuiTest]
		public void TestSetARCreditNoteApprovalsStatus_TwoApprover_Reject() => AssertSetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Rejected, Constants.AuthorizationMode.Codes.TwoApprovers);
		[GuiTest]
		public void TestSetARCreditNoteApprovalsStatus_TwoApprover_Cancel() => AssertSetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Cancelled, Constants.AuthorizationMode.Codes.TwoApprovers);

		[GuiTest]
		public void TestSetARCreditNoteApprovalsStatus_Sequential_Approve() => AssertSetARCreditNoteApprovalsStatus_Sequential();
		[GuiTest]
		public void TestSetARCreditNoteApprovalsStatus_Sequential_Reject() => AssertSetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Rejected, Constants.AuthorizationMode.Codes.SequentialApprovers);
		[GuiTest]
		public void TestSetARCreditNoteApprovalsStatus_Sequential_Cancel() => AssertSetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Cancelled, Constants.AuthorizationMode.Codes.SequentialApprovers);

		public void AssertSetARCreditNoteApprovalsStatus(string action, string option)
		{
			var branch = TestObjectCreator.CreateBranch("BRS", GlbCompany.CurrentCompany);
			var dept = TestObjectCreator.CreateDepartment("DPS");
			SetupThreeLevelSecurity(branch, dept, option);

			var staff = TestObjectCreator.CreateStaff("US1");
			var otherStaff = TestObjectCreator.CreateStaff("US2");
			var request1 = CreateRequestWithTestData(option, 50, branch.PK, dept.PK, otherStaff);
			var request2 = CreateRequestWithTestData(option, 50, branch.PK, dept.PK, otherStaff);
			var request3 = CreateRequestWithTestData(option, 50, branch.PK, dept.PK, otherStaff);
			Factory.Save();

			var bulkSingleApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request1 }), request1);
			var bulkMultipleApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request2, request3 }), request2, request3);
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				DoActionAndAssertFailed(action, bulkSingleApproval);
				DoActionAndAssertFailed(action, bulkMultipleApproval);

				CreateSecurityRight(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, staff.PK, branch.PK, dept.PK);
				Factory.Save();

				if (action == Constants.GenApprovalRequestApprovalStatus.Cancelled)
				{
					DoActionAndAssertSuccessful(action, true, bulkSingleApproval);
					DoActionAndAssertSuccessful(action, true, bulkMultipleApproval);
				}
				else
				{
					DoActionAndAssertSuccessful(action, true, bulkSingleApproval, staff.GS_Code);
					DoActionAndAssertSuccessful(action, true, bulkMultipleApproval, staff.GS_Code);
				}
			}
		}

		public void AssertSetARCreditNoteApprovalsStatus_TwoApprover()
		{
			var branch = TestObjectCreator.CreateBranch("BRS", GlbCompany.CurrentCompany);
			var dept = TestObjectCreator.CreateDepartment("DPS");
			SetupThreeLevelSecurity(branch, dept, Constants.AuthorizationMode.Codes.TwoApprovers);

			var staff = TestObjectCreator.CreateStaff("US1");
			var otherStaff = TestObjectCreator.CreateStaff("US2");
			var request1 = CreateRequestWithTestData(Constants.AuthorizationMode.Codes.TwoApprovers, 50, branch.PK, dept.PK, otherStaff);
			var request2 = CreateRequestWithTestData(Constants.AuthorizationMode.Codes.TwoApprovers, 50, branch.PK, dept.PK, otherStaff);
			var request3 = CreateRequestWithTestData(Constants.AuthorizationMode.Codes.TwoApprovers, 50, branch.PK, dept.PK, otherStaff);
			Factory.Save();

			var bulkSingleApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request1 }), request1);
			var bulkMultipleApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request2, request3 }), request2, request3);
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				DoActionAndAssertFailed(Constants.GenApprovalRequestApprovalStatus.Approved, bulkSingleApproval);
				DoActionAndAssertFailed(Constants.GenApprovalRequestApprovalStatus.Approved, bulkMultipleApproval);

				CreateSecurityRight(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, staff.PK, branch.PK, dept.PK);
				Factory.Save();

				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Approved, false, bulkSingleApproval, staff.GS_Code);
				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Approved, false, bulkMultipleApproval, staff.GS_Code);

				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Approved, true, bulkSingleApproval, staff.GS_Code, staff.GS_Code);
				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Approved, true, bulkMultipleApproval, staff.GS_Code, staff.GS_Code);
			}
		}

		public void AssertSetARCreditNoteApprovalsStatus_Sequential()
		{
			var branch = TestObjectCreator.CreateBranch("BRS", GlbCompany.CurrentCompany);
			var dept = TestObjectCreator.CreateDepartment("DPS");
			SetupThreeLevelSecurity(branch, dept, Constants.AuthorizationMode.Codes.SequentialApprovers);

			var staff = TestObjectCreator.CreateStaff("US1");
			var otherStaff = TestObjectCreator.CreateStaff("US2");
			var request01 = CreateRequestWithTestData(Constants.AuthorizationMode.Codes.SequentialApprovers, 5, branch.PK, dept.PK, otherStaff);
			var request02 = CreateRequestWithTestData(Constants.AuthorizationMode.Codes.SequentialApprovers, 5, branch.PK, dept.PK, otherStaff);
			var request03 = CreateRequestWithTestData(Constants.AuthorizationMode.Codes.SequentialApprovers, 5, branch.PK, dept.PK, otherStaff);
			var request1 = CreateRequestWithTestData(Constants.AuthorizationMode.Codes.SequentialApprovers, 500, branch.PK, dept.PK, otherStaff);
			var request2 = CreateRequestWithTestData(Constants.AuthorizationMode.Codes.SequentialApprovers, 500, branch.PK, dept.PK, otherStaff);
			var request3 = CreateRequestWithTestData(Constants.AuthorizationMode.Codes.SequentialApprovers, 500, branch.PK, dept.PK, otherStaff);
			Factory.Save();

			var bulkSingleApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request01 }), request01);
			var bulkMultipleApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request02, request03 }), request02, request03);
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Approved, true, bulkSingleApproval, staff.GS_Code);
				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Approved, true, bulkMultipleApproval, staff.GS_Code);
			}

			bulkSingleApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request1 }), request1);
			bulkMultipleApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request2, request3 }), request2, request3);

			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				DoActionAndAssertFailed(Constants.GenApprovalRequestApprovalStatus.Approved, bulkSingleApproval);
				DoActionAndAssertFailed(Constants.GenApprovalRequestApprovalStatus.Approved, bulkMultipleApproval);

#if !WINZOR
				CreateSecurityRight(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, staff.PK, branch.PK, dept.PK);
				Factory.Save();

				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Approved, false, bulkSingleApproval, staff.GS_Code);
				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Approved, false, bulkMultipleApproval, staff.GS_Code);

				DoActionAndAssertFailed(Constants.GenApprovalRequestApprovalStatus.Approved, bulkSingleApproval, staff.GS_Code);
				DoActionAndAssertFailed(Constants.GenApprovalRequestApprovalStatus.Approved, bulkMultipleApproval, staff.GS_Code);

				CreateSecurityRight(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code, staff.PK, branch.PK, dept.PK);
				CreateSecurityRight(Env.Security.CreditAdjustmentNotePostingApprovalThirdLevelApproval.Code, staff.PK, branch.PK, dept.PK);
				Factory.Save();

				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Approved, false, bulkSingleApproval, staff.GS_Code, staff.GS_Code);
				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Approved, false, bulkMultipleApproval, staff.GS_Code, staff.GS_Code);

				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Approved, true, bulkSingleApproval, staff.GS_Code, staff.GS_Code, staff.GS_Code);
				DoActionAndAssertSuccessful(Constants.GenApprovalRequestApprovalStatus.Approved, true, bulkMultipleApproval, staff.GS_Code, staff.GS_Code, staff.GS_Code);
#endif
			}
		}

		public void TestSetARCreditNoteApprovalsStatus_Reject_DoesNotCareAboutClosedJobs()
		{
			Env.Security.ReopenJob.IsAllowed = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

			using (AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var (_, request) = CreateARCreditNoteForInvoiceReversalApprovalRequest(closeJob: true);
				var bulkApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request }), request);
				bulkApproval.SetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Rejected, out _);
				AssertNull("No form should have opened for security override", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Request should have been rejected", Constants.GenApprovalRequestApprovalStatus.Rejected, request.XP_ApprovalStatus);
			}
		}

		public void TestSetARCreditNoteApprovalsStatus_Approve_DoesNotCareAboutOpenJobs()
		{
			Env.Security.ReopenJob.IsAllowed = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

			using (AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var (_, request) = CreateARCreditNoteForInvoiceReversalApprovalRequest(closeJob: false);
				var bulkApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request }), request);
				bulkApproval.SetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Approved, out _);
				AssertNull("No form should have opened for security override", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Request should have been approved", Constants.GenApprovalRequestApprovalStatus.Approved, request.XP_ApprovalStatus);
			}
		}

		public void TestSetARCreditNoteApprovalsStatus_Approve_DoesNotCareAboutClosedJobsIfNotFinalApproval()
		{
			Env.Security.ReopenJob.IsAllowed = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

			using (AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var (_, request) = CreateARCreditNoteForInvoiceReversalApprovalRequest(closeJob: true);
				request.XP_GS_NKApprovingUser1 = null;
				request.XP_ApprovalDate = ZDateTime.Empty;
				request.PostingDetails.ApprovingOption = Constants.AuthorizationMode.Codes.TwoApprovers;
				request.PostingDetails.MaxAuthorisationLevelRequired = 1;
				request.UpdateIsFinalApproval();
				Assert("Pre-requisite: Request should not be at final approval", !request.IsFinalApproval);

				var bulkApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request }), request);
				bulkApproval.SetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Approved, out _);
				AssertNull("No form should have opened for security override", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Request should remain in requested status", Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			}
		}

		public void TestSetARCreditNoteApprovalsStatus_Approve_DoesNotCareAboutClosedJobsIfAutoPostingIsNotEnabled()
		{
			Env.Security.ReopenJob.IsAllowed = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

			using (AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, false))
			{
				var (_, request) = CreateARCreditNoteForInvoiceReversalApprovalRequest(closeJob: true);
				var bulkApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request }), request);
				bulkApproval.SetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Approved, out _);
				AssertNull("No form should have opened for security override", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Request should have been approved", Constants.GenApprovalRequestApprovalStatus.Approved, request.XP_ApprovalStatus);
			}
		}

		public void TestSetARCreditNoteApprovalsStatus_Approve_ApprovesOnlyOpenJobsIfNoReopenRight()
		{
			Env.Security.ReopenJob.IsAllowed = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

			using (AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var (_, request1) = CreateARCreditNoteForInvoiceReversalApprovalRequest(closeJob: true);
				Assert("Pre-requisite: Approval request must be transaction related", request1.XP_ParentTableCode == AccTransactionHeaderSchema.Constants.Prefix);
				var (_, request2) = CreateARCreditNoteForInvoiceReversalApprovalRequest(closeJob: false);
				var bulkApproval = new ARCreditNoteApprovalBulk(Factory, new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { request1, request2 }), request1, request2);
				bulkApproval.SetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Approved, out _);

				AssertNotNull("Form should have opened for security override", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Requested, request1.XP_ApprovalStatus);
				AssertEquals(Constants.GenApprovalRequestApprovalStatus.Approved, request2.XP_ApprovalStatus);
			}
		}

		public void TestSetARCreditNoteApprovalsStatus_Approve_DistinguishesBetweenReopenJobAndReopenJobPastAllowedReopenPeriod()
		{
			Env.Security.ReopenJob.IsAllowed = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

			var username = "TestUser";
			var password = "TestPassword";
			var staff = TestObjectCreator.CreateStaffWithSecurityRights(username, null, Env.Security.ReopenJob.Code, password);
			var securityRight = Factory.New<GlbSecurity>();
			securityRight.GU_GS = staff.PK;
			securityRight.GU_SecurityRight = Env.Security.ReopenJobPastAllowedReOpenPeriod.Code;
			securityRight.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			var loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());
			Assert("Pre-requisite: Login credentials should work", loginController.ValidateUserLoginAndPassword(username, password).LoginValidated);

			using (AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var config = new JobClosureConfiguration()
				{
					JobType = "ALL",
					DirectionCode = "ALL",
					Mode = "ALL",
					JobClosureDateOptionCode = "JOP",
					Offset = 5,
					ReopenRestrictionOffset = 7,
				};

				var regValue = new JobClosureConfigurationHeader();
				regValue.ConfigurationCollection.RemoveAll();
				regValue.ConfigurationCollection.Add(config);

				using (AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var (_, request1) = CreateARCreditNoteForInvoiceReversalApprovalRequest(closeJob: true);
					var (oldJob, request2) = CreateARCreditNoteForInvoiceReversalApprovalRequest(closeJob: true);
					oldJob.JH_A_JOP = ZDateTime.MinSmallDateTimeValue;
					Factory.Save();

					var provider = new DummyInteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new[] { request1, request2 });
					provider.SetLoginCredentials(username, password);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					var messages = new List<string>();
					ZFormModaliser.PreShowInvoker trackDialogs = (formOrDialog) =>
					{
						if (formOrDialog is LoginFormForARCreditNoteApprovalOverride loginForm)
						{
							messages.Add(loginForm.Message);
						}
					};
					using (ZFormModaliser.SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs(trackDialogs))
					{
						var bulkApproval = new ARCreditNoteApprovalBulk(Factory, provider, request1, request2);
						bulkApproval.SetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Approved, out _);

						AssertNotNull("Login form should have opened for security override", ZFormModaliser.LastFormShownDialogForTest);
						Assert("SecurityOverride for ReopenJob should have appeared", messages.Any(x => x.Contains(Env.Security.ReopenJob.DisplayText)));
						Assert("SecurityOverride for ReopenJobPastAllowedReOpenPeriod should have appeared", messages.Any(x => x.Contains(Env.Security.ReopenJobPastAllowedReOpenPeriod.DisplayText)));
					}
				}
			}
		}

		public void TestSetARCreditNoteApprovalsStatus_Approve_LogsAnAthEventOnUserOverride()
		{
			Env.Security.ReopenJob.IsAllowed = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

			var username = "TestUser";
			var password = "TestPassword";
			var staff = TestObjectCreator.CreateStaffWithSecurityRights(username, null, Env.Security.ReopenJob.Code, password);
			var securityRight = Factory.New<GlbSecurity>();
			securityRight.GU_GS = staff.PK;
			securityRight.GU_SecurityRight = Env.Security.ReopenJobPastAllowedReOpenPeriod.Code;
			securityRight.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			var loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());
			Assert("Pre-requisite: Login credentials should work", loginController.ValidateUserLoginAndPassword(username, password).LoginValidated);

			using (AccountingConfigurationRegistry.Instance.PostCreditNoteOnApproval.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				var config = new JobClosureConfiguration()
				{
					JobType = "ALL",
					DirectionCode = "ALL",
					Mode = "ALL",
					JobClosureDateOptionCode = "JOP",
					Offset = 5,
					ReopenRestrictionOffset = 7,
				};

				var regValue = new JobClosureConfigurationHeader();
				regValue.ConfigurationCollection.RemoveAll();
				regValue.ConfigurationCollection.Add(config);

				using (AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, regValue))
				{
					var (job, request1) = CreateARCreditNoteForInvoiceReversalApprovalRequest(closeJob: true);
					var (oldJob, request2) = CreateARCreditNoteForInvoiceReversalApprovalRequest(closeJob: true);
					oldJob.JH_A_JOP = ZDateTime.MinSmallDateTimeValue;
					Factory.Save();

					var provider = new DummyInteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new[] { request1, request2 });
					provider.SetLoginCredentials(username, password);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

					var messages = new List<string>();
					ZFormModaliser.PreShowInvoker trackDialogs = (formOrDialog) =>
					{
						if (formOrDialog is LoginFormForARCreditNoteApprovalOverride loginForm)
						{
							messages.Add(loginForm.Message);
						}
					};
					using (ZFormModaliser.SetTemporaryDelegateToCallBeforeShowingFormsOrDialogs(trackDialogs))
					{
						var bulkApproval = new ARCreditNoteApprovalBulk(Factory, provider, request1, request2);
						bulkApproval.SetARCreditNoteApprovalsStatus(Constants.GenApprovalRequestApprovalStatus.Approved, out _);

						AssertNotNull("Login form should have opened for security override", ZFormModaliser.LastFormShownDialogForTest);
						Assert("SecurityOverride for ReopenJob should have appeared", messages.Any(x => x.Contains(Env.Security.ReopenJob.DisplayText)));
						Assert("SecurityOverride for ReopenJobPastAllowedReOpenPeriod should have appeared", messages.Any(x => x.Contains(Env.Security.ReopenJobPastAllowedReOpenPeriod.DisplayText)));
					}

					Func<Job, bool> hasAthEventInJobLog = (job) =>
					{
						return job.Logs
							.Find((log) => log.SL_SE_NKEvent == "ATH" && log.SL_Table == "JobHeader" && log.SL_Reference.Contains("Job reopening authorized"))
							.Any();
					};

					job = Factory.Load<Job>(job.PK);
					Assert("Job should have an ATH event in its logs", hasAthEventInJobLog(job));

					oldJob = Factory.Load<Job>(oldJob.PK);
					Assert("Job should have an ATH event in its logs", hasAthEventInJobLog(oldJob));
				}
			}
		}

		(Job, ARCreditNoteApprovalRequest) CreateARCreditNoteForInvoiceReversalApprovalRequest(bool closeJob)
		{
			var request = TestObjectCreator.CreateInvoiceReversalApprovalRequest(100m);
			var invoice = Factory.Load<ARInvoice>(request.XP_ParentID);
			var job = invoice.RelatedJobsForReversing.FirstOrDefault();
			if (closeJob)
			{
				job.Close(
					(job, errorMessage) => { Fail(errorMessage); },
					(sender, args) => { Fail(args.QueryMessage); }
				);
			}

			return (job, request);
		}

		ARCreditNoteApprovalRequest CreateRequestWithTestData(string option, ZDecimal amount, ZGuid branchPK, ZGuid deptPK, GlbStaff creatingUser)
		{
			var request = GetNewApprovalRequest();
			request.PostingDetails.ApprovingOption = option;
			request.PostingDetails.MaxAmountToApprove = amount;
			request.XP_GB_JobBranch = branchPK;
			request.XP_GE_JobDepartment = deptPK;
			request.XP_SystemCreateUser = creatingUser.GS_Code;

			return request;
		}

		void DoActionAndAssertSuccessful(ZString action, bool expectChangesStatus, ARCreditNoteApprovalBulk approvalBulk, params ZString[] expectedApprovers)
			=> DoActionAndAssertResults(true, action, expectChangesStatus, approvalBulk, expectedApprovers);

		void DoActionAndAssertFailed(ZString action, ARCreditNoteApprovalBulk approvalBulk, params ZString[] expectedApprovers)
			=> DoActionAndAssertResults(false, action, false, approvalBulk, expectedApprovers);

		void DoActionAndAssertResults(bool expectSuccessful, string action, bool expectChangesStatus, ARCreditNoteApprovalBulk approvalBulk, params ZString[] expectedApprovers)
		{
			var approved = approvalBulk.SetARCreditNoteApprovalsStatus(action, out var alreadyHandledResult);
			AssertEquals("Should only be true for single approval", approvalBulk.Approvals.Count == 1, alreadyHandledResult);
			AssertEquals(expectSuccessful ? approvalBulk.Approvals.Count : 0, approved.Count);
			AssertEquals("Login form should show for failed approval when only one request is selected", approvalBulk.Approvals.Count == 1 && !expectSuccessful ? "Security Override Login" : null, ZFormModaliser.LastFormShownDialogForTest?.Text);

			foreach (var request in approvalBulk.Approvals)
			{
				AssertEquals(expectedApprovers.Length > 0 ? expectedApprovers[0] : ZString.Empty, request.XP_GS_NKApprovingUser1);
				AssertEquals(expectedApprovers.Length > 1 ? expectedApprovers[1] : ZString.Empty, request.XP_GS_NKApprovingUser2);
				AssertEquals(expectedApprovers.Length > 2 ? expectedApprovers[2] : ZString.Empty, request.XP_GS_NKApprovingUser3);
				AssertEquals(expectedApprovers.Length > 3 ? expectedApprovers[3] : ZString.Empty, request.XP_GS_NKApprovingUser4);
				AssertEquals(expectedApprovers.Length > 4 ? expectedApprovers[4] : ZString.Empty, request.XP_GS_NKApprovingUser5);
				AssertEquals(expectedApprovers.Length > 5 ? expectedApprovers[5] : ZString.Empty, request.XP_GS_NKApprovingUser6);
				AssertEquals(expectChangesStatus ? action : Constants.GenApprovalRequestApprovalStatus.Requested, request.XP_ApprovalStatus);
			}
			ZFormModaliser.LastFormShownDialogForTest = null;
		}

		void CreateSecurityRight(string code, ZGuid staffPK, ZGuid branchPK, ZGuid departmentPK)
		{
			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staffPK;
			staffSecurity.GU_SecurityRight = code;
			staffSecurity.GU_SecurityItemIsAllowed = true;
			staffSecurity.GU_GB = branchPK;
			staffSecurity.GU_GE = departmentPK;
			Factory.Save();

			Env.Security.ResetData(null, GlbStaff.CurrentUser.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbCompany.CurrentCompany.PK.ToGuid());
		}

		protected override ARCreditNoteApprovalRequest SetupRequestForFirstApprovalLevel()
		{
			// request1 set current branch department
			var approvalRequest1 = GetNewApprovalRequest();
			approvalRequest1.XP_GB_JobBranch = GlbBranch.CurrentBranch.PK;
			approvalRequest1.XP_GE_JobDepartment = GlbDepartment.CurrentDepartment.PK;
			approvalRequest1.PostingDetails.MaxAmountToApprove = 100M;
			return approvalRequest1;
		}

		protected override ARCreditNoteApprovalRequest SetupRequestForSecondApprovalLevel(bool usingNonCurrentBranchDepartment)
		{
			// request 2 set to different branch department
			var approvalRequest2 = GetNewApprovalRequest();
			approvalRequest2.XP_GB_JobBranch = usingNonCurrentBranchDepartment ? TestObjectCreator.NonCurrentBranch.PK : GlbBranch.CurrentBranch.PK;
			approvalRequest2.XP_GE_JobDepartment = usingNonCurrentBranchDepartment ? TestObjectCreator.NonCurrentDepartment.PK : GlbDepartment.CurrentDepartment.PK;
			approvalRequest2.PostingDetails.MaxAmountToApprove = 500M;
			return approvalRequest2;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewApprovalBulk(new DefaultAccessSecurityProvider(), Factory.NewWithValidTestData<ARCreditNoteApprovalRequest>());
		}

		protected override ARCreditNoteApprovalBulk GetNewApprovalBulk(ISecurityOverrideProvider interactiveSecurityOverrideProvider, params ARCreditNoteApprovalRequest[] approvalRequests)
		{
			return new ARCreditNoteApprovalBulk(Factory, interactiveSecurityOverrideProvider, approvalRequests);
		}

		protected override void SetupSecurity(bool disallowSecondLevel = false, bool disallowTwoLevels = false, Guid? branchPK = null, Guid? departmentPK = null)
		{
			Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = !disallowTwoLevels;
			Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = !disallowSecondLevel && !disallowTwoLevels;

			var setting = new AuthorizationModeAndSettings();
			var valuesForTest = setting.AuthorisationSettings;
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;

			if (!branchPK.HasValue && !departmentPK.HasValue)
			{
				AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid(), setting);
			}
			else
			{
				AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, branchPK.Value, departmentPK.Value, setting);
			}
		}

		void SetupThreeLevelSecurity(GlbBranch branch, GlbDepartment department, string mode)
		{
			var setting = new AuthorizationModeAndSettings();
			setting.AuthorizationMode = mode;
			var valuesForTest = setting.AuthorisationSettings;
			var newSetting = valuesForTest.AddNew();
			newSetting.Amount = 10;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.NoApprovalRequired;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 100;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SecondApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.UpTo;
			newSetting = valuesForTest.AddNew();
			newSetting.Amount = 200;
			newSetting.AuthorisationRequirement = AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.ThirdApprovalRequiredOnly;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, branch.PK.ToGuid(), department.PK.ToGuid(), setting);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SecurityTestObject.CreateTestUser(true, Env.Security.ReceivablesTransactions.Code, "tst", "newuser", "password");
		}

		class DummyInteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD : InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD
		{
			public DummyInteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(BusinessObject[] businessObjects) : base(businessObjects)
			{
			}

			public void SetLoginCredentials(string userName, string password)
			{
				this.userName = userName;
				this.password = password;
			}

			string userName;
			string password;

			protected override LoginForm CreateNewLoginForm()
			{
				var form = base.CreateNewLoginForm();
				form.DoLoginForTest(userName, password);
				return form;
			}
		}
	}
}
