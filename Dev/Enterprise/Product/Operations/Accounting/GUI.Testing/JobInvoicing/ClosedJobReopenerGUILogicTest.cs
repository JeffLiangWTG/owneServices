using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.GUI.Testing.JobInvoicing
{
	// Additional security-related test cases are encompassed within the ClosedJobReopenerBusinessLogicTest located in Enterprise.Accounting.Business.JobInvoicing.Testing
	public class ClosedJobReopenerGUILogicTest : TestCaseWithFactory
	{
		public void TestClosedJobReopenActions_UniqueClosedJobs()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_Status = JobHeaderStatus.Closed.Code;
			job1.JH_JobNum = "TESTJOB1";

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_Status = JobHeaderStatus.Closed.Code;
			job2.JH_JobNum = "TESTJOB2";

			var jobs = new List<Job> { job1, job1, job2 }; //passing two equal jobs help to test unique jobs scenario.

			AssertEquals(2, jobs.Count(x => x == job1));
			AssertEquals(1, jobs.Count(x => x == job2));

			var closedJobReopener = GetClosedJobReopener(jobs);

			Env.Security.ReopenJob.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			AssertEquals(true, closedJobReopener.ReopenClosedJobs());

			Assert(!job1.IsClosed);
			Assert(!job2.IsClosed);

			var previousMessages = GetPreviousMessages();
			AssertContains(@"Caption: Reopen Jobs, Text: Closed Job(s): TESTJOB1, TESTJOB2
You are about to reopen these closed jobs. Do you want to proceed?", previousMessages);
		}

		public void TestClosedJobReopenActions_ReopenJobSecurity_LoggedInUserIsAuthorized()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_Status = JobHeaderStatus.Closed.Code;
			job1.JH_JobNum = "TESTJOB1";

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_Status = JobHeaderStatus.Closed.Code;
			job2.JH_JobNum = "TESTJOB2";

			var jobs = new List<Job> { job1, job2 };

			var closedJobReopener = GetClosedJobReopener(jobs);

			Env.Security.ReopenJob.IsAllowed = true;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
			AssertEquals(false, closedJobReopener.ReopenClosedJobs());

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			AssertEquals(true, closedJobReopener.ReopenClosedJobs());

			Assert(!job1.IsClosed);
			Assert(!job2.IsClosed);

			var previousMessages = GetPreviousMessages();
			AssertContains(@"Caption: Reopen Jobs, Text: Closed Job(s): TESTJOB1, TESTJOB2
You are about to reopen these closed jobs. Do you want to proceed?", previousMessages);
		}

		public void TestClosedJobReopenActions_ReopenJobSecurity_LoggedInUserIsNotAuthorized_CancelForm()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Closed.Code;
			job.JH_JobNum = "TESTJOB";

			var jobs = new List<Job>() { job };
			var closedJobReopener = GetClosedJobReopener(jobs);

			Env.Security.ReopenJob.IsAllowed = false;

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore;
			AssertEquals(false, closedJobReopener.ReopenClosedJobs());

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			AssertEquals(false, closedJobReopener.ReopenClosedJobs());
			Assert(job.IsClosed);
		}

		public void TestClosedJobReopenActions_ReopenJobSecurity_LoggedInUserIsNotAuthorized_InvalidCredentials()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_Status = JobHeaderStatus.Closed.Code;
			job.JH_JobNum = "TESTJOB";

			var jobs = new List<Job>() { job };
			var closedJobReopener = GetClosedJobReopener(jobs);

			Env.Security.ReopenJob.IsAllowed = false;

			var loginUserName = "User1";
			var loginPassword = "pass";

			var loginFormShownCount = 0;
			var loginFormMessage = string.Empty;

			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var loginForm = form as LoginForm;
				if (loginForm != null)
				{
					loginFormShownCount++;

					loginForm.DoLoginForTest(loginUserName, loginPassword);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					loginFormMessage = loginForm.Message;
				}
			});

			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			AssertEquals(false, closedJobReopener.ReopenClosedJobs());
			AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should Prompt Login Form Only Once", 1, loginFormShownCount);
			Assert(job.IsClosed);

			SecurityTestObject.CreateTestUser(false, Env.Security.ReopenJob.Code, "USR", loginUserName, loginPassword);
			ZFormModaliser.LastFormShownDialogForTest = null;

			AssertEquals(false, closedJobReopener.ReopenClosedJobs());
			AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should Prompt Login Form Only Once", 2, loginFormShownCount);
			Assert(job.IsClosed);

			var expectedMessage1 = @"The user name (or password) entered is incorrect or password is expired. Please re-enter the correct username and password.";
			var expectedMessage2 = @"The Authorizing user does not have security rights for this Branch or Department. Your system administrator maintains each user's security rights.";

			HelperMethodsForTests.AssertWindowsAndMessagesShown(new string[] { expectedMessage1 , expectedMessage2 }, "LoginForm");
		}

		public void TestClosedJobReopenActions_ReopenJobSecurity_LoggedInUserIsNotAuthorized_ValidCredential()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_Status = JobHeaderStatus.Closed.Code;
			job1.JH_JobNum = "TESTJOB1";

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_Status = JobHeaderStatus.Closed.Code;
			job2.JH_JobNum = "TESTJOB2";

			var jobs = new List<Job> { job1, job2 };

			var closedJobReopener = GetClosedJobReopener(jobs);

			Env.Security.ReopenJob.IsAllowed = false;

			var loginUserName = "User1";
			var loginPassword = "pass";

			var loginFormShownCount = 0;
			var loginFormMessage = string.Empty;

			ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
			{
				var loginForm = form as LoginForm;
				if (loginForm != null)
				{
					loginFormShownCount++;

					loginForm.DoLoginForTest(loginUserName, loginPassword);
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					loginFormMessage = loginForm.Message;
				}
			});

			ZFormModaliser.LastFormShownDialogForTest = null;
			SecurityTestObject.CreateTestUser(true, Env.Security.ReopenJob.Code, "USR", loginUserName, loginPassword);

			AssertEquals(true, closedJobReopener.ReopenClosedJobs());
			AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			AssertEquals("Should Prompt Login Form Only Once", 1, loginFormShownCount);
			Assert(!job1.IsClosed);
			Assert(!job2.IsClosed);
		}

		public void TestClosedJobReopenActions_ReopenJobSecurity_LoggedInUserIsNotAuthorized_VerifyPopupMessage()
		{
			var job1 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job1.JH_Status = JobHeaderStatus.Closed.Code;
			job1.JH_JobNum = "TESTJOB1";

			var job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			job2.JH_Status = JobHeaderStatus.Closed.Code;
			job2.JH_JobNum = "TESTJOB2";

			var jobs = new List<Job>() { job1, job2 };
			var closedJobReopener = GetClosedJobReopener(jobs);

			Env.Security.ReopenJob.IsAllowed = false;
			ZFormModaliser.LastFormShownDialogForTest = null;

			AssertEquals(false, closedJobReopener.ReopenClosedJobs());

			var expectedLoginFormText =
@"Closed Job(s): TESTJOB1, TESTJOB2
These jobs are currently closed.
Before proceeding they must be re-opened.
You do not have security rights to Re-open Closed Jobs.
A user with security rights to Re-open Closed jobs can authorize this transaction.To re-open these jobs, please have an authorized user enter their username and password below.";

			AssertContains("Login Form Text", expectedLoginFormText, ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);
			AssertEquals("Caption", "Security Override Login", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Text);
		}

		[TestDate(2023, 11, 16)]
		public void TestClosedJobReopenActions_ReopenJobPastAllowedReOpenPeriodSecurity_LoggedInUserIsAuthorized()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S00001", "CNSHA", "AUSYD", transportMode: TransportModes.Sea);
			var shipment2 = TestObjectCreator.CreateShipment("S00002", "CNSHA", "AUSYD", transportMode: TransportModes.Sea);

			using (var job1 = Job.CreateWithMutex(Factory, shipment1))
			using (var job2 = Job.CreateWithMutex(Factory, shipment2))
			{
				job1.JH_Status = JobHeaderStatus.Closed.Code;
				job2.JH_Status = JobHeaderStatus.Closed.Code;

				var jobs = new List<Job> { job1, job2 };
				var closedJobReopener = GetClosedJobReopener(jobs);
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = true;

				job1.JH_A_JOP = new ZDateTime(2023, 10, 31);
				job2.JH_A_JOP = new ZDateTime(2023, 10, 31);

				SetJobClosureConfigurationSetupRegistry();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				AssertEquals(false, closedJobReopener.ReopenClosedJobs());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				AssertEquals(true, closedJobReopener.ReopenClosedJobs());

				Assert(!job1.IsClosed);
				Assert(!job2.IsClosed);

				var previousMessages = GetPreviousMessages();
				AssertContains(@"Caption: Reopen Jobs, Text: Closed Job(s): S00001, S00002
You are about to reopen these closed jobs. Do you want to proceed?", previousMessages);
			}
		}

		[TestDate(2023, 11, 16)]
		public void TestClosedJobReopenActions_ReopenJobPastAllowedReOpenPeriodSecurity_LoggedInUserIsNotAuthorized_CancelForm()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "CNSHA", "AUSYD", transportMode: TransportModes.Sea);
			using (var job = Job.CreateWithMutex(Factory, shipment))
			{
				job.JH_Status = JobHeaderStatus.Closed.Code;
				job.JH_JobNum = "TESTJOB";

				var closedJobReopener = GetClosedJobReopener(new List<Job> { job });
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

				job.JH_A_JOP = new ZDateTime(2023, 10, 31);

				SetJobClosureConfigurationSetupRegistry();

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Ignore;
				AssertEquals(false, closedJobReopener.ReopenClosedJobs());

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				AssertEquals(false, closedJobReopener.ReopenClosedJobs());
				Assert(job.IsClosed);
			}
		}

		[TestDate(2023, 11, 16)]
		public void TestClosedJobReopenActions_ReopenJobPastAllowedReOpenPeriodSecurity_LoggedInUserIsNotAuthorized_InvalidCredentials()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "CNSHA", "AUSYD", transportMode: TransportModes.Sea);
			using (var job = Job.CreateWithMutex(Factory, shipment))
			{
				job.JH_Status = JobHeaderStatus.Closed.Code;
				job.JH_JobNum = "TESTJOB";

				var closedJobReopener = GetClosedJobReopener(new List<Job> { job });
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

				job.JH_A_JOP = new ZDateTime(2023, 10, 31);

				SetJobClosureConfigurationSetupRegistry();

				var loginUserName = "User1";
				var loginPassword = "pass";

				var loginFormShownCount = 0;
				var loginFormMessage = string.Empty;

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;
					if (loginForm != null)
					{
						loginFormShownCount++;

						loginForm.DoLoginForTest(loginUserName, loginPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
						loginFormMessage = loginForm.Message;
					}
				});

				ZFormModaliser.LastFormShownDialogForTest = null;

				AssertEquals(false, closedJobReopener.ReopenClosedJobs());
				AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Should Prompt Login Form Only Once", 1, loginFormShownCount);
				Assert(job.IsClosed);

				ZFormModaliser.LastFormShownDialogForTest = null;
				SecurityTestObject.CreateTestUser(false, Env.Security.ReopenJobPastAllowedReOpenPeriod.Code, "USR", loginUserName, loginPassword);

				AssertEquals(false, closedJobReopener.ReopenClosedJobs());
				AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Should Prompt Login Form Only Once", 2, loginFormShownCount);
				Assert(job.IsClosed);

				var expectedMessage1 = @"The user name (or password) entered is incorrect or password is expired. Please re-enter the correct username and password.";
				var expectedMessage2 = @"The Authorizing user does not have security rights for this Branch or Department. Your system administrator maintains each user's security rights.";

				HelperMethodsForTests.AssertWindowsAndMessagesShown(new string[] { expectedMessage1, expectedMessage2 }, "LoginForm");
			}
		}

		[TestDate(2023, 11, 16)]
		public void TestClosedJobReopenActions_ReopenJobPastAllowedReOpenPeriodSecurity_LoggedInUserIsNotAuthorized_ValidCredential()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001", "CNSHA", "AUSYD", transportMode: TransportModes.Sea);
			using (var job = Job.CreateWithMutex(Factory, shipment))
			{
				job.JH_Status = JobHeaderStatus.Closed.Code;
				job.JH_JobNum = "TESTJOB";

				var closedJobReopener = GetClosedJobReopener(new List<Job> { job });
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

				job.JH_A_JOP = new ZDateTime(2023, 10, 31);

				SetJobClosureConfigurationSetupRegistry();

				var loginUserName = "User1";
				var loginPassword = "pass";

				var loginFormShownCount = 0;
				var loginFormMessage = string.Empty;

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((form) =>
				{
					var loginForm = form as LoginForm;
					if (loginForm != null)
					{
						loginFormShownCount++;

						loginForm.DoLoginForTest(loginUserName, loginPassword);
						ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
						loginFormMessage = loginForm.Message;
					}
				});

				ZFormModaliser.LastFormShownDialogForTest = null;
				SecurityTestObject.CreateTestUser(true, Env.Security.ReopenJobPastAllowedReOpenPeriod.Code, "USR", loginUserName, loginPassword);

				AssertEquals(true, closedJobReopener.ReopenClosedJobs());
				AssertEquals("Should Prompt Login Form", typeof(LoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				AssertEquals("Should Prompt Login Form Only Once", 1, loginFormShownCount);
				Assert(!job.IsClosed);
			}
		}

		[TestDate(2023, 11, 16)]
		public void TestClosedJobReopenActions_ReopenJobPastAllowedReOpenPeriodSecurity_LoggedInUserIsNotAuthorized_VerifyPopupMessage()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S00001", "CNSHA", "AUSYD", transportMode: TransportModes.Sea);
			var shipment2 = TestObjectCreator.CreateShipment("S00002", "CNSHA", "AUSYD", transportMode: TransportModes.Sea);

			using (var job1 = Job.CreateWithMutex(Factory, shipment1))
			using (var job2 = Job.CreateWithMutex(Factory, shipment2))
			{
				job1.JH_Status = JobHeaderStatus.Closed.Code;
				job2.JH_Status = JobHeaderStatus.Closed.Code;

				var jobs = new List<Job>() { job1, job2 };
				var closedJobReopener = GetClosedJobReopener(jobs);
				Env.Security.ReopenJobPastAllowedReOpenPeriod.IsAllowed = false;

				job1.JH_A_JOP = new ZDateTime(2023, 10, 31);
				job2.JH_A_JOP = new ZDateTime(2023, 10, 31);

				SetJobClosureConfigurationSetupRegistry();

				ZFormModaliser.LastFormShownDialogForTest = null;

				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				AssertEquals(false, closedJobReopener.ReopenClosedJobs());

				var expectedLoginFormText =
@"Closed Job(s): S00001, S00002
These jobs are currently closed.
Before proceeding they must be re-opened.
You do not have security rights to Re-open Closed Jobs subjected to Re-Open Restriction.
A user with security rights 'Allow Reopen Jobs Past Allowed Reopen Period' can authorize this transaction.To re-open these jobs, please have an authorized user enter their username and password below.";

				AssertContains("Login Form Text", expectedLoginFormText, ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Message);
				AssertEquals("Caption", "Security Override Login", ((LoginForm)ZFormModaliser.LastFormShownDialogForTest).Text);
			}
		}

		IClosedJobReopener GetClosedJobReopener(IReadOnlyCollection<Job> jobs)
		{
			var mockReOpenClosedJobDataProvider = new Mock<IReOpenClosedJobDataProvider>();
			var reopenClosedJobSecurityOverrideProvider = new ReopenClosedJobSecurityOverrideProvider();

			mockReOpenClosedJobDataProvider.Setup(x => x.Factory).Returns(Factory);
			mockReOpenClosedJobDataProvider.Setup(x => x.GetAllJobs()).Returns(jobs);
			mockReOpenClosedJobDataProvider.Setup(x => x.JobReopenLogText()).Returns(" - Test Log for Job Reopening");

			return new ClosedJobReopener(mockReOpenClosedJobDataProvider.Object, reopenClosedJobSecurityOverrideProvider);
		}

		string GetPreviousMessages() => new ZStringBuilder(UnitTestUserNotification.Instance.PreviousMessages.Select(x => $"Caption: {x.Caption}, Text: {x.Text}")).ToStringWithNewLineBetweenAppends();

		void SetJobClosureConfigurationSetupRegistry()
		{
			var header = new JobClosureConfigurationHeader();
			var configLine = header.ConfigurationCollection.AddNew();
			configLine.JobType = "SHP";
			configLine.DirectionCode = FreightShipmentDirection.Code.Import;
			configLine.Mode = TransportModes.Sea;
			configLine.JobClosureDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobOpenDate;
			configLine.Offset = 5;
			configLine.ReopenRestrictionOffset = 7;
			AccountingConfigurationRegistry.Instance.JobClosureConfigurationSetup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, header);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
