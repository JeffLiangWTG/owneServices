using System.Collections.Generic;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

[assembly: ArchiveSystemDescriptorProvider(typeof(PARPurgeSystemDescriptor))]

namespace Enterprise.ArchiveManager.Business
{
	public sealed class PARPurgeSystemDescriptor : IArchiveSystemDescriptor
	{
		public string Code
			=> ArchiveManagerConstants.Codes.PAR;

		public MultilingualString Name
			=> ResString.GetMultilingualString("79F00255-8D1C-4A6F-B5E8-FD3B464A409D", ArchiveManagerConstants.Names.PAR);

		public MultilingualString Noun
			=> SystemDescriptorStrings.GetNoun(SystemDescriptorType.Purge);

		public MultilingualString PresentTenseVerb
			=> SystemDescriptorStrings.GetPresentTenseVerb(SystemDescriptorType.Purge);

		public MultilingualString PastTenseVerb
			=> SystemDescriptorStrings.GetPastTenseVerb(SystemDescriptorType.Purge);

		public bool AllowShouldArchiveDeclaration
			=> false;

		public bool AllowRecordsWithOrWithoutJobs
			=> false;

		public bool AllowDateParameterSelection
			=> false;

		public bool AllowUsingOnOrBeforeDateWhenWatermarkReset
			=> false;

		public IReportGenerator ReportGenerator
			=> new PurgeReportGenerator();

		public IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new PARDeleteStageDescriptor();
		}

		public IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config)
		{
			foreach (var stageDescriptor in GetArchiveStageDescriptors(config))
			{
				yield return new PARArchiveStage(stageDescriptor, this);
			}
		}

		public IEnumerable<string> GetRegistryLogs()
		{
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.PurgeArchivedRecordsOnOrBeforeMinimum.Name);
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.PurgeArchivedRecordsBatchSizeControl.Name);
		}
	}
}
