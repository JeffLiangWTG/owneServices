namespace Enterprise.ZArchitecture.Core.Testing
{
	public class DummyExceptionBuilder : ExceptionBuilder
	{
		public DummyExceptionBuilder(ExceptionReportArgs reportArgs, string errorTime)
				: base(reportArgs, errorTime)
		{
		}
	}
}
