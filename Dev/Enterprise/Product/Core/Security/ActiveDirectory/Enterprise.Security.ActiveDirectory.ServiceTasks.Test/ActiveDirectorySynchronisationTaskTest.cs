using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.InteropServices;
using CargoWise.ActiveDirectory;
using CargoWise.ActiveDirectory.TestFramework;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.ActiveDirectory.ServiceTasks;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Security.ActiveDirectory.Test
{
	class ActiveDirectorySynchronisationTaskTest : TestCaseWithFactoryAndMocks
	{
		public void TestHostedServiceAttribute()
		{
			Type serviceTaskType = typeof(ActiveDirectorySynchronisationTask);
			var hostedServiceAttributes = (from HostedServiceAttribute x in serviceTaskType.Assembly.GetCustomAttributes(typeof(HostedServiceAttribute), inherit: false)
										   where x.TypeName.Equals(serviceTaskType.FullName, StringComparison.Ordinal)
										   select x).ToArray();

			AssertEquals("Expected single attribute", 1, hostedServiceAttributes.Length);
			var hostedServiceAttribute = hostedServiceAttributes.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Code", "ADS", hostedServiceAttribute.Code);
				AssertEquals("Description", "Active Directory Synchronizer", hostedServiceAttribute.Description);
				AssertEquals("Category", "SYS", hostedServiceAttribute.Category);
				AssertEquals("AllowsMultipleInstances", false, hostedServiceAttribute.AllowsMultipleInstances);
				AssertEquals("IsMandatory", false, hostedServiceAttribute.IsMandatory);
				AssertEquals("MinimumPeriod", "1day", hostedServiceAttribute.MinimumPeriod);
				AssertEquals("CanRunInAnyBranch", true, hostedServiceAttribute.CanRunInAnyBranch);
				AssertEquals("DefaultScheduleRunEvery", "1day", hostedServiceAttribute.DefaultScheduleRunEvery);
				AssertEquals("DefaultScheduleStartAtLocal", "2hours", hostedServiceAttribute.DefaultScheduleStartAtLocal);
				Assert("ActiveByDefault", hostedServiceAttribute.ActiveByDefault);
			});
		}

		public void TestReportActivationChanged_Success()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;

			var directoryEntry = DummyDirectoryEntryWrapper.CreateUser("lord.sauron");
			directoryEntry.SetLastModified(ZDateTime.UtcNow.AddMinutes(-1).ToDateTime());

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "sauron";
			staff.GS_ActiveDirectoryObjectGuid = directoryEntry.Guid;
			staff.GS_IsActive = false;
			Factory.Save();

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(directoryEntry.Guid, "root/Accounts/ADUnitTesting")).Returns(directoryEntry);

			AssertEquals("Before Sync: AD User is acitve", true, directoryEntry.IsActive);

			var logger = new SimpleLogger();
			var task = new ActiveDirectorySynchronisationTask { ServiceLogger = logger };
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = DirectorySearcherProviderSubstitution.DirectorySearcherMock.Object;
			task.RunTask();

			AssertEquals("AD user deactivated", false, directoryEntry.IsActive);
			AssertContains(@"Warning: 100% Activation status changed - Staff 'sauron' has been deactivated in CW1 and successfully deactivated in AD.", logger.ToString());
			AssertNullOrEmpty(ErrorReporter.LastMessageReported);
		}

		public void TestReportActivationChanged_Failed()
		{
			EnvProxy.SetHostedLocationForTest("ABC");
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;

			var directoryEntryMock = new Mock<IUserDirectoryEntry>();
			directoryEntryMock.As<IDirectoryEntry>().SetupGet(x => x.IsActive).Returns(true);
			directoryEntryMock.SetupGet(x => x.CanUpdate).Returns(true);
			directoryEntryMock.SetupGet(x => x.LastModified).Returns(ZDateTime.UtcNow.AddMinutes(-1).ToDateTime());
			directoryEntryMock.SetupGet(x => x.Guid).Returns(Guid.NewGuid());

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "sauron";
			staff.GS_ActiveDirectoryObjectGuid = directoryEntryMock.Object.Guid;
			staff.GS_IsActive = false;
			Factory.Save();

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), "root/Accounts/ADUnitTesting")).Returns(directoryEntryMock.Object);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, "root/Accounts/ADUnitTesting")).Returns(directoryEntryMock.Object);

			var logger = new SimpleLogger();
			var task = new ActiveDirectorySynchronisationTask { ServiceLogger = logger };
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = DirectorySearcherProviderSubstitution.DirectorySearcherMock.Object;
			task.RunTask();

			var expectedMessage = "Staff 'sauron' has been deactivated in CW1, but it is still active in AD.";
			AssertContains(@"Activation status changed - " + expectedMessage, logger.ToString());
			AssertContains(expectedMessage, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestReportActivationChanged_ADException()
		{
			EnvProxy.SetHostedLocationForTest("ABC");
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;

			var exception = new DirectoryServicesException("Could not locate or write to directory entry for entity 'sauron'");

			var directoryEntryMock = new Mock<IUserDirectoryEntry>();
			directoryEntryMock.As<IDirectoryEntry>().SetupSequence(x => x.IsActive).Returns(true).Throws(exception); // return exeption only in reporting call (second call)

			directoryEntryMock.SetupGet(x => x.CanUpdate).Returns(true);
			directoryEntryMock.SetupGet(x => x.LastModified).Returns(ZDateTime.UtcNow.AddMinutes(-1).ToDateTime());
			directoryEntryMock.SetupGet(x => x.Guid).Returns(Guid.NewGuid());

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "sauron";
			staff.GS_ActiveDirectoryObjectGuid = Guid.NewGuid();
			staff.GS_IsActive = false;
			Factory.Save();

			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), "root/Accounts/ADUnitTesting")).Returns(directoryEntryMock.Object);
			DirectorySearcherProviderSubstitution.DirectorySearcherMock.Setup(s => s.FindUser(staff.GS_LoginName, "root/Accounts/ADUnitTesting")).Returns(directoryEntryMock.Object);

			var logger = new SimpleLogger();
			var task = new ActiveDirectorySynchronisationTask { ServiceLogger = logger };
			DirectorySearcherFactory.DirectorySearcherOverride_ForTest = DirectorySearcherProviderSubstitution.DirectorySearcherMock.Object;
			task.RunTask();

			var expectedMessage = @$"Staff 'sauron' has been deactivated in CW1, and an exception happened when trying to retrieve the new AD value:";
			AssertContains("Activation status changed - " + expectedMessage, logger.ToString());
			AssertContains(exception.Message, logger.ToString());

			AssertContains(expectedMessage, ErrorReporter.LastMessageReported);
			AssertContains(exception.Message, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestShouldLogError_NonAccessibleDomain()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			//Add a linked staff so sync could run
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "lord.sauron";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			using (ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.DataType.SuspendValidation())
			{
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection(domainName: "FakeNews", domainUserName: "Sean Spicer", domainUserPassword: "Sean Spider"));
			}

			var logger = new SimpleLogger();
			var task = new ActiveDirectorySynchronisationTask { ServiceLogger = logger };

			DirectorySearcherProviderSubstitution.DoNotMock = true;
			task.RunTask();

#if NET48
			AssertContains(@"Error: Error synchronising directory objects:

Could not perform the action on domain fakenews.
The server is not operational.

Please contact your system administrator.
Active Directory Synchronizer finished", logger.ToString());
#else
			AssertContains(@"Error: Error synchronising directory objects:

Could not perform the action on domain fakenews.
The server is not operational.
Please contact your system administrator.
Active Directory Synchronizer finished", logger.ToString());
#endif

		}

		public void TestShouldLogError_NonAccessibleOU()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			//Add a linked staff so sync could run
			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "lord.sauron";
			staff.GS_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			using (ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.DataType.SuspendValidation())
			{
				ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection(domainName: TestConstants.Domain, domainUserName: TestConstants.ADTestUserAccountNoOURight.NameWithDomainPreWindows2000, domainUserPassword: TestConstants.ADTestUserAccountNoOURight.Password));
			}

			var logger = new SimpleLogger();
			var task = new ActiveDirectorySynchronisationTask { ServiceLogger = logger };

			DirectorySearcherProviderSubstitution.DoNotMock = true;
			task.RunTask();

			AssertContains(@"Error: Error synchronising directory objects:

Could not perform the action on domain sand.wtg.zone.
Domain user does not have write privileges to the Organizational Units for domain sand.wtg.zone set in the registry 'System -> Staff -> Active Directory -> Domain Credentials Collection'
Please contact your system administrator.
Active Directory Synchronizer finished", logger.ToString());
		}

		public void TestShouldLogError_NoDomainPrivilegeException()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var logger = new SimpleLogger();
			var task = new ActiveDirectorySynchronisationTask { ServiceLogger = logger };
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;

			syncer.Setup(s => s.Save()).Throws(new NoDomainPrivilegeException("Sync to RPA fail"));
			task.RunTask();
			AssertContains(@"Error: Error synchronising directory objects:

Sync to RPA fail
Active Directory Synchronizer finished", logger.ToString());
		}

		public void TestShouldRethrow_UnauthorizedAccessException()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var task = new ActiveDirectorySynchronisationTask();
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;
			task.Logger = new Mock<ILogger>().Object;

			syncer.Setup(s => s.Save()).Throws(new UnauthorizedAccessException("Access denied..."));

			AssertExceptionThrown<HostedServiceException>(() => task.RunTask());
		}

		public void TestShouldRethrow_UnhandledException()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var task = new ActiveDirectorySynchronisationTask();
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;
			task.Logger = new Mock<ILogger>().Object;

			syncer.Setup(s => s.Save()).Throws(new Exception("Unhandled exception..."));

			AssertExceptionThrown<Exception>(() => task.RunTask());
		}

		[ExpectNoExceptions]
		public void TestShouldLogError_InvalidOUException()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var task = new ActiveDirectorySynchronisationTask();
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;
			var loggerMock = new Mock<ILogger>();
			task.Logger = loggerMock.Object;

			syncer.Setup(a => a.Synchronise(It.IsAny<EntitiesToSync?>(), It.IsAny<SyncMode?>())).Throws(new InvalidOUException(TestConstants.InvalidOU));

			task.RunTask();

			loggerMock.Verify(l => l.Log(LogType.Error, It.Is<string>(m => m.StartsWith($"The Organizational Unit {TestConstants.InvalidOU} is invalid"))), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestRunTask_ADEnabled_ShouldSyncAndSave()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var task = new ActiveDirectorySynchronisationTask();
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;
			var loggerMock = new Mock<ILogger>();
			task.Logger = loggerMock.Object;

			task.RunTask();

			syncer.Verify(s => s.Synchronise(null, null));
			syncer.Verify(s => s.Save());
			loggerMock.Verify(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Exactly(2));
		}

		public void TestRunTask_ADEnabled_ShouldSyncAndSave_WithOneOffSyncMode_AD()
		{
			AssertRunTask_ADEnabled_ShouldSyncAndSave_WithOneOffSyncMode(SyncModeList.Codes.ActiveDirectory, SyncMode.ADIsMaster);
		}

		public void TestRunTask_ADEnabled_ShouldSyncAndSave_WithOneOffSyncMode_CW1()
		{
			AssertRunTask_ADEnabled_ShouldSyncAndSave_WithOneOffSyncMode(SyncModeList.Codes.CargoWise, SyncMode.EnterpriseIsMaster);
		}

		public void TestRunTask_ADEnabled_ShouldSyncAndSave_WithOneOffSyncMode_Empty()
		{
			AssertRunTask_ADEnabled_ShouldSyncAndSave_WithOneOffSyncMode(string.Empty, null);
		}

		void AssertRunTask_ADEnabled_ShouldSyncAndSave_WithOneOffSyncMode(string oneOffSyncMode, SyncMode? expectedSyncMode)
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.OneOffSyncMode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oneOffSyncMode);

			var task = new ActiveDirectorySynchronisationTask();
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;
			var loggerMock = new Mock<ILogger>();
			task.Logger = loggerMock.Object;

			AssertEquals(oneOffSyncMode, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);
			task.RunTask();
			AssertEquals("OneOffSyncMode should be reset after the task has run", string.Empty, ActiveDirectoryRegistry.Instance.OneOffSyncMode.Value);

			syncer.Verify(s => s.Synchronise(null, expectedSyncMode), Times.Once);
			syncer.Verify(s => s.Save(), Times.Once);
			loggerMock.Verify(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Exactly(2));

			syncer.Reset();
			task.Synchroniser = syncer.Object;
			loggerMock.Reset();
			task.Logger = loggerMock.Object;

			task.RunTask();

			syncer.Verify(s => s.Synchronise(null, null), Times.Once);
			syncer.Verify(s => s.Save(), Times.Once());
			loggerMock.Verify(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Exactly(2));
		}

		[ExpectNoExceptions]
		public void TestRunTask_ADEnabled_ShouldSubscribeToProgressUpdates()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var task = new ActiveDirectorySynchronisationTask();
			task.Logger = new Mock<ILogger>().Object;
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;

			task.RunTask();

			syncer.VerifyAdd(s => s.ProgressUpdated += task.UpdateLog);
			syncer.VerifyRemove(s => s.ProgressUpdated -= task.UpdateLog);
		}

		public void TestHostedServiceRequirement()
		{
			var methodInfo = typeof(ActiveDirectorySynchronisationTask).GetMethod(nameof(ActiveDirectorySynchronisationTask.CheckIsADIntegrationEnabled));
			Assert("HostedServiceRequirement for ActiveDirectory is applied", Attribute.IsDefined(methodInfo, typeof(HostedServiceRequirementAttribute)));
		}

		public void TestRunTask_ADDisabled_ShouldNotRun()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = false;
			AssertEquals("Active Directory Synchronizer not started, Active Directory integration is disabled", ActiveDirectorySynchronisationTask.CheckIsADIntegrationEnabled());
		}

		[ExpectNoExceptions]
		public void TestRunTask_ADDisabled_ShouldNotSync()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = false;

			var task = new ActiveDirectorySynchronisationTask();
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;
			var loggerMock = new Mock<ILogger>();
			task.Logger = loggerMock.Object;

			task.RunTask();

			syncer.Verify(s => s.Synchronise(null, null), Times.Never);
			loggerMock.Verify(l => l.Log(LogType.Information, "Active Directory Synchronizer not started, Active Directory integration is disabled"), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestShouldLogError_DirectoryServicesCOMException()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var task = new ActiveDirectorySynchronisationTask();
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;
			var loggerMock = new Mock<ILogger>();
			task.Logger = loggerMock.Object;

			syncer.Setup(s => s.Synchronise(null, null)).Throws(new DirectoryServicesCOMException());

			task.RunTask();

			loggerMock.Verify(l => l.Log(LogType.Error, It.Is<string>(s => s.StartsWith("Error synchronising directory objects:"))), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestShouldLogError_ActiveDirectoryUserException()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var task = new ActiveDirectorySynchronisationTask();
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;
			var loggerMock = new Mock<ILogger>();
			task.Logger = loggerMock.Object;
			var exception = new Mock<ActiveDirectoryUserException>();
			exception.SetupGet(ex => ex.Message).Returns("user exception occurred");

			syncer.Setup(s => s.Synchronise(null, null)).Throws(exception.Object);

			task.RunTask();

			loggerMock.Verify(l => l.Log(LogType.Error, It.Is<string>(s => s.StartsWith("Error synchronising directory objects:") && s.Contains("user exception occurred"))), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestShouldLogError_ZSaveException()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var row = ((INeedRow)Factory.New<GlbStaff>()).Row;
			var connection = ((IDbConnected)Factory).Connection;

			var task = new ActiveDirectorySynchronisationTask();
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;
			var loggerMock = new Mock<ILogger>();
			task.Logger = loggerMock.Object;

			syncer.Setup(s => s.Save()).Throws(new ZSaveException(new ZDataException(new Exception(), row, connection), Factory));

			task.RunTask();

			syncer.Verify(s => s.Synchronise(null, null), Times.Once);
			loggerMock.Verify(l => l.Log(LogType.Error, It.Is<string>(s => s.StartsWith("Error saving after synchronise:"))), Times.Once);
		}

		public void TestRunTaskTwice_ShouldNotReuseSameSynchroniser()
		{
			Helper.PurgeAllGroups();
			Helper.PurgeAllStaff();

			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;

			var task = new ActiveDirectorySynchronisationTask();
			task.Logger = new Mock<ILogger>().Object;
			task.RunTask();
			AssertNull("Should have reset synchroniser after run to release resources", task.Synchroniser);
		}

		public void TestShouldLogError_ZCannotSaveException()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			var task = new ActiveDirectorySynchronisationTask();
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;
			var loggerMock = new Mock<ILogger>();
			task.Logger = loggerMock.Object;

			syncer.Setup(s => s.Save()).Throws(new ZCannotSaveException("You are attempting to affect the last active non operational or controller staff member.", "Heading 1"));

			AssertNoExceptionThrown("No exception is thrown out", task.RunTask);

			syncer.Verify(s => s.Synchronise(null, null), Times.Once);
			loggerMock.Verify(l => l.Log(LogType.Error, It.Is<string>(s => s.StartsWith("Error synchronising directory objects:\r\n\r\nYou are attempting to affect the last active non operational or controller staff member."))), Times.Once);
		}

		[TestDate]
		public void TestRunTask_ADEnabled_ShouldRecordLastSyncDetails()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.DefaultValue);
			ActiveDirectoryRegistry.Instance.LastFailedSyncStaffPKs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<Guid>());
			ActiveDirectoryRegistry.Instance.LastFailedSyncGroupPKs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<Guid>());

			var task = new ActiveDirectorySynchronisationTask();
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;
			var loggerMock = new Mock<ILogger>();
			task.Logger = loggerMock.Object;

			var failedStaff = Factory.NewWithValidTestData<GlbStaff>();
			var failedGroup = Factory.NewWithValidTestData<GlbGroup>();

			TestDateAttribute.Date = new DateTime(2020, 2, 26, 14, 07, 05);
			var startTime = ZDateTime.UtcNow;

			syncer.Setup(s => s.Synchronise(null, null)).Callback(() =>
			{
				// pretend the sync take 30 minutes to complete
				TestDateAttribute.Date = startTime.AddMinutes(30).ToDateTime();
			});
			syncer.Setup(s => s.EntitiesWithErrors).Returns(new List<IADEntity>() { new ADUser(failedStaff), new ADGroup(failedGroup) });

			task.RunTask();

			syncer.Verify(s => s.Synchronise(null, null));
			syncer.Verify(s => s.Save());
			loggerMock.Verify(l => l.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Exactly(2));

			AssertNotEquals("Ensure sync is called", ZDateTime.UtcNow, startTime);
			AssertEquals("Should record LastSuccessfulSyncUTC from start time", startTime, ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value);
			AssertEquals("Should record LastFailedSyncStaffPKs", 1, ActiveDirectoryRegistry.Instance.LastFailedSyncStaffPKs.Value.Length);
			AssertEquals("Should record LastFailedSyncGroupPKs", 1, ActiveDirectoryRegistry.Instance.LastFailedSyncGroupPKs.Value.Length);
			AssertEquals("Should record failedStaff.PK to LastFailedSyncStaffPKs", failedStaff.PK, ActiveDirectoryRegistry.Instance.LastFailedSyncStaffPKs.Value[0]);
			AssertEquals("Should record failedGroup.PK to LastFailedSyncGroupPKs", failedGroup.PK, ActiveDirectoryRegistry.Instance.LastFailedSyncGroupPKs.Value[0]);
		}

		public void TestRunTask_ADEnabled_ShouldNotRecordLastSyncDetails_SyncThrowsException()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			var lastSyncTime = new DateTime(2018, 5, 3, 22, 33, 44);
			ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, lastSyncTime);
			ActiveDirectoryRegistry.Instance.LastFailedSyncStaffPKs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<Guid>());
			ActiveDirectoryRegistry.Instance.LastFailedSyncGroupPKs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<Guid>());

			var task = new ActiveDirectorySynchronisationTask();
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;
			task.Logger = new Mock<ILogger>().Object;

			syncer.Setup(s => s.Synchronise(null, null)).Throws(new COMException("Anthing"));
			syncer.Setup(s => s.EntitiesWithErrors).Returns(new List<IADEntity>() { new Mock<IADEntity>().Object, new Mock<IADEntity>().Object });

			task.RunTask();

			syncer.Verify(s => s.Save(), Times.Never);

			AssertEquals("Should not record LastSuccessfulSyncUTC", lastSyncTime, ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value);
			AssertEquals("Should not record LastFailedSyncStaffPKs", 0, ActiveDirectoryRegistry.Instance.LastFailedSyncStaffPKs.Value.Length);
			AssertEquals("Should not record LastFailedSyncGroupPKs", 0, ActiveDirectoryRegistry.Instance.LastFailedSyncGroupPKs.Value.Length);
		}

		[TestDate]
		public void TestRunTask_ADEnabled_ShouldNotRecordLastSyncDetails_SaveThrowsException()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			var oldSyncTime = new DateTime(2018, 5, 3, 22, 33, 44);
			ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldSyncTime);
			ActiveDirectoryRegistry.Instance.LastFailedSyncStaffPKs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<Guid>());
			ActiveDirectoryRegistry.Instance.LastFailedSyncGroupPKs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<Guid>());

			var task = new ActiveDirectorySynchronisationTask();
			var syncer = new Mock<IEntitySynchroniser>();
			task.Synchroniser = syncer.Object;
			task.Logger = new Mock<ILogger>().Object;

			syncer.Setup(s => s.Save()).Throws(new ZCannotSaveException("Anything", "any"));
			syncer.Setup(s => s.EntitiesWithErrors).Returns(new List<IADEntity>() { new Mock<IADEntity>().Object, new Mock<IADEntity>().Object });
			task.RunTask();

			syncer.Verify(s => s.Synchronise(null, null), Times.Once);

			AssertEquals("Should not record LastSuccessfulSyncUTC", oldSyncTime, ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.Value);
			AssertEquals("Should not record LastFailedSyncStaffPKs", 0, ActiveDirectoryRegistry.Instance.LastFailedSyncStaffPKs.Value.Length);
			AssertEquals("Should not record LastFailedSyncGroupPKs", 0, ActiveDirectoryRegistry.Instance.LastFailedSyncGroupPKs.Value.Length);
		}

		public void TestRunTask_ADEnabled_ExceptionInCommitChanges_ShouldUpdateLastFailedSyncStaffPKs()
		{
			ActiveDirectoryRegistry.Instance.IsIntegrationEnabled = true;
			ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ActiveDirectoryRegistry.Instance.LastSuccessfulSyncUTC.DefaultValue);
			ActiveDirectoryRegistry.Instance.LastFailedSyncStaffPKs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<Guid>());
			ActiveDirectoryRegistry.Instance.LastFailedSyncGroupPKs.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Array.Empty<Guid>());
			ActiveDirectoryRegistry.Instance.SyncDirection = SyncDirection.TwoWay;
			ActiveDirectoryRegistry.Instance.SyncMode = SyncMode.EnterpriseIsMaster;

			var staff = Factory.New<GlbStaff>();
			staff.GS_LoginName = "user1";
			staff.GS_IsActive = false;
			staff.GS_SystemLastEditTimeUtc = ZDateTime.UtcNow;

			var exception = new DirectoryServicesException("Commit failed");
			var userEntry = new Mock<IUserDirectoryEntry>();
			userEntry.Setup(x => x.CommitChanges()).Throws(exception);
			userEntry.SetupGet(x => x.CanUpdate).Returns(true);
			userEntry.SetupGet(x => x.Guid).Returns(Guid.NewGuid());
			staff.GS_ActiveDirectoryObjectGuid = userEntry.Object.Guid;
			Factory.Save();

			var hasChanges = false;
			userEntry.SetupGet(x => x.HasChanges).Returns(() => hasChanges);
			userEntry.As<IDirectoryEntry>().SetupSet(x => x.IsActive = It.IsAny<bool>()).Callback<bool>(x => hasChanges = true);
			directorySearcherMock.Setup(s => s.FindUser(staff.GS_ActiveDirectoryObjectGuid.ToGuid(), TestConstants.ValidOU)).Returns(userEntry.Object);

			AssertEquals("hasChanges should be false before the ActiveDirectorySynchronisationTask", false, hasChanges);

			var task = new ActiveDirectorySynchronisationTask();
			var loggerMock = new Mock<ILogger>();
			task.Logger = loggerMock.Object;
			task.RunTask();

			AssertEquals("hasChanges should be true after the ActiveDirectorySynchronisationTask", true, hasChanges);

			var lastFailedSyncStaffPKs = ActiveDirectoryRegistry.Instance.LastFailedSyncStaffPKs.Value;
			AssertCollectionContains("Commit failed staff should be recorded", staff.PK.ToGuid(), lastFailedSyncStaffPKs);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ActiveDirectoryRegistry.Instance.DomainCredentialsCollection.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ADTestHelper.CreateDomainCredentialsCollection());
		}
	}

	[TestedType(typeof(ActiveDirectorySynchronisationTask))]
	class ActiveDirectorySynchronisationTaskServiceTaskTestCase : ServiceTaskTestCase<ActiveDirectorySynchronisationTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
	}
}
