using System;

namespace Enterprise.Integration.DocumentEngine
{
	public interface IReportErrorHandler
	{
		(bool ShouldSkip, string DisplayMessage) ShouldSkipReportError(Exception exceptionMessage);
	}
}
