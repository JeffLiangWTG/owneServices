using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.Types
{
	[TypeConverter(typeof(ZLongTypeConverter))]
	[ValueRange(long.MaxValue, long.MinValue), HasDecimals(false)]
	[DebuggerDisplay("{fValue}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZLong : INumericZType, IZTypeInternals, IFormattable
	{
		[DebuggerStepThrough]
		public ZLong(object value)
		{
			if (value == null)
			{
				this = Zero;
			}
			else if (value is long longValue)
			{
				this = new ZLong(longValue);
			}
			else
			{
				TypeConverter converter = ZLongTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					var converted = converter.ConvertFrom(value);
					this = (ZLong)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZLong), value);
				}
			}
		}

		[DebuggerStepThrough]
		public ZLong(long value)
		{
			fValue = value;
		}

		[DebuggerStepThrough]
		public ZLong(byte value)
		{
			fValue = value;
		}

		#region Object Overrides

		public override bool Equals(object obj)
		{
			return (obj is ZLong zLongObj && zLongObj == this) ||
					(obj is long longObj && longObj == fValue);
		}

		public override int GetHashCode()
		{
			return fValue.GetHashCode();
		}

		public override string ToString()
		{
			return fValue.ToString(ObjectCache.CultureProvider.Culture.NumberFormat);
		}

		#endregion

		#region Wrapping Int64 Functionality

		public string ToString(string format)
		{
			return fValue.ToString(format, ObjectCache.CultureProvider.Culture.NumberFormat);
		}

		public static ZLong Parse(string valueToParse)
		{
			Argument.NotNull(valueToParse, nameof(valueToParse));
			return long.Parse(valueToParse, ObjectCache.CultureProvider.Culture.NumberFormat);
		}

		#endregion

		#region Casting

		public static implicit operator ZLong(long value)
		{
			return new ZLong(value);
		}

		public static implicit operator long(ZLong value)
		{
			return value.fValue;
		}

		public static implicit operator decimal(ZLong value)
		{
			return value.fValue;
		}

		public static implicit operator ZLong(byte value)
		{
			return new ZLong(value);
		}

		[DebuggerStepThrough]
		public static implicit operator ZLong(ZShort value)
		{
			return new ZLong(Convert.ToInt32(value));
		}

		[DebuggerStepThrough]
		public static explicit operator ZShort(ZLong value)
		{
			return new ZShort(Convert.ToInt16(value.fValue));
		}

		[DebuggerStepThrough]
		public static explicit operator ZDecimal(ZLong value)
		{
			return new ZDecimal(value);
		}

		#endregion

		#region Operator Overloads

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(ZLong lhs, ZLong rhs)
		{
			return lhs.fValue < rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(ZLong lhs, ZLong rhs)
		{
			return lhs.fValue <= rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(ZLong lhs, ZLong rhs)
		{
			return lhs.fValue > rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(ZLong lhs, ZLong rhs)
		{
			return lhs.fValue >= rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZLong lhs, ZLong rhs)
		{
			return lhs.fValue != rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZLong lhs, ZLong rhs)
		{
			return lhs.fValue == rhs.fValue;
		}

		public static ZLong operator ++(ZLong value)
		{
			return new ZLong(value.fValue + 1);
		}

		public static ZLong operator --(ZLong value)
		{
			return new ZLong(value.fValue - 1);
		}

		public static ZLong operator -(ZLong valueInitial, ZLong valueToDecrement)
		{
			return valueInitial.fValue - valueToDecrement.fValue;
		}

		public static ZLong operator +(ZLong valueInitial, ZLong valueToIncrement)
		{
			return valueInitial.fValue + valueToIncrement.fValue;
		}

		public static ZLong Add(ZLong valueInitial, ZLong valueToAdd)
		{
			return valueInitial + valueToAdd;
		}

		public static ZLong Subtract(ZLong valueInitial, ZLong valueToSubtract)
		{
			return valueInitial - valueToSubtract;
		}

		public static ZLong Increment(ZLong valueInitial)
		{
			return valueInitial++;
		}

		public static ZLong Decrement(ZLong valueInitial)
		{
			return valueInitial--;
		}

		#endregion

		#region TryParse

		/// <summary>
		/// Can the given string be converted to a long? If so, returns the converted long, else zero.
		/// </summary>
		/// <param name="value">A string containing a number to convert.</param>
		/// <param name="result">Returned as the long equivalent of the given string, else zero if it cannot be converted.</param>
		public static bool TryParse(string value, out ZLong result)
		{
			bool success = long.TryParse(value, out var tryParseResult);
			result = success ? tryParseResult : 0;
			return success;
		}

		public static ZLong ParseSafe(ZString value, ZLong defaultValue)
		{
			return TryParse(value, out var parsed) ? parsed : defaultValue;
		}

		#endregion

		#region XmlSerializedValue

		[XmlText]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public long XmlSerializedValue
		{
			get { return this; }
			set { this = value; }
		}

		#endregion

		#region IZType Members

		public ZInt ToZInt()
		{
			return (int)fValue;
		}

		[XmlIgnore]
		public ZDataType DataType
		{
			get { return ZDataType.Integer; }
		}

		[XmlIgnore]
		public Type BaseDataType
		{
			get { return typeof(long); }
		}

		[XmlIgnore]
		public bool IsEmpty
		{
			get { return fValue == 0; }
		}

		[XmlIgnore]
		public bool IsValid
		{
			get { return true; }
		}

		[XmlIgnore]
		public bool IsDefault
		{
			get { return this == (ZLong)Default; }
		}

		[XmlIgnore]
		public IZType Default
		{
			get { return new ZLong(); }
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (obj is IZTypeInternals zTypeInternals)
			{
				obj = zTypeInternals.GetValueForLogicalDataLayer(false);
			}

			return obj is DBNull && IsEmpty ? 0 : fValue.CompareTo(obj);
		}

		#endregion

		#region IFormattable Members

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return fValue.ToString(format, formatProvider);
		}

		#endregion

		#region IZTypeInternals Members

		object IZTypeInternals.GetValueForLogicalDataLayer(bool isNullable)
		{
			return IsEmpty && isNullable ? DBNull.Value : fValue;
		}

		#endregion

		#region Zero
		public static readonly ZLong Zero = new ZLong((long)0);

		#endregion

		#region Implementation

		internal readonly long fValue;

		#endregion
	}
}
