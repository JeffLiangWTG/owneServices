using System.Collections.Generic;
using System.Linq;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Business.StageDescriptors;
using Enterprise.ArchiveManager.Business.SystemDescriptors;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.HAR
{
	sealed class HARArchiveSystemTest : BaseArchiveSystemTest
	{
		protected override IArchiveSystemDescriptor SystemDescriptor
			=> new HARArchiveSystemDescriptor();

		protected override string ExpectedCode
			=> ArchiveManagerConstants.Codes.HAR;

		protected override string ExpectedName
			=> ArchiveManagerConstants.Names.HAR;

		protected override string ExpectedNoun
			=> "Archive";

		protected override string ExpectedPresentTenseVerb
			=> "Archiving";

		protected override string ExpectedPastTenseVerb
			=> "Archived";

		protected override IReportGenerator ExpectedReportGeneratorType
			=> new ArchiveReportGenerator();

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptors
			=> new() { new HARArchiveStageDescriptor() };

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptorsWithCustoms
			=> ExpectedArchiveStageDescriptors;

		protected override List<IArchiveStage> ExpectedArchiveStages
			=> new() { new HARArchiveStage(new HARArchiveStageDescriptor(), SystemDescriptor) };

		public override void TestAllowShouldArchiveDeclaration()
			=> Assert(!SystemDescriptor.AllowShouldArchiveDeclaration);

		public override void TestGetRegistryLogs()
		{
			var registryLogs = SystemDescriptor.GetRegistryLogs().ToList();

			CombineAssertions("Registry Settings", () =>
			{
				AssertEquals(3, registryLogs.Count);
				AssertEquals("On or Before Minimum: 1", registryLogs[0]);
				AssertEquals("Set Batch Size: 100", registryLogs[1]);
				AssertEquals("Archiving File Format: CSV", registryLogs[2]);
			});
		}
	}
}
