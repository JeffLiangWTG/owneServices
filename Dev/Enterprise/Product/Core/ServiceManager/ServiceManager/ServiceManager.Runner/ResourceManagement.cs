using System;
using System.Diagnostics.CodeAnalysis;
using CargoWise.Common;
using Microsoft.Extensions.Logging;
using ServiceManager.Runner.Abstractions;

namespace Enterprise.ServiceManager.Runner
{
	class ResourceManagement : IResourceManagement
	{
		public ResourceManagement(IRunnerLogger runnerLogger)
		{
			this.runnerLogger = runnerLogger ?? throw new ArgumentNullException(nameof(runnerLogger));
		}

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", Justification = "Required for process memory size management")]
		public void ReclaimMemory(ref IServiceTaskHandler serviceTaskHandler)
		{
			var code = serviceTaskHandler.HostedServiceAttribute.Code;
			var reclaimResult = GCWrapper.ReclaimMemory(ref serviceTaskHandler);
			if (reclaimResult != null)
			{
				var bytesReclaimed = reclaimResult.MemoryBeforeCollect - reclaimResult.MemoryAfterCollect;
				if (bytesReclaimed > 1000000 && reclaimResult.GenerationOfObject > 0)
				{
					runnerLogger.Log(LogLevel.Debug, FormattableString.Invariant($"Service Task [{code}], Memory reclaimed from Gen : {reclaimResult.GenerationOfObject}. {reclaimResult.MemoryBeforeCollect} - {bytesReclaimed} = {reclaimResult.MemoryAfterCollect} bytes left."));
				}
			}
		}

		readonly IRunnerLogger runnerLogger;
	}
}
