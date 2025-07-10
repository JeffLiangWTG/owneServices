using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveSystemDescriptor
	{
		/// <summary>
		/// A 3 letter code unique for this archive sub system.
		/// </summary>
		string Code { get; }

		/// <summary>
		/// Human readable name for this archive sub system.
		/// </summary>
		MultilingualString Name { get; }

		MultilingualString Noun { get; }

		MultilingualString PresentTenseVerb { get; }

		MultilingualString PastTenseVerb { get; }

		IReportGenerator ReportGenerator { get; }

		bool AllowShouldArchiveDeclaration { get; }

		bool AllowRecordsWithOrWithoutJobs { get; }

		bool AllowDateParameterSelection { get; }

		bool AllowUsingOnOrBeforeDateWhenWatermarkReset { get; }

		IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config);

		IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config);

		IEnumerable<string> GetRegistryLogs();
	}
}
