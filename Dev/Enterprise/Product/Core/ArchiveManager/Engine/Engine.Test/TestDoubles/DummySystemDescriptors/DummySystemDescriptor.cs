using System.Collections.Generic;
using System.Linq;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ArchiveManager.Engine.Test.TestDoubles.DummySystemDescriptors
{
	public abstract class DummySystemDescriptor : ISelfContainedArchiveSystemDescriptor
	{
		public abstract string Code { get; }

		public abstract MultilingualString Name { get; }

		public virtual MultilingualString Noun
			=> SystemDescriptorStrings.GetNoun(SystemDescriptorType.Archive);

		public virtual MultilingualString PresentTenseVerb
			=> SystemDescriptorStrings.GetPresentTenseVerb(SystemDescriptorType.Archive);

		public virtual MultilingualString PastTenseVerb
			=> SystemDescriptorStrings.GetPastTenseVerb(SystemDescriptorType.Archive);

		public virtual IReportGenerator ReportGenerator
			=> null;

		public bool AllowShouldArchiveDeclaration
			=> true;

		public bool AllowRecordsWithOrWithoutJobs
			=> false;

		public virtual bool AllowDateParameterSelection
			=> false;

		public bool AllowUsingOnOrBeforeDateWhenWatermarkReset
			=> false;

		public int OnOrBeforeMinimumValue
			=> ArchiveManagerConstants.MinimumDataRetentionRequirementYears.Default;

		public int BatchSizeControlValue
			=> 50;

		public abstract IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config);

		public IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config)
		{
			foreach (var stageDescriptor in GetArchiveStageDescriptors(config))
			{
				yield return new ArchiveStage(stageDescriptor, this);
			}
		}

		public virtual IEnumerable<string> GetRegistryLogs()
			=> Enumerable.Empty<string>();
	}
}
