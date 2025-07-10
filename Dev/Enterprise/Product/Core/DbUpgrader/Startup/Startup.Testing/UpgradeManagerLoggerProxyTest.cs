using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Startup.Testing
{
	public class UpgradeManagerLoggerProxyTest : TestCase
	{
		[ExpectNoExceptions]
		public void TestLogReportErrorWhenErrorIsNotHandled()
		{
			CombineAssertions(() =>
			{
				TestLogReportErrorWhenErrorIsNotHandled(new InvalidOperationException("Not important message"));
				TestLogReportErrorWhenErrorIsNotHandled(new FormatException("Not important message"));
				TestLogReportErrorWhenErrorIsNotHandled(new Exception(string.Empty));
			});

			void TestLogReportErrorWhenErrorIsNotHandled(Exception exception)
			{
				// Arrange
				var errorReporterMock = new Mock<IErrorReporter>();

				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				{
					var errorHandlerMock = new Func<Exception, IUpgradeTaskWorkflowLogger, bool>((e, logger) => false);
					var logger = new UpgradeManagerLoggerProxy(Mock.Of<IUpgradeTaskWorkflowLogger>(), errorHandlerMock);

					// Act
					logger.Log(Integration.LogType.Error, "Not important", exception);

					// Assert
					errorReporterMock.Verify(x => x.Report(It.IsAny<string>(), It.IsAny<string>(), It.Is<UpgradeException>(y => y.InnerException == exception)), Times.Once);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestLogDoNotErrorReportWhenErrorIsHandled()
		{
			CombineAssertions(() =>
			{
				TestLogDoNotErrorReportWhenErrorIsHandled(new InvalidOperationException("Not important message"));
				TestLogDoNotErrorReportWhenErrorIsHandled(new FormatException("Not important message"));
				TestLogDoNotErrorReportWhenErrorIsHandled(new Exception(string.Empty));
			});

			void TestLogDoNotErrorReportWhenErrorIsHandled(Exception exception)
			{
				// Arrange
				var errorReporterMock = new Mock<IErrorReporter>();

				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				{
					var errorHandlerMock = new Func<Exception, IUpgradeTaskWorkflowLogger, bool>((e, logger) => true);
					var logger = new UpgradeManagerLoggerProxy(Mock.Of<IUpgradeTaskWorkflowLogger>(), errorHandlerMock);

					// Act
					logger.Log(Integration.LogType.Error, "Not important", exception);

					// Assert
					errorReporterMock.VerifyNoOtherCalls();
				}
			}
		}
	}
}
