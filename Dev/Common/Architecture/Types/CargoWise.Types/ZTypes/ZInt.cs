using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;

namespace CargoWise.Types
{
	/// <summary>
	/// The Z verion of Int32.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes"), TypeConverter(typeof(ZIntTypeConverter))]
	[ValueRange(int.MaxValue, int.MinValue), HasDecimals(false)]
	[DebuggerDisplay("{fValue}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZInt : INumericZType, IZTypeInternals, IFormattable
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="value">Must be an int, short, byte, ZInt, ZShort or ZByte.</param>
		[DebuggerStepThrough]
		public ZInt(object value)
		{
			if (value == null)
			{
				this = Zero;
			}
			else if (value is int @int)
			{
				this = new ZInt(@int);
			}
			else
			{
				TypeConverter converter = ZIntTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					var converted = converter.ConvertFrom(value);
					this = (ZInt)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZInt), value);
				}
			}
		}

		[DebuggerStepThrough]
		public ZInt(int value)
		{
			fValue = value;
		}

		/// <summary>
		/// Is this ZInt within a given range?
		/// </summary>
		public bool IsInRange(ZInt lowValue, ZInt highValue)
		{
			return (this >= lowValue && this <= highValue);
		}

		#region Object Overrides

		public override bool Equals(object obj)
		{
			return (obj is ZInt @int && @int == this) ||
					(obj is int int1 && int1 == fValue);
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

		#region Wrapping Int32 Functionality

		public string ToString(string format)
		{
			return fValue.ToString(format, ObjectCache.CultureProvider.Culture.NumberFormat);
		}

		#endregion

		#region Casting

		[DebuggerStepThrough]
		public static implicit operator ZInt(int value)
		{
			return new ZInt(value);
		}

		[DebuggerStepThrough]
		public static implicit operator int(ZInt value)
		{
			return value.fValue;
		}

		[DebuggerStepThrough]
		public static implicit operator ZInt(byte value)
		{
			return new ZInt(Convert.ToInt32(value));
		}

		[DebuggerStepThrough]
		public static implicit operator ZInt(ZShort value)
		{
			return new ZInt(Convert.ToInt32(value));
		}

		[DebuggerStepThrough]
		public static explicit operator ZShort(ZInt value)
		{
			return new ZShort(Convert.ToInt16(value.fValue));
		}

		[DebuggerStepThrough]
		public static explicit operator ZDecimal(ZInt value)
		{
			return new ZDecimal(value);
		}

		#endregion

		#region Operator Overloads

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZInt lhs, ZInt rhs)
		{
			return lhs.fValue != rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZInt lhs, ZInt rhs)
		{
			return lhs.fValue == rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(ZInt lhs, ZInt rhs)
		{
			return lhs.fValue > rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(ZInt lhs, ZInt rhs)
		{
			return lhs.fValue >= rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(ZInt lhs, ZInt rhs)
		{
			return lhs.fValue < rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(ZInt lhs, ZInt rhs)
		{
			return lhs.fValue <= rhs.fValue;
		}

		[DebuggerStepThrough]
		public static ZInt operator ++(ZInt value)
		{
			return new ZInt(value.fValue + 1);
		}

		[DebuggerStepThrough]
		public static ZInt operator --(ZInt value)
		{
			return new ZInt(value.fValue - 1);
		}

		#endregion

		#region Parse / CanParse / TryParse

		public static ZInt ParseEmptyAsZero(ZString value)
		{
			return value.IsEmpty ? Zero : Parse(value);
		}

		/// <summary>
		/// Can the given string be converted to an integer?
		/// </summary>
		/// <param name="value">A string containing a number to convert.</param>
		public static bool CanParse(string value)
		{
			return TryParse(value, out var tempInt);
		}

		/// <summary>
		/// The integer equivalent of the given string.
		/// </summary>
		/// <param name="value">A string containing a number to convert.</param>
		public static ZInt Parse(string value)
		{
			if (!TryParse(value, out var result))
			{
				throw new ArgumentOutOfRangeException(nameof(value));
			}

			return result;
		}

		/// <summary>
		/// Can the given string be converted to an integer? If so, returns the converted integer, else zero.
		/// </summary>
		/// <param name="value">A string containing a number to convert.</param>
		/// <param name="result">Returned as the integer equivalent of the given string, else zero if it cannot be converted.</param>
		public static bool TryParse(string value, out ZInt result)
		{
			bool success =
				double.TryParse(value, NumberStyles.Integer, ObjectCache.CultureProvider.Culture.NumberFormat, out var tryParseResult)
				&& CanConvertToInt(tryParseResult);

			result = success ? (int)tryParseResult : 0;

			return success;
		}

		public static ZInt ParseSafe(ZString value, ZInt defaultValue)
		{
			return TryParse(value, out var parsed) ? parsed : defaultValue;
		}

		static bool CanConvertToInt(double value)
		{
			return (value >= int.MinValue && value <= int.MaxValue);
		}

		#endregion

		#region Date/time Offset Conversion

		public ZDateTime GetDateTimeFromMinutes() => TimeSpan.FromMinutes(this);

		#endregion

		#region XmlSerializedValue

		[XmlText]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public int XmlSerializedValue
		{
			get { return this; }
			set { this = value; }
		}

		#endregion

		#region IZType Members

		ZInt INumericZType.ToZInt()
		{
			return this;
		}

		[XmlIgnore]
		public ZDataType DataType
		{
			get { return ZDataType.Integer; }
		}

		[XmlIgnore]
		public Type BaseDataType
		{
			get { return typeof(int); }
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
			get { return this == (ZInt)Default; }
		}

		[XmlIgnore]
		public IZType Default
		{
			get { return new ZInt(); }
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
		public static readonly ZInt Zero = new ZInt(0);
		#endregion

		#region Implementation

		internal readonly int fValue;

		#endregion
	}
}
