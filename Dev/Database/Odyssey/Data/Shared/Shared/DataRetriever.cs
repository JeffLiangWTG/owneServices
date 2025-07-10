using System;
using System.Data;
using System.Linq;
using CargoWise.Data;

namespace Enterprise.DbUpgrader.Data
{
	public static class DataRetriever
	{
		public static void LoadDataFromDatabase(DataSet initialDataSet, string query, string[] tableNames)
		{
			int numberOfTableInDataSet = initialDataSet.Tables.Count;

			try
			{
				using (var adapter = Db.Connection.Command(query).NewDataAdapter())
				{
					adapter.Fill(initialDataSet);
				}
			}
			catch (SqlException ex)
			{
				throw new Exception("Reading data failed. See inner exception for more details.\r\nSQL Command : " + query, ex);
			}

			#if DEBUG

			var expectedTableCount = numberOfTableInDataSet + tableNames.Length;
			var actualTableCount = initialDataSet.Tables.Count;
			if (actualTableCount < expectedTableCount)
			{
				throw new Exception(
					$"Dataset contains {actualTableCount - numberOfTableInDataSet} new table(s), but {tableNames.Length} were expected.\n" +
					$"Initial dataset count: {numberOfTableInDataSet}, Expected total: {expectedTableCount}, Actual total: {actualTableCount}.\n" +
					$"Provided query: {query}.\n" +
					$"Provided table names: {string.Join(", ", tableNames)}.");
			}

			#endif

			for (int index = 0; index < tableNames.Length; index++)
			{
				var dataTable = initialDataSet.Tables[index + numberOfTableInDataSet];
				dataTable.TableName = tableNames[index];
				dataTable.PrimaryKey = new[] { dataTable.Columns[0] };

				// First column is PK - leave it in position, order all others alphabetically
				Func<DataColumn, string> keySelector = (column) => { return column.Ordinal == 0 ? "__AARDVARK" : column.ColumnName; };

				int columnNo = 0;
				var orderedColumns = dataTable.Columns.Cast<DataColumn>().OrderBy(keySelector).ToArray();
				foreach (var column in orderedColumns)
				{
					column.SetOrdinal(columnNo++);
				}

				// This must be SQLGuid - to keep ordering same as that returned by SQL.
				var sqlGuids = dataTable.Rows.Cast<DataRow>().Select(row => new { sqlGuid = new SqlGuid((Guid)row[0]), rowNumber = dataTable.Rows.IndexOf(row) }).OrderBy(x => x.sqlGuid);

				int rowNumber = 0;
				foreach (var guid in sqlGuids)
				{
					if (guid.rowNumber != rowNumber++)
					{
						throw new Exception("'" + dataTable.TableName + "' is not ordered correctly - this will cause random data changes and make comparison impossible");
					}
				}
			}
		}
	}
}
