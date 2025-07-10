using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.Types
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1036:OverrideMethodsOnComparableTypes"), TypeConverter(typeof(ZDecimalTypeConverter))]
	[DebuggerDisplay("{fValue}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZDecimal : INumericZType, IZTypeInternals, IFormattable
	{
		[DebuggerStepThrough]
		public ZDecimal(object value)
		{
			if (value == null)
			{
				this = Zero;
			}
			else if (value is decimal @decimal)
			{
				this = new ZDecimal(@decimal);
			}
			else
			{
				TypeConverter converter = ZDecimalTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					var converted = converter.ConvertFrom(value);
					this = (ZDecimal)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZDecimal), value);
				}
			}
		}

		[DebuggerStepThrough]
		public ZDecimal(decimal value)
		{
			fValue = value;
		}

		[DebuggerStepThrough]
		public ZDecimal(ZInt value)
		{
			fValue = value;
		}

		public bool IsInRange(ZDecimal lowValue, ZDecimal highValue)
		{
			return (this >= lowValue && this <= highValue);
		}

		public ZDecimal Round(int numberOfDecimalPlaces)
		{
			if (numberOfDecimalPlaces < 0)
			{
				throw new ArgumentException("Number of decimal places should be positive for rounding.", nameof(numberOfDecimalPlaces));
			}

			return ObjectCache.RoundingProvider.Round(fValue, numberOfDecimalPlaces);
		}

		public bool IsWithinSqlPrecisionAndScale(int precision, int scale)
		{
			decimal maxIntegralPart = GetMaxIntegralPart(precision, scale);
			return (Math.Abs(decimal.Truncate(fValue)) <= maxIntegralPart);
		}

		[XmlIgnore]
		public int DecimalPlaces
		{
			get
			{
				int decimalPlaces = 0;
				ZDecimal decimalPart = this - decimal.Truncate(this);   // To stop potential overflow
				while (decimal.Truncate(decimalPart) != decimalPart)
				{
					decimalPart *= 10;
					decimalPlaces++;
				}
				return decimalPlaces;
			}
		}

		internal decimal ToDecimal()
		{
			return fValue;
		}

		/// <summary>
		/// Gets rid of trailing Zero's from ZDecimal
		/// </summary>
		/// <returns></returns>
		public ZDecimal Normalize()
		{
			return this / 1.0000000000000000000000000000m;
		}

		#region Object Overrides

		public override int GetHashCode()
		{
			return fValue.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return (obj is ZDecimal @decimal && @decimal == this) ||
				(obj is decimal decimal1 && decimal1 == fValue);
		}

		public override string ToString()
		{
			return fValue.ToString(ObjectCache.CultureProvider.Culture.NumberFormat);
		}

		#region ToStringTrimZeros

		public ZString ToStringTrimZeros()
		{
			return ToStringTrimZerosCore(ToString());
		}

		public ZString ToStringTrimZeros(string format)
		{
			return ToStringTrimZerosCore(ToString(format));
		}

		public ZString ToStringTrimZeros(int decimalPlaces)
		{
			return ToStringTrimZerosCore(ToString(decimalPlaces));
		}

		ZString ToStringTrimZerosCore(ZString value)
		{
			ZString result;

			if (value.Contains(ObjectCache.CultureProvider.Culture.NumberFormat.NumberDecimalSeparator))
			{
				result = value.TrimEnd('0');

				int toRemove = result.LastIndexOf(ObjectCache.CultureProvider.Culture.NumberFormat.NumberDecimalSeparator);
				if (toRemove != -1 && (toRemove + 1 == result.Length))
				{
					result = result.Left(toRemove);
				}
			}
			else
			{
				result = value;
			}

			return result;
		}

		#endregion

		public string ToString(int decimals)
		{
			string[] split = ToString().Replace(DecimalSeparator, "X").Split('X');
			string decimalPart = "";

			if (decimals > 0)
			{
				decimalPart = split.Length > 1 ? split[1] : "";
				decimalPart = decimalPart.Length < decimals ? decimalPart.PadRight(decimals, '0') : decimalPart.Substring(0, decimals);
				decimalPart = DecimalSeparator + decimalPart;
			}

			return split[0] + decimalPart;
		}

		/// <summary>
		/// This takes a number and returns the number in string format. If UseCommas is true it
		/// will replace the decimals with commas. If PadDecimalPlacesWithZeros is true it
		/// will pad right the decimal part with zeros.
		/// </summary>
		public string ToString(int decimals, bool useCommas)
		{
			ZString result = ToString(decimals);
			if (useCommas)
			{
				result = result.Replace(".", ",");
			}

			return result;
		}

		public ZDecimal Truncate(int decimals)
		{
			return ZTypeUtils.TruncateDecimal(fValue, decimals);
		}

		public ZDecimal Truncate()
		{
			return decimal.Truncate(fValue);
		}

		public ZDecimal Ceiling(int decimals)
		{
			if (decimals < 0)
			{
				throw new ArgumentException("Invalid argument.", nameof(decimals));
			}

			decimal scale = (decimal)Math.Pow(10, decimals);
			return Math.Ceiling(fValue * scale) / scale;
		}

		#endregion

		#region Wrapping Decimal Functionality

		#region Zero
		public static readonly ZDecimal Zero = new ZDecimal(0m);
		#endregion

		public static ZDecimal Parse(ZString value)
		{
			return Parse(value, ObjectCache.CultureProvider.Culture.NumberFormat);
		}

		public static ZDecimal Parse(ZString value, NumberFormatInfo numberFormat)
		{
			return decimal.Parse(value, numberFormat);
		}

		public static bool CanParse(ZString value)
		{
			return TryParse(value, out var dummyResult);
		}

		public static bool CanParseAsInteger(ZString value)
		{
			return TryParse(value, out var dummyResult) && dummyResult.IsInteger;
		}

		public static ZDecimal ParseSafe(ZString value, ZDecimal defaultValue)
		{
			return TryParse(value, out var parsed) ? parsed : defaultValue;
		}

		public string ToString(string format)
		{
			return fValue.ToString(format, ObjectCache.CultureProvider.Culture.NumberFormat);
		}

		#endregion

		#region Casting

		[DebuggerStepThrough]
		public static implicit operator ZDecimal(decimal value)
		{
			return new ZDecimal(value);
		}

		[DebuggerStepThrough]
		public static implicit operator decimal(ZDecimal value)
		{
			return value.fValue;
		}

		[DebuggerStepThrough]
		public static implicit operator ZDecimal(int value)
		{
			return new ZDecimal(value);
		}

		[DebuggerStepThrough]
		public static implicit operator ZDecimal(long value)
		{
			return new ZDecimal(value);
		}

		[DebuggerStepThrough]
		public static implicit operator ZDecimal(double value)
		{
			return new ZDecimal(value);
		}

		[DebuggerStepThrough]
		public static implicit operator ZDecimal(float value)
		{
			return new ZDecimal(value);
		}

		[DebuggerStepThrough]
		public static explicit operator double(ZDecimal value)
		{
			return (double)value.fValue;
		}

		[DebuggerStepThrough]
		public static explicit operator ZInt(ZDecimal value)
		{
			return new ZInt(decimal.ToInt32(value.fValue));
		}

		[DebuggerStepThrough]
		public static explicit operator int(ZDecimal value)
		{
			return decimal.ToInt32(value.fValue);
		}

		[DebuggerStepThrough]
		public static explicit operator long(ZDecimal value)
		{
			return decimal.ToInt64(value.fValue);
		}

		#endregion

		#region Operator Overloads

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZDecimal lhs, ZDecimal rhs)
		{
			return lhs.fValue != rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZDecimal lhs, ZDecimal rhs)
		{
			return lhs.fValue == rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(ZDecimal lhs, ZDecimal rhs)
		{
			return lhs.fValue < rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(ZDecimal lhs, ZDecimal rhs)
		{
			return lhs.fValue <= rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(ZDecimal lhs, ZDecimal rhs)
		{
			return lhs.fValue > rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(ZDecimal lhs, ZDecimal rhs)
		{
			return lhs.fValue >= rhs.fValue;
		}

		[DebuggerStepThrough]
		public static ZDecimal operator ++(ZDecimal value)
		{
			return new ZDecimal(value.fValue + 1m);
		}

		[DebuggerStepThrough]
		public static ZDecimal operator --(ZDecimal value)
		{
			return new ZDecimal(value.fValue - 1m);
		}

		#endregion

		#region TryParse

		/// <summary>
		/// Can the given string be converted to a decimal? If so, returns the converted decimal, else zero.
		/// </summary>
		/// <param name="value">A string containing a number to convert.</param>
		/// <param name="result">Returned as the decimal equivalent of the given string, else zero if it cannot be converted.</param>
		public static bool TryParse(string value, out ZDecimal result)
		{
			return TryParse(value, ObjectCache.CultureProvider.Culture.NumberFormat, out result);
		}

		public static bool TryParse(string value, IFormatProvider provider, out ZDecimal result)
		{
			result = 0;
			if (value == null || provider == null)
			{
				return false;
			}

			bool success = false;
			if (value.Length > 0)
			{
				try
				{
					result = decimal.Parse(value, provider);
					success = true;
				}
				catch (ArgumentNullException) { }
				catch (FormatException) { }
				catch (OverflowException) { }
			}

			return success;
		}

		#endregion

		#region XmlSerializedValue

		[XmlText]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public decimal XmlSerializedValue
		{
			get { return this; }
			set { this = value; }
		}

		#endregion

		#region IZType Members

		public ZInt ToZInt()
		{
			return (int)decimal.Truncate(fValue);
		}

		public ZLong ToZLong()
		{
			return (long)decimal.Truncate(fValue);
		}

		[XmlIgnore]
		public ZDataType DataType
		{
			get { return ZDataType.Numeric; }
		}

		[XmlIgnore]
		public Type BaseDataType
		{
			get { return typeof(decimal); }
		}

		[XmlIgnore]
		public bool IsEmpty
		{
			get { return fValue == 0M; }
		}

		[XmlIgnore]
		public bool IsValid
		{
			get { return true; }
		}

		[XmlIgnore]
		public bool IsDefault
		{
			get { return this == (ZDecimal)Default; }
		}

		[XmlIgnore]
		public bool IsInteger
		{
			get { return this % 1 == 0; }
		}

		[XmlIgnore]
		public IZType Default
		{
			get { return new ZDecimal(); }
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

		#region Implementation

		readonly decimal fValue;

		static string DecimalSeparator
		{
			get
			{
				var returnVal = ObjectCache.CultureProvider.Culture.NumberFormat.NumberDecimalSeparator;
				return returnVal;
			}
		}

		static decimal GetMaxIntegralPart(int precision, int scale)
		{
			return (decimal)Math.Pow(10, precision - scale) - 1;
		}

		#endregion
	}
}

