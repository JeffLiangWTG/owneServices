using CargoWise.Types;

namespace Enterprise.ZArchitecture.Core.Diagnostics
{
	public interface ITracerConfiguration
	{
		void InitializeTraceSources(TraceSourceConfiguration traceSourceConfiguration);

		void RemoveAllTraceSources();

		void RemoveTraceSource(params ZString[] traceSourceNames);

		bool CheckWhetherTraceSourceExists(params ZString[] traceSourceName);
	}
}
