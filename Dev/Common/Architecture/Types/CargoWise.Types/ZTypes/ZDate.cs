using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;

namespace CargoWise.Types
{
	[TypeConverter(typeof(ZDateTypeConverter))]
	[DebuggerDisplay("{inner}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZDate : IZType, IZTypeInternals, IComparable, IFormattable
	{
		[DebuggerStepThrough]
		public ZDate(int year, int month, int day)
		{
			if (!(year >= 1 && year <= 9999))
			{
				throw new ArgumentException("An invalid year provided while initializing a ZDate object.", nameof(year));
			}

			if (!(month >= 1 && month <= 12))
			{
				throw new ArgumentException("An invalid month provided while initializing a ZDate object.", nameof(month));
			}

			if (!(day >= 1 && day <= 31))
			{
				throw new ArgumentException("An invalid day provided to initialize a ZDateTime object.", nameof(day));
			}

			if ((year == 1 && month == 1 && day == 1))
			{
				throw new ArgumentException("Do not use 1 1 1 to initialise as it will be ZDate.Invalid. Use ZDate.Invalid instead", nameof(year));
			}

			inner = new ZDateTime(year, month, day, ZDateTime.DefaultKind);
		}

		[DebuggerStepThrough]
		public ZDate(object value)
		{
			if (value == null)
			{
				inner = Empty;
			}
			else if (value is DateTime dateTimeValue)
			{
				inner = new ZDate(dateTimeValue);
			}
			else
			{
				TypeConverter converter = ZDateTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					var converted = converter.ConvertFrom(value);
					inner = (ZDate)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZDate), value);
				}
			}
		}

		[DebuggerStepThrough]
		public ZDate(object value, DateTimeKind kind)
			: this(value)
		{
			inner = new ZDateTime(inner, kind);
		}

		internal ZDate(ZDateTime value)
		{
			if (!value.IsValid)
			{
				inner = value;
			}
			else
			{
				inner = new ZDateTime(new DateTime(value.Ticks / TimeSpan.TicksPerDay * TimeSpan.TicksPerDay));
			}
		}

		#region Static

		/// <summary>
		/// The empty ZDate.
		/// </summary>
		public static readonly ZDate Empty = new ZDate(ZDateTime.Empty);

		/// <summary>
		/// The invalid (but not empty) ZDate.
		/// </summary>
		public static readonly ZDate Invalid = new ZDate(ZDateTime.Invalid);

		/// <summary>
		/// The current local date.
		/// </summary>
		public static ZDate Today
		{
			get { return (ZDate)ZDateTime.Today; }
		}

		#endregion

		public ZDateTime Add(TimeSpan ticks)
		{
			return inner.Add(ticks);
		}

		public ZDateTime AddHours(int hours)
		{
			return inner.AddHours(hours);
		}

		public ZDateTime AddMinutes(int minutes)
		{
			return inner.AddMinutes(minutes);
		}

		public string ToShortDateString()
		{
			return inner.ToShortDateString();
		}

		public ZDateTime ToZDateTime()
		{
			return inner;
		}

		#region Object Overrides

		/// <summary>
		/// Is this instance equal to the specified object?
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		public override bool Equals(object obj)
		{
			return
				(obj is ZDate zDateObj && zDateObj == this) ||
				(obj is DateTime && inner.Equals(obj)) ||
				(obj is ZDateTime && inner.Equals(obj));
		}

		/// <summary>
		/// The hash code for this instance.
		/// </summary>
		public override int GetHashCode()
		{
			return inner.GetHashCode();
		}

		#endregion

		#region Brett's Birthday

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Bretts")]
		public static ZDate BrettsBirthday
		{
			get { return new ZDate(1971, 9, 18); }
		}

		#endregion

		#region Casting

		/// <summary>
		/// Converts the ZDate to a DateTime. WARNING - an exception will be thrown if ToDateTime() is invoked on an empty or invalid ZDate!".
		/// </summary>
		[DebuggerStepThrough]
		public DateTime ToDateTime()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot convert an invalid ZDate to DateTime.");
			}

			return inner.ToDateTime();
		}

		[DebuggerStepThrough]
		public static explicit operator ZDate(DateTime value)
		{
			return new ZDate(value);
		}

		[DebuggerStepThrough]
		public static explicit operator ZDate(ZDateTime value)
		{
			return new ZDate(value.Date);
		}

		[DebuggerStepThrough]
		public static implicit operator ZDateTime(ZDate value)
		{
			return value.inner;
		}

		#endregion

		#region Operator Overloads: ZDate & ZDate

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static TimeSpan operator -(ZDate lhs, ZDate rhs)
		{
			if (!lhs.IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot manipulate invalid ZDates (operator-, lhs).");
			}

			if (!rhs.IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot manipulate invalid ZDates (operator-, rhs).");
			}

			return lhs.inner - rhs.inner;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZDate lhs, ZDate rhs)
		{
			return lhs.inner == rhs.inner;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZDate lhs, ZDate rhs)
		{
			return lhs.inner != rhs.inner;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(ZDate lhs, ZDate rhs)
		{
			return lhs.inner < rhs.inner;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(ZDate lhs, ZDate rhs)
		{
			return lhs.inner > rhs.inner;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(ZDate lhs, ZDate rhs)
		{
			return lhs.inner <= rhs.inner;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(ZDate lhs, ZDate rhs)
		{
			return lhs.inner >= rhs.inner;
		}

		#endregion

		#region Wrapping the internal DateTime

		/// <summary>
		/// The year component for this instance.
		/// </summary>
		[XmlIgnore]
		public int Year
		{
			get
			{
				return inner.Year;
			}
		}

		/// <summary>
		/// The month component for this instance.
		/// </summary>
		[XmlIgnore]
		public int Month
		{
			get
			{
				return inner.Month;
			}
		}

		/// <summary>
		/// The day of the month represented by this instance.
		/// </summary>
		[XmlIgnore]
		public int Day
		{
			get
			{
				return inner.Day;
			}
		}

		/// <summary>
		/// The day of the week represented by this instance.
		/// </summary>
		[XmlIgnore]
		public DayOfWeek DayOfWeek
		{
			get
			{
				return inner.DayOfWeek;
			}
		}

		/// <summary>
		/// The day of the year represented by this instance.
		/// </summary>
		[XmlIgnore]
		public int DayOfYear
		{
			get
			{
				return inner.DayOfYear;
			}
		}

		/// <summary>
		/// The value of this instance plus the specified number of days.
		/// </summary>
		public ZDate AddDays(int days)
		{
			return (ZDate)inner.AddDays(days);
		}

		/// <summary>
		/// The value of this instance plus the specified number of months.
		/// </summary>
		public ZDate AddMonths(int months)
		{
			if (!(months >= -120000 && months <= 120000))
			{
				throw new ArgumentException("An invalid month provided to add to a ZDate.", nameof(months));
			}

			return (ZDate)inner.AddMonths(months);
		}

		/// <summary>
		/// The value of this instance plus the specified number of years.
		/// </summary>
		public ZDate AddYears(int years)
		{
			if (!(years >= -10000 && years <= 10000))
			{
				throw new ArgumentException("An invalid year provided to add to a ZDate.", nameof(years));
			}

			return (ZDate)inner.AddYears(years);
		}

		#endregion

		#region XmlSerializedValue

		[XmlText(DataType = "date")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public DateTime XmlSerializedValue
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZDateTimeException("Cannot convert an invalid ZDate to DateTime.");
				}

				return ToDateTime();
			}
			set { this = new ZDate(value); }
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
			get { return typeof(DateTime); }
		}

		[XmlIgnore]
		public bool IsDefault
		{
			get { return inner.IsDefault; }
		}

		[XmlIgnore]
		public IZType Default
		{
			get { return Empty; }
		}

		/// <summary>
		/// Is the ZDate empty - that is, ZDate.Empty?
		/// </summary>
		[XmlIgnore]
		public bool IsEmpty
		{
			[DebuggerStepThrough]
			get { return inner.IsEmpty; }
		}

		/// <summary>
		/// Is the ZDate valid - that is, not ZDate.Empty and not ZDate.Invalid?
		/// IsValid must be tested before casting a ZDate to a DateTime as an exception will be throw if the ZDate is not valid.
		/// </summary>
		[XmlIgnore]
		public bool IsValid
		{
			[DebuggerStepThrough]
			get
			{
				return inner.IsValid;
			}
		}

		#endregion

		#region IZTypeInternals Members

		[DebuggerStepThrough]
		object IZTypeInternals.GetValueForLogicalDataLayer(bool isNullable)
		{
			return (inner as IZTypeInternals).GetValueForLogicalDataLayer(isNullable);
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			return inner.CompareTo(obj);
		}

		#endregion

		#region IFormattable Members

		public string ToString(string format, IFormatProvider formatProvider)
		{
			return inner.ToString(format, formatProvider);
		}

		#endregion

		#region TryParse

		/// <summary>
		/// Can the given string be converted to a date? If so, Result contains the ZDate resulting from the conversion
		/// </summary>
		/// <param name="value">A string containing a date string to convert; the date string must be in the format of yyddd (2 year digits plus day of year </param>
		/// <param name="result">Returned as the datetime equivalent of the given string, else invalid if it cannot be converted.</param>
		public static bool TryParseJulianDate(string value, out ZDate result)
		{
			bool success = false;
			result = Invalid;

			if (string.IsNullOrEmpty(value))
			{
				result = Empty;
				success = true;
			}
			else if (value.Length == 5)
			{
				string yearString = value.Substring(0, 2);
				if (ZDateTime.TryParseExact(yearString + "0101", out var year, "yyMMdd"))
				{
					if (ZInt.TryParse(value.Substring(2), out var dayOfyear) && year.IsValid)
					{
						if (dayOfyear == 1)
						{
							result = year.Date;
							success = true;
						}
						else if (dayOfyear > 1)
						{
							result = year.AddDays(dayOfyear - 1).Date;
							success = true;
						}
					}
				}
			}
			return success;
		}

		#endregion

		#region ToString

		public override string ToString()
		{
			return inner.ToShortDateString();
		}

		public string ToString(string format)
		{
			return inner.ToString(format);
		}

		/// <summary>
		/// The string representation of this instance in Julian Date format.
		/// </summary>
		public string ToJulianDateString()
		{
			string result = inner.ToString("yy");
			return (IsEmpty || !IsValid) ? result : result + inner.DayOfYear.ToString().PadLeft(3, '0');
		}

		public string ToISO8601ShortDateString()
		{
			return inner.ToISO8601ShortDateString();
		}

		#endregion

		#region Implementation

		readonly ZDateTime inner;

		#endregion
	}
}
