using CargoWise.Common;
using Enterprise.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Accounting.Business
{
	public static class LoggerWrapper
	{
		public static void ReportAndLogError(IXmlImportLogger logger, LogType logType, string errorMessage, string errorReportKey = null)
		{
			ErrorReporter.ReportOnce(errorReportKey, errorMessage);
			logger.LogBoth(logType, errorMessage);
		}
	}
}
