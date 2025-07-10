namespace CargoWise.EntityFramework
{
	public static class ZSqlParameterHelper
	{
		public static void CreateParameterWithNewNameAndAddToCollection(ZSqlParameter parameter, ZSqlParameterCollection collection, string newParameterName)
		{
			collection.Add(CreateParameterWithNewName(parameter, newParameterName));
		}

		static ZSqlParameter CreateParameterWithNewName(ZSqlParameter parameter, string newParameterName)
		{
			if (parameter.ValueForSql.ToString().Contains("%"))
			{
				return ZSqlParameter.New(newParameterName, parameter.ValueForSql, parameter.SchemaColumn, isTableValued: parameter.IsTableValued);
			}

			return ZSqlParameter.New(newParameterName, parameter.ValueForSql, parameter.SchemaColumn, parameter.ComparisonOperator, parameter.IsTableValued);
		}
	}
}
