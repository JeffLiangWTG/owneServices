namespace Enterprise.BusinessObjectGenerator.ModelView
{
	static class SqlHelper
	{
		internal static string GetColumnScript(AddInfo addInfo, string addInfoColumnName)
		{
			var len = addInfo.NormalizedName.Length + 2;
			var isNullable = addInfo.IsNullable && !addInfo.DataType.Contains("String");

			const string valueSeparator = "*";
			var sourceColumnExpr = $"'{valueSeparator}'+{addInfoColumnName}";
			var keyLocSearchExpr = $"CHARINDEX('{valueSeparator}{addInfo.NormalizedName}=', {sourceColumnExpr})";
			var keyStartLocExpr = $"{keyLocSearchExpr} + 1";
			var keyEndLocExpr = $"{keyLocSearchExpr} + {len}";

			var whenExpr = $"{keyLocSearchExpr} > 0";

			var nextKeyStartLocExpr = $"CHARINDEX('{valueSeparator}', {sourceColumnExpr}+'{valueSeparator}', {keyStartLocExpr})";
			var subStrLengthExpr = $"{nextKeyStartLocExpr} - ({keyEndLocExpr})";

			var subStrExpr = $"SUBSTRING({sourceColumnExpr}, {keyEndLocExpr}, {subStrLengthExpr})";
			var thenExpr = $"REPLACE({subStrExpr}, '¤', '*')";
			var elseExpr = isNullable ? "NULL" : "''";
			var caseExpr = $"CASE WHEN {whenExpr} THEN {thenExpr} ELSE {elseExpr} END";

			string addInfoScript;
			switch (addInfo.DataType)
			{
				case "Boolean":
					addInfoScript = $"CONVERT(BIT, CASE WHEN {caseExpr} = 'Y' THEN 1 ELSE 0 END)";
					break;
				case "Date":
					addInfoScript = $"TRY_CONVERT(DATE, {caseExpr}, 121)";
					break;
				case "DateTime":
					addInfoScript = $"TRY_CONVERT(DATETIME, {caseExpr}, 121)";
					break;
				case "Decimal":
					addInfoScript = $"TRY_CONVERT(DECIMAL({addInfo.Precision}, {addInfo.Scale}), {caseExpr})";
					break;
				case "Guid":
					addInfoScript = $"TRY_CONVERT(UNIQUEIDENTIFIER, {caseExpr})";
					break;
				case "Int16":
					addInfoScript = $"TRY_CONVERT(SMALLINT, {caseExpr})";
					break;
				case "Int32":
					addInfoScript = $"TRY_CONVERT(INT, {caseExpr})";
					break;
				default:
					if (addInfo.MaxLength > 0)
					{
						if (addInfo.IsUnicode)
						{
							addInfoScript = $"CONVERT(NVARCHAR({addInfo.MaxLength}), {caseExpr})";
						}
						else
						{
							addInfoScript = $"CONVERT(VARCHAR({addInfo.MaxLength}), {caseExpr})";
						}
					}
					else
					{
						addInfoScript = "addInfoCase";
					}
					break;
			}

			return addInfoScript;
		}

		internal static string GetColumnDefinition(AddInfo addInfo, string addInfoColumnName)
		{
			return $"{addInfo.Name} = {GetColumnScript(addInfo, addInfoColumnName)}";
		}
	}
}
