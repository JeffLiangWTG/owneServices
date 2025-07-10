using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Business.StageDescriptors.PDO;
using Enterprise.ArchiveManager.Engine;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.PDO
{
	sealed class PDOPurgeSystemTest : BaseArchiveSystemTest
	{
		protected override IArchiveSystemDescriptor SystemDescriptor
			=> new PDOPurgeSystemDescriptor();

		protected override string ExpectedCode
			=> ArchiveManagerConstants.Codes.PDO;

		protected override string ExpectedName
			=> ArchiveManagerConstants.Names.PDO;

		protected override string ExpectedNoun
			=> "Purge";

		protected override string ExpectedPresentTenseVerb
			=> "Purging";

		protected override string ExpectedPastTenseVerb
			=> "Purged";

		protected override IReportGenerator ExpectedReportGeneratorType
			=> new PurgeReportGenerator();

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptors
			=> new() { new PDOPurgeStageDescriptor() };

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptorsWithCustoms
			=> ExpectedArchiveStageDescriptors;

		protected override List<IArchiveStage> ExpectedArchiveStages
			=> new() { new PDOPurgeStage(new PDOPurgeStageDescriptor(), SystemDescriptor) };

		public void TestGetArchiveStageDescriptors_WhenShouldIncludeDeclarations_AndShouldIncludeRecordsWithoutJobsForPDO()
		{
			var expectedStages = new List<IArchiveStageDescriptor>()
			{
				new PDOWithoutJobsJobCartageDescriptor(),
				new PDOWithoutJobsJobConsolDescriptor(),
				new PDOWithoutJobsJobShipmentDescriptor(),
				new PDOWithoutJobsRatingHeaderDescriptor(),
				new PDOWithoutJobsJobDeclarationDescriptor(),
			};

			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, shouldIncludeDeclarations: true, shouldIncludeRecordsWithoutJobs: true);
			var archiveStageDescriptors = SystemDescriptor.GetArchiveStageDescriptors(config).ToList();
			var archiveStageDescriptorListsEqual = expectedStages.Select(s => s.GetType()).SequenceEqual(archiveStageDescriptors.Select(s => s.GetType()));

			Assert($"Following archive stage descriptors were expected:\n{string.Join("\n", expectedStages.Select(t => t.Name))}", archiveStageDescriptorListsEqual);
		}

		public void TestGetArchiveStageDescriptors_WhenShouldIncludeRecordsWithoutJobsForPDO()
		{
			var expectedStages = new List<IArchiveStageDescriptor>()
			{
				new PDOWithoutJobsJobCartageDescriptor(),
				new PDOWithoutJobsJobConsolDescriptor(),
				new PDOWithoutJobsJobShipmentDescriptor(),
				new PDOWithoutJobsRatingHeaderDescriptor(),
			};

			var config = new ArchiveConfiguration(ZDateTime.Now, 1, ZDateTime.Now, false, false, shouldIncludeRecordsWithoutJobs: true);
			var archiveStageDescriptors = SystemDescriptor.GetArchiveStageDescriptors(config).ToList();
			var archiveStageDescriptorListsEqual = expectedStages.Select(s => s.GetType()).SequenceEqual(archiveStageDescriptors.Select(s => s.GetType()));

			Assert($"Following archive stage descriptors were expected:\n{string.Join("\n", expectedStages.Select(t => t.Name))}", archiveStageDescriptorListsEqual);
		}

		public override void TestGetRegistryLogs()
		{
			var registryLogs = SystemDescriptor.GetRegistryLogs().ToList();

			AssertEquals(2, registryLogs.Count);
			CombineAssertions("Registry logs", () =>
			{
				AssertEquals("On or Before Minimum: 10", registryLogs[0]);
				AssertEquals("Set Batch Size: 50", registryLogs[1]);
			});
		}
	}
}
