using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Accounting.GUI.InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.Testing
{
	class InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRDTest : InteractiveSecurityOverrideProviderTest
	{
		protected override SecurityOverrideProvider GetSecurityOverrideProvider() => new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(Array.Empty<BusinessObject>());

		public void TestCredentialsFormMessage()
		{
			var checkPoint = new SecurityCheckpoint("Code", (NoResString)"DisplayText", null, Env.Security);
			var provider = GetSecurityOverrideProvider();
			provider.PromptForTemporaryAccessCore(checkPoint);

			var expectedMessage = $@"One or more selected credit note approval requests require {checkPoint.DisplayText} security rights.
To override this security, a user with the required security right must login.
Please enter username and password details below.";

			var loginForm = ZFormModaliser.LastFormShownDialogForTest as LoginForm;
			AssertNotNull("Prerequisite: Login Form is shown", loginForm);
			AssertEquals(expectedMessage, loginForm.Message);
		}

		public void TestCredentialsFormErrorMessages()
		{
			var checkPoint = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval;
			var (approval, brnDeptPair) = SetupApproval(AuthorizationMode.Codes.Default, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FirstApprovalRequiredOnly);
			var provider = new DummyInteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new BusinessObject[] { approval });
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			var invalidName = new string('A', GlbStaffSchema.GS_LoginName.MaxLength);
			provider.SetLoginCredentials(invalidName, "");
			provider.PromptForTemporaryAccessCore(checkPoint);
			AssertEquals("Please enter a valid username.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			var staff = TestObjectCreator.Staff;
			staff.GS_LoginName = "Test";
			Factory.Save();
			provider.SetLoginCredentials(staff.GS_LoginName, "incorrect");
			provider.PromptForTemporaryAccessCore(checkPoint);
			AssertEquals("The password entered is incorrect or expired. Please re-enter the password or reset the password at login.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			staff.ChangePasswordAtNextLogin = true;
			Factory.Save();
			provider.SetLoginCredentials(staff.GS_LoginName, staff.StaffPlainTextPassword);
			provider.PromptForTemporaryAccessCore(checkPoint);
			AssertEquals("The password entered is incorrect or expired. Please re-enter the password or reset the password at login.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			staff.ChangePasswordAtNextLogin = false;
			staff.GS_IsActive = false;
			Factory.Save();
			provider.PromptForTemporaryAccessCore(checkPoint);
			AssertEquals("This user is inactive, Please enter credentials for an active user.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			staff.GS_IsActive = true;
			Factory.Save();
			provider.PromptForTemporaryAccessCore(checkPoint);
			AssertEquals("The Authorizing user does not have security rights for this Branch or Department. Your system administrator maintains each user's security rights.", UnitTestUserNotification.Instance.LastMessage.Text);
			UnitTestUserNotification.Instance.ClearMessages();

			var staffSecurity = Factory.New<GlbSecurity>();
			staffSecurity.GU_GS = staff.PK;
			staffSecurity.GU_SecurityRight = checkPoint.Code;
			staffSecurity.GU_SecurityItemIsAllowed = true;
			staffSecurity.GU_GB = brnDeptPair.Branch;
			staffSecurity.GU_GE = brnDeptPair.Department;
			Factory.Save();
			provider.PromptForTemporaryAccessCore(checkPoint);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestCredentialsFormBranchDepartments()
		{
			var (defApproval, defPair) = SetupApproval(AuthorizationMode.Codes.Default, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FourthApprovalRequiredOnly);
			var (twoApproval, twoPair) = SetupApproval(AuthorizationMode.Codes.TwoApprovers, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.FifthApprovalRequiredOnly);
			var (seqApproval, seqPair) = SetupApproval(AuthorizationMode.Codes.SequentialApprovers, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SixthApprovalRequiredOnly);

			var provider = new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new[] { defApproval, twoApproval, seqApproval });
			provider.PromptForTemporaryAccessCore(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval);
			var loginForm = ZFormModaliser.LastFormShownDialogForTest as LoginFormForARCreditNoteApprovalOverride;

			AssertNotNull("Prerequisite: Login Form is shown", loginForm);
			AssertListContainsBranchDepartmentPair(loginForm.FirstLevelSecurityRequiredBranchDepartment, seqPair);
			Assert(loginForm.SecondLevelSecurityRequiredBranchDepartment.IsNullOrEmpty());
			Assert(loginForm.ThirdLevelSecurityRequiredBranchDepartment.IsNullOrEmpty());
			AssertListContainsBranchDepartmentPair(loginForm.FourthLevelSecurityRequiredBranchDepartment, defPair);
			AssertListContainsBranchDepartmentPair(loginForm.FifthLevelSecurityRequiredBranchDepartment, twoPair);
			Assert(loginForm.SixthLevelSecurityRequiredBranchDepartment.IsNullOrEmpty());
			ZFormModaliser.LastFormShownDialogForTest = null;

			seqApproval.XP_GS_NKApprovingUser1 = Env.CurrentUser.Initials;
			provider = new InteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new[] { defApproval, twoApproval, seqApproval });
			provider.PromptForTemporaryAccessCore(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval);
			loginForm = ZFormModaliser.LastFormShownDialogForTest as LoginFormForARCreditNoteApprovalOverride;

			AssertNotNull("Prerequisite: Login Form is shown", loginForm);
			Assert(loginForm.FirstLevelSecurityRequiredBranchDepartment.IsNullOrEmpty());
			AssertListContainsBranchDepartmentPair(loginForm.SecondLevelSecurityRequiredBranchDepartment, seqPair);
			Assert(loginForm.ThirdLevelSecurityRequiredBranchDepartment.IsNullOrEmpty());
			AssertListContainsBranchDepartmentPair(loginForm.FourthLevelSecurityRequiredBranchDepartment, defPair);
			AssertListContainsBranchDepartmentPair(loginForm.FifthLevelSecurityRequiredBranchDepartment, twoPair);
			Assert(loginForm.SixthLevelSecurityRequiredBranchDepartment.IsNullOrEmpty());
		}

		public void TestJobReopenCheckpointsUseSingleUserOverride()
		{
			string username = "TestUser";
			string password = "TestPassword";
			var staff = TestObjectCreator.CreateStaffWithSecurityRights(username, null, Env.Security.ReopenJob.Code, password);
			var securityRight = Factory.New<GlbSecurity>();
			securityRight.GU_GS = staff.PK;
			securityRight.GU_SecurityRight = Env.Security.ReopenJobPastAllowedReOpenPeriod.Code;
			securityRight.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			var loginController = (IUserLoginController)Activator.CreateInstance(ObjectFactory.GetType<IUserLoginController>());
			Assert("Pre-requisite: Login credentials should work", loginController.ValidateUserLoginAndPassword(username, password).LoginValidated);

			Env.Security.ReopenJob.IsAllowed = false;
			Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

			var (seqApproval, _) = SetupApproval(AuthorizationMode.Codes.SequentialApprovers, AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes.SixthApprovalRequiredOnly);
			var provider = new DummyInteractiveSecurityOverrideProviderWithBranchDepartmentSupportForARCRD(new[] { seqApproval });
			provider.SetLoginCredentials(username, password);
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			AssertEquals("Security right should have been granted.", SecurityCertificate.Granted, provider.PromptForTemporaryAccessCore(Env.Security.ReopenJob));
			AssertEquals("Security right should have been granted.", SecurityCertificate.Granted, provider.PromptForTemporaryAccessCore(Env.Security.ReopenJobPastAllowedReOpenPeriod));
		}

		void AssertListContainsBranchDepartmentPair(List<BranchDepartmentPair> list, BranchDepartmentPair pair)
		{
			AssertEquals(1, list.Count);
			AssertEquals(pair.Branch, list[0].Branch);
			AssertEquals(pair.Department, list[0].Department);
		}

		(ARCreditNoteApprovalRequest approval, BranchDepartmentPair pair) SetupApproval(string authMode, string level)
		{
			var branch = TestObjectCreator.CreateBranch(authMode, GlbCompany.CurrentCompany);
			var dept = TestObjectCreator.CreateDepartment(authMode, authMode);
			var pair = new BranchDepartmentPair(branch.PK, dept.PK);

			var authSettings = new AuthorizationModeAndSettings();
			authSettings.AuthorizationMode = authMode;
			var newSetting = authSettings.AuthorisationSettings.AddNew();
			newSetting.Amount = 0;
			newSetting.AuthorisationRequirement = level;
			newSetting.Range = AmountBasedMultiLevelAuthorisationRequirement.RangeCodes.Above;

			ARCreditNoteApprovalRequest approval;
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, authSettings))
			{
				approval = TestObjectCreator.CreateARCreditNoteApprovalRequest(100);
				approval.XP_GB_JobBranch = branch.PK;
				approval.XP_GE_JobDepartment = dept.PK;
			}

			return (approval, pair);
		}

		protected TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());
		BusinessObjectFactory factory;

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
