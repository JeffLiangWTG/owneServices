using System.Collections.Generic;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.OPS
{
	sealed class OPSArchiveSystemTest : BaseArchiveSystemTest
	{
		protected override IArchiveSystemDescriptor SystemDescriptor
			=> new OPSArchiveSystemDescriptor();

		protected override string ExpectedCode
			=> ArchiveManagerConstants.Codes.OPS;

		protected override string ExpectedName
			=> ArchiveManagerConstants.Names.OPS;

		protected override string ExpectedNoun
			=> "Archive";

		protected override string ExpectedPresentTenseVerb
			=> "Archiving";

		protected override string ExpectedPastTenseVerb
			=> "Archived";

		protected override IReportGenerator ExpectedReportGeneratorType
			=> new ArchiveReportGenerator();

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptors
			=> new() { new OPSArchiveStageDescriptor() };

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptorsWithCustoms
			=> ExpectedArchiveStageDescriptors;

		protected override List<IArchiveStage> ExpectedArchiveStages
			=> new() { new OPSArchiveStage(new OPSArchiveStageDescriptor(), SystemDescriptor) };
	}
}
