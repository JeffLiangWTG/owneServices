using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Enterprise.DataTransfer.Native.DB
{
	public class ReadOnlyConstraint : IEquatable<ReadOnlyConstraint>
	{
		public string Name { get; }

		public string TableName { get; }

		public IReadOnlyList<ColumnDef> Columns { get; }

		public int ColumnSpan
		{
			get { return Columns.Count; }
		}

		ReadOnlyConstraint(string name, string tableName, IReadOnlyList<ColumnDef> columns)
		{
			Name = name;
			TableName = tableName;
			Columns = columns;
		}

		public static ReadOnlyConstraint Create(Constraint constraint)
		{
			return new ReadOnlyConstraint(constraint.Name, constraint.Table.Name, constraint.Columns.ToImmutableList());
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ReadOnlyConstraint);
		}

		public bool Equals(ReadOnlyConstraint other)
		{
			return other is not null &&
				   Name == other.Name &&
				   TableName == other.TableName &&
				   EqualityComparer<IEnumerable<ColumnDef>>.Default.Equals(Columns, other.Columns);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 1255355134;
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
				hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TableName);
				hashCode = hashCode * -1521134295 + EqualityComparer<IEnumerable<ColumnDef>>.Default.GetHashCode(Columns);
				return hashCode;
			}
		}
	}
}
