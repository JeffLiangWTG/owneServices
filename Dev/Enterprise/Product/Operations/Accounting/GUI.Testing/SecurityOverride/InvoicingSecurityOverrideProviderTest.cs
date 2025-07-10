using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using AuthorisationCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.AuthorisationRequirementCodes;
using RangeCodes = Enterprise.Registry.Business.AmountBasedMultiLevelAuthorisationRequirement.RangeCodes;

namespace Enterprise.Accounting.GUI.Testing
{
	public class InvoicingSecurityOverrideProviderTest : SecurityOverrideProviderWithJobReopenSupportTest<InvoicingSecurityOverrideProvider>
	{
		protected override InvoicingSecurityOverrideProvider GetSecurityProvider(bool showApprovalRequestButton = false, bool alwaysCreateApprovalRequest = false, bool keepLoginFormResultAfterFirstUserAnswer = false)
		{
			return new InvoicingSecurityOverrideProvider(Factory.NewWithValidTestData<APInvoice>());
		}

		protected override bool ShouldPromptForGranted
		{
			get { return true; }
		}

		protected override bool IsApprovalRequestButtonSupported
		{
			get { return false; }
		}

		public void TestPopulateFirstAndSecondLevelSecurityRequiredBranchDepartment()
		{
			var branchBNE = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
			var branchSYD = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "SYD"));

			var setting = new AuthorizationModeAndSettings();
			var collection = setting.AuthorisationSettings;
			var settings = collection.AddNew();
			settings.Range = RangeCodes.Above;
			settings.Amount = 0.00m;
			settings.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, branchBNE.PK.ToGuid(), TestObjectCreator.FEADepartment.PK.ToGuid(), setting);

			setting = new AuthorizationModeAndSettings();
			collection = setting.AuthorisationSettings;
			settings = collection.AddNew();
			settings.Range = RangeCodes.Above;
			settings.Amount = 0.00m;
			settings.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, branchBNE.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid(), setting);

			setting = new AuthorizationModeAndSettings();
			collection = setting.AuthorisationSettings;
			settings = collection.AddNew();
			settings.Range = RangeCodes.Above;
			settings.Amount = 0.00m;
			settings.AuthorisationRequirement = AuthorisationCodes.SecondApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, branchSYD.PK.ToGuid(), TestObjectCreator.FIADepartment.PK.ToGuid(), setting);

			setting = new AuthorizationModeAndSettings();
			collection = setting.AuthorisationSettings;
			settings = collection.AddNew();
			settings.Range = RangeCodes.Above;
			settings.Amount = 0.00m;
			settings.AuthorisationRequirement = AuthorisationCodes.FirstApprovalRequiredOnly;
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, branchSYD.PK.ToGuid(), TestObjectCreator.FISDepartment.PK.ToGuid(), setting);

			var shipment = TestObjectCreator.CreateShipment("S001");
			using (var job = TestObjectCreator.CreateJob(shipment))
			{
				var creditNote = TestObjectCreator.CreateARCreditNote(50m, TestObjectCreator.LocalClient.PK, branchBNE.PK, TestObjectCreator.FEADepartment.PK);
				TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, branchBNE.PK, TestObjectCreator.FIADepartment.PK, 5m);
				TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, branchBNE.PK, TestObjectCreator.FIADepartment.PK, 1m);
				TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, branchBNE.PK, TestObjectCreator.FISDepartment.PK, 10m);
				TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, branchSYD.PK, TestObjectCreator.FIADepartment.PK, 15m);
				TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, branchSYD.PK, TestObjectCreator.FISDepartment.PK, 20m);
				TestObjectCreator.AddLineToCreditNote(creditNote, job.PK, branchSYD.PK, TestObjectCreator.FISDepartment.PK, 2m);

				var provider = new InvoicingSecurityOverrideProvider(creditNote);
				AssertEquals(0, provider.FirstLevelSecurityRequiredBranchDepartment.Count);
				AssertEquals(0, provider.SecondLevelSecurityRequiredBranchDepartment.Count);
				creditNote.TransactionsForAuthorisationCalculation.AddRange(creditNote.GetLineLevelTransactionsGroupedByBranchAndDept());
				provider.PopulateAllLevelsSecurityRequiredBranchDepartment_ForTestOnly();
				AssertEquals(2, provider.FirstLevelSecurityRequiredBranchDepartment.Count);
				AssertEquals(2, provider.SecondLevelSecurityRequiredBranchDepartment.Count);
				Assert(provider.FirstLevelSecurityRequiredBranchDepartment.Exists(x => x.Branch == branchBNE.PK && x.Department == TestObjectCreator.FIADepartment.PK));
				Assert(provider.FirstLevelSecurityRequiredBranchDepartment.Exists(x => x.Branch == branchSYD.PK && x.Department == TestObjectCreator.FISDepartment.PK));
				Assert(provider.SecondLevelSecurityRequiredBranchDepartment.Exists(x => x.Branch == branchBNE.PK && x.Department == TestObjectCreator.FEADepartment.PK));
				Assert(provider.SecondLevelSecurityRequiredBranchDepartment.Exists(x => x.Branch == branchSYD.PK && x.Department == TestObjectCreator.FIADepartment.PK));
			}
		}

		public virtual void TestPromptForTemporaryAccessBasedOnRequiresMultipleApproversOption()
		{
			var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.Default });
			var provider = new InvoicingSecurityOverrideProvider(arCreditNote);
			provider.PromptForTemporaryAccessCore(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval);
			Assert(!provider.RequiresTwoApprovers);
			AssertEquals("Should prompt regular login form when RequiresTwoApprovers is false", typeof(LoginFormForARCreditNoteApprovalOverride), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals(@"You do not have security rights to post a credit note/adjustment note for this amount.
A user with security rights to post a credit note/adjustment note can authorize this transaction.
To post this transaction, please have an authorized user enter their username and password below.", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);

			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers });
			provider = new InvoicingSecurityOverrideProvider(arCreditNote);
			provider.PromptForTemporaryAccessCore(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval);
			Assert(provider.RequiresTwoApprovers);
			Assert(!provider.IsApprovalRequestButtonVisible_ForTestOnly);
			AssertEquals("Should prompt 2 credential login form when RequiresTwoApprovers is true and IsApprovalRequestButtonVisible is false", typeof(LoginFormWithTwoCredentialSupportBranchDepartmentLevel), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals(@"You do not have security rights [1st Level Approval] to post a credit note/adjustment note for this amount.
Two users with security rights to post a credit note/adjustment note can authorize this transaction.
To post this transaction, please have two authorized users enter their username and password below.", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);

			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.SequentialApprovers });
			provider = new InvoicingSecurityOverrideProvider(arCreditNote);
			provider.PromptForTemporaryAccessCore(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval);
			Assert(provider.RequiresSequentialApprovals);
			Assert(!provider.IsApprovalRequestButtonVisible_ForTestOnly);
			AssertEquals("Should prompt 2 credential login form when RequiresSequentialApprovals is true and IsApprovalRequestButtonVisible is false", typeof(LoginFormWithTwoCredentialSupportBranchDepartmentLevel), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals(@"You do not have security rights [1st Level Approval] to post a credit note/adjustment note for this amount.
Two users with security rights to post a credit note/adjustment note can authorize this transaction.
To post this transaction, please have two authorized users enter their username and password below.", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);
		}

		public void TestPrePopulateLoginUserForTwoApproverOption()
		{
			var setting = TestObjectCreator.CreateAuthorizationModeAndSettings(100m, 200m, Constants.AuthorizationMode.Codes.TwoApprovers);
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting))
			using (AccountingConfigurationRegistry.Instance.EnableLineLevelApprovalRequestForARCreditNote.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.APInvoiceApproval.Code, "password", false);

				var arCreditNote = TestObjectCreator.CreateARCreditNote(-4000m, TestObjectCreator.Debtor.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
				TestObjectCreator.AddLineToCreditNote(arCreditNote, ZGuid.Empty, GlbBranch.CurrentBranch.PK, TestObjectCreator.FIADepartment.PK, 2000m);
				TestObjectCreator.AddLineToCreditNote(arCreditNote, ZGuid.Empty, GlbBranch.CurrentBranch.PK, TestObjectCreator.FEADepartment.PK, 2000m);

				arCreditNote.SecurityOverrideProvider = new InvoicingSecurityOverrideProvider(arCreditNote);
				using (Env.SetTemporaryUserContext("newuser", GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(GlbDepartment.CurrentDepartment.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), staff.PK, true);
					TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), staff.PK, false);
					TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FEADepartment.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), staff.PK, true);
					ZFormModaliser.ShowDialogsInTest = true;
					new InvoicingPreSaveHelper().PreSaveActions(arCreditNote, null, false);
					AssertType("Should Prompt LoginFormWithTwoCredentialSupportBranchDepartmentLevel Form", typeof(LoginFormWithTwoCredentialSupportBranchDepartmentLevel), ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("First user should be blank because login user doesn't have security rights for all branch/department.", null, ((LoginFormWithTwoCredentialSupportBranchDepartmentLevel)ZFormModaliser.LastFormShownDialogForTest).prePopulatedFirstLoginUser);

					TestObjectCreator.SetUpCreditAdjustmentNotePostingApprovalLevelForBranchDepartment(TestObjectCreator.FIADepartment.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), staff.PK, true);
					new InvoicingPreSaveHelper().PreSaveActions(arCreditNote, null, false);
					AssertType("Should Prompt LoginFormWithTwoCredentialSupportBranchDepartmentLevel Form", typeof(LoginFormWithTwoCredentialSupportBranchDepartmentLevel), ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("First user should be the login user because it has security rights for all branch/department.", "newuser", ((LoginFormWithTwoCredentialSupportBranchDepartmentLevel)ZFormModaliser.LastFormShownDialogForTest).prePopulatedFirstLoginUser);
				}
			}
		}

		public void TestUserInitiatorAndNotAllowedToApprove()
		{
			var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.Default });
			var provider = new InvoicingSecurityOverrideProvider(arCreditNote);
			Assert(!provider.UserInitiatorAndNotAllowedToApprove_ForTestOnly);
			AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AuthorizationModeAndSettings { AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers });
			provider = new InvoicingSecurityOverrideProvider(arCreditNote);
			Assert(provider.UserInitiatorAndNotAllowedToApprove_ForTestOnly);
		}

		public void TestLoginWithTwoCredentialForm()
		{
			var setting = TestObjectCreator.CreateAuthorizationModeAndSettings(100m, 200m);
			setting.AuthorizationMode = Constants.AuthorizationMode.Codes.TwoApprovers;

			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting))
			{
				var arCreditNote = TestObjectCreator.CreateARCreditNote(-150m, TestObjectCreator.Debtor.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);
				TestObjectCreator.AddLineToCreditNote(arCreditNote, ZGuid.Empty, GlbBranch.CurrentBranch.PK, TestObjectCreator.FIADepartment.PK, 150m);
				arCreditNote.TransactionsForAuthorisationCalculation.Add(arCreditNote);
				var testProvider = new InvoicingSecurityOverrideProviderWithTwoCredentialTestClass(arCreditNote);

				var staff = TestObjectCreator.CreateStaffWithSecurityRights("newuser", "tst", Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, "password", false);

				testProvider.Username1 = "invalid user";
				testProvider.UserPassword1 = "invalid pwd";
				// Test case 1 - login user 1 is invalid
				using (Env.SetTemporaryUserContext(staff.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					AssertEquals(false, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval].IsAllowed);
					AssertEquals("1st user error: " + SecurityOverrideProvider.InvalidLoginErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should be no PKs when values are invalid", 0, testProvider.UserPKsForTwoCredentialLogin?.Count ?? 0);
					UnitTestUserNotification.Instance.ClearMessages();
				}

				// Test case 2 - login user 1 is valid and has security right, but login user 2 is invalid
				var staff1 = TestObjectCreator.CreateStaffWithSecurityRights("testUser1", "TU1", Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, "testPassword1", true);

				testProvider = new InvoicingSecurityOverrideProviderWithTwoCredentialTestClass(arCreditNote);
				testProvider.Username1 = staff1.GS_LoginName;
				testProvider.UserPassword1 = staff1.StaffPlainTextPassword;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				AssertEquals(false, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval].IsAllowed);
				AssertEquals("2nd user error: " + SecurityOverrideProvider.InvalidLoginErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Requires two valid users to be entered", 0, testProvider.UserPKsForTwoCredentialLogin?.Count ?? 0);
				UnitTestUserNotification.Instance.ClearMessages();

				// Test case 3 - login user 1 is valid and has security right, login user 2 is valid but does not have security right
				var staff2 = TestObjectCreator.CreateStaffWithSecurityRights("testUser2", "TU2", Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, "testPassword2", false);

				testProvider = new InvoicingSecurityOverrideProviderWithTwoCredentialTestClass(arCreditNote);
				testProvider.Username1 = staff1.GS_LoginName;
				testProvider.UserPassword1 = staff1.StaffPlainTextPassword;
				testProvider.Username2 = staff2.GS_LoginName;
				testProvider.UserPassword2 = staff2.StaffPlainTextPassword;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				using (Env.SetTemporaryUserContext(staff2.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					AssertEquals(false, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval].IsAllowed);
					AssertEquals("The Authorizing user does not have security rights for this Branch or Department. Your system administrator maintains each user's security rights.", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("Should contain both valid users entered", 2, testProvider.UserPKsForTwoCredentialLogin.Count);
					Assert("Should contain both valid users entered", testProvider.UserPKsForTwoCredentialLogin.Contains(staff1.PK));
					Assert("Should contain both valid users entered", testProvider.UserPKsForTwoCredentialLogin.Contains(staff2.PK));
				}

				UnitTestUserNotification.Instance.ClearMessages();

				// Test case 4 - login user 1 is valid and has security right, login user 2 is valid and has security right
				var staff3 = TestObjectCreator.CreateStaffWithSecurityRights("testUser3", "TU3", Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, "testPassword3", true);

				testProvider = new InvoicingSecurityOverrideProviderWithTwoCredentialTestClass(arCreditNote);
				testProvider.Username1 = staff1.GS_LoginName;
				testProvider.UserPassword1 = staff1.StaffPlainTextPassword;
				testProvider.Username2 = staff3.GS_LoginName;
				testProvider.UserPassword2 = staff3.StaffPlainTextPassword;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				AssertEquals(true, ((ISecurityOverrideProvider)testProvider).SecurityCertificates[Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval].IsAllowed);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should contain both valid users entered", 2, testProvider.UserPKsForTwoCredentialLogin.Count);
				Assert("Should contain both valid users entered", testProvider.UserPKsForTwoCredentialLogin.Contains(GlbStaff.CurrentUser.PK));
				Assert("Should contain both valid users entered", testProvider.UserPKsForTwoCredentialLogin.Contains(staff3.PK));
				UnitTestUserNotification.Instance.ClearMessages();
			}
		}

		class InvoicingSecurityOverrideProviderWithTwoCredentialTestClass : InvoicingSecurityOverrideProvider, IDisposable
		{
			public InvoicingSecurityOverrideProviderWithTwoCredentialTestClass(bool supportMultipleApprover)
				: base(supportMultipleApprover: supportMultipleApprover)
			{ }

			public InvoicingSecurityOverrideProviderWithTwoCredentialTestClass(InvoicingBase transaction)
				: base(transaction)
			{ }

			public string Username1 { get; set; }
			public string UserPassword1 { get; set; }

			public string Username2 { get; set; }
			public string UserPassword2 { get; set; }

			protected override LoginFormWithTwoCredentialSupportBranchDepartmentLevel CreateNewLoginFormWithTwoCredential()
			{
				LoginForm = new LoginFormWithTwoCredentialSupportBranchDepartmentLevel(FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
																						ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
																						FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
				LoginForm.Message = "test message";

				if (!string.IsNullOrEmpty(Username1) && !string.IsNullOrEmpty(UserPassword1))
				{
					LoginForm.DoLogin1ForTest(Username1, UserPassword1);
				}

				if (!string.IsNullOrEmpty(Username2) && !string.IsNullOrEmpty(UserPassword2))
				{
					LoginForm.DoLogin2ForTest(Username2, UserPassword2);
				}

				return LoginForm;
			}

			protected virtual void Dispose(bool disposing)
			{
				if (disposing)
				{
					// dispose managed resources
					LoginForm.Dispose();
				}
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			public LoginFormWithTwoCredentialSupportBranchDepartmentLevel LoginForm;
		}

		public virtual void TestGetClosedJobs()
		{
			Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_Status = JobHeaderStatus.Closed.Code;
			job1.JH_JobNum = "TESTJOB1";
			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_Status = JobHeaderStatus.Closed.Code;
			job2.JH_JobNum = "TESTJOB2";
			Job job3 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job3.JH_Status = JobHeaderStatus.Working.Code;
			job3.JH_JobNum = "TESTJOB3";

			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AddRelatedJobsForReversing_ForTestOnly(job1);
			invoice.AddRelatedJobsForReversing_ForTestOnly(job2);
			invoice.AddRelatedJobsForReversing_ForTestOnly(job3);

			InvoicingSecurityOverrideProvider testProvider = new InvoicingSecurityOverrideProvider(invoice);
			AssertEquals("TESTJOB1, TESTJOB2", testProvider.GetClosedJobs_ForTestOnly());
		}

		public virtual void TestSecurityOverrideMessageForInvoiceLevels()
		{
			var firstLevelSecurity = new SecurityCheckpoint(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code, SecurityCore.Captions.New, null, null, false);
			var secondLevelSecurity = new SecurityCheckpoint(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.Code, SecurityCore.Captions.New, null, null, false);
			var testProvider = GetSecurityProvider();
			string expectedAdditionalText = testProvider.InvoiceLevelsSecurityOverrideMessage_ForTestOnly;
			AssertEquals(expectedAdditionalText, testProvider.GetSecurityOverrideMessage_ForTestOnly(firstLevelSecurity));
			AssertEquals(expectedAdditionalText, testProvider.GetSecurityOverrideMessage_ForTestOnly(secondLevelSecurity));
		}

		public void TestSecurityOverrideMessageForAPInvoiceLevels()
		{
			InvoicingSecurityOverrideProvider testProvider = GetSecurityProvider();
			var expectedMessage = "This transaction must be approved by a user with Level {0} Approval authority.\r\nIf a user with this level of authority enters their username and password below you may continue. Otherwise hit cancel to continue without approving this transaction.";
			AssertEquals(string.Format(expectedMessage, "1"),
				testProvider.GetSecurityOverrideMessage_ForTestOnly(Env.Security.APUnapprovedInvoicesFirstApproval));
			AssertEquals(string.Format(expectedMessage, "2"),
				testProvider.GetSecurityOverrideMessage_ForTestOnly(Env.Security.APUnapprovedInvoicesSecondApproval));
			AssertEquals(string.Format(expectedMessage, "3"),
				testProvider.GetSecurityOverrideMessage_ForTestOnly(Env.Security.APUnapprovedInvoicesThirdApproval));
		}

		public void TestSecurityGrantedMessageForInvoiceLevels()
		{
			InvoicingSecurityOverrideProvider testProvider = GetSecurityProvider();
			AssertEquals("", testProvider.GetSecurityGrantedMessage_ForTestOnly(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval));
			AssertEquals("", testProvider.GetSecurityGrantedMessage_ForTestOnly(Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval));
		}

		public void TestJobReopenChecksShouldUseRegularSingleUserLoginForm()
		{
			var setting = TestObjectCreator.CreateAuthorizationModeAndSettings(0.1m, 200m, Constants.AuthorizationMode.Codes.SequentialApprovers);
			using (AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, setting))
			{
				Job job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
				job1.JH_Status = JobHeaderStatus.Closed.Code;
				job1.JH_JobNum = "CLOSEDJOB1";

				var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
				arCreditNote.AddRelatedJobsForReversing_ForTestOnly(job1);

				var provider = new InvoicingSecurityOverrideProvider(arCreditNote);
				Assert(provider.RequiresSequentialApprovals);

				provider.PromptForTemporaryAccessCore(Env.Security.ReopenJob);
				AssertEquals("Should prompt regular single user login form even though RequiresSequentialApprovals is true", typeof(LoginFormForARCreditNoteApprovalOverride), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals(@"Closed Job(s) :CLOSEDJOB1
These jobs are currently closed.
Before proceeding they must be re-opened.
You do not have security rights to Re-open Closed Jobs.
A user with security rights to Re-open Closed jobs can authorize this transaction.To re-open these jobs, please have an authorized user enter their username and password below.
", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);

				provider.PromptForTemporaryAccessCore(Env.Security.ReopenJobPastAllowedReOpenPeriod);
				AssertEquals("Should prompt regular single user login form even though RequiresSequentialApprovals is true", typeof(LoginFormForARCreditNoteApprovalOverride), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals(@"Closed Job(s) :CLOSEDJOB1
These jobs are currently closed.
Before proceeding they must be re-opened.
You do not have security rights to Re-open Closed Jobs subjected to Re-Open Restriction.
A user with security rights 'Allow Reopen Jobs Past Allowed Reopen Period' can authorize this transaction.To re-open these jobs, please have an authorized user enter their username and password below.
", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);
			}
		}

		TestObjectCreator TestObjectCreator;
		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}
	}
}
