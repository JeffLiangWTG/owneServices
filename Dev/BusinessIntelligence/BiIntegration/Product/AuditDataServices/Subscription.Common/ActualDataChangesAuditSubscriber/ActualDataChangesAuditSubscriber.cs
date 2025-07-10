using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Schema;
using Enterprise.Integration;

namespace Enterprise.AuditDataServices.Subscription.Common
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("Types implementing IAuditSubscriber are loaded by reflection.")]
	public abstract class ActualDataChangesAuditSubscriber : IActualDataChangesAuditSubscriber
	{
		public abstract bool NotifyInsert { get; }

		public abstract bool NotifyUpdate { get; }

		public abstract bool NotifyDelete { get; }

		public abstract string Code { get; }

		public abstract string Description { get; }

		/// <summary>
		/// Table is mandatory
		/// </summary>
		public abstract ITableSchema Table { get; }

		/// <summary>
		/// Only interested in changes on specified columns.
		/// Null or Empty enumerable means all columns are considered.
		/// Applies to updates only.
		/// </summary>
		public abstract IEnumerable<SchemaColumn> SpecificColumns { get; }

		public abstract Action<DataRow> CustomFilter { get; }

		/// <summary>
		/// SQL filter containing expressions with table columns
		/// </summary>

		public IAuditSubscriberWrapper GetWrapper(DbConnection auditConnection, ILogger logger)
		{
			return new ActualDataChangesAuditSubscriberWrapper(this, auditConnection, logger);
		}

		public abstract bool IsRequired();

		public abstract void ProcessChanges(ILogger logger, DataTable changeTable);

		#region Implementation

		protected static bool HasCellChanged(DataRow row, DataColumn col)
		{
			var originalVersion = row.HasVersion(DataRowVersion.Original) ? row[col, DataRowVersion.Original] : null;
			var currentVersion = row.HasVersion(DataRowVersion.Current) ? row[col, DataRowVersion.Current] : null;

			if (originalVersion == DBNull.Value && currentVersion == DBNull.Value)
			{
				return false;
			}
			else if (originalVersion != DBNull.Value && currentVersion != DBNull.Value)
			{
				if (originalVersion == null)
				{
					return currentVersion != null;
				}

				return !originalVersion.Equals(currentVersion);
			}

			return true;
		}

		#endregion
	}
}
