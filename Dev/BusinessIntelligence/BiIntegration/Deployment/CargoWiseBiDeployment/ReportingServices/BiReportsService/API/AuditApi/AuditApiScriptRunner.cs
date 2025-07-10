using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Bi.Common;
using CargoWise.Data;

namespace CargoWise.Bi.Deployment.ReportingServices
{
	internal class AuditApiScriptRunner
	{
		string AuditServer => BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);

		DbConnection AuditConnection => lazyAuditConnection.Value;
		Lazy<DbConnection> lazyAuditConnection => new(() => Db.NewExtraConnectionWithMainDbCredentials(AuditServer, Db.AuditDatabaseName));

		public virtual DataTable GetChangedTables(byte[] fromLsn)
		{
			using (var cmd = AuditConnection.Command("biadmin.usp_GetChangedTables"))
			{
				cmd.AddParameter("@fromLsn", SqlDbType.Binary, 10, fromLsn);
				cmd.CommandType = CommandType.StoredProcedure;
				return DataUtils.GetDataTableFromCommand(cmd);
			}
		}

		public virtual byte[] GetMaxLsn()
		{
			byte[] maxLsn;
			maxLsn = BiMasterState.GetParameterAsByteArray(AuditConnection, BiConstants.LastMaxLsnProcessed);
			return maxLsn;
		}

		#region Change Detail

#pragma warning disable CW1108 // Do Not Use DataSet
		public DataSet GetSchemaMappingDetails(string schemaName, string tableName, byte[] afterLsn)
#pragma warning restore CW1108 // Do Not Use DataSet
		{
			using (var cmd = AuditConnection.Command("biadmin.usp_GetSchemaMappingDetails"))
			{
				cmd.AddParameter("@schemaName", SqlDbType.NVarChar, 128, schemaName);
				cmd.AddParameter("@tableName", SqlDbType.NVarChar, 128, tableName);
				cmd.AddParameter("@afterLsn", SqlDbType.Binary, 10, afterLsn);

				cmd.CommandType = CommandType.StoredProcedure;

#pragma warning disable CW1108 // Do Not Use DataSet
				DataSet dataSet = DataUtils.GetDataSetFromQuery(cmd);
#pragma warning restore CW1108 // Do Not Use DataSet
				using (var adapter = cmd.NewDataAdapter())
				{
					adapter.Fill(dataSet);
				}

				return dataSet;
			}
		}

		public List<ChangeData> GetChangeData(string schemaName, string tableName, byte[] afterLsnBytes, byte[] maxLsn, List<string> columnDetails, byte[] afterSeqVal, int afterCommandId, int afterOperation, int pageSize)
		{
			var changeDataDetails = new List<ChangeData>();

			var afterLsnPeriod = GetLsnPeriod(afterLsnBytes);
			var maxLsnPeriod = GetLsnPeriod(maxLsn);

			var query =
$@"SELECT TOP {pageSize}
	CONVERT(VARCHAR(50), __$start_lsn, 1) AS __$start_lsn,
	CONVERT(VARCHAR(50), __$seqval, 1) AS __$seqval,
	CONVERT(VARCHAR(500), __$update_mask, 1) AS __$update_mask,
	__$lsn_period,
	__$command_id,
	__$operation,
	{string.Join(", ", columnDetails)}
FROM [{schemaName}].[{tableName}]
WHERE
	__$lsn_period >= @AfterLsnPeriod AND __$lsn_period <= @MaxLsnPeriod
	AND __$start_lsn <= @MaxLsn
	AND (__$start_lsn > @AfterLsn OR
	(
		__$start_lsn = @AfterLsn AND (__$seqval > @AfterSeqVal OR
		(
			__$seqval = @AfterSeqVal AND __$command_id > @AfterCommandId OR
			(
				__$command_id = @AfterCommandId AND __$operation > @AfterOperation OR
				(
					__$operation = @AfterOperation AND __$seqval > @AfterSeqVal
				)
			)
		)
	)))
ORDER BY __$start_lsn, __$command_id, __$seqval, __$operation";

			using (var cmd = AuditConnection.Command(query))
			{
				cmd.AddParameter("@AfterLsn", SqlDbType.Binary, 10, afterLsnBytes);
				cmd.AddParameter("@AfterSeqVal", SqlDbType.Binary, 10, afterSeqVal);
				cmd.AddParameter("@AfterCommandId", SqlDbType.Int, afterCommandId);
				cmd.AddParameter("@AfterOperation", SqlDbType.Int, afterOperation);
				cmd.AddParameter("@MaxLsn", SqlDbType.Binary, 10, maxLsn);

				cmd.AddParameter("@AfterLsnPeriod", SqlDbType.SmallInt, afterLsnPeriod);
				cmd.AddParameter("@MaxLsnPeriod", SqlDbType.SmallInt, maxLsnPeriod);

				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						var changeData = new ChangeData();
						changeData.start_lsn = reader.GetString(0);
						changeData.seqval = reader.GetString(1);
						changeData.update_mask = reader.GetString(2);
						changeData.lsn_period = reader.GetInt16(3);
						changeData.command_id = reader.GetInt32(4);
						changeData.operation = reader.GetInt32(5);

						var data = new List<ColumnData>();
						for (int i = 6; i < reader.FieldCount; i++)
						{
							var columnData = new ColumnData();
							columnData.columnName = reader.GetName(i);

							var val = reader.GetValue(i);
							if (val != DBNull.Value)
							{
								var type = reader.GetFieldType(i).Name;
								var binaryTypes = new List<string> { "Byte[]" };
								if (binaryTypes.Contains(type))
								{
									columnData.value = HexStringHelper.BytesToHexString((byte[])val);
								}
								else
								{
									columnData.value = Convert.ToString(val, CultureInfo.InvariantCulture);
								}
							}
							else
							{
								columnData.value = null;
							}
							data.Add(columnData);
						}

						changeData.data = data.ToArray();
						changeDataDetails.Add(changeData);
					}
				}
			}

			return changeDataDetails;
		}

		short? GetLsnPeriod(byte[] lsn)
		{
			using (var cmd = AuditConnection.Command("SELECT ISNULL(biadmin.udf_GetLsnPeriodFromLsn(@Lsn), 0)"))
			{
				cmd.AddParameter("@Lsn", SqlDbType.Binary, 10, lsn);
				return Convert.ToInt16(cmd.ExecuteScalar(), CultureInfo.InvariantCulture);
			}
		}

		#endregion
	}
}
