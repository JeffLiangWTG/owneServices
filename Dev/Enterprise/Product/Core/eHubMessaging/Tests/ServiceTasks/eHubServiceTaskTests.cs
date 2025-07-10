using System;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Threading;
using CargoWise.BrandManager;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.eHubMessaging.Business;
using Enterprise.eHubMessaging.ServiceTasks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.eHubMessaging.Tests.ServiceTasks
{
	[TestsSubclassesOf(typeof(eHubServiceTask))]
	abstract class eHubServiceTaskTest<TServiceTask, TServiceTaskJob> : ServiceTaskTestCase<TServiceTask>
		where TServiceTask : eHubServiceTask
		where TServiceTaskJob : ServiceTaskJob
	{
		public void TestRunTask_ClientCodeCargowiseNetworkEHubTestRegistryFalse()
		{
			var serviceTask = new Mock<TServiceTask>() { CallBase = true };
			var notifier = new Mock<INotifications>(MockBehavior.Strict);
			serviceTask.Setup(m => m.Notifier).Returns(notifier.Object);

			serviceTask.Setup(m => m.IsClientEnterpriseCode).Returns(true);
			serviceTask.SetupSequence(m => m.DomainName)
				.Returns("corporate.cargowise.com")
				.Returns("dev.corporate.cargowise.com")
				.Returns("wtg.zone")
				.Returns("sand.wtg.zone");
			serviceTask.Setup(m => m.IsEHubTestingEnabled).Returns(false);

			notifier.Setup(m => m.Add(It.IsAny<INotification>()))
				.Callback(new Action<INotification>(notification =>
				{
					Assert(notification.GetType().IsAssignableFrom(typeof(InfoNotification)));
					AssertEquals($"{BrandingFactory.Instance.ProductName} has detected that this installation belongs to a CargoWise client, but is currently run within CargoWise corporate network for testing. In this case, all eHub Service tasks are disabled to prevent accidentally sending or retrieving live messages for this client.", notification.Message);
				}));

			serviceTask.Object.RunTask();
			serviceTask.Object.RunTask();
			serviceTask.Object.RunTask();
			serviceTask.Object.RunTask();
			serviceTask.VerifyAll();
			notifier.VerifyAll();
			notifier.Verify(m => m.Add(It.IsAny<INotification>()), Times.Exactly(4));
		}

		public void TestRunTask_CargowiseCodeCargowiseNetworkEHubTestRegistryFalse()
		{
			var serviceTask = new Mock<TServiceTask>() { CallBase = true };
			var notifier = new Mock<INotifications>(MockBehavior.Strict);
			serviceTask.Setup(m => m.Notifier).Returns(notifier.Object);

			serviceTask.Setup(m => m.IsClientEnterpriseCode).Returns(false);
			serviceTask.Setup(m => m.DomainName).Returns("corporate.cargowise.com");
			serviceTask.Setup(m => m.IsEHubTestingEnabled).Returns(false);

			notifier.Setup(m => m.Add(It.IsAny<INotification>()))
				.Callback(new Action<INotification>(notification =>
				{
					Assert(notification.GetType().IsAssignableFrom(typeof(InfoNotification)));
					AssertEquals($"{BrandingFactory.Instance.ProductName} has detected that this installation is a CargoWise test or internal production system, with Enterprise code HYE, EDI, EHW or WTL. Service Task is therefore disabled by default. Please turn on System->Testing->eHub Testing registry explicitly if you want to test eHub messaging.", notification.Message);
				}));

			serviceTask.Object.RunTask();
			serviceTask.VerifyAll();
			notifier.VerifyAll();
		}

		public void TestRunTask_CargowiseInternalLicense()
		{
			var serviceTask = new Mock<TServiceTask>() { CallBase = true };
			var notifier = new Mock<INotifications>(MockBehavior.Strict);
			serviceTask.Setup(m => m.Notifier).Returns(notifier.Object);

			serviceTask.Setup(m => m.DomainName).Returns("corporate.cargowise.com");
			serviceTask.Setup(m => m.IsEHubTestingEnabled).Returns(false);

			notifier.Setup(m => m.Add(It.IsAny<INotification>()))
				.Callback(new Action<INotification>(notification =>
				{
					Assert(notification.GetType().IsAssignableFrom(typeof(InfoNotification)));
					AssertEquals($"{BrandingFactory.Instance.ProductName} has detected that this installation is a CargoWise test or internal production system, with Enterprise code HYE, EDI, EHW or WTL. Service Task is therefore disabled by default. Please turn on System->Testing->eHub Testing registry explicitly if you want to test eHub messaging.", notification.Message);
				}));

			ProductRegistration.Client.ProductRegister.TestHelper.KeyForTest.EnterpriseCodeForTest = "HYE";
			serviceTask.Object.RunTask();
			ProductRegistration.Client.ProductRegister.TestHelper.KeyForTest.EnterpriseCodeForTest = "EDI";
			serviceTask.Object.RunTask();
			ProductRegistration.Client.ProductRegister.TestHelper.KeyForTest.EnterpriseCodeForTest = "EHW";
			serviceTask.Object.RunTask();
			ProductRegistration.Client.ProductRegister.TestHelper.KeyForTest.EnterpriseCodeForTest = "WTL";
			serviceTask.Object.RunTask();
			serviceTask.VerifyAll();
			notifier.VerifyAll();
			notifier.Verify(m => m.Add(It.IsAny<INotification>()), Times.Exactly(4));
		}

		[ExpectNoExceptions]
		public void TestRunTask_ClientNetworkEHubTestModeFalse()
		{
			var serviceTask = new Mock<TServiceTask>() { CallBase = true };
			var notifier = new Mock<INotifications>(MockBehavior.Strict);
			var serviceTaskJob = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);

			serviceTask.Setup(m => m.DomainName).Returns("test.com");
			serviceTask.Setup(m => m.GetJobs()).Returns(new[] { serviceTaskJob.Object });
			serviceTaskJob.Setup(m => m.Execute(It.IsAny<CancellationToken>()));
			serviceTaskJob.Setup(m => m.NextExecuteIterationIsScheduled).Returns(false);

			serviceTask.Object.RunTask();
			serviceTask.VerifyAll();
			notifier.VerifyAll();
			serviceTaskJob.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestRunTask_CargowiseNetworkEHubTestModeTrue()
		{
			var mock = new MockRepository(MockBehavior.Default);
			var serviceTask = new Mock<TServiceTask>() { CallBase = true };
			var notifier = mock.Create<INotifications>(MockBehavior.Strict);
			var serviceTaskJob = mock.Create<IeHubServiceTaskJob>(MockBehavior.Strict);

			serviceTask.Setup(m => m.Notifier).Returns(notifier.Object);
			serviceTask.Setup(m => m.DomainName).Returns("corporate.cargowise.com");
			serviceTask.Setup(m => m.IsEHubTestingEnabled).Returns(true);
			serviceTask.Setup(m => m.GetJobs()).Returns(new[] { serviceTaskJob.Object });
			serviceTaskJob.Setup(m => m.Execute(It.IsAny<CancellationToken>()));
			serviceTaskJob.Setup(m => m.NextExecuteIterationIsScheduled).Returns(false);

			serviceTask.Object.RunTask();
			mock.VerifyAll();
		}

		public void TestRunTask_Exceptions()
		{
			var serviceTask = new Mock<TServiceTask>() { CallBase = true };
			var notifier = new Mock<INotifications>(MockBehavior.Strict);

			serviceTask.Setup(m => m.DomainName).Returns("corporate.cargowise.com");
			serviceTask.Setup(m => m.IsEHubTestingEnabled).Returns(true);
			serviceTask.Setup(m => m.GetJobs()).Throws(new Exception("test"));

			AssertExceptionThrown(typeof(Exception), "test", serviceTask.Object.RunTask);
			serviceTask.VerifyAll();
			notifier.VerifyAll();
		}

		public void TestJobsNotCached()
		{
			var task = CreateMock();

			var jobs1 = task.Object.GetJobs().ToList();
			var jobs2 = task.Object.GetJobs().ToList();

			AssertEquals("Jobs count is fixed", jobs1.Count, jobs2.Count);
			for (int i = 0; i < jobs1.Count; i++)
			{
				AssertEquals("Job types are the same", jobs1[i].GetType(), jobs2[i].GetType());
				AssertNotEquals("Jobs are not cached", jobs1[i], jobs2[i]);
			}
		}

		public virtual void TestJobExceptionDoesNotAffectOtherJobs()
		{
			JobExceptionDoesNotAffectOtherJobs(false);
		}

		protected void JobExceptionDoesNotAffectOtherJobs(bool exceptionsHandled)
		{
			var mockServiceTask = CreateMock();
			var mockServiceTaskJob1 = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);
			var exception1 = new Exception("Some exception");
			mockServiceTaskJob1.Setup(m => m.Execute(CancellationToken.None)).Throws(exception1);
			mockServiceTaskJob1.Setup(m => m.NextExecuteIterationIsScheduled).Returns(false);
			var mockServiceTaskJob2 = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);
			var exception2 = new NullReferenceException("Null Exception");
			mockServiceTaskJob2.Setup(m => m.Execute(CancellationToken.None)).Throws(exception2);
			mockServiceTaskJob2.Setup(m => m.NextExecuteIterationIsScheduled).Returns(false);
			var mockServiceTaskJob3 = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);
			mockServiceTaskJob3.Setup(m => m.Execute(It.IsAny<CancellationToken>()));
			mockServiceTaskJob3.Setup(m => m.NextExecuteIterationIsScheduled).Returns(false);
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskJob1.Object, mockServiceTaskJob2.Object, mockServiceTaskJob3.Object });

			if (exceptionsHandled)
			{
				AssertNoExceptionThrown(() => InitialiseAndRunTaskSchedule(mockServiceTask.Object));
				AssertEquals(2, ErrorReporter.TotalErrorCount);

				ErrorReporter.Clear();
			}
			else
			{
				var e = AssertExceptionThrown<AggregateException>(() => InitialiseAndRunTaskSchedule(mockServiceTask.Object));
				AssertArrayEqualsByElements(new[] { exception1, exception2 }, e.InnerExceptions.ToArray());
				mockServiceTaskJob3.VerifyAll();
			}
		}

		public void TestTaskStopsRunningAfterKnownJobException()
		{
			var error = SqlExceptionBuilder.CreateSqlError(6522, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors, new Win32Exception("The wait operation timed out"));
			var mockServiceTask = CreateMock(companySettingsManager: null, runContinuously: true);
			var mockServiceTaskJob1 = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);
			mockServiceTaskJob1.Setup(m => m.Execute(CancellationToken.None)).Throws(exception);
			mockServiceTaskJob1.Setup(m => m.NextExecuteIterationIsScheduled).Returns(true);
			var mockServiceTaskJob2 = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);
			mockServiceTaskJob2.Setup(m => m.Execute(It.IsAny<CancellationToken>()));
			mockServiceTaskJob2.Setup(m => m.NextExecuteIterationIsScheduled).Returns(true);
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskJob1.Object, mockServiceTaskJob2.Object });

			InitialiseAndRunTaskSchedule(mockServiceTask.Object);
			mockServiceTaskJob2.VerifyAll();
		}

		public virtual void TestStop()
		{
			var mockServiceTask = CreateMock();
			var mockServiceTaskJob = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);

			mockServiceTaskJob.Setup(m => m.Execute(mockServiceTask.Object.CancellationToken)).Throws(new OperationCanceledException());
			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskJob.Object });

			// run service task & check results
			AssertNoExceptionThrown(() => InitialiseAndRunTaskSchedule(mockServiceTask.Object));
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			mockServiceTask.Object.Notifier.AssertNotificationExists("Service task was canceled.");
		}

		public void TestContinuousRun()
		{
			var mockServiceTask = CreateMock(runContinuously: true);

			var mockServiceTaskJob = new Mock<IeHubServiceTaskJob>(MockBehavior.Strict);
			mockServiceTaskJob.Setup(m => m.Execute(It.IsAny<CancellationToken>()));
			mockServiceTaskJob.SetupSequence(m => m.NextExecuteIterationIsScheduled)
				.Returns(true)
				.Returns(true)
				.Returns(true)
				.Returns(false);

			mockServiceTask.Setup(m => m.GetJobs()).Returns(new[] { mockServiceTaskJob.Object });

			InitialiseAndRunTaskSchedule(mockServiceTask.Object);
			mockServiceTaskJob.VerifyAll();
		}

		protected const string configurationErrorMessage = "Error creating the Web Proxy specified in the 'system.net/defaultProxy' configuration section.";

		[TestDate(2016, 06, 12)]
		public void TestConfigurationErrorsExceptionWithInnerExceptionHandling()
		{
			var exception1 = new ConfigurationErrorsException(
				configurationErrorMessage,
				new TypeInitializationException("The type initializer for 'System.Net.HybridWebProxyFinder' threw an exception.", new NullReferenceException()));
			TestExceptionHandling(exception1, exception1.Message);

			var exception2 = new ConfigurationErrorsException(configurationErrorMessage, new DllNotFoundException());
			TestExceptionHandling(exception2, exception1.Message);
		}

		[TestDate(2016, 06, 12)]
		public virtual void TestConfigurationErrorsExceptionHandling()
		{
			TestExceptionNotHandled(new ConfigurationErrorsException(configurationErrorMessage));
		}

		[TestDate(2016, 06, 12)]
		public void TestInvalidOperationExceptionSpecialICaseHandling()
		{
			var expectedMessage = $"InvalidOperationException caused by Json Convert in Company : {Env.CurrentCompany.Code}, CompanyPK '{Env.CurrentCompanyPK}'";
			TestExceptionHandling(new InvalidOperationException("Timeouts are not supported on this stream."), expectedMessage);
		}

		[TestDate(2016, 06, 12)]
		public void TestTransactionNoLongerHasConnectionExceptionHandling()
		{
			var message = "The transaction no longer has an active connection.";
			TestExceptionHandling(new InvalidOperationException(message), message);
		}

		[TestDate(2016, 06, 12)]
		public void TestSqlTimeoutExceptionHandling()
		{
			var error = SqlExceptionBuilder.CreateSqlError(6522, byte.MaxValue, byte.MinValue, Db.Connection.ServerName, "Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.", "", 0);
			var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
			var exception = SqlExceptionBuilder.CreateSqlException(errors, new Win32Exception("The wait operation timed out"));
			TestExceptionHandling(exception, exception.Message);
		}

		public void TestMinimumPeriod()
		{
			var serviceAttributes = GetHostedServiceAttributes().Single();
			string expectedMinimum;
			switch (serviceAttributes.Code)
			{
				case ServiceTaskCodes.EHI:
				case ServiceTaskCodes.OCS:
				case ServiceTaskCodes.EHO:
				case ServiceTaskCodes.USS:
				case ServiceTaskCodes.THI:
					expectedMinimum = "1minute";
					break;
				case ServiceTaskCodes.EAM:
					expectedMinimum = "10seconds";
					break;
				case ServiceTaskCodes.USF:
					expectedMinimum = "1day";
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			AssertEquals(expectedMinimum, serviceAttributes.MinimumPeriod);
		}

		protected void TestExceptionNotHandled(Exception e)
		{
			var mockServiceTask = CreateMockWithJobException(e);
			AssertExceptionThrown(e.Message, e.GetType(), () => InitialiseAndRunTaskSchedule(mockServiceTask.Object));
		}

		protected virtual void TestExceptionHandling(Exception e, string logMessage, bool logExists = true, bool hasErrorReport = false, bool shouldContainDiagnostics = false, bool shouldReportCommunicationExceptionsAsIssues = false, bool isProduction = false)
		{
			var mockServiceTask = CreateMockWithJobException(e, isProduction);
			var exceptionString = e.ToString();
			var stackTraceIndex = exceptionString.IndexOf("at ");
			var fullLogMessage = logMessage + (shouldContainDiagnostics ? "\r\n**Diagnostics**" : "") + "\r\nInternal Exception: " + ((stackTraceIndex >= 0) ? exceptionString.Remove(stackTraceIndex) : exceptionString);
			var errorReporterMessage = hasErrorReport ? mockServiceTask.Object.GetErrorReportKey(e) : null;

			if (hasErrorReport)
			{
				AssertNoExceptionThrown(() => InitialiseAndRunTaskSchedule(mockServiceTask.Object));
				AssertError(mockServiceTask.Object, logMessage, fullLogMessage, logExists, mockServiceTask.Object.GetErrorReportKey(e));
			}
			else if (shouldReportCommunicationExceptionsAsIssues)
			{
				using (eHubMessagingRegistry.Instance.eHubOutageExpiryTimeInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
				{
					SetOutageStartTime(mockServiceTask.Object, DateTime.MinValue);
					AssertNoExceptionThrown(() => InitialiseAndRunTaskSchedule(mockServiceTask.Object));
					AssertWarning(mockServiceTask.Object, fullLogMessage, logExists);

					TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(3);
					mockServiceTask = CreateMockWithJobException(e, isProduction);
					AssertNoExceptionThrown(() => InitialiseAndRunTaskSchedule(mockServiceTask.Object));
					AssertWarning(mockServiceTask.Object, fullLogMessage, logExists);

					TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(3);
					mockServiceTask = CreateMock(isProduction: isProduction);
					var job = StubMockJob(mockServiceTask);
					job.Setup(m => m.ExecuteInternal());
					AssertNoExceptionThrown(() => InitialiseAndRunTaskSchedule(mockServiceTask.Object));
					AssertEquals(string.Empty, ErrorReporter.LastMessageReported);

					TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(3);
					mockServiceTask = CreateMockWithJobException(e, isProduction);
					AssertNoExceptionThrown(() => InitialiseAndRunTaskSchedule(mockServiceTask.Object));
					AssertWarning(mockServiceTask.Object, fullLogMessage, logExists);

					TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(3);
					mockServiceTask = CreateMockWithJobException(e, isProduction);
					AssertNoExceptionThrown(() => InitialiseAndRunTaskSchedule(mockServiceTask.Object));
					AssertWarning(mockServiceTask.Object, fullLogMessage, logExists);

					TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(3);
					mockServiceTask = CreateMockWithJobException(e, isProduction);
					AssertNoExceptionThrown(() => InitialiseAndRunTaskSchedule(mockServiceTask.Object));
					AssertError(mockServiceTask.Object, logMessage, fullLogMessage, logExists, errorReporterMessage);
				}
			}
			else
			{
				using (eHubMessagingRegistry.Instance.eHubOutageExpiryTimeInMinutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
				{
					SetOutageStartTime(mockServiceTask.Object, TestDateAttribute.Date.AddMinutes(-10));
					AssertNoExceptionThrown(() =>
					{
						InitialiseTaskSchedule(mockServiceTask.Object, out _, out var scheduleGovernor);
						scheduleGovernor.SetBranchPk(Env.CurrentBranchPK);
						RunTaskSchedule(mockServiceTask.Object);
					});
					AssertWarning(mockServiceTask.Object, fullLogMessage, logExists);
				}
			}
		}

		protected virtual void SetOutageStartTime(TServiceTask serviceTask, DateTime startTime)
		{
		}

		void AssertWarning(TServiceTask mockServiceTask, string logMessage, bool logExists)
		{
			AssertNotification(mockServiceTask, logMessage, logExists, ErrorType.Warning);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			AssertNoErrorEmail();
		}

		protected void AssertError(TServiceTask mockServiceTask, string message, string fullLogMessage, bool logExists, string errorReporterMessage)
		{
			AssertNotification(mockServiceTask, fullLogMessage, logExists, ErrorType.Error);
			Assert("Wrong error reported", ErrorReporter.LastMessageReported.StartsWith(errorReporterMessage ?? message));
			ErrorReporter.Clear();
		}

		void AssertNotification(TServiceTask mockServiceTask, string logMessage, bool logExists, ErrorType errorType)
		{
			if (logExists)
			{
				mockServiceTask.Notifier.AssertNotificationContains(logMessage, errorType);
			}
			else
			{
				mockServiceTask.Notifier.AssertNotificationDoesNotExists(logMessage);
			}
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			ProductRegistration.Client.ProductRegister.TestHelper.KeyForTest.EnterpriseCodeForTest = "NUL"; // to avoid IsDeveloperSystem() check
			TestHelpers.CreateStaff("TST", Factory);
		}

		protected abstract Mock<TServiceTask> CreateMock(ICompanySettingsManager companySettingsManager = null, bool runContinuously = false, bool isProduction = false);

		protected void SetupMock(Mock<TServiceTask> mockServiceTask, ICompanySettingsManager companySettingsManager = null, bool runContinuously = false, bool isProduction = false)
		{
			var actualCompanySettingsManager = companySettingsManager ?? CreateMockCompanySettingsManager().Object;
			mockServiceTask.Setup(m => m.RunContinuously).Returns(runContinuously);
			mockServiceTask.Setup(m => m.IsCargoWiseDomain).Returns(true);
			mockServiceTask.Setup(m => m.IsEHubTestingEnabled).Returns(true);
			mockServiceTask.Setup(m => m.CompanySettingsManager).Returns(actualCompanySettingsManager);
			mockServiceTask.Setup(m => m.IsProduction).Returns(isProduction);
			mockServiceTask.Object.Notifier = new NotificationBuffer();
		}

		protected Mock<TServiceTask> CreateMockWithJobException(Exception e, bool isProduction = false)
		{
			var mockServiceTask = CreateMock(isProduction: isProduction);
			StubMockJobWithException(mockServiceTask, e);
			return mockServiceTask;
		}

		internal static Mock<IeHubServiceTaskSupport> CreateMockServiceTaskSupport(string serviceTaskName)
		{
			var mockConfig = new Mock<IeHubServiceTaskSupport>(MockBehavior.Strict);
			mockConfig.Setup(m => m.ServiceTaskName).Returns(serviceTaskName);
			mockConfig.Setup(m => m.CompanySettingsManager).Returns(CreateMockCompanySettingsManager().Object);
			return mockConfig;
		}

		internal static Mock<ICompanySettingsManager> CreateMockCompanySettingsManager(GlbCompany[] companies = null, ICompanySettings[] settings = null)
		{
			var actualCompanies = companies ?? new[] { GlbCompany.GetCurrentCompany(new BusinessObjectFactory()) };
			var actualSettings = settings ?? actualCompanies.Select(company => CreateMockCompanySettings()).ToArray();

			AssertEquals("Counts of companies and settings is the same", actualCompanies.Length, actualSettings.Length);

			var mockCompanySettingsManager = new Mock<ICompanySettingsManager>(MockBehavior.Strict);
			mockCompanySettingsManager.Setup(m => m.Companies).Returns(actualCompanies).Verifiable();
			mockCompanySettingsManager.Setup(x => x.GetSetting(It.IsAny<GlbCompany>()))
				.Returns((Func<GlbCompany, ICompanySettings>)(company =>
			{
				for (int i = 0; i < actualCompanies.Length; i++)
				{
					if (actualCompanies[i].PK == company.PK)
					{
						return actualSettings[i];
					}
				}
				return null;
			})).Verifiable();

			return mockCompanySettingsManager;
		}

		internal static ICompanySettings CreateMockCompanySettings(bool passwordExists = true)
		{
			var mockCompanySettings = new Mock<ICompanySettings>(MockBehavior.Strict);
			mockCompanySettings.Setup(m => m.PasswordExists).Returns(passwordExists);
			mockCompanySettings.Setup(m => m.GetPassword()).Returns(string.Empty);

			return mockCompanySettings.Object;
		}

		protected virtual Mock<TServiceTaskJob> StubMockJob(Mock<TServiceTask> serviceTask)
		{
			var job = new Mock<TServiceTaskJob>(serviceTask.Object, serviceTask.Object.Notifier) { CallBase = true };
			job.Setup(m => m.CanExecute).Returns(true);
			serviceTask.Setup(m => m.GetJobs()).Returns(new[] { job.Object });
			return job;
		}

		protected IeHubServiceTaskJob StubMockJobWithException(Mock<TServiceTask> serviceTask, Exception e)
		{
			var job = StubMockJob(serviceTask);
			job.Setup(m => m.ExecuteInternal()).Throws(e);
			job.Setup(m => m.NextExecuteIterationIsScheduled).Returns(false);
			return job.Object;
		}

		protected static void AssertNoErrorEmail()
		{
			TestHelpers.AssertNoErrorEmail();
		}

		protected virtual void AssertErrorEmail(string containsMessage)
		{
			TestHelpers.AssertErrorEmail(containsMessage);
		}

		protected virtual string EndpointNotFoundMessage
		{
			get { return LogMessages.EHubEndpointNotFoundMessage; }
		}
	}
}
