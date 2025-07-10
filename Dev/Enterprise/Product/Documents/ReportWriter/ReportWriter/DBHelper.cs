using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ReportWriter
{
	public static class DBHelper
	{
		public static bool ExistsFunction(string sqlFunction)
		{
			try
			{
				var sqlText = string.Format(Culture.Invariant, "IF OBJECT_ID('{0}') IS NOT NULL BEGIN SELECT 1 END ELSE SELECT 0;", sqlFunction);
				using (var command = Db.Connection.Command(sqlText))// We are not loading a business Object
				{
					return (int)command.ExecuteScalar() == 1;
				}
			}
			catch (SqlException)
			{
				return false;
			}
		}

		public static DataTable GetDataTable(string sqlFunctionName, bool isAFunction)
		{
			var parameters = GetFunctionParameters(sqlFunctionName, isAFunction);
			var sql = string.Format(Culture.Invariant, "SELECT TOP 1 * FROM {0}{1}", sqlFunctionName, parameters);
			using (var command = Db.Connection.Command(sql))// We are not loading a business Object
			{
				return DataUtils.GetDataTableFromCommand(command);
			}
		}

		static string GetFunctionParameters(string sqlFunctionName, bool isAFunction)
		{
			var result = "";
			if (isAFunction)
			{
				const string sql = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.PARAMETERS WHERE SPECIFIC_NAME = @specificName";
				using (var cmd = Db.Connection.Command(sql))// We are not loading a business Object
				{
					cmd.AddParameter("@specificName", SqlDbType.NVarChar, sqlFunctionName);
					var noOfParameters = (int)cmd.ExecuteScalar();
					result = "(" + string.Join(", ", Enumerable.Repeat("NULL", noOfParameters)) + ")";
				}
			}
			return result;
		}
	}
}
