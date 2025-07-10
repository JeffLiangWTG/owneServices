using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.Types
{
	[TypeConverter(typeof(ZShortTypeConverter))]
	[ValueRange(short.MaxValue, short.MinValue), HasDecimals(false)]
	[DebuggerDisplay("{fValue}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZShort : INumericZType, IZTypeInternals, IFormattable
	{
		[DebuggerStepThrough]
		public ZShort(object value)
		{
			if (value == null)
			{
				this = Zero;
			}
			else if (value is short @int)
			{
				this = new ZShort(@int);
			}
			else
			{
				TypeConverter converter = ZShortTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					var converted = converter.ConvertFrom(value);
					this = (ZShort)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZShort), value);
				}
			}
		}

		[DebuggerStepThrough]
		public ZShort(short value)
		{
			fValue = value;
		}

		[DebuggerStepThrough]
		public ZShort(byte value)
		{
			fValue = value;
		}

		#region Object Overrides

		public override bool Equals(object obj)
		{
			return (obj is ZShort @short && @short == this) ||
					(obj is short @int && @int == fValue);
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

		#region Wrapping Int16 Functionality

		public string ToString(string format)
		{
			return fValue.ToString(format, ObjectCache.CultureProvider.Culture.NumberFormat);
		}

		public static ZShort Parse(string valueToParse)
		{
			Argument.NotNull(valueToParse, nameof(valueToParse));
			return (ZShort)short.Parse(valueToParse, ObjectCache.CultureProvider.Culture.NumberFormat);
		}

		#endregion

		#region Casting

		public static implicit operator ZShort(short value)
		{
			return new ZShort(value);
		}

		public static implicit operator short(ZShort value)
		{
			return value.fValue;
		}

		public static implicit operator int(ZShort value)
		{
			return value.fValue;
		}

		public static implicit operator decimal(ZShort value)
		{
			return value.fValue;
		}

		public static implicit operator ZShort(byte value)
		{
			return new ZShort(value);
		}

		[DebuggerStepThrough]
		public static explicit operator ZDecimal(ZShort value)
		{
			return new ZDecimal(value);
		}

		#endregion

		#region Operator Overloads

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(ZShort lhs, ZShort rhs)
		{
			return lhs.fValue < rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(ZShort lhs, ZShort rhs)
		{
			return lhs.fValue <= rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(ZShort lhs, ZShort rhs)
		{
			return lhs.fValue > rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(ZShort lhs, ZShort rhs)
		{
			return lhs.fValue >= rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZShort lhs, ZShort rhs)
		{
			return lhs.fValue != rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZShort lhs, ZShort rhs)
		{
			return lhs.fValue == rhs.fValue;
		}

		public static ZShort operator ++(ZShort value)
		{
			return new ZShort((short)(value.fValue + 1));
		}

		public static ZShort operator --(ZShort value)
		{
			return new ZShort((short)(value.fValue - 1));
		}

		public static ZShort operator -(ZShort valueInitial, ZShort valueToDecrement)
		{
			return (ZShort)(valueInitial.fValue - valueToDecrement.fValue);
		}

		public static ZShort operator +(ZShort valueInitial, ZShort valueToIncrement)
		{
			return (ZShort)(valueInitial.fValue + valueToIncrement.fValue);
		}

		#endregion

		#region TryParse

		/// <summary>
		/// Can the given string be converted to a short integer? If so, returns the converted integer, else zero.
		/// </summary>
		/// <param name="value">A string containing a number to convert.</param>
		/// <param name="result">Returned as the integer equivalent of the given string, else zero if it cannot be converted.</param>
		public static bool TryParse(string value, out ZShort result)
		{
			bool success =
				double.TryParse(value, NumberStyles.Integer, ObjectCache.CultureProvider.Culture.NumberFormat, out var tryParseResult)
				&& CanConvertToShort(tryParseResult);

			result = success ? (short)tryParseResult : (short)0;

			return success;
		}

		public static ZShort ParseSafe(ZString value, ZShort defaultValue)
		{
			return TryParse(value, out var parsed) ? parsed : defaultValue;
		}

		static bool CanConvertToShort(double value)
		{
			return (value >= short.MinValue && value <= short.MaxValue);
		}

		#endregion

		#region XmlSerializedValue

		[XmlText]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public short XmlSerializedValue
		{
			get { return this; }
			set { this = value; }
		}

		#endregion

		#region IZType Members

		public ZInt ToZInt()
		{
			return fValue;
		}

		[XmlIgnore]
		public ZDataType DataType
		{
			get { return ZDataType.Integer; }
		}

		[XmlIgnore]
		public Type BaseDataType
		{
			get { return typeof(short); }
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
			get { return this == (ZShort)Default; }
		}

		[XmlIgnore]
		public IZType Default
		{
			get { return new ZShort(); }
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
		public static readonly ZShort Zero = new ZShort((short)0);
		#endregion

		#region Implementation

		internal readonly short fValue;

		#endregion
	}
}
