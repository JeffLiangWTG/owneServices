using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.DocumentScanning
{
	class PopulateSC_SystemCreateUserFromLogs : DataTransformation
	{
		public override string UserDescription => "Populate SC_SystemCreateUser from StmALog logs since 27-Apr-21";

		public int TopCount { get; set; } = 5000;

		protected override void OnlinePostUpgradeTransform(CancellationToken token)
		{
			base.OnlinePostUpgradeTransform(token);

			var dbListToBeUpgraded = Db.Connection.GetDatabases(DatabaseType.SD).ToList();
			dbListToBeUpgraded.Add(Db.DatabaseName);

			var dbUpgraded = ExtProperty.Database.Select(Db.Connection, UpdateStorageDocsAuditColumnsValuesDbUpgraded);
			if (!string.IsNullOrEmpty(dbUpgraded) && dbListToBeUpgraded.Contains(dbUpgraded))
			{
				dbListToBeUpgraded.RemoveRange(0, dbListToBeUpgraded.IndexOf(dbUpgraded) + 1);
			}

			var currentDbIndex = 0;
			foreach (var database in dbListToBeUpgraded)
			{
				using (((ICurrentDbControl)Db.Connection).UseDatabase(database))
				{
					TransformCore(database, token);
				}

				ExtProperty.Database.Update(Db.Connection, UpdateStorageDocsAuditColumnsValuesDbUpgraded, database);
				manager?.ShowInfoMessage($"Finished updating audit columns values of storagedocs in {database}, {++currentDbIndex}/{dbListToBeUpgraded.Count}.");
			}

			ExtProperty.Database.Delete(Db.Connection, UpdateStorageDocsAuditColumnsValuesDbUpgraded);
			return;
		}

		void TransformCore(string database, CancellationToken token)
		{
			using (TemporaryAlterDocManagerDbWriteableIfNeeded(database))
			{
				var sqlText = $@"
DECLARE @top bigint = {TopCount}
DECLARE @maxCreateTime smalldatetime = ''

;WITH Docs as
(
	SELECT TOP (@top) with ties
		SC_PK, SC_SystemCreateUser, SC_SystemLastEditUser, SC_SystemCreateTimeUtc
	FROM
		dbo.StorageDocs WITH (INDEX(NR_RX__SC_SystemCreateTimeUtc), FORCESEEK)
	WHERE
		SC_SystemCreateTimeUtc > {LowWatermarkName} AND SC_SystemCreateUser = '~BP' ORDER BY SC_SystemCreateTimeUtc
),
MaxCreateTimeDocs as
(
	SELECT Docs.*, MaxCreateTime = MAX(SC_SystemCreateTimeUtc) OVER() FROM Docs
)
UPDATE
	MaxCreateTimeDocs
SET
	@maxCreateTime = MaxCreateTime,
	SC_SystemCreateUser      = ISNULL(Logs.CreateUser, '~BP'),
	SC_SystemLastEditUser    = ISNULL(Logs.LastEditUser, '~BP')
FROM
	MaxCreateTimeDocs OUTER APPLY
	(
		SELECT TOP (1)
			CreateUser   = FIRST_VALUE(IIF(SL_SE_NKEvent = 'ADD', SL_GS_NKUser, NULL)) OVER (ORDER BY SL_PostedTimeUtc ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW),
			LastEditUser = LAST_VALUE(SL_GS_NKUser) OVER (ORDER BY SL_PostedTimeUtc ROWS BETWEEN CURRENT ROW AND UNBOUNDED FOLLOWING)
		FROM
			{Db.DatabaseName.QuoteName()}.dbo.StmAlog
		WHERE
			SL_Parent = MaxCreateTimeDocs.SC_PK
	) AS Logs
WHERE Logs.CreateUser IS NOT NULL AND Logs.CreateUser != '~BP'

SELECT @@ROWCOUNT, @maxCreateTime
";

				var upgradedCount = 0;
				var lowWatermarkValue = new DateTime(2021, 4, 27, 0, 0, 0, DateTimeKind.Utc);
				var stopWatch = Stopwatch.StartNew();
				manager?.ShowInfoMessage($"Start updating SC_SystemCreateUser values of storagedocs in {database}.");
				while (true)
				{
					var count = 0;
					Db.Connection.ExecuteReader(
						sqlText,
						cmd => cmd.AddParameter(LowWatermarkName, System.Data.SqlDbType.SmallDateTime, lowWatermarkValue),
						reader => {
							count = reader.GetInt32(0);
							if (!reader.IsDBNull(1))
							{
								lowWatermarkValue = reader.GetDateTime(1);
							}
						});
					if (count > 0)
					{
						upgradedCount += count;
						if (stopWatch.Elapsed > TimeSpan.FromMinutes(1))
						{
							manager?.ShowInfoMessage($"Finished processing {upgradedCount:N0} item(s) in storagedocs in {database}.");
							stopWatch.Restart();
						}
						token.ThrowIfCancellationRequested();
					}
					else
					{
						break;
					}
				}
			}
		}

		IDisposable TemporaryAlterDocManagerDbWriteableIfNeeded(string database)
		{
			if (DocManagerUtils.IsDocManagerDatabase(database) && !DocManagerUtils.IsDbWriteableForDocManager(database))
			{
				Db.Connection.AlterDbWriteableStateForDocManager(database, true);
				return new DisposableAction(() => Db.Connection.AlterDbWriteableStateForDocManager(database, false));
			}
			else
			{
				return null;
			}
		}

		const string UpdateStorageDocsAuditColumnsValuesDbUpgraded = "UpdateStorageDocsAuditColumnsValues.DbUpgraded";
		const string LowWatermarkName = "@lowWatermark";
	}
}
