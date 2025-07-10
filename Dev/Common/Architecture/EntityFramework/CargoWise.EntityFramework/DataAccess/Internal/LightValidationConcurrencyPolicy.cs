using System.ComponentModel;
using System.Data;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework
{
	[Immutable]
	[ImmutableObject(true)]
	sealed class LightValidationConcurrencyPolicy : ConcurrencyPolicy
	{
		internal static readonly ConcurrencyPolicy Instance = new LightValidationConcurrencyPolicy();

		public LightValidationConcurrencyPolicy()
			: base(allowMerge: true, collisionCheck: CollisionCheck.OnChange)
		{
		}

		public override bool ShouldCheck(DataRow row, DataColumn column)
		{
			// Only do concurrency check on Light validation field, if row has changed, and light validation changed from FALSE to TRUE

			return row.RowState == DataRowState.Modified // Row is updated (ignore added and deleted rows)
				&& GetCurrentValue(row, column) // Current value: IsValid = true
				&& !GetOriginalValue(row, column); // Original value: IsValid = false
		}

		bool GetOriginalValue(DataRow row, DataColumn column)
		{
			return GetValue(row, column, DataRowVersion.Original);
		}

		bool GetCurrentValue(DataRow row, DataColumn column)
		{
			return GetValue(row, column, DataRowVersion.Current);
		}

		static bool GetValue(DataRow row, DataColumn column, DataRowVersion version)
		{
			var valueObject = row.HasVersion(version) ? row[column, version] : null;
			return (valueObject is bool) && (bool)valueObject;
		}

		public override bool AllowAutomaticMergeIfDatabaseValuesAreEqual(DataRow row, DataColumn column)
		{
			// I.e. use following in WHERE:
			//     AND (XX_IsValid = @OldValue OR XX_IsValid = @NewValue)
			// If this is the only change, and db row has changed and has same value - allow automatic merge.
			// If local row has other changes - use strict concurrency check, as combination of different changed columns may result in non-valid record.

			var columns = row.Table.Columns;
			for (int i = 0; i < columns.Count; i++)
			{
				var col = columns[i];
				if (col != column && !ZUpdateCommandBuilder.IsSystemColumn(col) && row.IsValueChanged(col))
				{
					// Do not allow auto-merging light validation in concurrency check if there are other non-system column changes
					return false;
				}
			}

			return base.AllowAutomaticMergeIfDatabaseValuesAreEqual(row, column);
		}

		public override string ToString()
		{
			return Default.ToString(); // Actual policy depends on light validation column value, but it is not accessible here
		}
	}
}
