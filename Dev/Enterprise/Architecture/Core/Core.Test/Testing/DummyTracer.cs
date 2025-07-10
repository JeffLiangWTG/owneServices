using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core.Diagnostics;

namespace Enterprise.ZArchitecture.Core.Testing
{
	public class DummyTracer : ITracer
	{
		public List<string> Traces { get; private set; }

		public DummyTracer(params string[] enabledTraceSourceCodes)
		{
			Traces = new List<string>();
			EnabledTraceSourceCodes = enabledTraceSourceCodes ?? Array.Empty<string>(); // Empty means "everything is enabled"
		}

		public void TraceInformation(ZString traceSourceCode, Func<string> buildMessage)
		{
			Trace(traceSourceCode, buildMessage);
		}

		public string GetTraceFilter(ZString traceSourceName)
		{
			return "";
		}

		public bool CheckWhetherTraceSourceExists(params ZString[] traceSourceName)
		{
			return true;
		}

		public void RemoveAllTraceSources()
		{
		}

		public void RemoveTraceSource(params ZString[] traceSourceNames)
		{
		}

		public void TraceCriticalEvent(ZString traceSourceCode, Func<string> buildMessage)
		{
			Trace(traceSourceCode, buildMessage);
		}

		public void TraceErrorEvent(ZString traceSourceCode, Func<string> buildMessage)
		{
			Trace(traceSourceCode, buildMessage);
		}

		public void TraceResumptionOfActivity(ZString traceSourceCode, Func<string> buildMessage)
		{
			Trace(traceSourceCode, buildMessage);
		}

		public void TraceStartingOfActivity(ZString traceSourceCode, Func<string> buildMessage)
		{
			Trace(traceSourceCode, buildMessage);
		}

		public void TraceStoppingOfActivity(ZString traceSourceCode, Func<string> buildMessage)
		{
			Trace(traceSourceCode, buildMessage);
		}

		public void TraceSuspensionOfActivity(ZString traceSourceCode, Func<string> buildMessage)
		{
			Trace(traceSourceCode, buildMessage);
		}

		public void TraceTransferEvent(ZString traceSourceCode, Func<string> buildMessage)
		{
			Trace(traceSourceCode, buildMessage);
		}

		public void TraceVerbose(ZString traceSourceCode, Func<string> buildMessage)
		{
			Trace(traceSourceCode, buildMessage);
		}

		public void TraceWarningEvent(ZString traceSourceCode, Func<string> buildMessage)
		{
			Trace(traceSourceCode, buildMessage);
		}

		public bool IsEnabled(ZString traceSourceCode)
			=> !EnabledTraceSourceCodes.Any()
			|| EnabledTraceSourceCodes.Any(code => code == traceSourceCode);

		void Trace(ZString traceSourceCode, Func<string> buildMessage)
		{
			if (IsEnabled(traceSourceCode))
			{
				Traces.Add(buildMessage.Invoke());
			}
		}

		readonly string[] EnabledTraceSourceCodes;
	}
}
