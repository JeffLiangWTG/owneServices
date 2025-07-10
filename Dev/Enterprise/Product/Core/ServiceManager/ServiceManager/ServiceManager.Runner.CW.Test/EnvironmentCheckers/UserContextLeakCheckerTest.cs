using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Shared.Testing;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.Abstractions;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test.EnvironmentCheckers
{
	[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
	class UserContextLeakCheckerTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestMultiThreadedHandling()
		{
			var threads = new List<Thread>();
			var allStaff = new List<GlbStaff>();
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };

			for (var i = 0; i < 10; ++i)
			{
				var staff = factory.NewWithValidTestData<GlbStaff>();
				staff.GS_LoginName = $"User {i}";
				staff.GS_Code = $"t{i}";
				allStaff.Add(staff);
			}

			factory.Save();

			for (var i = 0; i < 20; ++i)
			{
				threads.Add(new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						for (var j = 0; j < 10; ++j)
						{
							Env.SetUserContext(new UserContext(allStaff[j].PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK));
						}
					}
				}));
			}

			var checker = new UserContextLeakChecker(errorReporterProxyMock.Object, Mock.Of<IRunnerRegistrySettings>());
			checker.Initialize(runCommandInfoMock.Object);

			foreach (var thread in threads)
			{
				thread.Start();
			}

			foreach (var thread in threads)
			{
				thread.Join();
			}

			Env.SetUserContext(new UserContext(allStaff[1].PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK));

			AssertExceptionThrown<UserContextCorruptedException>(() =>
			{
				checker.CheckOnServiceTaskCompletion(serviceTaskMock.Object);
			});

			errorReporterProxyMock.Verify(
				proxy => proxy.ReportOnce(It.IsAny<string>(), It.Is<string>(msg => msg.Contains("[TestMultiThreadedHandling]"))),
				Times.Once);
			errorReporterProxyMock.VerifyNoOtherCalls();
		}

		[UseSnapshotProtection]
		public void TestOnDisposeWithNotEqualUserContext()
		{
			var oldUserContext = Env.CurrentUserContext;
			var newUserContext = new UserContext("TestUser", Env.CurrentBranchPK, Env.CurrentDepartmentPK);

			var checker = new UserContextLeakChecker(errorReporterProxyMock.Object, Mock.Of<IRunnerRegistrySettings>());
			checker.Initialize(runCommandInfoMock.Object);

			Env.SetUserContext(newUserContext);

			AssertExceptionThrown<UserContextCorruptedException>(() =>
			{
				checker.CheckOnServiceTaskCompletion(serviceTaskMock.Object);
			});

			var expected1 = "Service task [xxx] changed user context without using a temporary context.";
			var expected2 = $@"Starting User Context:
Company: {oldUserContext.Company?.Code} - PK: {oldUserContext.Company?.PK}
Branch: {oldUserContext.Branch?.Code} - PK: {oldUserContext.Branch?.PK}
User: {oldUserContext.User?.LoginName} - PK: {oldUserContext.User?.PK}
Department: {oldUserContext.Department?.Code} - PK: {oldUserContext.Department?.PK}
Current User Context:
Company: {newUserContext.Company?.Code} - PK: {newUserContext.Company?.PK}
Branch: {newUserContext.Branch?.Code} - PK: {newUserContext.Branch?.PK}
User: {newUserContext.User?.LoginName} - PK: {newUserContext.User?.PK}
Department: {newUserContext.Department?.Code} - PK: {newUserContext.Department?.PK}";

			errorReporterProxyMock.Verify(
				proxy => proxy.ReportOnce(It.IsAny<string>(), It.Is<string>(msg => msg.Contains(expected1) && msg.Contains(expected2))),
				Times.Once);
			errorReporterProxyMock.VerifyNoOtherCalls();
		}

		[UseSnapshotProtection]
		public void TestDisposableEnvironmentLeakTracking()
		{
			IDisposable environment;

			var checker = new UserContextLeakChecker(errorReporterProxyMock.Object, Mock.Of<IRunnerRegistrySettings>());
			checker.Initialize(runCommandInfoMock.Object);

			environment = DisposableEnvironment.ForBranchCodeSlowerThanPK("BNE");

			AssertExceptionThrown<UserContextCorruptedException>(() =>
			{
				checker.CheckOnServiceTaskCompletion(serviceTaskMock.Object);
			});

			errorReporterProxyMock.Verify(
				proxy => proxy.ReportOnce(It.IsAny<string>(), It.Is<string>(msg => msg.Contains("[TestDisposableEnvironmentLeakTracking]"))),
				Times.Once);
			errorReporterProxyMock.VerifyNoOtherCalls();
			errorReporterProxyMock.Reset();
			environment.Dispose();

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var company = factory.NewWithValidTestData<GlbCompany>();
			var branch = factory.NewWithValidTestData<GlbBranch>();
			branch.GB_BranchName = "TestBranch";
			branch.GB_GC = company.PK;
			factory.Save();

			checker = new UserContextLeakChecker(errorReporterProxyMock.Object, Mock.Of<IRunnerRegistrySettings>());
			checker.Initialize(runCommandInfoMock.Object);

			environment = DisposableEnvironment.ForBranch(branch.PK.ToGuid());

			AssertExceptionThrown<UserContextCorruptedException>(() =>
			{
				checker.CheckOnServiceTaskCompletion(serviceTaskMock.Object);
			});

			errorReporterProxyMock.Verify(
				proxy => proxy.ReportOnce(It.IsAny<string>(), It.Is<string>(msg => msg.Contains("[TestDisposableEnvironmentLeakTracking]"))),
				Times.Once);
			errorReporterProxyMock.VerifyNoOtherCalls();
			errorReporterProxyMock.Reset();
			environment.Dispose();

			checker = new UserContextLeakChecker(errorReporterProxyMock.Object, Mock.Of<IRunnerRegistrySettings>());
			checker.Initialize(runCommandInfoMock.Object);

			environment = DisposableEnvironment.ForBranch("BNE");

			AssertExceptionThrown<UserContextCorruptedException>(() =>
			{
				checker.CheckOnServiceTaskCompletion(serviceTaskMock.Object);
			});

			errorReporterProxyMock.Verify(
				proxy => proxy.ReportOnce(It.IsAny<string>(), It.Is<string>(msg => msg.Contains("[TestDisposableEnvironmentLeakTracking]"))),
				Times.Once);
			errorReporterProxyMock.VerifyNoOtherCalls();
			errorReporterProxyMock.Reset();
			environment.Dispose();

			checker = new UserContextLeakChecker(errorReporterProxyMock.Object, Mock.Of<IRunnerRegistrySettings>());
			checker.Initialize(runCommandInfoMock.Object);

			environment = DisposableEnvironment.ForCompany(company.GC_Code);

			AssertExceptionThrown<UserContextCorruptedException>(() =>
			{
				checker.CheckOnServiceTaskCompletion(serviceTaskMock.Object);
			});

			errorReporterProxyMock.Verify(
				proxy => proxy.ReportOnce(It.IsAny<string>(), It.Is<string>(msg => msg.Contains("[TestDisposableEnvironmentLeakTracking]"))),
				Times.Once);
			errorReporterProxyMock.VerifyNoOtherCalls();
			environment.Dispose();
		}

		[UseSnapshotProtection]
		public void TestUserContextLeakTrackerTracksTempUserContext()
		{
			var userContext = new TemporaryUserContext()
			{
				StaffLoginName = "Test",
				BranchPK = Env.CurrentBranch.PK,
				DepartmentPK = Env.CurrentDepartment.PK
			};

			IDisposable environment;
			var checker = new UserContextLeakChecker(errorReporterProxyMock.Object, Mock.Of<IRunnerRegistrySettings>());
			checker.Initialize(runCommandInfoMock.Object);

			environment = userContext.Set();

			AssertExceptionThrown<UserContextCorruptedException>(() =>
			{
				checker.CheckOnServiceTaskCompletion(serviceTaskMock.Object);
			});

			errorReporterProxyMock.Verify(
				proxy => proxy.ReportOnce(It.IsAny<string>(), It.Is<string>(msg => msg.Contains("[TestUserContextLeakTrackerTracksTempUserContext]"))),
				Times.Once);
			errorReporterProxyMock.VerifyNoOtherCalls();

			environment.Dispose();
		}

		[UseSnapshotProtection]
		public void TestUserContextLeakTrackerTracksTempUserContextWithStack()
		{
			var userContext = new TemporaryUserContext()
			{
				StaffLoginName = "Test",
				BranchPK = Env.CurrentBranch.PK,
				DepartmentPK = Env.CurrentDepartment.PK
			};

			var checker = new UserContextLeakChecker(errorReporterProxyMock.Object, Mock.Of<IRunnerRegistrySettings>(o => o.EnableStackTraceInUserContextSwitcher));
			checker.Initialize(runCommandInfoMock.Object);

			var environment = userContext.Set();

			AssertExceptionThrown<UserContextCorruptedException>(() =>
			{
				checker.CheckOnServiceTaskCompletion(serviceTaskMock.Object);
			});

			errorReporterProxyMock.Verify(
				proxy => proxy.ReportOnce(
					It.IsAny<string>(),
					It.Is<string>(msg =>
						msg.Contains("[TestUserContextLeakTrackerTracksTempUserContextWithStack]") &&
						msg.Contains("Stack: at"))),
				Times.Once);
			errorReporterProxyMock.VerifyNoOtherCalls();
			environment.Dispose();
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestUserContextLeakTrackerDoesNotReportFromUnhandledException()
		{
			// Arrange
			var initialUserContext = Env.CurrentUserContext;
			var currentBranch = Env.CurrentBranchPK;
			var currentDept = Env.CurrentDepartmentPK;

			try
			{
				CreateStaff("TestUser", "ts1");
				var commandInfo = new DirectRunCommandInfo(string.Empty, "serviceTaskCode", Guid.Empty);

				serviceTaskHandlerInitializer
					.Setup(x => x.CreateServiceTaskHandler(commandInfo.AssemblyName, commandInfo.Code, commandInfo.ConfigString))
					.Returns(serviceTaskMock.Object);

				void RunServiceTask(Exception exceptionToThrow)
				{
					serviceTaskMock
						.Setup(x => x.Run(It.IsAny<CancellationToken>()))
						.Callback(() =>
						{
							Env.SetUserContext(new UserContext("TestUser", currentBranch, currentDept));
							throw exceptionToThrow;
						});

					serviceTaskMock
						.Setup(x => x.HandleException(It.IsAny<Exception>(), It.IsAny<string>()))
						.Callback((Exception exception, string serviceTaskCode) =>
						{
							if (!exception.IsCriticalException())
							{
								errorReporterProxyMock.Object.ReportOnce(ExceptionConstants.RunnerExceptionLocation, exception);
							}
						});

					using var cancellationTokenSource = new CancellationTokenSource();

					// Act
					serviceTaskRunner.RunServiceTask(commandInfo, cancellationTokenSource);
				}

				RunServiceTask(new SqlLockLostException());
				errorReporterProxyMock.Verify(
					proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()),
					Times.Never);
				errorReporterProxyMock.Reset();

				Env.SetUserContext(initialUserContext);

				RunServiceTask(new Exception());
				errorReporterProxyMock.Verify(
					proxy => proxy.ReportOnce(It.Is<string>(msg => !msg.Contains("changed user context")), It.IsAny<Exception>()),
					Times.Once);
			}
			finally
			{
				Env.SetUserContext(initialUserContext);
			}
		}

		[ExpectNoExceptions]
		[UseSnapshotProtection]
		public void TestUserContextLeakTrackerDoesNotReportFromException_ThrownDuringContextSwitching()
		{
			void OnUserContextChangingIShouldExplode(object sender, IUserContextChangingEventArgs e)
			{
				throw new SqlLockLostException();
			}

			// Arrange
			var initialUserContext = Env.CurrentUserContext;
			var currentBranch = Env.CurrentBranchPK;
			var currentDept = Env.CurrentDepartmentPK;

			try
			{
				CreateStaff("TestUser", "ts1");
				var commandInfo = new DirectRunCommandInfo(string.Empty, "serviceTaskCode", Guid.Empty);

				serviceTaskHandlerInitializer
					.Setup(x => x.CreateServiceTaskHandler(commandInfo.AssemblyName, commandInfo.Code, commandInfo.ConfigString))
					.Returns(serviceTaskMock.Object);

				void RunServiceTask(Exception exceptionToThrow)
				{
					serviceTaskMock
						.Setup(x => x.Run(It.IsAny<CancellationToken>()))
						.Callback(() =>
						{
							Env.Instance.UserContextChanging += OnUserContextChangingIShouldExplode;
							Env.SetUserContext(new UserContext("TestUser", currentBranch, currentDept));
						});

					using var cancellationTokenSource = new CancellationTokenSource();

					// Act
					serviceTaskRunner.RunServiceTask(commandInfo, cancellationTokenSource);
				}

				RunServiceTask(new SqlLockLostException());
				errorReporterProxyMock.Verify(
					proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<Exception>()),
					Times.Never);
			}
			finally
			{
				Env.Instance.UserContextChanging -= OnUserContextChangingIShouldExplode;
				Env.SetUserContext(initialUserContext);
			}
		}

		public void TestOnUserContextChange_WhenDatabaseIsUpgraded_DoesNotOpenDbConnection()
		{
			RegistryItemDictionary.Instance.PurgeAll();
			var newUserContext = new UserContext(Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK);

			// Before creating the UserContextLeakTracker, do a user context change
			// so any registry settings it uses are loaded from the db.
			// Any db hit after this will just come from UserContextLeakTracker.
			using (Env.SetTemporaryUserContext(newUserContext))
			{
			}

			var checker = new UserContextLeakChecker(errorReporterProxyMock.Object, Mock.Of<IRunnerRegistrySettings>());
			checker.Initialize(runCommandInfoMock.Object);

			Db.Connection.CloseConnection();

			checker.CheckOnServiceTaskCompletion(It.IsAny<IServiceTaskHandler>());

			using (Env.SetTemporaryUserContext(newUserContext))
			{
				Db.Connection.DatabaseUpgradedExceptionHasBeenThrown = true;
			}

			AssertEquals("ConnectionState", ConnectionState.Closed, Db.Connection.State);
		}

		public void TestUserContextManagerUnsubscribesOnUserContextChanged()
		{
			var onUserContextChangingCalled = 0;
			var onUserContextChangedCalled = 0;
			var newUserContext = new UserContext(Env.CurrentUser, Env.CurrentBranchPK, Env.CurrentDepartmentPK);

			using (var manager = new UserContextManager((IUserContextChangingEventArgs a) => onUserContextChangingCalled++, (IUserContextChangingEventArgs a) => onUserContextChangedCalled++))
			{
				Env.SetUserContext(newUserContext);
				AssertEquals("OnUserContextChanging subscribed", 1, onUserContextChangingCalled);
				AssertEquals("OnUserContextChanged subscribed", 1, onUserContextChangedCalled);
			}

			Env.SetUserContext(newUserContext);
			AssertEquals("OnUserContextChanging unsubscribed", 1, onUserContextChangingCalled);
			AssertEquals("OnUserContextChanged unsubscribed", 1, onUserContextChangedCalled);
		}

		static GlbStaff CreateStaff(string loginName, string code)
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.New<GlbStaff>();
			staff.GS_IsResource = false;
			staff.GS_CanLogin = true;
			staff.GS_LoginName = loginName;
			staff.GS_Code = code;
			staff.StaffPlainTextPassword = "passworb";
			staff.GS_IsActive = true;
			staff.GS_IsOperational = true;
			staff.GS_IsController = false;
			staff.GS_IsTwoFactorAuthenticationEnabled = false;
			staff.GS_EmailAddress = "e@mail.com";

			var security1 = factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = staff.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";

			factory.Save();
			return staff;
		}

		protected override void SetUp()
		{
			base.SetUp();
			listener = ApplicationLoggingTestHelper.Listen();
			serviceTaskLoggerMock = new Mock<IServiceTaskLogger>();
			runnerLoggerMock = new Mock<IRunnerLogger>();
			processEnvironmentRecorderMock = new Mock<IProcessEnvironmentRecorder>();
			errorReporterProxyMock = new Mock<IErrorReporterProxy>();
			errorReporterProxyMock.Setup(proxy => proxy.ReportOnce(It.IsAny<string>(), It.IsAny<string>()));
			serviceTaskHandlerInitializer = new Mock<IServiceTaskHandlerInitializer>();
			serviceTaskRunner = new ServiceTaskRunner(serviceTaskLoggerMock.Object, runnerLoggerMock.Object, processEnvironmentRecorderMock.Object, Mock.Of<IEnvironmentCheckerStrategy>(), errorReporterProxyMock.Object, serviceTaskHandlerInitializer.Object, ApplicationLoggingTestHelper.MockCategorizedLoggerFactory());

			serviceTaskMock = new Mock<IDisposableServiceTaskHandler>();
			var serviceTaskConfigMock = new Mock<IHostedServiceAttribute>();
			serviceTaskConfigMock.Setup(x => x.Code).Returns(taskCode);
			serviceTaskConfigMock.Setup(x => x.TypeName).Returns("typeName1");
			serviceTaskConfigMock.Setup(x => x.TypeAssemblyName).Returns("assemblyName1");
			serviceTaskMock.Setup(x => x.HostedServiceAttribute).Returns(serviceTaskConfigMock.Object);

			runCommandInfoMock = new Mock<IRunCommandInfo>();
			runCommandInfoMock
				.SetupGet(info => info.Code)
				.Returns(taskCode);
		}

		protected override void TearDown()
		{
			listener.Dispose();
			base.TearDown();
		}

		Mock<IProcessEnvironmentRecorder> processEnvironmentRecorderMock;
		Mock<IRunnerLogger> runnerLoggerMock;
		Mock<IServiceTaskLogger> serviceTaskLoggerMock;
		Mock<IDisposableServiceTaskHandler> serviceTaskMock;
		Mock<IRunCommandInfo> runCommandInfoMock;
		Mock<IErrorReporterProxy> errorReporterProxyMock;
		Mock<IServiceTaskHandlerInitializer> serviceTaskHandlerInitializer;
		ServiceTaskRunner serviceTaskRunner;
		readonly string taskCode = "xxx";
		IDisposable listener;
	}
}
