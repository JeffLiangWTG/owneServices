using System.Collections.Generic;
using System.Linq;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Integration;
using Enterprise.Client.EDI;
using Enterprise.ZArchitecture.Core;
using ZClientEDI.Business.ArchiveManager;
using ZClientEDI.Business.ArchiveManager.StageDescriptors;

[assembly: ArchiveSystemDescriptorProvider(typeof(ESTPurgeSystemDescriptor))]

namespace ZClientEDI.Business.ArchiveManager
{
	public class ESTPurgeSystemDescriptor : ISelfContainedArchiveSystemDescriptor
	{
		public string Code
			=> ArchiveManagerConstants.Codes.EST;

		public MultilingualString Name
			=> (NoResString)ArchiveManagerConstants.Names.EST;

		public MultilingualString Noun
			=> SystemDescriptorStrings.GetNoun(SystemDescriptorType.Purge);

		public MultilingualString PresentTenseVerb
			=> SystemDescriptorStrings.GetPresentTenseVerb(SystemDescriptorType.Purge);

		public MultilingualString PastTenseVerb
			=> SystemDescriptorStrings.GetPastTenseVerb(SystemDescriptorType.Purge);

		public IReportGenerator ReportGenerator
			=> new PurgeReportGenerator();

		public bool AllowShouldArchiveDeclaration
			=> false;

		public bool AllowRecordsWithOrWithoutJobs
			=> false;

		public bool AllowDateParameterSelection
			=> false;

		public bool AllowUsingOnOrBeforeDateWhenWatermarkReset
			=> false;

		public int OnOrBeforeMinimumValue
			=> EDIDataRegistry.Instance.PurgeEdiProdSpecificTablesOnOrBeforeMinimum.Value;

		public int BatchSizeControlValue
			=> EDIDataRegistry.Instance.PurgeEdiProdSpecificTablesBatchSizeControl.Value;

		public IEnumerable<IArchiveStageDescriptor> GetArchiveStageDescriptors(IArchiveConfiguration config)
		{
			yield return new ESTApplicationActiveLoggerStageDescriptor();
		}

		public IEnumerable<IArchiveStage> GetArchiveStages(IArchiveConfiguration config)
		{
			return GetArchiveStageDescriptors(config)
				.Select(stageDescriptor => new ESTPurgeStage(stageDescriptor, this));
		}

		public IEnumerable<string> GetRegistryLogs()
		{
			yield return $"Set Batch Size: {EDIDataRegistry.Instance.PurgeEdiProdSpecificTablesBatchSizeControl.Value}";
			yield return $"On or Before Minimum: {EDIDataRegistry.Instance.PurgeEdiProdSpecificTablesOnOrBeforeMinimum.Value}";
		}
	}
}
