using System.Collections.Generic;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

[assembly: ArchiveSystemDescriptorProvider(typeof(IPSArchiveSystemDescriptor))]

namespace Enterprise.ArchiveManager.Business
{
	public sealed class IPSArchiveSystemDescriptor : IArchiveSystemDescriptor
	{
		public string Code
			=> ArchiveManagerConstants.Codes.IPS;

		public MultilingualString Name
			=> ResString.GetMultilingualString("4F20AFF8-5B9C-43F9-9F3A-E72B625264BA", ArchiveManagerConstants.Names.IPS);

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
			=> false;

		public bool AllowUsingOnOrBeforeDateWhenWatermarkReset
			=> false;

		public IReportGenerator ReportGenerator
			=> new ArchiveReportGenerator();

		public IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new IPSInactiveShipmentArchiveStageDescriptor();
			yield return new IPSInactiveConsolArchiveStageDescriptor();
			yield return new IPSInactiveRatingHeaderArchiveStageDescriptor();
			yield return new IPSInactiveJobCartageArchiveStageDescriptor();

			if (config.ShouldIncludeDeclarations)
			{
				yield return new IPSInactiveJobDeclarationArchiveStageDescriptor();
			}
		}

		public IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config)
		{
			foreach (var stageDescriptor in GetArchiveStageDescriptors(config))
			{
				yield return new IPSArchiveStage(stageDescriptor, this);
			}
		}

		public IEnumerable<string> GetRegistryLogs()
		{
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.InactiveOperationalJobsArchiveSystemOnOrBeforeMinimum.Name);
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.BatchSizeControl.Name);
		}
	}
}
