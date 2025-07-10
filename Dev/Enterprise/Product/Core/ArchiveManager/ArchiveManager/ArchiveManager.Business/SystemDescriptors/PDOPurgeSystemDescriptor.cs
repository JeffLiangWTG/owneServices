using System.Collections.Generic;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Business.StageDescriptors.PDO;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

[assembly: ArchiveSystemDescriptorProvider(typeof(PDOPurgeSystemDescriptor))]

namespace Enterprise.ArchiveManager.Business
{
	public sealed class PDOPurgeSystemDescriptor : IArchiveSystemDescriptor
	{
		public string Code
			=> ArchiveManagerConstants.Codes.PDO;

		public MultilingualString Name
			=> ResString.GetMultilingualString("AC15F88E-D494-4F50-A985-98AD05E50239", ArchiveManagerConstants.Names.PDO);

		public MultilingualString Noun
			=> SystemDescriptorStrings.GetNoun(SystemDescriptorType.Purge);

		public MultilingualString PresentTenseVerb
			=> SystemDescriptorStrings.GetPresentTenseVerb(SystemDescriptorType.Purge);

		public MultilingualString PastTenseVerb
			=> SystemDescriptorStrings.GetPastTenseVerb(SystemDescriptorType.Purge);

		public bool AllowShouldArchiveDeclaration
			=> true;

		public bool AllowRecordsWithOrWithoutJobs
			=> true;

		public bool AllowDateParameterSelection
			=> false;

		public bool AllowUsingOnOrBeforeDateWhenWatermarkReset
			=> true;

		public IReportGenerator ReportGenerator
			=> new PurgeReportGenerator();

		public IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			if (config.ShouldIncludeRecordsWithoutJobs)
			{
				yield return new PDOWithoutJobsJobCartageDescriptor();
				yield return new PDOWithoutJobsJobConsolDescriptor();
				yield return new PDOWithoutJobsJobShipmentDescriptor();
				yield return new PDOWithoutJobsRatingHeaderDescriptor();

				if (config.ShouldIncludeDeclarations)
				{
					yield return new PDOWithoutJobsJobDeclarationDescriptor();
				}
			}
			else
			{
				yield return new PDOPurgeStageDescriptor();
			}
		}

		public IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config)
		{
			foreach (var stageDescriptor in GetArchiveStageDescriptors(config))
			{
				yield return new PDOPurgeStage(stageDescriptor, this);
			}
		}

		public IEnumerable<string> GetRegistryLogs()
		{
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemOnOrBeforeMinimum.Name);
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.PurgeDocumentsOfOperationalRecordsSystemBatchSizeControl.Name);
		}
	}
}
