using System;
using System.Collections.Generic;

namespace Enterprise.DataTransfer.Native.DB
{
	public class Constraint : IEquatable<Constraint>
	{
		public string Name { get; internal set; }
		public Table Table { get; internal set; }

		public IEnumerable<ColumnDef> Columns
		{
			get { return columns; }
		}

		readonly List<ColumnDef> columns = new List<ColumnDef>();

		public int ColumnSpan
		{
			get { return columns.Count; }
		}

		public bool Contains(ColumnDef columnDef)
		{
			return columns.Contains(columnDef);
		}

		public bool Contains(IEnumerable<ColumnDef> cols)
		{
			var result = true;
			foreach (var column in cols)
			{
				if (!columns.Contains(column))
				{
					result = false;
					break;
				}
			}
			return result;
		}

		public bool OverlapWith(IEnumerable<ColumnDef> cols)
		{
			var result = false;
			foreach (var column in cols)
			{
				if (columns.Contains(column))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public bool IsUnique { get; internal set; }

		public override string ToString()
		{
			return "IndexMetadata[" + Name + ']';
		}

		internal void AddColumn(ColumnDef columnDef)
		{
			if (columnDef == null || columns.Contains(columnDef))
			{
				return;
			}
			columns.Add(columnDef);
		}

		#region Equality

		public bool Equals(Constraint other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(other.Name, Name) && Equals(other.Table, Table) && other.IsUnique.Equals(IsUnique);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != typeof(Constraint))
			{
				return false;
			}

			return Equals((Constraint)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (Name != null ? Name.GetHashCode() : 0);
				result = (result * 397) ^ (Table != null ? Table.GetHashCode() : 0);
				result = (result * 397) ^ IsUnique.GetHashCode();
				return result;
			}
		}

		#endregion
	}
}
