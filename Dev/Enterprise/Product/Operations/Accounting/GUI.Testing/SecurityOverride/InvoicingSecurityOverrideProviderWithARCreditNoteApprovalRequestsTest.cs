using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class InvoicingSecurityOverrideProviderWithARCreditNoteApprovalRequestsTest : InvoicingSecurityOverrideProviderTest
	{
		class InvoicingSecurityOverrideProviderWithARCreditNoteApprovalRequestsTestClass : InvoicingSecurityOverrideProvider, IDisposable
		{
			public InvoicingSecurityOverrideProviderWithARCreditNoteApprovalRequestsTestClass(bool showApprovalRequestButton = false, bool alwaysCreateApprovalRequest = false, bool keepLoginFormResultAfterFirstUserAnswer = false, bool supportMultipleApprover = false, ARCreditNoteApprovalRequest[] aRCreditNoteApprovalRequests = null)
				: base(showApprovalRequestButton, alwaysCreateApprovalRequest, keepLoginFormResultAfterFirstUserAnswer, supportMultipleApprover, aRCreditNoteApprovalRequests)
			{
			}

			public LoginFormWithRequest LoginForm;
			public string Username1 { get; set; }
			public string UserPassword1 { get; set; }
			protected override LoginForm CreateNewLoginForm()
			{
				LoginForm = new LoginFormWithRequest(FirstLevelSecurityRequiredBranchDepartment, SecondLevelSecurityRequiredBranchDepartment,
													ThirdLevelSecurityRequiredBranchDepartment, FourthLevelSecurityRequiredBranchDepartment,
													FifthLevelSecurityRequiredBranchDepartment, SixthLevelSecurityRequiredBranchDepartment);
				LoginForm.Message = "test message";

				if (!string.IsNullOrEmpty(Username1) && !string.IsNullOrEmpty(UserPassword1))
				{
					LoginForm.DoLoginForTest(Username1, UserPassword1);
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
		}

		protected TestObjectCreator TestObjectCreator;
		ARCreditNoteApprovalRequest Approval1;
		ARCreditNoteApprovalRequest Approval2;
		ARCreditNoteApprovalRequest Approval3;
		ARCreditNoteApprovalRequest Approval4;

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
			SetUpARCreditNoteApprovalRequests();
		}

		InvoicingSecurityOverrideProviderWithARCreditNoteApprovalRequestsTestClass GetTestSecurityProvider(bool showApprovalRequestButton = true, bool alwaysCreateApprovalRequest = false, bool keepLoginFormResultAfterFirstUserAnswer = false)
		{
			var request = Factory.New<ARCreditNoteApprovalRequest>();
			ARCreditNoteApprovalRequest[] approvalRequests = new ARCreditNoteApprovalRequest[] { Approval1, Approval2, Approval3, Approval4 };
			return new InvoicingSecurityOverrideProviderWithARCreditNoteApprovalRequestsTestClass(showApprovalRequestButton, alwaysCreateApprovalRequest, keepLoginFormResultAfterFirstUserAnswer, aRCreditNoteApprovalRequests: approvalRequests);
		}

		void SetUpARCreditNoteApprovalRequests()
		{
			var shipment = TestObjectCreator.CreateShipment("S0001");
			var job = TestObjectCreator.CreateJob(shipment, false);
			Factory.Save();

			Approval1 = CreateARCreditNoteApprovalRequest(job, 1000M);
			Approval2 = CreateARCreditNoteApprovalRequest(job, 2000M);
			Approval3 = CreateARCreditNoteApprovalRequest(job, 3000M);
			Approval4 = CreateARCreditNoteApprovalRequest(job, 3000M);
		}

		ARCreditNoteApprovalRequest CreateARCreditNoteApprovalRequest(Job job, decimal amount)
		{
			var approval = Factory.New<ARCreditNoteApprovalRequest>();
			approval.XP_ParentID = job.PK;
			approval.XP_ParentTableCode = job.TablePrefix;
			approval.XP_SystemCreateTimeUtc = ZDateTime.Today.AddDays(-1);
			approval.XP_ApprovalStatus = Constants.GenApprovalRequestApprovalStatus.Requested;
			approval.XP_GB_JobBranch = job.Branch.PK;
			approval.XP_GE_JobDepartment = job.Department.PK;
			approval.PostingDetails.MaxAmountToApprove = amount;
			return approval;
		}

		public void TestPromptForTemporaryAccessCore()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_LoginName = "testUser1";
			staff1.GS_Code = "TU1";
			staff1.StaffPlainTextPassword = "testPassword1";
			staff1.GS_ChangePasswordAtNextLogin = false;
			staff1.GS_IsSystemAccount = false;
			staff1.GS_IsActive = true;

			var glbSecurity1 = Factory.New<GlbSecurity>();
			glbSecurity1.GU_GS = staff1.PK;
			glbSecurity1.GU_SecurityRight = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.Code;
			glbSecurity1.GU_SecurityItemIsAllowed = true;
			Factory.Save();

			var provider = GetTestSecurityProvider();
			provider.Username1 = "testUser1";
			provider.UserPassword1 = "testPassword1";

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			provider.PromptForTemporaryAccessCore(Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval);
			AssertEquals("Should prompt LoginFormWithRequest when RequiresTwoApprovers is false", typeof(LoginFormWithRequest), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals(@"You do not have security rights to post a credit note/adjustment note for this amount.
A user with security rights to post a credit note/adjustment note can authorize this transaction.
To post this transaction, please have an authorized user enter their username and password below.

To queue a request for approval and postpone posting, press 'Approval Request' button.", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);
		}
	}
}
