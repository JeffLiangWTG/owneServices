using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.Client.EDI.ApplicationLogging;
using Enterprise.Client.EDI.ApplicationLogging.Business;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace ZClientEDI.Test.ApplicationLogging
{
	[TestedType(typeof(ApplicationLoggingActiveLoggerDisablerServiceTask))]
	class ApplicationLoggingActiveLoggerDisablerServiceTaskTest : ServiceTaskTestCase<ApplicationLoggingActiveLoggerDisablerServiceTask>
	{
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		public void TestHostedServiceAttributes()
		{
			var serviceAttribute = GetHostedServiceAttributes().Single();
			AssertEquals("CSP", serviceAttribute.Category);
			AssertEquals("ALD", serviceAttribute.Code);
			AssertEquals("Application Logging Active Logger Disabler", serviceAttribute.Description);
			AssertEquals("1hour", serviceAttribute.DefaultScheduleRunEvery);
			AssertEquals("4hours", serviceAttribute.MaximumPeriod);
			AssertEquals("15minutes", serviceAttribute.MinimumPeriod);
			AssertEquals(true, serviceAttribute.IsMandatory);
			AssertEquals(true, serviceAttribute.ActiveByDefault);
		}

		public void TestExpiredLoggersGetDisabled()
		{
			// Arrange
			var activeLogger = GetApplicationActiveLogger();
			Factory.Save();

			// Act
			serviceTask.RunTask(CancellationToken.None);

			// Assert
			activeLogger = Factory.Load<ApplicationActiveLogger>(activeLogger.PK);
			loggerMock.Verify(x => x.Log(LogType.Information, It.IsAny<string>()), Times.Once);
			AssertEquals(ZDateTimeOffset.Empty, activeLogger.AAL_ActiveUntil);
		}

		[TestDate(2025, 1, 1)]
		public void TestActiveLoggersDoNotGetDisabled()
		{
			// Arrange
			var activeUntil = ZDateTimeOffset.Now.AddDays(10);
			var activeLogger = GetApplicationActiveLogger(activeUntil);
			Factory.Save();

			// Act
			serviceTask.RunTask(CancellationToken.None);

			// Assert
			activeLogger = Factory.Load<ApplicationActiveLogger>(activeLogger.PK);
			loggerMock.Verify(x => x.Log(LogType.Information, It.IsAny<string>()), Times.Never);
			AssertEquals(activeUntil, activeLogger.AAL_ActiveUntil);
		}

		[ExpectNoExceptions]
		public void TestCorrectLogsAreWritten()
		{
			// Arrange
			var activeUntil = new ZDateTimeOffset(new DateTime(2025, 1, 1), DateTimeKind.Utc);
			var activeLogger = GetApplicationActiveLogger(activeUntil);
			activeLogger.ApplicationLogger.ALG_Name = "TestLogger";
			Factory.Save();

			// Act
			serviceTask.RunTask(CancellationToken.None);

			// Assert
			loggerMock.Verify(x => x.Log(LogType.Information, $"The {activeLogger.ApplicationLogger.ALG_Name} Active Logger expired on {activeUntil} and has been deactivated."));
		}

		[ExpectNoExceptions]
		public void TestDisabledActiveLoggersAreNotDisabledAgain()
		{
			// Arrange
			var expiredActiveLogger = GetApplicationActiveLogger(ZDateTimeOffset.Empty);
			Factory.Save();

			// Act
			serviceTask.RunTask(CancellationToken.None);

			// Assert
			AssertEquals(ZDateTimeOffset.Empty, Factory.Load<ApplicationActiveLogger>(expiredActiveLogger.PK).AAL_ActiveUntil);
			loggerMock.Verify(x => x.Log(LogType.Information, It.IsAny<string>()), Times.Never);
		}

		public void TestServiceTaskRespondsToCancellationToken()
		{
			// Arrange
			GetApplicationActiveLogger();
			Factory.Save();

			var cancellationTokenSource = new CancellationTokenSource();
			cancellationTokenSource.Cancel();

			// Act & Assert
			AssertExceptionThrown<OperationCanceledException>(() => serviceTask.RunTask(cancellationTokenSource.Token));
		}

		ApplicationActiveLogger GetApplicationActiveLogger(ZDateTimeOffset? activeUntil = null)
		{
			var logger = Factory.New<ApplicationLogger>();
			logger.ALG_Name = Guid.NewGuid().ToString();
			logger.ALG_Product = "CargoWise";

			var activeLogger = Factory.New<ApplicationActiveLogger>();
			activeLogger.AAL_ActiveUntil = activeUntil ?? new ZDateTimeOffset(2025, 1, 1);
			activeLogger.AAL_Environment = "TestEnv";
			activeLogger.AAL_ALG_ApplicationLogger = logger.PK;
			return activeLogger;
		}

		protected override void SetUpCore()
		{
			base.SetUpCore();
			serviceTask = new ApplicationLoggingActiveLoggerDisablerServiceTask();
			loggerMock = new Mock<ILogger>();
			serviceTask.ServiceLogger = loggerMock.Object;
		}

		Mock<ILogger> loggerMock;
		ApplicationLoggingActiveLoggerDisablerServiceTask serviceTask;
	}
}
