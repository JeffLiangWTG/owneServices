using System.Collections.Generic;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

[assembly: ArchiveSystemDescriptorProvider(typeof(PDRPurgeSystemDescriptor))]

namespace Enterprise.ArchiveManager.Business
{
	sealed public class PDRPurgeSystemDescriptor : IArchiveSystemDescriptor
	{
		public string Code
			=> ArchiveManagerConstants.Codes.PDR;

		public MultilingualString Name
			=> ResString.GetMultilingualString("6D0E416C-C6B8-4AF3-B821-4B853DF7D36D", ArchiveManagerConstants.Names.PDR);

		public MultilingualString Noun
			=> SystemDescriptorStrings.GetNoun(SystemDescriptorType.Purge);

		public MultilingualString PresentTenseVerb
			=> SystemDescriptorStrings.GetPresentTenseVerb(SystemDescriptorType.Purge);

		public MultilingualString PastTenseVerb
			=> SystemDescriptorStrings.GetPastTenseVerb(SystemDescriptorType.Purge);

		public bool AllowShouldArchiveDeclaration
			=> true;

		public bool AllowRecordsWithOrWithoutJobs
			=> false;

		public bool AllowDateParameterSelection
			=> true;

		public bool AllowUsingOnOrBeforeDateWhenWatermarkReset
			=> false;

		public IReportGenerator ReportGenerator
			=> new PurgeReportGenerator();

		public IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new PDRArchiveStageDescriptor();
		}

		public IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config)
		{
			foreach (var stageDescriptor in GetArchiveStageDescriptors(config))
			{
				yield return new PDRPurgeStage(stageDescriptor, this);
			}
		}

		public IEnumerable<string> GetRegistryLogs()
		{
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.PurgeDocumentsAndRecordsOnOrBeforeMinimum.Name);
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.BatchSizeControl.Name);
		}
	}
}
