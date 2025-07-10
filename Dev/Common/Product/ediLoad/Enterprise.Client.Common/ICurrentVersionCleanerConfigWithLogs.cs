using System;
using Enterprise.Upgrades;

namespace Enterprise.Client.Common
{
	public interface ICurrentVersionCleanerConfigWithLogs : ICurrentVersionCleanerConfig
	{
		DateTime LastStartTime { get; set; }
		DateTime LastSuccessTime { get; set; }
		DateTime NextRuntime { get; set; }
	}
}
