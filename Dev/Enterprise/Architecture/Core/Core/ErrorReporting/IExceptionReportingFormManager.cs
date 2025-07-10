using System;

namespace Enterprise.ZArchitecture.Core
{
	public interface IExceptionReportingFormManager
	{
		bool ShowReportForm(Exception ex, string errorReportId, string key, string message, bool isFullMode, Action<ExceptionReportArgs> sendErrorReport);
	}
}
