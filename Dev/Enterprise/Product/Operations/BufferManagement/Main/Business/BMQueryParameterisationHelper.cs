using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public static class BMQueryParameterisationHelper
	{
		public static string ReplaceAutoGenParamNamesWithCustomParamNames(string stringWithParameters, string customParameterName)
		{
			var stringToModify = stringWithParameters;
			if (stringToModify.Contains(ParameterNameFactory.ParameterPrefix))
			{
				stringToModify = stringToModify.Replace(ParameterNameFactory.ParameterPrefix, customParameterName);
			}

			return stringToModify;
		}

		public static QueryTextAndParameters ReplaceAutoParamNames(QueryTextAndParameters textAndOldParameters, string customParameterName)
		{
			var sqlParameters = new ZSqlParameterCollection();

			foreach (ZSqlParameter param in textAndOldParameters.Parameters)
			{
				if (param.IsTableValued)
				{
					sqlParameters.Add((ZSqlParameter)param.DeepClone());
				}
				else
				{
					var newParamName = ReplaceAutoGenParamNamesWithCustomParamNames(param.ParameterName, customParameterName);
					var newValue = param.ValueForSql.ToString().Contains("%") ? param.ValueForSql : param.Value;
					sqlParameters.Add(newParamName, newValue, param.SchemaColumn);
				}
			}

			var sql = ReplaceAutoGenParamNamesWithCustomParamNames(textAndOldParameters.QueryText, customParameterName);

			return new QueryTextAndParameters(sql, sqlParameters);
		}

		public static string GetNextUniqueParameterName(string nameBase, ZSqlParameterCollection parameters)
		{
			var name = nameBase;
			var nameSuffix = 1;

			while (parameters.ContainsKey(name))
			{
				name = nameBase + nameSuffix++;
			}

			return name;
		}
	}
}
