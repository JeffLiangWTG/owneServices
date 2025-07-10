using System;
using System.Collections.Generic;

namespace Enterprise.DbUpgrader.Schema
{
	public readonly struct SchemaTableNamePair
	{
		public SchemaTableNamePair(string schemaName, string tableName)
		{
			SchemaName = schemaName;
			TableName = tableName;
		}

		public string SchemaName { get; }
		public string TableName { get; }

		public class IgnoreCaseComparer : IEqualityComparer<SchemaTableNamePair>
		{
			public bool Equals(SchemaTableNamePair x, SchemaTableNamePair y)
			{
				return x.SchemaName.Equals(y.SchemaName, StringComparison.OrdinalIgnoreCase)
					&& x.TableName.Equals(y.TableName, StringComparison.OrdinalIgnoreCase);
			}

			public int GetHashCode(SchemaTableNamePair pair)
				=> StringComparer.OrdinalIgnoreCase.GetHashCode(pair.SchemaName)
				 ^ StringComparer.OrdinalIgnoreCase.GetHashCode(pair.TableName);
		}
	}
}
