using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace ZClientEDI.Business.ArchiveManager.StageDescriptors
{
	public class ESTApplicationActiveLoggerStageDescriptor : CommonArchiveStageDescriptor
	{
		public override string Name
			=> "Purge Application Active Logger";

		public override SchemaColumn MainArchivePKColumn
			=> ApplicationActiveLoggerSchema.PK;

		public override SchemaColumn MainArchiveNKColumn
			=> null;

		public override SchemaDateTimeColumn MainDateFilterColumn
			=> ApplicationActiveLoggerSchema.AAL_SystemLastEditTimeUtc;

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
			=> new(MainDateFilterColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, config.ArchiveJobsOnOrBeforeThisDate);

		public override IEnumerable<IArchivePreparationAction> GetPreparationAction(IArchiveLogger logger, IArchiveSet set, IArchiveSystemCache cache, IArchiveConfiguration config)
			=> [];

		public override IEnumerable<IArchiveAction> GetArchiveAction(IArchiveLogger logger, IArchiveSet set)
			=> [];
	}
}
