using System;
using WTG.ErrorReporting;

namespace Enterprise.ZArchitecture.Core
{
	public interface IErrorReportingClientProvider
	{
		IErrorReportingClient CreateClient(Uri uri, TimeSpan? timeout = null);
	}

	public class ErrorReportingClientProvider : IErrorReportingClientProvider
	{
		public IErrorReportingClient CreateClient(Uri uri, TimeSpan? timeout = null)
		{
			var errorReportingClient = new ErrorReportingClient(uri);
			if (timeout.HasValue)
			{
				errorReportingClient.Timeout = timeout.Value;
			}
			return errorReportingClient;
		}
	}
}
