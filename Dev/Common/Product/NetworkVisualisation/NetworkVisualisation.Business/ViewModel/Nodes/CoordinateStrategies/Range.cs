using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace CargoWise.NetworkVisualisation.Business
{
	[SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes")]
	public readonly struct Range<T> : IComparable<Range<T>>
		where T : struct, IConvertible, IComparable<T>
	{
		public Range(T start, T end)
		{
			Minimum = start;
			Maximum = end;
		}

		public T Minimum { get; }

		public T Maximum { get; }

		public override string ToString()
		{
			return string.Format(CultureInfo.InvariantCulture, "[{0} - {1}]", Minimum, Maximum);
		}

		public bool IsValid()
		{
			return Minimum.CompareTo(Maximum) <= 0;
		}

		public bool ContainsValue(T value)
		{
			return (Minimum.CompareTo(value) <= 0) && (value.CompareTo(Maximum) <= 0);
		}

		public bool IsInsideRange(Range<T> range)
		{
			return IsValid() && range.IsValid() && range.ContainsValue(Minimum) && range.ContainsValue(Maximum);
		}

		public bool ContainsRange(Range<T> range)
		{
			return IsValid() && range.IsValid() && ContainsValue(range.Minimum) && ContainsValue(range.Maximum);
		}

		int IComparable<Range<T>>.CompareTo(Range<T> other)
		{
			var maxEqual = Maximum.Equals(other.Maximum);
			if (maxEqual && Minimum.Equals(other.Minimum))
			{
				return 0;
			}
			else if (maxEqual)
			{
				return Minimum.CompareTo(other.Minimum);
			}
			else
			{
				return Maximum.CompareTo(other.Maximum);
			}
		}

		#region Equality

		public override bool Equals(object obj)
		{
			return obj is Range<T> other && this == other;
		}

		public override int GetHashCode()
		{
			return Minimum.GetHashCode() ^ Maximum.GetHashCode();
		}

		public static bool operator ==(Range<T> lhs, Range<T> rhs)
		{
			return lhs.Minimum.CompareTo(rhs.Minimum) == 0
				&& lhs.Maximum.CompareTo(rhs.Maximum) == 0;
		}

		public static bool operator !=(Range<T> lhs, Range<T> rhs)
		{
			return !(lhs == rhs);
		}

		#endregion
	}
}
