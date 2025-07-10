using System;
using CargoWise.Application;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.Accounting.Business.JobInvoicing.AutoJobClosureServiceTask.Diagnostic
{
	public interface IDiagnosticLogger : ILogger
	{
	}

	internal class JCSDiagnosisLogger : IDiagnosticLogger
	{
		internal JCSDiagnosisLogger()
		{
			Tracer = ObjectFactory.Get<ITracer>();
		}
		readonly ITracer Tracer;

		void ILogger.Log(LogType type, string message) =>
			Tracer.TraceInformation(AccountingTraceSourceCodes.JCS, () => FormattableString.Invariant($"{type}: {message}"));

		void ILogger.Log(LogType type, string message, Exception ex) =>
			Tracer.TraceErrorEvent(AccountingTraceSourceCodes.JCS, () => FormattableString.Invariant($"{type}: {message}\r\nException Type:{ex.GetType()}\r\nMessage:{ex.Message}"));
	}
}
