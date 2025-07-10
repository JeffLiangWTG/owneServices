using System.Collections.Generic;
using System.Linq;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.STA
{
	sealed class STAArchiveSystemTest : BaseArchiveSystemTest
	{
		protected override IArchiveSystemDescriptor SystemDescriptor
			=> new STAArchiveSystemDescriptor();

		protected override string ExpectedCode
			=> ArchiveManagerConstants.Codes.STA;

		protected override string ExpectedName
			=> ArchiveManagerConstants.Names.STA;

		protected override string ExpectedNoun
			=> "Archive";

		protected override string ExpectedPresentTenseVerb
			=> "Archiving";

		protected override string ExpectedPastTenseVerb
			=> "Archived";

		protected override IReportGenerator ExpectedReportGeneratorType
			=> new ArchiveReportGenerator();

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptors
			=> new()
			{
				new STAHVLVBookingHeaderArchiveStageDescriptor(),
				new STAHVLVOriginLoadListArchiveStageDescriptor()
			};

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptorsWithCustoms
			=> new()
			{
				new STACusCAeMHMasterArchiveStageDescriptor(),
				new STACusCAeMHHouseArchiveStageDescriptor(),
				new STACusTempStorageJobHeaderArchiveStageDescriptor(),
				new STACusTempStorageRegHeaderArchiveStageDescriptor(),
				new STACusExitHeaderArchiveStageDescriptor(),
				new STAAsycudaManifestHeaderArchiveStageDescriptor(),
				new STACusInBondHeaderArchiveStageDescriptor(),
				new STACusSCAOceanBillArchiveStageDescriptor(),
				new STACusUnderbondArchiveStageDescriptor(),
				new STACusIntraStatGroupArchiveStageDescriptor(),
				new STACusIntrastatHeaderArchiveStageDescriptor(),
				new STAHVLVBookingHeaderArchiveStageDescriptor(),
				new STAHVLVOriginLoadListArchiveStageDescriptor()
			};

		protected override List<IArchiveStage> ExpectedArchiveStages
			=> new()
			{
				new STAArchiveStage(new STAHVLVBookingHeaderArchiveStageDescriptor(), SystemDescriptor),
				new STAArchiveStage(new STAHVLVOriginLoadListArchiveStageDescriptor(), SystemDescriptor),
			};

		public override void TestGetRegistryLogs()
		{
			var registryLogs = SystemDescriptor.GetRegistryLogs().ToList();

			AssertEquals(2, registryLogs.Count);
			CombineAssertions("Registry logs", () =>
			{
				AssertEquals("On or Before Minimum: 7", registryLogs[0]);
				AssertEquals("Set Batch Size: 200", registryLogs[1]);
			});
		}
	}
}
