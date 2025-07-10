using System;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework
{
	public class ZCountDataQuery
	{
		public ZCountDataQuery(ZConnectionInfo connectionInfo, string tableName, ZQuery filter)
		{
			if (connectionInfo == null)
			{
				throw new ArgumentNullException(nameof(connectionInfo));
			}

			if (tableName == null)
			{
				throw new ArgumentNullException(nameof(tableName));
			}

			if (filter == null)
			{
				throw new ArgumentNullException(nameof(filter));
			}

			ConnectionInfo = connectionInfo;
			TableName = tableName;
			Filter = filter;
		}

		public string ParameterisedQueryText
		{
			get
			{
				string result = "SELECT COUNT(*) FROM ";
				if (Filter.IsUnionQuery)
				{
					var innerText = Filter.GetAsCompleteSQLStatement(TableName, false);
					result += "(" + innerText + (NoResString)") AS MyUnion";
				}
				else
				{
					result += SchemaPrepender.AddSchemaName(ConnectionInfo.PathToTables + TableName);

					var parameterisedQueryText = Filter.ParameterisedText.ParameterisedQueryText;

					if (parameterisedQueryText.Length > 0)
					{
						result += " WHERE " + parameterisedQueryText;
					}
				}
				return result;
			}
		}

		public ZSqlParameter[] Parameters
		{
			get
			{
				return Filter.Params;
			}
		}

		public readonly ZConnectionInfo ConnectionInfo;
		public readonly string TableName;
		public readonly ZQuery Filter;
	}
}
