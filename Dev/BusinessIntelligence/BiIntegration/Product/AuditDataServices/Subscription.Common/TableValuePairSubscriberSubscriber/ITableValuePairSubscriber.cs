using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.AuditDataServices.Subscription.Common
{
	public interface ITableValuePairSubscriber : IAuditSubscriber
	{
		/// <summary>
		/// After we check the desired columns for changes,
		/// we populate this field with the list of the values that have changed
		/// </summary>
		Dictionary<IChangedTableSchema, List<string>> ChangedTableColumnValues { get; }

		void ProcessChanges(ILogger logger, Dictionary<IChangedTableSchema, List<string>> changedTableColumnValues);
	}
}
