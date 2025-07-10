using System.Collections.Generic;
using System.Linq;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.PAR
{
	sealed class PARPurgeSystemTest : BaseArchiveSystemTest
	{
		protected override IArchiveSystemDescriptor SystemDescriptor
			=> new PARPurgeSystemDescriptor();

		protected override string ExpectedCode
			=> ArchiveManagerConstants.Codes.PAR;

		protected override string ExpectedName
			=> ArchiveManagerConstants.Names.PAR;

		protected override string ExpectedNoun
			=> "Purge";

		protected override string ExpectedPresentTenseVerb
			=> "Purging";

		protected override string ExpectedPastTenseVerb
			=> "Purged";

		protected override IReportGenerator ExpectedReportGeneratorType
			=> new PurgeReportGenerator();

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptors
			=> new() { new PARDeleteStageDescriptor() };

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptorsWithCustoms
			=> ExpectedArchiveStageDescriptors;

		protected override List<IArchiveStage> ExpectedArchiveStages
			=> new() { new PARArchiveStage(new PARDeleteStageDescriptor(), SystemDescriptor) };

		public override void TestAllowShouldArchiveDeclaration()
			=> Assert(!SystemDescriptor.AllowShouldArchiveDeclaration);

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
