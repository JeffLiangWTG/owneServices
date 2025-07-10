using System.Collections.Generic;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

[assembly: ArchiveSystemDescriptorProvider(typeof(STAArchiveSystemDescriptor))]

namespace Enterprise.ArchiveManager.Business
{
	sealed class STAArchiveSystemDescriptor : IArchiveSystemDescriptor
	{
		public string Code
			=> ArchiveManagerConstants.Codes.STA;

		public MultilingualString Name
			=> ResString.GetMultilingualString("6A1FAE14-5F4D-4B7C-825D-1BB6B78377D8", ArchiveManagerConstants.Names.STA);

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
			if (config.ShouldIncludeDeclarations)
			{
				yield return new STACusCAeMHMasterArchiveStageDescriptor();
				yield return new STACusCAeMHHouseArchiveStageDescriptor();
				yield return new STACusTempStorageJobHeaderArchiveStageDescriptor();
				yield return new STACusTempStorageRegHeaderArchiveStageDescriptor();
				yield return new STACusExitHeaderArchiveStageDescriptor();
				yield return new STAAsycudaManifestHeaderArchiveStageDescriptor();
				yield return new STACusInBondHeaderArchiveStageDescriptor();
				yield return new STACusSCAOceanBillArchiveStageDescriptor();
				yield return new STACusUnderbondArchiveStageDescriptor();
				yield return new STACusIntraStatGroupArchiveStageDescriptor();
				yield return new STACusIntrastatHeaderArchiveStageDescriptor();
			}

			yield return new STAHVLVBookingHeaderArchiveStageDescriptor();
			yield return new STAHVLVOriginLoadListArchiveStageDescriptor();
		}

		public IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config)
		{
			foreach (var stageDescriptor in GetArchiveStageDescriptors(config))
			{
				yield return new STAArchiveStage(stageDescriptor, this);
			}
		}

		public IEnumerable<string> GetRegistryLogs()
		{
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.StandaloneRecordsOnOrBeforeMinimum.Name);
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.StandaloneRecordsBatchSizeControl.Name);
		}
	}
}
