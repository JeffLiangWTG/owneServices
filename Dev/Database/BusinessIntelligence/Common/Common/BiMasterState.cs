namespace CargoWise.Bi.Common
{
	using System;
	using System.Data;
	using System.Globalization;
	using CargoWise.Common;
	using CargoWise.Data;
	using CargoWise.Data.Utils;

	public static class BiMasterState
	{
		#region SuppressResourceStringsCheckRegion

		static void ExecuteWithLock(DbConnection connection, string databaseName, string paramName, Action action)
		{
			var lockName = $"bimasterstate_{paramName}";
			if (connection.TryGetLock(lockName, TimeSpan.FromSeconds(1), out SqlApplicationLock applock, databaseName))
			{
				using (applock)
				{
					action();
				}
			}
			else
			{
				throw new TimeoutException($"Could not acquire lock for parameter: {paramName}");
			}
		}

		public static void SetParameter(DbConnection connection, string paramName, string paramValue)
		{
			ExecuteWithLock(connection, connection.CurrentDatabase, paramName, () =>
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture, @"[{0}].usp_SetMasterStateParameter", BiConstants.BiAdminSchemaName);

				using (var cmd = connection.Command(sqlText))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					cmd.AddParameter("@ParamName", SqlDbType.NVarChar, -1, paramName);
					cmd.AddParameter("@ParamValue", SqlDbType.NVarChar, -1, paramValue);
					cmd.ExecuteNonQuery();
				}
			});
		}

		public static string GetParameter(DbConnection connection, string paramName, string databaseName = null)
		{
			databaseName = databaseName.IsNullOrEmpty() ? connection.CurrentDatabase : databaseName;

			var sqlText = string.Format(CultureInfo.InvariantCulture, "[{0}].[{1}].usp_GetMasterStateParameter", databaseName, "biadmin");
			using DbCommand dbCommand = connection.Command(sqlText);
			dbCommand.CommandType = CommandType.StoredProcedure;
			dbCommand.AddParameter("@ParamName", SqlDbType.NVarChar, -1, paramName);
			dbCommand.AddOutputParameter("@ParamValue", SqlDbType.NVarChar, -1, 0, 0, null);
			dbCommand.ExecuteNonQuery();
			var parameterValue = dbCommand.GetParameterValue("@ParamValue");
			var result = (parameterValue == DBNull.Value) ? string.Empty : parameterValue.ToString();

			return result;
		}

		public static byte[] GetParameterAsByteArray(DbConnection connection, string paramName)
		{
			var sqlText = $@"
DECLARE @ParamValue BINARY(10) = 0x0
SELECT @ParamValue = ISNULL(
	CONVERT(BINARY(10), ParamValue, 1),
	0x00000000000000000000
) FROM [{BiConstants.BiAdminSchemaName}].MasterState
WHERE ParamName = '{paramName}' AND ParamValue <> ''
SELECT @ParamValue";
			return (byte[])connection.ExecuteScalar(sqlText);
		}

		public static DateTime? GetParameterDate(DbConnection connection, string paramName)
		{
			var paramValue = GetParameter(connection, paramName);
			if (!string.IsNullOrEmpty(paramValue))
			{
				return Convert.ToDateTime(paramValue, CultureInfo.InvariantCulture);
			}
			else
			{
				return null;
			}
		}

		public static void DeleteParameter(DbConnection connection, string paramName)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, "[{0}].usp_DeleteMasterStateParameter", BiConstants.BiAdminSchemaName);
			using (var cmd = connection.Command(sqlText))
			{
				cmd.CommandType = CommandType.StoredProcedure;
				cmd.AddParameter("@paramName", SqlDbType.NVarChar, 128, paramName);
				cmd.ExecuteNonQuery();
			}
		}

		public static string GetBiDatabaseExtPty(DbConnection connection, string dbName, string paramName)
		{
			using (((ICurrentDbControl)connection).UseDatabase(dbName))
			{
				var sqlText = $@"
					declare @value varchar(128) = ''
					select @value = convert(varchar(128), value) from sys.extended_properties where name = '{paramName}' AND class_desc = 'DATABASE'
					select @value";
				return Convert.ToString(connection.ExecuteScalar(sqlText));
			}
		}

		public static short? GetLastMaxLsnProcessedPeriod(DbConnection auditConnection)
		{
			var sqlText = "SELECT biadmin.udf_GetLsnPeriodFromLsn(CONVERT(binary(10), (SELECT TOP 1 ParamValue FROM biadmin.MasterState WHERE ParamName = 'LAST_MAX_LSN_PROCESSED'), 1))";
			var result = auditConnection.ExecuteScalar(sqlText);
			return result is short period ? period : null;
		}

		#endregion
	}
}
