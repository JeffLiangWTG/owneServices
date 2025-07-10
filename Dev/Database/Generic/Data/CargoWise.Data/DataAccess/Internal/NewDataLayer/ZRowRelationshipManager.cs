using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace CargoWise.EntityFramework
{
	public static class ZRowRelationshipManager
	{
		public static void RemoveNotNullConstraintsOnFKsAndDates(DataTable table)
		{
			foreach (DataColumn column in table.Columns)
			{
				if (!column.AllowDBNull &&
					(column.DataType == typeof(Guid) || column.DataType == typeof(DateTime) || column.DataType == typeof(DateTimeOffset)))
				{
					column.AllowDBNull = true;
				}
			}
		}

		public static string[] GetSelfReferentialFKsForTable(DataTable table)
		{
			List<string> result = new List<string>();
			foreach (string columnName in GetFKColumnNames(table))
			{
				string[] colNameParts = columnName.Split('_');

				if (colNameParts.Length >= 2 && colNameParts[0] == colNameParts[1])
				{
					result.Add(columnName);
				}
			}
			return result.ToArray();
		}

		public static IEnumerable<string> GetFKColumnNames(DataTable childTable)
		{
			DataColumn[] childColumns = (DataColumn[])new ArrayList(childTable.Columns).ToArray(typeof(DataColumn));
			string[] childColumnNames = Array.ConvertAll(childColumns, delegate(DataColumn column)
			{ return column.ColumnName; });
			return GetFKColumnNames(childColumnNames);
		}

		public static IEnumerable<string> GetFKColumnNames(IEnumerable<string> childColumnNames)
		{
			foreach (string fkColumnName in childColumnNames)
			{
				string fkPrefix = GetForeignKeyTablePrefix(fkColumnName);
				if (fkPrefix != null)
				{
					yield return fkColumnName;
				}
			}
		}

		public static string GetForeignKeyTablePrefix(string fkColumnName)
		{
			string fkPrefix = null;

			string[] colNameParts = fkColumnName.Split('_');

			if (colNameParts.Length >= 2 && colNameParts[1].Length <= 3 && colNameParts[1] != "PK")
			{
				fkPrefix = colNameParts[1];
			}

			return fkPrefix;
		}
	}
}
