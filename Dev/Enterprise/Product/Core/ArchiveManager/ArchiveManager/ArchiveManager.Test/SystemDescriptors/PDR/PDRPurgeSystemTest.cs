using System.Collections.Generic;
using System.Linq;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.PDR
{
	sealed class PDRPurgeSystemTest : BaseArchiveSystemTest
	{
		protected override IArchiveSystemDescriptor SystemDescriptor
			=> new PDRPurgeSystemDescriptor();

		protected override string ExpectedCode
			=> ArchiveManagerConstants.Codes.PDR;

		protected override string ExpectedName
			=> ArchiveManagerConstants.Names.PDR;

		protected override string ExpectedNoun
			=> "Purge";

		protected override string ExpectedPresentTenseVerb
			=> "Purging";

		protected override string ExpectedPastTenseVerb
			=> "Purged";

		protected override IReportGenerator ExpectedReportGeneratorType
			=> new PurgeReportGenerator();

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptors
			=> new() { new PDRArchiveStageDescriptor() };

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptorsWithCustoms
			=> ExpectedArchiveStageDescriptors;

		protected override List<IArchiveStage> ExpectedArchiveStages
			=> new() { new PDRPurgeStage(new PDRArchiveStageDescriptor(), SystemDescriptor) };

		public override void TestGetRegistryLogs()
		{
			var registryLogs = SystemDescriptor.GetRegistryLogs().ToList();

			AssertEquals(2, registryLogs.Count);
			CombineAssertions("Registry logs", () =>
			{
				AssertEquals("On or Before Minimum: 7", registryLogs[0]);
				AssertEquals("Set Batch Size for Archiving and Purging Operational Jobs: 50", registryLogs[1]);
			});
		}
	}
}
