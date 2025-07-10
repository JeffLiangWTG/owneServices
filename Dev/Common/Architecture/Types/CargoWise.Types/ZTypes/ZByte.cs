using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.Types
{
	[TypeConverter(typeof(ZByteTypeConverter))]
	[ValueRange(byte.MaxValue, byte.MinValue), HasDecimals(false)]
	[DebuggerDisplay("{fValue}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZByte : INumericZType, IZTypeInternals, IFormattable
	{
		[DebuggerStepThrough]
		public ZByte(object value)
		{
			if (value == null)
			{
				this = Zero;
			}
			else if (value is byte @byte)
			{
				this = new ZByte(@byte);
			}
			else
			{
				TypeConverter converter = ZByteTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					var converted = converter.ConvertFrom(value);
					this = (ZByte)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZByte), value);
				}
			}
		}

		[DebuggerStepThrough]
		public ZByte(byte value)
		{
			fValue = value;
		}

		#region Object Overrides

		public override bool Equals(object obj)
		{
			return (obj is ZByte @byte && @byte == this) ||
					(obj is byte byte1 && byte1 == fValue);
		}

		public override int GetHashCode()
		{
			return fValue.GetHashCode();
		}

		public override string ToString()
		{
			return fValue.ToString();
		}

		#endregion

		#region Wrapping Byte Functionality

		public string ToString(string format)
		{
			Argument.NotNull(format, nameof(format));
			return fValue.ToString(format);
		}

		#endregion

		#region Casting

		public static implicit operator ZByte(byte value)
		{
			return new ZByte(value);
		}

		public static implicit operator byte(ZByte value)
		{
			return value.fValue;
		}

		public static implicit operator ZInt(ZByte value)
		{
			return new ZInt(Convert.ToInt32(value));
		}

		#endregion

		internal byte ToByte()
		{
			return fValue;
		}

		#region Operator Overloads

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(ZByte lhs, ZByte rhs)
		{
			return lhs.fValue < rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(ZByte lhs, ZByte rhs)
		{
			return lhs.fValue > rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator !=(ZByte lhs, ZByte rhs)
		{
			return lhs.fValue != rhs.fValue;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs")]
		public static bool operator ==(ZByte lhs, ZByte rhs)
		{
			return lhs.fValue == rhs.fValue;
		}

		public static ZByte operator ++(ZByte value)
		{
			return new ZByte((byte)(value.fValue + 1));
		}

		public static ZByte operator --(ZByte value)
		{
			return new ZByte((byte)(value.fValue - 1));
		}

		#endregion

		#region TryParse / ParseSafe

		/// <summary>
		/// Can the given string be converted to a byte? If so, returns the converted byte, else zero.
		/// </summary>
		/// <param name="value">A string containing a number to convert.</param>
		/// <param name="result">Returned as the byte equivalent of the given string, else zero if it cannot be converted.</param>
		public static bool TryParse(string value, out ZByte result)
		{
			bool success =
				double.TryParse(value, NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out var tryParseResult) &&
				CanConvertToByte(tryParseResult);

			result = success ? (byte)tryParseResult : (byte)0;

			return success;
		}

		public static ZByte ParseSafe(ZString value, ZByte defaultValue)
		{
			return TryParse(value, out var parsed) ? parsed : defaultValue;
		}

		static bool CanConvertToByte(double value)
		{
			return (value >= byte.MinValue && value <= byte.MaxValue);
		}

		#endregion

		#region Zero
		public static readonly ZByte Zero = new ZByte(0);
		#endregion

		#region XmlSerializedValue

		[XmlText]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public byte XmlSerializedValue
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
			get { return typeof(byte); }
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
			get { return this == (ZByte)Default; }
		}

		[XmlIgnore]
		public IZType Default
		{
			get { return new ZByte(); }
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

		#region IZTypeInternals Members

		object IZTypeInternals.GetValueForLogicalDataLayer(bool isNullable)
		{
			return IsEmpty && isNullable ? DBNull.Value : fValue;
		}

		#endregion

		#region IFormattable Members

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return fValue.ToString(format, formatProvider);
		}

		#endregion

		#region Implementation

		readonly byte fValue;

		#endregion
	}
}
