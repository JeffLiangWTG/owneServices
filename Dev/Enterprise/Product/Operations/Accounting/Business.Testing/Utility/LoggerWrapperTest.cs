using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class LoggerWrapperTest : TestCase
	{
		public void TestReportAndLogError()
		{
			TestReportAndLogErrorCore("Test error report key");
		}

		public void TestReportAndLogErrorWithoutErrorReportKey()
		{
			TestReportAndLogErrorCore();
		}

		void TestReportAndLogErrorCore(string errorReportKey = null)
		{
			var mockLogger = new Mock<IXmlImportLogger>();
			mockLogger.Setup(l => l.LogBoth(It.IsAny<LogType>(), It.IsAny<string>())).Verifiable();
			var logType = LogType.Error;
			var errorMessage = "Test error message";

			LoggerWrapper.ReportAndLogError(mockLogger.Object, logType, errorMessage, errorReportKey);

			AssertEquals(errorMessage, ErrorReporter.LastMessageReported);
			AssertEquals(errorReportKey, ErrorReporter.LastKeyReported);
			mockLogger.Verify(x => x.LogBoth(It.IsAny<LogType>(), It.IsAny<string>()), Times.Once());
			ErrorReporter.Instance.Clear();
		}
	}
}
