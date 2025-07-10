using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.Integration;

namespace Enterprise.AuditDataServices.Subscription.Common
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Types implementing IAuditSubscriber are loaded by reflection.")]
	public abstract class ChangedTableListOnlyAuditSubscriber : IChangedTableListOnlyAuditSubscriber
	{
		public abstract string Code { get; }
		public abstract string Description { get; }

		/// <summary>
		/// Tables is mandatory and must yield at least one element.
		/// </summary>
		public abstract IEnumerable<ITableSchema> SubscribedTables { get; }

		public IAuditSubscriberWrapper GetWrapper(DbConnection auditConnection, ILogger logger)
		{
			return new ChangedTableListOnlyAuditSubscriberWrapper(this, auditConnection, logger);
		}

		public abstract bool IsRequired();

		public abstract void ProcessChanges(ILogger logger, IEnumerable<ITableSchema> changedTables);
	}
}
