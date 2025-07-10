using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformation.Common
{
	public abstract class PurgeGhostRecordsFromStmALog : DataTransformation
	{
		public virtual bool ParentTableExists => true;
		protected abstract string ParentTableName { get; }
		protected virtual string ParentTablePKColumnName { get; }
		protected virtual IList<string> EventCodes { get; }

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			if (ParentTableExists)
			{
				if (ParentTablePKColumnName.IsNullOrEmpty())
				{
					var paramName = nameof(ParentTablePKColumnName);
					throw new ArgumentNullException(paramName, $"Must specify {paramName} when parent table exists");
				}

				var tableSize = DataUtils.GetApproximateRowCountForTable(Db.Connection, ParentTableName);

				if (tableSize < MaxTableSizeForPurgingByPKs)
				{
					PurgeByPKs(token);
					return;
				}
			}
			PurgeByTableCodeInBatches(token);
		}

		const int MaxTableSizeForPurgingByPKs = 800; // I'd put 1000 here, but for tests just to be safe I would need to add more than 1000 rows to avoid an amnesty, and the maximum number of row value expressions in INSERT is 1000 - it's easier to decrease the max value here leaving 1000 for the test

		void PurgeByPKs(CancellationToken token)
		{
			var whereClause = GenerateWhereClause();

			string sqlText = $@"
DELETE FROM dbo.StmALog
WHERE 1=1
{whereClause}
	AND SL_Parent IN (SELECT {ParentTablePKColumnName} FROM {ParentTableName})
	AND SL_Table = '{ParentTableName}'
OPTION (MAXDOP 1)
";

			Db.Connection.ExecuteNonQuery(sqlText);
		}

		void PurgeByTableCodeInBatches(CancellationToken token)
		{
#pragma warning disable CW1061 // Do not use System.DateTime.UtcNow Rule
			var currentUtcDateTime = DateTime.UtcNow;
#pragma warning restore CW1061 // Do not use System.DateTime.UtcNow Rule

			if (!DateTime.TryParse(ExtProperty.Database.Select(Db.Connection, GetStoredWatermarkName()), out var startWatermarkValue))
			{
				var lowestWatermarkValue = Db.Connection.ExecuteScalar("SELECT MIN(SL_PostedTimeUtc) FROM dbo.StmALog");

				if (lowestWatermarkValue == DBNull.Value || lowestWatermarkValue == null)
				{
					return;
				}

				startWatermarkValue = (DateTime)lowestWatermarkValue;
			}

			var highestWatermarkValue = currentUtcDateTime;
			var watermarkDelta = GetWatermarkDelta();

			var whereClause = GenerateWhereClause(); 

			string sqlText = $@"
DELETE FROM dbo.StmALog
WHERE 1=1
{whereClause}
	AND SL_PostedTimeUtc >= {LowWatermarkName}
	AND SL_PostedTimeUtc < {HighWatermarkName}
	AND SL_Table = '{ParentTableName}'
OPTION (MAXDOP 1)
";

			var lowWatermarkValue = startWatermarkValue;

			while (lowWatermarkValue <= highestWatermarkValue)
			{
				token.ThrowIfCancellationRequested();

				var highWatermarkValue = lowWatermarkValue + watermarkDelta;

				Db.Connection.ExecuteNonQuery(sqlText, cmd =>
				{
					cmd.AddParameter(LowWatermarkName, SqlDbType.DateTime, lowWatermarkValue);
					cmd.AddParameter(HighWatermarkName, SqlDbType.DateTime, highWatermarkValue);
				});

				lowWatermarkValue = highWatermarkValue;
				ExtProperty.Database.Update(Db.Connection, GetStoredWatermarkName(), lowWatermarkValue.ToString());
				OnBatchProcessed();
			}
			ExtProperty.Database.Delete(Db.Connection, GetStoredWatermarkName());
		}

		internal string GenerateWhereClause()
		{
			if (!EventCodes.IsNullOrEmpty())
			{
				return " AND SL_SE_NKEvent IN ('" + String.Join("', '", EventCodes) + "')";
			}
			else
			{
				return "";
			}
		}

		string GetStoredWatermarkName() => $"{GetType().Name}.SL_PostedTimeUtc";

		const string LowWatermarkName = "@lowWatermark";
		const string HighWatermarkName = "@highWatermark";

		protected virtual TimeSpan GetWatermarkDelta() => TimeSpan.FromDays(1);

		public EventHandler<EventArgs> BatchProcessed;

		void OnBatchProcessed() => BatchProcessed?.Invoke(this, EventArgs.Empty);
	}
}
