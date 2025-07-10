using System.Collections.Generic;
using System.Linq;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Business.StageDescriptors;
using Enterprise.ArchiveManager.Business.SystemDescriptors;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.RED
{
	internal class REDArchiveSystemTest : BaseArchiveSystemTest
	{
		protected override IArchiveSystemDescriptor SystemDescriptor =>
			new REDArchiveSystemDescriptor();

		protected override string ExpectedCode
			=> ArchiveManagerConstants.Codes.RED;

		protected override string ExpectedName
			=> ArchiveManagerConstants.Names.RED;

		protected override string ExpectedNoun
			=> "Purge";

		protected override string ExpectedPresentTenseVerb
			=> "Purging";

		protected override string ExpectedPastTenseVerb
			=> "Purged";

		public override void TestAllowShouldArchiveDeclaration()
			=> Assert(!SystemDescriptor.AllowShouldArchiveDeclaration);

		protected override IReportGenerator ExpectedReportGeneratorType
			=> new PurgeReportGenerator();

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptors
			=> new() { new REDArchiveStageDescriptor() };

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptorsWithCustoms
			=> ExpectedArchiveStageDescriptors;

		protected override List<IArchiveStage> ExpectedArchiveStages
			=> new() { new REDArchiveStage(new REDArchiveStageDescriptor(), SystemDescriptor) };

		public override void TestGetRegistryLogs()
		{
			var registryLogs = SystemDescriptor.GetRegistryLogs().ToList();

			AssertEquals(2, registryLogs.Count);
			CombineAssertions("Registry logs", () =>
			{
				AssertEquals("On or Before Minimum: 7", registryLogs[0]);
				AssertEquals("Set Batch Size: 100", registryLogs[1]);
			});
		}
	}
}
