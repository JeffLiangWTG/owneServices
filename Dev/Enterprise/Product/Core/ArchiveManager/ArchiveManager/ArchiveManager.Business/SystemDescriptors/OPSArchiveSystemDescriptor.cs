using System.Collections.Generic;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

[assembly: ArchiveSystemDescriptorProvider(typeof(OPSArchiveSystemDescriptor))]

namespace Enterprise.ArchiveManager.Business
{
	sealed public class OPSArchiveSystemDescriptor : IArchiveSystemDescriptor
	{
		public string Code
			=> ArchiveManagerConstants.Codes.OPS;

		public MultilingualString Name
			=> ResString.GetMultilingualString("bde510b0-ce42-4fab-ae94-75c8003b6220", ArchiveManagerConstants.Names.OPS);

		public MultilingualString Noun
			=> SystemDescriptorStrings.GetNoun(SystemDescriptorType.Archive);

		public MultilingualString PresentTenseVerb
			=> SystemDescriptorStrings.GetPresentTenseVerb(SystemDescriptorType.Archive);

		public MultilingualString PastTenseVerb
			=> SystemDescriptorStrings.GetPastTenseVerb(SystemDescriptorType.Archive);

		public bool AllowShouldArchiveDeclaration
			=> true;

		public bool AllowRecordsWithOrWithoutJobs
			=> false;

		public bool AllowDateParameterSelection
			=> true;

		public bool AllowUsingOnOrBeforeDateWhenWatermarkReset
			=> false;

		public IReportGenerator ReportGenerator
			=> new ArchiveReportGenerator();

		public IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new OPSArchiveStageDescriptor();
		}

		public IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config)
		{
			foreach (var stageDescriptor in GetArchiveStageDescriptors(config))
			{
				yield return new OPSArchiveStage(stageDescriptor, this);
			}
		}

		public IEnumerable<string> GetRegistryLogs()
		{
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.ArchiveRecordsOnOrBeforeMinimum.Name);
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.BatchSizeControl.Name);
		}
	}
}
