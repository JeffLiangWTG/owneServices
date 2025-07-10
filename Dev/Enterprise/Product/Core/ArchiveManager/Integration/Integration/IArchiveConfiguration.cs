using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveConfiguration
	{
		bool IsFilteringByJobOpenDate { get; }

		ZDateTime ArchiveJobsOnOrBeforeThisDate { get; }

		ZInt MaxRunDurationInMinutes { get; }

		ZDateTime StartRunTime { get; }

		ZDateTime StopRunTime { get; }

		bool IsVerboseLog { get; }

		bool ShouldIncludeDeclarations { get; }

		bool ShouldIncludeRecordsWithoutJobs { get; }

		bool UseOnOrBeforeDateWhenWatermarkReset { get; }

		IEnumerable<string> GetConfigurationLogs(IArchiveSystem system);

		Dictionary<string, object> AMUsageReportValues { get; }

		void SetIsFilteringByJobOpenDate(bool isFilteringByJobOpenDate);
	}
}
