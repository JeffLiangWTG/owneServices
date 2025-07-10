using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.Common;

namespace CargoWise.Data
{
	public static partial class MetaData
	{
		public static string GetTableNameFromErrorMessage(string errorMessage)
		{
			Argument.NotNull(errorMessage, nameof(errorMessage));

			string tablePattern = @"(?<=^(The\s+)?(INSERT|UPDATE|DELETE)\b.+?\b(TABLE)\b[^'""]+['""](\w+\.)?)#?\w+(?=['""])";
			Regex tableRegex = new Regex(tablePattern, RegexOptions.IgnoreCase);

			return tableRegex.Match(errorMessage).Value;
		}

		public static string GetCheckConstraintNameFromErrorMessage(string errorMessage)
		{
			Argument.NotNull(errorMessage, nameof(errorMessage));

			string result = ConstraintRegex.Match(errorMessage).Value;

			if (string.IsNullOrEmpty(result))
			{
				throw new ArgumentException("Cannot get check constraint name, not valid check constraint violation error message.\r\n" + errorMessage);
			}

			return result;
		}

		public static string GetColumnNameFromErrorMessage(string errorMessage)
		{
			Argument.NotNull(errorMessage, nameof(errorMessage));

			var pattern = @"column '([^']*)'"; // regex expression
			var columnRegex = new Regex(pattern, RegexOptions.IgnoreCase);

			return columnRegex.Match(errorMessage).Groups[1].Value;
		}

		public static string GetDuplicatedValueFromErrorMessage(string errorMessage)
		{
			Argument.NotNull(errorMessage, nameof(errorMessage));

			var pattern = @"duplicate key value is \((.*)\)"; // regex expression
			var duplicatedValueRegex = new Regex(pattern, RegexOptions.IgnoreCase);

			return duplicatedValueRegex.Match(errorMessage).Groups[1].Value;
		}

		public static string[] GetColumnNamesByIndexNameAndTablePrefix(string errorMessage, string tablePrefix)
		{
			Argument.NotNull(errorMessage, nameof(errorMessage));
			var indexName = GetUniqueIndexNameFromErrorMessage(errorMessage);

			var partialColumnNames = indexName.Split([tablePrefix], StringSplitOptions.None);
			string[] result = [];
			if (partialColumnNames.Length > 1)
			{
				var resultList = new List<string>();
				for (var i = 1; i < partialColumnNames.Length; i++)
				{
					resultList.Add(tablePrefix + partialColumnNames[i].TrimEnd('_'));
				}

				result = [..resultList];
			}

			return result;
		}

		public static string GetUniqueIndexNameFromErrorMessage(string errorMessage)
		{
			Argument.NotNull(errorMessage, nameof(errorMessage));

			var uniqueIndexMatch = UniqueIndexRegex.Match(errorMessage);

			if (!uniqueIndexMatch.Success)
			{
				throw new ArgumentException(errorMessage + " does not contain any of the accepted unique index prefixes");
			}

			return uniqueIndexMatch.Value;
		}

		public static string GetFKNameFromErrorMessage(string errorMessage)
		{
			Argument.NotNull(errorMessage, nameof(errorMessage));

			string fkViolationPattern = @"(?<=^(The\s+)?(INSERT|UPDATE|DELETE)\b.+?\b(FOREIGN KEY|REFERENCE)\b[^'""]+['""])\w+(?=['""])"; // regex expression

			Regex fkRegex = new Regex(fkViolationPattern, RegexOptions.IgnoreCase);
			string result = fkRegex.Match(errorMessage).Value;

			if (string.IsNullOrEmpty(result))
			{
				throw new ArgumentException("Cannot get FK name, not a valid FK violation error message.\r\n" + errorMessage);
			}

			return result;
		}

		public static string[] GetParentAndChildTablesFromFK(DbConnection connection, string fkName)
		{
			Argument.NotNull(connection, nameof(connection));
			Argument.NotNullOrEmpty(fkName, nameof(fkName));

			string sqlText = @"
				SELECT
					OBJECT_NAME(referenced_object_id) ParentTable,
					OBJECT_NAME(parent_object_id) ChildTable
				FROM sys.foreign_Keys
				WHERE name = '" + fkName + "'";

			var fkInfo = DataUtils.GetDataTableFromQuery(connection, sqlText);

			if (fkInfo.Rows == null || fkInfo.Rows.Count == 0)
			{
				return null;
			}

			var row = fkInfo.Rows[0];

			var parentTableObj = row["ParentTable"];
			var childTableObj = row["ChildTable"];

			var parentTable = parentTableObj.ToString();
			var childTable = childTableObj.ToString();

			return new string[2] { parentTable, childTable };
		}

		#region Implementation

		static readonly Regex UniqueIndexRegex = new Regex(@"\b((NR|FK|PK)_U[XC]_|PK_)\w+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
		static readonly Regex ConstraintRegex = new Regex(@"(?<=^(The\s+)?(INSERT|UPDATE|DELETE)\b.+?\b(CHECK CONSTRAINT)\b[^'""]+['""])\w+(?=['""])", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		#endregion
	}
}
