using System.Reflection;
using Enterprise.ServiceManager.Runner;
using Enterprise.ServiceManager.Runner.Exceptions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServiceManager.Logging.Abstractions;
using ServiceManager.Runner.Abstractions;

namespace CargoWise.ServiceManager.Runner.Test
{
	public class GrpcRunnerErrorReporterTest
	{
		[Test]
		public void TestRunnerLogger()
		{
			// Arrange

			// Act
			var result = grpcRunnerErrorReporter.CurrentLogger;

			// Assert
			Assert.That(result, Is.EqualTo(runnerLoggerMock.Object));
		}

		[Test]
		public void TestServiceTaskLogger()
		{
			// Arrange
			serviceTaskLoggerMock
				.SetupGet(logger => logger.TaskLoggerIsActive)
				.Returns(true);

			// Act
			var result = grpcRunnerErrorReporter.CurrentLogger;

			// Assert
			Assert.That(result, Is.EqualTo(serviceTaskLoggerMock.Object));
		}

		[Test]
		public void TestExceptionHandler()
		{
			// Arrange
			var fieldInfo = typeof(GrpcRunnerErrorReporter)?
				.BaseType?
				.GetField("exceptionHandler", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.IgnoreCase);

			// Act
			var result = fieldInfo?.GetValue(grpcRunnerErrorReporter);

			// Assert
			Assert.That(result, Is.TypeOf<RunnerExceptionHandler>());
		}

		[SetUp]
		public void SetUp()
		{
			runnerLoggerMock = new Mock<IRunnerLogger>();
			serviceTaskLoggerMock = new Mock<IServiceTaskLogger>();
			grpcRunnerErrorReporter = new GrpcRunnerErrorReporterForTest(runnerLoggerMock.Object, serviceTaskLoggerMock.Object);
		}

		GrpcRunnerErrorReporterForTest grpcRunnerErrorReporter = null!;

		Mock<IRunnerLogger> runnerLoggerMock = null!;
		Mock<IServiceTaskLogger> serviceTaskLoggerMock = null!;

		class GrpcRunnerErrorReporterForTest : GrpcRunnerErrorReporter
		{
			public GrpcRunnerErrorReporterForTest(IRunnerLogger runnerLogger, IServiceTaskLogger serviceTaskLogger)
				: base(runnerLogger, serviceTaskLogger)
			{
			}

			public new ILogger CurrentLogger => base.CurrentLogger;
		}
	}
}
