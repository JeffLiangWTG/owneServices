using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.Integration;

namespace Enterprise.AuditDataServices.Subscription.Common
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Types implementing IAuditSubscriber are loaded by reflection.")]
	public interface IChangedTableListOnlyAuditSubscriber : IAuditSubscriber
	{
		/// <summary>
		/// Tables is mandatory and must yield at least one element.
		/// </summary>
		IEnumerable<ITableSchema> SubscribedTables { get; }

		void ProcessChanges(ILogger logger, IEnumerable<ITableSchema> changedTables);
	}
}
