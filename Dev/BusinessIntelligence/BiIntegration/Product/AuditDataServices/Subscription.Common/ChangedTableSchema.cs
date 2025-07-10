using System;

namespace Enterprise.AuditDataServices.Subscription.Common
{
	public class ChangedTableSchema : IChangedTableSchema, IEquatable<ChangedTableSchema>
	{
		public ChangedTableSchema(string schemaName, string tableName, string pkName)
		{
			this.SchemaName = schemaName;
			this.TableName = tableName;
			this.PkName = pkName;
		}

		#region Properties and Fields

		public string SchemaName { get; }
		public string TableName { get; }
		public string PkName { get; }

		#endregion

		#region IEquatable

		public bool Equals(ChangedTableSchema other)
		{
			if (other is null)
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return string.Equals(SchemaName, other.SchemaName, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(TableName, other.TableName, StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ChangedTableSchema);
		}

		public override int GetHashCode()
		{
			var schemaNameHashCode = string.IsNullOrEmpty(SchemaName) ? 0 : SchemaName.Trim().GetHashCode();
			var tableNameHashCode = string.IsNullOrEmpty(TableName) ? 0 : TableName.Trim().GetHashCode();
			return schemaNameHashCode ^ tableNameHashCode;
		}

		#endregion
	}
}
