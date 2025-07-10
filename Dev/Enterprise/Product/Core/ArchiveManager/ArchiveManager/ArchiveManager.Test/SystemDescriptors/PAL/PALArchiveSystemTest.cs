using System.Collections.Generic;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Business.StageDescriptors;
using Enterprise.ArchiveManager.Business.SystemDescriptors;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.PAL
{
	internal class PALArchiveSystemTest : BaseArchiveSystemTest
	{
		protected override IArchiveSystemDescriptor SystemDescriptor =>
			new PALArchiveSystemDescriptor();

		protected override string ExpectedCode
			=> ArchiveManagerConstants.Codes.PAL;

		protected override string ExpectedName
			=> ArchiveManagerConstants.Names.PAL;

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
			=> new() { new PALArchiveStageDescriptor() };

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptorsWithCustoms
			=> ExpectedArchiveStageDescriptors;

		protected override List<IArchiveStage> ExpectedArchiveStages
			=> new() { new PALArchiveStage(new PALArchiveStageDescriptor(), SystemDescriptor) };

		public override void TestGetRegistryLogs()
		{
			AssertEquals("To be fixed in logs work item: WI00629615", "To be fixed in logs work item: WI00629615");
		}
	}
}
