using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.Schema;
using Microsoft.SqlServer.Types;

namespace Enterprise.Build.Database.Script.Public.HRM
{
	abstract class ManagedTypeTest<T> : DbCreateScriptTest
		where T : CargoWise.Schema.Schema, ITableSchema
	{
		protected ITableSchema Schema => (T)typeof(T).GetField("Instance").GetValue(null);

		protected abstract string StaffFKColumnName { get; }

		protected void AssertRowValues(DataRow expectedRow, DataRow actualRow, List<string> colNames)
		{
			foreach (var colName in colNames)
			{
				if (expectedRow[colName] == DBNull.Value)
				{
					AssertEquals(colName, expectedRow[colName], actualRow[colName]);
				}
				else if (expectedRow.Table.Columns[colName].DataType == typeof(SqlGeography))
				{
					var expectedSqlGeography = (SqlGeography)expectedRow[colName];
					var actualSqlGeography = actualRow[colName];

					AssertNotNull($"{colName} expectedSqlGeography", expectedSqlGeography);
					AssertNotNull($"{colName} actualSqlGeography", actualSqlGeography);
					AssertEquals(colName, expectedSqlGeography.ToString(), actualSqlGeography);
				}
				else if (expectedRow.Table.Columns[colName].DataType == typeof(byte[]))
				{
					var expectedBytes = (byte[])expectedRow[colName];
					var actualBytes = (byte[])actualRow[colName];

					AssertNotNull($"{colName} expectedBytes", expectedBytes);
					AssertNotNull($"{colName} actualBytes", actualBytes);
					AssertSequencesEqual(colName, expectedBytes, actualBytes);
				}
				else
				{
					AssertEquals(colName, expectedRow[colName], actualRow[colName]);
				}
			}
		}

		protected DataTable Execute(Guid loggedInStaff, string managerTypes, DateTimeOffset effectiveAsAt)
		{
			var schema = (T)typeof(T).GetField("Instance").GetValue(null);

			using (var command = TestConnection.Command($"SELECT * FROM {schema.SqlSchemaName}.{schema.TableName}_WithManaged(@loggedInStaff, @managerTypes, @effectiveAsAt)"))
			{
				command.AddParameter("@loggedInStaff", SqlDbType.UniqueIdentifier, loggedInStaff);
				command.AddParameter("@managerTypes", SqlDbType.VarChar, managerTypes);
				command.AddParameter("@effectiveAsAt", SqlDbType.DateTimeOffset, effectiveAsAt);

				return DataUtils.GetDataTableFromCommand(command);
			}
		}
	}
}
