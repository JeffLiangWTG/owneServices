using System;
using System.Data;
using System.Globalization;
using Microsoft.SqlServer.Types;

namespace CargoWise.Data
{
	static class DataParametersExtensions
	{
		public static string GetFormattedDbType(this IDataParameter parameter)
		{
			if (parameter == null)
			{
				return null;
			}

			var sqlParameter = new SqlParameterWrapper(parameter);

			if (sqlParameter.InvalidAsThisIsNotSqlParameter)
			{
				return null;
			}

			if (sqlParameter.SqlDbType == SqlDbType.Money || sqlParameter.SqlDbType == SqlDbType.SmallMoney)
			{ }
			else if (sqlParameter.Precision != 0)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0} ({1}, {2})", sqlParameter.SqlDbType, sqlParameter.Precision, sqlParameter.Scale);
			}
			else if (sqlParameter.Size != 0)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0} ({1})", sqlParameter.SqlDbType, sqlParameter.Size);
			}

			return sqlParameter.SqlDbType.ToString();
		}

		public static string GetFormattedValue(this IDataParameter parameter)
		{
			if (parameter == null)
			{
				return null;
			}

			if (parameter.Value == null || Convert.IsDBNull(parameter.Value))
			{
				return "null";
			}

			var sqlParameter = new SqlParameterWrapper(parameter);
			if (sqlParameter.InvalidAsThisIsNotSqlParameter)
			{
				return null;
			}

			switch (sqlParameter.SqlDbType)
			{
				case SqlDbType.Char:
				case SqlDbType.NChar:
				case SqlDbType.NText:
				case SqlDbType.Text:
				case SqlDbType.VarChar:
				case SqlDbType.Xml:
				case SqlDbType.Time:
				case SqlDbType.UniqueIdentifier:
					return string.Format("'{0}'", parameter.Value.ToString().Replace("'", "''"));
				case SqlDbType.NVarChar:
					{
						if (parameter.Value is SqlGeography geo) //note - if we refactor SqlGeography to map to Udt instead of NVarChar, have to change this code
						{
							string text = geo.AsTextZM().ToSqlString().ToString();
							return string.Format("'{0}'", text.Replace("'", "''"));
						}
						return string.Format("'{0}'", parameter.Value.ToString().Replace("'", "''"));
					}
				case SqlDbType.Bit:
					return Convert.ToInt16(parameter.Value).ToString();
				case SqlDbType.Date:
				case SqlDbType.DateTime:
				case SqlDbType.DateTime2:
				case SqlDbType.SmallDateTime:
					return string.Format("'{0}'", ((DateTime)parameter.Value).ToString(CultureInfo.InvariantCulture));
				case SqlDbType.DateTimeOffset:
					return string.Format("'{0}'", ((DateTimeOffset)parameter.Value).ToString(CultureInfo.InvariantCulture));
				default:
					return parameter.Value.ToString().Replace("'", "''");
			}
		}
	}
}
