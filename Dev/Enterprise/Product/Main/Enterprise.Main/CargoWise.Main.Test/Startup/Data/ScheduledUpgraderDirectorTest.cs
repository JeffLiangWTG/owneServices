using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DbUpgrader.Shared;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Upgrades;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Shared.CW1.Test;

namespace Enterprise.Startup.Testing
{
	sealed class ScheduledUpgraderDirectorTest : TransactionedTestCase
	{
		public void TestServiceManagerRunnerCw1ExeIsDetectableByLoginForUpgrade()
		{
			const string serviceManagerRunnerCw1Exe = "CargoWiseOne.ServiceManager.Runner.AnyCPU.exe";

			Assert(serviceManagerRunnerCw1Exe.Contains(ScheduledUpgraderDirector.ServiceManagerRunnerName));
		}

		public void TestServiceManagerRunnerExeIsDetectableByLoginForUpgrade()
		{
			Assert(ServiceManagerConstants.ServiceManagerRunnerExe.Contains(ScheduledUpgraderDirector.ServiceManagerRunnerName));
		}

		public void TestEmail()
		{
			var factory = new BusinessObjectFactory();
			GlbGroup nEWGROUP = factory.NewWithValidTestData<GlbGroup>();
			nEWGROUP.GG_Code = "NEW";
			GlbGroup pMG = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			GlbStaff postMaster = pMG.Staff.AddNew();
			postMaster.GS_EmailAddress = "a@x.com";
			factory.Save();

			var currentEmails = EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count;

			var director = new ScheduledUpgraderDirectorForTest(true, pMG.PK.ToString());

			director.HandleSuccessNotificationEmail();
			AssertEquals(currentEmails + 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			var lastEmail = EnvProxy.Instance.OutgoingMailManager.EmailsCreated[currentEmails];
			AssertEquals("Upgrade to version (none) success notification", lastEmail.Subject);
			Assert(lastEmail.Body, lastEmail.Body.StartsWith($@"Upgrade to version (none) finished successfully.
	DB Server name:	{Db.ServerName}
	Database name:	{Db.DatabaseName}
	Upgrade time:	", StringComparison.InvariantCulture));

			AssertEquals(1, lastEmail.Recipients.Count);
			AssertEquals("a@x.com", lastEmail.Recipients[0].Email);

			director = new ScheduledUpgraderDirectorForTest(false, pMG.PK.ToString());
			director.HandleSuccessNotificationEmail();
			AssertEquals("No email sent if success and NotifyOnSuccess = false", currentEmails + 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestEmailExceptionIsLogged()
		{
			var logs = new List<string>();

			var logger = new Mock<ILogger>();
			logger.Setup(o => o.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string message) => logs.Add(message));

			var factory = new BusinessObjectFactory();
			var noEmailGroup = factory.NewWithValidTestData<GlbGroup>();
			noEmailGroup.GG_Code = "NEG";
			factory.Save();

			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				// group with no email
				var director = new ScheduledUpgraderDirectorForTest(true, noEmailGroup.PK.ToString(), logger.Object);
				director.HandleSuccessNotificationEmail();
				AssertEquals("No email sent + no exception if no recipients", 0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);

				var expectedLogMessage = $"Failure to send upgrade email: Could not find a recipient for group (PK = {noEmailGroup.PK}, Code = {noEmailGroup.GG_Code}). Reason: No staff members were found in the staff group.";
				AssertEndsWith("Should log", expectedLogMessage, logs.Last());

				// Non exist group
				var randomPK = Guid.NewGuid();
				director = new ScheduledUpgraderDirectorForTest(true, randomPK.ToString(), logger.Object);
				director.HandleSuccessNotificationEmail();
				AssertEquals("No email sent + no exception if no recipients", 0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);

				expectedLogMessage = $"Failure to send upgrade email: Could not find a recipient for group (PK = {randomPK}, Code = (Unknown)). Reason: No staff members were found in the staff group.";
				AssertEndsWith("Should log", expectedLogMessage, logs.Last());

				var newFactories = PersistentFactoryCacheManager.Instance.TrackedFactories_ForTest.Where(f => f.BusinessObjectsInformation.Contains(GlbGroupSchema.Constants.TableName));
				AssertEquals("Should not use factory for GlbGroup", 0, newFactories.Count());
			}
		}

		public void TestLogsAsItGoes()
		{
			var logs = new List<string>();

			var logger = new Mock<ILogger>();
			logger.Setup(o => o.Log(It.IsAny<LogType>(), It.IsAny<string>()))
				.Callback((LogType logType, string message) => logs.Add(message));

			SystemDataRegistry.Instance.ProcessControllerNLogInternalLoggingEnabled.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var mockUpgradeRunner = new MockDbUpgradeRunner();
			ObjectFactory.Substitute<IDbUpgraderRunner>(mockUpgradeRunner);

			var upgradeThread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var director = new ScheduledUpgraderDirectorForTest(true, null, logger.Object);
					director.DoUpgrade();
				}
			});

			upgradeThread.Start();
			mockUpgradeRunner.UpgradeStartedEvent.WaitOne();

			try
			{
				Assert("Log file should contain first unique message", logs.Last().Contains(mockUpgradeRunner.FirstUniqueMessage));
			}
			finally
			{
				mockUpgradeRunner.FinishUpgradeEvent.Set();
			}
			upgradeThread.Join();

			Assert("Log file should contain first unique message", logs.ElementAt(logs.Count - 2).Contains(mockUpgradeRunner.FirstUniqueMessage));
			Assert("Log file should contain second unique message", logs.Last().Contains(mockUpgradeRunner.SecondUniqueMessage));
		}

		[ExpectNoExceptions()] // Mock.Verify()
		public void TestTrackServiceTaskErrorNotCalled()
		{
			var director = new ScheduledUpgraderDirectorForTest(true, logger: Mock.Of<ILogger>());
			var mockServiceTaskScheduleErrorTracker = new Mock<IServiceTaskErrorTracker>();
			ObjectFactory.Substitute(mockServiceTaskScheduleErrorTracker.Object);
			ObjectFactory.Substitute<IDbUpgraderRunner>(new MockDbUpgradeRunnerWithError());
			director.DoUpgrade();
			mockServiceTaskScheduleErrorTracker.Verify(o => o.TrackServiceTaskError(It.IsAny<string>()), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestSchemaVersionCheckIsDisabledWhenInitializeScheduledUpgradeLogger()
		{
			//Arrange
			var actualTransformationVersion = DataRegistry.Instance.DatabaseMajorTransformationVersion;
			DataRegistry.Instance.DatabaseMajorTransformationVersion = actualTransformationVersion - 1;
			var arguments = new ApplicationArguments(new[] { ApplicationArguments.OptionUpgrade + Guid.NewGuid(), ApplicationArguments.OptionScheduledDbUpgrader, ApplicationArguments.OptionServiceProcess });
			var director = new ScheduledUpgraderDirector(false, null);

			//Act
			var result = director.Execute(arguments);

			//Assert
			AssertEquals(false, result);
		}

		[ExpectNoExceptions]
		public void TestLoggerWillLogWhenCheckSoftwareUpgradeFailed()
		{
			//Arrange
			var logger = new Mock<ILogger>();
			var actualTransformationVersion = DataRegistry.Instance.DatabaseMajorTransformationVersion;
			DataRegistry.Instance.DatabaseMajorTransformationVersion = actualTransformationVersion - 1;
			var arguments = new ApplicationArguments(new[] { ApplicationArguments.OptionUpgrade + Guid.NewGuid(), ApplicationArguments.OptionScheduledDbUpgrader, ApplicationArguments.OptionServiceProcess });
			var director = new ScheduledUpgraderDirectorForTest(true, logger: logger.Object);

			//Act
			var result = director.Execute(arguments);

			//Assert
			AssertEquals(false, result);
			logger.Verify(l => l.Log(LogType.Warning, "Software Upgrade Failure - The requrested upgrade package cannot be found."), Times.Once);
		}

		public class ScheduledUpgraderDirectorForTest : ScheduledUpgraderDirector
		{
			public ScheduledUpgraderDirectorForTest(bool notifyOnSuccess = false, string notificationGroupPK = null, ILogger logger = null)
			: base(notifyOnSuccess, notificationGroupPK, () => logger)
			{
			}

			public new void DoUpgrade()
			{
				base.DoUpgrade();
			}
		}

		public class MockDbUpgradeRunner : IDbUpgraderRunner
		{
			public ValidationResponse FullSilentUpgrade(IVersionChangeInfo versionInfo, UpgradeInfo softwareUpgrade, UpgraderEvent upgraderEvent)
			{
				upgraderEvent(UpgradeEventType.InfoMessage, FirstUniqueMessage);
				UpgradeStartedEvent.Set();
				FinishUpgradeEvent.WaitOne();
				upgraderEvent(UpgradeEventType.InfoMessage, SecondUniqueMessage);
				var result = new ValidationResponse();
				result.Successful = true;
				return result;
			}

			public readonly ManualResetEvent UpgradeStartedEvent = new ManualResetEvent(false);
			public readonly ManualResetEvent FinishUpgradeEvent = new ManualResetEvent(false);

			public ValidationResponse FullUpgrade(IVersionChangeInfo versionInfo, UpgradeInfo softwareUpgrade, UpgraderEvent upgradeEventHandler, Func<BaseUpgradeManager, bool> runUpgradeGui)
			{
				throw new NotImplementedException();
			}

			public void RemoveOldUpgradePackages()
			{
				throw new NotImplementedException();
			}

			public readonly string FirstUniqueMessage = Guid.NewGuid().ToString();
			public readonly string SecondUniqueMessage = Guid.NewGuid().ToString();
		}

		public class MockDbUpgradeRunnerWithError : IDbUpgraderRunner
		{
			public ValidationResponse FullSilentUpgrade(IVersionChangeInfo versionInfo, UpgradeInfo softwareUpgrade, UpgraderEvent upgradeEventHandler)
			{
				upgradeEventHandler(UpgradeEventType.TaskFailed, "Task failed");
				return new ValidationResponse() { Successful = false, Information = "Failure" };
			}

			public ValidationResponse FullUpgrade(IVersionChangeInfo versionInfo, UpgradeInfo softwareUpgrade, UpgraderEvent upgradeEventHandler, Func<BaseUpgradeManager, bool> runUpgradeGui)
			{
				throw new NotImplementedException();
			}

			public void RemoveOldUpgradePackages()
			{
				throw new NotImplementedException();
			}
		}
	}
}
