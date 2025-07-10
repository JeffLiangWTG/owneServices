using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.DataTransfer.Native.DB.Sql
{
	public class Criteria : MultipleValueCriteria, IEquatable<Criteria>
	{
		public virtual object Value { get; set; }

		public IColumnDef ColumnDefinition { get; set; }

		public override IEnumerable<object> Values
		{
			get { return Enumerable.Repeat(Value, 1); }
			set { throw new InvalidOperationException("Setting Values is not supported. Use class 'MultipleValueCriteria' instead."); } // This message is for developers.
		}

		public bool IsBinary { get; set; }

		#region Equality & Format

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "ToString result shouldn't be localised")]
		public override string ToString()
		{
			return string.Format("TableName: {0}, ColumnName: {1}, Value: {2}", TableName, ColumnName, Value);
		}

		public bool Equals(Criteria other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(other.TableName, TableName) && Equals(other.ColumnName, ColumnName) && Equals(other.Value, Value);
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

			if (obj.GetType() != typeof(Criteria))
			{
				return false;
			}

			return Equals((Criteria)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (TableName != null ? TableName.GetHashCode() : 0);
				result = (result * 397) ^ (ColumnName != null ? ColumnName.GetHashCode() : 0);
				result = (result * 397) ^ (Value != null ? Value.GetHashCode() : 0);
				return result;
			}
		}

		public static bool operator ==(Criteria left, Criteria right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Criteria left, Criteria right)
		{
			return !Equals(left, right);
		}
		#endregion
	}
}
