using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Integration;

namespace Enterprise.ArchiveManager.Business
{
	abstract class IPSArchiveDescriptor : CommonArchiveStageDescriptor
	{
		protected internal abstract SchemaBoolColumn IsCancelledSchemaColumn { get; }

		public override ZQuery GetMainArchiveableFilter(IArchiveConfiguration config)
		{
			var query = new ZQuery(IsCancelledSchemaColumn, true);
			_ = query.AddToFilter(MainDateFilterColumn, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, config.ArchiveJobsOnOrBeforeThisDate);
			query.OrderBy = MainDateFilterColumn.Name;

			return query;
		}
	}
}
