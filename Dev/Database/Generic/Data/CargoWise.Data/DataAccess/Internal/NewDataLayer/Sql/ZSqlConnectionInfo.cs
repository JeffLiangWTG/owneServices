using System.Collections;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Schema;

namespace CargoWise.EntityFramework
{
	public class ZSqlConnectionInfo : ZConnectionInfo
	{
		public ZSqlConnectionInfo(DbConnection dbConnection, string nonDefaultDatabaseName)
		{
			this.dbConnection = dbConnection;
			this.NonDefaultDatabaseName = nonDefaultDatabaseName ?? "";
		}

		public DbConnection DbConnection
		{
			get { return dbConnection ?? Db.Connection; }
		}

		readonly DbConnection dbConnection;

		public readonly string NonDefaultDatabaseName;

		public override string PathToTables
		{
			get
			{
				return string.IsNullOrEmpty(NonDefaultDatabaseName) ? "" : NonDefaultDatabaseName + ".dbo.";
			}
		}

		public DbCommand GetNewDbCommandForStoredProcedure(string query, params IZSqlParameter[] parameters)
		{
			return GetNewDbCommand(query, parameters, true, null, true);
		}

		public DbCommand GetNewDbCommandForSelect(string query, params IZSqlParameter[] parameters)
		{
			if (parameters?.Length > 0)
			{
				var comment = new SmartParameterisationCommentGenerator().Generate(parameters);
				if (comment.Length > 0)
				{
					query += comment;
				}
			}
			return GetNewDbCommand(query, parameters, true, null, false);
		}

		public DbCommand GetNewDbCommandForSelect(string query, int? cmdTimeoutInSeconds, params IZSqlParameter[] parameters)
		{
			return GetNewDbCommand(query, parameters, true, cmdTimeoutInSeconds, false);
		}

		public DbCommand GetNewDbCommandForUpdate(string query, params IZSqlParameter[] parameters)
		{
			return GetNewDbCommand(query, parameters, false, null, false);
		}

		DbCommand GetNewDbCommand(string query, IZSqlParameter[] parameters, bool isSelect, int? cmdTimeoutInSeconds, bool isStoredProc)
		{
			using (PerformanceStatisticsCollector.StartMonitoring("ZSqlConnectionInfo", "GetNewDbCommand"))
			{
				DbCommand cmd = DbConnection.Command(query, cmdTimeoutInSeconds);
				cmd.CommandType = isStoredProc ? System.Data.CommandType.StoredProcedure : System.Data.CommandType.Text;

				if (parameters != null)
				{
					foreach (IZSqlParameter param in parameters.Distinct())
					{
						if (param.IsTableValued)
						{
							cmd.AddTableValuedParameter(param.ParameterName, param.SchemaColumn, (IEnumerable)param.Value);
						}
						else
						{
							object value = param.ValueForSql;

							SchemaDecimalColumn decimalColumn = param.SchemaColumn as SchemaDecimalColumn;
							if (decimalColumn != null)
							{
								cmd.AddParameterBasedOnDbColumn(param.ParameterName, value, param.SchemaColumn);
							}
							else
							{
								if (param.SchemaColumn.ColumnType == SchemaColumnType.String)
								{
									cmd.AddParameterBasedOnDbColumn(param.ParameterName, value, param.SchemaColumn, isSelect);
								}
								else
								{
									cmd.AddParameter(param.ParameterName, param.SchemaColumn.SqlDbTypeForValue(value), value);
								}
							}
						}
					}
				}

				return cmd;
			}
		}
	}
}
