using System;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Web.Common;

namespace Enterprise.ZArchitecture.Web.Utilities.Exceptions
{
	/// <summary>
	/// Exceptions reporter for web applications.
	/// </summary>
	public class WebExceptionReporter : BaseWebExceptionReporter, IErrorReporter
	{
		public WebExceptionReporter()
			: this(new TopLevelWebExceptionHandler())
		{
		}

		public WebExceptionReporter(TopLevelExceptionHandler topLevelExceptionHandler)
			: base(topLevelExceptionHandler)
		{
		}

		protected override void ShowReportForm(Exception ex, string errorReportId, string key, string message, bool isFullMode)
		{
			ExceptionReportArgs aSPExceptionReportArgs = new ExceptionReportArgs(ex, errorReportId, key, message);
			SendReport(this, aSPExceptionReportArgs);
		}

		protected override void DoReportException(Exception ex, string key, string message)
		{
			ExceptionReportArgs aSPExceptionReportArgs = new ExceptionReportArgs(ex, GenerateErrorReportID(), key, message);

			var builder = GetNewExceptionReportBuilder(aSPExceptionReportArgs);
			var report = builder.GenerateReport();

			SendErrorReport(report);
		}

		public ExceptionMessage ReportWebException(Exception exception, string key, string message)
		{
			DoReportException(exception, key, message);
			return new ExceptionMessage(exception, key, message);
		}

		protected override ExceptionReportBuilder GetNewExceptionReportBuilder(ExceptionReportArgs reportArgs)
		{
			return new WebExceptionReportBuilder(reportArgs);
		}

#if DEBUG
		internal ExceptionReportBuilder GetNewExceptionReportBuilderForTesting(ExceptionReportArgs reportArgs) => GetNewExceptionReportBuilder(reportArgs);
#endif
	}

	#region Exception Message

	public class ExceptionMessage
	{
		public ExceptionMessage(Exception exception, string key, string message)
		{
			this.Exception = exception;
			this.Key = key;
			this.Message = message;
		}

		public string Message { get; set; }
		public string Key { get; set; }
		public Exception Exception { get; set; }
	}

	#endregion WebExceptionMessage
}
