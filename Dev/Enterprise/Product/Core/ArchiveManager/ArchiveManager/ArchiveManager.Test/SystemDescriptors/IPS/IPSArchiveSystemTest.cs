using System.Collections.Generic;
using System.Linq;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions.ArchiveReport;
using Enterprise.ArchiveManager.Engine.ArchiveStages;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Test.SystemDescriptors.IPS
{
	sealed class IPSArchiveSystemTest : BaseArchiveSystemTest
	{
		protected override IArchiveSystemDescriptor SystemDescriptor
			=> new IPSArchiveSystemDescriptor();

		protected override string ExpectedCode
			=> ArchiveManagerConstants.Codes.IPS;

		protected override string ExpectedName
			=> ArchiveManagerConstants.Names.IPS;

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
			new IPSInactiveShipmentArchiveStageDescriptor(),
			new IPSInactiveConsolArchiveStageDescriptor(),
			new IPSInactiveRatingHeaderArchiveStageDescriptor(),
			new IPSInactiveJobCartageArchiveStageDescriptor(),
		};

		protected override List<IArchiveStageDescriptor> ExpectedArchiveStageDescriptorsWithCustoms
			=> ExpectedArchiveStageDescriptors.Append(new IPSInactiveJobDeclarationArchiveStageDescriptor()).ToList();

		protected override List<IArchiveStage> ExpectedArchiveStages
		=> new()
		{
			new IPSArchiveStage(new IPSInactiveShipmentArchiveStageDescriptor(), SystemDescriptor),
			new IPSArchiveStage(new IPSInactiveConsolArchiveStageDescriptor(), SystemDescriptor),
			new IPSArchiveStage(new IPSInactiveRatingHeaderArchiveStageDescriptor(), SystemDescriptor),
			new IPSArchiveStage(new IPSInactiveJobCartageArchiveStageDescriptor(), SystemDescriptor),
		};
	}
}
