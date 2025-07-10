using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.EmailNotification.Testing
{
	public class JobReopenedEmailTest : MasterFiles.Business.Testing.AccountingEmailDefTest
	{
		protected override Type EmailDefType
		{
			get
			{
				return typeof(JobReopenedEmail);
			}
		}

		public void TestReopenedUser()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var email = new JobReopenedEmail(job, ZGuid.Empty);
			var expectedBody = string.Format(@"The following job was reopened by {0} ({1}) : 
Number: '{2}', Company: '{3}'.
", GlbStaff.CurrentUser.GS_LoginName, GlbStaff.CurrentUser.GS_FullName, job.JH_JobNum, GlbCompany.CurrentCompany.GC_Code);
			AssertEquals("Email Body", expectedBody, GetBody(email));
		}

		public void TestEmailBodyWithReopenedUser()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var email = new JobReopenedEmail(job, staff.PK);
			var expectedBody = string.Format(@"The following job was reopened by {0} ({1}) : 
Number: '{2}', Company: '{3}'.

Access granted by {4} ({5})", GlbStaff.CurrentUser.GS_LoginName, GlbStaff.CurrentUser.GS_FullName, job.JH_JobNum, GlbCompany.CurrentCompany.GC_Code, staff.GS_LoginName, staff.GS_FullName);
			AssertEquals("Email Body", expectedBody, GetBody(email));
		}

		public void TestEmailBodyWithReopenedUserButJobClosed()
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Closed.Code;
			var email = new JobReopenedEmail(job, staff.PK);
			var expectedBody = string.Format(@"The following job was reopened by {0} ({1}) : 
Number: '{2}', Company: '{3}'. However this was closed again after some changes.

Access granted by {4} ({5})", GlbStaff.CurrentUser.GS_LoginName, GlbStaff.CurrentUser.GS_FullName, job.JH_JobNum, GlbCompany.CurrentCompany.GC_Code, staff.GS_LoginName, staff.GS_FullName);
			AssertEquals("Email Body", expectedBody, GetBody(email));
		}

		public void TestSendEmailWithFallBackJobReopenNotifyGroup()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_EmailAddress = "test@test.com";
			GlbGroupLink link = Factory.NewWithValidTestData<GlbGroupLink>();
			link.GK_GG = group.PK;
			link.GK_GS = staff.PK;
			Factory.Save();

			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			var email = new JobReopenedEmail(job, staff.PK, job.JH_GB);

			AssertEquals("Precondition:", Guid.Empty, AccountingConfigurationRegistry.Instance.JobReopenNotifyGroup.Value);
			using (AccountingConfigurationRegistry.Instance.JobReopenNotifyGroup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var result = email.Send();
				AssertEquals(EmailSendResult.Successful, result);
			}

			AssertEquals("Precondition:", Guid.Empty, AccountingConfigurationRegistry.Instance.JobReopenNotifyGroup.Value);
			using (AccountingConfigurationRegistry.Instance.JobReopenNotifyGroup.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, group.PK.ToGuid()))
			{
				var result = email.Send();
				AssertEquals(EmailSendResult.Successful, result);
			}

			AssertEquals("Precondition:", Guid.Empty, AccountingConfigurationRegistry.Instance.JobReopenNotifyGroup.Value);
			using (AccountingConfigurationRegistry.Instance.JobReopenNotifyGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, group.PK.ToGuid()))
			{
				var result = email.Send();
				AssertEquals(EmailSendResult.Successful, result);
			}
		}
	}
}
