using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DataTransfer.Native.DB.Helpers
{
	public static class ColumnNameHelper
	{
		public static string RemovePrefix(string columnName)
		{
			var result = columnName;

			if (result.StartsWith("_"))
			{
				result = result.TrimStart('_');
			}

			var colNameParts = result.Split('_');
			int prefixesToSkip = 0;

			for (int i = 0; i < Math.Min(colNameParts.Length - 1, 2); i++)
			{
				if (colNameParts[i].Length == 2 || colNameParts[i].Length == 3)
				{
					prefixesToSkip++;
				}
			}

			result = String.Join("_", colNameParts.Skip(prefixesToSkip));
			return result;
		}

		public static string GetParentTablePrefix(string columnName)
		{
			string[] colNameParts = columnName.Split('_');
			return (colNameParts.Length > 1) ? colNameParts[1] : "";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "static list of strings")]
		public static string TransformNumberToString(string columnName)
		{
			if (columnName.Length <= 0)
			{
				return string.Empty;
			}
			// TODO: Need Refactoring
			var dict = new Dictionary<int, string> { { 1, "One" }, { 2, "Two" }, { 3, "Three" }, { 4, "Four" }, { 5, "Five" }, { 6, "Six" }, { 7, "Seven" }, { 8, "Eight" }, { 9, "Nine" } };
			var firstChar = columnName.Substring(0, 1);
			var leftString = columnName.Substring(1);
			int key;
			if (int.TryParse(firstChar, out key))
			{
				firstChar = dict[key];
			}
			return string.Concat(firstChar, leftString);
		}
	}
}
