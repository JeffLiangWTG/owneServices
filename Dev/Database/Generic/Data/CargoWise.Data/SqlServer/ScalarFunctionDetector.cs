using CargoWise.Common;

namespace CargoWise.Data.SqlServer
{
	public class ScalarFunctionDetector
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public bool IsScalarFunction(DbCommand command, DbConnection connection)
		{
			Argument.NotNull(command, nameof(command));
			Argument.NotNull(connection, nameof(connection));

			var originalCommand = command.CommandText;
			connection.ExecuteNonQuery("SET SHOWPLAN_TEXT ON");// Inline SQL Statement
			try
			{
				command.CommandText = command.CommandText.Replace(DbCommand.ExecuteAsReaderFlagComments, "");
				var resultCount = 0;
				using (var reader = command.ExecuteReader())
				{
					while (reader.NextResult())
					{
						resultCount++;
					}
				}
				return resultCount > 1;
			}
			finally
			{
				command.CommandText = originalCommand;
				connection.ExecuteNonQuery("SET SHOWPLAN_TEXT OFF");// Inline SQL Statement
			}
		}
	}
}
