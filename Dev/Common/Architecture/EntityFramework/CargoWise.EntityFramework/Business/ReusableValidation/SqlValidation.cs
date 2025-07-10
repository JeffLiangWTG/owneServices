using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework
{
	public static class SqlValidation
	{
		public static void CheckValidStatement(ZPropertyInfo propertyInfo)
		{
			AddErrors(propertyInfo, GetInvalidStatementErrors(propertyInfo));
		}

		public static void CheckValidStatement(ZPropertyInfo propertyInfo, ZString value, ZString valueToCheckColumns, params string[] columns)
		{
			AddErrors(propertyInfo, GetInvalidStatementErrors(value, valueToCheckColumns, columns));
		}

		public static void CheckValidStatementWithOptionalColumns(ZPropertyInfo propertyInfo, ZString value, ZString valueToCheckColumns, string[] requiredColumns, string[] optionalColumns)
		{
			AddErrors(propertyInfo, GetInvalidStatementErrorsWithOptionalColumns(value, valueToCheckColumns, requiredColumns, optionalColumns));
		}

		public static void CheckValidStatementWithOptionalColumns(ZPropertyInfo propertyInfo, DbCommand value, DbCommand valueToCheckColumns, string[] requiredColumns, string[] optionalColumns)
		{
			AddErrors(propertyInfo, GetInvalidStatementErrorsWithOptionalColumns(value, valueToCheckColumns, requiredColumns, optionalColumns));
		}

		static void AddErrors(ZPropertyInfo propertyInfo, IEnumerable<string> errors)
		{
			foreach (var error in errors)
			{
				propertyInfo.AddError(error);
			}
		}

		public static List<string> GetInvalidStatementErrors(ZPropertyInfo propertyInfo)
		{
			return GetInvalidStatementErrors((ZString)propertyInfo.Value, (ZString)propertyInfo.Value);
		}

		public static List<string> GetInvalidStatementErrors(ZString value, ZString valueToCheckColumns, params string[] columns)
		{
			return GetInvalidStatementErrorsWithOptionalColumns(value, valueToCheckColumns, columns, null);
		}

		public static List<string> GetInvalidStatementErrorsWithOptionalColumns(DbCommand value, DbCommand valueToCheckColumns, string[] requiredColumns, string[] optionalColumns)
		{
			return GetInvalidStatementErrorsWithOptionalColumnsCore((v) => DataUtils.GetDataSetFromQuery(v), value, valueToCheckColumns, requiredColumns, optionalColumns);
		}

		public static List<string> GetInvalidStatementErrorsWithOptionalColumns(ZString value, ZString valueToCheckColumns, string[] requiredColumns, string[] optionalColumns)
		{
			return GetInvalidStatementErrorsWithOptionalColumnsCore((v) => DataUtils.GetDataSetFromQuery(v), value, valueToCheckColumns, requiredColumns, optionalColumns);
		}

		static List<string> GetInvalidStatementErrorsWithOptionalColumnsCore<T>(Func<T, DataSet> datasetGetter, T value, T valueToCheckColumns, string[] requiredColumns, string[] optionalColumns)
		{
			var results = new List<string>();

			try
			{
				using (datasetGetter.Invoke(value))
				{
				}
			}
			catch (SqlException ex)
			{
				results.Add(GetSqlExceptionError(ex));
			}

			try
			{
				using (var dataSet = datasetGetter.Invoke(valueToCheckColumns))
				{
					if (dataSet.Tables.Count != 1)
					{
						results.Add(Res.GetString("6a124b54-f821-487c-a0e9-12cbf23e73cf", "The SQL statement must only have a single table result."));
					}
					else if (requiredColumns != null && requiredColumns.Length > 0)
					{
						var table = dataSet.Tables[0];
						var tableColumnCount = table.Columns.Count;
						var requiredColumnsCount = requiredColumns.Length;
						var optionalColumnCount = optionalColumns?.Length ?? 0;

						if (tableColumnCount < requiredColumnsCount || tableColumnCount > requiredColumnsCount + optionalColumnCount)
						{
							results.Add(GetNumberOfColumnsError(requiredColumns, optionalColumns));
						}
						else
						{
							var hasColumnNameErrorBeenAdded = false;

							for (var i = 0; i < tableColumnCount; i++)
							{
								var tableColumn = table.Columns[i].ColumnName;

								if (i < requiredColumnsCount)
								{
									if (!string.Equals(tableColumn, requiredColumns[i], StringComparison.OrdinalIgnoreCase))
									{
										results.Add(GetWrongColumnNameError(i + 1, requiredColumns[i]));
										hasColumnNameErrorBeenAdded = true;
									}
								}
								else if (!hasColumnNameErrorBeenAdded && optionalColumns != null && !optionalColumns.Contains(tableColumn, StringComparer.OrdinalIgnoreCase))
								{
									results.Add(GetNumberOfColumnsError(requiredColumns, optionalColumns));
									hasColumnNameErrorBeenAdded = true;
								}
							}
						}
					}
				}
			}
			catch (SqlException ex)
			{
				results.Add(GetSqlExceptionError(ex));
			}

			return results;
		}

		static string GetSqlExceptionError(SqlException ex)
		{
			return Res.GetString("f9ed5cc8-3560-4c6a-9c78-5632db2256ae", "Invalid SQL Statement\r\n{0}", ex.Message);
		}

		static string GetNumberOfColumnsError(string[] requiredColumns, string[] optionalColumns)
		{
			var requiredColumnCount = requiredColumns.Length;

			var error = requiredColumnCount > 1
				? Res.GetString("4068cce0-ab19-44e1-a7c5-4fd2d6b5f7ae", "The SQL statement must return {0} columns", requiredColumnCount)
				: Res.GetString("3106a877-42e4-498b-a701-332e18aab2db", "The SQL statement must return 1 column");

			error = string.Format(CultureInfo.InvariantCulture, "{0}: {1}", error, string.Join(", ", requiredColumns));

			if (optionalColumns != null && optionalColumns.Any())
			{
				var optionalColumnsError = Res.GetString("cc4788e8-c64b-4208-b47d-f745a13224cc", "The following columns are optional: {0}", string.Join(", ", optionalColumns));
				error = string.Format(CultureInfo.InvariantCulture, "{0}\r\n{1}", error, optionalColumnsError);
			}

			return error;
		}

		static string GetWrongColumnNameError(int columnPosition, string columnName)
		{
			return Res.GetString("8f3419c4-174c-47e9-a73a-ae8b805e4e08", "The column at position {0} must have the name {1}.", columnPosition, columnName);
		}
	}
}
