using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Client.EDI.ReleaseBuilds;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Test;
using Enterprise.Client.EDI.ServiceTasks;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using WTG.DevTools.Definitions;
using WTG.DevTools.ServiceClient.Assess;
using WTG.DevTools.SourceControl;
using ZClientEDI.Business.Test;
using static System.FormattableString;

namespace Enterprise.Client.EDI.Test
{
	[TestedType(typeof(ProcessedShelfsServiceTask))]
	public class ProcessedShelfsServiceTaskTest : ServiceTaskTestCase<ProcessedShelfsServiceTask>
	{
		public void TestServiceTaskCanRunInAnyBranch()
		{
			AssertNotNull(GetHostedServiceAttributes().Single(x => x.CanRunInAnyBranch));

			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);
			testHelper.AddWorkflow("YES", null, "AS0,LCD,ASN,5", "CDF,LCD,CLS,1", "SH0,LCD,CLS,3", "CHK,LCD,ASN,20");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(4);

			var logger = new TestServiceLogger();
			ServiceTask.ServiceLogger = logger;

			AssertEquals("Precondition: ", 0, ErrorReporter.TotalErrorCount);
			using (ClearUserContext())
			using (Env.Instance.TemporaryServiceTaskContext(ServiceTask.GetType().Name, canRunInAnyBranch: true))
			{
				AssertNoExceptionThrown(() => ServiceTask.RunTask());
			}
			AssertEquals("No Exception Report", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		static IDisposable ClearUserContext()
		{
			var userContext = EnvProxy.Instance.CurrentUserContext;
			EnvProxy.Instance.ClearUserContext();
			(EnvProxy.Instance as IEnvironmentForTest)?.ResetSecurityForTest();
			return new DisposableAction(() =>
			{
				EnvProxy.Instance.SetUserContext(userContext);
			});
		}

		#region Git Test Results Email
		public void TestSingleGitPassed()
		{
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(ShelfStatuses.Passed, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.PassedAndNotified, shelfset.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Subject", "Success: GitTitle has passed", email.Subject);
			var expectedBodyStart = $@"
""GitTitle"" has passed: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210"">http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210</a>

See <a href=""http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}</a> for details.

<a href=""edient:".Replace(System.Environment.NewLine, "<br />");
			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		public void TestSingleGitCheckedIn()
		{
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(ShelfStatuses.CheckedIn, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.CheckedInAndNotified, shelfset.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Subject", "Success: GitTitle has completed", email.Subject);
			var expectedBodyStart = $@"
""GitTitle"" has completed: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210"">http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210</a>

See <a href=""http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}</a> for details.

<a href=""edient:".Replace(System.Environment.NewLine, "<br />");
			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		public void TestSingleGitRejected()
		{
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(ShelfStatuses.Rejected, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.RejectedAndNotified, shelfset.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Subject", "Failure: GitTitle has been rejected", email.Subject);
			var expectedBodyStart = $@"
""GitTitle"" has been rejected: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210"">http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210</a>

See <a href=""http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}</a> for details.

<a href=""edient:".Replace(System.Environment.NewLine, "<br />");

			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		public void TestSingleGitRejectedForPendingAspectData()
		{
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(ShelfStatuses.RejectedForPendingAspectData, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.RejectedForPendingAspectDataAndNotified, shelfset.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Subject", "Failure: GitTitle has been rejected due to pending review requirements", email.Subject);
			var expectedBodyStart = $@"
""GitTitle"" has been rejected due to pending review requirements: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210"">http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210</a>

See <a href=""http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}</a> for details.

<a href=""edient:".Replace(System.Environment.NewLine, "<br />");

			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		public void TestSingleGitDeploymentFailedShelf()
		{
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(EDIShelvesetInfo.ActionTypes.ShelfsetTest, ShelfStatuses.DeploymentJobFailed, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.DeploymentJobFailedAndNotified, shelfset.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Subject", "Failure: GitTitle has passed but has a failed deployment job", email.Subject);
			var expectedBodyStart = $@"
A deployment job has failed.
""GitTitle"" has passed: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210"">http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210</a>

See <a href=""http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}</a> for details.

<a href=""edient:".Replace(System.Environment.NewLine, "<br />");

			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		public void TestSingleGitDeploymentFailedCheckin()
		{
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(EDIShelvesetInfo.ActionTypes.ShelfCheckin, ShelfStatuses.DeploymentJobFailed, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.DeploymentJobFailedAndNotified, shelfset.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Subject", "Failure: GitTitle has completed but has a failed deployment job", email.Subject);
			var expectedBodyStart = $@"
A deployment job has failed.
""GitTitle"" has completed: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210"">http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210</a>

See <a href=""http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}</a> for details.

<a href=""edient:".Replace(System.Environment.NewLine, "<br />");

			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		public void TestSingleGitDeploymentFailedUat()
		{
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(EDIShelvesetInfo.ActionTypes.UATBuild, ShelfStatuses.DeploymentJobFailed, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.DeploymentJobFailedAndNotified, shelfset.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Subject", "Failure: UAT Build GitTitle has been built but has a failed deployment job", email.Subject);
			var expectedBodyStart = $@"
A deployment job has failed.
UAT Build ""GitTitle"" has been built: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210"">http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210</a>

See <a href=""http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}</a> for details.

<a href=""edient:".Replace(System.Environment.NewLine, "<br />");

			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		public void TestSingleGitDeploymentFailedUatCombined()
		{
			//even with a Git only uat combined test the combined build name will be in the shelf field for the header
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(EDIShelvesetInfo.ActionTypes.UATCombinedBuild, ShelfStatuses.DeploymentJobFailed, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle", "UATCombinedBuildName");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.DeploymentJobFailedAndNotified, shelfset.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Subject", "Failure: UAT Combined Build UATCombinedBuildName has been built but has a failed deployment job", email.Subject);
			var expectedBodyStart = $@"
A deployment job has failed.
UAT Combined Build ""GitTitle"" has been built: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210"">http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210</a>

See <a href=""http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}</a> for details.

<a href=""edient:".Replace(System.Environment.NewLine, "<br />");

			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		public void TestGitSubmissionPassedUat()
		{
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(EDIShelvesetInfo.ActionTypes.UATBuild, ShelfStatuses.Passed, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.PassedAndNotified, shelfset.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Subject", "Success: UAT Build GitTitle has been built", email.Subject);
			var expectedBodyStart = $@"
UAT Build ""GitTitle"" has been built: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210"">http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210</a>

See <a href=""http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}</a> for details.

<a href=""edient:".Replace(System.Environment.NewLine, "<br />");
			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		public void TestGitSubmissionRejectedCheckin()
		{
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(EDIShelvesetInfo.ActionTypes.ShelfCheckin, ShelfStatuses.Rejected, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.RejectedAndNotified, shelfset.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Subject", "Failure: GitTitle has been rejected", email.Subject);
			var expectedBodyStart = $@"
""GitTitle"" has been rejected: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210"">http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210</a>

See <a href=""http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}</a> for details.

<a href=""edient:".Replace(System.Environment.NewLine, "<br />");
			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		public void TestMultipleGitSubmissionsCheckedIn()
		{
			var gitPulls = new List<(string gitPull, string title)>
			{
				("http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle1"),
				("http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/MagicTrick/pullrequest/1497?_a=overview", "GitTitle2"),
			};

			var shelfset = ConfigureShelfsetForServiceTaskWithMultipleGitPulls(EDIShelvesetInfo.ActionTypes.ShelfCheckin, ShelfStatuses.CheckedIn, null, gitPulls);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.CheckedInAndNotified, shelfset.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertContains("email.Subject", "Success: GitTitle1 has completed", email.Subject);
			var expectedBodyStart = $@"
""GitTitle1"" has completed: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210"">http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210</a>
""GitTitle2"" has completed: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/MagicTrick/pullrequest/1497"">http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/MagicTrick/pullrequest/1497</a>

See <a href=""http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}</a> for details.

<a href=""edient:".Replace(System.Environment.NewLine, "<br />");

			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		public void TestMultipleGitAspect()
		{
			var gitPulls = new List<(string gitPull, string title)>
					{
						("http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle1"),
						("http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/MagicTrick/pullrequest/1497?_a=overview", "GitTitle2"),
						("http://tfs.wtg.zone:8080/tfs/Collection/Project/_git/Repo/pullrequest/123?_a=overview", "GitTitle3"),
					};

			var shelfset = ConfigureShelfsetForServiceTaskWithMultipleGitPulls(EDIShelvesetInfo.ActionTypes.AspectOnlyBuild, ShelfStatuses.Passed, submissionTitle: null, gitPulls);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.PassedAndNotified, shelfset.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertContains("email.Subject", "Success: Aspect Build GitTitle1 has been aspected", email.Subject);
			var expectedBodyStart = $@"
Aspect Build ""GitTitle1"" has been aspected: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210"">http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210</a>
Aspect Build ""GitTitle2"" has been aspected: <a href=""http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/MagicTrick/pullrequest/1497"">http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/MagicTrick/pullrequest/1497</a>
Aspect Build ""GitTitle3"" has been aspected: <a href=""http://tfs.wtg.zone:8080/tfs/Collection/Project/_git/Repo/pullrequest/123"">http://tfs.wtg.zone:8080/tfs/Collection/Project/_git/Repo/pullrequest/123</a>

See <a href=""http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}</a> for details.

<a href=""edient:".Replace(System.Environment.NewLine, "<br />");

			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		public void TestBadTestWithNoShelfOrGit()
		{
			(var staffCode, var workItem, var processTask) = SetupWorkItem();

			processTask.P9_Notes = new ZBlob(System.Text.Encoding.UTF8.GetBytes("Invalid Git Pull"));

			var shelfset = new EDIShelvesetInfo(staffCode, string.Empty, EDIShelvesetInfo.ActionTypes.ShelfsetTest, processTask)
			{
				UserHeaderPK = Guid.NewGuid(),
				Status = ShelfStatuses.Rejected,
			};

			ServiceTask.processTasks.Add(processTask);
			ServiceTask.shelvesets.Add(shelfset);
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.RejectedAndNotified, shelfset.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Subject", "Failure: Your submission to DAT has been rejected", email.Subject);
			var expectedBodyStart = $@"

See <a href=""http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK}</a> for details.

<a href=""edient:".Replace(System.Environment.NewLine, "<br />");
			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		EDIShelvesetInfo ConfigureShelfsetForServiceTaskWithGitPull(string shelfStatus, string gitPull, string title)
		{
			var shelfType = shelfStatus == ShelfStatuses.CheckedIn ? EDIShelvesetInfo.ActionTypes.ShelfCheckin : EDIShelvesetInfo.ActionTypes.ShelfsetTest;
			return ConfigureShelfsetForServiceTaskWithGitPull(shelfType, shelfStatus, gitPull, title, null);
		}

		EDIShelvesetInfo ConfigureShelfsetForServiceTaskWithGitPull(string shelfType, string shelfStatus, string gitPull, string title, string submissionTitle = null)
		{
			var gitPulls = new List<(string gitPull, string title)>
			{
				(gitPull, title),
			};

			return ConfigureShelfsetForServiceTaskWithMultipleGitPulls(shelfType, shelfStatus, submissionTitle, gitPulls);
		}

		EDIShelvesetInfo ConfigureShelfsetForServiceTaskWithMultipleGitPulls(string shelfType, string shelfStatus, string submissionTitle, List<(string gitPull, string title)> gitPulls)
		{
			var (staffCode, _, processTask) = SetupWorkItem();

			var utStatus = shelfStatus;
			if (shelfStatus == ShelfStatuses.DeploymentJobFailed)
			{
				utStatus = shelfType == EDIShelvesetInfo.ActionTypes.ShelfCheckin ? ShelfStatuses.CheckedIn : ShelfStatuses.Passed;
			}

			var shelfset = AddMockedShelvesetInfo(staffCode, shelfType, shelfStatus, processTask, gitPulls, submissionTitle, utStatus);

			ServiceTask.processTasks.Add(processTask);
			ServiceTask.shelvesets.Add(shelfset);
			Factory.Save();

			return shelfset;
		}

		(string staffCode, NewWorkItem workItem, WorkItemProcessTask processTask) SetupWorkItem()
		{
			var staffCode = CreateStaffCodeForEmailNotifications();
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_Summary = "Update email messages to work with git pull requests";

			var processTask = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();
			processTask.P9_ActualDuration = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, ZDateTime.Now.Day);
			processTask.P9_GS_NKAssignedStaffMember = staffCode;
			return (staffCode, workItem, processTask);
		}

		string CreateStaffCodeForEmailNotifications()
		{
			const string staffCode = "XYZ";
			const string groupCode = "TST";

			var staff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffCode);
			if (staff is null)
			{
				staff = Factory.New<GlbStaff>();
				staff.GS_Code = staffCode;
				staff.GS_EmailAddress = "blah@blah.com";
			}

			var group = Factory.LoadFromNaturalKey<GlbGroup>(GlbGroupSchema.GG_Code, groupCode);
			if (group is null)
			{
				group = Factory.New<GlbGroup>();
				group.GG_Code = groupCode;
				group.Staff.Add(staff);
				EDIDataRegistry.Instance.ProcessedShelfsNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			}

			return staffCode;
		}

		EDIShelvesetInfo AddMockedShelvesetInfo(string staffCode, string actionType, string uhStatus, WorkItemProcessTask processTask, List<(string gitPull, string title)> gitPulls, string submissionTitle, string utStaus)
		{
			var uhComments = string.Join(System.Environment.NewLine, gitPulls.Select(x => x.gitPull));
			processTask.P9_Notes = new ZBlob(System.Text.Encoding.UTF8.GetBytes(uhComments));

			var shelfset = new EDIShelvesetInfo(staffCode, submissionTitle ?? string.Empty, actionType, processTask)
			{
				UserHeaderPK = Guid.NewGuid(),
				Status = uhStatus,
			};

			foreach (var gitPull in gitPulls)
			{
				var pullUrl = PullRequestUrl.ParseMany(gitPull.gitPull).FirstOrDefault();
				AssertNotNull(pullUrl);
				shelfset.DatGitPullRequests.Add(new DatGitPullRequest(pullUrl.RepositoryUrl, pullUrl.PullRequestId, gitPull.title, utStaus));
			}

			return shelfset;
		}

		#endregion

		[TestDate(2006, 1, 1, 19, 0, 0)]
		public void TestExecute()
		{
			ServiceTask.processTasks.Add(Factory.New<WorkItemProcessTask>());
			ServiceTask.processTasks.Add(Factory.New<WorkItemProcessTask>());
			ServiceTask.processTasks.Add(Factory.New<WorkItemProcessTask>());
			ServiceTask.processTasks.Add(Factory.New<WorkItemProcessTask>());
			ServiceTask.processTasks.Add(Factory.New<WorkItemProcessTask>());
			ServiceTask.processTasks.Add(Factory.New<WorkItemProcessTask>());
			ServiceTask.processTasks.Add(Factory.New<WorkItemProcessTask>());

			ServiceTask.processTasks[0].P9_ActualDuration = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, ZDateTime.Now.Day);
			ZDateTime originalDuration = ServiceTask.processTasks[0].P9_ActualDuration;

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "blah@blah.com";

			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[1].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[2].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[3].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[4].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[5].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[6].P9_GS_NKAssignedStaffMember = staff.GS_Code;

			ServiceTask.processTasks[0].P9_Type = "CH0";
			ServiceTask.processTasks[1].P9_Type = "CH0";
			ServiceTask.processTasks[2].P9_Type = "CH0";
			ServiceTask.processTasks[3].P9_Type = "CH0";
			ServiceTask.processTasks[4].P9_Type = "CH0";
			ServiceTask.processTasks[5].P9_Type = "CH0";
			ServiceTask.processTasks[6].P9_Type = "CH0";

			NewWorkItem newWorkItem1 = Factory.NewWithValidTestData<NewWorkItem>();

			NewWorkItem newWorkItem2 = Factory.NewWithValidTestData<NewWorkItem>();

			NewWorkItem newWorkItem3 = Factory.NewWithValidTestData<NewWorkItem>();

			NewWorkItem newWorkItem4 = Factory.NewWithValidTestData<NewWorkItem>();

			NewWorkItem newWorkItem5 = Factory.NewWithValidTestData<NewWorkItem>();

			NewWorkItem newWorkItem6 = Factory.NewWithValidTestData<NewWorkItem>();

			NewWorkItem newWorkItem7 = Factory.NewWithValidTestData<NewWorkItem>();

			ServiceTask.processTasks[0].P9_ParentID = newWorkItem1.PK;
			ServiceTask.processTasks[1].P9_ParentID = newWorkItem2.PK;
			ServiceTask.processTasks[2].P9_ParentID = newWorkItem3.PK;
			ServiceTask.processTasks[3].P9_ParentID = newWorkItem4.PK;
			ServiceTask.processTasks[4].P9_ParentID = newWorkItem5.PK;
			ServiceTask.processTasks[5].P9_ParentID = newWorkItem6.PK;
			ServiceTask.processTasks[6].P9_ParentID = newWorkItem6.PK;

			ServiceTask.processTasks[0].P9_ParentTableCode = "WKI";
			ServiceTask.processTasks[1].P9_ParentTableCode = "WKI";
			ServiceTask.processTasks[2].P9_ParentTableCode = "WKI";
			ServiceTask.processTasks[3].P9_ParentTableCode = "WKI";
			ServiceTask.processTasks[4].P9_ParentTableCode = "WKI";
			ServiceTask.processTasks[5].P9_ParentTableCode = "WKI";
			ServiceTask.processTasks[6].P9_ParentTableCode = "WKI";

			ServiceTask.processTasks[4].P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			ServiceTask.processTasks[5].P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();

			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			LicenceCompany company = Factory.New<LicenceCompany>();
			company.LC_CompanyCode = "COM";
			company.LC_LE = enterprise.PK;
			company.LC_OH = org.PK;

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			ServiceTask.supportIncidents.Add(Factory.NewWithValidTestData<SupportIncident>());
			ServiceTask.supportIncidents[0].RelatedItems.Add(newWorkItem1);
			ServiceTask.supportIncidents[0].IM_LD = database.PK;
			ServiceTask.supportIncidents[0].IM_LCC = clientCompany.PK;
			ServiceTask.supportIncidents[0].IM_Category = SupportIncidentCategoriesList.Codes.Support;
			ServiceTask.supportIncidents[0].IM_Status = IncidentMainLookups.Status.Working;

			ServiceTask.supportIncidents.Add(Factory.NewWithValidTestData<SupportIncident>());
			ServiceTask.supportIncidents[1].RelatedItems.Add(newWorkItem2);
			ServiceTask.supportIncidents[1].IM_LD = database.PK;
			ServiceTask.supportIncidents[1].IM_LCC = clientCompany.PK;
			ServiceTask.supportIncidents[1].IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			ServiceTask.supportIncidents[1].IM_Status = IncidentMainLookups.Status.Working;

			ServiceTask.supportIncidents.Add(Factory.NewWithValidTestData<SupportIncident>());
			ServiceTask.supportIncidents[2].RelatedItems.Add(newWorkItem3);
			ServiceTask.supportIncidents[2].IM_LD = database.PK;
			ServiceTask.supportIncidents[2].IM_LCC = clientCompany.PK;
			ServiceTask.supportIncidents[2].IM_Category = SupportIncidentCategoriesList.Codes.FeatureRequest;
			ServiceTask.supportIncidents[2].IM_Status = IncidentMainLookups.Status.Open;

			ServiceTask.supportIncidents.Add(Factory.NewWithValidTestData<SupportIncident>());
			ServiceTask.supportIncidents[3].RelatedItems.Add(newWorkItem4);
			ServiceTask.supportIncidents[3].IM_LD = database.PK;
			ServiceTask.supportIncidents[3].IM_LCC = clientCompany.PK;

			ServiceTask.supportIncidents.Add(Factory.NewWithValidTestData<SupportIncident>());
			ServiceTask.supportIncidents[4].RelatedItems.Add(newWorkItem5);
			ServiceTask.supportIncidents[4].IM_LD = database.PK;
			ServiceTask.supportIncidents[4].IM_LCC = clientCompany.PK;

			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "TST";
			group.Staff.Add(staff);
			EDIDataRegistry.Instance.ProcessedShelfsNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Pavlo", "WI00008181", "", ServiceTask.processTasks[0]));
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Alex", "WI00001488", "", ServiceTask.processTasks[1]));
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Igor", "WI00000001", "", ServiceTask.processTasks[2]));
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Anna", "WI00000666", "", ServiceTask.processTasks[3]));
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Paul", "WI00000777", "", ServiceTask.processTasks[4]));
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Kot Matroskin", "WI00000888", "", ServiceTask.processTasks[5]));
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Bret", "WI00011111", "SHV", ServiceTask.processTasks[6]));
			ServiceTask.shelvesets[0].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[1].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[2].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[3].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[4].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[5].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[6].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[0].Status = ShelfStatuses.CheckedIn;
			ServiceTask.shelvesets[1].Status = ShelfStatuses.CheckedIn;
			ServiceTask.shelvesets[2].Status = ShelfStatuses.Rejected;
			ServiceTask.shelvesets[3].Status = ShelfStatuses.Rejected;
			ServiceTask.shelvesets[4].Status = ShelfStatuses.Rejected;
			ServiceTask.shelvesets[5].Status = ShelfStatuses.Rejected;
			ServiceTask.shelvesets[6].Status = ShelfStatuses.Passed;

			Factory.Save();

			AutomaticProcessRegistryBusinessObject registryBusinessObject = new AutomaticProcessRegistryBusinessObject();
			registryBusinessObject.NextRunDateTime = ZDateTime.Now.AddHours(-1);

			InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(originalDuration.AddMinutes(10), ServiceTask.processTasks[0].P9_ActualDuration);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[0].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[1].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, ServiceTask.processTasks[2].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Suspended, ServiceTask.processTasks[3].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Working, ServiceTask.processTasks[4].P9_Status);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[6].P9_Status);
			AssertEquals(ShelfStatuses.CheckedInAndNotified, ServiceTask.shelvesets[0].Status);
			AssertEquals(ShelfStatuses.CheckedInAndNotified, ServiceTask.shelvesets[1].Status);
			AssertEquals(ShelfStatuses.RejectedAndNotified, ServiceTask.shelvesets[2].Status);
			AssertEquals(ShelfStatuses.RejectedAndNotified, ServiceTask.shelvesets[3].Status);
			AssertEquals(ShelfStatuses.PassedAndNotified, ServiceTask.shelvesets[6].Status);
			AssertEquals(1, ServiceTask.processTasks[0].Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.JobClose.Code)).Length);

			AssertEquals(7, Env.OutgoingMailManager.EmailsCreated.Count);

			EmailDef item = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Success: WI00008181 has completed", item.Subject);
			AssertStringInRtf("edient:Command=ShowEditForm&ControllerID=WorkItem&BusinessEntityPK=" + ServiceTask.shelvesets[0].RelatedProcessTask.Parent.PK, item.Body);
			AssertStringInRtf($"See http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK} for details", ServiceTask.processTasks[0].P9_NotesAsString);
			AssertStringInRtf($@"See <a href=""http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK}</a> for details", item.Body);

			item = Env.OutgoingMailManager.EmailsCreated[1];
			AssertEquals("Success: WI00001488 has completed", item.Subject);
			AssertStringInRtf("edient:Command=ShowEditForm&ControllerID=WorkItem&BusinessEntityPK=" + ServiceTask.shelvesets[1].RelatedProcessTask.Parent.PK, item.Body);
			AssertStringInRtf($"See http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[1].UserHeaderPK} for details", ServiceTask.processTasks[1].P9_NotesAsString);
			AssertStringInRtf($@"See <a href=""http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[1].UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[1].UserHeaderPK}</a> for details", item.Body);

			item = Env.OutgoingMailManager.EmailsCreated[2];
			AssertEquals("Failure: WI00000001 has been rejected", item.Subject);
			AssertStringInRtf($@"See <a href=""http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[2].UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[2].UserHeaderPK}</a> for details", item.Body);
			AssertStringInRtf("edient:Command=ShowEditForm&ControllerID=WorkItem&BusinessEntityPK=" + ServiceTask.shelvesets[2].RelatedProcessTask.Parent.PK, item.Body);

			item = Env.OutgoingMailManager.EmailsCreated[3];
			AssertEquals("Failure: WI00000666 has been rejected", item.Subject);
			AssertStringInRtf($@"See <a href=""http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[3].UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[3].UserHeaderPK}</a> for details", item.Body);
			AssertStringInRtf("edient:Command=ShowEditForm&ControllerID=WorkItem&BusinessEntityPK=" + ServiceTask.shelvesets[3].RelatedProcessTask.Parent.PK, item.Body);
			AssertStringInRtf("http://crikey.wtg.zone/TestResults/" + ServiceTask.shelvesets[3].UserHeaderPK, ServiceTask.shelvesets[3].RelatedProcessTask.P9_Notes.ToUTF8());

			item = Env.OutgoingMailManager.EmailsCreated[6];
			AssertEquals("Success: WI00011111 has passed", item.Subject);
			AssertStringInRtf($@"See <a href=""http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[6].UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[6].UserHeaderPK}</a> for details", item.Body);

			AssertEquals(IncidentMainLookups.Status.Closed, ServiceTask.supportIncidents[0].IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, ServiceTask.supportIncidents[0].IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, ServiceTask.supportIncidents[1].IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, ServiceTask.supportIncidents[2].IM_ResolutionCode);

			AssertEquals("Process Task should be suspended", ServiceTask.processTasks[5].P9_Status, ProcessTaskStatusCodeList.Codes.Suspended);
			AssertStringInRtf($"See http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[5].UserHeaderPK} for details", ServiceTask.processTasks[5].P9_NotesAsString);
		}

		public void TestNonCompletedSkillsInEmail_ForTestRun_Rejected()
		{
			RunSubmissionWithNonCompletedLearningUnits(EDITaskTypes.TaskActiveShelfTest, EDIShelvesetInfo.ActionTypes.ShelfsetTest, ShelfStatuses.Rejected, "Failure: WI00011111 has been rejected");
		}

		public void TestNonCompletedSkillsInEmail_ForTestRun_Passed()
		{
			RunSubmissionWithNonCompletedLearningUnits(EDITaskTypes.TaskActiveShelfTest, EDIShelvesetInfo.ActionTypes.ShelfsetTest, ShelfStatuses.Passed, "Success: WI00011111 has passed");
		}

		public void TestNonCompletedSkillsInEmail_ForCheckin_Rejected()
		{
			RunSubmissionWithNonCompletedLearningUnits(EDITaskTypes.TaskActiveCheckin, EDIShelvesetInfo.ActionTypes.ShelfCheckin, ShelfStatuses.Rejected, "Failure: WI00011111 has been rejected");
		}

		public void TestNonCompletedSkillsInEmail_ForCheckin_RejectedDueToPendingAspectData()
		{
			RunSubmissionWithNonCompletedLearningUnits(EDITaskTypes.TaskActiveCheckin, EDIShelvesetInfo.ActionTypes.ShelfCheckin, ShelfStatuses.RejectedForPendingAspectData, "Failure: WI00011111 has been rejected due to pending review requirements");
		}

		public void TestNonCompletedSkillsInEmail_ForCheckin_CheckedIn()
		{
			RunSubmissionWithNonCompletedLearningUnits(EDITaskTypes.TaskActiveCheckin, EDIShelvesetInfo.ActionTypes.ShelfCheckin, ShelfStatuses.CheckedIn, "Success: WI00011111 has completed");
		}

		void RunSubmissionWithNonCompletedLearningUnits(string submissionTaskType, string actionType, string submissionStatus, string emailSubject)
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: true);
			testHelper.AddWorkflow("YES", null, submissionTaskType + ",LCD,ASN,2", "CDF,LCD,CLS,1", "CBC,LCD,ASN,3");

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "blah@blah.com";

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", "WI00011111", actionType, testHelper.ShelfTask)
			{
				Status = submissionStatus,
				UserHeaderPK = Guid.NewGuid()
			});

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, submissionTaskType + ",LCD,ASN,2", "CDF,LCD,CLS,1", "CBC,LCD,ASN,3");
			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", "WI00011111", actionType, testHelperWithAspectDat.ShelfTask)
			{
				Status = submissionStatus,
				UserHeaderPK = Guid.NewGuid(),
			};

			var aspectReview1 = AspectReviewSummary.ForAssessAspect(Guid.NewGuid(), aspectPK1, "AspectName");
			var aspectReview2 = AspectReviewSummary.ForAssessAspect(Guid.NewGuid(), aspectPK2, "AspectName");

			shelvesetWithAspectReview.AspectReviews.Add(aspectReview1);
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview2);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";

			testHelper.AssertWorkItemTasks(3);
			testHelperWithAspectDat.AssertWorkItemTasks(3);
			var tasksBefore = testHelperWithAspectDat.WorkItemTasks;

			tasksBefore[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			tasksBefore[1].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			tasksBefore[2].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			tasksBefore[1].P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type != WorkItemProcessTask.AspectReviewTaskType && WorkItemProcessTask.IsReviewTypeTask(t.P9_Type));
			reviewTask.P9_Description = "12345";

			Factory.Save();

			assessServiceClientMock
				.SetupHasCompletedLearningUnit(staff, aspectPK1, false)
				.SetupHasCompletedLearningUnit(staff, aspectPK2, false)
				.SetupGetLearningUnitUrl(aspectPK1, "https://nothing/assess/1")
				.SetupGetLearningUnitUrl(aspectPK2, "https://nothing/assess/2");

			var logger = InitialiseAndRunTaskSchedule(ServiceTask);
			var logOutput = logger.ToString();

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var item = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(emailSubject, item.Subject);
			AssertStringInRtf($@"See <a href=""http://crikey.wtg.zone/TestResults/{shelvesetWithAspectReview.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelvesetWithAspectReview.UserHeaderPK}</a> for details", item.Body);
			AssertStringInRtf("edient:Command=ShowEditForm&ControllerID=WorkItem&BusinessEntityPK=" + shelvesetWithAspectReview.RelatedProcessTask.Parent.PK, item.Body);
			AssertStringInRtf("We have detected the following areas of knowledge relevant to your submission. We suggest you complete these courses by following the links below:", item.Body);
			AssertStringInRtf(@"<a href=""https://nothing/assess/1"">Awesome Skill</a><br />", item.Body);
			AssertStringInRtf(@"<a href=""https://nothing/assess/2"">New Skill</a><br />", item.Body);

			var (status, logFragment) = GetExpectedSubmissionTaskDetails();
			var submissionTask = testHelperWithAspectDat.AssertTaskExists(2, submissionTaskType, status);
			const string pendingAspectFailureMessage = "Task suspended due to pending aspects.";

			if (submissionStatus == ShelfStatuses.RejectedForPendingAspectData)
			{
				AssertContains(pendingAspectFailureMessage, submissionTask.P9_NotesAsString);
			}
			else
			{
				AssertNotContains(pendingAspectFailureMessage, submissionTask.P9_NotesAsString);
			}
			AssertContains(logFragment + submissionTask.P9_TaskID, logOutput);

			(string status, string logFragment) GetExpectedSubmissionTaskDetails()
			{
				switch (submissionStatus)
				{
					case ShelfStatuses.Passed:
						return (ProcessTaskStatusCodeList.Codes.Closed, "Marking task as passed : ");

					case ShelfStatuses.CheckedIn:
						return (ProcessTaskStatusCodeList.Codes.Closed, "Marking task as checked in : ");

					case ShelfStatuses.RejectedForPendingAspectData:
						return (ProcessTaskStatusCodeList.Codes.Suspended, "Marking task as suspended : "); // No QI is needed since we instead use specifically created review tasks.

					case ShelfStatuses.Rejected:
						return (ProcessTaskStatusCodeList.Codes.Cancelled, "Marking task as cancelled : "); // A QI is needed, so we cancel the submission task and create a new one as part of the QI.

					default:
						throw new ArgumentException("Invalid submissionStatus: " + submissionStatus);
				}
			}
		}

		public void TestEmailsIncludeWorkItemSummary()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "XYZ";
			staff.GS_EmailAddress = "blah@blah.com";

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			workItem.WKI_Summary = "This should be in the final email";

			var processTask = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();
			processTask.P9_ActualDuration = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, ZDateTime.Now.Day);
			processTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			processTask.P9_Type = "CH0";

			ServiceTask.processTasks.Add(processTask);

			var group = Factory.New<GlbGroup>();
			group.GG_Code = "TST";
			group.Staff.Add(staff);

			EDIDataRegistry.Instance.ProcessedShelfsNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			var shelfset = new EDIShelvesetInfo("Jacob", "WI00008181", "", processTask);
			shelfset.UserHeaderPK = Guid.NewGuid();
			shelfset.Status = ShelfStatuses.CheckedIn;
			ServiceTask.shelvesets.Add(shelfset);

			Factory.Save();

			var registryBusinessObject = new AutomaticProcessRegistryBusinessObject();
			registryBusinessObject.NextRunDateTime = ZDateTime.Now.AddHours(-1);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, processTask.P9_Status);
			AssertEquals(serviceLogger.ToString(), ShelfStatuses.CheckedInAndNotified, shelfset.Status);
			AssertEquals(1, processTask.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.JobClose.Code)).Length);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);

			var item = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Success: WI00008181 has completed", item.Subject);
			AssertContains(workItem.WKI_WorkItemNumber + " - This should be in the final email", item.Body);
		}

		protected Guid AddUserTestHeader(DbConnection connection, string status, Guid uhPk)
		{
			using (var command = connection.Command(
				@"insert into UserTestHeader (UH_PK, UH_U1, UH_Submitted, UH_Type, UH_WorkItem, UH_Status, UH_ChangeSetId, UH_DateRecordAdded)
					values (@uhPk, (select top 1 U1_PK from [User]), getdate(), @type, @wiPk, @status, @changesetId, getdate())"))
			{
				command.AddParameter("uhPk", SqlDbType.UniqueIdentifier, uhPk);
				command.AddParameter("type", SqlDbType.VarChar, 3, "DBL");
				command.AddParameter("wiPk", SqlDbType.UniqueIdentifier, DBNull.Value);
				command.AddParameter("status", SqlDbType.VarChar, 3, status);
				command.AddParameter("changesetId", SqlDbType.Int, DBNull.Value);
				command.ExecuteNonQuery();
			}
			return uhPk;
		}

		protected void AddUserTest(DbConnection connection, Guid uhPk, string branch, string status)
		{
			using (var command = connection.Command(
				@"insert into UserTest (UT_PK, UT_UH, UT_Branch, UT_Status, UT_IsBranchInShelfChanges, UT_IsDeployOnly)
					values (@utPk, @uhPk, @branch, @status, 1, 0)"))
			{
				command.AddParameter("utPk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("uhPk", SqlDbType.UniqueIdentifier, AddUserTestHeader(connection, status, uhPk));
				command.AddParameter("branch", SqlDbType.VarChar, 128, branch);
				command.AddParameter("status", SqlDbType.VarChar, 3, status);
				command.ExecuteNonQuery();
			}
		}

		[TestDate(2006, 1, 1, 19, 0, 0)]
		public void TestNoException_WhenPivotAlreadyExists()
		{
			NewWorkItem newWorkItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)newWorkItem1.WorkflowItems.Tasks.AddNew());

			ServiceTask.processTasks[0].P9_ActualDuration = new ZDateTime(ZDateTime.Now.Year, ZDateTime.Now.Month, ZDateTime.Now.Day);
			ZDateTime originalDuration = ServiceTask.processTasks[0].P9_ActualDuration;

			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "blah@blah.com";

			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			ServiceTask.supportIncidents.Add(Factory.NewWithValidTestData<SupportIncident>());
			ServiceTask.supportIncidents[0].RelatedItems.Add(newWorkItem1);
			ServiceTask.supportIncidents[0].IM_LCC = clientCompany.PK;
			ServiceTask.supportIncidents[0].IM_Category = SupportIncidentCategoriesList.Codes.Support;
			ServiceTask.supportIncidents[0].IM_Status = IncidentMainLookups.Status.Working;

			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "TST";
			group.Staff.Add(staff);
			EDIDataRegistry.Instance.ProcessedShelfsNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());
			Guid failedDebugBuildPK = Guid.NewGuid();
			Guid failedReleaseBuildPK = Guid.NewGuid();

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Pavlo", "WI00008181", "", ServiceTask.processTasks[0]));
			ServiceTask.shelvesets[0].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[0].Status = ShelfStatuses.CheckedIn;

			Factory.Save();
			{
				AutomaticProcessRegistryBusinessObject registryBusinessObject = new AutomaticProcessRegistryBusinessObject();
				registryBusinessObject.NextRunDateTime = ZDateTime.Now.AddHours(-1);

				var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

				AssertEquals(serviceLogger.ToString(), ShelfStatuses.CheckedInAndNotified, ServiceTask.shelvesets[0].Status);
				AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, newWorkItem1.WKI_Status);
			}

			ServiceTask.shelvesets[0].Status = ShelfStatuses.CheckedIn;
			ServiceTask.shelvesets[0].UserHeaderPK = Guid.NewGuid();
			newWorkItem1.WKI_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ServiceTask.processTasks[0].P9_Status = "ASN";

			Factory.Save();
			{
				AutomaticProcessRegistryBusinessObject registryBusinessObject = new AutomaticProcessRegistryBusinessObject();
				registryBusinessObject.NextRunDateTime = ZDateTime.Now.AddHours(-1);

				var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

				AssertEquals(serviceLogger.ToString(), ShelfStatuses.CheckedInAndNotified, ServiceTask.shelvesets[0].Status);
				AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, newWorkItem1.WKI_Status);
			}
		}

		[TestDate(2006, 1, 1, 19, 0, 0)]
		public void TestIncidentDeployment_MultipleCheckinTasks()
		{
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "blah@blah.com";

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			LicenceEnterprise enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			LicenceDatabase database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;

			ClientCompany clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			ServiceTask.supportIncidents.Add(Factory.NewWithValidTestData<SupportIncident>());
			ServiceTask.supportIncidents[0].IM_LD = database.PK;
			ServiceTask.supportIncidents[0].IM_LCC = clientCompany.PK;
			ServiceTask.supportIncidents[0].IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			ServiceTask.supportIncidents[0].IM_Status = IncidentMainLookups.Status.Working;

			for (int i = 0; i < 2; ++i)
			{
				NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
				var task = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();
				task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
				task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
				ServiceTask.processTasks.Add(task);

				task.P9_ParentID = workItem.PK;
				ServiceTask.supportIncidents[0].RelatedItems.Add(workItem);
			}

			GlbGroup group = Factory.New<GlbGroup>();
			group.GG_Code = "TST";
			group.Staff.Add(staff);
			EDIDataRegistry.Instance.ProcessedShelfsNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Pavlo", "WI00008180", "", ServiceTask.processTasks[0]));
			ServiceTask.shelvesets[0].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[0].Status = ShelfStatuses.CheckedIn;
			Factory.Save();

			AutomaticProcessRegistryBusinessObject registryBusinessObject = new AutomaticProcessRegistryBusinessObject();
			registryBusinessObject.NextRunDateTime = ZDateTime.Now.AddHours(-1);

			// First ALPHA check-in
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[0].P9_Status);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Assigned, ServiceTask.processTasks[1].P9_Status);
			EmailDef item = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Success: WI00008180 has completed", item.Subject);
			AssertEquals(IncidentMainLookups.Status.Working, ServiceTask.supportIncidents[0].IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Working.WorkItemCreated, ServiceTask.supportIncidents[0].IM_ResolutionCode);

			// Second (last) ALPHA check-in
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Pavlo", "WI00008181", "", ServiceTask.processTasks[1]));
			ServiceTask.shelvesets[1].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[1].Status = ShelfStatuses.CheckedIn;
			Factory.Save();
			registryBusinessObject.NextRunDateTime = ZDateTime.Now.AddHours(-1);
			serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[0].P9_Status);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[1].P9_Status);
			AssertEquals(IncidentMainLookups.Status.Working, ServiceTask.supportIncidents[0].IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, ServiceTask.supportIncidents[0].IM_ResolutionCode);
		}

		public void TestIncidentDeployment_NonCW1CheckinTask()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "USA";
			staff.GS_EmailAddress = "user.a@test.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enterprise = Factory.New<LicenceEnterprise>();
			enterprise.LE_EnterpriseCode = "DDD";
			enterprise.LE_OH = org.PK;

			var database = Factory.New<LicenceDatabase>();
			database.LD_ServerCode = "SRV";
			database.LD_LE = enterprise.PK;
			database.LD_ReleaseRing = ReleaseRings.Codes.ALP;

			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_Code = "COM";
			clientCompany.LCC_LD = database.PK;

			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_LD = database.PK;
			incident.IM_LCC = clientCompany.PK;
			incident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;
			incident.IM_Status = IncidentMainLookups.Status.Working;
			ServiceTask.supportIncidents.Add(incident);

			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			incident.RelatedItems.Add(workItem);

			var task = workItem.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.ALP).CheckinTask;
			ServiceTask.processTasks.Add(task);

			var shelveset = new EDIShelvesetInfo("Tester", "WI00008888", "", task)
			{
				UserHeaderPK = Guid.NewGuid(),
				Status = ShelfStatuses.CheckedIn
			};
			ServiceTask.shelvesets.Add(shelveset);
			Factory.Save();

			ReleaseBuildContent.Factory.Value = _ => new MockReleaseBuildContent();

			var registryBusinessObject = new AutomaticProcessRegistryBusinessObject
			{
				NextRunDateTime = ZDateTime.Now.AddHours(-1)
			};

			InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(IncidentMainLookups.Status.Closed, incident.IM_Status);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.ResolvedAndClosed, incident.IM_ResolutionCode);
			AssertEquals(SupportIncidentLookups.DispositionList.Constants.Closed.Completed, incident.IM_ClosureResolution);
		}

		class MockReleaseBuildContent : IReleaseBuildContent
		{
			public bool IsCargoWiseOneChange(NewWorkItem workItem) => false;
			public bool IsPatchedTo(NewWorkItem workItem, ReleaseBuild releaseBuild) => false;

			public bool IsPatchedTo(NewWorkItem workItem, Version currentVersion)
			{
				throw new NotImplementedException();
			}
		}

		[TestDate(2006, 1, 1, 19, 0, 0)]
		public void TestExecute_GprShelvedForGpcIncidentWhileGpcIsNotAvailable()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "blah@blah.com";

			var gpcIncident = CreateNewGpcIncidentWithWorkItem(workItem, staff);
			gpcIncident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;

			var gpcCheckinTask = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();
			gpcCheckinTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			gpcCheckinTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GPC).CheckinTask;
			gpcCheckinTask.P9_Status = ProcessTaskStatusCodeList.Codes.Cancelled;
			ServiceTask.processTasks.Add(gpcCheckinTask);

			var gprCheckinTask = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();
			gprCheckinTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			gprCheckinTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GPR).CheckinTask;
			gprCheckinTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			ServiceTask.processTasks.Add(gprCheckinTask);

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Andrew", "WI00008180", "", gprCheckinTask));
			ServiceTask.shelvesets[0].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[0].Status = ShelfStatuses.CheckedIn;
			Factory.Save();

			AssertEquals("Precondition: GpcReleaseBuildIsAvailable", false, ServiceTask.GetBuildsForTesting().GpcReleaseBuildIsAvailable);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, gprCheckinTask.P9_Status);
			AssertEquals("Success: WI00008180 has completed", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals("Should have marked Incident as waiting upgrade, because there is a cancelled GPC check-in task", SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, gpcIncident.IM_ResolutionCode);
		}

		[TestDate(2006, 1, 1, 19, 0, 0)]
		public void TestExecute_GprShelvedForGpcIncidentWhileGpcIsNotAvailable_NoCancelledGpcTask()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "blah@blah.com";

			var gpcIncident = CreateNewGpcIncidentWithWorkItem(workItem, staff);

			WorkItemProcessTask closedGpcCheckinTask = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();
			closedGpcCheckinTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			closedGpcCheckinTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GPC).CheckinTask;
			closedGpcCheckinTask.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			ServiceTask.processTasks.Add(closedGpcCheckinTask);

			WorkItemProcessTask gprCheckinTask = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();
			gprCheckinTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			gprCheckinTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GPR).CheckinTask;
			gprCheckinTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			ServiceTask.processTasks.Add(gprCheckinTask);

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Andrew", "WI00008180", "", gprCheckinTask));
			ServiceTask.shelvesets[0].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[0].Status = ShelfStatuses.CheckedIn;
			Factory.Save();

			AssertEquals("Precondition: GpcReleaseBuildIsAvailable", false, ServiceTask.GetBuildsForTesting().GpcReleaseBuildIsAvailable);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, gprCheckinTask.P9_Status);
			AssertEquals("Success: WI00008180 has completed", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals("Shouldn't have marked Incident as waiting upgrade, because there isn't a cancelled GPC check-in task", SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, gpcIncident.IM_ResolutionCode);
		}

		[TestDate(2006, 1, 1, 19, 0, 0)]
		public void TestExecute_GprShelvedForGpcIncidentWhileGpcIsNotAvailable_HasOpenGpcTask()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "blah@blah.com";

			var gpcIncident = CreateNewGpcIncidentWithWorkItem(workItem, staff);

			var openGprCheckinTask = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();
			openGprCheckinTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			openGprCheckinTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GPR).CheckinTask;
			openGprCheckinTask.P9_Status = ProcessTaskStatusCodeList.Codes.Open;
			ServiceTask.processTasks.Add(openGprCheckinTask);

			WorkItemProcessTask gprCheckinTask = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();
			gprCheckinTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			gprCheckinTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GPR).CheckinTask;
			gprCheckinTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			ServiceTask.processTasks.Add(gprCheckinTask);

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Andrew", "WI00008180", "", gprCheckinTask));
			ServiceTask.shelvesets[0].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[0].Status = ShelfStatuses.CheckedIn;
			Factory.Save();

			AssertEquals("Precondition: GpcReleaseBuildIsAvailable", false, ServiceTask.GetBuildsForTesting().GpcReleaseBuildIsAvailable);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, gprCheckinTask.P9_Status);
			AssertEquals("Success: WI00008180 has completed", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals("Shouldn't have marked Incident as waiting upgrade, because there is an open GPR check-in task", SupportIncidentLookups.DispositionList.Constants.Open.AddedAwaitingAssignment, gpcIncident.IM_ResolutionCode);
		}

		[TestDate(2006, 1, 1, 19, 0, 0)]
		public void TestExecute_GprShelvedForGpcIncidentWhileGpcIsNotAvailable_HasNoGpcTask()
		{
			NewWorkItem workItem = Factory.NewWithValidTestData<NewWorkItem>();
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "blah@blah.com";

			var gpcIncident = CreateNewGpcIncidentWithWorkItem(workItem, staff);
			gpcIncident.IM_Category = SupportIncidentCategoriesList.Codes.Defect;

			var gprCheckinTask = (WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew();
			gprCheckinTask.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			gprCheckinTask.P9_Type = ReleaseRings.Lookup(ReleaseRings.Codes.GPR).CheckinTask;
			gprCheckinTask.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			ServiceTask.processTasks.Add(gprCheckinTask);

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Andrew", "WI00008180", "", gprCheckinTask));
			ServiceTask.shelvesets[0].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[0].Status = ShelfStatuses.CheckedIn;
			Factory.Save();

			AssertEquals("Precondition: GpcReleaseBuildIsAvailable", false, ServiceTask.GetBuildsForTesting().GpcReleaseBuildIsAvailable);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, gprCheckinTask.P9_Status);
			AssertEquals("Success: WI00008180 has completed", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertEquals("Shouldn't have marked Incident as waiting upgrade, because there is no GPC check-in task", SupportIncidentLookups.DispositionList.Constants.Closed.WaitingUpgrade, gpcIncident.IM_ResolutionCode);
		}

		public void TestWithNonRtfTaskNote()
		{
			NewWorkItem newWorkItem1 = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)newWorkItem1.WorkflowItems.Tasks.AddNew());
			GlbStaff staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "blah@blah.com";
			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[0].P9_Notes = System.Text.Encoding.UTF8.GetBytes("Some text");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Bret", "WI00022791", "", ServiceTask.processTasks[0]));
			ServiceTask.shelvesets[0].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[0].Status = ShelfStatuses.CheckedIn;
			Factory.Save();
			AutomaticProcessRegistryBusinessObject registryBusinessObject = new AutomaticProcessRegistryBusinessObject();
			registryBusinessObject.NextRunDateTime = ZDateTime.Now.AddHours(-1);
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			var failureMessage = "ProcessTask.P9_Notes should contain something about the completed submission." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString();
			AssertContains(failureMessage, "Your submission to DAT has completed.", ServiceTask.processTasks[0].P9_Notes.ToUTF8());
		}

		public void TestRegenShelfNotification()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "brett.shearer";
			staff.GS_EmailAddress = "Brett.Shearer@wisetechglobal.com";
			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "igor.nuzhnov";
			staff.GS_EmailAddress = "Igor.Nuzhnov@wisetechglobal.com";
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Brett.Shearer", "Regen 2015-04-10 1355", "", null) { Status = "REJ", UserHeaderPK = Guid.NewGuid() });
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Igor.Nuzhnov", "Regen 2015-04-10 1357", "", null) { Status = ShelfStatuses.CheckedIn });
			Factory.Save();
			new AutomaticProcessRegistryBusinessObject { NextRunDateTime = ZDateTime.Now.AddHours(-1) };
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertContainsExactElementsInAnyOrder(serviceLogger.ToString(), new[] { "Failure: Regen 2015-04-10 1355 has been rejected", "Success: Regen 2015-04-10 1357 has completed" }, Env.OutgoingMailManager.EmailsCreated.Select(o => o.Subject));
			AssertEquals("Brett.Shearer@wisetechglobal.com", Env.OutgoingMailManager.EmailsCreated.Single(o => o.Subject.Contains("1355")).Recipients[0].Email);
			AssertEquals("Igor.Nuzhnov@wisetechglobal.com", Env.OutgoingMailManager.EmailsCreated.Single(o => o.Subject.Contains("1357")).Recipients[0].Email);
		}

		public void TestSQLExceptionOnDatStatusUpdate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "werner.pereira";
			staff.GS_EmailAddress = "werner.pereira@wisetechglobal.com";
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[0].P9_Notes = System.Text.Encoding.UTF8.GetBytes("Some text");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Werner.Pereira", "FailDatabaseUpdateShelvesetStatus", "", ServiceTask.processTasks[0]) { Status = ShelfStatuses.CheckedIn, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();
			var userTestPK = ServiceTask.shelvesets[0].UserHeaderPK;

			var previousSuppressReportingOfErrors = ErrorReporter.SuppressReportingOfErrors;
			ErrorReporter.SuppressReportingOfErrors = true;

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			ErrorReporter.SuppressReportingOfErrors = previousSuppressReportingOfErrors;

			AssertStringInRtf(serviceLogger.ToString(), "http://crikey.wtg.zone/TestResults/" + ServiceTask.shelvesets[0].UserHeaderPK.ToString() + " for details.", ServiceTask.shelvesets[0].RelatedProcessTask.P9_Notes.ToAscii());
			AssertEquals(serviceLogger.ToString(), ServiceTask.shelvesets[0].Status, "CIN");

			ServiceTask.shelvesets.RemoveAt(0);
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Werner.Pereira", "No Problem", "", ServiceTask.processTasks[0]) { Status = ShelfStatuses.CheckedIn, UserHeaderPK = Guid.NewGuid() });
			ServiceTask.shelvesets[0].UserHeaderPK = userTestPK;
			Factory.Save();

			InitialiseAndRunTaskSchedule(ServiceTask);

			AssertStringInRtf("Workitem updated more than once" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), "http://crikey.wtg.zone/TestResults/" + ServiceTask.shelvesets[0].UserHeaderPK.ToString() + " for details.", ServiceTask.shelvesets[0].RelatedProcessTask.P9_Notes.ToAscii());
			AssertEquals(serviceLogger.ToString(), ServiceTask.shelvesets[0].Status, "CNF");
		}

		[TestDate(2018, 6, 6, 18, 30, 0)]
		public void TestTaskNotesShouldIncludeStatusAndTimestamp()
		{
			var testCases = new List<string[]>
			{
				new[] { "AspectPass",                   "AS0", EDIShelvesetInfo.ActionTypes.AspectOnlyBuild, ShelfStatuses.Passed,              @"DAT 06-Jun-18 18:30: Your submission to DAT has been aspected." },
				new[] { "AspectFail",                   "AS0", EDIShelvesetInfo.ActionTypes.AspectOnlyBuild, ShelfStatuses.Rejected,            @"DAT 06-Jun-18 18:30: Your submission to DAT has been rejected." },
				new[] { "UATPass",                      "UA0", EDIShelvesetInfo.ActionTypes.UATBuild,        ShelfStatuses.Passed,              @"DAT 06-Jun-18 18:30: Your submission to DAT has been built." },
				new[] { "UATFail",                      "UA0", EDIShelvesetInfo.ActionTypes.UATBuild,        ShelfStatuses.Rejected,            @"DAT 06-Jun-18 18:30: Your submission to DAT has been rejected." },
				new[] { "UATDeployFail",                "UA0", EDIShelvesetInfo.ActionTypes.UATBuild,        ShelfStatuses.DeploymentJobFailed, @"DAT 06-Jun-18 18:30: Your submission to DAT has been built. A deployment job has failed." },
				new[] { "ShelfPass",                    "SH0", EDIShelvesetInfo.ActionTypes.ShelfsetTest,    ShelfStatuses.Passed,              @"DAT 06-Jun-18 18:30: Your submission to DAT has passed." },
				new[] { "ShelfFail",                    "SH0", EDIShelvesetInfo.ActionTypes.ShelfsetTest,    ShelfStatuses.Rejected,            @"DAT 06-Jun-18 18:30: Your submission to DAT has been rejected." },
				new[] { "ShelfDeployFail",              "SH0", EDIShelvesetInfo.ActionTypes.ShelfsetTest,    ShelfStatuses.DeploymentJobFailed, @"DAT 06-Jun-18 18:30: Your submission to DAT has passed. A deployment job has failed." },
				new[] { "CheckinPass",                  "CH0", EDIShelvesetInfo.ActionTypes.ShelfCheckin,    ShelfStatuses.CheckedIn,           @"DAT 06-Jun-18 18:30: Your submission to DAT has completed." },
				new[] { "CheckinFail",                  "CH0", EDIShelvesetInfo.ActionTypes.ShelfCheckin,    ShelfStatuses.Rejected,            @"DAT 06-Jun-18 18:30: Your submission to DAT has been rejected." },
				new[] { "CheckinDeployFail",            "CH0", EDIShelvesetInfo.ActionTypes.ShelfCheckin,    ShelfStatuses.DeploymentJobFailed, @"DAT 06-Jun-18 18:30: Your submission to DAT has completed. A deployment job has failed." },
			};

			foreach (var testCase in testCases)
			{
				var userTestPK = Guid.NewGuid();
				var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
				testHelper.AddWorkflow(null, null, $"{testCase[1]},LCD,ASN,2");
				ServiceTask.shelvesets.Clear();
				ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testCase[0], testCase[2], testHelper.ShelfTask) { Status = testCase[3], UserHeaderPK = userTestPK, PrimaryBranch = "$/Dev" });
				Factory.Save();

				var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
				AssertContains(serviceLogger.ToString(), testCase[4], testHelper.ShelfTask.P9_NotesAsString);
				AssertStringInRtf(serviceLogger.ToString(), $"See http://crikey.wtg.zone/TestResults/{userTestPK} for details", testHelper.ShelfTask.P9_NotesAsString);
			}
		}

		[TestDate(2020, 3, 24, 11, 30, 0)]
		public void TestTaskNotesForGitTest()
		{
			var initialTaskNotes = "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview";
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(ShelfStatuses.Passed, initialTaskNotes, "GitTitle");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.PassedAndNotified, shelfset.Status);
			var expectedNotes = $@"{initialTaskNotes}

DAT 24-Mar-20 11:30: ""GitTitle"" has passed. See http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK} for details.
";
			AssertRtfText(expectedNotes, ServiceTask.processTasks[0].P9_Notes);
		}

		[TestDate(2020, 3, 24, 11, 30, 0)]
		public void TestTaskNotesForSingleGitDeploymentFailedUat()
		{
			var initialTaskNotes = "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview";
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(EDIShelvesetInfo.ActionTypes.UATBuild, ShelfStatuses.DeploymentJobFailed, initialTaskNotes, "GitTitle");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.DeploymentJobFailedAndNotified, shelfset.Status);
			var expectedNotes = $@"{initialTaskNotes}

DAT 24-Mar-20 11:30: UAT Build ""GitTitle"" has been built. A deployment job has failed. See http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK} for details.
";
			AssertRtfText(expectedNotes, ServiceTask.processTasks[0].P9_Notes);
		}

		[TestDate(2020, 3, 24, 11, 30, 0)]
		public void TestTaskNotesForGitSubmissionPassedUat()
		{
			var initialTaskNotes = "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview";
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(EDIShelvesetInfo.ActionTypes.UATBuild, ShelfStatuses.Passed, initialTaskNotes, "GitTitle");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.PassedAndNotified, shelfset.Status);
			var expectedNotes = $@"{initialTaskNotes}

DAT 24-Mar-20 11:30: UAT Build ""GitTitle"" has been built. See http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK} for details.
";
			AssertRtfText(expectedNotes, ServiceTask.processTasks[0].P9_Notes);
		}

		[TestDate(2020, 3, 24, 11, 30, 0)]
		public void TestTaskNotesForGitSubmissionRejectedCheckin()
		{
			var initialTaskNotes = "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview";
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(EDIShelvesetInfo.ActionTypes.ShelfCheckin, ShelfStatuses.Rejected, initialTaskNotes, "GitTitle");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.RejectedAndNotified, shelfset.Status);
			var expectedNotes = $@"{initialTaskNotes}

DAT 24-Mar-20 11:30: ""GitTitle"" has been rejected. See http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK} for details.
";
			AssertRtfText(expectedNotes, ServiceTask.processTasks[0].P9_Notes);
		}

		[TestDate(2020, 3, 24, 11, 30, 0)]
		public void TestTaskNotesForMultipleGitSubmissionsCheckedIn()
		{
			var gitPullUri1 = "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview";
			var gitPullUri2 = "http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/MagicTrick/pullrequest/1497?_a=overview";
			var gitPulls = new List<(string gitPull, string title)>
			{
				(gitPullUri1, "GitTitle1"),
				(gitPullUri2, "GitTitle2"),
			};

			var shelfset = ConfigureShelfsetForServiceTaskWithMultipleGitPulls(EDIShelvesetInfo.ActionTypes.ShelfCheckin, ShelfStatuses.CheckedIn, null, gitPulls);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.CheckedInAndNotified, shelfset.Status);
			var expectedNotes = $@"{gitPullUri1}
{gitPullUri2}

DAT 24-Mar-20 11:30: ""GitTitle1"" has completed. ""GitTitle2"" has completed. See http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK} for details.
";
			AssertRtfText(expectedNotes, ServiceTask.processTasks[0].P9_Notes);
		}

		[TestDate(2020, 3, 24, 11, 30, 0)]
		public void TestTaskNotesForMultipleGitAspect()
		{
			var gitPulls = new List<(string gitPull, string title)>
					{
						("http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle1"),
						("http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/MagicTrick/pullrequest/1497?_a=overview", "GitTitle2"),
						("http://tfs.wtg.zone:8080/tfs/Collection/Project/_git/Repo/pullrequest/123?_a=overview", "GitTitle3"),
					};

			var shelfset = ConfigureShelfsetForServiceTaskWithMultipleGitPulls(EDIShelvesetInfo.ActionTypes.AspectOnlyBuild, ShelfStatuses.Passed, submissionTitle: null, gitPulls);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.PassedAndNotified, shelfset.Status);
			var expectedNotes = $@"http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview
http://tfs.wtg.zone:8080/tfs/CargoWise/InternalTools/_git/MagicTrick/pullrequest/1497?_a=overview
http://tfs.wtg.zone:8080/tfs/Collection/Project/_git/Repo/pullrequest/123?_a=overview

DAT 24-Mar-20 11:30: Aspect Build ""GitTitle1"" has been aspected. Aspect Build ""GitTitle2"" has been aspected. Aspect Build ""GitTitle3"" has been aspected. See http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK} for details.
";
			AssertRtfText(expectedNotes, ServiceTask.processTasks[0].P9_Notes);
		}

		[TestDate(2020, 3, 24, 11, 30, 0)]
		public void TestTaskNotesForBadTestWithNoShelfOrGit()
		{
			(var staffCode, var workItem, var processTask) = SetupWorkItem();

			processTask.P9_Notes = new ZBlob(System.Text.Encoding.UTF8.GetBytes("Invalid Git Pull"));

			var shelfset = new EDIShelvesetInfo(staffCode, string.Empty, EDIShelvesetInfo.ActionTypes.ShelfsetTest, processTask)
			{
				UserHeaderPK = Guid.NewGuid(),
				Status = ShelfStatuses.Rejected,
			};

			ServiceTask.processTasks.Add(processTask);
			ServiceTask.shelvesets.Add(shelfset);
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.RejectedAndNotified, shelfset.Status);
			var expectedNotes = $@"Invalid Git Pull

DAT 24-Mar-20 11:30: Your submission to DAT has been rejected. See http://crikey.wtg.zone/TestResults/{shelfset.UserHeaderPK} for details.
";
			AssertRtfText(expectedNotes, ServiceTask.processTasks[0].P9_Notes);
		}

		[TestDate(2018, 6, 7, 11, 30, 0)]
		public void TestMultipleTaskNotesOnSameTask()
		{
			var userTestPKFail = Guid.NewGuid();
			var userTestPKDeployFail = Guid.NewGuid();
			var userTestPKPass = Guid.NewGuid();
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null, $"SH0,LCD,ASN,2");

			ServiceTask.shelvesets.Clear();
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", "FirstShelfFail", EDIShelvesetInfo.ActionTypes.ShelfsetTest, testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = userTestPKFail });
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			ServiceTask.shelvesets.Clear();
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", "SecondShelfDeployFail", EDIShelvesetInfo.ActionTypes.ShelfsetTest, testHelper.ShelfTask) { Status = ShelfStatuses.DeploymentJobFailed, UserHeaderPK = userTestPKDeployFail });
			testHelper.ShelfTask.P9_Status = "ASN";
			Factory.Save();

			InitialiseAndRunTaskSchedule(ServiceTask);

			ServiceTask.shelvesets.Clear();
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", "LastShelfPass", EDIShelvesetInfo.ActionTypes.ShelfsetTest, testHelper.ShelfTask) { Status = ShelfStatuses.Passed, UserHeaderPK = userTestPKPass });
			testHelper.ShelfTask.P9_Status = "ASN";
			Factory.Save();

			InitialiseAndRunTaskSchedule(ServiceTask);

			var expectedFormat = $@"DAT 07-Jun-18 11:30: Your submission to DAT has been rejected. See http://crikey.wtg.zone/TestResults/{userTestPKFail} for details.


DAT 07-Jun-18 11:30: Your submission to DAT has passed. A deployment job has failed. See http://crikey.wtg.zone/TestResults/{userTestPKDeployFail} for details.


DAT 07-Jun-18 11:30: Your submission to DAT has passed. See http://crikey.wtg.zone/TestResults/{userTestPKPass} for details.";

			MasterFilesTestHelper.AssertRtfText("The notes should contain all results nicely spaced." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), expectedFormat, testHelper.ShelfTask.P9_Notes);
		}

		public void TestSuccessfulShelfTestAddsCrikeyLinkToTaskNotes()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "blah@blah.com";
			var task = ServiceTask.processTasks[0];
			task.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task.P9_Notes = System.Text.Encoding.UTF8.GetBytes("Some text");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Bret", "WI00022791", "", ServiceTask.processTasks[0]));
			ServiceTask.shelvesets[0].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[0].Status = ShelfStatuses.Passed;
			Factory.Save();

			new AutomaticProcessRegistryBusinessObject { NextRunDateTime = ZDateTime.Now.AddHours(-1) };
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			var expectedLogs =
$@"Information|Processing  submission 'WI00022791' from task: {task.P9_TaskID}
Information|Marking task as passed : {task.P9_TaskID}
Debug|ASSESS review requirements not found for submission: 'WI00022791', Related task: TaskID:'{task.P9_TaskID}', Pk:{task.PK}
Debug|Email sent to blah@blah.com for submission 'WI00022791'
Debug|Saving batch factory
Information|Updated Dat Status for submission 'WI00022791' from task: {task.P9_TaskID}
";
			AssertEquals(expectedLogs, serviceLogger.ToString());
			AssertStringInRtf(@"Your submission to DAT has passed. See http://crikey.wtg.zone/TestResults/" + ServiceTask.shelvesets[0].UserHeaderPK, task.P9_Notes.ToUTF8());
		}

		public void TestStateAfterErrorUpdatingShelvesetStatus()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "bret@wtg.com";
			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[0].P9_Notes = System.Text.Encoding.UTF8.GetBytes("Some text");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Bret", "NoProblem", "", ServiceTask.processTasks[0]));
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Bret", "FailUpdateShelvesetStatus", "", ServiceTask.processTasks[1]));
			ServiceTask.shelvesets[0].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[0].Status = ShelfStatuses.CheckedIn;
			ServiceTask.shelvesets[1].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[1].Status = ShelfStatuses.CheckedIn;
			Factory.Save();
			new AutomaticProcessRegistryBusinessObject { NextRunDateTime = ZDateTime.Now.AddHours(-1) };
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			var logOutput = serviceLogger.ToString();
			Factory.ReloadAll<WorkItemProcessTask>();
			AssertEquals(logOutput, ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[0].P9_Status);
			AssertEquals(logOutput, ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[1].P9_Status);
			AssertEquals(logOutput, ShelfStatuses.CheckedInAndNotified, ServiceTask.shelvesets[0].Status);
			AssertEquals(logOutput, ShelfStatuses.CheckedIn, ServiceTask.shelvesets[1].Status);
			AssertContains($"Information|Marking task as checked in : {serviceTask.processTasks[0].P9_TaskID}", logOutput);
			AssertContains($"Information|Marking task as checked in : {serviceTask.processTasks[1].P9_TaskID}", logOutput);
			AssertContains($"Error|Unable to process submission 'FailUpdateShelvesetStatus' from task: {serviceTask.processTasks[1].P9_TaskID}", logOutput);
		}

		public void TestOtherShelvesProcessedAfterErrors()
		{
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "lee@wtg.com";
			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[0].P9_Notes = System.Text.Encoding.UTF8.GetBytes("Some text");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Bret", "FailUpdateShelvesetStatus", "", ServiceTask.processTasks[1]));
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Bret", "NoProblem", "", ServiceTask.processTasks[0]));
			ServiceTask.shelvesets[0].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[0].Status = ShelfStatuses.CheckedIn;
			ServiceTask.shelvesets[1].UserHeaderPK = Guid.NewGuid();
			ServiceTask.shelvesets[1].Status = ShelfStatuses.CheckedIn;
			Factory.Save();
			new AutomaticProcessRegistryBusinessObject { NextRunDateTime = ZDateTime.Now.AddHours(-1) };
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			var logOutput = serviceLogger.ToString();
			Factory.ReloadAll<WorkItemProcessTask>();
			AssertEquals(logOutput, ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[0].P9_Status);
			AssertEquals(logOutput, ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[1].P9_Status);
			AssertEquals(logOutput, ShelfStatuses.CheckedIn, ServiceTask.shelvesets[0].Status);
			AssertEquals(logOutput, ShelfStatuses.CheckedInAndNotified, ServiceTask.shelvesets[1].Status);

			AssertContains($"Information|Marking task as checked in : {serviceTask.processTasks[0].P9_TaskID}", logOutput);
			AssertContains($"Information|Marking task as checked in : {serviceTask.processTasks[1].P9_TaskID}", logOutput);
			AssertContains($"Error|Unable to process submission 'FailUpdateShelvesetStatus' from task: {serviceTask.processTasks[1].P9_TaskID}", logOutput);
			serviceLogger.ClearLog();
			serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			logOutput = serviceLogger.ToString();
			AssertNotContains($"Information|Marking task as checked in : {serviceTask.processTasks[0].P9_TaskID}", logOutput);
			AssertContains($"Error|Unable to process submission 'FailUpdateShelvesetStatus' from task: {serviceTask.processTasks[1].P9_TaskID}", logOutput);
		}

		public void TestSuccessfulUATBuild()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "bret@wtg.com";
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[0].P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ServiceTask.processTasks[0].P9_ParentID = workItem.PK;
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Bret", "WI01010101", "UAB", ServiceTask.processTasks[0]) { UserHeaderPK = Guid.NewGuid(), Status = ShelfStatuses.Passed });
			Factory.Save();
			new AutomaticProcessRegistryBusinessObject { NextRunDateTime = ZDateTime.Now.AddHours(-1) };
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[0].P9_Status);
			AssertStringInRtf(@"Your submission to DAT has been built. See http://crikey.wtg.zone/TestResults/" + ServiceTask.shelvesets[0].UserHeaderPK, ServiceTask.processTasks[0].P9_Notes.ToUTF8());
			AssertEquals("Success: UAT Build WI01010101 has been built", Env.OutgoingMailManager.EmailsCreated.Single().Subject);
		}

		public void TestFailedUATBuild()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "bret@wtg.com";
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[0].P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Bret", "WI01010101", "UAB", ServiceTask.processTasks[0]) { UserHeaderPK = Guid.NewGuid(), Status = ShelfStatuses.Rejected });
			Factory.Save();
			new AutomaticProcessRegistryBusinessObject { NextRunDateTime = ZDateTime.Now.AddHours(-1) };
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Suspended, ServiceTask.processTasks[0].P9_Status);
			AssertStringInRtf("See http://crikey.wtg.zone/TestResults/" + ServiceTask.shelvesets[0].UserHeaderPK, ServiceTask.processTasks[0].P9_Notes.ToUTF8());
			AssertEquals("Failure: UAT Build WI01010101 has been rejected", Env.OutgoingMailManager.EmailsCreated.Single().Subject);
		}

		[TestDate(2020, 3, 24, 11, 30, 0)]
		public void TestSuccessfulAspectOnlyBuild()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "lee@wtg.com";
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[0].P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Bret", "WI01010101", "ASB", ServiceTask.processTasks[0]) { UserHeaderPK = Guid.NewGuid(), Status = ShelfStatuses.Passed });
			Factory.Save();
			new AutomaticProcessRegistryBusinessObject { NextRunDateTime = ZDateTime.Now.AddHours(-1) };
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[0].P9_Status);
			AssertStringInRtf(@"Your submission to DAT has been aspected. See http://crikey.wtg.zone/TestResults/" + ServiceTask.shelvesets[0].UserHeaderPK, ServiceTask.processTasks[0].P9_Notes.ToUTF8());
			AssertEquals("Success: Aspect Build WI01010101 has been aspected", Env.OutgoingMailManager.EmailsCreated.Single().Subject);
		}

		[TestDate(2020, 3, 24, 11, 30, 0)]
		public void TestSuccessfulExperimentalPullRequestBuild()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "yaakov@example.com";
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[0].P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ServiceTask.processTasks[0].P9_Type = EDITaskTypes.TaskActiveExperimentalPullRequest;
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Yaakov", string.Empty, "SHV", ServiceTask.processTasks[0])
			{
				UserHeaderPK = Guid.NewGuid(),
				Status = ShelfStatuses.Passed,
				DatGitPullRequests =
				{
					new DatGitPullRequest("http://tfs.wtg.zone:8080/tfs/CargoWise/DevTools/_git/DevTools", 12345, "Janitor Pull Request 2020-03-24", ShelfStatuses.Passed)
				}
			});
			Factory.Save();
			new AutomaticProcessRegistryBusinessObject { NextRunDateTime = ZDateTime.Now.AddHours(-1) };
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[0].P9_Status);
			AssertEquals("PSS", ServiceTask.processTasks[0].P9_Outcome);
			AssertStringInRtf(@"""Janitor Pull Request 2020-03-24"" has passed. See http://crikey.wtg.zone/TestResults/" + ServiceTask.shelvesets[0].UserHeaderPK, ServiceTask.processTasks[0].P9_Notes.ToUTF8());
			AssertEquals("Success: Janitor Pull Request 2020-03-24 has passed", Env.OutgoingMailManager.EmailsCreated.Single().Subject);
		}

		[TestDate(2020, 3, 24, 11, 30, 0)]
		public void TestFailedExperimentalPullRequestBuild()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "yaakov@example.com";
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[0].P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ServiceTask.processTasks[0].P9_Type = EDITaskTypes.TaskActiveExperimentalPullRequest;
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Yaakov", string.Empty, "SHV", ServiceTask.processTasks[0])
			{
				UserHeaderPK = Guid.NewGuid(),
				Status = ShelfStatuses.Rejected,
				DatGitPullRequests =
				{
					new DatGitPullRequest("http://tfs.wtg.zone:8080/tfs/CargoWise/DevTools/_git/DevTools", 12345, "Janitor Pull Request 2020-03-24", ShelfStatuses.Rejected)
				}
			});
			Factory.Save();
			new AutomaticProcessRegistryBusinessObject { NextRunDateTime = ZDateTime.Now.AddHours(-1) };
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[0].P9_Status);
			AssertEquals("FAI", ServiceTask.processTasks[0].P9_Outcome);
			AssertStringInRtf(@"""Janitor Pull Request 2020-03-24"" has been rejected. See http://crikey.wtg.zone/TestResults/" + ServiceTask.shelvesets[0].UserHeaderPK, ServiceTask.processTasks[0].P9_Notes.ToUTF8());
			AssertEquals("Failure: Janitor Pull Request 2020-03-24 has been rejected", Env.OutgoingMailManager.EmailsCreated.Single().Subject);
		}

		[TestDate(2020, 3, 24, 11, 30, 0)]
		public void TestFailedDeploymentExperimentalPullRequestBuild()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "yaakov@example.com";
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[0].P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ServiceTask.processTasks[0].P9_Type = EDITaskTypes.TaskActiveExperimentalPullRequest;
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Yaakov", string.Empty, "SHV", ServiceTask.processTasks[0])
			{
				UserHeaderPK = Guid.NewGuid(),
				Status = ShelfStatuses.DeploymentJobFailed,
				DatGitPullRequests =
				{
					new DatGitPullRequest("http://tfs.wtg.zone:8080/tfs/CargoWise/DevTools/_git/DevTools", 12345, "Janitor Pull Request 2020-03-24", ShelfStatuses.DeploymentJobFailed)
				}
			});
			Factory.Save();
			new AutomaticProcessRegistryBusinessObject { NextRunDateTime = ZDateTime.Now.AddHours(-1) };
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Closed, ServiceTask.processTasks[0].P9_Status);
			AssertEquals("FAI", ServiceTask.processTasks[0].P9_Outcome);
			AssertStringInRtf(@"""Janitor Pull Request 2020-03-24"" has passed. A deployment job has failed. See http://crikey.wtg.zone/TestResults/" + ServiceTask.shelvesets[0].UserHeaderPK, ServiceTask.processTasks[0].P9_Notes.ToUTF8());
			AssertEquals("Failure: Janitor Pull Request 2020-03-24 has passed but has a failed deployment job", Env.OutgoingMailManager.EmailsCreated.Single().Subject);
		}

		public void TestFailedAspectOnlyBuild()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "leet@wtg.com";
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[0].P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Bret", "WI01010101", "ASB", ServiceTask.processTasks[0]) { UserHeaderPK = Guid.NewGuid(), Status = ShelfStatuses.Rejected });
			Factory.Save();
			new AutomaticProcessRegistryBusinessObject { NextRunDateTime = ZDateTime.Now.AddHours(-1) };
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), ProcessTaskStatusCodeList.Codes.Suspended, ServiceTask.processTasks[0].P9_Status);
			AssertStringInRtf("See http://crikey.wtg.zone/TestResults/" + ServiceTask.shelvesets[0].UserHeaderPK, ServiceTask.processTasks[0].P9_Notes.ToUTF8());
			AssertEquals("Failure: Aspect Build WI01010101 has been rejected", Env.OutgoingMailManager.EmailsCreated.Single().Subject);
		}

		public void TestSuccessfulUATCombinedBuild()
		{
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("DAT", null, EDIShelvesetInfo.ActionTypes.UATCombinedBuild, null) { UserHeaderPK = Guid.NewGuid(), Status = ShelfStatuses.Passed, NotificationEmail = "lee+ucb@wtg.com" });
			Factory.Save();
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), "Success: UAT Combined Build Your submission to DAT has been built", Env.OutgoingMailManager.EmailsCreated.Single().Subject);
			AssertEquals("lee+ucb@wtg.com", Env.OutgoingMailManager.EmailsCreated.Single().Recipients[0].Email);
			AssertStringInRtf($@"See <a href=""http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK}</a> for details", Env.OutgoingMailManager.EmailsCreated.Single().Body);
		}

		public void TestFailedUATCombinedBuild()
		{
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("DAT", null, EDIShelvesetInfo.ActionTypes.UATCombinedBuild, null) { UserHeaderPK = Guid.NewGuid(), Status = ShelfStatuses.Rejected, NotificationEmail = "lee+ucb@wtg.com" });
			Factory.Save();
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), "Failure: UAT Combined Build Your submission to DAT has been rejected", Env.OutgoingMailManager.EmailsCreated.Single().Subject);
			AssertEquals("lee+ucb@wtg.com", Env.OutgoingMailManager.EmailsCreated.Single().Recipients[0].Email);
			AssertStringInRtf($@"See <a href=""http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK}</a> for details", Env.OutgoingMailManager.EmailsCreated.Single().Body);
		}

		public void TestSuccessfulUATCombinedChild()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "lee+ucc@wtg.com";
			staff.GS_LoginName = "lee.coady";
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\lee.coady", null, EDIShelvesetInfo.ActionTypes.UATCombinedChild, null) { UserHeaderPK = Guid.NewGuid(), Status = ShelfStatuses.Passed });
			Factory.Save();
			_ = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestFailedUATCombinedChild()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "lee+ucc@wtg.com";
			staff.GS_LoginName = "lee.coady";
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\lee.coady", null, EDIShelvesetInfo.ActionTypes.UATCombinedChild, null) { UserHeaderPK = Guid.NewGuid(), Status = ShelfStatuses.Rejected });
			Factory.Save();
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertEquals(serviceLogger.ToString(), "Failure: UAT Combined Child Your submission to DAT has been rejected", Env.OutgoingMailManager.EmailsCreated.Single().Subject);
			AssertEquals("lee+ucc@wtg.com", Env.OutgoingMailManager.EmailsCreated.Single().Recipients[0].Email);
			AssertStringInRtf($@"See <a href=""http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK}</a> for details", Env.OutgoingMailManager.EmailsCreated.Single().Body);
		}

		public void TestUatCombinedGitChild()
		{
			var (staffCode, _, _) = SetupWorkItem();

			var shelfsetComb = new EDIShelvesetInfo(@"CORP\s_datservice", "UAT Combined Build Name", EDIShelvesetInfo.ActionTypes.UATCombinedBuild, null)
			{
				UserHeaderPK = Guid.NewGuid(),
				Status = ShelfStatuses.Passed,
				PrimaryBranch = "master",
				NotificationEmail = "some.email@wisetechglobal.com"
			};

			var shelfsetChild = new EDIShelvesetInfo(staffCode, null, EDIShelvesetInfo.ActionTypes.UATCombinedChild, null)
			{
				UserHeaderPK = Guid.NewGuid(),
				Status = ShelfStatuses.Passed,
				PrimaryBranch = "master",
				NotificationEmail = "",
			};

			shelfsetChild.DatGitPullRequests.Add(new DatGitPullRequest("http://tfs.wtg.zone:8080/tfs/CargoWise/DevTools/_git/DevTools", 2213, "Combined Child", ShelfStatuses.Passed));

			ServiceTask.shelvesets.Add(shelfsetComb);
			ServiceTask.shelvesets.Add(shelfsetChild);
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			AssertEquals(serviceLogger.ToString(), ShelfStatuses.PassedAndNotified, shelfsetComb.Status);
			AssertEquals(serviceLogger.ToString(), ShelfStatuses.PassedAndNotified, shelfsetChild.Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("email.Subject", "Success: UAT Combined Build UAT Combined Build Name has been built", email.Subject);
			var expectedBodyStart = $@"

See <a href=""http://crikey.wtg.zone/TestResults/{shelfsetComb.UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{shelfsetComb.UserHeaderPK}</a> for details."
			.Replace(System.Environment.NewLine, "<br />");

			AssertStartsWith("email.Body", expectedBodyStart, email.Body);
		}

		public void TestExceptionInRunTaskIsReported()
		{
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("DAT", "PleaseThrowArgumentException", EDIShelvesetInfo.ActionTypes.UATCombinedBuild, null) { UserHeaderPK = Guid.NewGuid(), Status = ShelfStatuses.Rejected, NotificationEmail = "lee+ucb@wtg.com" });
			Factory.Save();
			var serviceLogger = InitialiseTaskSchedule(ServiceTask);
			AssertExceptionThrown(serviceLogger.ToString(), typeof(ArgumentException), () => RunTaskSchedule(ServiceTask));

			AssertContains($"Error|RunTask Failed in ProcessedShelfsServiceTask|System.ArgumentException", serviceLogger.ToString());
			AssertContains("RunTask Failed in ProcessedShelfsServiceTask", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestNullCrikeyConnectionDoesNotThrow()
		{
			DbConnectionCrikey.NoCrikeyConnection.Value = true;
			var serviceLogger = new TestServiceLogger();
			new ProcessedShelfsServiceTask() { ServiceLogger = serviceLogger }.RunTask();
			AssertContains("Crikey db server not configured, this is probably not the production instance of ediProd", serviceLogger.ToString());
		}

		public void TestSuccessfulShelfWithFailedDeploymentJob()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null, "SH0,LCD,ASN,2", "CDF,LCD,CLS,1");
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "LCD";
			staff.GS_EmailAddress = "lee@wtg.com";
			testHelper.WorkItem.WKI_WorkItemNumber = "WI0123";
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, EDIShelvesetInfo.ActionTypes.ShelfsetTest, testHelper.ShelfTask) { Status = ShelfStatuses.DeploymentJobFailed, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();
			testHelper.RefreshWorkItemTasks();

			var task = testHelper.AssertTaskExists("The shelf test task status should be assigned.", 2, "SH0", ProcessTaskStatusCodeList.Codes.Assigned);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.RefreshWorkItemTasks();

			var expectedLogs =
$@"Information|Processing SHV submission 'WI0123' from task: {task.P9_TaskID}
Information|Marking task as suspended : {task.P9_TaskID}
Debug|ASSESS review requirements not found for submission: 'WI0123', Related task: TaskID:'{task.P9_TaskID}', Pk:{task.PK}
Debug|Email sent to lee@wtg.com for submission 'WI0123'
Debug|Saving batch factory
Information|Updated Dat Status for submission 'WI0123' from task: {task.P9_TaskID}
";
			AssertEquals(expectedLogs, serviceLogger.ToString());
			testHelper.AssertTaskExists("The shelf test task status should be updated to suspended.", 2, "SH0", ProcessTaskStatusCodeList.Codes.Suspended);
			AssertEquals("Status should be notified if the deployment fails", ShelfStatuses.DeploymentJobFailedAndNotified, ServiceTask.shelvesets.First().Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Failure: WI0123 has passed but has a failed deployment job", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertContains($@"A deployment job has failed.", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertContains("A deployment job has failed.", testHelper.ShelfTask.P9_NotesAsString);
			AssertStringInRtf($"See http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK} for details", testHelper.ShelfTask.P9_NotesAsString);
			AssertContains($@"See <a href=""http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK}</a> for details", Env.OutgoingMailManager.EmailsCreated[0].Body);
		}

		[TestDate(2018, 6, 6, 18, 30, 0)]
		public void TestFailedShelfWithSuccessfulPullRequestsShouldIncludeSubmissionFailureInTaskNote()
		{
			var testCases = new List<string[]>
			{
				new[] { "AspectOnlyBuild",     "AS0", EDIShelvesetInfo.ActionTypes.AspectOnlyBuild },
				new[] { "UATBuild",            "UA0", EDIShelvesetInfo.ActionTypes.UATBuild },
				new[] { "ShelfsetTest",        "SH0", EDIShelvesetInfo.ActionTypes.ShelfsetTest },
				new[] { "ShelfCheckin",        "CH0", EDIShelvesetInfo.ActionTypes.ShelfCheckin },
				new[] { "ExperimentalTest",    "JP0", EDIShelvesetInfo.ActionTypes.ExperimentalTest },
			};

			foreach (var testCase in testCases)
			{
				var userTestPK = Guid.NewGuid();
				var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
				testHelper.AddWorkflow(null, null, $"{testCase[1]},MM2,ASN,2");
				ServiceTask.shelvesets.Clear();

				var shelfInfo = new EDIShelvesetInfo(@"CORP\Test.User", testCase[0], testCase[2], testHelper.ShelfTask)
				{
					Status = ShelfStatuses.Rejected,
					UserHeaderPK = userTestPK,
					DatGitPullRequests =
					{
						new DatGitPullRequest("http://tfs.wtg.zone:8080/tfs/CargoWise/DevTools/_git/DevTools", 346567, "Pull Request Title", ShelfStatuses.Passed),
						new DatGitPullRequest("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev", 237647, "Pull Request Title", ShelfStatuses.Passed),
						new DatGitPullRequest("https://devops.wisetechglobal.com/wtg/CargoWise/_git/Shared", 989745, "Pull Request Title", ShelfStatuses.Passed),
					},

					PrimaryBranch = "master"
				};

				ServiceTask.shelvesets.Add(shelfInfo);
				Factory.Save();

				var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
				AssertContains(serviceLogger.ToString(), @"DAT 06-Jun-18 18:30: Your submission to DAT has been rejected.", testHelper.ShelfTask.P9_NotesAsString);
			}
		}

		public void TestSuccessfulCheckInWithFailedDeploymentJob()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null, "CH0,LCD,ASN,3", "CDF,LCD,CLS,1", "CBC,REV,CLS,2");
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "LCD";
			staff.GS_EmailAddress = "lee@wtg.com";
			testHelper.WorkItem.WKI_WorkItemNumber = "WI0123";
			var userTestPK = Guid.NewGuid();
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, EDIShelvesetInfo.ActionTypes.ShelfCheckin, testHelper.ShelfTask) { Status = ShelfStatuses.DeploymentJobFailed, UserHeaderPK = userTestPK });
			Factory.Save();
			testHelper.RefreshWorkItemTasks();

			testHelper.AssertTaskExists("The checkin task status should be assigned.", 3, "CH0", ProcessTaskStatusCodeList.Codes.Assigned);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertTaskExists("The shelf test task status should be updated to closed." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 3, "CH0", ProcessTaskStatusCodeList.Codes.Closed);
			AssertEquals("Status should be notified if the deployment fails", ShelfStatuses.DeploymentJobFailedAndNotified, ServiceTask.shelvesets.First().Status);
			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Failure: WI0123 has completed but has a failed deployment job", Env.OutgoingMailManager.EmailsCreated[0].Subject);
			AssertContains("A deployment job has failed.", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertStringInRtf($@"See <a href=""http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK}"">http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK}</a> for details", Env.OutgoingMailManager.EmailsCreated[0].Body);
			AssertStringInRtf($@"Your submission to DAT has completed. A deployment job has failed. See http://crikey.wtg.zone/TestResults/{ServiceTask.shelvesets[0].UserHeaderPK} for details", testHelper.ShelfTask.P9_NotesAsString);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		#region Quality Iteration Tests

		public void TestFailedShelfWithReleaseGroupExplicitlyDisabled_ShouldNotCreateQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);

			testHelper.AddWorkflow("NO", null, "SH0,ADK,ASN,2", "CDF,ADK,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(2);
			testHelper.AssertTaskExists("The shelf test task should be suspended because the release group has quality iterations for failed shelves explicitly disabled." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, "SH0", "SUS");
		}

		public void TestFailedShelfWithReleaseGroupNotSpecifiedAndDefaultIsFalse_ShouldNotCreateQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			testHelper.AddWorkflow("NS", null, "SH0,ADK,ASN,2", "CDF,ADK,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(2);
			testHelper.AssertTaskExists("The shelf test task should be suspended because the release group wasn't specified in the registry and quality iterations are disabled by default." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, "SH0", "SUS");
		}

		public void TestFailedShelfWithReleaseGroupExplicitlyEnabled_ShouldCreateQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			testHelper.AddWorkflow("YES", null, "CH0,ADK,ASN,2", "CDF,ADK,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(4);
			testHelper.AssertTaskExists("The checkin task should be cancelled because quality iterations are enabled for the release group." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, "CH0", "CAN");
			testHelper.AssertTaskExists(3, "CDF", "ASN");
			testHelper.AssertTaskExists(4, "CHK", "ASN");
		}

		public void TestFailedShelfWithReleaseGroupNotSpecifiedAndDefaultIsTrue_ShouldCreateQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);

			testHelper.AddWorkflow("NS", null, "CH0,ADK,ASN,2", "CDF,ADK,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(4);
			testHelper.AssertTaskExists("The checkin task should be cancelled because the release group wasn't specified in the registry but quality iterations are enabled by default." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, "CH0", "CAN");
			testHelper.AssertTaskExists(3, "CDF", "ASN");
			testHelper.AssertTaskExists(4, "CHK", "ASN");
		}

		public void TestFailedShelfWithReleaseGroupNotSpecifiedOnWorkflowAndDefaultIsTrue_ShouldCreateQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);

			testHelper.AddWorkflow(null, null, "CH0,ADK,ASN,2", "CDF,ADK,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(4);
			testHelper.AssertTaskExists("The checkin task should be cancelled because the release group wasn't specified in the registry and the workflow didn't have one either but quality iterations are enabled by default." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, "CH0", "CAN");
			testHelper.AssertTaskExists(3, "CDF", "ASN");
			testHelper.AssertTaskExists(4, "CHK", "ASN");
		}

		public void TestFailedShelfWithCancelledIterateFromTask_ShouldCreateQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);

			testHelper.AddWorkflow("NS", null, "CH0,ADK,ASN,2", "CDF,ADK,CAN,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(4);
			testHelper.AssertTaskExists("The checkin task should be cancelled because the release group wasn't specified in the registry but quality iterations are enabled by default." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, "CH0", "CAN");
			testHelper.AssertTaskExists(3, "CDF", "ASN");
			testHelper.AssertTaskExists(4, "CHK", "ASN");
		}

		public void TestFailedShelfWithNonCheckinTaskLaterInSequenceThanShelfTask_ShouldCreateQualityIterationFromTaskBeforeShelfTask()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);

			testHelper.AddWorkflow("NS", null, "CH0,ADK,ASN,2", "CDU,ADK,CLS,1", "CDF,ADK,CLS,3");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(3);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(5);
			testHelper.AssertTaskExists("The checkin task should be cancelled because the release group wasn't specified in the registry but quality iterations are enabled by default." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, "CH0", "CAN");
			testHelper.AssertTaskExists(3, "CDU", "ASN");
			testHelper.AssertTaskExists(4, "CHK", "ASN");
		}

		public void TestFailedShelfWithOnlyNonCheckinTaskLaterInSequenceThanShelfTask_ShouldNotCreateQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);

			testHelper.AddWorkflow("NS", null, "CH0,ADK,ASN,2", "SH0,ADK,CLS,1", "CDF,ADK,CLS,3");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(3);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(3);
			testHelper.AssertTaskExists("The checkin task should be suspended because the there was no coding task BEFORE the checkin task to iterate from." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, "CH0", "SUS");
		}

		public void TestFailedShelfWithOneWorkflowAndNoNonCheckinTask_ShouldNotCreateQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			testHelper.AddWorkflow("YES", null, "CH1,ADK,ASN,3", "CH0,ADK,CLS,2", "SH0,ADK,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(3);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(3);
			testHelper.AssertTaskExists("The checkin task should be suspended because there were no non-cancelled coding tasks to iterate from." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 3, "CH1", "SUS");
		}

		public void TestFailedShelfWithOneWorkflowAndNoTaskFromSameResource_ShouldNotCreateQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);
			testHelper.AddWorkflow("YES", null, "CH0,ADK,ASN,3", "CDF,DEA,CLS,2", "CDU,DEA,CLS,1");

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(3);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(3);
			testHelper.AssertTaskExists("The checkin task should be suspended because there were no appropriate tasks from the same resource to iterate from." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 3, "CH0", "SUS");
		}

		public void TestFailedShelfWithClosedCheckInTasks_ShouldCreateQualityIterationWithoutCheckInTasks()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);

			testHelper.AddWorkflow("NS", null, "CHG,ADK,ASN,7", "CDF,ADK,CLS,1", "SH0,ADK,CLS,2", "CH0,ADK,CLS,3", "CH1,ADK,CLS,4", "CH2,ADK,CLS,5", "CHC,DAT,CAN,6");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(7);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(10);
			testHelper.AssertTaskExists("The checkin task should be cancelled because a quality iteration was created, and yet..." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 7, "CHG", "CAN");
			testHelper.AssertTaskExists(8, "CDF", "ASN");
			testHelper.AssertTaskExists(9, "SHV", "ASN");
			testHelper.AssertTaskExists("The new checkin task in the quality iteration should be suspended because it is an active checkin type (otherwise it would queue a check-in immediately). And yet...", 10, "CHG", "SUS");
		}

		public void TestFailedAutoPatchShelfWithPullRequest()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);

			testHelper.AddWorkflow("NS", null, "CHG,LCD,ASN,7", "CDF,LCD,CLS,1", "SH0,LCD,CLS,2", "CH0,LCD,CLS,3", "CH1,LCD,CLS,4", "CH2,LCD,CLS,5", "CHC,DAT,CAN,6");
			testHelper.ShelfTask.P9_Notes = System.Text.Encoding.UTF8.GetBytes("https://devops.wisetechglobal.com/wtg/_git/DevTools/pullrequest/7?_a=overview");

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(7);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertStringInRtf(serviceLogger.ToString(), $"https://devops.wisetechglobal.com/wtg/_git/DevTools/pullrequest/7?_a=overview", testHelper.ShelfTask.P9_NotesAsString);

			testHelper.AssertWorkItemTasks(10);
			testHelper.AssertTaskExists("The checkin task should be cancelled because a quality iteration was created, and yet...", 7, "CHG", "CAN");
			testHelper.AssertTaskExists(8, "CDF", "ASN");
			testHelper.AssertTaskExists(9, "SHV", "ASN");
			testHelper.AssertTaskExists("The new checkin task in the quality iteration should be suspended because it is an active checkin type (otherwise it would queue a check-in immediately). And yet...", 10, "CHG", "SUS");
		}

		public void TestFailedShelfWithNonCheckinTaskInDependentWorkflow_ShouldCreateQualityIterationFromTaskInDependentWorkflow()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			var firstWorkflow = testHelper.AddWorkflow("YES", null, null, "INV,ADK,CLS,1", "CDU,ADK,CLS,2", "CDF,ADK,CLS,3");
			testHelper.AddWorkflow("YES", firstWorkflow, "CHB,ADK,ASN,5", "CBC,DNK,CLS,4");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(5);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(8);
			testHelper.AssertTaskExists("The checkin task should be cancelled because a quality iteration should have been created." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 5, "CHB", "CAN");
			testHelper.AssertTaskExists(6, "CDF", "ASN");
			testHelper.AssertTaskExists(7, "CBC", "ASN");
			testHelper.AssertTaskExists(8, "CHB", "SUS");
		}

		public void TestFailedShelfWithDependentWorkflowWitNonCheckinTasksInBothWorkflows_ShouldCreateQualityIterationFromTaskInSecondWorkflow()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			var firstWorkflow = testHelper.AddWorkflow("YES", null, null, "INV,ADK,CLS,1", "CDU,ADK,CLS,2");
			testHelper.AddWorkflow("YES", firstWorkflow, "CH0,ADK,ASN,5", "CDF,ADK,CLS,3", "CBC,DNK,CLS,4");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(5);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(8);
			testHelper.AssertTaskExists("The checkin task should be cancelled because a quality iteration should have been created." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 5, "CH0", "CAN");
			testHelper.AssertTaskExists(6, "CDF", "ASN");
			testHelper.AssertTaskExists(7, "CBC", "ASN");
			testHelper.AssertTaskExists(8, "CHK", "ASN");
		}

		public void TestFailedShelfInQualityIterationWithNoNonCheckinTask_ShouldNotCreateQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);
			testHelper.AddWorkflow("YES", null, "CH0,ADK,ASN,5", "INV,ADK,CLS,1", "CDU,ADK,CLS,2", "CDF,ADK,CLS,3", "CBC,DNK,CLS,4");
			Factory.Save();

			testHelper.AssertWorkItemTasks(5);

			testHelper.CreateQualityIteration();

			testHelper.AssertWorkItemTasks(8);
			testHelper.AssertTaskExists(5, "CH0", "CAN");
			testHelper.AssertTaskExists(6, "CDF", "ASN");
			testHelper.AssertTaskExists(7, "CBC", "ASN");
			testHelper.AssertTaskExists(8, "CHK", "ASN");

			var taskToChange = testHelper.GetTaskBySequenceNumber(6);
			taskToChange.P9_GS_NKAssignedStaffMember = "AE";
			taskToChange.P9_Status = "CLS";
			testHelper.GetTaskBySequenceNumber(7).P9_Status = "CLS";
			testHelper.ShelfTask = testHelper.GetTaskBySequenceNumber(8);
			testHelper.ShelfTask.P9_Type = "CH0";
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(8);
			testHelper.AssertTaskExists("The shelf test task should be suspended because there was no non-checkin task for the same resource in the quality iteration, so no new quality iteration should have been created." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 8, "CH0", "SUS");
		}

		public void TestFailedShelfInQualityIterationWithCodingTask_ShouldCreateQualityIterationFromTaskInQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			testHelper.AddWorkflow("YES", null, "CH0,ADK,ASN,5", "INV,ADK,CLS,1", "CDU,ADK,CLS,2", "CDF,ADK,CLS,3", "CBC,DNK,CLS,4");
			Factory.Save();

			testHelper.AssertWorkItemTasks(5);

			testHelper.CreateQualityIteration();

			testHelper.AssertWorkItemTasks(8);
			testHelper.AssertTaskExists(5, "CH0", "CAN");
			testHelper.AssertTaskExists(6, "CDF", "ASN");
			testHelper.AssertTaskExists(7, "CBC", "ASN");
			testHelper.AssertTaskExists(8, "CHK", "ASN");

			var taskToChange = testHelper.GetTaskBySequenceNumber(6);
			taskToChange.P9_Type = "CDU";
			taskToChange.P9_Status = "CLS";
			testHelper.GetTaskBySequenceNumber(6).P9_Status = "CLS";
			testHelper.ShelfTask = testHelper.GetTaskBySequenceNumber(8);
			testHelper.ShelfTask.P9_Type = "CH0";
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(11);
			testHelper.AssertTaskExists("The checkin task should be cancelled because a quality iteration should have been created." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 8, "CH0", "CAN");
			testHelper.AssertTaskExists(9, "CDU", "ASN");
			testHelper.AssertTaskExists(10, "CBC", "ASN");
			testHelper.AssertTaskExists(11, "CHK", "ASN");
		}

		public void TestFailedShelfInQualityIterationWithCodingTaskInWorkflowDependentOn_ShouldCreateQualityIterationFromTaskInWorkflowDependentOn()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			var firstWorkflow = testHelper.AddWorkflow("YES", null, null, "INV,ADK,CLS,1", "CDU,ADK,CLS,2", "CBC,DNK,CLS,3");
			testHelper.AddWorkflow("YES", firstWorkflow, "CH0,ADK,ASN,6", "CDF,ADK,CLS,4", "CBF,DEA,CLS,5");
			Factory.Save();

			testHelper.AssertWorkItemTasks(6);

			testHelper.CreateQualityIteration();

			testHelper.AssertWorkItemTasks(9);
			testHelper.AssertTaskExists(6, "CH0", "CAN");
			testHelper.AssertTaskExists(7, "CDF", "ASN");
			testHelper.AssertTaskExists(8, "CBF", "ASN");
			testHelper.AssertTaskExists(9, "CHK", "ASN");

			testHelper.ShelfTask = testHelper.GetTaskBySequenceNumber(9);
			testHelper.ShelfTask.P9_Type = "CH0";
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(14);
			testHelper.AssertTaskExists("The checkin task should be cancelled because a quality iteration should have been created." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 9, "CH0", "CAN");
			testHelper.AssertTaskExists(10, "CDU", "ASN");
			testHelper.AssertTaskExists(11, "CBC", "ASN");
			testHelper.AssertTaskExists(12, "CDF", "ASN");
			testHelper.AssertTaskExists(13, "CBF", "ASN");
			testHelper.AssertTaskExists(14, "CHK", "ASN");
		}

		public void TestQualityIterationWithNoDatUser_ShouldNotCreateQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: false);

			testHelper.AddWorkflow("YES", null, "SH0,ADK,ASN,2", "CDF,ADK,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(2);
			testHelper.AssertTaskExists("The shelf test task should be suspended because there was no DAT user found." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, "SH0", "SUS");
		}

		public void TestQualityIterationWithNoIterationReason_ShouldNotCreateQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True, shouldAddQualityIterationReason: false);

			testHelper.AddWorkflow("YES", null, "SH0,ADK,ASN,2", "CDF,ADK,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(2);
			testHelper.AssertTaskExists("The shelf test task should be suspended because there was no SHV Quality Iteration Reason registry item code found." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, "SH0", "SUS");
		}

		[TestDate(2015, 8, 25, 10, 30, 0)]
		public void TestFailedShelf_ShouldCreateQualityIterationWithCorrectUser()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);
			testHelper.WorkItem.WKI_WorkItemNumber = "WI0123";
			testHelper.AddWorkflow("YES", null, "SH0,ADK,ASN,2", "CDF,ADK,CLS,1");
			var userTestPk = Guid.NewGuid();
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = userTestPk });
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			var expectedFormat = @"DAT 25-Aug-15 10:30: Containment Barrier triggered a Quality Iteration - Quality Iteration Reason: SHV - Failed Shelf Test/Checkin

DAT 25-Aug-15 10:30: Your submission to DAT has been rejected. See http://crikey.wtg.zone/TestResults/{0} for details.";

			MasterFilesTestHelper.AssertRtfText("The qcbTask's notes should contain a timestamp that refers to the specified user." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), string.Format(expectedFormat, userTestPk), testHelper.ShelfTask.P9_Notes);
		}

		public void TestGetDatUserLoginName()
		{
			var logger = new LoggerForTest();
			var qualityIterationHelper = new QualityIterationHelper(new QualityIterationInfo(null, @"Shelf Iteration", "SHV", null), Factory, logMessage => logger.Log(LogType.Error, logMessage));
			var loginName = qualityIterationHelper.GetQualityIterationLoginName();

			AssertNull("The name should be null because the DAT user cannot be found.", loginName);
			AssertCollectionContains(logger.LogEntries, x => x == "Could not find a user with GS_Code value of DAT, which is required for quality iterations to be created for failed shelves.");

			logger.ClearLog();
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddDatUser();
			Factory.Save();

			loginName = qualityIterationHelper.GetQualityIterationLoginName();

			Assert("The DAT user has been added, so a login name should have been found by the service task.", !string.IsNullOrEmpty(loginName));
			AssertCollectionNotContains(logger.LogEntries, x => x.Contains("Could not find a user with GS_Code value of DAT, which is required for quality iterations to be created for failed shelves."));
		}

		public void TestIterationReasonPk()
		{
			var logger = new LoggerForTest();
			var qualityIterationHelper = new QualityIterationHelper(new QualityIterationInfo(null, @"Shelf Iteration", "SHV", null), Factory, logMessage => logger.Log(LogType.Error, logMessage));
			MasterFilesTestHelper.ClearIterationReasonsFromRegistry(QualityIterationHelper.FailedQualityIterationRegistryCategoryCode, true);
			var reasonPk = qualityIterationHelper.GetReasonPkForQualityIteration();

			AssertEquals("The service task should have returned an empty PK because SHV has not yet been defined.", ZGuid.Empty, reasonPk);
			AssertCollectionContains(logger.LogEntries, x => x.Contains("Quality Iteration Reasons registry item"));

			logger.ClearLog();
			ProcessedShelfsServiceTaskTestHelper.AddQualityIterationReason("SHV", @"Failed Shelf Test/Checkin");
			reasonPk = qualityIterationHelper.GetReasonPkForQualityIteration();
			var expected = WorkflowDataRegistry.Instance.IterationReasons.Value.GetIterationReason(QualityIterationHelper.FailedQualityIterationRegistryCategoryCode, qualityIterationHelper.QualityIterationReasonCode).PK;

			AssertEquals("The SHV quality iteration reason registry item has been added, so the correct reason PK should have been found by the service task.", expected, reasonPk);
			AssertCollectionNotContains(logger.LogEntries, x => x.Contains("Quality Iteration Reasons registry item"));
		}

		public void TestIterateFromTask_ShouldOnlyPickCodingTaskTypes()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			testHelper.AddWorkflow("YES", null, "CH0,ADK,ASN,3", "CDF,ADK,CLS,1", "INV,ADK,CLS,2");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(3);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(6);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 3, "CH0", "CAN");
			testHelper.AssertTaskExists("IterateFromTask should have picked the last coding task, not the INV task, and yet..." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 4, "CDF", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 5, "INV", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 6, "CHK", "ASN");
		}

		public void TestQualityIterationFromShelf_TasksToIncludeShouldHaveGreaterSequenceNumbersThanTheIterateFromTask()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			//Create a workflow with a quality iteration
			testHelper.SetUpForQualityIterations(ZBool.False);

			var firstWorkflow = testHelper.AddWorkflow("YES", null, "SH0,AGA,ASN,3", "CDF,AGA,CLS,1", "INV,AGA,CLS,2");
			Factory.Save();

			testHelper.AssertWorkItemTasks(3);

			testHelper.CreateQualityIteration();

			testHelper.AssertWorkItemTasks(5);
			testHelper.AssertTaskExists(3, "SH0", "CAN");
			testHelper.AssertTaskExists(4, "COD", "ASN");
			testHelper.AssertTaskExists(5, "SHV", "ASN");

			var taskToChange = testHelper.GetTaskBySequenceNumber(4);
			taskToChange.P9_Status = "CLS";

			testHelper.ShelfTask = testHelper.GetTaskBySequenceNumber(5);
			testHelper.ShelfTask.P9_Type = "SH0";
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(7);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 5, "SH0", "CAN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 6, "COD", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 7, "SHV", "ASN");

			taskToChange = testHelper.GetTaskBySequenceNumber(6);
			taskToChange.P9_Status = "CLS";
			testHelper.ShelfTask = testHelper.GetTaskBySequenceNumber(7);
			testHelper.ShelfTask.P9_Type = "SH0";
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Passed, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			InitialiseAndRunTaskSchedule(ServiceTask);

			//Create a second main workflow dependent on the first one.
			//Create a quality iteration for the second main workflow and assert that it doesn't contain extra tasks from the child workflow.
			testHelper.AddWorkflow("YES", firstWorkflow, "CH0,AGA,ASN,10", "PRV,AGA,CLS,8", "CBF,ADK,CLS,9");
			Factory.Save();

			testHelper.AssertWorkItemTasks(10);
			testHelper.ShelfTask = testHelper.GetTaskBySequenceNumber(10);
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(15);
			testHelper.AssertTaskExists("The checkin task should be cancelled because a quality iteration should have been created." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 10, "CH0", "CAN");
			testHelper.AssertTaskExists("This task should be from the last shelf iteration", 11, "COD", "ASN");
			testHelper.AssertTaskExists(12, "SHV", "ASN");
			testHelper.AssertTaskExists("This task should be a PRV since it's from the second main workflow", 13, "PRV", "ASN");
			testHelper.AssertTaskExists(14, "CBF", "ASN");
			testHelper.AssertTaskExists(15, "CHK", "ASN");
		}

		public void TestUpdateRelatedProcessTask_TaskInformationShouldBeLoggedWhenQualityIterationIsNotCreated()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: false);

			testHelper.AddWorkflow("YES", null, "SH0,ADK,ASN,2", "CDF,ADK,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			var logOutput = serviceLogger.ToString();
			AssertContains("The error shoud be logged", "Error|Could not find a user with GS_Code value of DAT, which is required for quality iterations to be created for failed shelves.", logOutput);
			AssertContains("The task status information should be logged", $"Information|Marking task as suspended : {testHelper.GetTaskBySequenceNumber(2).HumanReadableShortcutName}", logOutput);
		}

		public void TestUpdateRelatedProcessTask_TaskInformationShouldBeLoggedWhenQualityIterationIsCreated()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			testHelper.AddWorkflow("YES", null, "SH0,ADK,ASN,2", "CDF,ADK,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			var logOutput = serviceLogger.ToString();

			AssertContains("The task status information should be logged", $"Information|Marking task as cancelled : {testHelper.GetTaskBySequenceNumber(2).HumanReadableShortcutName}", logOutput);
		}

		public void TestIterationType()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			var workflow = testHelper.AddWorkflow("YES", null, "SH0,ADK,ASN,3", "CDF,ADK,CLS,1", "INV,ADK,CLS,2");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			var query = new ZQuery(ProcessHeaderSchema.FH_ParentId, testHelper.WorkItem.PK);
			query.AddToFilter(ProcessHeaderSchema.PK, SQLComparisonOperator.NotEqual, workflow.PK);
			query.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, DBNull.Value);
			var iteration = Factory.LoadTop1<ProcessHeader>(query);

			AssertEquals(serviceLogger.ToString(), "Silly Hats Only (Shelf Iteration 1)", iteration.FH_CompletionStatement);
		}

		public void TestEligibleIterateFromTaskTypes_ShouldUseValuesFromRegistry()
		{
			var helper = new QualityIterationHelper(null, null, null);
			AssertContainsExactElementsInAnyOrder(new[] { "CDU", "CDF", "COD", "RCD" }, helper.EligibleIterateFromTaskTypes);
		}

		public void TestIteration_WhenCreateNewWorkflowsForQualityIterationsByDefaultRegistryItemEnabled_ShouldCreateNewWorkflowForIteration()
		{
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			var workflow = testHelper.AddWorkflow("YES", null, "CH0,ADK,ASN,3", "CDF,ADK,CLS,1", "INV,ADK,CLS,2");
			AssertEquals(3, workflow.Tasks.Count());

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			var query = new ZQuery(ProcessHeaderSchema.FH_ParentId, testHelper.WorkItem.PK);
			query.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, DBNull.Value);
			var workflowsForJob = Factory.Load<ProcessHeader>(query);

			AssertContainsExactElementsInAnyOrder("A new workflow for the iteration should have been created. SAD!" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), new[] { "Silly Hats Only", "Silly Hats Only (Shelf Iteration 1)" }, workflowsForJob.Select(x => x.FH_CompletionStatement));

			((BusinessObject)workflow).Reload();
			AssertEquals(3, workflow.Tasks.Count());

			var iterationWorkflow = workflow.JobHeader.ProcessHeaders.Cast<ProcessHeader>().Single(x => x.FH_CompletionStatement == "Silly Hats Only (Shelf Iteration 1)");
			AssertEquals(3, iterationWorkflow.Tasks.Count());
		}

		public void TestIteration_WhenCreateNewWorkflowsForQualityIterationsByDefaultRegistryItemDisabled_ShouldShouldUseOriginalWorkflowForIteration()
		{
			WorkflowDataRegistry.Instance.CreateNewWorkflowsForQualityIterationsByDefault.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			var workflow = testHelper.AddWorkflow("YES", null, "CH0,ADK,ASN,3", "CDF,ADK,CLS,1", "INV,ADK,CLS,2");
			AssertEquals(3, workflow.Tasks.Count());

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			var query = new ZQuery(ProcessHeaderSchema.FH_ParentId, testHelper.WorkItem.PK);
			query.AddToFilter(ProcessHeaderSchema.FH_FH_ParentHeader, SQLComparisonOperator.NotEqual, DBNull.Value);
			var workflowsForJob = Factory.Load<ProcessHeader>(query);

			AssertContainsExactElementsInAnyOrder("The same workflow should have been used for the iteration. SAD!" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), new[] { "Silly Hats Only" }, workflowsForJob.Select(x => x.FH_CompletionStatement));

			((BusinessObject)workflow).Reload();
			AssertEquals(6, workflow.Tasks.Count());
		}

		[TestDate(2021, 9, 17)]
		public void TestIteration_ShouldSelectContainmentBarrierTaskAssignedUser_ForResourceUnderReview_PrefillEnabled()
		{
			WorkflowDataRegistry.Instance.PrefillResourceUnderReview.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			testHelper.AddUser("ADK", "Adam");
			testHelper.AddUser("AE", "Alex");

			var workflow = testHelper.AddWorkflow("YES", null, "SH0,ADK,ASN,3", "CDF,ADK,CLS,1", "INV,AE,ASN,2");
			AssertEquals(3, workflow.Tasks.Count());

			var bigTask = workflow.Tasks.Single(x => x.P9_Type == "INV");
			bigTask.P9_Status = "WRK";

			TestDateAttribute.AddHours(1);
			bigTask.P9_Status = "CLS";

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			var link = Factory.LoadTop1<IProcessTaskIterationLink>(new ZQuery());
			AssertEquals("The staff assigned to the shelf task should always be the resource under review for DAT, even when prefill is enabled." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), "ADK", link.P9I_GS_NKResourceUnderReview);
		}

		[TestDate(2021, 9, 17)]
		public void TestIteration_ShouldSelectContainmentBarrierTaskAssignedUser_ForResourceUnderReview_PrefillDisabled()
		{
			WorkflowDataRegistry.Instance.PrefillResourceUnderReview.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);

			testHelper.AddUser("ADK", "Adam");
			testHelper.AddUser("AE", "Alex");

			var workflow = testHelper.AddWorkflow("YES", null, "SH0,ADK,ASN,3", "CDF,ADK,CLS,1", "INV,AE,ASN,2");
			AssertEquals(3, workflow.Tasks.Count());

			var bigTask = workflow.Tasks.Single(x => x.P9_Type == "INV");
			bigTask.P9_Status = "WRK";

			TestDateAttribute.AddHours(1);
			bigTask.P9_Status = "CLS";

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			var link = Factory.LoadTop1<IProcessTaskIterationLink>(new ZQuery());
			AssertEquals("The staff assigned to the shelf task should always be the resource under review for DAT, even when prefill is disabled." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), "ADK", link.P9I_GS_NKResourceUnderReview);
		}

		public void TestFailedShelfForNonCheckinSubmissions_ShouldCreateQualityIterationWithResolveSubmissionTask_SH0()
		{
			TestFailedShelfForNonCheckInSubmissions_ShouldCreateQualityIterationWithResolveSubmissionTask("SH0", "SHV");
		}

		public void TestFailedShelfForNonCheckinSubmissions_ShouldCreateQualityIterationWithResolveSubmissionTask_AS0()
		{
			TestFailedShelfForNonCheckInSubmissions_ShouldCreateQualityIterationWithResolveSubmissionTask("AS0", "ASP");
		}

		public void TestFailedShelfForNonCheckinSubmissions_ShouldCreateQualityIterationWithResolveSubmissionTask_UA0()
		{
			TestFailedShelfForNonCheckInSubmissions_ShouldCreateQualityIterationWithResolveSubmissionTask("UA0", "UAT");
		}

		void TestFailedShelfForNonCheckInSubmissions_ShouldCreateQualityIterationWithResolveSubmissionTask(string shelfCode, string newShelfCode)
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);

			testHelper.AddWorkflow("YES", null, $"{shelfCode},ADK,ASN,2", "CDF,ADK,CLS,1", "CHK,ADK,ASN,3");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(3);

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(5);
			testHelper.AssertTaskExists(1, "CDF", "CLS");
			testHelper.AssertTaskExists(2, shelfCode, "CAN");
			testHelper.AssertTaskExists(3, "COD", "ASN");
			testHelper.AssertTaskExists(4, newShelfCode, "ASN");
			testHelper.AssertTaskExists(5, "CHK", "ASN");

			var resolveSubmissionFailureTask = testHelper.GetTaskBySequenceNumber(3);
			testHelper.AssertTaskDetails(resolveSubmissionFailureTask, "COD", "ASN", "Resolve submission failure", "ADK", 20, null);
		}

		#endregion

		#region Other

		public void TestNotificationEmail()
		{
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\s_datservice", "Something 1", "", null) { Status = "REJ", NotificationEmail = "team1@wisetechglobal.com" });
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\s_datservice", "Something 2", "", null) { Status = ShelfStatuses.CheckedIn, NotificationEmail = "team2@wisetechglobal.com" });
			Factory.Save();
			new AutomaticProcessRegistryBusinessObject { NextRunDateTime = ZDateTime.Now.AddHours(-1) };
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				"Failure: Something 1 has been rejected",
				"Success: Something 2 has completed",
			},
			Env.OutgoingMailManager.EmailsCreated.Select(o => o.Subject));
			var sorted = Env.OutgoingMailManager.EmailsCreated.OrderBy(mail => mail.Subject).ToArray();
			AssertEquals(serviceLogger.ToString(), "team1@wisetechglobal.com", sorted[0].Recipients[0].Email);
			AssertEquals(serviceLogger.ToString(), "team2@wisetechglobal.com", sorted[1].Recipients[0].Email);
		}

		public void TestFailedWithUATbuildInQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);
			testHelper.AddWorkflow("YES", null, "CH0,LCD,ASN,10", "CDF,LCD,CLS,1", "SH0,LCD,CLS,3", "UA0,LCD,CLS,5");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(4);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(8);

			testHelper.AssertTaskExists(serviceLogger.ToString(), 11, "CDF", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 12, "SHV", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 13, "UAT", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 14, "CHK", "ASN");
		}

		public void TestFailedWithAspectOnlyBuildInQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);
			testHelper.AddWorkflow("YES", null, "CH0,LCD,ASN,10", "CDF,LCD,CLS,1", "SH0,LCD,CLS,3", "AS0,LCD,CLS,5");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(4);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(8);

			testHelper.AssertTaskExists(serviceLogger.ToString(), 11, "CDF", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 12, "SHV", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 13, "ASP", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 14, "CHK", "ASN");
		}

		public void TestFailedAspectOnlyBuildWithQualityIteration()
		{
			//A rejected aspect only build probably means a build failure
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);
			testHelper.AddWorkflow("YES", null, "AS0,LCD,ASN,5", "CDF,LCD,CLS,1", "SH0,LCD,CLS,3", "CHK,LCD,ASN,20");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(4);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(6);

			testHelper.AssertTaskExists(serviceLogger.ToString(), 1, "CDF", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 3, "SH0", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 5, "AS0", "CAN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 6, "COD", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 7, "ASP", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 22, "CHK", "ASN");
		}

		public void TestFailedAspectOnlyBuildWithoutQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null, "AS0,LCD,ASN,5", "CDF,LCD,CLS,1", "SH0,LCD,CLS,3", "CHK,LCD,ASN,20");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(4);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(4);

			testHelper.AssertTaskExists(serviceLogger.ToString(), 1, "CDF", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 3, "SH0", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 5, "AS0", "SUS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 20, "CHK", "ASN");
		}

		public void TestFailedCheckinShouldNotCopyAspectTaskInIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);

			testHelper.AddWorkflow("NS", null, "CH0,LCD,ASN,100", "CDU,LCD,CLS,10", "CDF,LCD,CLS,20", "SH0,LCD,CLS,30", "CBC,RV1,CLS,50", "CBS,RV2,CLS,99");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask) { Status = ShelfStatuses.Rejected, UserHeaderPK = Guid.NewGuid() });
			Factory.Save();

			testHelper.AssertWorkItemTasks(6);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertTaskExists(serviceLogger.ToString(), 10, "CDU", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 20, "CDF", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 30, "SH0", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 50, "CBC", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 99, "CBS", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 100, "CH0", "CAN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 101, "CDF", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 102, "SHV", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 103, "CBC", "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 104, "CHK", "ASN");
			testHelper.AssertWorkItemTasks(10);
		}

		public void TestVerboseLoggingHelpsTrackProcessingSubmission()
		{
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(EDIShelvesetInfo.ActionTypes.ShelfCheckin, ShelfStatuses.CheckedIn, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1212?_a=overview", "GitTitle");
			var logger = InitialiseAndRunTaskSchedule(ServiceTask);
			var logOutput = logger.ToString();

			AssertEquals(ShelfStatuses.CheckedInAndNotified, shelfset.Status);
			AssertContains($"Information|Processing SCH submission 'GitTitle' from task: {serviceTask.processTasks[0].P9_TaskID}", logOutput);
			AssertContains($"Information|Marking task as checked in : {serviceTask.processTasks[0].P9_TaskID}", logOutput);
			AssertContains("Debug|ASSESS review requirements not found for submission: 'GitTitle', Related task:", logOutput);
			AssertContains("Debug|Email sent to blah@blah.com for submission 'GitTitle'", logOutput);
			AssertContains("Debug|Marked related incidents as awaiting auto deploy for submission 'GitTitle'", logOutput);
			AssertContains($"Information|Updated Dat Status for submission 'GitTitle' from task: {serviceTask.processTasks[0].P9_TaskID}", logOutput);

			logger.ClearLog();
			shelfset.Status = ShelfStatuses.CheckedIn;
			logger = InitialiseAndRunTaskSchedule(ServiceTask);
			logOutput = logger.ToString();
			AssertContains($"Information|Already Processed SCH submission 'GitTitle' from task: {serviceTask.processTasks[0].P9_TaskID}", logOutput);
			AssertContains($"Information|Updated Dat Status for submission 'GitTitle' from task: {serviceTask.processTasks[0].P9_TaskID}", logOutput);
			AssertNotContains("Debug|", logOutput);
		}

		public void TestVerboseLoggingHelpsTrackProcessingSubmissionWithAssess()
		{
			var shelfset = ConfigureShelfsetForServiceTaskWithGitPull(EDIShelvesetInfo.ActionTypes.ShelfsetTest, ShelfStatuses.Passed, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1212?_a=overview", "GitTitle", "SubmissionTitle");

			Factory.Save();

			var aspectReview1 = AspectReviewSummary.ForAssessAspect(Guid.NewGuid(), aspectPK1, "AspectName");
			SetAsCompleted(shelfset.RelatedProcessTask.AssignedStaffMember, aspectPK1, hasPassed: false);

			ServiceTask.shelvesets[0].AspectReviews.Add(aspectReview1);

			var logger = InitialiseAndRunTaskSchedule(ServiceTask);
			var logOutput = logger.ToString();

			AssertEquals(ShelfStatuses.PassedAndNotified, shelfset.Status);
			AssertContains($"Information|Processing SHV submission 'SubmissionTitle' from task: {serviceTask.processTasks[0].P9_TaskID}", logOutput);
			AssertContains($"Information|Marking task as passed : {serviceTask.processTasks[0].P9_TaskID}", logOutput);
			AssertContains("Debug|Processed ASSESS info for submission 'SubmissionTitle'", logOutput);
			AssertContains("Debug|Email sent to blah@blah.com for submission 'SubmissionTitle'", logOutput);
			AssertContains($"Information|Updated Dat Status for submission 'SubmissionTitle' from task: {serviceTask.processTasks[0].P9_TaskID}", logOutput);
		}

		public void TestVerboseLoggingHelpsTrackProcessingSubmissionWithUATCombinedBuild()
		{
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("DAT", "UAT Combined Name", EDIShelvesetInfo.ActionTypes.UATCombinedBuild, null) { UserHeaderPK = Guid.NewGuid(), Status = ShelfStatuses.Passed });
			Factory.Save();
			var logger = InitialiseAndRunTaskSchedule(ServiceTask);
			var logOutput = logger.ToString();
			AssertContains("Information|Processing UCB submission 'UAT Combined Name' from task: Unknown RelatedProcessTask", logOutput);
			AssertContains("Debug|ASSESS review requirements not found", logOutput);
			AssertContains("Debug|No email sent, recipient: '', shouldNotify: 'True' for submission 'UAT Combined Name'", logOutput);
			AssertContains("Information|Updated Dat Status for submission 'UAT Combined Name' from task: Unknown RelatedProcessTask", logOutput);

			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("DAT", "Another UAT Combined Name", EDIShelvesetInfo.ActionTypes.UATCombinedBuild, null) { UserHeaderPK = Guid.NewGuid(), Status = ShelfStatuses.Passed, NotificationEmail = "Someone@somewhere.com" });
			Factory.Save();
			logger.ClearLog();
			logger = InitialiseAndRunTaskSchedule(ServiceTask);
			logOutput = logger.ToString();
			AssertContains("Information|Processing UCB submission 'Another UAT Combined Name' from task: Unknown RelatedProcessTask", logOutput);
			AssertContains("Debug|ASSESS review requirements not found", logOutput);
			AssertContains("Debug|Email sent to Someone@somewhere.com for submission 'Another UAT Combined Name'", logOutput);
			AssertContains("Information|Updated Dat Status for submission 'Another UAT Combined Name' from task: Unknown RelatedProcessTask", logOutput);
		}

		public void TestVerboseLoggingHelpsTrackProcessingSubmissionWithUATCombinedChild()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "lee+ucc@wtg.com";
			staff.GS_LoginName = "lee.coady";
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\lee.coady", "ChildShelfName", EDIShelvesetInfo.ActionTypes.UATCombinedChild, null) { UserHeaderPK = Guid.NewGuid(), Status = ShelfStatuses.Passed });
			Factory.Save();
			var logger = InitialiseAndRunTaskSchedule(ServiceTask);
			var logOutput = logger.ToString();
			AssertContains("Information|Processing UCC submission 'ChildShelfName' from task: Unknown RelatedProcessTask", logOutput);
			AssertContains("Debug|ASSESS review requirements not found for submission: 'ChildShelfName', Related task: Task Not Found", logOutput);
			AssertContains("Debug|No email sent, recipient: 'lee+ucc@wtg.com', shouldNotify: 'False' for submission 'ChildShelfName'", logOutput);
			AssertContains("Information|Updated Dat Status for submission 'ChildShelfName' from task: Unknown RelatedProcessTask", logOutput);
		}

		public void TestVerboseLoggingHelpsTrackProcessingSubmissionWithUAT()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "TST";
			staff.GS_EmailAddress = "bret@wtg.com";
			var workItem = Factory.NewWithValidTestData<NewWorkItem>();
			ServiceTask.processTasks.Add((WorkItemProcessTask)workItem.WorkflowItems.Tasks.AddNew());
			ServiceTask.processTasks[0].P9_GS_NKAssignedStaffMember = staff.GS_Code;
			ServiceTask.processTasks[0].P9_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			ServiceTask.processTasks[0].P9_ParentID = workItem.PK;
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo("Bret", "WI01010101", "UAB", ServiceTask.processTasks[0]) { UserHeaderPK = Guid.NewGuid(), Status = ShelfStatuses.Passed });
			Factory.Save();
			new AutomaticProcessRegistryBusinessObject { NextRunDateTime = ZDateTime.Now.AddHours(-1) };
			var logger = InitialiseAndRunTaskSchedule(ServiceTask);
			var logOutput = logger.ToString();
			AssertContains($"Information|Processing UAB submission 'WI01010101' from task: {serviceTask.processTasks[0].P9_TaskID}", logOutput);
			AssertContains($"Information|Marking task as passed : {serviceTask.processTasks[0].P9_TaskID}", logOutput);
			AssertContains("Debug|ASSESS review requirements not found for submission: 'WI01010101', Related task:", logOutput);
			AssertContains("Debug|Email sent to bret@wtg.com for submission 'WI01010101'", logOutput);
			AssertContains($"Information|Updated Dat Status for submission 'WI01010101' from task: {serviceTask.processTasks[0].P9_TaskID}", logOutput);
		}

		#endregion

		#region Skill Aspects

		public void TestSkillAspect_ShelfPass()
		{
			var learningTaskType = SetUpLearningTaskType();
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			var topWorkflow = testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,2");

			AddShelfWithAssessToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Passed, [aspectPK1]);

			testHelperWithAspectDat.AssertWorkItemTasks(1);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "ASN", topWorkflow.PK);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 2, "SH0", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 2, learningTaskType, "ASN", topWorkflow.PK);

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == learningTaskType).ToArray();

			AssertEquals("(Optional) Awesome Skill", learningTasks[0].P9_Description);
			AssertEquals("A.G", learningTasks[0].P9_GS_NKAssignedStaffMember);
		}

		public void TestSkillAspect_LearningTaskNotCreatedForSkilledCoder()
		{
			var staff = MasterFilesTestHelper.CreateStaff(Factory, "A.G", "Anton");
			SetUpLearningTaskType();
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,1", "CBC,JMK,ASN,2");

			AddShelfWithAssessToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Passed, [aspectPK1]);

			testHelperWithAspectDat.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertTaskExists(1, "SH0", "ASN");
			testHelperWithAspectDat.AssertTaskExists(2, "CBC", "ASN");

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));

			SetAsCompleted(staff, aspectPK1);
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 1, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 2, "CBC", "ASN");

			AssertEquals(1, reviewTask.SkillsPivots.Count);
		}

		public void TestSkillAspect_LearningTaskCreatedForPartiallySkilledCoder()
		{
			var staff = MasterFilesTestHelper.CreateStaff(Factory, "A.G", "Anton");
			var learningTaskType = SetUpLearningTaskType();
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,1", "CBC,JMK,ASN,2");

			AddShelfWithAssessToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Passed, new[] { aspectPK1, aspectPK2 });

			testHelperWithAspectDat.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertTaskExists(1, "SH0", "ASN");
			testHelperWithAspectDat.AssertTaskExists(2, "CBC", "ASN");
			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));

			SetAsCompleted(staff, aspectPK1);
			SetAsCompleted(staff, aspectPK2, false);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(3);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 1, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 2, "CBC", "ASN");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 2, learningTaskType, "ASN");

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == learningTaskType).ToArray();
			AssertEquals("(Optional) New Skill", learningTasks[0].P9_Description);
			AssertEquals("A.G", learningTasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals(2, reviewTask.SkillsPivots.Count);
		}

		public void TestSkillAspect_BothAspectAndSkill()
		{
			var learningTaskType = SetUpLearningTaskType();
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,A.G,ASN,10", "CBC,JMK,CLS,3", "CDF,A.G,CLS,1");

			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelperWithAspectDat.WorkItem.WKI_WorkItemNumber, "", testHelperWithAspectDat.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};

			var aspectReview1 = AspectReviewSummary.ForAssessAspect(Guid.NewGuid(), aspectPK1, "AspectAssess");
			var aspectReview2 = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectCapability");

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			Factory.Save();

			shelvesetWithAspectReview.AspectReviews.Add(aspectReview1);
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview2);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview);

			testHelperWithAspectDat.AssertWorkItemTasks(3);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(3, "CBC", "CLS");
			testHelperWithAspectDat.AssertTaskExists(10, "CH0", "ASN");

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(5);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 3, "CBC", "CLS");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 9, WorkItemProcessTask.AspectReviewTaskType, "ASN");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 9, "CBC", "ASN");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 10, "CH0", "SUS");

			AssertEquals("Review is closed so no skills added", 0, reviewTask.SkillsPivots.Count);
		}

		public void TestSkillAspect_ForTestRun_SubmissionRejected()
		{
			RunSkillAspectForRejectedSubmission(EDITaskTypes.TaskActiveShelfTest, ShelfStatuses.Rejected, shouldCreateLearningTask: true);
		}

		public void TestSkillAspect_ForCheckin_SubmissionRejected()
		{
			RunSkillAspectForRejectedSubmission(EDITaskTypes.TaskActiveCheckin, ShelfStatuses.Rejected, shouldCreateLearningTask: false);
		}

		public void TestSkillAspect_ForCheckin_SubmissionRejectedForPendingAspectData()
		{
			RunSkillAspectForRejectedSubmission(EDITaskTypes.TaskActiveCheckin, ShelfStatuses.RejectedForPendingAspectData, shouldCreateLearningTask: false);
		}

		void RunSkillAspectForRejectedSubmission(string submissionTaskType, string rejectionStatus, bool shouldCreateLearningTask)
		{
			var learningTaskType = SetUpLearningTaskType();
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			var topWorkflow = testHelperWithAspectDat.AddWorkflow("YES", null, submissionTaskType + ",A.G,ASN,2");

			AddShelfWithAssessToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, rejectionStatus, [aspectPK1]);

			testHelperWithAspectDat.AssertWorkItemTasks(1);
			testHelperWithAspectDat.AssertTaskExists(2, submissionTaskType, "ASN", topWorkflow.PK);

			Factory.Save();
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			testHelperWithAspectDat.RefreshWorkItemTasks();
			testHelperWithAspectDat.AssertTaskExists(2, submissionTaskType, ProcessTaskStatusCodeList.Codes.Suspended, topWorkflow.PK);

			var learningTask = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == learningTaskType).SingleOrDefault();

			if (shouldCreateLearningTask)
			{
				AssertNotNull(serviceLogger.ToString(), learningTask);
				AssertEquals("(Optional) Awesome Skill", learningTask.P9_Description);
				AssertEquals("A.G", learningTask.P9_GS_NKAssignedStaffMember);
				AssertEquals(ProcessTaskStatusCodeList.Codes.Assigned, learningTask.P9_Status);
			}
			else
			{
				AssertNull(serviceLogger.ToString(), learningTask);
			}
		}

		public void TestSkillAspect_ShelfPassWithReviewAndCheckin()
		{
			var learningTaskType = SetUpLearningTaskType();
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,2", "CDF,A.G,CLS,1", "CBC,REV,ASN,3", "CHK,A.G,ASN,4");

			AddShelfWithAssessToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Passed, [aspectPK1]);

			testHelperWithAspectDat.AssertWorkItemTasks(4);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "ASN");
			testHelperWithAspectDat.AssertTaskExists(3, "CBC", "ASN");
			testHelperWithAspectDat.AssertTaskExists(4, "CHK", "ASN");

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type != WorkItemProcessTask.AspectReviewTaskType && t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(5);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 2, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 3, "CBC", "ASN");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 3, learningTaskType, "ASN");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 4, "CHK", "ASN");

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == learningTaskType).ToArray();

			AssertEquals(1, reviewTask.SkillsPivots.Count);
			AssertEquals(aspectPK1, reviewTask.SkillsPivots[0].P9S_Aspect);

			AssertEquals("(Optional) Awesome Skill", learningTasks[0].P9_Description);
			AssertEquals("A.G", learningTasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals(reviewTask.P9_Sequence, learningTasks[0].P9_Sequence);
		}

		public void TestSkillAspect_NoLearningTaskCreatedForSuccessfulCheckin()
		{
			var learningTaskType = SetUpLearningTaskType();

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,A.G,ASN,4", "CDF,A.G,CLS,1", "SH0,A.G,CLS,2", "CBC,REV,CLS,3");

			AddShelfWithAssessToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.CheckedIn, [aspectPK1]);

			testHelperWithAspectDat.AssertWorkItemTasks(4);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(3, "CBC", "CLS");
			testHelperWithAspectDat.AssertTaskExists(4, "CH0", "ASN");
			var tasksBefore = testHelperWithAspectDat.WorkItemTasks;

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(4);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 1, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 2, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 3, "CBC", "CLS");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 4, "CH0", "CLS");
		}

		public void TestSkillAspect_CheckinRejectedNoReviewDoesNotCreateLearningTask()
		{
			var learningTaskType = SetUpLearningTaskType();
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,A.G,ASN,5", "CDU,A.G,CLS,1", "CDF,A.G,CLS,2", "SH0,A.G,CLS,3", "CBC,REV,CLS,4");

			AddShelfWithAssessToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.RejectedForPendingAspectData, [aspectPK1]);

			testHelperWithAspectDat.AssertWorkItemTasks(5);
			testHelperWithAspectDat.AssertTaskExists(1, "CDU", "CLS");
			testHelperWithAspectDat.AssertTaskExists(2, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(3, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(4, "CBC", "CLS");
			testHelperWithAspectDat.AssertTaskExists(5, "CH0", "ASN");
			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(6);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 1, "CDU", "CLS");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 2, "CDF", "CLS");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 3, "SH0", "CLS");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 4, "CBC", "CLS");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 4, "CBC", "ASN");
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 5, "CH0", "SUS");

			AssertEquals("no skills should be added to the previously closed review task", 0, reviewTask.SkillsPivots.Count);
		}

		public void TestSkillAspect_PassedAspectOnlyBuild()
		{
			var learningTaskType = SetUpLearningTaskType();
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null, "AS0,A.G,ASN,5", "CDF,A.G,CLS,1", "SH0,A.G,CLS,3", "CHK,A.G,ASN,20");
			AddShelfWithAssessToServiceTaskShelvsetList(testHelper.ShelfTask, ShelfStatuses.Passed, [aspectPK1]);

			Factory.Save();

			testHelper.AssertWorkItemTasks(4);
			testHelper.AssertTaskExists(1, "CDF", "CLS");
			testHelper.AssertTaskExists(3, "SH0", "CLS");
			testHelper.AssertTaskExists(5, "AS0", "ASN");
			testHelper.AssertTaskExists(20, "CHK", "ASN");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(5);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 1, "CDF", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 3, "SH0", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 5, "AS0", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 5, learningTaskType, "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 20, "CHK", "ASN");
		}

		public void TestSkillAspect_FailedAspectOnlyBuildWithoutQualityIteration()
		{
			var learningTaskType = SetUpLearningTaskType();
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null, "AS0,A.G,ASN,5", "CDF,A.G,CLS,1", "SH0,A.G,CLS,3", "CHK,A.G,ASN,20");
			AddShelfWithAssessToServiceTaskShelvsetList(testHelper.ShelfTask, ShelfStatuses.Rejected, [aspectPK1]);

			Factory.Save();

			testHelper.AssertWorkItemTasks(4);
			testHelper.AssertTaskExists(1, "CDF", "CLS");
			testHelper.AssertTaskExists(3, "SH0", "CLS");
			testHelper.AssertTaskExists(5, "AS0", "ASN");
			testHelper.AssertTaskExists(20, "CHK", "ASN");

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(5);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 1, "CDF", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 3, "SH0", "CLS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 4, learningTaskType, "ASN");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 5, "AS0", "SUS");
			testHelper.AssertTaskExists(serviceLogger.ToString(), 20, "CHK", "ASN");
		}

		public void TestSkillAspect_FailedAspectOnlyBuildWithQualityIteration()
		{
			var learningTaskType = SetUpLearningTaskType();
			//A rejected aspect only build probably means a build failure (but what if release build fails and debug build records aspect data?
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);
			var topWorkflow = testHelper.AddWorkflow("YES", null, "AS0,A.G,ASN,5", "CDF,A.G,CLS,1", "SH0,A.G,CLS,3", "CHK,A.G,ASN,20");

			AddShelfWithAssessToServiceTaskShelvsetList(testHelper.ShelfTask, ShelfStatuses.Rejected, [aspectPK1]);

			testHelper.AssertWorkItemTasks(4);
			testHelper.AssertTaskExists(1, "CDF", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(3, "SH0", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(5, "AS0", "ASN", topWorkflow.PK);
			testHelper.AssertTaskExists(20, "CHK", "ASN", topWorkflow.PK);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(7);
			var newCoding = testHelper.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "COD" && t.P9_Status == "ASN");
			var iterationHeader = newCoding.P9_FH_ProcessHeader;

			testHelper.AssertTaskExists(serviceLogger.ToString(), 1, "CDF", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 3, "SH0", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 5, "AS0", "CAN", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 5, learningTaskType, "ASN", iterationHeader);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 6, "COD", "ASN", iterationHeader);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 7, "ASP", "ASN", iterationHeader);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 22, "CHK", "ASN", topWorkflow.PK);
		}

		public void TestSkillAspect_CheckinRejectedWithReviewInIteration()
		{
			var learningTaskType = SetUpLearningTaskType();
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True);
			var topWorkflow = testHelper.AddWorkflow("YES", null, "CH0,LCD,ASN,50", "CDU,LCD,CLS,10", "CDF,LCD,CLS,20", "SH0,LCD,CLS,30", "CBC,REV,CLS,40");
			testHelper.ShelfTask.P9_Description = "Shelf Checkin Task";
			var isQCBTask = testHelper.ShelfTask.IsQualityContainmentBarrierTask();
			AddShelfWithAssessToServiceTaskShelvsetList(testHelper.ShelfTask, ShelfStatuses.RejectedForPendingAspectData, [aspectPK1]);

			testHelper.AssertWorkItemTasks(5);
			testHelper.AssertTaskExists(10, "CDU", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(20, "CDF", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(30, "SH0", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(40, "CBC", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(50, "CH0", "ASN", topWorkflow.PK);
			var oldReviewTask = testHelper.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(6);

			testHelper.AssertTaskExists(serviceLogger.ToString(), 10, "CDU", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 20, "CDF", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 30, "SH0", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 40, "CBC", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 49, "CBC", "ASN", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 50, "CH0", "SUS", topWorkflow.PK);

			AssertEquals("no skills should be added to the previously closed review task", 0, oldReviewTask.SkillsPivots.Count);
			var newReviewTask = testHelper.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType) && t.P9_Status == "ASN");
			AssertEquals(1, newReviewTask.SkillsPivots.Count);
			AssertEquals(aspectPK1, newReviewTask.SkillsPivots[0].P9S_Aspect);
		}

		public void TestSkillAspect_ShelfRejectedWithReviewAndCheckin_Iteration()
		{
			var learningTaskType = SetUpLearningTaskType();

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False);
			var workflow = testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,LCD,ASN,2", "CDF,LCD,CLS,1", "CBC,REV,ASN,3");
			testHelperWithAspectDat.ShelfTask.P9_Description = "This Shelf Will Be REJ";

			AddShelfWithAssessToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Rejected, [aspectPK1]);

			testHelperWithAspectDat.AssertWorkItemTasks(3);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS", workflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "ASN", workflow.PK);
			testHelperWithAspectDat.AssertTaskExists(3, "CBC", "ASN", workflow.PK);

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type != WorkItemProcessTask.AspectReviewTaskType && t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));
			reviewTask.P9_Description = "Code Review Of Functionality";

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(6);
			var newCoding = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "COD" && t.P9_Status == "ASN");
			var iterationHeader = newCoding.P9_FH_ProcessHeader;
			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == learningTaskType).ToArray();

			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 1, "CDF", "CLS", workflow.PK);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 2, "SH0", "CAN", workflow.PK);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 2, learningTaskType, "ASN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 3, "COD", "ASN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 4, "SHV", "ASN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 5, "CBC", "ASN", workflow.PK);

			AssertNotNull(reviewTask);
			AssertEquals(1, reviewTask.SkillsPivots.Count);
			AssertEquals(aspectPK1, reviewTask.SkillsPivots[0].P9S_Aspect);

			AssertEquals("(Optional) Awesome Skill", learningTasks[0].P9_Description);
		}

		public void TestSkillAspect_ShelfPassedInsideOfIteration()
		{
			var learningTaskType = SetUpLearningTaskType();
			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False);
			var topWorkflow = testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,A.G,ASN,2", "CDF,A.G,CLS,1", "CBC,JMK,ASN,3");
			Factory.Save();

			testHelperWithAspectDat.CreateQualityIteration();

			testHelperWithAspectDat.AssertWorkItemTasks(5);
			var newShelf = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "SHV");
			var iterationWorkflow = newShelf.P9_FH_ProcessHeader;
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(3, "COD", "ASN", iterationWorkflow);
			testHelperWithAspectDat.AssertTaskExists(4, "SHV", "ASN", iterationWorkflow);
			testHelperWithAspectDat.AssertTaskExists(5, "CBC", "ASN", topWorkflow.PK);

			newShelf.P9_Type = "SH0";
			AddShelfWithAssessToServiceTaskShelvsetList(newShelf, ShelfStatuses.Passed, [aspectPK1]);
			testHelperWithAspectDat.AssertWorkItemTasks(5);
			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(6);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 1, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 2, "SH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 3, "COD", "ASN", iterationWorkflow);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 4, "SH0", "CLS", iterationWorkflow);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 5, "CBC", "ASN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 5, learningTaskType, "ASN", topWorkflow.PK);

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == learningTaskType).ToArray();
			AssertEquals(1, learningTasks.Length);
			AssertNotNull(reviewTask);
			AssertEquals(1, reviewTask.SkillsPivots.Count);
			AssertEquals(true, learningTasks[0].IsCurrent);
			AssertEquals(aspectPK1, reviewTask.SkillsPivots[0].P9S_Aspect);
			AssertEquals("(Optional) Awesome Skill", learningTasks[0].P9_Description);
		}

		public void TestSkillAspect_ShelfRejectedInsideOfIteration()
		{
			var learningTaskType = SetUpLearningTaskType();

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False);
			var topWorkflow = testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,COD,ASN,2", "CDF,COD,CLS,1", "CBC,REV,ASN,3");
			Factory.Save();

			testHelperWithAspectDat.CreateQualityIteration();
			testHelperWithAspectDat.RefreshWorkItemTasks();

			var newCoding = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "COD" && t.P9_Status == "ASN");
			var firstIterationHeader = newCoding.P9_FH_ProcessHeader;
			newCoding.P9_Status = "CLS";
			var newShelf = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "SHV");
			testHelperWithAspectDat.ShelfTask = newShelf;
			newShelf.P9_Type = "SH0";
			newShelf.P9_Description = "This Shelf Will Be REJ";

			AddShelfWithAssessToServiceTaskShelvsetList(newShelf, ShelfStatuses.Rejected, [aspectPK1]);

			testHelperWithAspectDat.AssertWorkItemTasks(5);
			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, "SH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(3, "COD", "CLS", firstIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(4, "SH0", "ASN", firstIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(5, "CBC", "ASN", topWorkflow.PK);

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type != WorkItemProcessTask.AspectReviewTaskType && t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(8);

			var codingInSecondIteration = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "COD" && t.P9_Status == "ASN");
			var secondIterationHeader = codingInSecondIteration.P9_FH_ProcessHeader;

			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 1, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 2, "SH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 3, "COD", "CLS", firstIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 4, "SH0", "CAN", firstIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 4, learningTaskType, "ASN", secondIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 5, "COD", "ASN", secondIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 6, "SHV", "ASN", secondIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 7, "CBC", "ASN", topWorkflow.PK);

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == learningTaskType).ToArray();

			AssertEquals(aspectPK1, reviewTask.SkillsPivots[0].P9S_Aspect);

			AssertEquals("(Optional) Awesome Skill", learningTasks[0].P9_Description);
		}

		public void TestSkillAspect_LearningTaskShouldNotBeCopiedInIteration()
		{
			var learningTaskType = SetUpLearningTaskType();

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False);
			var topWorkflow = testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,COD,ASN,3", "CDF,COD,CLS,1", learningTaskType + ",COD,CLS,2", "CH1,REV,ASN,4");
			Factory.Save();

			testHelperWithAspectDat.CreateQualityIteration();
			testHelperWithAspectDat.AssertWorkItemTasks(6);

			var newCoding = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "CDF" && t.P9_Status == "ASN");
			var firstIterationHeader = newCoding.P9_FH_ProcessHeader;

			testHelperWithAspectDat.AssertTaskExists(1, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(2, learningTaskType, "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(3, "CH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(4, "CDF", "ASN", firstIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(5, "CHK", "ASN", firstIterationHeader);
			testHelperWithAspectDat.AssertTaskExists(6, "CH1", "ASN", topWorkflow.PK);
		}

		public void TestSkillAspect_LearningTaskShouldNotBeCopiedInIteration2()
		{
			var staff = MasterFilesTestHelper.CreateStaff(Factory, "COD", "Coder");
			var learningTaskType = SetUpLearningTaskType();

			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);
			var topWorkflow = testHelper.AddWorkflow("YES", null, "SH0,COD,ASN,3", "CDF,COD,CLS,1", learningTaskType + ",COD,CLS,2", "CBC,REV,ASN,4");
			testHelper.ShelfTask.P9_Description = "This Shelf Will Be REJ";
			Factory.Save();
			AddShelfWithAssessToServiceTaskShelvsetList(testHelper.ShelfTask, ShelfStatuses.Rejected, [aspectPK1]);
			SetAsCompleted(staff, aspectPK1);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.RefreshWorkItemTasks();
			var newCoding = testHelper.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "COD" && t.P9_Status == "ASN");
			var firstIterationHeader = newCoding.P9_FH_ProcessHeader;

			testHelper.AssertWorkItemTasks(6);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 1, "CDF", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 2, learningTaskType, "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 3, "SH0", "CAN", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 4, "COD", "ASN", firstIterationHeader);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 5, "SHV", "ASN", firstIterationHeader);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 6, "CBC", "ASN", topWorkflow.PK);
		}

		public void TestSkillAspect_CancelledReviewAndFailedCheckin()
		{
			var staff = MasterFilesTestHelper.CreateStaff(Factory, "COD", "Coder");
			var learningTaskType = SetUpLearningTaskType();

			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);
			var topWorkflow = testHelper.AddWorkflow(
				"YES",
				null,
				shelfTask: "CH1,COD,ASN,500",
				"CDF,COD,CLS,220",
				"SHV,COD,CAN,230",
				"PRV,COD,CAN,300",
				"CBC,COD,CAN,310",
				"CBF,COD,CAN,400"
				);
			testHelper.ShelfTask.P9_Description = "This Shelf Will Be REJ";
			Factory.Save();
			SetAsCompleted(staff, aspectPK1);

			SkillAspect_CancelledReviewAndFailedCheckin_Core(testHelper, topWorkflow, aspectPK1);

			testHelper.ShelfTask.P9_Status = "ASN";
			Factory.Save();

			SkillAspect_CancelledReviewAndFailedCheckin_Core(testHelper, topWorkflow, aspectPK1);
		}

		void SkillAspect_CancelledReviewAndFailedCheckin_Core(ProcessedShelfsServiceTaskTestHelper testHelper, IProcessHeader topWorkflow, Guid aspectPK)
		{
			AddShelfWithAssessToServiceTaskShelvsetList(testHelper.ShelfTask, ShelfStatuses.RejectedForPendingAspectData, EDIShelvesetInfo.ActionTypes.ShelfCheckin, [aspectPK]);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.RefreshWorkItemTasks();

			testHelper.AssertWorkItemTasks(7);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 220, "CDF", "CLS", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 230, "SHV", "CAN", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 300, "PRV", "CAN", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 310, "CBC", "CAN", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 400, "CBF", "CAN", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 499, "CBC", "ASN", topWorkflow.PK);
			testHelper.AssertTaskExists(serviceLogger.ToString(), 500, "CH1", "SUS", topWorkflow.PK);

			var reviewTask = testHelper.WorkItemTasks.Single(t => t.P9_Sequence == 499);
			AssertEquals(aspectPK, reviewTask.SkillsPivots[0].P9S_Aspect);
		}

		string SetUpLearningTaskType()
		{
			var learningTaskType = "LUP";
			WorkItemProcessTaskTestHelper.SetupLearningTaskRegistry(learningTaskType);
			return learningTaskType;
		}

		void AddShelfWithAssessToServiceTaskShelvsetList(WorkItemProcessTask newShelf, string status, Guid[] aspectPKs)
		{
			var submissionAction = newShelf.P9_Type == EDITaskTypes.TaskActiveCheckin ? EDIShelvesetInfo.ActionTypes.ShelfCheckin : EDIShelvesetInfo.ActionTypes.ShelfsetTest;
			AddShelfWithAssessToServiceTaskShelvsetList(newShelf, status, submissionAction, aspectPKs);
		}

		void AddShelfWithAssessToServiceTaskShelvsetList(WorkItemProcessTask newShelf, string status, string shelfActionType, Guid[] aspectPKs)
		{
			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", newShelf.P9_Description, shelfActionType, newShelf)
			{
				Status = status,
				UserHeaderPK = Guid.NewGuid(),
			};
			foreach (var aspectPK in aspectPKs)
			{
				var aspectReview = AspectReviewSummary.ForAssessAspect(Guid.NewGuid(), aspectPK, "Assess Aspect " + aspectPK);
				shelvesetWithAspectReview.AspectReviews.Add(aspectReview);
			}
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview);
			Factory.Save();
		}

		public void TestSkillAspect_ChangedSkillShouldUpdateReviewTask()
		{
			var learningTaskType = SetUpLearningTaskType();

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False);
			var topWorkflow = testHelperWithAspectDat.AddWorkflow("YES", null, "SH0,LCD,ASN,20", "CDF,LCD,CLS,10", "CBC,REV,ASN,30", "CHK,LCD,ASN,40");

			AddShelfWithAssessToServiceTaskShelvsetList(testHelperWithAspectDat.ShelfTask, ShelfStatuses.Rejected, [aspectPK1]);

			testHelperWithAspectDat.AssertWorkItemTasks(4);
			testHelperWithAspectDat.AssertTaskExists(10, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(20, "SH0", "ASN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(30, "CBC", "ASN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(40, "CHK", "ASN", topWorkflow.PK);

			var reviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));
			Factory.Save();
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(7);
			var newCoding = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "COD" && t.P9_Status == "ASN");
			var iterationHeader = newCoding.P9_FH_ProcessHeader;
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 10, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 20, "SH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 20, learningTaskType, "ASN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 21, "COD", "ASN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 22, "SHV", "ASN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 32, "CBC", "ASN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(serviceLogger.ToString(), 42, "CHK", "ASN", topWorkflow.PK);

			var learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == learningTaskType).ToArray();
			AssertEquals("LCD", learningTasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("(Optional) Awesome Skill", learningTasks[0].P9_Description);
			Assert(learningTasks[0].IsCurrent);
			AssertEquals(1, reviewTask.SkillsPivots.Count);
			AssertEquals(aspectPK1, reviewTask.SkillsPivots[0].P9S_Aspect);

			learningTasks[0].P9_Status = "CAN";
			newCoding.P9_Status = "CLS";
			var newShelf = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type == "SHV" && t.P9_Status == "ASN");
			newShelf.P9_Type = "SH0";
			AddShelfWithAssessToServiceTaskShelvsetList(newShelf, ShelfStatuses.Passed, new[] { aspectPK2 });

			InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperWithAspectDat.AssertWorkItemTasks(8);
			testHelperWithAspectDat.AssertTaskExists(10, "CDF", "CLS", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(20, "SH0", "CAN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(20, learningTaskType, "CAN", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(21, "COD", "CLS", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(22, "SH0", "CLS", iterationHeader);
			testHelperWithAspectDat.AssertTaskExists(32, "CBC", "ASN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(32, learningTaskType, "ASN", topWorkflow.PK);
			testHelperWithAspectDat.AssertTaskExists(42, "CHK", "ASN", topWorkflow.PK);

			learningTasks = testHelperWithAspectDat.WorkItemTasks.Where(t => t.P9_Type == learningTaskType && t.P9_Status == "ASN").ToArray();
			AssertEquals("LCD", learningTasks[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("(Optional) New Skill", learningTasks[0].P9_Description);
			Assert(learningTasks[0].IsCurrent);

			testHelperWithAspectDat.RefreshWorkItemTasks();
			var updatedReviewTask = testHelperWithAspectDat.WorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));
			var anotherFactory = new BusinessObjectFactory();
			var anotherFactoryWorkItemTasks = anotherFactory.Load<WorkItemProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, SQLComparisonOperator.Equal, testHelperWithAspectDat.WorkItem.PK));
			var anotherFactoryReviewTask = anotherFactoryWorkItemTasks.FirstOrDefault(t => t.P9_Type.EqualsIgnoringCase(WorkItemProcessTask.CodeReviewTaskType));
			AssertEquals(1, anotherFactoryReviewTask.SkillsPivots.Count);
			AssertEquals("Review skill updated to new skill", aspectPK2, anotherFactoryReviewTask.SkillsPivots[0].P9S_Aspect);
		}

		public void TestSkillAspect_FailedCheckInWithDeletedProcessTask()
		{
			var learningTaskType = SetUpLearningTaskType();
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.AddWorkflow(null, null, "CH0,LCD,ASN,3", "CDF,LCD,CLS,1", "CBC,REV,CLS,2");
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "LCD";
			staff.GS_EmailAddress = "lee@wtg.com";
			testHelper.WorkItem.WKI_WorkItemNumber = "WI0123";

			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, EDIShelvesetInfo.ActionTypes.ShelfCheckin, processTask: null)
			{
				Status = ShelfStatuses.Rejected,
				UserHeaderPK = Guid.NewGuid(),
				NotificationEmail = "lee@wtg.com",
			};
			var aspectReview = AspectReviewSummary.ForAssessAspect(Guid.NewGuid(), aspectPK1, "Assess Aspect " + aspectPK1);
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview);

			Factory.Save();
			testHelper.RefreshWorkItemTasks();

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.RefreshWorkItemTasks();
			testHelper.AssertWorkItemTasks(3);
			AssertEquals(serviceLogger.ToString(), ShelfStatuses.RejectedAndNotified, ServiceTask.shelvesets[0].Status);
		}

		#endregion

		#region Failed Checkin With Pending Aspect Data

		[TestDate(2020, 4, 2, 4, 30, 0)]
		public void TestFailedCheckin_CreatesReviewTaskWhenThereIsPendingAspectData()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: false);
			testHelper.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask)
			{
				Status = ShelfStatuses.Rejected,
				UserHeaderPK = Guid.NewGuid()
			});

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1");
			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelperWithAspectDat.WorkItem.WKI_WorkItemNumber, "", testHelperWithAspectDat.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};
			var aspectReview = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectName");
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertWorkItemTasks(2);
			var tasksBefore = testHelperWithAspectDat.WorkItemTasks;

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertWorkItemTasks(3);

			var newTask = testHelperWithAspectDat.WorkItemTasks.Except(tasksBefore).SingleOrDefault();
			AssertNotNull("Unable to find new task" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), newTask);
			var submissionTask = testHelperWithAspectDat.AssertTaskExists("The shelf checkin task should be suspended because it failed due to pending aspect data - so just create aspect review task.", 2, "CH0", "SUS");
			AssertStringInRtf(serviceLogger.ToString(), "Task suspended due to pending aspects.", submissionTask.P9_NotesAsString);

			CombineAssertions(() =>
			{
				AssertEquals(1, newTask.P9_Sequence);
				AssertEquals(WorkItemProcessTask.AspectReviewTaskType, newTask.P9_Type);
				AssertEquals("ASN", newTask.P9_Status);

				AssertStringInRtf("Task notes should be set", "DAT 02-Apr-20 04:30: " + aspectReview.GetTaskNotes(), newTask.P9_NotesAsString.ToString());
				AssertEquals("Task description should be set", "Aspect Review - AspectName (with CB)", newTask.P9_Description.ToString());
				AssertEquals("Task should be assigned to the correct capability", capability.PK, newTask.P9_G4_RequiredCapability);
			});
		}

		[TestDate(2020, 4, 2, 4, 30, 0)]
		public void TestFailedCheckin_CreatesReviewTaskWhenThereIsPendingAspectData_WithQualityIteration()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);
			testHelper.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask)
			{
				Status = ShelfStatuses.Rejected,
				UserHeaderPK = Guid.NewGuid()
			});

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1");
			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelperWithAspectDat.WorkItem.WKI_WorkItemNumber, "", testHelperWithAspectDat.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};
			var aspectReview = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectName");
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertWorkItemTasks(2);
			var tasksBefore = testHelperWithAspectDat.WorkItemTasks;

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(4);
			testHelper.AssertTaskExists("The shelf checkin task should be cancelled because quality iterations are enabled for the release group." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, "CH0", "CAN");
			testHelper.AssertTaskExists(3, "CDF", "ASN");
			testHelper.AssertTaskExists(4, "CHK", "ASN");
			testHelperWithAspectDat.AssertWorkItemTasks(3);
			var newTask = testHelperWithAspectDat.WorkItemTasks.Except(tasksBefore).SingleOrDefault();
			AssertNotNull("Unable to find new task", newTask);
			testHelperWithAspectDat.AssertTaskExists("The shelf checkin task should be suspended because it failed due to pending aspect data - so just create aspect review task.", 2, "CH0", "SUS");
			CombineAssertions(() =>
			{
				AssertEquals(1, newTask.P9_Sequence);
				AssertEquals(WorkItemProcessTask.AspectReviewTaskType, newTask.P9_Type);
				AssertEquals("ASN", newTask.P9_Status);

				AssertStringInRtf("Task notes should be set", "DAT 02-Apr-20 04:30: " + aspectReview.GetTaskNotes(), newTask.P9_NotesAsString.ToString());
				AssertEquals("Task description should be set", "Aspect Review - AspectName (with CB)", newTask.P9_Description.ToString());
				AssertEquals("Task should be assigned to the correct capability", capability.PK, newTask.P9_G4_RequiredCapability);
			});
		}

		[TestDate(2020, 4, 2, 4, 30, 0)]
		public void TestFailedCheckin_UsesExitingReviewWhenThereIsPendingAspectDataAndCorrectReviewExists()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: false);
			testHelper.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1", $"{WorkItemProcessTask.AspectReviewTaskType},,ASN,2,{capability.PK}");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask)
			{
				Status = ShelfStatuses.Rejected,
				UserHeaderPK = Guid.NewGuid()
			});

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1", $"{WorkItemProcessTask.AspectReviewTaskType},,ASN,2,{capability.PK},Aspect Review - AspectName (with CB)");
			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelperWithAspectDat.WorkItem.WKI_WorkItemNumber, "", testHelperWithAspectDat.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};
			var aspectReview = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectName");
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview);

			Factory.Save();

			testHelper.AssertWorkItemTasks(3);
			testHelperWithAspectDat.AssertWorkItemTasks(3);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(3);
			testHelperWithAspectDat.AssertWorkItemTasks(3);

			var reviewTask = testHelper.WorkItemTasks.SingleOrDefault(x => x.P9_Sequence == 2 && x.P9_Type == WorkItemProcessTask.AspectReviewTaskType && x.P9_Status == "ASN");
			AssertEquals("No Task notes should be set with no aspect data" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), "", reviewTask.P9_NotesAsString.ToString());

			var reviewTaskwithAspectReview = testHelperWithAspectDat.WorkItemTasks.SingleOrDefault(x => x.P9_Sequence == 2 && x.P9_Type == WorkItemProcessTask.AspectReviewTaskType && x.P9_Status == "ASN");
			AssertStringInRtf("Task notes should be set", "DAT 02-Apr-20 04:30: " + aspectReview.GetTaskNotes(), reviewTaskwithAspectReview.P9_NotesAsString.ToString());
		}

		[TestDate(2020, 4, 2, 4, 30, 0)]
		public void TestFailedCheckin_UsesExitingReviewWhenThereIsPendingAspectDataAndCorrectReviewExists_WithQualityIteration()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.False);
			testHelper.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1", $"{WorkItemProcessTask.AspectReviewTaskType},,ASN,2,{capability.PK}");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask)
			{
				Status = ShelfStatuses.Rejected,
				UserHeaderPK = Guid.NewGuid()
			});

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1", $"{WorkItemProcessTask.AspectReviewTaskType},,ASN,2,{capability.PK},Aspect Review - AspectName (with CB)");
			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelperWithAspectDat.WorkItem.WKI_WorkItemNumber, "", testHelperWithAspectDat.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};
			var aspectReview = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectName");
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview);

			Factory.Save();

			testHelper.AssertWorkItemTasks(3);
			testHelperWithAspectDat.AssertWorkItemTasks(3);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(5);
			testHelperWithAspectDat.AssertWorkItemTasks(3);

			testHelperWithAspectDat.AssertTaskExists("The shelf checkin task should be suspended because it failed due to pending aspect data - so just create aspect review task." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, "CH0", "SUS");

			var aspectReviewTasks = testHelper.WorkItemTasks.Where(x => x.P9_Type == WorkItemProcessTask.AspectReviewTaskType && x.P9_Status == "ASN");
			AssertEquals("Only one active aspect review task", 1, aspectReviewTasks.Count());

			var reviewTaskwithAspectReview = testHelperWithAspectDat.WorkItemTasks.SingleOrDefault(x => x.P9_Sequence == 2 && x.P9_Type == WorkItemProcessTask.AspectReviewTaskType && x.P9_Status == "ASN");
			AssertStringInRtf("Task notes should be set", "DAT 02-Apr-20 04:30: " + aspectReview.GetTaskNotes(), reviewTaskwithAspectReview.P9_NotesAsString.ToString());
		}

		[TestDate(2020, 4, 2, 4, 30, 0)]
		public void TestFailedCheckin_UsesExitingReviewWhenThereIsPendingAspectData_OnlyWithCorrectName()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: false);
			testHelper.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1", $"{WorkItemProcessTask.AspectReviewTaskType},,ASN,2,{capability.PK},Aspect Review - AspectName (with CB)");
			var shelvesetWithSameAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};
			var aspectReview = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectName");
			shelvesetWithSameAspectReview.AspectReviews.Add(aspectReview);
			ServiceTask.shelvesets.Add(shelvesetWithSameAspectReview);
			Factory.Save();

			testHelper.AssertWorkItemTasks(3);
			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);
			testHelper.AssertWorkItemTasks(3);

			var existingTask = testHelper.WorkItemTasks.SingleOrDefault(x => x.P9_Type == WorkItemProcessTask.AspectReviewTaskType);
			AssertNotNull("Unable to find aspect review task" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), existingTask);
			CombineAssertions(() =>
			{
				AssertEquals(2, existingTask.P9_Sequence);
				AssertEquals(WorkItemProcessTask.AspectReviewTaskType, existingTask.P9_Type);
				AssertEquals("ASN", existingTask.P9_Status);
				AssertStringInRtf("Task notes should be set", "DAT 02-Apr-20 04:30: " + aspectReview.GetTaskNotes(), existingTask.P9_NotesAsString.ToString());
			});

			var shelvesetWithDifferentAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};
			var differentAspectReview = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "Different Aspect Name");
			shelvesetWithDifferentAspectReview.AspectReviews.Add(differentAspectReview);
			ServiceTask.shelvesets.Add(shelvesetWithDifferentAspectReview);
			testHelper.ShelfTask.P9_Status = "ASN";
			Factory.Save();

			testHelper.AssertWorkItemTasks(3);
			InitialiseAndRunTaskSchedule(ServiceTask);
			testHelper.AssertWorkItemTasks(4);

			var existingAspectReviewTask = testHelper.WorkItemTasks.SingleOrDefault(x => x.P9_Type == WorkItemProcessTask.AspectReviewTaskType && x.P9_Description == "Aspect Review - AspectName (with CB)");
			var newAspectReviewTask = testHelper.WorkItemTasks.SingleOrDefault(x => x.P9_Type == WorkItemProcessTask.AspectReviewTaskType && x.P9_Description == "Aspect Review - Different Aspect Name (with CB)");
			AssertNotNull("Unable to find old aspect review task" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), existingAspectReviewTask);
			AssertNotNull("Unable to find new aspect review task" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), newAspectReviewTask);
			CombineAssertions(() =>
			{
				AssertEquals(2, existingAspectReviewTask.P9_Sequence);
				AssertEquals(WorkItemProcessTask.AspectReviewTaskType, existingAspectReviewTask.P9_Type);
				AssertEquals("ASN", existingAspectReviewTask.P9_Status);
				AssertStringInRtf("Task notes should be set", "DAT 02-Apr-20 04:30: " + aspectReview.GetTaskNotes(), existingAspectReviewTask.P9_NotesAsString.ToString());
				AssertNotContains("Task notes should be set", differentAspectReview.GetTaskNotes(), existingAspectReviewTask.P9_NotesAsString.ToString());

				AssertEquals("New review should be one less than checkin task", 1, newAspectReviewTask.P9_Sequence);
				AssertEquals(WorkItemProcessTask.AspectReviewTaskType, newAspectReviewTask.P9_Type);
				AssertEquals("ASN", newAspectReviewTask.P9_Status);
				AssertNotContains("Task notes should be set", aspectReview.GetTaskNotes(), newAspectReviewTask.P9_NotesAsString.ToString());
				AssertStringInRtf("Task notes should be set", "DAT 02-Apr-20 04:30: " + differentAspectReview.GetTaskNotes(), newAspectReviewTask.P9_NotesAsString.ToString());
			});
		}

		[TestDate(2020, 4, 2, 4, 30, 0)]
		public void TestFailedCheckin_UsesExitingReviewWhenThereIsPendingAspectData_UnlessClosedOrCancelled()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			var testHelperASN = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperASN.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: false);
			var aspectReviewASN = CreateWorkflowAndShelfWithAspect(testHelperASN, capability, "ASN");

			var testHelperWRK = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWRK.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			var aspectReviewWRK = CreateWorkflowAndShelfWithAspect(testHelperWRK, capability, "WRK");

			var testHelperSUS = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperSUS.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			var aspectReviewSUS = CreateWorkflowAndShelfWithAspect(testHelperSUS, capability, "SUS");

			var testHelperCAN = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperCAN.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			var aspectReviewCAN = CreateWorkflowAndShelfWithAspect(testHelperCAN, capability, "CAN");

			var testHelperCLS = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperCLS.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			var aspectReviewCLS = CreateWorkflowAndShelfWithAspect(testHelperCLS, capability, "CLS");

			Factory.Save();

			testHelperASN.AssertWorkItemTasks(3);
			testHelperWRK.AssertWorkItemTasks(3);
			testHelperSUS.AssertWorkItemTasks(3);
			testHelperCAN.AssertWorkItemTasks(3);
			testHelperCLS.AssertWorkItemTasks(3);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperASN.AssertWorkItemTasks(3);
			testHelperWRK.AssertWorkItemTasks(3);
			testHelperSUS.AssertWorkItemTasks(3);
			testHelperCAN.AssertWorkItemTasks(4);
			testHelperCLS.AssertWorkItemTasks(4);

			CheckReviewUsed(serviceLogger, testHelperASN, aspectReviewASN, 2, "ASN");
			CheckReviewUsed(serviceLogger, testHelperWRK, aspectReviewWRK, 2, "WRK");
			CheckReviewUsed(serviceLogger, testHelperSUS, aspectReviewSUS, 2, "SUS");
			CheckReviewUsed(serviceLogger, testHelperCAN, aspectReviewCAN, 1, "ASN");
			CheckReviewUsed(serviceLogger, testHelperCLS, aspectReviewCLS, 1, "ASN");
		}

		[TestDate(2020, 4, 2, 4, 30, 0)]
		public void TestFailedCheckin_UsesExitingReviewWithTruncatedName()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: false);
			testHelper.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1", $"{WorkItemProcessTask.AspectReviewTaskType},,ASN,2,{capability.PK},Aspect Review - AspectNameTruncatedAfter (with CB)");
			var shelvesetWithAspectReview2 = new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};
			var aspectReview = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectNameTruncatedAfter24Characters");
			shelvesetWithAspectReview2.AspectReviews.Add(aspectReview);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview2);

			Factory.Save();

			testHelper.AssertWorkItemTasks(3);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(3);

			CheckReviewUsed(serviceLogger, testHelper, aspectReview, 2, "ASN");
		}

		[TestDate(2020, 4, 2, 4, 30, 0)]
		public void TestFailedCheckin_UsesCurrentTaskWhenWorkflowIsMisconfigured()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: false);
			var workflowParent = testHelper.AddWorkflow("YES", null, null, "UDF,LCD,ASN,1000");
			var workflowChild = testHelper.AddWorkflow("YES", null, "CH0,LCD,ASN,200", "CDF,LCD,CLS,100");
			workflowChild.GetOrCreateLinkToParent(workflowParent);

			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};
			var aspectReview = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectName");
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			Factory.Save();

			testHelper.AssertWorkItemTasks(3);
			var tasksBefore = testHelper.WorkItemTasks;

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(4);
			var newTask = testHelper.WorkItemTasks.Except(tasksBefore).SingleOrDefault();
			AssertNotNull("Unable to find new task" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), newTask);
			testHelper.AssertTaskExists("The shelf checkin task should be suspended because it failed due to pending aspect data - so just create aspect review task." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 200, "CH0", "SUS");
			CombineAssertions(() =>
			{
				AssertEquals(199, newTask.P9_Sequence);
				AssertEquals(WorkItemProcessTask.AspectReviewTaskType, newTask.P9_Type);
				AssertEquals("ASN", newTask.P9_Status);

				AssertStringInRtf("Task notes should be set", "DAT 02-Apr-20 04:30: " + aspectReview.GetTaskNotes(), newTask.P9_NotesAsString.ToString());
				AssertEquals("Task description should be set", "Aspect Review - AspectName (with CB)", newTask.P9_Description.ToString());
				AssertEquals("Task should be assigned to the correct capability", capability.PK, newTask.P9_G4_RequiredCapability);
			});
		}

		void CheckReviewUsed(TestServiceLogger serviceLogger, ProcessedShelfsServiceTaskTestHelper testHelper, AspectReviewSummary aspectReview, int expectedSeqNumber, string expectedStatus)
		{
			var validReviewTask = testHelper.WorkItemTasks.SingleOrDefault(x => x.P9_Type == WorkItemProcessTask.AspectReviewTaskType && x.P9_Status != "CAN" && x.P9_Status != "CLS");
			AssertNotNull("Unable to find aspect review task", validReviewTask);
			CombineAssertions(() =>
			{
				AssertEquals(serviceLogger.ToString(), expectedSeqNumber, validReviewTask.P9_Sequence);
				AssertEquals(serviceLogger.ToString(), WorkItemProcessTask.AspectReviewTaskType, validReviewTask.P9_Type);
				AssertEquals(serviceLogger.ToString(), expectedStatus, validReviewTask.P9_Status);
				var truncatedAspectName = aspectReview.AspectName.Substring(0, Math.Min(24, aspectReview.AspectName.Length));
				AssertEquals(serviceLogger.ToString(), $"Aspect Review - {truncatedAspectName} (with CB)", validReviewTask.P9_Description.ToString());
				AssertStringInRtf("Task notes should be set", "DAT 02-Apr-20 04:30: " + aspectReview.GetTaskNotes(), validReviewTask.P9_NotesAsString.ToString());
			});
		}

		AspectReviewSummary CreateWorkflowAndShelfWithAspect(ProcessedShelfsServiceTaskTestHelper testHelper, GlbCapability capability, string aspectReviewTaskStatus)
		{
			testHelper.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1", $"{WorkItemProcessTask.AspectReviewTaskType},,{aspectReviewTaskStatus},2,{capability.PK},Aspect Review - AspectName (with CB)");
			var shelvesetWithAspectReview2 = new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};
			var aspectReview = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectName");
			shelvesetWithAspectReview2.AspectReviews.Add(aspectReview);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview2);
			return aspectReview;
		}

		[TestDate(2020, 4, 2, 4, 30, 0)]
		public void TestFailedCheckin_UsesExitingReviewWhenThereIsPendingAspectData_OnlyWithCorrectCapability()
		{
			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";
			var wrongCapability = Factory.NewWithValidTestData<GlbCapability>();
			wrongCapability.G4_Code = "DOH";

			var testHelperCAP = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperCAP.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: false);
			var aspectReviewCAP = CreateWorkflowAndShelfWithAspect(testHelperCAP, capability, "ASN");

			var testHelperDOH = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperDOH.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			var aspectReviewDOH = CreateWorkflowAndShelfWithAspect(testHelperDOH, wrongCapability, "ASN");

			Factory.Save();

			testHelperCAP.AssertWorkItemTasks(3);
			testHelperDOH.AssertWorkItemTasks(3);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperCAP.AssertWorkItemTasks(3);
			testHelperDOH.AssertWorkItemTasks(4);

			CheckReviewUsed(serviceLogger, testHelperCAP, aspectReviewCAP, 2, "ASN");
			var validReviewTasks = testHelperDOH.WorkItemTasks.Where(x => x.P9_Type == WorkItemProcessTask.AspectReviewTaskType);
			AssertEquals("Should be two review tasks" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, validReviewTasks.Count());
			var wrongReview = validReviewTasks.Single(t => t.P9_G4_RequiredCapability == wrongCapability.PK);
			CombineAssertions(() =>
			{
				AssertEquals(2, wrongReview.P9_Sequence);
				AssertEquals(WorkItemProcessTask.AspectReviewTaskType, wrongReview.P9_Type);
				AssertEquals("ASN", wrongReview.P9_Status);
				AssertNotContains("Task notes should be set", aspectReviewDOH.GetTaskNotes(), wrongReview.P9_NotesAsString.ToString());
			});
			var rightReview = validReviewTasks.Single(t => t.P9_G4_RequiredCapability == capability.PK);
			CombineAssertions(() =>
			{
				AssertEquals("New review should be 1 less than checkin task", 1, rightReview.P9_Sequence);
				AssertEquals(WorkItemProcessTask.AspectReviewTaskType, rightReview.P9_Type);
				AssertEquals("ASN", rightReview.P9_Status);
				AssertEquals("Aspect Review - AspectName (with CB)", rightReview.P9_Description.ToString());
				AssertStringInRtf("Task notes should be set", aspectReviewDOH.GetTaskNotes(), rightReview.P9_NotesAsString.ToString());
			});
		}

		[TestDate(2020, 4, 2, 4, 30, 0)]
		public void TestFailedCheckin_CreatesMultipleAspectReviewsWithMultipleAspect_DifferentCapabilities()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: false);
			testHelper.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask)
			{
				Status = ShelfStatuses.Rejected,
				UserHeaderPK = Guid.NewGuid()
			});

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1");
			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelperWithAspectDat.WorkItem.WKI_WorkItemNumber, "", testHelperWithAspectDat.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};
			var aspectReview1 = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CP1", "AspectName1");
			var aspectReview2 = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CP2", "AspectName2");
			var aspectReview3 = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CP3", "AspectName3");
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview1);
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview2);
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview3);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview);

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			var capability3 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "CP1";
			capability2.G4_Code = "CP2";
			capability3.G4_Code = "CP3";
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertWorkItemTasks(2);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertWorkItemTasks(5);

			var newTasks = testHelperWithAspectDat.WorkItemTasks.Where(x => x.P9_Sequence == 1 && x.P9_Type == WorkItemProcessTask.AspectReviewTaskType && x.P9_Status == "ASN");
			var newTask1 = newTasks.SingleOrDefault(t => t.P9_G4_RequiredCapability == capability1.PK);
			AssertNotNull("Task should exist and assigned to the correct capability" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), newTask1);
			AssertStringInRtf("Task notes should be set", aspectReview1.GetTaskNotes(), newTask1.P9_NotesAsString.ToString());
			AssertEquals("Task description should be set", "Aspect Review - AspectName1 (with CB)", newTask1.P9_Description.ToString());

			var newTask2 = newTasks.SingleOrDefault(t => t.P9_G4_RequiredCapability == capability2.PK);
			AssertNotNull("Task should exist and assigned to the correct capability" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), newTask2);
			AssertStringInRtf("Task notes should be set", aspectReview2.GetTaskNotes(), newTask2.P9_NotesAsString.ToString());
			AssertEquals("Task description should be set", "Aspect Review - AspectName2 (with CB)", newTask2.P9_Description.ToString());

			var newTask3 = newTasks.SingleOrDefault(t => t.P9_G4_RequiredCapability == capability3.PK);
			AssertNotNull("Task should exist and assigned to the correct capability" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), newTask3);
			AssertStringInRtf("Task notes should be set", aspectReview3.GetTaskNotes(), newTask3.P9_NotesAsString.ToString());
			AssertEquals("Task description should be set", "Aspect Review - AspectName3 (with CB)", newTask3.P9_Description.ToString());
		}

		[TestDate(2020, 4, 2, 4, 30, 0)]
		public void TestFailedCheckinCreatesMultipleAspectReviewsWithMultipleAspect_SameCapabilities()
		{
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations(ZBool.True, shouldAddDatUser: false);
			testHelper.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1");
			ServiceTask.shelvesets.Add(new EDIShelvesetInfo(@"CORP\Test.User", testHelper.WorkItem.WKI_WorkItemNumber, "", testHelper.ShelfTask)
			{
				Status = ShelfStatuses.Rejected,
				UserHeaderPK = Guid.NewGuid()
			});

			var testHelperWithAspectDat = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperWithAspectDat.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperWithAspectDat.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1");
			var shelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelperWithAspectDat.WorkItem.WKI_WorkItemNumber, "", testHelperWithAspectDat.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};
			var aspectReview1 = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CP1", "AspectName1");
			var aspectReview2 = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CP1", "AspectName2");
			var aspectReview3 = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CP1", "AspectName3");
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview1);
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview2);
			shelvesetWithAspectReview.AspectReviews.Add(aspectReview3);
			ServiceTask.shelvesets.Add(shelvesetWithAspectReview);

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "CP1";
			Factory.Save();

			testHelper.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertWorkItemTasks(2);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelper.AssertWorkItemTasks(2);
			testHelperWithAspectDat.AssertWorkItemTasks(5);

			var newTasks = testHelperWithAspectDat.WorkItemTasks.Where(x => x.P9_Sequence == 1 && x.P9_Type == WorkItemProcessTask.AspectReviewTaskType && x.P9_Status == "ASN");
			var newTask1 = newTasks.SingleOrDefault(t => t.P9_Description == "Aspect Review - AspectName1 (with CB)");
			AssertNotNull("Task should exist and assigned to the correct capability" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), newTask1);
			AssertStringInRtf("Task notes should be set", aspectReview1.GetTaskNotes(), newTask1.P9_NotesAsString.ToString());
			AssertEquals("Task capability should be set", capability1.PK, newTask1.P9_G4_RequiredCapability);

			var newTask2 = newTasks.SingleOrDefault(t => t.P9_Description == "Aspect Review - AspectName2 (with CB)");
			AssertNotNull("Task should exist and assigned to the correct capability" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), newTask2);
			AssertStringInRtf("Task notes should be set", aspectReview2.GetTaskNotes(), newTask2.P9_NotesAsString.ToString());
			AssertEquals("Task capability should be set", capability1.PK, newTask2.P9_G4_RequiredCapability);

			var newTask3 = newTasks.SingleOrDefault(t => t.P9_Description == "Aspect Review - AspectName3 (with CB)");
			AssertNotNull("Task should exist and assigned to the correct capability" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), newTask3);
			AssertStringInRtf("Task notes should be set", aspectReview3.GetTaskNotes(), newTask3.P9_NotesAsString.ToString());
			AssertEquals("Task capability should be set", capability1.PK, newTask3.P9_G4_RequiredCapability);
		}

		[TestDate(2020, 4, 2, 4, 30, 0)]
		public void TestAspectReviewOnlyCreatedOnFailedCheckin()
		{
			var testHelperForPassedShelf = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperForPassedShelf.SetUpForQualityIterations(ZBool.False);
			testHelperForPassedShelf.AddWorkflow("YES", null, "SH0,LCD,ASN,2", "CDF,LCD,CLS,1");
			var passedShelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelperForPassedShelf.WorkItem.WKI_WorkItemNumber, "", testHelperForPassedShelf.ShelfTask)
			{
				Status = ShelfStatuses.Passed,
				UserHeaderPK = Guid.NewGuid(),
			};
			passedShelvesetWithAspectReview.AspectReviews.Add(AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectName"));
			ServiceTask.shelvesets.Add(passedShelvesetWithAspectReview);

			var testHelperForFailedShelf = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperForFailedShelf.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperForFailedShelf.AddWorkflow("YES", null, "SH0,LCD,ASN,2", "CDF,LCD,CLS,1");
			var failedShelvesetWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelperForFailedShelf.WorkItem.WKI_WorkItemNumber, "", testHelperForFailedShelf.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};
			failedShelvesetWithAspectReview.AspectReviews.Add(AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectName"));
			ServiceTask.shelvesets.Add(failedShelvesetWithAspectReview);

			var testHelperForCheckin = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperForCheckin.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperForCheckin.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1");
			var checkinWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelperForCheckin.WorkItem.WKI_WorkItemNumber, "", testHelperForCheckin.ShelfTask)
			{
				Status = ShelfStatuses.CheckedIn,
				UserHeaderPK = Guid.NewGuid(),
			};
			checkinWithAspectReview.AspectReviews.Add(AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectName"));
			ServiceTask.shelvesets.Add(checkinWithAspectReview);

			var testHelperForFailedCheckin = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelperForFailedCheckin.SetUpForQualityIterations(ZBool.False, shouldAddDatUser: false, shouldCreateBMSystem: false);
			testHelperForFailedCheckin.AddWorkflow("YES", null, "CH0,LCD,ASN,2", "CDF,LCD,CLS,1");
			var failedCheckinWithAspectReview = new EDIShelvesetInfo(@"CORP\Test.User", testHelperForFailedCheckin.WorkItem.WKI_WorkItemNumber, "", testHelperForFailedCheckin.ShelfTask)
			{
				Status = ShelfStatuses.RejectedForPendingAspectData,
				UserHeaderPK = Guid.NewGuid(),
			};
			var aspectReview = AspectReviewSummary.ForCapabilityAspect(Guid.NewGuid(), Guid.NewGuid(), "CAP", "AspectName");
			failedCheckinWithAspectReview.AspectReviews.Add(aspectReview);
			ServiceTask.shelvesets.Add(failedCheckinWithAspectReview);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Code = "CAP";

			Factory.Save();

			testHelperForPassedShelf.AssertWorkItemTasks(2);
			testHelperForCheckin.AssertWorkItemTasks(2);
			testHelperForFailedShelf.AssertWorkItemTasks(2);
			testHelperForFailedCheckin.AssertWorkItemTasks(2);

			var serviceLogger = InitialiseAndRunTaskSchedule(ServiceTask);

			testHelperForPassedShelf.AssertWorkItemTasks(2);
			testHelperForCheckin.AssertWorkItemTasks(2);
			testHelperForFailedShelf.AssertWorkItemTasks(4);
			AssertNull(testHelperForFailedShelf.WorkItemTasks.Where(x => x.P9_Type == WorkItemProcessTask.AspectReviewTaskType).FirstOrDefault());

			testHelperForFailedCheckin.AssertWorkItemTasks(3);
			testHelperForFailedCheckin.AssertTaskExists("The shelf checkin task should be suspended because it failed due to pending aspect data - so just create aspect review task." + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), 2, "CH0", "SUS");
			var newTask = testHelperForFailedCheckin.WorkItemTasks.Where(x => x.P9_Type == WorkItemProcessTask.AspectReviewTaskType).SingleOrDefault();

			AssertNotNull("Unable to find new task" + System.Environment.NewLine + "Logs:" + serviceLogger.ToString(), newTask);
			CombineAssertions(() =>
			{
				AssertEquals(1, newTask.P9_Sequence);
				AssertEquals(WorkItemProcessTask.AspectReviewTaskType, newTask.P9_Type);
				AssertEquals("ASN", newTask.P9_Status);

				AssertStringInRtf("Task notes should be set", "DAT 02-Apr-20 04:30: " + aspectReview.GetTaskNotes(), newTask.P9_NotesAsString.ToString());
				AssertEquals("Task description should be set", "Aspect Review - AspectName (with CB)", newTask.P9_Description.ToString());
				AssertEquals("Task should be assigned to the correct capability", capability.PK, newTask.P9_G4_RequiredCapability);
			});
		}

		#endregion

		#region Database Error Handling

		public void TestConcurrencyErrorHandling()
		{
			// Arrange
			var submission = ConfigureShelfsetForServiceTaskWithGitPull(ShelfStatuses.CheckedIn, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle");

			var factoryForConcurrentEdit = new BusinessObjectFactory { RefreshEnabled = false };
			var loadedTask = factoryForConcurrentEdit.Load<ProcessTask>(submission.RelatedProcessTask.PK);
			var hasMadeConcurrentEdit = false;

			InitialiseTaskSchedule(ServiceTask);
			var logger = new Mock<ILogger>();
			logger
				.Setup(l => l.Log(LogType.Debug, "Saving batch factory"))
				.Callback((LogType a, string b) =>
				{
					if (!hasMadeConcurrentEdit)
					{
						loadedTask.P9_Status = ProcessTaskStatusCodeList.Codes.Suspended;
						factoryForConcurrentEdit.Save();
						hasMadeConcurrentEdit = true;
					}
				})
				.Verifiable();
			logger
				.Setup(l => l.Log(LogType.Information, "Concurrency error encountered; retrying batch."))
				.Verifiable();

			ServiceTask.ServiceLogger = logger.Object;

			// Act
			RunTaskSchedule(ServiceTask);

			// Assert
			logger.VerifyAll();
			AssertEquals(nameof(hasMadeConcurrentEdit), true, hasMadeConcurrentEdit);
			AssertEquals(ShelfStatuses.CheckedInAndNotified, submission.Status);

			var reloadedTask = Factory.CreateNewFactory().Load<ProcessTask>(submission.RelatedProcessTask.PK);
			AssertEquals(ProcessTaskStatusCodeList.Codes.Closed, reloadedTask.P9_Status);
		}

		public void TestConstraintViolationHandling()
		{
			// Arrange
			var testHelper = new ProcessedShelfsServiceTaskTestHelper(Factory);
			testHelper.SetUpForQualityIterations();

			var reviewer = MasterFilesTestHelper.CreateStaff(Factory, "DE", "davey@daveeast.com");

			var aspectPK = Guid.NewGuid();
			var aspectReview1 = AspectReviewSummary.ForAssessAspect(Guid.NewGuid(), aspectPK, "Same Aspect");
			var aspectReview2 = AspectReviewSummary.ForAssessAspect(Guid.NewGuid(), aspectPK, "Same Aspect");

			var submission1 = ConfigureShelfsetForServiceTaskWithGitPull(EDIShelvesetInfo.ActionTypes.ShelfsetTest, ShelfStatuses.Rejected, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1210?_a=overview", "GitTitle 1");
			submission1.AspectReviews.Add(aspectReview1);
			submission1.AspectReviews.Add(aspectReview2);
			var submission2 = ConfigureShelfsetForServiceTaskWithGitPull(EDIShelvesetInfo.ActionTypes.ShelfsetTest, ShelfStatuses.Rejected, "http://tfs.wtg.zone:8080/tfs/CargoWise/DataScience/_git/VesselSpeedService/pullrequest/1211?_a=overview", "GitTitle 2");
			var codeReview1 = MasterFilesTestHelper.CreateTask(submission1.RelatedProcessTask.Parent, reviewer.GS_Code, taskType: WorkItemProcessTask.CodeReviewTaskType, sequence: 10);
			MasterFilesTestHelper.CreateTask(submission2.RelatedProcessTask.Parent, reviewer.GS_Code, taskType: WorkItemProcessTask.CodeReviewTaskType, sequence: 10);

			SetAsCompleted(submission1.RelatedProcessTask.AssignedStaffMember, aspectPK, hasPassed: true);
			Factory.Save();

			InitialiseTaskSchedule(ServiceTask);

			var hasAddedDuplicatePivot = false;
			var logger = new Mock<ILogger>();
			logger
				.Setup(l => l.Log(LogType.Debug, "Saving batch factory"))
				.Callback((LogType a, string b) =>
				{
					if (!hasAddedDuplicatePivot)
					{
						var factory = new BusinessObjectFactory { RefreshEnabled = false };
						var loadedTask = factory.Load<ProcessTask>(codeReview1.PK);
						loadedTask.SkillsPivots.AddAspect(aspectPK);
						factory.Save();
						hasAddedDuplicatePivot = true;
					}
				})
				.Verifiable();
			logger
				.Setup(l => l.Log(LogType.Error, It.IsAny<string>(), It.Is<Exception>(e => e.Message.Contains("unique index 'FK_UX__P9S_P9_P9S_Aspect_P9S_WiseTechAcademySubjectCode_P9S_HS'", StringComparison.Ordinal))))
				.Verifiable();
			logger
				.Setup(l => l.Log(LogType.Information, It.Is<string>(s => s.Contains("Error encountered when processing a submission; retrying batch (excluding that submission).", StringComparison.Ordinal))))
				.Verifiable();

			ServiceTask.ServiceLogger = logger.Object;

			// Act
			RunTaskSchedule(ServiceTask);

			// Assert
			AssertEquals("Constraint violation should prevent submission1 from being updated", ShelfStatuses.Rejected, submission1.Status);
			AssertEquals("Constraint violation in submission1 should not affect submission2", ShelfStatuses.RejectedAndNotified, submission2.Status);
			logger.VerifyAll();
		}

		#endregion

		#region Implementation

		ProcessedShelfsServiceTaskForTest ServiceTask => serviceTask ?? (serviceTask = new ProcessedShelfsServiceTaskForTest(Factory));
		ProcessedShelfsServiceTaskForTest serviceTask;

		#region AssertRtfTextsEqualLanguageIndependent

		public static void AssertRtfText(string expectedText, ZBlob rawRTF)
		{
			AssertRtfText(string.Empty, expectedText, rawRTF);
		}

		public static void AssertRtfText(string message, string expectedText, ZBlob rawRtf)
		{
			MasterFilesTestHelper.AssertRtfText(message, expectedText, rawRtf);
		}

		public static void AssertStringInRtf(string input, string rtf)
		{
			AssertStringInRtf(string.Empty, input, rtf);
		}

		public static void AssertStringInRtf(string message, string input, string rtf)
		{
			if (rtf.Contains(input))
			{
				AssertContains(message, input, rtf);
				return;
			}
			if (rtf.Contains("{HYPERLINK"))
			{
				input = Hyperlinkify(input);
				input = GetRtfStringIndependentOfMachineSettings(input);
				rtf = GetRtfStringIndependentOfMachineSettings(rtf);
			}
			AssertContains(message, input, rtf);
		}

		public static void AssertRtfTextsEqualLanguageIndependent(string expectedRtf, string actualRtf)
		{
			AssertRtfTextsEqualLanguageIndependent(string.Empty, expectedRtf, actualRtf);
		}

		public static void AssertRtfTextsEqualLanguageIndependent(string message, string expectedRtf, string actualRtf)
		{
			AssertEquals(message, GetRtfStringIndependentOfMachineSettings(expectedRtf), GetRtfStringIndependentOfMachineSettings(actualRtf));
		}

		static string GetRtfStringIndependentOfMachineSettings(string rtf)
		{
			string result = rtf;
			result = ReplaceWithCultureIndependentString(result, "lang");
			result = ReplaceWithCultureIndependentString(result, "charset");
			result = ReplaceWithCultureIndependentString(result, "ansicpg");
			result = ReplaceWithCultureIndependentString(result, "fs");
			result = ReplaceWithCultureIndependentString(result, "deflang");
			result = RemoveGenerator(result);
			return result.TrimWithUnicodeWhitespace();
		}

		static string RemoveGenerator(string rtf)
		{
			int startIndex = rtf.IndexOf("generator");
			if (startIndex < 0)
			{
				return rtf;
			}
			int endindex = rtf.IndexOf("}", startIndex);
			if (endindex < 0)
			{
				return rtf;
			}
			return rtf.Replace(rtf.Substring(startIndex, endindex - startIndex), "generator Riched20 ZZZ");
		}

		static string ReplaceWithCultureIndependentString(string rtf, string stringToReplace)
		{
			int startIndex = rtf.IndexOf(stringToReplace);
			string result = rtf;
			if (startIndex != -1)
			{
				int endIndex = startIndex + stringToReplace.Length;
				while (char.IsDigit(rtf[endIndex]))
				{
					endIndex++;
				}
				string fullStringToReplace = result.Substring(startIndex, (endIndex - startIndex));
				result = result.Replace(fullStringToReplace, stringToReplace + "ZZZ");
			}
			return result;
		}

		static string Hyperlinkify(string input)
		{
			var hyperlinkRegex = new Regex(@"http:\/\/(\S)+");

			var matches = hyperlinkRegex.Matches(input);

			for (var i = matches.Count - 1; i >= 0; --i)
			{
				var hyperlinkString = matches[i].Value;
				input = input.Replace(hyperlinkString, @"{{\field{\*\fldinst{HYPERLINK " + hyperlinkString + @" }}{\fldrslt{" + hyperlinkString + @"\ul0\cf0}}}}\f0\fs20 ");
			}

			return input;
		}

		#endregion

		SupportIncident CreateNewGpcIncidentWithWorkItem(NewWorkItem workItem, GlbStaff staff)
		{
			GlbGroup group = Factory.New<GlbGroup>();
			group.Staff.Add(staff);

			EDIDataRegistry.Instance.ProcessedShelfsNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, group.PK.ToGuid());

			LicenceDatabase licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			licenceDatabase.LD_ReleaseRing = ReleaseRings.Codes.GPC;
			ClientCompany clientCompany = Factory.NewWithValidTestData<ClientCompany>();
			clientCompany.LCC_LD = licenceDatabase.PK;

			var supportIncident = Factory.NewWithValidTestData<SupportIncident>();
			supportIncident.IM_LCC = clientCompany.PK;
			supportIncident.IM_LD = licenceDatabase.PK;
			supportIncident.IM_Status = IncidentMainLookups.Status.Working;
			ServiceTask.supportIncidents.Clear();
			ServiceTask.supportIncidents.Add(supportIncident);

			supportIncident.RelatedWorkItems.Add(workItem);

			return supportIncident;
		}

		public class ProcessedShelfsServiceTaskForTest : ProcessedShelfsServiceTask
		{
			public ProcessedShelfsServiceTaskForTest(BusinessObjectFactory factory)
			{
				Factory = factory;
			}

			protected BusinessObjectFactory Factory { get; }

			protected override IDatSubmissionsProvider CreateDatSubmissionsProvider()
			{
				return new DummyDatSubmissionsProvider(this);
			}

			/// <summary>
			/// Upsettingly, this class duplicates a lot of the logic in the real class. TODO in WI00861675: refactor such that we can mock one layer down at ICrikeyDataAccess instead.
			/// </summary>
			class DummyDatSubmissionsProvider : IDatSubmissionsProvider
			{
				public DummyDatSubmissionsProvider(ProcessedShelfsServiceTaskForTest serviceTask)
				{
					this.serviceTask = serviceTask;
				}

				public bool CanProvideSubmissions => true;

				public DatSubmissionBatch GetNextBatch()
				{
					var submissions = serviceTask.shelvesets.Where(s => !submissionsThatCouldNotBeProcessed.Contains(s.UserHeaderPK)).ToArray();
					if (submissions.Any(s => s.Name == "PleaseThrowArgumentException"))
					{
						throw new ArgumentException();
					}

					var factory = serviceTask.Factory.CreateNewFactory();
					foreach (var submission in submissions)
					{
						if (submission.RelatedProcessTask != null)
						{
							var task = factory.Load<WorkItemProcessTask>(submission.RelatedProcessTask.PK) ?? throw new InvalidOperationException("Could not load related task from the db. Make sure you save the factory before running the service task.");
							submission.RelatedProcessTask = task;
						}
					}

					return new DatSubmissionBatch(submissions, factory, isLastBatch: true);
				}

				public void UpdateStatusToNotified(EDIShelvesetInfo submission)
				{
					if (submission.Name.Contains("FailUpdateShelvesetStatus"))
					{
						throw new IOException("Failed to update shelveset status");
					}
					if (submission.Name.Contains("FailDatabaseUpdateShelvesetStatus"))
					{
						var error = SqlExceptionBuilder.CreateSqlError(1222, 1, 1, Db.Connection.ServerName, "Lock request time out period exceeded.", "", 1);
						var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
						var exception = SqlExceptionBuilder.CreateSqlException(errors);
						throw exception;
					}
					if (submission.GetNotifiedEquivalentOfCurrentStatus() is { } newStatus)
					{
						submission.Status = newStatus;
					}
				}

				public void Dispose()
				{
				}

				public void NotifySubmissionCouldNotBeProcessed(EDIShelvesetInfo submission)
				{
					submissionsThatCouldNotBeProcessed.Add(submission.UserHeaderPK);
				}

				readonly ProcessedShelfsServiceTaskForTest serviceTask;
				readonly HashSet<Guid> submissionsThatCouldNotBeProcessed = [];
			}

			public readonly List<EDIShelvesetInfo> shelvesets = new List<EDIShelvesetInfo>();
			public readonly List<WorkItemProcessTask> processTasks = new List<WorkItemProcessTask>();
			public readonly List<SupportIncident> supportIncidents = new List<SupportIncident>();

			public LatestReleaseBuildsDictionary GetBuildsForTesting()
			{
				return Builds;
			}
		}

		protected override void SetUpCore()
		{
			ReleaseBuildContentForLegacyTest.Enable();
			base.SetUpCore();

			aspectPK1 = Guid.NewGuid();
			aspectPK2 = Guid.NewGuid();

			assessServiceClientMock = new();
			assessServiceClientMock
				.SetupGetLearningUnitName(aspectPK1, "Awesome Skill")
				.SetupGetLearningUnitName(aspectPK2, "New Skill")
				.SetupGetLearningUnitUrl(aspectPK1, "https://nothing/assess/123")
				.SetupGetLearningUnitUrl(aspectPK2, "https://nothing/assess/456");

			ObjectFactory.Substitute(assessServiceClientMock.Object);
		}

		void SetAsCompleted(GlbStaff staff, Guid aspectPK, bool hasPassed = true)
		{
			assessServiceClientMock.SetupHasCompletedLearningUnit(staff, aspectPK, hasPassed);
		}

		Guid aspectPK1;
		Guid aspectPK2;
		Mock<IAssessServiceClient> assessServiceClientMock;

		#endregion
	}

	public class ProcessedShelfsServiceTaskTestHelper
	{
		public ProcessedShelfsServiceTaskTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		#region Properties

		public WorkItemProcessTask ShelfTask { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public WorkItemProcessTask[] WorkItemTasks { get; set; }

		public NewWorkItem WorkItem => workItem ?? (workItem = factory.NewWithValidTestData<NewWorkItem>());

		#endregion

		NewWorkItem workItem;
		readonly BusinessObjectFactory factory;
		readonly Dictionary<string, ZGuid> releaseGroups = new Dictionary<string, ZGuid>();

		#region SetUpForQualityIteration

		public void SetUpForQualityIterations(bool defaultSettingForReleaseGroupsNotSpecified = true, string reasonCode = "SHV", string reasonMessage = @"Failed Shelf Test/Checkin", bool shouldAddQualityIterationReason = true, bool shouldAddDatUser = true, bool shouldAddTaskTypes = true, bool shouldCreateBMSystem = true)
		{
			Globals.IsUserInteractive = false;

			VisualBoardsTestCase.EnableBMSInRegistry();
			if (shouldCreateBMSystem)
			{
				VisualBoardsTestHelper.CreateSystem(factory, "WKI");
			}

			if (shouldAddQualityIterationReason)
			{
				AddQualityIterationReason(reasonCode, reasonMessage);
			}

			if (shouldAddDatUser)
			{
				AddDatUser();
			}

			AddReleaseGroups(defaultSettingForReleaseGroupsNotSpecified);

			if (shouldAddTaskTypes)
			{
				AddCheckinTaskTypes();
			}

			factory.Save();
		}

		public static void AddQualityIterationReason(string reasonCode, string reasonMessage)
		{
			MasterFilesTestHelper.AddIterationReasonToRegistry(QualityIterationHelper.FailedQualityIterationRegistryCategoryCode, reasonCode, reasonMessage, true);
		}

		public void AddUser(string code, string loginName)
		{
			var user = factory.NewWithValidTestData<GlbStaff>();
			user.GS_Code = code;
			user.GS_LoginName = loginName;
		}

		public void AddDatUser()
		{
			AddUser("DAT", "Dat1");
		}

		void AddReleaseGroups(bool defaultSettingForReleaseGroupsNotSpecified)
		{
			if (AddReleaseGroup("YES") | AddReleaseGroup("NO") | AddReleaseGroup("NS"))
			{
				factory.Save();
			}

			var header = new QualityIterationAssignmentHeader(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), factory);
			header.IsDefaultOptionSelected = defaultSettingForReleaseGroupsNotSpecified;
			header.AddNewAssignment("YES", ZBool.True);
			header.AddNewAssignment("NO", ZBool.False);

			EDIDataRegistry.Instance.QualityIterationAssignments.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, header);
		}

		bool AddReleaseGroup(string code)
		{
			var wasReleaseGroupAdded = false;
			var releaseGroup = factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, SQLComparisonOperator.Equal, code));

			if (releaseGroup == null)
			{
				releaseGroup = factory.NewWithValidTestData<GlbGroup>();
				releaseGroup.GG_Code = code;
				wasReleaseGroupAdded = true;
			}

			releaseGroups.Add(code, releaseGroup.PK);

			return wasReleaseGroupAdded;
		}

		public void CreateQualityIteration(string learningTaskType = "LUP")
		{
			var creator = ObjectFactory.Get<IContainmentBarrierCreator>("IContainmentBarrierCreator");
			creator.QcbTaskNewStatus = ProcessTaskStatusCodeList.Codes.Cancelled;
			creator.QcbCreatingUserLoginName = @"Dat1";
			creator.TaskTypesToNotRepeat = new[] { learningTaskType };

			var isCheckinShelf = WorkItemProcessTask.IsCheckinTypeTask(ShelfTask.P9_Type);

			var iterateFromTaskPk = isCheckinShelf
				? creator.FindBestIterateFromTask(ShelfTask.PK, ContainmentBarrierIterateFromTaskSelectionMode.ExcludeDifferentResourceAsQcbTask | ContainmentBarrierIterateFromTaskSelectionMode.ExcludeContainmentBarrierTasks, QualityIterationHelper.GetEligibleIterateFromTaskTypesFromRegistry())
				: ShelfTask.PK;

			var qualityIterationHelper = new QualityIterationHelper(new QualityIterationInfo(ShelfTask, @"Shelf Iteration", "SHV", ShelfTask.P9_GS_NKAssignedStaffMember), factory, logMessage => new LoggerForTest().Log(LogType.Error, logMessage));
			var reasonPk = qualityIterationHelper.GetReasonPkForQualityIteration();
			IEnumerable<QualityIterationTaskDescriptor> QualityIterationTasksForNonCheckInShelves()
			{
				yield return QualityIterationHelper.ResolveSubmissionFailureTaskDescriptor(ShelfTask.P9_GS_NKAssignedStaffMember);
			}

			creator.CreateQualityIteration(ShelfTask.PK, iterateFromTaskPk, reasonPk, customQualityIterationTasks: isCheckinShelf ? null : QualityIterationTasksForNonCheckInShelves());
		}

		#endregion

		#region TaskManipulation

		static void AddCheckinTaskTypes()
		{
			var categorisedTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
			var taskTypes = categorisedTaskTypes.GetTaskTypesFromWorkflowCode(QualityIterationHelper.FailedQualityIterationRegistryCategoryCode);
			string[] typesToAdd = { "SH0", "SHV", "CHK", "CH0", "CH1", "CH2", "CH3", "CHB", "CHC", "CHG", "CHL", "CHX", "UA0", "UAT", "AS0", "ASP" };

			foreach (var taskType in typesToAdd)
			{
				AddTaskType(taskTypes, taskType);
			}

			AddTaskType(taskTypes, WorkItemProcessTask.CodeReviewTaskType, "Code Review");
			AddTaskType(taskTypes, "DEP", "Deploy", isContainmentBarrier: false);

			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categorisedTaskTypes);
		}

		static void AddTaskType(WorkflowTaskTypeCollection taskTypes, string code, string description = "Checkin to something", bool isContainmentBarrier = true)
		{
			var taskType = (WorkflowTaskType)taskTypes.FindByCode(code);
			if (taskType == null)
			{
				taskType = taskTypes.AddNew();
				taskType.Code = code;
				taskType.Description = (NoResString)description;
			}
			taskType.ContainmentBarrierIterationType = (isContainmentBarrier) ? ContainmentBarrierIterationTypeList.Codes.GLB : ContainmentBarrierIterationTypeList.Codes.NCB;
		}

		public IProcessHeader AddWorkflow(string releaseGroup, IProcessHeader workflowToBeDependentOn, string shelfTask, params string[] tasks)
		{
			var jobHeader = ProcessJobHeader.GetForParent(WorkItem, factory, addDefaultProcessHeaderIfNone: false);

			var workflow = (releaseGroup == null)
				? VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Silly Hats Only")
				: VisualBoardsTestHelper.CreateWorkflow(jobHeader, "Silly Hats Only", releaseGroupPK: releaseGroups[releaseGroup]);

			workflowToBeDependentOn?.GetOrCreateDependencyLink(workflow);

			if (shelfTask != null)
			{
				ShelfTask = AddTask(workflow, shelfTask.Split(','));
			}

			foreach (var taskCodes in tasks)
			{
				AddTask(workflow, taskCodes.Split(','));
			}

			return workflow;
		}

		public IProcessHeader AddChildWorkflow(IProcessHeader workflowOfParent, string childWorkflowName, string shelfTask, params string[] tasks)
		{
			var jobHeader = ProcessJobHeader.GetForParent(WorkItem, factory, addDefaultProcessHeaderIfNone: false);

			var workflow = VisualBoardsTestHelper.CreateWorkflow(jobHeader, childWorkflowName);

			workflow.GetOrCreateLinkToParent((ProcessHeader)workflowOfParent);

			if (shelfTask != null)
			{
				ShelfTask = AddTask(workflow, shelfTask.Split(','));
			}

			foreach (var taskCodes in tasks)
			{
				AddTask(workflow, taskCodes.Split(','));
			}

			return workflow;
		}

		public static void CreateQualityIterationLinks(IProcessTask containmentBarrierTask, IProcessHeader qualityIterationWorkflow)
		{
			CreateQualityIterationLinks(containmentBarrierTask, qualityIterationWorkflow.Tasks, qualityIterationWorkflow);
		}

		public static void CreateQualityIterationLinks(IProcessTask containmentBarrierTask, IEnumerable<IProcessTask> qualityIterationTasks, IProcessHeader qualityIterationWorkflow = null)
		{
			var factory = ((IBusiness)containmentBarrierTask).Factory;
			var iterationLink = factory.New<IProcessTaskIterationLink>();
			iterationLink.P9I_FH_IterationWorkflow = qualityIterationWorkflow?.PK ?? containmentBarrierTask.P9_FH_ProcessHeader;
			iterationLink.P9I_P9_ContainmentBarrierTask = containmentBarrierTask.PK;
			iterationLink.P9I_LinkType = IterationLinkTypeList.Codes.QualityIterationTask;
			iterationLink.P9I_Outcome = IterationLinkOutcomeList.Codes.IterationRequired;
			foreach (var task in qualityIterationTasks)
			{
				var taskPivot = factory.New<IProcessTaskIterationLinkPivot>();
				taskPivot.P9P_P9I_Iteration = iterationLink.PK;
				taskPivot.P9P_P9_Task = task.PK;
				taskPivot.P9P_ParentId = task.P9_ParentID;
				taskPivot.P9P_ParentTableCode = task.P9_ParentTableCode;
			}
		}

		public void RefreshWorkItemTasks()
		{
			WorkItemTasks = factory.Load<WorkItemProcessTask>(new ZQuery(ProcessTasksSchema.P9_ParentID, SQLComparisonOperator.Equal, WorkItem.PK));
		}

		public WorkItemProcessTask AddTask(IProcessHeader workflow, string[] codes)
		{
			var task = (WorkItemProcessTask)WorkItem.WorkflowItems.Tasks.AddNew();
			task.P9_FH_ProcessHeader = workflow.PK;
			task.P9_Type = codes[0];
			task.P9_GS_NKAssignedStaffMember = codes[1];
			task.P9_Status = codes[2];
			task.P9_Sequence = ZInt.Parse(codes[3]);

			if (codes.Length >= 5 && ZGuid.TryParse(codes[4], out var capabilityPK))
			{
				task.P9_G4_RequiredCapability = capabilityPK;
			}

			if (codes.Length.Equals(6))
			{
				task.P9_Description = codes[5];
			}

			return task;
		}

		public WorkItemProcessTask GetTaskBySequenceNumber(int sequence)
		{
			return WorkItemTasks.FirstOrDefault(x => x.P9_Sequence == sequence);
		}

		public string GetWorkflowCompletionStatement(WorkItemProcessTask task)
		{
			var workflow = factory.Load<ProcessHeader>(task.P9_FH_ProcessHeader);
			return workflow.FH_CompletionStatement;
		}

		#endregion

		#region Assertions

		public void AssertWorkItemTasks(int expected)
		{
			RefreshWorkItemTasks();
			Assertion.AssertEquals(Invariant($"The number of tasks for this work item is not correct: {System.Environment.NewLine}{GetTaskDetails()}"), expected, WorkItemTasks.Length);
		}

		public ProcessTask AssertTaskExists(int sequenceNumber, string taskType, string status)
		{
			return AssertTaskExists(
				Invariant($@"The expected task [{sequenceNumber} - {taskType} - {status}] was not found for the workitem in:
				{GetTaskDetails()}."),
				sequenceNumber,
				taskType,
				status);
		}

		public ProcessTask AssertTaskExists(string message, int sequenceNumber, string taskType, string status)
		{
			var task = WorkItemTasks.SingleOrDefault(x => x.P9_Sequence == sequenceNumber && x.P9_Type == taskType && x.P9_Status == status);
			Assertion.AssertNotNull(message, task);
			return task;
		}

		public ProcessTask AssertTaskExists(int sequenceNumber, string taskType, string status, ZGuid processHeader)
		{
			return AssertTaskExists(
				Invariant($@"The expected task [{sequenceNumber} - {taskType} - {status} - {processHeader}] was not found for the workitem in:
				{GetTaskDetails()}."),
				sequenceNumber,
				taskType,
				status,
				processHeader);
		}

		public ProcessTask AssertTaskExists(string message, int sequenceNumber, string taskType, string status, ZGuid processHeader)
		{
			var task = WorkItemTasks.SingleOrDefault(x => x.P9_Sequence == sequenceNumber && x.P9_Type == taskType && x.P9_Status == status && x.P9_FH_ProcessHeader == processHeader);
			Assertion.AssertNotNull(message, task);
			return task;
		}

		string GetTaskDetails() => GetTaskDetails(WorkItemTasks);

		public static string GetTaskDetails(IEnumerable<ProcessTask> tasks) => string.Join(System.Environment.NewLine, tasks.OrderBy(t => t.P9_Sequence).Select(t => (t.P9_Sequence, t.P9_Type, t.P9_Status, t.P9_Description, t.P9_FH_ProcessHeader).ToString()));

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		public void AssertTaskDetails(ProcessTask task, string type, string status, string description, string assignedStaffMember, int lowEstimateMinutes, GlbCapability requiredCapability)
		{
			Assertion.CombineAssertions(() =>
			{
				Assertion.AssertEquals(nameof(task.P9_Type), type, task.P9_Type);
				Assertion.AssertEquals(nameof(task.P9_Status), status, task.P9_Status);
				Assertion.AssertEquals(nameof(task.P9_Description), description, task.P9_Description);
				Assertion.AssertEquals(nameof(task.P9_GS_NKAssignedStaffMember), assignedStaffMember, task.P9_GS_NKAssignedStaffMember);
				Assertion.AssertEquals(nameof(task.LowEstimatedDurationHours), lowEstimateMinutes, (int)Math.Round(task.LowEstimatedDurationHours * 60));
				Assertion.AssertEquals(nameof(task.RequiredCapability), requiredCapability, task.RequiredCapability);
			});
		}

		#endregion
	}
}
