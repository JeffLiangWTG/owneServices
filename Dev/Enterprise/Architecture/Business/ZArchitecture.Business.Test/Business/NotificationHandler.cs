using System;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class NotificationHandler : INotificationHandler
	{
		public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
		{
			LastErrorMessageReported = message;
			LastErrorCaptionReported = caption;
			LastErrorContextReported = errorContext;
			LastErrorExceptionReported = exception;
		}
		public string LastErrorMessageReported;
		public string LastErrorCaptionReported;
		public string LastErrorContextReported;
		public Exception LastErrorExceptionReported;

		public void ReportInformation(string message, string caption)
		{
			LastInformationMessageReported = message;
			LastInformationCaptionReported = caption;
		}
		public string LastInformationMessageReported;
		public string LastInformationCaptionReported;
	}
}
