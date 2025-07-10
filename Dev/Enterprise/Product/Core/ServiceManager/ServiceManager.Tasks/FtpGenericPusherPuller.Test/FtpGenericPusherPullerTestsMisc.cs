using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.FTP.Testing
{
	[TestedType(typeof(FtpGenericPusherPullerServiceTask))]
	sealed class FtpGenericPusherPullerTestsMisc : ServiceTaskTestCase<FtpGenericPusherPullerServiceTask>
	{
		public void TestMinimumPeriod()
		{
			AssertEquals("5minutes", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		public void TestMissingLocalFolder()
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpUploadConfigs(tempLocalDir.DirectoryName + @"\This sucks", Factory, FtpClobberingOptions.Codes.Clobber);
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				AssertContains("environmental data not valid", log[0]);
			}
		}

		[TestDate(1986, 3, 12, 1, 2, 3)]
		public void TestRuntimes()
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpUploadConfigs(tempLocalDir.DirectoryName + @"\This sucks", Factory, FtpClobberingOptions.Codes.Clobber);
				Factory.Save();
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				AssertEquals(1, log.Count);
				var configCollection = FtpRegistry.Instance.Profiles.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
				var profile = configCollection[0];
				AssertEquals(profile.LastRunUtc, new ZDateTime(1986, 3, 12, 1, 2, 3));
				log = InitialiseAndRunTaskSchedule(ftpTask);
				AssertEquals(1, log.Count);  // did not run again
				AssertEquals(profile.LastRunUtc, new ZDateTime(1986, 3, 12, 1, 2, 3));
			}
		}

		public void TestMultipleProfilesAndEmailExceptionHandling()
		{
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpConfigs(tempLocalDir.DirectoryName, Factory, "EXC", FtpClobberingOptions.Codes.Clobber, emailAddress: "test@MainCompany.com");
				helper.SetUpConfigs(tempLocalDir.DirectoryName, Factory, "EXC", FtpClobberingOptions.Codes.Clobber, emailAddress: "test@Second.com");
				Factory.Save();
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				var log = InitialiseAndRunTaskSchedule(ftpTask);
				var emailProfile1 = Env.OutgoingMailManager.EmailsCreated.Find(e => e.Recipients.RecipientsAsDelimitedString().Contains("test@MainCompany.com"));
				var emailProfile2 = Env.OutgoingMailManager.EmailsCreated.Find(e => e.Recipients.RecipientsAsDelimitedString().Contains("test@Second.com"));
				AssertNotNull(emailProfile1);
				AssertNotNull(emailProfile2);
				foreach (var e in new EmailDef[] { emailProfile1, emailProfile2 })
				{
					AssertContains("Exception One", e.Body);
					AssertContains("Exception Two", e.Body);
					AssertContains("Exception while running profile p123", e.Subject);
					AssertContains("CargoWise FTP Engine", e.Subject);
				}
			}
		}

		public void TestCanRunInAnyBranch()
		{
			ErrorReporter.Clear();
			using (var tempLocalDir = new TempDirectory())
			{
				helper.SetUpConfigs(tempLocalDir.DirectoryName, Factory, "EXC", FtpClobberingOptions.Codes.Clobber, emailAddress: "test@MainCompany.com");
				Factory.Save();
				var ftpTask = new FtpGenericPusherPullerServiceTask();
				InitialiseAndRunTaskSchedule(ftpTask);

				AssertEquals("Error should not be reported, we should set a user context", 0, ErrorReporter.TotalErrorCount);
				AssertNotContains("Service Task: FTP accesses environment current branch without setting the environment first.", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		protected override void SetUpCore()
		{
			base.SetUpCore();
			helper = new Helper();
			helper.Start();
		}
		protected override void TearDownCore()
		{
			base.TearDownCore();
			helper.Dispose();
		}

		Helper helper;
	}
}
