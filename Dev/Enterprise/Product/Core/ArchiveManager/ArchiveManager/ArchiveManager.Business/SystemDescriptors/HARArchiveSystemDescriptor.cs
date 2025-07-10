using System.Collections.Generic;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Business.StageDescriptors;
using Enterprise.ArchiveManager.Business.SystemDescriptors;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

[assembly: ArchiveSystemDescriptorProvider(typeof(HARArchiveSystemDescriptor))]

namespace Enterprise.ArchiveManager.Business.SystemDescriptors
{
	public class HARArchiveSystemDescriptor : IArchiveSystemDescriptor
	{
		public string Code
			=> ArchiveManagerConstants.Codes.HAR;

		public MultilingualString Name
			=> ResString.GetMultilingualString("4B2677E3-6CBF-4FEF-95D9-21D7D5778955", ArchiveManagerConstants.Names.HAR);

		public MultilingualString Noun
			=> SystemDescriptorStrings.GetNoun(SystemDescriptorType.Archive);

		public MultilingualString PresentTenseVerb
			=> SystemDescriptorStrings.GetPresentTenseVerb(SystemDescriptorType.Archive);

		public MultilingualString PastTenseVerb
			=> SystemDescriptorStrings.GetPastTenseVerb(SystemDescriptorType.Archive);

		public bool AllowShouldArchiveDeclaration
			=> false;

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
			yield return new HARArchiveStageDescriptor();
		}

		public IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config)
		{
			foreach (var stageDescriptor in GetArchiveStageDescriptors(config))
			{
				yield return new HARArchiveStage(stageDescriptor, this);
			}
		}

		public IEnumerable<string> GetRegistryLogs()
		{
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.HVLVArchiveSystemOnOrBeforeMinimum.Name);
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.HVLVArchiveSystemBatchSizeControl.Name);
			yield return RegistryHelper.ToLog(SystemDataRegistry.Instance.ArchivingFileFormat.Name);
		}
	}
}
