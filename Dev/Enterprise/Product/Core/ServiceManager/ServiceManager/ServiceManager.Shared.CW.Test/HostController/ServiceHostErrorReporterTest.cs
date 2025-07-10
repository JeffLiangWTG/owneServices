using System;
using System.Net;
using CargoWise.Common;
using Moq;
using NUnit.Framework;
using ServiceManager.Shared.CW;

namespace Enterprise.ServiceManager.HostClient.Testing
{
	sealed class ServiceHostErrorReporterTest : TestCase
	{
		public void TestReportException()
		{
			Test("key1", "web exception timeout", new WebException("Problem", WebExceptionStatus.Timeout));
			Test("key2", "web exception unknown error", new WebException("Problem", WebExceptionStatus.UnknownError));
			Test("key3", "serialization exception", new System.Runtime.Serialization.SerializationException());
			Test("key4", "argument exception", new ArgumentException());

			void Test(string key, string message, Exception exception)
			{
				// Arrange
				var errorReporterMock = new Mock<IErrorReporter>();
				var serviceHostErrorReporter = new ServiceHostErrorReporter();

				// Act
				using (ErrorReporter.SetTemporaryInstanceForTest(errorReporterMock.Object))
				{
					serviceHostErrorReporter.ReportException(key, message, exception);
					serviceHostErrorReporter.ReportException(key, message, exception);
				}

				// Assert
				CombineAssertions(() =>
				{
					AssertEquals(1, ErrorReporter.TotalErrorCount);
					errorReporterMock.Verify(reporter => reporter.Report(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
					errorReporterMock.Verify(reporter => reporter.Report(key, message, exception), Times.Once);
				});
				ErrorReporter.Clear();
			}
		}
	}
}
