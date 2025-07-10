using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml.Serialization;
using CargoWise.Common;
using SqlDateTime = System.Data.SqlTypes.SqlDateTime;

namespace CargoWise.Types
{
	[TypeConverter(typeof(ZDateTimeTypeConverter))]
	[DebuggerDisplay("{DebuggerDisplay}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	public struct ZDateTime : IZDate, IZTypeInternals, IFormattable, IConvertible
	{
		[XmlIgnore]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Debugging")]
		public string DebuggerDisplay
		{
			get { return IsEmpty ? "Empty" : (IsValid ? SqlFormat.ToString() : "Invalid"); }
		}

		[DebuggerStepThrough]
		public ZDateTime(object value)
			: this(value, DefaultKind)
		{
		}

		[DebuggerStepThrough]
		public ZDateTime(object value, DateTimeKind kind)
		{
			if (value == null)
			{
				this = Empty;
			}
			else if (value is DateTime time)
			{
				this = new ZDateTime(time);
			}
			else
			{
				TypeConverter converter = ZDateTimeTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					var converted = converter.ConvertFrom(value);
					this = (ZDateTime)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZDateTime), value);
				}
			}
			fValueUnsafe = DateTime.SpecifyKind(fValueUnsafe, kind);
		}

		[DebuggerStepThrough]
		public ZDateTime(DateTime value, DateTimeKind kind)
		{
			fValueUnsafe = DateTime.SpecifyKind(value, kind);
			isNotEmpty = true;
			isValid = fValueUnsafe != DateTime.MinValue;
		}

		[DebuggerStepThrough]
		public ZDateTime(DateTime value)
			: this(value, value.Kind)
		{
		}

		[DebuggerStepThrough]
		public ZDateTime(int year, int month, int day)
			: this(year, month, day, DefaultKind)
		{
			if (!(year >= 1 && year <= 9999))
			{
				throw new ArgumentException("An invalid year provided to initialize a ZDateTime object.", nameof(year));
			}

			if (!(month >= 1 && month <= 12))
			{
				throw new ArgumentException("An invalid month provided to initialize a ZDateTime object.", nameof(month));
			}

			if (!(day >= 1 && day <= 31))
			{
				throw new ArgumentException("An invalid day provided to initialize a ZDateTime object.", nameof(day));
			}

			if ((year == 1 && month == 1 && day == 1))
			{
				throw new ArgumentException("Do not use 1 1 1 to initialise as it will result in ZDateTime.Invalid. Use ZDateTime.Invalid instead", nameof(year));
			}
		}

		[DebuggerStepThrough]
		public ZDateTime(int year, int month, int day, DateTimeKind kind)
		{
			if (!(year >= 1 && year <= 9999))
			{
				throw new ArgumentException("An invalid year provided to initialize a ZDateTime object.", nameof(year));
			}

			if (!(month >= 1 && month <= 12))
			{
				throw new ArgumentException("An invalid month provided to initialize a ZDateTime object.", nameof(month));
			}

			if (!(day >= 1 && day <= 31))
			{
				throw new ArgumentException("An invalid day provided to initialize a ZDateTime object.", nameof(day));
			}

			if ((year == 1 && month == 1 && day == 1))
			{
				throw new ArgumentException("Do not use 1 1 1 to initialise as it will result in ZDateTime.Invalid. Use ZDateTime.Invalid instead", nameof(year));
			}

			this = DateTime.SpecifyKind(new DateTime(year, month, day), kind);
		}

		[DebuggerStepThrough]
		public ZDateTime(int year, int month, int day, int hour, int minute, int second)
		{
			if (!(year >= 1 && year <= 9999))
			{
				throw new ArgumentException("An invalid year provided to initialize a ZDateTime object.", nameof(year));
			}

			if (!(month >= 1 && month <= 12))
			{
				throw new ArgumentException("An invalid month provided to initialize a ZDateTime object.", nameof(month));
			}

			if (!(day >= 1 && day <= 31))
			{
				throw new ArgumentException("An invalid day provided to initialize a ZDateTime object.", nameof(day));
			}

			if (!(hour >= 0 && hour < 24))
			{
				throw new ArgumentException("An invalid hour provided to initialize a ZDateTime object.", nameof(hour));
			}

			if (!(minute >= 0 && minute < 60))
			{
				throw new ArgumentException("An invalid minute provided to initialize a ZDateTime object.", nameof(minute));
			}

			if (!(second >= 0 && second < 60))
			{
				throw new ArgumentException("An invalid second provided to initialize a ZDateTime object.", nameof(second));
			}

			if ((year == 1 && month == 1 && day == 1 && hour == 0 && minute == 0 && second == 0))
			{
				throw new ArgumentException("Do not use 1 1 1 0 0 0 to initialise as it will result in ZDateTime.Invalid. Use ZDateTime.Invalid instead", nameof(year));
			}

			this = new ZDateTime(year, month, day, hour, minute, second, DefaultKind);
		}

		[DebuggerStepThrough]
		public ZDateTime(int year, int month, int day, int hour, int minute, int second, DateTimeKind kind)
		{
			if (!(year >= 1 && year <= 9999))
			{
				throw new ArgumentException("An invalid year provided to initialize a ZDateTime object.", nameof(year));
			}

			if (!(month >= 1 && month <= 12))
			{
				throw new ArgumentException("An invalid month provided to initialize a ZDateTime object.", nameof(month));
			}

			if (!(day >= 1 && day <= 31))
			{
				throw new ArgumentException("An invalid day provided to initialize a ZDateTime object.", nameof(day));
			}

			if (!(hour >= 0 && hour < 24))
			{
				throw new ArgumentException("An invalid hour provided to initialize a ZDateTime object.", nameof(hour));
			}

			if (!(minute >= 0 && minute < 60))
			{
				throw new ArgumentException("An invalid minute provided to initialize a ZDateTime object.", nameof(minute));
			}

			if (!(second >= 0 && second < 60))
			{
				throw new ArgumentException("An invalid second provided to initialize a ZDateTime object.", nameof(second));
			}

			if (!(!(year == 1 && month == 1 && day == 1 && hour == 0 && minute == 0 && second == 0)))
			{
				throw new ArgumentException("Do not use 1 1 1 0 0 0 to initialise as it will result in ZDateTime.Invalid. Use ZDateTime.Invalid instead", nameof(year));
			}

			this = new DateTime(year, month, day, hour, minute, second, kind);
		}

		[DebuggerStepThrough]
		public ZDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond)
			: this(year, month, day, hour, minute, second, millisecond, DefaultKind)
		{
			if (!(year >= 1 && year <= 9999))
			{
				throw new ArgumentException("An invalid year provided to initialize a ZDateTime object.", nameof(year));
			}

			if (!(month >= 1 && month <= 12))
			{
				throw new ArgumentException("An invalid month provided to initialize a ZDateTime object.", nameof(month));
			}

			if (!(day >= 1 && day <= 31))
			{
				throw new ArgumentException("An invalid day provided to initialize a ZDateTime object.", nameof(day));
			}

			if (!(hour >= 0 && hour < 24))
			{
				throw new ArgumentException("An invalid hour provided to initialize a ZDateTime object.", nameof(hour));
			}

			if (!(minute >= 0 && minute < 60))
			{
				throw new ArgumentException("An invalid minute provided to initialize a ZDateTime object.", nameof(minute));
			}

			if (!(second >= 0 && second < 60))
			{
				throw new ArgumentException("An invalid second provided to initialize a ZDateTime object.", nameof(second));
			}

			if (!(millisecond >= 0 && millisecond < 1000))
			{
				throw new ArgumentException("An invalid millisecond provided to initialize a ZDateTime object.", nameof(millisecond));
			}

			if ((year == 1 && month == 1 && day == 1 && hour == 0 && minute == 0 && second == 0 && millisecond == 0))
			{
				throw new ArgumentException("Do not use 1 1 1 0 0 0 to initialise as it will result in ZDateTime.Invalid. Use ZDateTime.Invalid instead", nameof(year));
			}
		}

		[DebuggerStepThrough]
		public ZDateTime(int year, int month, int day, int hour, int minute, int second, int millisecond, DateTimeKind kind)
		{
			if (!(year >= 1 && year <= 9999))
			{
				throw new ArgumentException("An invalid year provided to initialize a ZDateTime object.", nameof(year));
			}

			if (!(month >= 1 && month <= 12))
			{
				throw new ArgumentException("An invalid month provided to initialize a ZDateTime object.", nameof(month));
			}

			if (!(day >= 1 && day <= 31))
			{
				throw new ArgumentException("An invalid day provided to initialize a ZDateTime object.", nameof(day));
			}

			if (!(hour >= 0 && hour < 24))
			{
				throw new ArgumentException("An invalid hour provided to initialize a ZDateTime object.", nameof(hour));
			}

			if (!(minute >= 0 && minute < 60))
			{
				throw new ArgumentException("An invalid minute provided to initialize a ZDateTime object.", nameof(minute));
			}

			if (!(second >= 0 && second < 60))
			{
				throw new ArgumentException("An invalid second provided to initialize a ZDateTime object.", nameof(second));
			}

			if (!(millisecond >= 0 && millisecond < 1000))
			{
				throw new ArgumentException("An invalid millisecond provided to initialize a ZDateTime object.", nameof(millisecond));
			}

			if ((year == 1 && month == 1 && day == 1 && hour == 0 && minute == 0 && second == 0 && millisecond == 0))
			{
				throw new ArgumentException("Do not use 1 1 1 0 0 0 to initialise as it will result in ZDateTime.Invalid. Use ZDateTime.Invalid instead", nameof(year));
			}

			this = new DateTime(year, month, day, hour, minute, second, millisecond, kind);
		}

		[DebuggerStepThrough]
		public ZDateTime(long ticks)
			: this(ticks, DefaultKind)
		{
			if (ticks <= 0)
			{
				throw new ArgumentException("A negative ticks value provided to initialize a ZDateTime object.", nameof(ticks));
			}

			if (ticks > 3155378975999999999)
			{
				throw new ArgumentException("An invalid ticks value provided to initialize a ZDateTime object.", nameof(ticks));
			}
		}

		[DebuggerStepThrough]
		public ZDateTime(long ticks, DateTimeKind kind)
		{
			if (ticks <= 0)
			{
				throw new ArgumentException("A negative ticks value provided to initialize a ZDateTime object.", nameof(ticks));
			}

			if (ticks > 3155378975999999999)
			{
				throw new ArgumentException("An invalid ticks value provided to initialize a ZDateTime object.", nameof(ticks));
			}

			var returnVal = DateTime.SpecifyKind(new DateTime(ticks), kind);
			this = new ZDateTime(returnVal);
		}

		/// <summary>
		/// Converts the ZDateTime to an SQL-legal format. The ZDateTime must be valid before attempting to access this property.
		/// </summary>
		[XmlIgnore]
		public ZString SqlFormat
		{
			get
			{
				return ObjectCache.DateTimeProvider.FormatSqlDateTime(ValueSafe);
			}
		}

		[XmlIgnore]
		public bool IsToday
		{
			get
			{
				ZDateTime today = Today;
				return (IsValid && Year == today.Year && Month == today.Month && Day == today.Day);
			}
		}

		#region Is in the future or is in the past

		public bool IsInThePast(double deltaInMilliSeconds = 1e-5)
		{
			if (!IsValid)
			{
				return false;
			}

			return !IsInTheFuture(deltaInMilliSeconds);
		}

		public bool IsInTheFuture(double deltaInMilliSeconds = 1e-5)
		{
			return IsInTheFuture(Now, deltaInMilliSeconds);
		}

		public bool IsInThePastUtc(double deltaInMilliSeconds = 1e-5)
		{
			if (!IsValid)
			{
				return false;
			}

			return !IsInTheFutureUtc(deltaInMilliSeconds);
		}

		public bool IsInTheFutureUtc(double deltaInMilliSeconds = 1e-5)
		{
			return IsInTheFuture(UtcNow, deltaInMilliSeconds);
		}

		public bool IsInTheFuture(ZDateTime referenceDateTime, double deltaInMilliSeconds = 1e-5)
		{
			if (!IsValid || !referenceDateTime.IsValid)
			{
				return false;
			}

			var diffTimeSpan = this - referenceDateTime;
			var isInTheFuture = (diffTimeSpan.Ticks > 0) && (diffTimeSpan.TotalMilliseconds >= deltaInMilliSeconds);

			return isInTheFuture;
		}

		/// <summary>
		/// Disregards the time part and checks if this is in the past
		/// </summary>
		[XmlIgnore]
		public bool IsInThePastDatePartOnly
		{
			get
			{
				return !IsToday && this < Today;
			}
		}

		/// <summary>
		/// Disregards the time part and checks if this is in the future
		/// </summary>
		[XmlIgnore]
		public bool IsInTheFutureDatePartOnly
		{
			get
			{
				return !IsToday && this > Today;
			}
		}

		#endregion

		[XmlIgnore]
		public bool IsValidSmallDateTime
		{
			get { return MinSmallDateTimeValue <= this && this <= MaxSmallDateTimeValue; }
		}

		[XmlIgnore]
		public bool IsValidSqlDateTime
		{
			get { return SqlDateTime.MinValue.Value <= this && this <= SqlDateTime.MaxValue.Value; }
		}

		[XmlIgnore]
		public TimeSpan TimeSpan6MonthsFromStartOfYear
		{
			get
			{
				if (Month < 6 && Year > 1)
				{
					return this - new ZDateTime(Year, 1, 1);
				}
				else
				{
					return this.AddYears(DefaultDurationEpoch.Year - this.Year) - new ZDateTime(DefaultDurationEpoch.Year + 1, 1, 1);
				}
			}
		}

		#region Static

		/// <summary>
		/// The empty ZDateTime.
		/// </summary>
		public static readonly ZDateTime Empty;

		/// <summary>
		/// The invalid (but not empty) ZDateTime.
		/// </summary>
		static readonly ZDateTime invalid = new ZDateTime(DateTime.MinValue);
		public static ZDateTime Invalid
		{
			get
			{
				return invalid;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Literal string")]
		public const string InvalidLiteral = "<Invalid>";

		/// <summary>
		/// The earliest date that can be stored in a smalldatetime field.
		/// </summary>
		public static readonly ZDateTime MinSmallDateTimeValue = new ZDateTime(1900, 1, 1, 0, 0, 0);

		/// <summary>
		/// The latest date that can be stored in a smalldatetime field.
		/// </summary>
		public static readonly ZDateTime MaxSmallDateTimeValue = new ZDateTime(2079, 6, 6, 23, 59, 29);

		/// <summary>
		/// The default kind
		/// </summary>
		public static readonly DateTimeKind DefaultKind = DateTimeKind.Local;

		/// <summary>
		/// The current local date and time.
		/// </summary>
		public static ZDateTime Now
		{
			[DebuggerStepThrough]
			get
			{
				var zdt = new ZDateTime(ObjectCache.DateTimeProvider.CurrentLocalDateTime, DateTimeKind.Local);
				return zdt;
			}
		}

		/// <summary>
		/// The current local date and time, as a small date time.
		/// </summary>
		public static ZDateTime SmallDateTimeNow
		{
			[DebuggerStepThrough]
			get
			{
				var zdt = new ZDateTime(ObjectCache.DateTimeProvider.CurrentLocalDateTime, DateTimeKind.Local);
				return zdt.ToSmallDateTimeFloor();
			}
		}

		/// <summary>
		/// The current UTC date and time.
		/// </summary>
		public static ZDateTime UtcNow
		{
			[DebuggerStepThrough]
			get
			{
				var zdt = new ZDateTime(ObjectCache.DateTimeProvider.CurrentUtcDateTime, DateTimeKind.Utc);
				return zdt;
			}
		}

		/// <summary>
		/// The current UTC date and time.
		/// </summary>
		public static ZDateTime SmallDateTimeUtcNow
		{
			[DebuggerStepThrough]
			get
			{
				var zdt = new ZDateTime(ObjectCache.DateTimeProvider.CurrentUtcDateTime, DateTimeKind.Utc);
				return zdt.ToSmallDateTimeFloor();
			}
		}

		/// <summary>
		/// The current local date.
		/// </summary>
		public static ZDateTime Today
		{
			[DebuggerStepThrough]
			get
			{
				var zdt = new ZDateTime(ObjectCache.DateTimeProvider.CurrentLocalDate, DateTimeKind.Local);
				return zdt;
			}
		}

		/// <summary>
		/// The current UTC date.
		/// </summary>
		public static ZDateTime UtcToday
		{
			[DebuggerStepThrough]
			get
			{
				var zdt = new ZDateTime(ObjectCache.DateTimeProvider.CurrentUtcDate, DateTimeKind.Utc);
				return zdt;
			}
		}

		/// <summary>
		/// Maximum value for a SmallDateTime
		/// </summary>
		public static ZDateTime MaxSmallDateTime
		{
			get
			{
				return new ZDateTime(2079, 06, 06);
			}
		}

		public static ZDateTime MaxSmallDateTimeUtc
		{
			get
			{
				return new ZDateTime(2079, 06, 06, DateTimeKind.Utc);
			}
		}

		public static ZDateTime GetValidSmallDateTime(ZDateTime date)
		{
			var result = date;

			if (date < MinSmallDateTimeValue)
			{
				result = MinSmallDateTimeValue;
			}
			else if (date > MaxSmallDateTimeValue)
			{
				result = MaxSmallDateTimeValue;
			}

			return result;
		}

		public static ZDateTime FromSqlFormat(ZString dateTime)
		{
			return ObjectCache.DateTimeProvider.FromSqlDateTime(dateTime);
		}

		/// <summary>
		/// Converts a 2-digit date to a 4-digit date.
		/// </summary>
		/// <param name="year"></param>
		/// <returns></returns>
		public static int Get4DigitYearFrom2DigitYear(int year)
		{
			if (!(year >= 0 && year <= 99))
			{
				throw new ArgumentOutOfRangeException(nameof(year), "The value provided is not a 2-digit year.");
			}

			return (year < 50) ? 2000 + year : 1900 + year;
		}

		/// <summary>
		/// Returns copy of input dateTime without seconds or microseconds.
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static ZDateTime TruncateSeconds(ZDateTime dateTime)
		{
			return Truncate(dateTime, TimeSpan.TicksPerMinute);
		}

		/// <summary>
		/// Returns copy of input dateTime rounding by a resolution
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static ZDateTime Truncate(ZDateTime dateTime, long resolution)
		{
			if (!dateTime.IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot truncate an invalid ZDateTime.");
			}

			if (dateTime.Year <= 1)
			{
				throw new ArgumentException("ZDateTime to be truncated must a have a Year > 1 to prevent creating a invalid ZDateTime.", nameof(dateTime));
			}

			return dateTime.AddTicks(-(dateTime.Ticks % resolution));
		}

		/// <summary>
		/// Return copy of input dateTime without hours, minutes, seconds or microseconds.
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		public static ZDateTime TruncateToDay(ZDateTime dateTime)
		{
			if (!dateTime.IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot truncate an invalid ZDateTime.");
			}

			if (!(dateTime.Year > 1 || dateTime.Month > 1 || dateTime.Day > 1))
			{
				throw new ArgumentException("ZDateTime to be truncated must a have a Year, Month or Day greater than 1 to prevent creating a invalid ZDateTime.", nameof(dateTime));
			}

			return new ZDateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);
		}

		#endregion

		#region Object Overrides

		/// <summary>
		/// Is this instance equal to the specified object?
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		public override bool Equals(object obj)
		{
			return (obj is ZDateTime time && time == this) ||
				(obj is DateTime time1 && time1 == ValueSafe);
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
		/// Converts the ZDateTime to a DateTime. WARNING - an exception will be thrown if ToDateTime() is invoked on an empty or invalid ZDateTime!".
		/// </summary>
		[DebuggerStepThrough]
		public DateTime ToDateTime()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot convert an Empty or Invalid ZDateTime to a DateTime!");
			}

			return ValueSafe;
		}

		public TimeSpan ToTimeSpan()
		{
			var startOfYear = new ZDateTime(Year, 1, 1);
			return new TimeSpan((this - startOfYear).Days, Hour, Minute, Second, Millisecond);
		}

		/// <summary>
		/// Rounds the ZDateTime to a SmallDateTime with SQL Server behaviour (round to the nearest minute).
		/// Note that when we save a business object to the database with smalldatetime properties we _floor_ rather than let the database handle it.
		/// (see ZSqlParameter.cs TruncateIfNecessary() )
		/// </summary>
		public ZDateTime ToSmallDateTime()
		{
			var result = ToSmallDateTimeFloor();
			if (Second >= 30 || (Second == 29 && Millisecond == 999)) //with SQL Server's level of precision, 29.999 actually rounds up to 30 (20.998 would round down to 29.997)
			{
				result = result.AddMinutes(1);
			}
			return result;
		}

		/// <summary>
		/// Rounds the ZDateTime to a SmallDateTime the same way we would if we put it into the database (see ZSqlParameter.cs TruncateIfNecessary() ).
		/// </summary>
		public ZDateTime ToSmallDateTimeFloor()
		{
			return AddTicks(-(Ticks % TimeSpan.TicksPerMinute));
		}

		[DebuggerStepThrough]
		public static implicit operator ZDateTime(DateTime value)
		{
			return new ZDateTime(value, value.Kind);
		}

		[DebuggerStepThrough]
		public static implicit operator ZDateTime(TimeSpan value)
		{
			return new ZDateTime(DefaultDurationEpoch.Add(value));
		}

		#endregion

		#region Operator Overloads: ZDateTime & ZDateTime

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static TimeSpan operator -(ZDateTime lhs, ZDateTime rhs)
		{
			if (!lhs.IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot subtract empty ZDateTime (lhs).");
			}

			if (!rhs.IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot subtract empty ZDateTime (rhs).");
			}

			return lhs.ToDateTime() - rhs.ToDateTime();
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZDateTime lhs, ZDateTime rhs)
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
		public static bool operator !=(ZDateTime lhs, ZDateTime rhs)
		{
			return !(lhs == rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(ZDateTime lhs, ZDateTime rhs)
		{
			return lhs.IsValid && rhs.IsValid && lhs.ValueSafe < rhs.ValueSafe;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(ZDateTime lhs, ZDateTime rhs)
		{
			return lhs.IsValid && rhs.IsValid && lhs.ValueSafe > rhs.ValueSafe;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(ZDateTime lhs, ZDateTime rhs)
		{
			return lhs < rhs || lhs == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(ZDateTime lhs, ZDateTime rhs)
		{
			return lhs > rhs || lhs == rhs;
		}

		#endregion

		#region Operator Overloads: ZDateTime & DateTime

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZDateTime lhs, DateTime rhs)
		{
			return lhs == new ZDateTime(rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(DateTime lhs, ZDateTime rhs)
		{
			return new ZDateTime(lhs) == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZDateTime lhs, DateTime rhs)
		{
			return !(lhs == rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(DateTime lhs, ZDateTime rhs)
		{
			return !(lhs == rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(ZDateTime lhs, DateTime rhs)
		{
			return lhs < new ZDateTime(rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(DateTime lhs, ZDateTime rhs)
		{
			return new ZDateTime(lhs) < rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(ZDateTime lhs, DateTime rhs)
		{
			return lhs > new ZDateTime(rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(DateTime lhs, ZDateTime rhs)
		{
			return new ZDateTime(lhs) > rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(ZDateTime lhs, DateTime rhs)
		{
			return lhs < rhs || lhs == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(DateTime lhs, ZDateTime rhs)
		{
			return lhs < rhs || lhs == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(ZDateTime lhs, DateTime rhs)
		{
			return lhs > rhs || lhs == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(DateTime lhs, ZDateTime rhs)
		{
			return lhs > rhs || lhs == rhs;
		}

		#endregion

		#region Operator Overloads: ZDateTime & TimeSpan

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static ZDateTime operator +(ZDateTime lhs, TimeSpan rhs)
		{
			if (!lhs.IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot convert an Empty or Invalid ZDateTime to a DateTime!");
			}

			return lhs.ToDateTime() + rhs;
		}

		#endregion

		#region TryParse

		/// <summary>
		/// Can the given string be converted to a datetime? If so, Result contains the ZDateTime resulting from the conversion
		/// </summary>
		/// <param name="value">A string containing a datetime string to convert.</param>
		/// <param name="result">Returned as the datetime equivalent of the given string, else invalid if it cannot be converted.</param>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ISO")]
		public static bool TryParseISO8601Date(string value, out ZDateTime result)
		{
			bool success;
			if (string.IsNullOrEmpty(value))
			{
				result = Empty;
				success = true;
			}
			else
			{
				success = DateTime.TryParse(value, out var dt);
				result = success ? dt : Invalid;
			}

			return success;
		}

		public static ZDateTime ParseISO8601DateSafe(string value)
		{
			TryParseISO8601Date(value, out var dateValue);
			return dateValue;
		}

		/// <summary>
		/// Can the given string in a certain format be converted to a datetime? If so, Result contains the ZDateTime resulting from the conversion
		/// </summary>
		/// <param name="value">A string containing a datetime string to convert.</param>
		/// <param name="result">Returned as the datetime equivalent of the given string, else invalid if it cannot be converted.</param>
		/// <param name="format">The format string to apply during parsing</param>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
		public static bool TryParseExact(string value, out ZDateTime result, string format)
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
				if (DateTime.TryParseExact(value, format, ObjectCache.CultureProvider.Culture, DateTimeStyles.None, out var parsedResult))
				{
					result = GetParsedTimeOnlyWithAdjustedDate(parsedResult, format);
					success = true;
				}
			}
			return success;
		}

		/// <summary>
		/// DateTime.ParseExact uses the local machine date to complete the DateTime in a Time only format parsing.
		/// This date might be different from the date in the current ZDateTime time zone, Hence it has to be adjusted here.
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String format")]
		static DateTime GetParsedTimeOnlyWithAdjustedDate(DateTime parsedResult, string format)
		{
			Argument.NotNull(format, nameof(format));

			DateTime result = parsedResult;

			if (!format.Contains("y"))
			{
				if (format.Contains("M") || (format.Contains("d") && format.Trim() != "d"))
				{
					int year = Now.Year;
					result = result.AddYears(year - result.Year);
				}
				else if (!format.Contains("d"))
				{
					TimeSpan parsedTime = parsedResult.TimeOfDay;
					result = Today.ToDateTime().Add(parsedTime);
				}
			}

			return result;
		}

		/// <summary>
		/// Can the given string be converted to a datetime? If so, Result contains the ZDateTime without the timezone conversion
		/// </summary>
		/// <param name="value">A string containing a datetime string to convert.</param>
		/// <param name="cultureInfo">CultureInfo used for parsing</param>
		/// <param name="result">Returned as the datetime equivalent of the given string, else invalid if it cannot be converted.</param>
		public static bool TryParseIgnoreTimezone(string value, CultureInfo cultureInfo, out ZDateTime result)
		{
			bool success;
			if (string.IsNullOrEmpty(value))
			{
				result = Empty;
				success = true;
			}
			else
			{
				success = DateTimeOffset.TryParse(value, cultureInfo, DateTimeStyles.None, out var dto);
				result = success ? dto.DateTime : Invalid;
			}

			return success;
		}

		#endregion

		#region Date/time Offset Conversion

		public double GetMinutesFromDateTimeSpan()
		{
			if (IsValid)
			{
				var start = new DateTime(Year, 1, 1);
				var span = ValueSafe - start;

				return span.TotalMinutes;
			}
			else
			{
				return 0;
			}
		}

		#endregion

		#region ISO Week Date

		/// <summary>
		/// Creates a new ZDateTime from an ISO Week-formatted string (eg "WC104", "WE104"). Returns ZDateTime.Invalid if the ISO Weel-formatted string is not valid.
		/// </summary>
		public static ZDateTime FromIsoWeekDateString(ZString isoText)
		{
			ZDateTime result = Invalid;

			bool isWeekCommencing = isoText.StartsWith("WC");
			bool isWeekEnding = isoText.StartsWith("WE");

			if ((isWeekCommencing || isWeekEnding) && (isoText.Length == 5 /* "WC104" */ || isoText.Length == 6) /* "WC2504" */)
			{
				ZString weekAndYear = isoText.SubstringSafe(2);

				if (weekAndYear.IsNumbersOnlyOrEmpty)
				{
					ZString yearString = isoText.SubstringSafe(isoText.Length - 2); // last 2 digits
					ZString weekString = isoText.SubstringSafe(2, isoText.Length - yearString.Length - 2); // inbetween "WC" and year

					int twoDigitYear = int.Parse(yearString);

					int year = Get4DigitYearFrom2DigitYear(twoDigitYear);
					int week = int.Parse(weekString);

					result = isWeekCommencing ? GetIsoWeekCommencing(week, year) : GetIsoWeekEnding(week, year);
				}
			}

			return result;
		}

		static ZDateTime GetIsoWeekCommencing(int week, int year)
		{
			ZDateTime result = Invalid;

			if (week >= 1 && week <= 53)
			{
				ZDateTime firstDayOfYear = new ZDateTime(year, 01, 01);
				int weeksToAdd = week;

				if (firstDayOfYear.DayOfWeek >= DayOfWeek.Monday && firstDayOfYear.DayOfWeek <= DayOfWeek.Thursday)
				{
					weeksToAdd--; // move to previous week
				}

				// Sunday is the first day of the week in the enum, but in the ISO Week standard Monday is the first day of the week
				int firstDayOfYearValue = (int)((firstDayOfYear.DayOfWeek == DayOfWeek.Sunday) ? DayOfWeek.Saturday + 1 : firstDayOfYear.DayOfWeek);

				int daysToGoBack = firstDayOfYearValue - (int)DayOfWeek.Monday; // locate the monday of this week
				ZDateTime monday = firstDayOfYear.AddDays(weeksToAdd * 7 - daysToGoBack);

				bool week53ThursdayIsInFollowingYear = (monday.AddDays(3).Year > year);
				if (!week53ThursdayIsInFollowingYear)
				{
					result = monday;
				}
			}
			return result;
		}

		static ZDateTime GetIsoWeekEnding(int week, int year)
		{
			ZDateTime result = GetIsoWeekCommencing(week, year);

			if (result.IsValid)
			{
				result = result.AddDays(7).AddSeconds(-1);
			}

			return result;
		}

		#endregion

		#region Wrapping the internal DateTime

		#region ToString

		/// <summary>
		/// The default string representation of this instance.
		/// </summary>

		[SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Should be the only place in the system that uses date format string")]
		public const string ShortDateFormat = "dd-MMM-yy";// Should be the only place in the system that uses date format string
		public const string ISO8601ShortDateFormat = "yyyy-MM-dd";// Should be the only place in the system that uses date format string
		[SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Should be the only place in the system that uses date format string")]
		public const string ShortTimeFormat = "HH:mm";// Should be the only place in the system that uses date format string
		[SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Should be the only place in the system that uses date format string")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Should be the only place in the system that uses date format string")]
		public const string LongTimeFormat = "dd-MMM-yy HH:mm";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Should be the only place in the system that uses date format string")]
		public const string BestReadableDateFormat = "dd MMM yyyy";
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Should be the only place in the system that uses date format string")]
		public const string BestReadableDateTimeFormat = "dd MMM yyyy HH:mm";

		public override string ToString()
		{
			return ToString(null);
		}

		public string ToShortDateString()
		{
			return ToString(ShortDateFormat);
		}

		public string ToISO8601ShortDateString()
		{
			return ToString(ISO8601ShortDateFormat, CultureInfo.InvariantCulture);
		}

		public string ToShortTimeString()
		{
			return ToString(ShortTimeFormat);
		}

		public string ToLongTimeString()
		{
			return ToString(LongTimeFormat);
		}

		public string ToBestReadableDateTimeString()
		{
			return ToString(BestReadableDateTimeFormat, CultureInfo.InvariantCulture);
		}

		public string ToBestReadableDateString()
		{
			return ToString(BestReadableDateFormat, CultureInfo.InvariantCulture);
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
				result = ObjectCache.DateTimeProvider.Format(ValueSafe, format);
			}

			return result;
		}

		/// <summary>
		/// The string representation of this instance in ISO8601 format.
		/// </summary>
		[SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ISO")]
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String format")]
		public string ToISO8601String()
		{
			return ToString("s");
		}

		#endregion

		/// <summary>
		/// The OLE Automation date equivalent to the value of this instance.
		/// </summary>
		public double ToOleAutomationDate()
		{
			return ValueSafe.ToOADate();
		}

		/// <summary>
		/// The date component of this instance.
		/// </summary>
		[XmlIgnore]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Member name")]
		public ZDate Date
		{
			get
			{
				if (IsEmpty)
				{
					return ZDate.Empty;
				}
				else if (!IsValid)
				{
					return ZDate.Invalid;
				}
				else
				{
					EnsureValidValueFor("Date");
					return new ZDate(ValueSafe.Date);
				}
			}
		}

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

		/// <summary>
		/// The time of day for this instance.
		/// </summary>
		[XmlIgnore]
		public TimeSpan TimeOfDay
		{
			get
			{
				return ValueSafe.TimeOfDay;
			}
		}

		/// <summary>
		/// The milliseconds component for this instance.
		/// </summary>
		[XmlIgnore]
		public int Millisecond
		{
			get
			{
				return ValueSafe.Millisecond;
			}
		}

		/// <summary>
		/// The seconds component for this instance.
		/// </summary>
		[XmlIgnore]
		public int Second
		{
			get
			{
				return ValueSafe.Second;
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
				return ValueSafe.Minute;
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
				return ValueSafe.Hour;
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
				return ValueSafe.Day;
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
				return ValueSafe.DayOfWeek;
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
				return ValueSafe.DayOfYear;
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
				return ValueSafe.Month;
			}
		}

		/// <summary>
		/// The year component for this instance.
		/// </summary>
		[XmlIgnore]
		public int Year
		{
			get
			{
				return ValueSafe.Year;
			}
		}

		/// <summary>
		/// The value of this instance plus the specified time span.
		/// </summary>
		public ZDateTime Add(TimeSpan ticks)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add timespans to an invalid date");
			}

			var returnVal = new ZDateTime(ValueSafe.Add(ticks));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of ticks added resulted in the minimum DateTime and hence an invalid ZDateTime");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of 100-nanosecond ticks.
		/// </summary>
		public ZDateTime AddTicks(long ticks)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add ticks to an invalid date");
			}

			var returnVal = new ZDateTime(ValueSafe.AddTicks(ticks));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of ticks added resulted in the minimum DateTime and hence an invalid ZDateTime");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of milliseconds.
		/// </summary>
		public ZDateTime AddMilliseconds(int milliseconds)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add miliseconds to an invalid date");
			}

			var returnVal = new ZDateTime(ValueSafe.AddMilliseconds(milliseconds));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of milliseconds added resulted in the minimum DateTime and hence an invalid ZDateTime");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of seconds.
		/// </summary>
		public ZDateTime AddSeconds(int seconds)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add seconds to an invalid date");
			}

			var returnVal = new ZDateTime(ValueSafe.AddSeconds(seconds));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of seconds added resulted in the minimum DateTime and hence an invalid ZDateTime");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of minutes.
		/// </summary>
		public ZDateTime AddMinutes(int minutes)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add minutes to an invalid date");
			}

			var returnVal = new ZDateTime(ValueSafe.AddMinutes(minutes));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of minutes added resulted in the minimum DateTime and hence an invalid ZDateTime");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of hours.
		/// </summary>
		public ZDateTime AddHours(int hours)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add hours to an invalid date.");
			}

			var returnVal = new ZDateTime(ValueSafe.AddHours(hours));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of hours added resulted in the minimum DateTime and hence an invalid ZDateTime");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of hours.
		/// </summary>
		public ZDateTime AddHours(ZDecimal hours)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add hours to an invalid date (ZDecimal).");
			}

			decimal decimalHours = (decimal)hours;
			var returnVal = new ZDateTime(ValueSafe.AddHours((double)decimalHours));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of hours added resulted in the minimum DateTime and hence an invalid ZDateTime");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of days.
		/// </summary>
		public ZDateTime AddDays(int days)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add days to an invalid date");
			}

			var returnVal = new ZDateTime(ValueSafe.AddDays(days));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of days added resulted in the minimum DateTime and hence an invalid ZDateTime");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of months.
		/// </summary>
		public ZDateTime AddMonths(int months)
		{
			if (!(months >= -120000 && months <= 120000))
			{
				throw new ArgumentException("An invalid number of months to add.", nameof(months));
			}

			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add months to an invalid date");
			}

			var returnVal = new ZDateTime(ValueSafe.AddMonths(months));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of months added resulted in the minimum DateTime and hence an invalid ZDateTime");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of years.
		/// </summary>
		public ZDateTime AddYears(int years)
		{
			if (!(years >= -10000 && years <= 10000))
			{
				throw new ArgumentException("An invalid number of years to add", nameof(years));
			}

			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add years to an invalid date");
			}

			var returnVal = new ZDateTime(ValueSafe.AddYears(years));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of months added resulted in the minimum DateTime and hence an invalid ZDateTime");
			}

			return returnVal;
		}

		public ZDateTime EndOfDay()
		{
			if (!(IsEmpty || IsValid))
			{
				throw new InvalidOperationException();
			}

			if (IsValid)
			{
				return new ZDateTime(ValueSafe.Year, ValueSafe.Month, ValueSafe.Day, 23, 59, 59);
			}
			else
			{
				return Empty;
			}
		}

		#endregion

		#region Kind

		[XmlIgnore]
		public DateTimeKind Kind
		{
			get { return fValueUnsafe.Kind; }
		}

		#endregion

		#region XmlSerializedValue

		[XmlText]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public DateTime XmlSerializedValue
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZDateTimeException();
				}

				return ToDateTime();
			}
			set { this = value; }
		}

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (obj is IZTypeInternals zTypeInternals)
			{
				obj = zTypeInternals.GetValueForLogicalDataLayer(false);
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
			get { return typeof(DateTime); }
		}

		[XmlIgnore]
		public bool IsDefault
		{
			get { return this == (ZDateTime)Default; }
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
		/// Is the ZDateTime empty - that is, not ZDateTime.Empty?
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
		/// Returns true if the ZDateTime is valid - that is, not ZDateTime.Empty and has a valid value
		/// IsValid must be tested before casting a ZDateTime to a DateTime as an exception will be throw if the ZDateTime is not valid.
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

		#region IZDate Members

		IZDate IZDate.Invalid => Invalid;

		IZDate IZDate.Now => Now;

		IZDate IZDate.Today => Today;

		int IZDate.Year => Year;

		string IZDate.SqlFormat => SqlFormat;

		IZDate IZDate.FirstDayOfLastCalendarYear => new ZDateTime(Now.Year - 1, 1, 1);

		IZDate IZDate.LastDayOfLastCalendarYear => new ZDateTime(Now.Year - 1, 12, 31);

		IZDate IZDate.AddDays(int days)
		{
			return AddDays(days);
		}

		IZDate IZDate.AddMonths(int months)
		{
			return AddMonths(months);
		}

		IZDate IZDate.AddYears(int years)
		{
			return AddYears(years);
		}

		string IZDate.ToISO8601String()
		{
			return ToISO8601String();
		}

		string IZDate.ToString(string format, IFormatProvider formatProvider)
		{
			return ToString(format, formatProvider);
		}

		bool IZDate.TryParse(string unparsedValue, out IZDate result)
		{
			var sucesss = TryParseISO8601Date(unparsedValue, out var date);
			result = date;
			return sucesss;
		}

		object IZDate.ToBaseDate()
		{
			return this.ToDateTime();
		}

		IZDate IZDate.EndOfDay()
		{
			return EndOfDay();
		}

		#endregion

		#region IZTypeInternals Members

		[DebuggerStepThrough]
		object IZTypeInternals.GetValueForLogicalDataLayer(bool isNullable)
		{
			return IsEmpty && isNullable ? DBNull.Value : ValueSafe;
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

		#region Brett's Birthday

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Bretts")]
		public static ZDateTime BrettsBirthday
		{
			get { return new ZDateTime(1971, 9, 18); }
		}

		#endregion

		#region Implementation

		DateTime ValueSafe
		{
			[DebuggerStepThrough]
			get
			{
				return fValueUnsafe;
			}
		}

		readonly DateTime fValueUnsafe;
		readonly bool isNotEmpty;
		readonly bool isValid;

		DateTime EnsureValidValueFor(string memberName)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot access " + /*memberName + */" member of an invalid or empty ZDateTime.");
			}

			return ValueSafe;
		}
		#endregion

		#region Operations With Dates

		public static bool Overlaps(ZDateTime aStart, ZDateTime aEnd, ZDateTime bStart, ZDateTime bEnd)
		{
			var aStartsBeforeBEnds = aStart.IsEmpty || bEnd.IsEmpty || aStart <= bEnd;
			var bStartsBeforeAEnds = bStart.IsEmpty || aEnd.IsEmpty || bStart <= aEnd;

			return aStartsBeforeBEnds && bStartsBeforeAEnds;
		}

		public static bool DifferentMinutes(ZDateTime a, ZDateTime b)
		{
			if (!a.IsValid || !b.IsValid)
			{
				return a != b;
			}

			var lhs = a.AddSeconds(-a.Second).AddMilliseconds(-a.Millisecond);
			var rhs = b.AddSeconds(-b.Second).AddMilliseconds(-b.Millisecond);

			return Math.Abs((lhs - rhs).TotalMinutes) >= 1;
		}

		#endregion

		#region A Temporary Hack

		/// <summary>
		/// This is a temporary hack so that this API matches the ZDateTimeOffset API so that converting between
		/// ZDateTimeOffset and ZDateTime for many properties of ProcessTasks is possible.
		/// </summary>
		/// <returns></returns>
		public ZDateTime ToZDateTime() => this;

		#endregion

		#region ToDateTimeOffset

		/// <summary>
		/// Creates a ZDateTimeOffset based on this DateTime. The behaviour depends on the Kind property:
		/// If Kind is Utc, then the resulting DateTimeOffset will have no offset and be considered Utc as well.
		/// If Kind is Local or Unspecified, the DateTime will be considered to be a local time, and its offset will be calculated based on the current Branch's UNLOCO's Time Zone (including DST information).
		/// </summary>
		public ZDateTimeOffset ToOffset() => new ZDateTimeOffset(this);

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
			TypeConverter converter = ZDateTimeTypeConverter.Instance;
			if (converter != null && converter.CanConvertTo(conversionType))
			{
				return converter.ConvertTo(this, conversionType);
			}
			else
			{
				throw new InvalidCastException("Converting from ZDateTime to " + conversionType + " not supported");
			}
		}

		#endregion

		#region UTC Handling

		/// <summary>
		/// Creates a ZDateTimeOffset from this UTC date time. (The Kind is ignored - it will be treated as UTC regardless.) It will be converted to a local date time and offset based on the current Branch's UNLOCO's Time Zone (including DST information).
		/// </summary>
		public ZDateTimeOffset UtcToDateTimeOffset()
		{
			var dateTime = ToDateTime();
			return new ZDateTimeOffset(ObjectCache.DateTimeProvider.GetLocalTimeFromUtc(dateTime), ObjectCache.DateTimeProvider.GetUtcOffsetBasedOnUtc(dateTime));
		}
		#endregion UTC Handling

		#region Duration

		// Default duration epoch ticks starting from the year 1900
		const long DefaultDurationEpochTicks_1900 = 599_266_080_000_000_000;

		// Default negatable offset epoch ticks starting from the year 2000
		const long DefaultNegatableDurationEpochTicks_2000 = 630_822_816_000_000_000;

		/// <summary>
		/// Default epoch for duration calculations, representing the start of 1900. The time is stored as Unspecified.
		/// This serves as a baseline for duration related properties.
		/// </summary>
		public static ZDateTime DefaultDurationEpoch => new DateTime(DefaultDurationEpochTicks_1900, DateTimeKind.Unspecified);

		/// <summary> 
		/// Default epoch for negative duration calculations, representing the start of 2000. The time is stored as Unspecified.
		/// This is used for specific duration properties that can have negative values.
		/// </summary>
		public static ZDateTime DefaultNegatableDurationEpoch => new DateTime(DefaultNegatableDurationEpochTicks_2000, DateTimeKind.Unspecified);

		#endregion
	}

	[Serializable]
	public sealed class OperationOnInvalidZDateTimeException : OperationOnInvalidZTypeException
	{
		public OperationOnInvalidZDateTimeException()
		{
		}

		public OperationOnInvalidZDateTimeException(string message)
			: base(message)
		{
		}

		public OperationOnInvalidZDateTimeException(string message, Exception ex)
			: base(message, ex)
		{
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		OperationOnInvalidZDateTimeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}

	[Serializable]
	public sealed class InvalidZDateTimeResultException : Exception
	{
		public InvalidZDateTimeResultException()
		{
		}

		public InvalidZDateTimeResultException(string message)
			: base(message)
		{
		}

		public InvalidZDateTimeResultException(string message, Exception ex)
			: base(message, ex)
		{
		}

#if NETFRAMEWORK
		[Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
		InvalidZDateTimeResultException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
			: base(info, context)
		{
		}
#endif
	}
}
