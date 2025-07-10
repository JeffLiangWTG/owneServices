using System.Data;
using CargoWise.Schema;
using Enterprise.Integration;

namespace Enterprise.AuditDataServices.Subscription.Common
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Types implementing IAuditSubscriber are loaded by reflection.")]
	public interface IActualDataChangesAuditSubscriber : IQueryTypeAuditSubscriber
	{
		/// <summary>
		/// Table is mandatory
		/// </summary>
		ITableSchema Table { get; }

		/// <summary>
		/// SQL filter containing expressions with table columns
		/// </summary>

		void ProcessChanges(ILogger logger, DataTable changeTable);
	}
}
