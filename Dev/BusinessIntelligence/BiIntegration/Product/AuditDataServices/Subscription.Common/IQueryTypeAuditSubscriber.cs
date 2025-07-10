using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Schema;
namespace Enterprise.AuditDataServices.Subscription.Common
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Types implementing IAuditSubscriber are loaded by reflection.")]
	public interface IQueryTypeAuditSubscriber : IAuditSubscriber
	{
		/// <summary>
		/// Notify when operation == 1
		/// </summary>
		bool NotifyDelete { get; }

		/// <summary>
		/// Notify when operation == 2
		/// </summary>
		bool NotifyInsert { get; }

		/// <summary>
		/// Notify when operation == 3 (before) and 3 (after)
		/// Or when there is an operation 1 and 2 with the same LSN, also known as a "deferred update"
		/// </summary>
		bool NotifyUpdate { get; }

		/// <summary>
		/// Only interested in changes on specified columns.
		/// Null or Empty enumerable means all columns are considered.
		/// Applies to updates only.
		/// </summary>
		IEnumerable<SchemaColumn> SpecificColumns { get; }

		Action<DataRow> CustomFilter { get; }
	}
}
