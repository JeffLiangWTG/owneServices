using System;
using CargoWise.Common;
using Moq;
using NUnit.Framework;
using ServiceManager.Common.CW;

namespace ServiceManager.Shared.CW.Test
{
	class ErrorReporterProxyTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			errorReporterMock = new Mock<IErrorReporter>();
			errorReporterProxy = new ErrorReporterProxy();
		}
		public void TestReportOnceWithMessageAndException()
		{
			// Arrange
			var msg = "message";
			var ex = new Exception();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				// Act
				errorReporterProxy.ReportOnce(msg, ex);
			}

			// Assert
			AssertNoExceptionThrown(() =>
			{
				errorReporterMock.Verify(x => x.Report(It.IsAny<string>(), msg, ex), Times.Once);
				errorReporterMock.VerifyNoOtherCalls();
			});
		}

		public void TestReportOnceWithKeyAndMessage()
		{
			// Arrange
			var key = "key";
			var msg = "message";

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				// Act
				errorReporterProxy.ReportOnce(key, msg);
			}

			// Assert
			AssertNoExceptionThrown(() =>
			{
				errorReporterMock.Verify(x => x.Report(key, msg, It.IsAny<Exception>()), Times.Once);
				errorReporterMock.VerifyNoOtherCalls();
			});
		}

		public void TestReportDeveloperExceptionOnceWithMessageAndException()
		{
			// Arrange
			var msg = "message";
			var ex = new Exception();

			using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
			{
				// Act
				errorReporterProxy.ReportDeveloperExceptionOnce(msg, ex);
			}

			// Assert
			AssertNoExceptionThrown(() =>
			{
				errorReporterMock.Verify(x => x.ReportDeveloperExceptionOrHandleSilently(It.IsAny<string>(), msg, ex), Times.Once);
				errorReporterMock.VerifyNoOtherCalls();
			});
		}

		Mock<IErrorReporter> errorReporterMock;
		ErrorReporterProxy errorReporterProxy;
	}
}
