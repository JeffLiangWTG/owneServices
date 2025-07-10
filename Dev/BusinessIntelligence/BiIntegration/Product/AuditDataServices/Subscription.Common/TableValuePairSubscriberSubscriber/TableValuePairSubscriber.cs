using System.Collections.Generic;
using CargoWise.Data;
using Enterprise.Integration;

namespace Enterprise.AuditDataServices.Subscription.Common
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Types implementing IAuditSubscriber are loaded by reflection.")]
	public abstract class TableValuePairSubscriber : ITableValuePairSubscriber
	{
		public abstract string Code { get; }

		public abstract string Description { get; }

		public Dictionary<IChangedTableSchema, List<string>> ChangedTableColumnValues { get; set; }

		public IAuditSubscriberWrapper GetWrapper(DbConnection auditConnection, ILogger logger)
		{
			return new TableValuePairSubscriberWrapper(this, auditConnection, logger);
		}

		public abstract bool IsRequired();

		public abstract void ProcessChanges(ILogger logger, Dictionary<IChangedTableSchema, List<string>> changedTableColumnValues);
	}
}
