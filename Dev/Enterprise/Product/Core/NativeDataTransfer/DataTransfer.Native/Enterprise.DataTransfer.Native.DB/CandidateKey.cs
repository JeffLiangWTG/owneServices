using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Enterprise.DataTransfer.Native.DB
{
	public class CandidateKey
	{
		public CandidateKey(ReadOnlyConstraint constraint, DataRow row)
		{
			this.row = row;
			this.constraint = constraint;
		}
		readonly DataRow row;
		readonly ReadOnlyConstraint constraint;

		public IEnumerable<DataCell> Values
		{
			get
			{
				foreach (var column in constraint.Columns)
				{
					yield return new DataCell(column) { Value = row[column.Name] };
				}
			}
		}

		public int Span
		{
			get { return constraint.ColumnSpan; }
		}

		#region Equals & ToString

		public override string ToString()
		{
			var builder = new StringBuilder();
			builder.Append("[");
			foreach (var property in Values)
			{
				builder.Append(property.ToString());
			}
			builder.Append("]");
			return builder.ToString();
		}

		public bool Equals(CandidateKey other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			if (Span != other.Span)
			{
				return false;
			}

			foreach (var property in Values)
			{
				if (!other.Values.Contains(property))
				{
					return false;
				}
			}

			return true;
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

			if (obj.GetType() != typeof(CandidateKey))
			{
				return false;
			}

			return Equals((CandidateKey)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 0;
				foreach (var property in Values)
				{
					hashCode ^= property.GetHashCode();
				}
				return hashCode;
			}
		}

		#endregion
	}
}
