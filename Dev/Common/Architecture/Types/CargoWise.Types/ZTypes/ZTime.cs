using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.Types
{
	[TypeConverter(typeof(ZTimeTypeConverter))]
	[DebuggerDisplay("{DebuggerDisplay}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZTime : IZType, IZTypeInternals, IFormattable, IConvertible
	{
		[XmlIgnore]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Debugging")]
		public string DebuggerDisplay
		{
			get { return IsEmpty ? "Empty" : (IsValid ? ToString(TimeFormat) : "Invalid"); }
		}

		[DebuggerStepThrough]
		public ZTime(object value)
		{
			if (value == null)
			{
				this = Empty;
			}
			else if (value is DateTime time)
			{
				this = new ZTime(time);
			}
			else if (value is TimeSpan timeSpan)
			{
				this = new ZTime(timeSpan);
			}
			else
			{
				TypeConverter converter = ZTimeTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					var converted = converter.ConvertFrom(value);
					this = (ZTime)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZTime), value);
				}
			}
		}

		[DebuggerStepThrough]
		public ZTime(TimeSpan time)
		{
			isNotEmpty = true;
			isValid = time.Ticks >= 0 && time.Days == 0;
			fValueUnsafe = new TimeSpan(time.Hours, time.Minutes, 0);
		}

		[DebuggerStepThrough]
		public ZTime(DateTime value)
		{
			isNotEmpty = true;
			isValid = value.Year != 2000;
			fValueUnsafe = new TimeSpan(value.Hour, value.Minute, 0);
		}

		[DebuggerStepThrough]
		public ZTime(int hour, int minute)
		{
			if (!(hour >= 0 && hour < 24))
			{
				throw new ArgumentException("An invalid hour provided to initialize a ZTime object.", nameof(hour));
			}

			if (!(minute >= 0 && minute < 60))
			{
				throw new ArgumentException("An invalid minute provided to initialize a ZTime object.", nameof(minute));
			}
			isNotEmpty = true;
			isValid = true;
			fValueUnsafe = new TimeSpan(hour, minute, 0);
		}

		[DebuggerStepThrough]
		public ZTime(long ticks)
		{
			if (ticks < 0)
			{
				throw new ArgumentException("A negative ticks value provided to initialize a ZTime object.", nameof(ticks));
			}
			
			if (ticks >= 599266944000000000)
			{
				throw new ArgumentException("An invalid ticks value provided to initialize a ZTime object.", nameof(ticks));
			}

			var returnVal = new DateTime(ticks);
			this = new ZTime(returnVal);
		}

		/// <summary>
		/// Converts the ZTime to an SQL-legal format. The ZTime must be valid before attempting to access this property.
		/// </summary>
		[XmlIgnore]
		public ZString SqlFormat
		{
			get
			{
				return ObjectCache.DateTimeProvider.FormatSqlTime(ValueSafe);
			}
		}

		#region Static

		/// <summary>
		/// The empty ZTime.
		/// </summary>
		public static readonly ZTime Empty;

		/// <summary>
		/// The invalid (but not empty) ZTime.
		/// </summary>
		static readonly ZTime invalid = new ZTime(new TimeSpan(-1));
		public static ZTime Invalid
		{
			get
			{
				return invalid;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string")]
		public const string InvalidLiteral = "<Invalid>";
		public static TimeSpan InvalidTime() { return new TimeSpan(-1); }

		/// <summary>
		/// The default kind
		/// </summary>
		public static readonly DateTimeKind DefaultKind = DateTimeKind.Local;

		public static ZTime FromSqlFormat(ZString time)
		{
			return ObjectCache.DateTimeProvider.FromSqlTime(time);
		}

		/// <summary>
		/// Returns copy of input dateTime rounding by a resolution
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static ZTime Truncate(ZTime dateTime, long resolution)
		{
			if (!dateTime.IsValid)
			{
				throw new OperationOnInvalidZTimeException("Cannot truncate an invalid ZTime.");
			}

			return new DateTime(dateTime.Ticks - (dateTime.Ticks % resolution));
		}

		#endregion

		#region Object Overrides

		/// <summary>
		/// Is this instance equal to the specified object?
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		public override bool Equals(object obj)
		{
			return (obj is ZTime time && time == this) ||
				(obj is TimeSpan timeSpan && timeSpan.Hours == ValueSafe.Hours && timeSpan.Minutes == ValueSafe.Minutes);
		}

		/// <summary>
		/// The hash code for this instance.
		/// </summary>
		public override int GetHashCode()
		{
			return ValueSafe.GetHashCode();
		}

		#endregion

		#region Casting

		/// <summary>
		/// Converts the ZTime to a DateTime. WARNING - an exception will be thrown if ToDateTime() is invoked on an empty or invalid ZTime!".
		/// </summary>
		//[DebuggerStepThrough]
		public DateTime ToDateTime()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZTimeException("Cannot convert an Empty or Invalid ZTime to a DateTime!");
			}

			return StartOfDay + ValueSafe;
		}

		public TimeSpan ToTimeSpan()
		{
			return ValueSafe;
		}

		[DebuggerStepThrough]
		public static implicit operator ZTime(DateTime value)
		{
			return new ZTime(value.TimeOfDay);
		}

		[DebuggerStepThrough]
		public static implicit operator ZTime(TimeSpan value)
		{
			return new ZTime(value);
		}

		#endregion

		#region Operator Overloads: ZTime & ZTime

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static TimeSpan operator -(ZTime lhs, ZTime rhs)
		{
			if (!lhs.IsValid)
			{
				throw new OperationOnInvalidZTimeException("Cannot subtract empty ZTime (lhs).");
			}

			if (!rhs.IsValid)
			{
				throw new OperationOnInvalidZTimeException("Cannot subtract empty ZTime (rhs).");
			}

			return lhs.ToDateTime() - rhs.ToDateTime();
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZTime lhs, ZTime rhs)
		{
			bool result = false;

			if (lhs.IsValid && rhs.IsValid)
			{
				result = (lhs.ValueSafe == rhs.ValueSafe);
			}
			else if (lhs.IsEmpty && rhs.IsEmpty)
			{
				result = true;
			}
			else if (!lhs.IsValid && !rhs.IsValid && (lhs.IsEmpty == rhs.IsEmpty))
			{
				result = true;
			}

			return result;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZTime lhs, ZTime rhs)
		{
			return !(lhs == rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(ZTime lhs, ZTime rhs)
		{
			return lhs.IsValid && rhs.IsValid && lhs.ValueSafe < rhs.ValueSafe;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(ZTime lhs, ZTime rhs)
		{
			return lhs.IsValid && rhs.IsValid && lhs.ValueSafe > rhs.ValueSafe;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(ZTime lhs, ZTime rhs)
		{
			return lhs < rhs || lhs == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(ZTime lhs, ZTime rhs)
		{
			return lhs > rhs || lhs == rhs;
		}

		#endregion

		#region Operator Overloads: ZTime & DateTime

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZTime lhs, DateTime rhs)
		{
			return lhs == new ZTime(rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(DateTime lhs, ZTime rhs)
		{
			return new ZTime(lhs) == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZTime lhs, DateTime rhs)
		{
			return !(lhs == rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(DateTime lhs, ZTime rhs)
		{
			return !(lhs == rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(ZTime lhs, DateTime rhs)
		{
			return lhs < new ZTime(rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(DateTime lhs, ZTime rhs)
		{
			return new ZTime(lhs) < rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(ZTime lhs, DateTime rhs)
		{
			return lhs > new ZTime(rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(DateTime lhs, ZTime rhs)
		{
			return new ZTime(lhs) > rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(ZTime lhs, DateTime rhs)
		{
			return lhs < rhs || lhs == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(DateTime lhs, ZTime rhs)
		{
			return lhs < rhs || lhs == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(ZTime lhs, DateTime rhs)
		{
			return lhs > rhs || lhs == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(DateTime lhs, ZTime rhs)
		{
			return lhs > rhs || lhs == rhs;
		}

		#endregion

		#region Operator Overloads: ZTime & TimeSpan

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static ZTime operator +(ZTime lhs, TimeSpan rhs)
		{
			if (!lhs.IsValid)
			{
				throw new OperationOnInvalidZTimeException("Cannot convert an Empty or Invalid ZTime to a DateTime!");
			}

			return lhs.ToDateTime() + rhs;
		}

		#endregion

		#region TryParse

		public static DateTime RemoveDate(DateTime dt)
		{
			return new DateTime(dt.TimeOfDay.Ticks, dt.Kind);
		}

		/// <summary>
		/// Can the given string in a certain format be converted to a datetime? If so, Result contains the ZTime resulting from the conversion
		/// </summary>
		/// <param name="value">A string containing a datetime string to convert.</param>
		/// <param name="result">Returned as the datetime equivalent of the given string, else invalid if it cannot be converted.</param>
		/// <param name="format">The format string to apply during parsing</param>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
		public static bool TryParseExact(string value, out ZTime result, string format)
		{
			Argument.NotNull(format, nameof(format));
			bool success = false;
			result = Invalid;

			if (string.IsNullOrEmpty(value))
			{
				result = Empty;
				success = true;
			}
			else
			{
				if (TimeSpan.TryParseExact(value, format, ObjectCache.CultureProvider.Culture, out var parsedResult))
				{
					result = parsedResult;
					success = true;
				}
			}
			return success;
		}

		#endregion

		#region Wrapping the internal TimeSpan

		#region ToString

		/// <summary>
		/// The default string representation of this instance.
		/// </summary>

		[SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Should be the only place in the system that uses date format string")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Should be the only place in the system that uses date format string")]
		public const string TimeFormat = "hh':'mm";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Should be the only place in the system that uses date format string")]
		public const string OptionalTimeFormat = "[hh]':'[mm]";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Invalid string conversion")]
		public override string ToString()
		{
			if (this == Invalid)
			{
				return "<Invalid>";
			}
			return ToString(TimeFormat);
		}

		public string ToTimeString()
		{
			if (this == Invalid)
			{
				return "";
			}
			return ToString(TimeFormat);
		}

		public string FormatDateTime()
		{
			return ObjectCache.DateTimeProvider.FormatDateTime(ToDateTime());
		}

		/// <summary>
		/// The string representation of this instance in the specified format.
		/// </summary>
		/// <param name="format">A format string.</param>
		public string ToString(string format)
		{
			string result;

			if (IsEmpty)
			{
				result = "";
			}
			else if (!IsValid)
			{
				result = InvalidLiteral;
			}
			else
			{
				if (format == null || string.IsNullOrEmpty(format))
				{
					result = ValueSafe.ToString(TimeFormat);
				}
				else
				{
					result = ValueSafe.ToString(format);
				}
			}

			return result;
		}

		#endregion

		/// <summary>
		/// The number of ticks that represent the date and time of this instance.
		/// </summary>
		[XmlIgnore]
		public long Ticks
		{
			[DebuggerStepThrough]
			get
			{
				return ValueSafe.Ticks;
			}
		}

		readonly public static DateTime StartOfDay = new DateTime(1900, 1, 1);
		readonly public static DateTime EndOfDay = new DateTime(1900, 1, 2);

		/// <summary>
		/// The time of day for this instance.
		/// </summary>
		[XmlIgnore]
		public TimeSpan TimeOfDay
		{
			get
			{
				return ValueSafe;
			}
		}

		/// <summary>
		/// The minutes component for this instance.
		/// </summary>
		[XmlIgnore]
		public int Minute
		{
			get
			{
				return ValueSafe.Minutes;
			}
		}

		/// <summary>
		/// The hour represented by this instance.
		/// </summary>
		[XmlIgnore]
		public int Hour
		{
			get
			{
				return ValueSafe.Hours;
			}
		}

		/// <summary>
		/// The value of this instance plus the specified time span.
		/// </summary>
		public ZTime Add(TimeSpan ticks)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add timespans to an invalid time");
			}

			var inner = ValueSafe.Add(ticks);
			var returnVal = new ZTime(inner.Hours, inner.Minutes);
			if (!returnVal.IsValid)
			{
				throw new InvalidZTimeResultException("Number of ticks added resulted in the minimum DateTime and hence an invalid ZTime");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of minutes.
		/// </summary>
		public ZTime AddMinutes(int minutes)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add minutes to an invalid time");
			}

			var returnVal = new ZTime(ValueSafe.Add(new TimeSpan(0, minutes, 0)));
			if (!returnVal.IsValid)
			{
				throw new InvalidZTimeResultException("Number of minutes added resulted in the minimum DateTime and hence an invalid ZTime");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of hours.
		/// </summary>
		public ZTime AddHours(int hours)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add hours to an invalid time.");
			}

			var returnVal = new ZTime(ValueSafe.Add(new TimeSpan(hours, 0, 0)));
			if (!returnVal.IsValid)
			{
				throw new InvalidZTimeResultException("Number of hours added resulted in an invalid ZTime");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of hours.
		/// </summary>
		public ZTime AddHours(ZDecimal hours)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add hours to an invalid time (ZDecimal).");
			}

			decimal decimalHours = (decimal)hours;
			var returnVal = new ZTime(Ticks + (long)((double)decimalHours * TimeSpan.TicksPerMinute * 60));
			if (!returnVal.IsValid)
			{
				throw new InvalidZTimeResultException("Number of hours added resulted in the minimum DateTime and hence an invalid ZTime");
			}

			return returnVal;
		}

		#endregion

		#region XmlSerializedValue

		[XmlText]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String format")]
		public String XmlSerializedValue
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZTimeException();
				}

				return ToString("c");// "[-][d'.']HH'\\:'mm'\\:'ss['.'fffffff]");
			}
			set { this = new ZTime(value); }
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (obj is ZTime zTime)
			{
				if (zTime.IsValid)
				{
					obj = zTime.ValueSafe;
				}
				else
				{
					return IsEmpty ? (zTime.IsEmpty ? 0 : 1) : (IsValid ? -1 : 0);
				}
			}
			else if (obj is IZTypeInternals zTypeInternals)
			{
				obj = zTypeInternals.GetValueForLogicalDataLayer(false);
				if (obj is DateTime objDateTime && objDateTime != DateTime.MinValue)
				{
					obj = new TimeSpan(objDateTime.Hour, objDateTime.Minute, 0);
				}
			}
			else if (obj is DateTime objDateTime)
			{
				obj = new TimeSpan(objDateTime.Hour, objDateTime.Minute, 0);
			}

			return obj is DBNull && IsEmpty ? 0 : ValueSafe.CompareTo(obj);
		}

		#endregion

		#region IZType Members

		[XmlIgnore]
		public ZDataType DataType
		{
			[DebuggerStepThrough]
			get { return ZDataType.NonNumeric; }
		}

		[XmlIgnore]
		public Type BaseDataType
		{
			get { return typeof(TimeSpan); }
		}

		[XmlIgnore]
		public bool IsDefault
		{
			get { return this == (ZTime)Default; }
		}

		[XmlIgnore]
		public IZType Default
		{
			get
			{
				return Empty;
			}
		}

		/// <summary>
		/// Is the ZTime empty - that is, not ZTime.Empty?
		/// </summary>
		[XmlIgnore]
		public bool IsEmpty
		{
			[DebuggerStepThrough]
			get
			{
				return !isNotEmpty;
			}
		}

		/// <summary>
		/// Returns true if the ZTime is valid - that is, not ZTime.Empty and has a valid value
		/// IsValid must be tested before casting a ZTime to a TimeSpan as an exception will be throw if the ZTime is not valid.
		/// </summary>
		[XmlIgnore]
		public bool IsValid
		{
			[DebuggerStepThrough]
			get
			{
				return isValid;
			}
		}

		#endregion

		#region IZTypeInternals Members

		[DebuggerStepThrough]
		object IZTypeInternals.GetValueForLogicalDataLayer(bool isNullable)
		{
			if (IsEmpty && isNullable)
			{
				return DBNull.Value;
			}
			if (!IsValid)
			{
				return InvalidTime();
			}
			return ToTimeSpan();
		}

		#endregion

		#region IFormattable Members

		[SuppressMessage("Microsoft.Globalization", "CA1305")]
		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (IsValid)
			{
				if (!string.IsNullOrEmpty(format) && formatProvider != null)
				{
					return ValueSafe.ToString(format, formatProvider);
				}
				else
				{
					return ToString(format);
				}
			}
			else
			{
				return string.Empty;
			}
		}

		#endregion

		#region Implementation

		TimeSpan ValueSafe
		{
			[DebuggerStepThrough]
			get
			{
				return fValueUnsafe;
			}
		}

		readonly TimeSpan fValueUnsafe;
		readonly bool isNotEmpty;
		readonly bool isValid;
		#endregion

		#region Operations With Dates

		public static bool Overlaps(ZTime aStart, ZTime aEnd, ZTime bStart, ZTime bEnd)
		{
			var aStartsBeforeBEnds = aStart.IsEmpty || bEnd.IsEmpty || aStart <= bEnd;
			var bStartsBeforeAEnds = bStart.IsEmpty || aEnd.IsEmpty || bStart <= aEnd;

			return aStartsBeforeBEnds && bStartsBeforeAEnds;
		}

		#endregion

		#region IConvertible Members

		TypeCode IConvertible.GetTypeCode()
		{
			return (this == Empty) ? TypeCode.Empty : TypeCode.Object;
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return (bool)((IConvertible)this).ToType(typeof(bool), provider);
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			return (char)((IConvertible)this).ToType(typeof(char), provider);
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			return (sbyte)((IConvertible)this).ToType(typeof(sbyte), provider);
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return (byte)((IConvertible)this).ToType(typeof(byte), provider);
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return (short)((IConvertible)this).ToType(typeof(short), provider);
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return (ushort)((IConvertible)this).ToType(typeof(ushort), provider);
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return (int)((IConvertible)this).ToType(typeof(int), provider);
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return (uint)((IConvertible)this).ToType(typeof(uint), provider);
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return (long)((IConvertible)this).ToType(typeof(long), provider);
		}

		ulong IConvertible.ToUInt64(IFormatProvider provider)
		{
			return (ulong)((IConvertible)this).ToType(typeof(ulong), provider);
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return (float)((IConvertible)this).ToType(typeof(float), provider);
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return (double)((IConvertible)this).ToType(typeof(double), provider);
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return (decimal)((IConvertible)this).ToType(typeof(decimal), provider);
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			return (DateTime)((IConvertible)this).ToType(typeof(DateTime), provider);
		}

		string IConvertible.ToString(IFormatProvider provider)
		{
			// To maintain existing ToString() behaviour
			if (provider == CultureInfo.InvariantCulture)
			{
				return ToString();
			}

			return (string)((IConvertible)this).ToType(typeof(string), provider);
		}

		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			TypeConverter converter = ZTimeTypeConverter.Instance;
			if (converter != null && converter.CanConvertTo(conversionType))
			{
				return converter.ConvertTo(this, conversionType);
			}
			else
			{
				throw new InvalidCastException("Converting from ZTime to " + conversionType + " not supported");
			}
		}

		#endregion
	}

	[Serializable]
	public sealed class OperationOnInvalidZTimeException : OperationOnInvalidZTypeException
	{
		public OperationOnInvalidZTimeException()
		{
		}

		public OperationOnInvalidZTimeException(string message)
			: base(message)
		{
		}

		public OperationOnInvalidZTimeException(string message, Exception ex)
			: base(message, ex)
		{
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		OperationOnInvalidZTimeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public sealed class InvalidZTimeResultException : Exception
	{
		public InvalidZTimeResultException()
		{
		}

		public InvalidZTimeResultException(string message)
			: base(message)
		{
		}

		public InvalidZTimeResultException(string message, Exception ex)
			: base(message, ex)
		{
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		InvalidZTimeResultException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
