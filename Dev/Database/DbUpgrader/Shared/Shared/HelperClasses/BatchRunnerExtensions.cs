using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Foundation;

namespace Enterprise.DbUpgrader.Shared
{
	public static class BatchRunnerExtensions
	{
		public static void RunCommandsGeneratedByCommand(this BatchRunner runner, DbCommand cmd)
		{
			var tableOfCommands = DataUtils.GetDataTableFromCommand(cmd);
			var commands = tableOfCommands.Rows.Cast<DataRow>().Select(r =>
				new KeyValuePair<string, string>(r.Table.Columns.Count > 1 ? r[1].ToString() : null, r[0].ToString())
			);

			runner.RunCollectionOfSqlCommands(cmd.DbConnection, commands);
		}
	}
}
