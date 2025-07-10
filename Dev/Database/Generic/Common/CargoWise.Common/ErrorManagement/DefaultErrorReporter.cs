using System;

namespace CargoWise.Common
{
	[WTG.StaticAnalysis.Annotation.Immutable]
	public class DefaultErrorReporter : IErrorReporter
	{
		public void Report(string key, string message, Exception exception)
		{
			GetExceptionReporter().Report(key, message, exception);
		}

		public void ReportDeveloperExceptionOrHandleSilently(string key, string message, Exception ex)
		{
			GetExceptionReporter().ReportDeveloperExceptionOrHandleSilently(key, message, ex);
		}

		IErrorReporter GetExceptionReporter()
		{
			var coreAssembly = AssemblyLoader.LoadAssembly("Enterprise.ZArchitecture.Core");
			var exceptionReporterType = coreAssembly.GetType("Enterprise.ZArchitecture.Core.ExceptionReporter");
			return (IErrorReporter)exceptionReporterType.GetProperty("Instance").GetValue(null);
		}

		void IErrorReporter.Clear()
		{
		}
	}
}
