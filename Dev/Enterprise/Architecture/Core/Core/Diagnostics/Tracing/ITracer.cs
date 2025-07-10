using System;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.Core.Diagnostics
{
	public interface ITracer
	{
		bool IsEnabled(ZString traceSourceCode);

		void TraceCriticalEvent(ZString traceSourceCode, Func<string> buildMessage);

		void TraceErrorEvent(ZString traceSourceCode, Func<string> buildMessage);

		void TraceWarningEvent(ZString traceSourceCode, Func<string> buildMessage);

		void TraceInformation(ZString traceSourceCode, Func<string> buildMessage);

		void TraceVerbose(ZString traceSourceCode, Func<string> buildMessage);

		void TraceStartingOfActivity(ZString traceSourceCode, Func<string> buildMessage);

		void TraceStoppingOfActivity(ZString traceSourceCode, Func<string> buildMessage);

		void TraceSuspensionOfActivity(ZString traceSourceCode, Func<string> buildMessage);

		void TraceResumptionOfActivity(ZString traceSourceCode, Func<string> buildMessage);

		void TraceTransferEvent(ZString traceSourceCode, Func<string> buildMessage);

		string GetTraceFilter(ZString traceSourceName);
	}
}
