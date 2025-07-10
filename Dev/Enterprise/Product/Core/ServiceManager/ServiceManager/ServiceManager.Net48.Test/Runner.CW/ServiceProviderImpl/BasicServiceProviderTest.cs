using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ServiceManager.Shared;
using Enterprise.ServiceManager.Shared.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using ServiceManager.Common;
using ServiceManager.Common.Abstractions;
using ServiceManager.Common.CW;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Logging.CW.Test;
using ServiceManager.Runner.Abstractions;
using ServiceManager.Shared.Abstractions;
using IDateTimeProvider = ServiceManager.Shared.Abstractions.IDateTimeProvider;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using ILoggerFactory = ServiceManager.Integration.ServiceTasks.CW.ILoggerFactory;
using User = Enterprise.ZArchitecture.Environment.User;

namespace Enterprise.ServiceManager.Runner.Testing
{
	[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
	public class BasicServiceProviderTest : TestCaseWithFactory
	{
		public void TestGlbStaffTypeNameIsCorrect()
		{
			AssertEquals(BasicServiceProvider.GlbStaffTypeName, $"{typeof(GlbStaff)}, {typeof(GlbStaff).Namespace}");
		}

		public void TestRunWithBackgroundAppDomainWorker()
		{
			// Arrange
			var hostedServiceAttribute = Mock.Of<IHostedServiceAttribute>(
				attribute => attribute.Code == "###"
							&& attribute.Description == "#DESC#"
							&& attribute.Category == "TST"
							&& attribute.Type == typeof(ProviderForTesting)
							&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

			var provider = new ProviderWithBackgroundAppDomainWorker();
			serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceAttribute });
			provider.InitializeRunningEnvironment(hostedServiceAttribute, "", loggerMock.Object);

			// Act
			// Assert
			AssertExceptionThrown<HostedServiceException>(() => ((IServiceTaskHandler)provider).Run(CancellationToken.None));
			AssertEquals(1, provider.TestLogger.Count);
			AssertStartsWith(string.Empty, "Error|", provider.TestLogger[0]);
			AssertContains("Test Exception in Child AppDomain", provider.TestLogger[0]);
			loggerMock.VerifyLog(LogLevel.Debug, o => Regex.IsMatch(o, @"Using environment: .+"), Times.Once);
			loggerMock.VerifyNoOtherCalls();
		}

		public void TestRunWithWebProxy_WithAutodetect()
		{
			// Arrange
			var proxyMock = new Mock<IWebRequestDefaultProxyWrapper>();
			proxyMock.SetupSequence(pm => pm.DefaultWebProxy).Returns((IWebProxy)null).Returns(WebRequest.DefaultWebProxy);

			var host = Factory.New<StmServiceHost>();
			host.SH_HostName = ServiceManagerHelper.GetHostName();
			Factory.Save();
			IServiceTaskHandler provider = new ProviderForTesting(Factory, proxyMock.Object);
			host.SH_ProxyAutoDetect = true;
			host.Factory.Save();

			var hostedServiceAttribute = Mock.Of<IHostedServiceAttribute>(
				attribute => attribute.Code == "###"
							&& attribute.Description == "#DESC#"
							&& attribute.Category == "TST"
							&& attribute.Type == typeof(ProviderForTesting)
							&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

			// Act
			provider.InitializeRunningEnvironment(hostedServiceAttribute, "", loggerMock.Object);

			// Assert
			proxyMock.Verify(pm => pm.DefaultWebProxy, Times.Once);
			proxyMock.Verify(pm => pm.SetDefaultProxyToSystemProxy(), Times.Once);
			AssertEquals(0, ((ProviderForTesting)provider).TestLogger.Count);
			loggerMock.VerifyLogAny(Times.Once);
			loggerMock.VerifyLog(
					LogLevel.Debug,
					$"Proxy autodetect for {host.SH_HostName} is set. Defaulting to system proxy settings.",
				Times.Once);
		}

		public void TestRunWithWebProxy_WithAutodetectAndNullHost()
		{
			// Arrange
			var proxyMock = new Mock<IWebRequestDefaultProxyWrapper>();
			proxyMock.SetupSequence(pm => pm.DefaultWebProxy).Returns((IWebProxy)null).Returns(WebRequest.DefaultWebProxy);

			IServiceTaskHandler provider = new ProviderForTesting(Factory, proxyMock.Object);
			host.SH_ProxyAutoDetect = true;
			host.Factory.Save();

			var hostedServiceAttribute = Mock.Of<IHostedServiceAttribute>(
				attribute => attribute.Code == "###"
							&& attribute.Description == "#DESC#"
							&& attribute.Category == "TST"
							&& attribute.Type == typeof(ProviderForTesting)
							&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

			// Act
			provider.InitializeRunningEnvironment(hostedServiceAttribute, "", loggerMock.Object);

			// Assert
			proxyMock.Verify(pm => pm.DefaultWebProxy, Times.Once);
			proxyMock.Verify(pm => pm.SetDefaultProxyToSystemProxy(), Times.Once);
			AssertEquals(0, ((ProviderForTesting)provider).TestLogger.Count);
			loggerMock.VerifyLogAny(Times.Once);
			loggerMock.VerifyLog(
					LogLevel.Warning,
					$"Unable to find host record with hostname {ServiceManagerHelper.GetHostName()} to retrieve proxy settings. Defaulting to system proxy settings.",
				Times.Once);
		}

		public void TestRunWithWebProxy_WithoutAutodetect()
		{
			// Arrange
			var proxyMock = new Mock<IWebRequestDefaultProxyWrapper>();
			proxyMock.SetupSequence(pm => pm.DefaultWebProxy).Returns((WebProxy)null).Returns(WebRequest.DefaultWebProxy);
			var host = Factory.New<StmServiceHost>();
			host.SH_HostName = ServiceManagerHelper.GetHostName();
			Factory.Save();

			host.SH_ProxyAutoDetect = false;
			host.SH_ProxyHost = "my.host";
			host.SH_ProxyPort = 1234;
			Factory.Save();

			var hostedServiceAttribute = Mock.Of<IHostedServiceAttribute>(
				attribute => attribute.Code == "###"
							&& attribute.Description == "#DESC#"
							&& attribute.Category == "TST"
							&& attribute.Type == typeof(ProviderForTesting)
							&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

			var provider = new ProviderForTesting(Factory, proxyMock.Object);
			serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceAttribute });

			// Act
			// new runner will be initialized with null proxy
			((IServiceTaskHandler)provider).InitializeRunningEnvironment(hostedServiceAttribute, "", loggerMock.Object);
			((IServiceTaskHandler)provider).Run(CancellationToken.None);

			// Assert
			proxyMock.Verify(pm => pm.DefaultWebProxy, Times.Exactly(2));
			proxyMock.Verify(pm => pm.SetDefaultProxyToHostProxy(host), Times.Once);
			AssertEquals(0, provider.TestLogger.Count);
			loggerMock.VerifyLog(
					LogLevel.Debug,
					$"Proxy autodetect for {ServiceManagerHelper.GetHostName()} is not set. Proxy settings will be set based on host configuration.",
				Times.Once);
		}

		public void TestRunWithWebProxy_SettingsAreRetrievedForTheActualHostRunningTheProcess()
		{
			// Arrange
			var proxyMock = new Mock<IWebRequestDefaultProxyWrapper>();
			proxyMock.SetupSequence(pm => pm.DefaultWebProxy).Returns((WebProxy)null).Returns(WebRequest.DefaultWebProxy);
			var hostSurrogate = Factory.New<StmServiceHost>();
			hostSurrogate.SH_HostName = "SurrogateHost";
			hostSurrogate.SH_ProxyAutoDetect = false;

			var actualCurrentHost = Factory.New<StmServiceHost>();
			actualCurrentHost.SH_HostName = ServiceManagerHelper.GetHostName();
			actualCurrentHost.SH_ProxyAutoDetect = false;
			actualCurrentHost.SH_ProxyHost = "my.host";
			actualCurrentHost.SH_ProxyPort = 1234;
			Factory.Save();

			var hostedServiceAttribute = Mock.Of<IHostedServiceAttribute>(
				attribute => attribute.Code == "###"
							&& attribute.Description == "#DESC#"
							&& attribute.Category == "TST"
							&& attribute.Type == typeof(ProviderForTesting)
							&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

			var provider = new ProviderForTesting(Factory, proxyMock.Object);
			serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceAttribute });

			// Act
			// new runner will be initialized with null proxy
			((IServiceTaskHandler)provider).InitializeRunningEnvironment(hostedServiceAttribute, "", loggerMock.Object);
			((IServiceTaskHandler)provider).Run(CancellationToken.None);

			// Assert
			proxyMock.Verify(pm => pm.DefaultWebProxy, Times.Exactly(2));
			proxyMock.Verify(pm => pm.SetDefaultProxyToHostProxy(actualCurrentHost), Times.Once);
			AssertEquals(0, provider.TestLogger.Count);
			loggerMock.VerifyLog(
					LogLevel.Debug,
					$"Proxy autodetect for {ServiceManagerHelper.GetHostName()} is not set. Proxy settings will be set based on host configuration.",
				Times.Once);
		}

		public void TestRun_WithBrandingReset()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			Factory.Save();
			var hostedServiceAttribute = Mock.Of<IHostedServiceAttribute>(
				attribute => attribute.Code == "###"
							&& attribute.Description == "#DESC#"
							&& attribute.Category == "TST"
							&& attribute.Type == typeof(ProviderForTesting)
							&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));

			IServiceTaskHandler provider2 = new ProviderForTesting();
			serviceTaskScheduleManager.ConfigureSchedules(new[] { hostedServiceAttribute });
			provider2.InitializeRunningEnvironment(hostedServiceAttribute, "", loggerMock.Object);
			provider2.Run(CancellationToken.None);
			AssertEquals("ProductivityWise", BrandingFactory.Instance.ProductName);

			DataRegistry.Instance.ProductivityWiseModeEnabled = false;
			Factory.Save();
			IServiceTaskHandler provider1 = new ProviderForTesting();
			provider1.InitializeRunningEnvironment(hostedServiceAttribute, "", loggerMock.Object);
			provider1.Run(CancellationToken.None);
			AssertEquals("CargoWise", BrandingFactory.Instance.ProductName);
		}

		public void TestWrongParamsCall()
		{
			var hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
			var provider = new Mock<BasicServiceProvider>(null, new WebRequestDefaultProxyWrapper(), ObjectFactory.Get<ILoggerFactory>(), Mock.Of<IServiceTaskLoader>());

			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => provider.Object.InitializeRunningEnvironment(null, string.Empty, loggerMock.Object));
				AssertEquals("hostedServiceAttribute", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => provider.Object.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, null));
				AssertEquals("logger", result.ParamName);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var serviceTaskLoaderFactoryMock = new Mock<IServiceTaskLoaderFactory>();
			attributeProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
			statusProviderMock = new Mock<IServiceTaskScheduleStatusProvider>();
			bindingsProviderMock = new Mock<IHostedServiceBusinessObjectBindingsProvider>();
			dateTimeProviderMock = new Mock<IDateTimeProvider>();

			serviceTaskLoaderFactoryMock
				.Setup(x => x.CreateServiceTaskLoader())
				.Returns(new NativeServiceTaskLoader(attributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object));

			var transactionAdapterFactoryMock = new Mock<ITransactionAdapterFactory>();
			transactionAdapterFactoryMock
				.Setup(x => x.CreateTransactionAdapter())
				.Returns(() => new NativeServiceTaskTransactionAdapter(attributeProviderMock.Object, statusProviderMock.Object, bindingsProviderMock.Object, dateTimeProviderMock.Object));
			serviceTaskScheduleManager = new ServiceTaskScheduleManager(serviceTaskLoaderFactoryMock.Object, transactionAdapterFactoryMock.Object, new DefaultScheduleConfigurer(new ServiceManagerDateTimeProvider()), new ServiceTaskRequirementsChecker());
			host = Factory.New<StmServiceHost>();
			host.SH_HostName = Guid.NewGuid().ToString();
			Factory.Save();

			loggerMock = new Mock<ILogger>();
		}

		StmServiceHost host;
		Mock<ILogger> loggerMock;
		ServiceTaskScheduleManager serviceTaskScheduleManager;
		Mock<IClientHostedServiceAttributeProvider> attributeProviderMock;
		Mock<IServiceTaskScheduleStatusProvider> statusProviderMock;
		Mock<IHostedServiceBusinessObjectBindingsProvider> bindingsProviderMock;
		Mock<IDateTimeProvider> dateTimeProviderMock;

		public class UserContextTest : TestCaseWithFactory
		{
			protected override void SetUp()
			{
				base.SetUp();
				var defaultScheduleMock = new Mock<IDefaultSchedule>();
				defaultScheduleMock
					.SetupGet(schedule => schedule.RunEvery)
					.Returns("15minutes");

				hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
				hostedServiceConfigMock
					.SetupGet(a => a.Code)
					.Returns("XXX");
				hostedServiceConfigMock
					.SetupGet(a => a.Description)
					.Returns("some description");
				hostedServiceConfigMock
					.SetupGet(a => a.Category)
					.Returns("some category");
				hostedServiceConfigMock
					.SetupGet(a => a.ConfigControlTypeAssemblyName)
					.Returns("A");
				hostedServiceConfigMock
					.SetupGet(a => a.ConfigControlTypeName)
					.Returns("B");
				hostedServiceConfigMock
					.SetupGet(a => a.MutuallyExclusiveTaskGroup)
					.Returns(MutuallyExclusiveServiceTaskGroups.NoGroup);
				hostedServiceConfigMock
					.SetupGet(a => a.DefaultSchedule)
					.Returns(defaultScheduleMock.Object);
				hostedServiceConfigMock
					.SetupGet(config => config.CanRunInAnyBranch)
					.Returns(true);
				hostedServiceConfigMock
					.SetupGet(config => config.Code)
					.Returns("XXX");

				attributeProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				attributeProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(hostedServiceConfigMock.Object);

				loggerMock = new Mock<ILogger>();

				serviceTaskMock = new Mock<BasicServiceProvider>(
					Factory,
					new WebRequestDefaultProxyWrapper(),
					Mock.Of<ILoggerFactory>(factory =>
						factory.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()) == Mock.Of<Integration.ILogger>()),
					new NativeServiceTaskLoader(Mock.Of<IClientHostedServiceAttributeProvider>(), Mock.Of<IServiceTaskScheduleStatusProvider>(), Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(), Mock.Of<IDateTimeProvider>())
					)
				{
					CallBase = true,
				};

				using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);

				var task = Factory.New<StmServiceTask>();
				task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
				task.SST_GB_Branch = Env.CurrentBranchPK;
				task.SST_NextRunTime = ZDateTimeOffset.Now;
				task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
				Factory.Save();
			}

			public void TestRequiresSetUserContext_NestedUsing_ParentUserContextIsRestored()
			{
				// Arrange
				var initialBranch = Env.CurrentUserContext.Branch;
				var branches = Factory.Load(ObjectFactory.GetType("IGlbBranch"), new ZQuery()).Where(x => x.PK != initialBranch.PK).ToList();
				var branch0 = Guid.Parse(branches[0][GlbBranchSchema.PK.Name].ToString());
				var branch1 = Guid.Parse(branches[1][GlbBranchSchema.PK.Name].ToString());
				hostedServiceConfigMock
					.SetupGet(config => config.CanRunInAnyBranch)
					.Returns(false);

				var result = new Queue<Guid>();
				using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
				{
					serviceTaskMock
						.Object
						.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);
					serviceTaskMock
						.Protected()
						.Setup("RunTask", ItExpr.IsAny<CancellationToken>())
						.Callback(() =>
						{
							result.Enqueue(Env.CurrentBranch.PK);

							using (DisposableEnvironment.ForBranch(branch0))
							{
								result.Enqueue(Env.CurrentBranch.PK);

								using (DisposableEnvironment.ForBranch(branch1))
								{
									result.Enqueue(Env.CurrentBranch.PK);
								}

								result.Enqueue(Env.CurrentBranch.PK);
							}

							result.Enqueue(Env.CurrentBranch.PK);
						});

					// Act
					serviceTaskMock
						.As<IServiceTaskHandler>()
						.Object
						.Run(CancellationToken.None);
				}

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals("Most outer context before using DisposableEnvironment", initialBranch.PK, result.Dequeue());
					AssertEquals("Middle layer context after using DisposableEnvironment", branch0, result.Dequeue());
					AssertEquals("Most inner context after using DisposableEnvironment", branch1, result.Dequeue());
					AssertEquals("Middle layer context after DisposableEnvironment disposed", branch0, result.Dequeue());
					AssertEquals("Most outer context after all DisposableEnvironment disposed", initialBranch.PK, result.Dequeue());

					Assert("No error is reported after environment is set by using DisposableEnvironment", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
				});
			}

			public void TestRequiresSetUserContext_UserContextNotSet_CanRunInAnyBranchFalse_NoErrorReport()
			{
				// Arrange
				hostedServiceConfigMock
					.SetupGet(config => config.CanRunInAnyBranch)
					.Returns(false);
				var provider = (IServiceTaskHandler)new ProviderForTesting(Factory,
					new WebRequestDefaultProxyWrapper(),
					() =>
					{
						_ = Env.CurrentBranch;
					});
				provider.InitializeRunningEnvironment(hostedServiceConfigMock.Object, "", loggerMock.Object);

				// Act
				provider.Run(CancellationToken.None);

				// Assert
				CombineAssertions(() =>
				{
					Assert("Error was not reported", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
				});
			}

			public void TestRequiresSetUserContext_SetUserContextIsSet_NoErrorReport()
			{
				// Arrange
				const string expectedBranchCode = "BB1";
				var userContext1 = CreateUserContext("login1@cargowise.com", "company 1", "DD1", expectedBranchCode, out var branchPk);
				var taskBranchCode = string.Empty;

				var provider = (IServiceTaskHandler)new ProviderForTesting(Factory,
					new WebRequestDefaultProxyWrapper(),
					() =>
					{
						using (Env.SetTemporaryUserContext(userContext1))
						using (DisposableEnvironment.ForBranch(branchPk))
						{
							taskBranchCode = Env.CurrentBranch.Code;
						}
					});
				provider.InitializeRunningEnvironment(hostedServiceConfigMock.Object, "", loggerMock.Object);

				ObjectFactory.Get<ISystemDataRegistry>().StatisticsCollectionEnabled = nameof(EnabledState.Detailed);
				TestPerformanceStatisticsCollector.ResetStatisticMode();
				PerformanceStatisticsCollector.ResetInstance();

				// Act
				provider.Run(CancellationToken.None);

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals("Task branch code is set", expectedBranchCode, taskBranchCode);
					AssertNullOrEmpty("No error is reported after environment is set by using DisposableEnvironment", ErrorReporter.LastMessageReported);
				});
			}

			public void TestRequiresSetUserContext_SetDisposableEnvironment_NoErrorReport()
			{
				// Arrange
				const string expectedBranchCode = "BB2";
				var initialUserContext = Env.CurrentUserContext;
				var userContext2 = CreateUserContext("login2@cargowise.com", "company 2", "DD2", expectedBranchCode, out var branchPk);
				var taskBranchCode = string.Empty;

				var provider = (IServiceTaskHandler)new ProviderForTesting(Factory,
					new WebRequestDefaultProxyWrapper(),
					() =>
					{
						Env.SetUserContext(userContext2);
						try
						{
							using (DisposableEnvironment.ForBranch(branchPk))
							{
								taskBranchCode = Env.CurrentBranch.Code;
							}
						}
						finally
						{
							Env.SetUserContext(initialUserContext);
						}
					});
				provider.InitializeRunningEnvironment(hostedServiceConfigMock.Object, "", loggerMock.Object);

				// Act
				provider.Run(CancellationToken.None);

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals("Task branch code is set", expectedBranchCode, taskBranchCode);
					Assert("No error is reported after environment is set by using DisposableEnvironment", string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
				});
			}

			UserContext CreateUserContext(string loginName, string companyName, string departmentCode, string branchCode, out Guid branchPK)
			{
				var user = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbStaff"));
				var company = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbCompany"));
				var branch = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbBranch"));
				var department = Factory.NewWithValidTestData(ObjectFactory.GetType("IGlbDepartment"));

				user[GlbStaffSchema.GS_LoginName.Name] = loginName;
				company[GlbCompanySchema.GC_Name.Name] = companyName;
				department[GlbDepartmentSchema.GE_Code.Name] = departmentCode;
				branch[GlbBranchSchema.GB_Code.Name] = branchCode;
				branch[GlbBranchSchema.GB_GC.Name] = company.PK;

				Factory.Save();

				branchPK = branch.PK.ToGuid();

				return new UserContext(loginName, branch.PK.ToGuid(), department.PK.ToGuid());
			}

			Mock<IHostedServiceAttribute> hostedServiceConfigMock;
			Mock<ILogger> loggerMock;
			Mock<BasicServiceProvider> serviceTaskMock;
			Mock<IClientHostedServiceAttributeProvider> attributeProviderMock;
		}

		public class HandleExceptionTest : TestCaseWithFactory
		{
			public void TestDatabaseUpgradeExceptionLoggedAsWarning()
			{
				// Arrange
				var proxyMock = new Mock<IWebRequestDefaultProxyWrapper>();
				var exception = new DatabaseUpgradedException();
				var loggerMock = new Mock<IServiceTaskLogger>();
				var hostedServiceAttribute = Mock.Of<IHostedServiceAttribute>(
					attribute => attribute.Code == "ABC"
						&& attribute.Category == "TST"
						&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));
				var provider = new ProviderForTesting(Factory, proxyMock.Object);
				provider.InitializeRunningEnvironment(hostedServiceAttribute, string.Empty, loggerMock.Object);

				// Act
				provider.HandleException(exception, "ABC");

				// Assert
				var logger = provider.TestLogger;
				CombineAssertions(() =>
				{
					AssertEquals(1, logger.Count);
					AssertEquals("Warning|Service task was interrupted due to a database upgrade.", logger[0]);
				});
			}

			public void TestCriticalExceptionsLoggedAsError()
			{
				CombineAssertions(() =>
				{
					foreach (var exception in ExceptionSource.CriticalExceptions)
					{
						HandleExceptionLogsException(exception);
					}
				});

				void HandleExceptionLogsException(Exception exception)
				{
					// Arrange
					var proxyMock = new Mock<IWebRequestDefaultProxyWrapper>();
					var serviceTaskLoggerMock = new Mock<IServiceTaskLogger>();
					var provider = new ProviderForTesting(Factory, proxyMock.Object);
					var hostedServiceAttribute = Mock.Of<IHostedServiceAttribute>(
						attribute => attribute.Code == "ABC"
							&& attribute.Category == "TST"
							&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));
					provider.InitializeRunningEnvironment(hostedServiceAttribute, string.Empty, serviceTaskLoggerMock.Object);

					// Act
					provider.HandleException(exception, "ABC");

					// Assert
					var logger = provider.TestLogger;
					AssertEquals(1, logger.Count);
					AssertEquals($"Error|Unhandled exception in the Service Task|{exception}", logger[0]);
				}
			}

			public void TestNonCriticalExceptionsReportedToErrorReporter()
			{
				CombineAssertions(() =>
				{
					foreach (var exception in ExceptionSource.NotCriticalExceptions)
					{
						HandleExceptionReportsException(exception);
					}
				});
				ErrorReporter.Clear();

				void HandleExceptionReportsException(Exception exception)
				{
					// Arrange
					var proxyMock = new Mock<IWebRequestDefaultProxyWrapper>();
					var provider = new ProviderForTesting(Factory, proxyMock.Object);

					// Act
					provider.HandleException(exception, "ABC");

					// Assert
					AssertEquals(exception, ErrorReporter.LastExceptionReported);
				}
			}

			public void TestWarningMessageLoggedForAllExceptions()
			{
				// Arrange
				var allExceptions = ExceptionSource.CriticalExceptions
					.Concat(ExceptionSource.NotCriticalExceptions)
					.ToList();
				CombineAssertions(() =>
				{
					foreach (var exception in allExceptions)
					{
						Test(exception);
					}
				});

				void Test(Exception exception)
				{
					var proxyMock = new Mock<IWebRequestDefaultProxyWrapper>();
					var provider = new ProviderForTesting(Factory, proxyMock.Object);
					var hostedServiceAttribute = Mock.Of<IHostedServiceAttribute>(
						attribute => attribute.Code == "ABC"
							&& attribute.Category == "TST"
							&& attribute.DefaultSchedule == Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));
					var loggerMock = new Mock<ILogger>();
					provider.InitializeRunningEnvironment(hostedServiceAttribute, string.Empty, loggerMock.Object);

					// Act
					provider.HandleException(exception, "ABC");
					ErrorReporter.Clear();

					// Assert
					AssertNoExceptionThrown(() =>
					{
						loggerMock.Verify(x => x.Log(
							LogLevel.Warning,
							It.IsAny<EventId>(),
							It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ABC threw an unhandled exception")),
							It.IsAny<Exception>(),
							It.IsAny<Func<It.IsAnyType, Exception, string>>()),
						Times.Once);
					});
				}
			}

			public void TestAllExceptionsCatchOutputsToErrorConsole()
			{
				// Arrange
				var allExceptions = ExceptionSource.CriticalExceptions
					.Concat(ExceptionSource.NotCriticalExceptions)
					.ToList();
				CombineAssertions(() =>
				{
					foreach (var exception in allExceptions)
					{
						Test(exception);
					}
				});

				void Test(Exception exception)
				{
					var proxyMock = new Mock<IWebRequestDefaultProxyWrapper>();
					var provider = new ProviderForTesting(Factory, proxyMock.Object);
					var originalError = Console.Error;

					using (var sw = new StringWriter())
					{
						Console.SetError(sw);

						// Act
						provider.HandleException(exception, "ABC");

						sw.Flush();
						var output = sw.ToString();
						ErrorReporter.Clear();

						// Assert
						AssertContains("Logging exception:", output);

						Console.SetError(originalError);
					}
				}
			}
		}

		public abstract class InitializeRunningEnvironmentTest : TestCaseWithFactory
		{
			protected override void SetUp()
			{
				base.SetUp();
				hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
				hostedServiceConfigMock
					.SetupGet(config => config.Code)
					.Returns("XXX");
				loggerMock = new Mock<ILogger>();

				serviceTaskMock = new Mock<BasicServiceProvider>(
					Factory,
					new WebRequestDefaultProxyWrapper(),
					Mock.Of<ILoggerFactory>(),
					new NativeServiceTaskLoader(Mock.Of<IClientHostedServiceAttributeProvider>(), Mock.Of<IServiceTaskScheduleStatusProvider>(), Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(), Mock.Of<IDateTimeProvider>()));
			}

			public void TestConfigStringIsSet()
			{
				// Arrange
				const string configString = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
				serviceTaskMock
					.As<IServiceTaskConfigurationUser>();

				// Act
				serviceTaskMock
					.Object
					.InitializeRunningEnvironment(hostedServiceConfigMock.Object, configString, loggerMock.Object);

				// Assert
				AssertNoExceptionThrown(() => serviceTaskMock
					.As<IServiceTaskConfigurationUser>()
					.VerifySet(user => user.ConfigString = configString, Times.Once));
			}

			public void TestEnvironmentIsResetWhenCurrentEnvironmentIsMadeInvalid()
			{
				//Arrange
				var testUser = Factory.NewWithValidTestData<GlbStaff>();
				var testBranch = Factory.NewWithValidTestData<GlbBranch>();
				var testDepartment = Factory.NewWithValidTestData<GlbDepartment>();

				Factory.Save();

				var initialUserContext = new UserContext((string)testUser.GS_LoginName, testBranch.PK.ToGuid(), testDepartment.PK.ToGuid());

				using (Env.SetTemporaryUserContext(initialUserContext))
				{
					serviceTaskMock
						.Object
						.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

					AssertEquals(Env.CurrentUserContext, initialUserContext);

					//Act
					testBranch.GB_IsActive = false;
					Factory.Save();

					serviceTaskMock
						.Object
						.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

					//Assert
					AssertNotEquals(Env.CurrentUserContext, initialUserContext);
				}
			}

			Mock<IHostedServiceAttribute> hostedServiceConfigMock;
			Mock<ILogger> loggerMock;
			Mock<BasicServiceProvider> serviceTaskMock;

			public abstract class BranchDependentTest : InitializeRunningEnvironmentTest
			{
				public void TestActiveBranchIsSelected()
				{
					// Arrange
					var branches = Factory.Load<GlbBranch>(new ZQuery());
					foreach (var branch in branches)
					{
						branch.GB_IsActive = false;
					}

					var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
					glbCompany.GC_IsActive = true;
					var glbBranch = Factory.NewWithValidTestData<GlbBranch>();
					glbBranch.GB_GC = glbCompany.PK;
					glbBranch.GB_IsActive = true;
					glbBranch.GB_SystemCreateTimeUtc = new ZDateTime(2006, 12, 26, DateTimeKind.Utc);
					Factory.Save();

					using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
					{
						// Act
						serviceTaskMock
							.Object
							.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

						// Assert
						AssertEquals(glbBranch.PK, Env.CurrentBranchPK);
					}
				}

				public void TestBranchFromActiveCompanyIsSelected()
				{
					// Arrange
					var companies = Factory.Load<GlbCompany>(new ZQuery());
					foreach (var company in companies)
					{
						company.GC_IsActive = false;
					}

					var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
					glbCompany.GC_IsActive = true;
					var glbBranch = Factory.NewWithValidTestData<GlbBranch>();
					glbBranch.GB_GC = glbCompany.PK;
					glbBranch.GB_IsActive = true;
					glbBranch.GB_SystemCreateTimeUtc = new ZDateTime(2006, 12, 26, DateTimeKind.Utc);
					Factory.Save();

					using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
					{
						// Act
						serviceTaskMock
							.Object
							.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

						// Assert
						AssertEquals(glbBranch.PK, Env.CurrentBranchPK);
					}
				}

				public void TestTheOldestBranchIsSelected()
				{
					// Arrange
					var glbCompany = Factory.NewWithValidTestData<GlbCompany>();
					glbCompany.GC_IsActive = true;

					var time = ZDateTime.Now;
					var branches = Factory.Load<GlbBranch>(new ZQuery());
					foreach (var branch in branches)
					{
						branch.GB_IsActive = false;
					}

					branches = Enumerable
						.Range(1, 3)
						.Select(i =>
						{
							var branch = Factory.NewWithValidTestData<GlbBranch>();
							branch.GB_GC = glbCompany.PK;
							branch.GB_IsActive = true;
							branch.GB_SystemCreateTimeUtc = time;
							return branch;
						})
						.ToArray();
					var glbBranch = branches
						.OrderBy(branch => branch.PK)
						.Skip(1)
						.Take(1)
						.Single();
					glbBranch.GB_SystemCreateTimeUtc = time.AddDays(-5);
					Factory.Save();

					using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
					{
						// Act
						serviceTaskMock
							.Object
							.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

						// Assert
						AssertEquals(glbBranch.PK, Env.CurrentBranchPK);
					}
				}

				public void TestDoesNotCreateTaskRecord()
				{
					// Arrange
					using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
					{
						// Act
						serviceTaskMock
							.Object
							.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

						// Assert
						var query = new ZQuery();
						query.AddToFilter(StmServiceTaskSchema.SST_ServiceTaskCode, hostedServiceConfigMock.Object.Code);
						var result = Factory.LoadTop1<StmServiceTask>(query);
						AssertNull(result);
					}
				}

				public class CanRunInAnyBranchTest : BranchDependentTest
				{
					protected override void SetUp()
					{
						base.SetUp();
						hostedServiceConfigMock
							.SetupGet(config => config.CanRunInAnyBranch)
							.Returns(true);
					}
				}

				public class CanRunInSpecificBranchTest : BranchDependentTest
				{
					protected override void SetUp()
					{
						base.SetUp();
						hostedServiceConfigMock
							.SetupGet(config => config.CanRunInAnyBranch)
							.Returns(false);
					}
				}
			}

			public class BranchIndependentTest : InitializeRunningEnvironmentTest
			{
				public void TestActiveDepartmentIsSelected()
				{
					// Arrange
					var departments = Factory.Load<GlbDepartment>(new ZQuery());
					foreach (var department in departments)
					{
						department.GE_IsActive = false;
					}

					var glbDepartment = Factory.NewWithValidTestData<GlbDepartment>();
					glbDepartment.GE_IsActive = true;
					glbDepartment.GE_SystemCreateTimeUtc = new ZDateTime(2006, 12, 26, DateTimeKind.Utc);
					Factory.Save();

					using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
					{
						// Act
						serviceTaskMock
							.Object
							.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

						// Assert
						AssertEquals(glbDepartment.PK, Env.CurrentDepartmentPK);
					}
				}

				public void TestTheOldestDepartmentIsSelected()
				{
					// Arrange
					var departments = Factory.Load<GlbDepartment>(new ZQuery());
					foreach (var department in departments)
					{
						department.GE_IsActive = false;
					}

					departments = new[]
						{
							new ZDateTime(2019, 12, 23, DateTimeKind.Utc),
							new ZDateTime(2006, 12, 26, DateTimeKind.Utc),
							new ZDateTime(2022, 7, 21, DateTimeKind.Utc),
						}
						.Select(time =>
						{
							var department = Factory.NewWithValidTestData<GlbDepartment>();
							department.GE_IsActive = true;
							department.GE_SystemCreateTimeUtc = time;
							return department;
						})
						.ToArray();
					var oldestDepartment = departments[1];
					Factory.Save();

					using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
					{
						// Act
						serviceTaskMock
							.Object
							.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

						// Assert
						AssertEquals(oldestDepartment.PK, Env.CurrentDepartmentPK);
					}
				}

				public void TestInitializationEnvironmentIsLogged()
				{
					// Arrange
					using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
					{
						// Act
						serviceTaskMock
							.Object
							.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

						// Assert
						AssertNoExceptionThrown(() =>
						{
							loggerMock.VerifyLog(LogLevel.Debug, o => Regex.IsMatch(o, @"Environment initialised: user \[.+\], branch \[.+\], department \[.+\]."),
								Times.Once);
							loggerMock.VerifyNoOtherCalls();
						});
					}
				}
			}
		}

		public abstract class RunTaskTest : TestCaseWithFactory
		{
			protected override void SetUp()
			{
				base.SetUp();
				var defaultScheduleMock = new Mock<IDefaultSchedule>();
				defaultScheduleMock
					.SetupGet(schedule => schedule.RunEvery)
					.Returns("15minutes");
				hostedServiceConfigMock = new Mock<IHostedServiceAttribute>();
				hostedServiceConfigMock
					.SetupGet(a => a.Code)
					.Returns("XXX");
				hostedServiceConfigMock
					.SetupGet(a => a.Description)
					.Returns("some description");
				hostedServiceConfigMock
					.SetupGet(a => a.Category)
					.Returns("some category");
				hostedServiceConfigMock
					.SetupGet(a => a.ConfigControlTypeAssemblyName)
					.Returns("A");
				hostedServiceConfigMock
					.SetupGet(a => a.ConfigControlTypeName)
					.Returns("B");
				hostedServiceConfigMock
					.SetupGet(a => a.MutuallyExclusiveTaskGroup)
					.Returns(MutuallyExclusiveServiceTaskGroups.NoGroup);
				hostedServiceConfigMock
					.SetupGet(a => a.DefaultSchedule)
					.Returns(defaultScheduleMock.Object);

				attributeProviderMock = new Mock<IClientHostedServiceAttributeProvider>();
				attributeProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(It.IsAny<string>()))
					.Returns(hostedServiceConfigMock.Object);

				loggerMock = new Mock<ILogger>();
				serviceTaskLoggerMock = new Mock<Integration.ILogger>();

				serviceTaskMock = new Mock<BasicServiceProvider>(
					Factory,
					new WebRequestDefaultProxyWrapper(),
					Mock.Of<ILoggerFactory>(factory =>
						factory.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()) == serviceTaskLoggerMock.Object),
					new NativeServiceTaskLoader(Mock.Of<IClientHostedServiceAttributeProvider>(), Mock.Of<IServiceTaskScheduleStatusProvider>(), Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(), Mock.Of<IDateTimeProvider>())
					)
				{
					CallBase = true,
				};
			}

			public void TestServiceTaskCodeIsSetToEnvironment()
			{
				// Arrange
				using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
				var task = Factory.New<StmServiceTask>();
				task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
				task.SST_NextRunTime = ZDateTimeOffset.Now;
				task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
				Factory.Save();

				var taskCode = string.Empty;
				serviceTaskMock
					.Protected()
					.Setup("RunTask", ItExpr.IsAny<CancellationToken>())
					.Callback(() =>
					{
						taskCode = Env.Instance.ServiceTaskCode;
					});
				serviceTaskMock
					.Object
					.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

				// Act
				serviceTaskMock
					.As<IServiceTaskHandler>()
					.Object
					.Run(CancellationToken.None);

				// Assert
				AssertEquals("XXX", taskCode);
			}

			public void TestCurrentUserIsSetToEnvironment()
			{
				// Arrange
				using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
				var task = Factory.New<StmServiceTask>();
				task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
				task.SST_NextRunTime = ZDateTimeOffset.Now;
				task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
				Factory.Save();

				using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
				{
					var loginName = string.Empty;
					serviceTaskMock
						.Protected()
						.Setup("RunTask", ItExpr.IsAny<CancellationToken>())
						.Callback(() =>
						{
							loginName = Env.CurrentUser.LoginName;
						});
					serviceTaskMock
						.Object
						.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

					// Act
					serviceTaskMock
						.As<IServiceTaskHandler>()
						.Object
						.Run(CancellationToken.None);

					// Assert
					AssertEquals(User.ServiceUserName, loginName);
				}
			}

			Mock<IHostedServiceAttribute> hostedServiceConfigMock;
			Mock<ILogger> loggerMock;
			Mock<Integration.ILogger> serviceTaskLoggerMock;
			Mock<BasicServiceProvider> serviceTaskMock;
			Mock<IClientHostedServiceAttributeProvider> attributeProviderMock;

			public abstract class BranchDependentTest : RunTaskTest
			{
				public void TestDoesNotCreateTaskRecord()
				{
					// Arrange
					using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
					{
						serviceTaskMock
							.Object
							.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

						// Act
						AssertExceptionThrown<Exception>(() => serviceTaskMock
							.As<IServiceTaskHandler>()
							.Object
							.Run(CancellationToken.None));

						// Assert
						var query = new ZQuery();
						query.AddToFilter(StmServiceTaskSchema.SST_ServiceTaskCode, hostedServiceConfigMock.Object.Code);
						var result = Factory.LoadTop1<StmServiceTask>(query);
						AssertNull(result);
					}
				}

				public void TestDoesNotRunOnNoTaskRecord()
				{
					// Arrange
					using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
					{
						serviceTaskMock
							.Object
							.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

						// Act
						AssertExceptionThrown<Exception>(() => serviceTaskMock
							.As<IServiceTaskHandler>()
							.Object
							.Run(CancellationToken.None));

						// Assert
						AssertNoExceptionThrown(() =>
						{
							serviceTaskMock
								.Protected()
								.Verify("RunTask", Times.Never(), ItExpr.IsAny<CancellationToken>());
						});
					}
				}

				public abstract void TestEnvironmentIsLogged();

				public class CanRunInAnyBranchTest : BranchDependentTest
				{
					protected override void SetUp()
					{
						base.SetUp();
						hostedServiceConfigMock
							.SetupGet(config => config.CanRunInAnyBranch)
							.Returns(true);
					}

					public void TestPreservesDefaultBranch()
					{
						// Arrange
						using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
						var task = Factory.New<StmServiceTask>();
						task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
						task.SST_NextRunTime = ZDateTimeOffset.Now;
						task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
						Factory.Save();

						using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
						{
							serviceTaskMock
								.Object
								.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

							var currentBranchCode = Env.Instance.CurrentBranch.Code;
							var resultBranchCode = string.Empty;
							serviceTaskMock
								.Protected()
								.Setup("RunTask", ItExpr.IsAny<CancellationToken>())
								.Callback(() =>
								{
									resultBranchCode = Env.Instance.CurrentBranch.Code;
								});

							// Act
							serviceTaskMock
								.As<IServiceTaskHandler>()
								.Object
								.Run(CancellationToken.None);

							// Assert
							AssertNotNullOrEmpty(resultBranchCode);
							AssertEquals(currentBranchCode, resultBranchCode);
						}
					}

					public override void TestEnvironmentIsLogged()
					{
						// Arrange
						using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
						var task = Factory.New<StmServiceTask>();
						task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
						task.SST_NextRunTime = ZDateTimeOffset.Now;
						task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
						Factory.Save();

						using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
						{
							serviceTaskMock
								.Object
								.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);
							loggerMock.Invocations.Clear();

							// Act
							serviceTaskMock
								.As<IServiceTaskHandler>()
								.Object
								.Run(CancellationToken.None);

							// Assert
							AssertNoExceptionThrown(() =>
							{
								loggerMock.VerifyLog(LogLevel.Debug, o => Regex.IsMatch(o, @"Using environment: user \[.+\], branch \[.+\], department \[.+\]."),
									Times.Once);
								loggerMock.VerifyNoOtherCalls();
							});
						}
					}
				}

				public class RunInSpecificBranchTest : BranchDependentTest
				{
					protected override void SetUp()
					{
						base.SetUp();
						hostedServiceConfigMock
							.SetupGet(config => config.CanRunInAnyBranch)
							.Returns(false);

						glbCompany = Factory.NewWithValidTestData<GlbCompany>();
						glbCompany.GC_IsActive = true;
						glbBranch = Factory.NewWithValidTestData<GlbBranch>();
						glbBranch.GB_GC = glbCompany.PK;
						glbBranch.GB_IsActive = true;
						Factory.Save();
					}

					public void TestChangesToRequestedBranch()
					{
						// Arrange
						using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
						var task = Factory.New<StmServiceTask>();
						task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
						task.SST_GB_Branch = glbBranch.PK;
						task.SST_NextRunTime = ZDateTimeOffset.Now;
						task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
						Factory.Save();

						using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
						{
							serviceTaskMock
								.Object
								.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

							var currentBranchCode = Env.Instance.CurrentBranch.Code;
							var resultBranchCode = string.Empty;
							serviceTaskMock
								.Protected()
								.Setup("RunTask", ItExpr.IsAny<CancellationToken>())
								.Callback(() =>
								{
									resultBranchCode = Env.Instance.CurrentBranch.Code;
								});

							// Act
							serviceTaskMock
								.As<IServiceTaskHandler>()
								.Object
								.Run(CancellationToken.None);

							// Assert
							AssertNotNullOrEmpty(resultBranchCode);
							AssertEquals(glbBranch.GB_Code, resultBranchCode);
							AssertNotEquals(currentBranchCode, resultBranchCode);
						}
					}

					public void TestChangesToRequestedBranchForThread()
					{
						// Arrange
						using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
						var task = Factory.New<StmServiceTask>();
						task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
						task.SST_GB_Branch = glbBranch.PK;
						task.SST_NextRunTime = ZDateTimeOffset.Now;
						task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
						Factory.Save();

						using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
						{
							serviceTaskMock
								.Object
								.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

							var currentBranchCode = Env.Instance.CurrentBranch.Code;
							var resultBranchCode = string.Empty;
							serviceTaskMock
								.Protected()
								.Setup("RunTask", ItExpr.IsAny<CancellationToken>())
								.Callback(() =>
								{
									var thread = new Thread(() =>
									{
										resultBranchCode = Env.Instance.CurrentBranch.Code;
									});
									thread.Start();
									thread.Join();
								});

							// Act
							serviceTaskMock
								.As<IServiceTaskHandler>()
								.Object
								.Run(CancellationToken.None);

							// Assert
							AssertNotNullOrEmpty(resultBranchCode);
							AssertEquals(glbBranch.GB_Code, resultBranchCode);
							AssertNotEquals(currentBranchCode, resultBranchCode);
						}
					}

					public void TestChangesToRequestedBranchForThreadPool()
					{
						// Arrange
						using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
						var task = Factory.New<StmServiceTask>();
						task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
						task.SST_GB_Branch = glbBranch.PK;
						task.SST_NextRunTime = ZDateTimeOffset.Now;
						task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
						Factory.Save();

						using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
						{
							serviceTaskMock
								.Object
								.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

							var currentBranchCode = Env.Instance.CurrentBranch.Code;
							var resultBranchCode = string.Empty;
							serviceTaskMock
								.Protected()
								.Setup("RunTask", ItExpr.IsAny<CancellationToken>())
								.Callback(() =>
								{
									using var resetEvent = new ManualResetEvent(false);
									ThreadPool.QueueUserWorkItem(_ => 
									{
										resultBranchCode = Env.Instance.CurrentBranch.Code;
										resetEvent.Set();
									});
									resetEvent.WaitOne();
								});

							// Act
							serviceTaskMock
								.As<IServiceTaskHandler>()
								.Object
								.Run(CancellationToken.None);

							// Assert
							AssertNotNullOrEmpty(resultBranchCode);
							AssertEquals(glbBranch.GB_Code, resultBranchCode);
							AssertNotEquals(currentBranchCode, resultBranchCode);
						}
					}

					public void TestChangesToRequestedBranchForTask()
					{
						// Arrange
						using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
						var task = Factory.New<StmServiceTask>();
						task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
						task.SST_GB_Branch = glbBranch.PK;
						task.SST_NextRunTime = ZDateTimeOffset.Now;
						task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
						Factory.Save();

						using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
						{
							serviceTaskMock
								.Object
								.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

							var currentBranchCode = Env.Instance.CurrentBranch.Code;
							var resultBranchCode = string.Empty;
							serviceTaskMock
								.Protected()
								.Setup("RunTask", ItExpr.IsAny<CancellationToken>())
								.Callback(() =>
								{
									Task.Run(() =>
										{
											resultBranchCode = Env.Instance.CurrentBranch.Code;
										})
										.Wait();
								});

							// Act
							serviceTaskMock
								.As<IServiceTaskHandler>()
								.Object
								.Run(CancellationToken.None);

							// Assert
							AssertNotNullOrEmpty(resultBranchCode);
							AssertEquals(glbBranch.GB_Code, resultBranchCode);
							AssertNotEquals(currentBranchCode, resultBranchCode);
						}
					}

					public void TestDoesNotChangeContextIfCurrentBranchIsTheSame()
					{
						// Arrange
						using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
						using (ObjectFactory.Substitute(attributeProviderMock.Object))
						{
							serviceTaskMock
								.Object
								.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

							var task = Factory.New<StmServiceTask>();
							task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
							task.SST_GB_Branch = Env.CurrentBranchPK;
							task.SST_NextRunTime = ZDateTimeOffset.Now;
							task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
							Factory.Save();

							UserContext currentContext = null;
							serviceTaskMock
								.Protected()
								.Setup("RunTask", ItExpr.IsAny<CancellationToken>())
								.Callback(() =>
								{
									currentContext = Env.CurrentUserContext;
								});

							// Act
							serviceTaskMock
								.As<IServiceTaskHandler>()
								.Object
								.Run(CancellationToken.None);

							// Assert
							AssertEquals(true, ReferenceEquals(currentContext, Env.CurrentUserContext));
						}
					}

					public override void TestEnvironmentIsLogged()
					{
						// Arrange
						using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
						var task = Factory.New<StmServiceTask>();
						task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
						task.SST_GB_Branch = glbBranch.PK;
						task.SST_NextRunTime = ZDateTimeOffset.Now;
						task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";
						Factory.Save();

						using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
						{
							serviceTaskMock
								.Object
								.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);
							loggerMock.Invocations.Clear();

							// Act
							serviceTaskMock
								.As<IServiceTaskHandler>()
								.Object
								.Run(CancellationToken.None);

							// Assert
							AssertNoExceptionThrown(() =>
							{
								loggerMock.VerifyLog(LogLevel.Debug, o => Regex.IsMatch(o, @"Using environment: user \[.+\], branch \[.+\], department \[.+\]."),
									Times.Once);
								loggerMock.VerifyNoOtherCalls();
							});
						}
					}

					GlbBranch glbBranch;
					GlbCompany glbCompany;
				}
			}

			public class RequiresCompanyInCountryAndCanRunInAnyBranchTest : RunTaskTest
			{
				protected override void SetUp()
				{
					base.SetUp();

					hostedServiceConfigMock
						.SetupGet(config => config.RequiresCompanyInCountry)
						.Returns(string.Join(",", new string[] { Constants.CountryCodes.Bahamas, Constants.CountryCodes.Canada }));
					hostedServiceConfigMock
						.SetupGet(config => config.CanRunInAnyBranch)
						.Returns(true);
					hostedServiceConfigMock
						.SetupGet(attribute => attribute.DefaultSchedule)
						.Returns(Mock.Of<IDefaultSchedule>(schedule => schedule.RunEvery == "1minute"));
				}

				public void TestOldestActiveBranchFromOldestActiveCompanyInCountry()
				{
					// Arrange
					using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
					var companies = Factory.Load<GlbCompany>(new ZQuery());
					companies.ForEach(gc => gc.GC_IsActive = false);

					var branches = Factory.Load<GlbBranch>(new ZQuery());
					branches.ForEach(gb => gb.GB_IsActive = false);

					var olderCompanyInCountry = Factory.NewWithValidTestData<GlbCompany>();
					olderCompanyInCountry.GC_RN_NKCountryCode = Constants.CountryCodes.Bahamas;
					olderCompanyInCountry.GC_IsActive = true;
					olderCompanyInCountry.GC_SystemCreateTimeUtc = DateTime.UtcNow - TimeSpan.FromDays(10);

					// suitable branch
					var olderBranchInOlderCompanyInCountry = Factory.NewWithValidTestData<GlbBranch>();
					olderBranchInOlderCompanyInCountry.GB_IsActive = true;
					olderBranchInOlderCompanyInCountry.GB_GC = olderCompanyInCountry.PK;
					olderBranchInOlderCompanyInCountry.GB_SystemCreateTimeUtc = DateTime.UtcNow - TimeSpan.FromDays(10);

					var newerBranchInOlderCompanyInCountry = Factory.NewWithValidTestData<GlbBranch>();
					newerBranchInOlderCompanyInCountry.GB_IsActive = true;
					newerBranchInOlderCompanyInCountry.GB_GC = olderCompanyInCountry.PK;

					var companyInCountryWithoutBranches = Factory.NewWithValidTestData<GlbCompany>();
					companyInCountryWithoutBranches.GC_RN_NKCountryCode = Constants.CountryCodes.Bahamas;
					companyInCountryWithoutBranches.GC_IsActive = true;

					var oldestCompanyNotInCountry = Factory.NewWithValidTestData<GlbCompany>();
					oldestCompanyNotInCountry.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
					oldestCompanyNotInCountry.GC_IsActive = true;
					oldestCompanyNotInCountry.GC_SystemCreateTimeUtc = DateTime.UtcNow - TimeSpan.FromDays(20);

					var oldestBranchOfOldestCompanyNotInCountry = Factory.NewWithValidTestData<GlbBranch>();
					oldestBranchOfOldestCompanyNotInCountry.GB_IsActive = true;
					oldestBranchOfOldestCompanyNotInCountry.GB_GC = oldestCompanyNotInCountry.PK;
					oldestBranchOfOldestCompanyNotInCountry.GB_SystemCreateTimeUtc = DateTime.UtcNow - TimeSpan.FromDays(20);

					var inactiveCompanyInCountry = Factory.NewWithValidTestData<GlbCompany>();
					inactiveCompanyInCountry.GC_RN_NKCountryCode = Constants.CountryCodes.Bahamas;
					inactiveCompanyInCountry.GC_IsActive = false;

					var activeBranchOfInactiveCompanyInCountry = Factory.NewWithValidTestData<GlbBranch>();
					activeBranchOfInactiveCompanyInCountry.GB_IsActive = true;
					activeBranchOfInactiveCompanyInCountry.GB_GC = inactiveCompanyInCountry.PK;

					var newerCompanyInCountry = Factory.NewWithValidTestData<GlbCompany>();
					newerCompanyInCountry.GC_RN_NKCountryCode = Constants.CountryCodes.Bahamas;
					newerCompanyInCountry.GC_IsActive = true;

					var newerBranchOfNewerCompanyInCountry = Factory.NewWithValidTestData<GlbBranch>();
					newerBranchOfNewerCompanyInCountry.GB_IsActive = true;
					newerBranchOfNewerCompanyInCountry.GB_GC = newerCompanyInCountry.PK;

					var task = Factory.New<StmServiceTask>();
					task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
					task.SST_GB_Branch = ZGuid.Empty;
					task.SST_NextRunTime = ZDateTimeOffset.Now;
					task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";

					Factory.Save();

					using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
					{
						serviceTaskMock
							.Object
							.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

						var currentBranchCode = Env.Instance.CurrentBranch.Code;
						AssertEquals(oldestBranchOfOldestCompanyNotInCountry.GB_Code, currentBranchCode);

						var resultBranchCode = string.Empty;

						serviceTaskMock
							.Protected()
							.Setup("RunTask", ItExpr.IsAny<CancellationToken>())
							.Callback(() =>
							{
								resultBranchCode = Env.Instance.CurrentBranch.Code;
							});

						// Act
						serviceTaskMock
							.As<IServiceTaskHandler>()
							.Object
							.Run(CancellationToken.None);

						// Assert
						AssertEquals(olderBranchInOlderCompanyInCountry.GB_Code, resultBranchCode);
					}
				}

				public void TestOldestActiveBranchHavingUnlocoInCountry()
				{
					// Arrange
					using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
					var companies = Factory.Load<GlbCompany>(new ZQuery());
					companies.ForEach(gc => gc.GC_IsActive = false);

					var branches = Factory.Load<GlbBranch>(new ZQuery());
					branches.ForEach(gb => gb.GB_IsActive = false);

					var bsUNLOCO = Factory.LoadTop1<RefUNLOCO>(
						new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Constants.CountryCodes.Bahamas));
					var auUNLOCO = Factory.LoadTop1<RefUNLOCO>(
						new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, Constants.CountryCodes.Australia));

					var oldestCompanyNotInCountry = Factory.NewWithValidTestData<GlbCompany>();
					oldestCompanyNotInCountry.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
					oldestCompanyNotInCountry.GC_IsActive = true;
					oldestCompanyNotInCountry.GC_SystemCreateTimeUtc = DateTime.UtcNow - TimeSpan.FromDays(20);

					var oldestBranchInOldestCompanNotInCountry = Factory.NewWithValidTestData<GlbBranch>();
					oldestBranchInOldestCompanNotInCountry.GB_GC = oldestCompanyNotInCountry.PK;
					oldestBranchInOldestCompanNotInCountry.GB_RL_NKHomePort = null;
					oldestBranchInOldestCompanNotInCountry.GB_IsActive = true;
					oldestBranchInOldestCompanNotInCountry.GB_SystemCreateTimeUtc = DateTime.UtcNow - TimeSpan.FromDays(20);

					var branchWithUnlocoNotInCountry = Factory.NewWithValidTestData<GlbBranch>();
					branchWithUnlocoNotInCountry.GB_GC = oldestCompanyNotInCountry.PK;
					branchWithUnlocoNotInCountry.GB_RL_NKHomePort = auUNLOCO.Code;
					branchWithUnlocoNotInCountry.GB_IsActive = true;

					var inactiveBranchWithUnlocoInCountry = Factory.NewWithValidTestData<GlbBranch>();
					inactiveBranchWithUnlocoInCountry.GB_GC = oldestCompanyNotInCountry.PK;
					inactiveBranchWithUnlocoInCountry.GB_RL_NKHomePort = bsUNLOCO.Code;
					inactiveBranchWithUnlocoInCountry.GB_IsActive = false;

					var newerBranchWithUnlocoInCountry = Factory.NewWithValidTestData<GlbBranch>();
					newerBranchWithUnlocoInCountry.GB_GC = oldestCompanyNotInCountry.PK;
					newerBranchWithUnlocoInCountry.GB_RL_NKHomePort = bsUNLOCO.Code;
					newerBranchWithUnlocoInCountry.GB_IsActive = true;

					// suitable branch
					var olderBranchWithUnlocoInCountry = Factory.NewWithValidTestData<GlbBranch>();
					olderBranchWithUnlocoInCountry.GB_GC = oldestCompanyNotInCountry.PK;
					olderBranchWithUnlocoInCountry.GB_RL_NKHomePort = bsUNLOCO.Code;
					olderBranchWithUnlocoInCountry.GB_IsActive = true;
					olderBranchWithUnlocoInCountry.GB_SystemCreateTimeUtc = DateTime.UtcNow - TimeSpan.FromDays(10);

					var task = Factory.New<StmServiceTask>();
					task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
					task.SST_GB_Branch = ZGuid.Empty;
					task.SST_NextRunTime = ZDateTimeOffset.Now;
					task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";

					Factory.Save();

					using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
					{
						serviceTaskMock
							.Object
							.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

						var currentBranchCode = Env.Instance.CurrentBranch.Code;
						AssertEquals(oldestBranchInOldestCompanNotInCountry.GB_Code, currentBranchCode);

						var resultBranchCode = string.Empty;

						serviceTaskMock
							.Protected()
							.Setup("RunTask", ItExpr.IsAny<CancellationToken>())
							.Callback(() =>
							{
								resultBranchCode = Env.Instance.CurrentBranch.Code;
							});

						// Act
						serviceTaskMock
							.As<IServiceTaskHandler>()
							.Object
							.Run(CancellationToken.None);

						// Assert
						AssertEquals(olderBranchWithUnlocoInCountry.GB_Code, resultBranchCode);
					}
				}

				public void TestNoSuitableBranch()
				{
					// Arrange
					using var mock = ObjectFactory.Substitute(attributeProviderMock.Object);
					var companies = Factory.Load<GlbCompany>(new ZQuery());
					companies.ForEach(gc => gc.GC_IsActive = false);

					var branches = Factory.Load<GlbBranch>(new ZQuery());
					branches.ForEach(gb => gb.GB_IsActive = false);

					var oldestCompanyNotInCountry = Factory.NewWithValidTestData<GlbCompany>();
					oldestCompanyNotInCountry.GC_RN_NKCountryCode = Constants.CountryCodes.Australia;
					oldestCompanyNotInCountry.GC_IsActive = true;
					oldestCompanyNotInCountry.GC_SystemCreateTimeUtc = DateTime.UtcNow - TimeSpan.FromDays(20);

					var oldestBranchInOldestCompanNotInCountry = Factory.NewWithValidTestData<GlbBranch>();
					oldestBranchInOldestCompanNotInCountry.GB_GC = oldestCompanyNotInCountry.PK;
					oldestBranchInOldestCompanNotInCountry.GB_RL_NKHomePort = null;
					oldestBranchInOldestCompanNotInCountry.GB_IsActive = true;
					oldestBranchInOldestCompanNotInCountry.GB_SystemCreateTimeUtc = DateTime.UtcNow - TimeSpan.FromDays(20);

					var task = Factory.New<StmServiceTask>();
					task.SST_ServiceTaskCode = hostedServiceConfigMock.Object.Code;
					task.SST_GB_Branch = ZGuid.Empty;
					task.SST_NextRunTime = ZDateTimeOffset.Now;
					task.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorMinutes Period=\"10\" /><SecondaryProcessesMaxCount>0</SecondaryProcessesMaxCount></ScheduleConfig>";

					Factory.Save();

					using (Env.SetTemporaryUserContext(Mock.Of<IUserContext>()))
					{
						serviceTaskMock
							.Object
							.InitializeRunningEnvironment(hostedServiceConfigMock.Object, string.Empty, loggerMock.Object);

						var currentBranchCode = Env.Instance.CurrentBranch.Code;
						AssertEquals(oldestBranchInOldestCompanNotInCountry.GB_Code, currentBranchCode);

						var resultBranchCode = string.Empty;

						serviceTaskMock
							.Protected()
							.Setup("RunTask", ItExpr.IsAny<CancellationToken>())
							.Callback(() =>
							{
								resultBranchCode = Env.Instance.CurrentBranch.Code;
							});

						// Act
						serviceTaskMock
							.As<IServiceTaskHandler>()
							.Object
							.Run(CancellationToken.None);

						// Assert
						AssertEquals(currentBranchCode, resultBranchCode);
					}
				}
			}
		}

		#region Test Classes

		public class ProviderForTesting : BasicServiceProvider
		{
			readonly Action runAction;

			public ProviderForTesting()
				: this(factory: null, new WebRequestDefaultProxyWrapper())
			{
			}

			public ProviderForTesting(BusinessObjectFactory factory, IWebRequestDefaultProxyWrapper proxyWrapper, Action runAction = null)
				: base(
					factory,
					proxyWrapper,
					new Mock<ILoggerFactory>().Object,
					new NativeServiceTaskLoader(Mock.Of<IClientHostedServiceAttributeProvider>(), Mock.Of<IServiceTaskScheduleStatusProvider>(), Mock.Of<IHostedServiceBusinessObjectBindingsProvider>(), Mock.Of<IDateTimeProvider>()))
			{
				this.runAction = runAction;
				Mock.Get(LoggerFactory).Setup(m => m.NewServiceTaskLogger(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(logger);
			}

			protected override void RunTask(CancellationToken cancellationToken)
			{
				DefaultWebProxyBeforeRunTask = WebRequest.DefaultWebProxy;

				runAction?.Invoke();

				RunCount++;
			}

			public TestServiceLogger TestLogger
			{
				get { return logger; }
			}

			readonly TestServiceLogger logger = new TestServiceLogger();

			public int RunCount { get; set; }

			public IWebProxy DefaultWebProxyBeforeRunTask;
		}

		class ProviderWithBackgroundAppDomainWorker : ProviderForTesting
		{
			protected override void RunTask(CancellationToken cancellationToken)
			{
				var asyncResult = BackgroundAppDomainWorker.QueueWorkItem("TestWorkItem", delegate
					{
						throw new InvalidOperationException("Test Exception in Child AppDomain");
					});

				var i = 0;
				while (!asyncResult.IsCompleted)
				{
					Thread.Sleep(1000);
					if (++i > 30)
					{
						break;
					}
				}
			}
		}

		#endregion
	}
}
