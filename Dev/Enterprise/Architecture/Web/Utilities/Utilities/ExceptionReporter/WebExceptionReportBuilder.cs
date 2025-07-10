using System;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.Utilities.Exceptions
{
	public class WebExceptionReportBuilder : ExceptionReportBuilder
	{
		public WebExceptionReportBuilder(ExceptionReportArgs reportArgs)
			: base(reportArgs)
		{
		}

		protected override ExceptionDetails GetExceptionDetails(Exception ex)
		{
			return new WebExceptionDetails(ex);
		}

#if DEBUG
		internal ExceptionDetails GetExceptionDetailsForTesting(Exception ex) => GetExceptionDetails(ex);
#endif
	}
}
