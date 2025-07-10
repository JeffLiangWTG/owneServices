using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using CargoWise.Common;

namespace CargoWise.Types
{
	[TypeConverter(typeof(ZDateTimeOffsetTypeConverter))]
	[DebuggerDisplay("{DebuggerDisplay}")]
	[WTG.StaticAnalysis.Annotation.Immutable]
	[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "If ZDateTime.fValueUnsafe is fine, this is fine too")]
	public struct ZDateTimeOffset : IZDate, IZTypeInternals, IFormattable, IConvertible
	{
		[XmlIgnore]
		[SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Debugging")]
		public string DebuggerDisplay
		{
			get { return IsEmpty ? "Empty" : (IsValid ? SqlFormat.ToString() : "Invalid"); }
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(object value)
		{
			if (value == null)
			{
				this = Empty;
			}
			else if (value is DateTimeOffset offset)
			{
				this = new ZDateTimeOffset(offset);
			}
			else if (value is ZDateTime time)
			{
				this = new ZDateTimeOffset(time);
			}
			else if (value is DateTime time1)
			{
				this = new ZDateTimeOffset(time1);
			}
			else
			{
				TypeConverter converter = ZDateTimeOffsetTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					var converted = converter.ConvertFrom(value);
					this = (ZDateTimeOffset)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZDateTimeOffset), value);
				}
			}
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(object value, DateTimeKind kind)
		{
			if (value == null)
			{
				this = Empty;
			}
			else if (value is DateTimeOffset offset)
			{
				this = new ZDateTimeOffset(offset);
			}
			else if (value is ZDateTime time)
			{
				this = new ZDateTimeOffset(time, kind);
			}
			else if (value is DateTime time1)
			{
				this = new ZDateTimeOffset(time1, kind);
			}
			else
			{
				TypeConverter converter = ZDateTimeOffsetTypeConverter.Instance;
				if (converter.CanConvertFrom(value.GetType()))
				{
					var converted = converter.ConvertFrom(value);
					this = (ZDateTimeOffset)converted;
				}
				else
				{
					throw new ZTypeValueException(typeof(ZDateTimeOffset), value);
				}
			}
		}

		/// <summary>
		/// Creates a ZDateTimeOffset based on provided DateTime. The behaviour depends on the Kind property:
		/// If Kind is Utc, then the resulting DateTimeOffset will have no offset and be considered Utc as well.
		/// If Kind is Local or Unspecified, the DateTime will be considered to be a local time, and its offset will be calculated based on the current Branch's UNLOCO's Time Zone (including DST information).
		///
		/// If this is not the behaviour you want, use one of the more specific constructors.
		///
		/// Also, if you pass a DateTime equal to MinValue or MaxValue (with ANY Kind), it will be converted to Invalid.
		/// Finally note that if datetime + offset is larger than DateTime.MaxValue or smaller than DateTime.MinValue, ArgumentOutOfRangeException will be thrown.
		/// </summary>
		[DebuggerStepThrough]
		public ZDateTimeOffset(DateTime value)
		{
			//Four cases to handle:
			//1) Equal to DateTime.MinValue or DateTime.MaxValue - give expected semantics
			//2) Utc - just pass through
			//3) Local - change to unspecified, create explicit offset
			//4) Unspecified - create explicit offset

			if (value.Ticks == DateTimeOffset.MinValue.Ticks || value.Ticks == DateTimeOffset.MaxValue.Ticks)
			{
				this = Invalid;
			}
			else if (value.Kind == DateTimeKind.Utc)
			{
				fValueUnsafe = new DateTimeOffset(value);
			}
			else if (value.Kind == DateTimeKind.Local)
			{
				fValueUnsafe = new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), ObjectCache.DateTimeProvider.GetUtcOffsetBasedOnLocal(value));
			}
			else //Unspecified
			{
				fValueUnsafe = new DateTimeOffset(value, ObjectCache.DateTimeProvider.GetUtcOffsetBasedOnLocal(value));
			}

			isNotEmpty = true;
			isValid = fValueUnsafe.Ticks != DateTimeOffset.MinValue.Ticks;
		}

		/// <summary>
		/// Creates a ZDateTimeOffset based on provided DateTime. The behaviour depends on the Kind property:
		/// If Kind is Utc, then the resulting DateTimeOffset will have no offset and be considered Utc as well.
		/// If Kind is Local or Unspecified, the DateTime will be considered to be a local time, and its offset will be calculated based on the current Branch's UNLOCO's Time Zone (including DST information).
		///
		/// If this is not the behaviour you want, use one of the more specific constructors.
		///
		/// Also, if you pass a DateTime equal to MinValue or MaxValue (with ANY Kind), it will be converted to Invalid.
		/// Finally note that if datetime + offset is larger than DateTime.MaxValue or smaller than DateTime.MinValue, ArgumentOutOfRangeException will be thrown.
		/// </summary>
		[DebuggerStepThrough]
		public ZDateTimeOffset(ZDateTime value)
		{
			if (value.IsEmpty)
			{
				this = Empty;
			}
			else if (!value.IsValid)
			{
				this = Invalid;
			}
			else
			{
				this = new ZDateTimeOffset(value.ToDateTime());

				isNotEmpty = true;
				isValid = fValueUnsafe.Ticks != DateTimeOffset.MinValue.Ticks;
			}
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(DateTime value, DateTimeKind kind)
		{
			this = new ZDateTimeOffset(DateTime.SpecifyKind(value, kind));

			isNotEmpty = true;
			isValid = fValueUnsafe.Ticks != DateTimeOffset.MinValue.Ticks;
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(ZDateTime value, DateTimeKind kind)
		{
			if (value.IsEmpty)
			{
				this = Empty;
			}
			else if (!value.IsValid)
			{
				this = Invalid;
			}
			else
			{
				this = new ZDateTimeOffset(DateTime.SpecifyKind(value.ToDateTime(), kind));

				isNotEmpty = true;
				isValid = fValueUnsafe.Ticks != DateTimeOffset.MinValue.Ticks;
			}
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(DateTime value, TimeSpan offset)
		{
			//TODO: What should the behaviour of invalid offsets (greater than +/- 14 hours) be? Clamp? Return ZDateTimeOffset.Invalid? Throw an exception? (currently exception. whatever the decision is, also change other constructors!)

			//Four cases to handle:
			//1) Equal to DateTime.MinValue or DateTime.MaxValue - give expected semantics
			//2) Utc - just pass through
			//3) Local - change to unspecified, use offset as provided
			//4) Unspecified - use offset as provided

			if (value.Ticks == DateTimeOffset.MinValue.Ticks || value.Ticks == DateTimeOffset.MaxValue.Ticks)
			{
				this = Invalid;
			}
			else if (value.Kind == DateTimeKind.Local)
			{
				fValueUnsafe = new DateTimeOffset(DateTime.SpecifyKind(value, DateTimeKind.Unspecified), offset);
			}
			else
			{
				fValueUnsafe = new DateTimeOffset(value, offset); //This will throw an exception if value.Kind is Utc and offset is not 0. This is intended - we don't know which one the user really wants.
			}

			isNotEmpty = true;
			isValid = fValueUnsafe.Ticks != DateTimeOffset.MinValue.Ticks;
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(ZDateTime value, TimeSpan offset)
		{
			if (value.IsEmpty)
			{
				this = Empty;
			}
			else if (!value.IsValid)
			{
				this = Invalid;
			}
			else
			{
				this = new ZDateTimeOffset(value.ToDateTime(), offset);

				isNotEmpty = true;
				isValid = fValueUnsafe.Ticks != DateTimeOffset.MinValue.Ticks;
			}
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(DateTime value, DateTimeKind kind, TimeSpan offset)
		{
			this = new ZDateTimeOffset(DateTime.SpecifyKind(value, kind), offset);

			isNotEmpty = true;
			isValid = fValueUnsafe.Ticks != DateTimeOffset.MinValue.Ticks;
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(ZDateTime value, DateTimeKind kind, TimeSpan offset)
		{
			if (value.IsEmpty)
			{
				this = Empty;
			}
			else if (!value.IsValid)
			{
				this = Invalid;
			}
			else
			{
				this = new ZDateTimeOffset(value.ToDateTime(), kind, offset);

				isNotEmpty = true;
				isValid = fValueUnsafe.Ticks != DateTimeOffset.MinValue.Ticks;
			}
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(DateTimeOffset value)
		{
			fValueUnsafe = value;
			isNotEmpty = true;
			isValid = fValueUnsafe.Ticks != DateTimeOffset.MinValue.Ticks;
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(long ticks, TimeSpan offset)
		{
			fValueUnsafe = new DateTimeOffset(ticks, offset);
			isNotEmpty = true;
			isValid = fValueUnsafe.Ticks != DateTimeOffset.MinValue.Ticks;
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(int year, int month, int day)
			: this(new DateTime(year, month, day))
		{
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(int year, int month, int day, int hour, int minute, int second)
			: this(new DateTime(year, month, day, hour, minute, second))
		{
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(int year, int month, int day, int hour, int minute, int second, TimeSpan offset)
		{
			fValueUnsafe = new DateTimeOffset(year, month, day, hour, minute, second, offset);
			isNotEmpty = true;
			isValid = fValueUnsafe.Ticks != DateTimeOffset.MinValue.Ticks;
		}

		[DebuggerStepThrough]
		public ZDateTimeOffset(int year, int month, int day, int hour, int minute, int second, int millisecond, TimeSpan offset)
		{
			fValueUnsafe = new DateTimeOffset(year, month, day, hour, minute, second, millisecond, offset);
			isNotEmpty = true;
			isValid = fValueUnsafe.Ticks != DateTimeOffset.MinValue.Ticks;
		}

		/// <summary>
		/// Converts the ZDateTimeFormat to an SQL-legal format. The ZDateTimeFormat must be valid before attempting to access this property.
		/// </summary>
		[XmlIgnore]
		public ZString SqlFormat
		{
			get
			{
				return ObjectCache.DateTimeProvider.FormatSqlDateTimeOffset(ValueSafe);
			}
		}

		#region Static

		/// <summary>
		/// The empty ZDateTime.
		/// </summary>
		public static readonly ZDateTimeOffset Empty;

		/// <summary>
		/// The invalid (but not empty) ZDateTime.
		/// </summary>
		static readonly ZDateTimeOffset invalid = new ZDateTimeOffset(DateTimeOffset.MinValue);
		public static ZDateTimeOffset Invalid
		{
			get
			{
				return invalid;
			}
		}

		/// <summary>
		/// The current local DateTimeOffset.
		/// </summary>
		public static ZDateTimeOffset Now
		{
			[DebuggerStepThrough]
			get
			{
				var result = new ZDateTimeOffset(ObjectCache.DateTimeProvider.CurrentLocalDateTime, ObjectCache.DateTimeProvider.GetUtcOffsetBasedOnUtc(ObjectCache.DateTimeProvider.CurrentUtcDateTime));
				return result;
			}
		}

		/// <summary>
		/// The current UTC DateTimeOffset (offset set to 0).
		/// </summary>
		public static ZDateTimeOffset UtcNow
		{
			[DebuggerStepThrough]
			get
			{
				var result = new ZDateTimeOffset(ObjectCache.DateTimeProvider.CurrentUtcDateTime, TimeSpan.Zero); //CurrentUtcDateTime will return a DateTime with Kind of Utc.
				return result;
			}
		}

		/// <summary>
		/// The current local DateTimeOffset
		/// </summary>
		public static ZDateTimeOffset Today
		{
			[DebuggerStepThrough]
			get
			{
				var result = new ZDateTimeOffset(ObjectCache.DateTimeProvider.CurrentLocalDate, ObjectCache.DateTimeProvider.GetUtcOffsetBasedOnUtc(ObjectCache.DateTimeProvider.CurrentUtcDate));
				return result;
			}
		}

		/// <summary>
		/// The current UTC DateTimeOffset (offset set to 0).
		/// </summary>
		public static ZDateTimeOffset UtcToday
		{
			[DebuggerStepThrough]
			get
			{
				var result = new ZDateTimeOffset(ObjectCache.DateTimeProvider.CurrentUtcDate, TimeSpan.Zero); //CurrentUtcDate will return a DateTime with Kind of Utc.
				return result;
			}
		}

		public static ZDateTimeOffset FromSqlFormat(ZString dateTime)
		{
			return ObjectCache.DateTimeProvider.FromSqlDateTimeOffset(dateTime);
		}

		/// <summary>
		/// The current time and offset (as a ZDateTimeOffset) in the given UNLOCO.
		/// </summary>
		public static ZDateTimeOffset GetNowFromUNLOCO(string unloco)
		{
			var (time, offset) = ObjectCache.DateTimeProvider.CurrentUNLOCODateTime(unloco);
			var result = new ZDateTimeOffset(time, offset);
			return result;
		}

		public static ZDateTimeOffset GetFromLocalAndUtc(ZDateTime local, ZDateTime utc)
		{
			if (local.IsValid)
			{
				if (utc.IsValid)
				{
					var timeSpan = local - utc;
					var offset = new TimeSpan(timeSpan.Hours, timeSpan.Minutes, 0);
					if (IsValidOffset(offset))
					{
						return new ZDateTimeOffset(local, offset);
					}
					else
					{
						return utc.UtcToDateTimeOffset();
					}
				}
				else
				{
					return new ZDateTimeOffset(local);
				}
			}
			else if (utc.IsValid)
			{
				return utc.UtcToDateTimeOffset();
			}
			else if (local.IsEmpty)
			{
				return ZDateTimeOffset.Empty;
			}
			else
			{
				return ZDateTimeOffset.Invalid;
			}
		}

		/// <summary>
		/// Returns copy of input dateTimeOffset without microseconds.
		/// </summary>
		/// <param name="dateTimeOffset"></param>
		/// <returns></returns>
		public static ZDateTimeOffset TruncateMilliseconds(ZDateTimeOffset dateTimeOffset)
		{
			return Truncate(dateTimeOffset, TimeSpan.TicksPerSecond);
		}

		/// <summary>
		/// Returns copy of input dateTimeOffset rounding by a resolution
		/// </summary>
		/// <param name="dateTime"></param>
		/// <returns></returns>
		static ZDateTimeOffset Truncate(ZDateTimeOffset dateTimeOffset, long resolution)
		{
			if (!dateTimeOffset.IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot truncate an invalid ZDateTimeOffset.");
			}

			if (dateTimeOffset.Year <= 1)
			{
				throw new ArgumentException("ZDateTimeOffset to be truncated must a have a Year > 1 to prevent creating a invalid ZDateTimeOffset.", nameof(dateTimeOffset));
			}

			return dateTimeOffset.AddTicks(-(dateTimeOffset.Ticks % resolution));
		}

		#endregion

		#region Object Overrides

		/// <summary>
		/// Is this instance equal to the specified object?
		/// </summary>
		/// <param name="obj">The object to compare with the current instance.</param>
		public override bool Equals(object obj)
		{
			return (obj is ZDateTimeOffset offset && offset == this) ||
				(obj is DateTimeOffset offset1 && offset1 == ValueSafe);
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
		/// Converts the ZDateTimeOffset to a DateTimeOffset. WARNING - an exception will be thrown if ToDateTimeOffset() is invoked on an empty or invalid ZDateTimeOffset!".
		/// </summary>
		[DebuggerStepThrough]
		public DateTimeOffset ToDateTimeOffset()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot convert an Empty or Invalid ZDateTime to a DateTime!");
			}

			return ValueSafe;
		}

		/// <summary>
		/// Converts the ZDateTimeOffset to a Nullable DateTimeOffset. If the value is invalid, null will be returned
		/// </summary>
		[DebuggerStepThrough]
		[Pure]
		public DateTimeOffset? ToDateTimeOffsetSafe()
		{
			if (IsValid)
			{
				return ValueSafe;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Converts the ZDateTimeOffset to a (local for the DateTimeOffset's time zone) DateTime. WARNING - an exception will be thrown if ToDateTime() is invoked on an empty or invalid ZDateTimeOffset!".
		/// </summary>
		[DebuggerStepThrough]
		public DateTime ToDateTime()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot convert an Empty or Invalid ZDateTime to a DateTime!");
			}

			var result = ValueSafe.DateTime;
			return DateTime.SpecifyKind(result, DateTimeKind.Unspecified);
		}

		/// <summary>
		/// Converts the ZDateTimeOffset's UTC date time to a UTC DateTime. WARNING - an exception will be thrown if ToUtcDateTime() is invoked on an empty or invalid ZDateTimeOffset!".
		/// </summary>
		[DebuggerStepThrough]
		public DateTime ToUtcDateTime()
		{
			if (!IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot convert an Empty or Invalid ZDateTime to a DateTime!");
			}

			var result = ValueSafe.UtcDateTime;
			return result;
		}

		/// <summary>
		/// Converts the ZDateTimeOffset to a (local for the DateTimeOffset's time zone) ZDateTime. Invalid/Empty will return Invalid/Empty. See also: ToLocalZDateTime(), ToUtcZDateTime().
		/// </summary>
		[DebuggerStepThrough]
		public ZDateTime ToZDateTime()
		{
			if (IsEmpty)
			{
				return ZDateTime.Empty;
			}
			else if (!IsValid)
			{
				return ZDateTime.Invalid;
			}
			var result = ValueSafe.DateTime;
			if (Offset != TimeSpan.Zero)
			{
				return new ZDateTime(result, DateTimeKind.Unspecified);
			}
			return new ZDateTime(result, DateTimeKind.Utc);
		}

		/// <summary>
		/// Converts the ZDateTimeOffset's UTC date time to a (local to the currently logged in Branch's UNLOCO) local ZDateTime. Invalid/Empty will return Invalid/Empty. See also: ToZDateTime(), ToUtcZDateTime().
		/// </summary>
		[DebuggerStepThrough]
		public ZDateTime ToLocalZDateTime()
		{
			if (IsEmpty)
			{
				return ZDateTime.Empty;
			}
			else if (!IsValid)
			{
				return ZDateTime.Invalid;
			}
			var result = ValueSafe.UtcDateTime;
			return new ZDateTime(result.Add(ObjectCache.DateTimeProvider.GetUtcOffsetBasedOnUtc(result)), DateTimeKind.Local);
		}

		/// <summary>
		/// Converts the ZDateTimeOffset's UTC date time to a UTC ZDateTime. Invalid/Empty will return Invalid/Empty. See also: ToZDateTime((), ToLocalZDateTime().
		/// </summary>
		[DebuggerStepThrough]
		public ZDateTime ToUtcZDateTime()
		{
			if (IsEmpty)
			{
				return ZDateTime.Empty;
			}
			else if (!IsValid)
			{
				return ZDateTime.Invalid;
			}
			return new ZDateTime(ValueSafe.UtcDateTime, DateTimeKind.Utc);
		}

		[DebuggerStepThrough]
		public static implicit operator ZDateTimeOffset(DateTimeOffset value)
		{
			return new ZDateTimeOffset(value);
		}

		[DebuggerStepThrough]
		public static implicit operator ZDateTimeOffset(DateTime value)
		{
			return new ZDateTimeOffset(value);
		}

		public ZDateTimeOffset ToSmallDateTime()
		{
			var result = ToSmallDateTimeFloor();
			if (Second >= 30 || (Second == 29 && Millisecond == 999)) //with SQL Server's level of precision, 29.999 actually rounds up to 30 (20.998 would round down to 29.997)
			{
				result = result.AddMinutes(1);
			}
			return result;
		}

		public ZDateTimeOffset ToSmallDateTimeFloor()
		{
			return AddTicks(-(Ticks % TimeSpan.TicksPerMinute));
		}

		#endregion

		#region Operator Overloads: ZDateTimeOffset & ZDateTimeOffset

		//Note that comparisons between DateTimeOffsets use UtcDateTime property.

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static TimeSpan operator -(ZDateTimeOffset lhs, ZDateTimeOffset rhs)
		{
			if (!lhs.IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot subtract empty ZDateTimeOffset (lhs).");
			}

			if (!rhs.IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot subtract empty ZDateTimeOffset (rhs).");
			}

			return lhs.ToDateTimeOffset() - rhs.ToDateTimeOffset();
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static TimeSpan Subtract(ZDateTimeOffset lhs, ZDateTimeOffset rhs)
		{
			if (!lhs.IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot subtract empty ZDateTimeOffset (lhs).");
			}

			if (!rhs.IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot subtract empty ZDateTimeOffset (rhs).");
			}

			return lhs - rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZDateTimeOffset lhs, ZDateTimeOffset rhs)
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
		public static bool operator !=(ZDateTimeOffset lhs, ZDateTimeOffset rhs)
		{
			return !(lhs == rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(ZDateTimeOffset lhs, ZDateTimeOffset rhs)
		{
			return lhs.IsValid && rhs.IsValid && lhs.ValueSafe < rhs.ValueSafe;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(ZDateTimeOffset lhs, ZDateTimeOffset rhs)
		{
			return lhs.IsValid && rhs.IsValid && lhs.ValueSafe > rhs.ValueSafe;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(ZDateTimeOffset lhs, ZDateTimeOffset rhs)
		{
			return lhs < rhs || lhs == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(ZDateTimeOffset lhs, ZDateTimeOffset rhs)
		{
			return lhs > rhs || lhs == rhs;
		}

		#endregion

		#region Operator Overloads: ZDateTimeOffset & DateTimeOffset

		//Note that comparisons between DateTimeOffsets use UtcDateTime property.

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(ZDateTimeOffset lhs, DateTimeOffset rhs)
		{
			return lhs == new ZDateTimeOffset(rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator ==(DateTimeOffset lhs, ZDateTimeOffset rhs)
		{
			return new ZDateTimeOffset(lhs) == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(ZDateTimeOffset lhs, DateTimeOffset rhs)
		{
			return !(lhs == rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator !=(DateTimeOffset lhs, ZDateTimeOffset rhs)
		{
			return !(lhs == rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(ZDateTimeOffset lhs, DateTimeOffset rhs)
		{
			return lhs < new ZDateTimeOffset(rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <(DateTimeOffset lhs, ZDateTimeOffset rhs)
		{
			return new ZDateTimeOffset(lhs) < rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(ZDateTimeOffset lhs, DateTimeOffset rhs)
		{
			return lhs > new ZDateTimeOffset(rhs);
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >(DateTimeOffset lhs, ZDateTimeOffset rhs)
		{
			return new ZDateTimeOffset(lhs) > rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(ZDateTimeOffset lhs, DateTimeOffset rhs)
		{
			return lhs < rhs || lhs == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator <=(DateTimeOffset lhs, ZDateTimeOffset rhs)
		{
			return lhs < rhs || lhs == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(ZDateTimeOffset lhs, DateTimeOffset rhs)
		{
			return lhs > rhs || lhs == rhs;
		}

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static bool operator >=(DateTimeOffset lhs, ZDateTimeOffset rhs)
		{
			return lhs > rhs || lhs == rhs;
		}

		#endregion

		#region Operator Overloads: ZDateTimeOffset & TimeSpan

		[SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "rhs"), DebuggerStepThrough]
		public static ZDateTimeOffset operator +(ZDateTimeOffset lhs, TimeSpan rhs)
		{
			if (!lhs.IsValid)
			{
				throw new OperationOnInvalidZDateTimeException("Cannot convert an Empty or Invalid ZDateTimeOffset to a DateTimeOffset!");
			}

			return lhs.ToDateTimeOffset() + rhs;
		}

		#endregion

		#region TryParse

		/// <summary>
		/// Can the given string be converted to a DateTimeOffset? If so, Result contains the ZDateTimeOffset resulting from the conversion
		/// </summary>
		/// <param name="unparsedValue">A string containing a DateTimeOffset string to convert.</param>
		/// <param name="result">Returned as the ZDateTimeOffset equivalent of the given string, else invalid if it cannot be converted.</param>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
		public static bool TryParse(string unparsedValue, out ZDateTimeOffset result)
		{
			return TryParse(unparsedValue, out result, null);
		}

		/// <summary>
		/// Can the given string be converted to a DateTimeOffset? If so, Result contains the ZDateTimeOffset resulting from the conversion
		/// </summary>
		/// <param name="unparsedValue">A string containing a DateTimeOffset string to convert.</param>
		/// <param name="result">Returned as the ZDateTimeOffset equivalent of the given string, else invalid if it cannot be converted.</param>
		/// <param name="formatProvider">Culture information.</param>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
		public static bool TryParse(string unparsedValue, out ZDateTimeOffset result, IFormatProvider formatProvider)
		{
			bool success;
			if (string.IsNullOrWhiteSpace(unparsedValue))
			{
				result = Empty;
				success = true;
			}
			else
			{
				unparsedValue = unparsedValue.Trim();

				//1) Use regex to find +\d\d?:\d\d / -\d\d?:\d\d / z / Z at end of string. If there is, it has time zone information - use it as-is.
				//2) If we didn't find any, then parse as DateTime. Compose with Offset of previousValue (if valid), else just use default offset (from current Branch/UNLOCO).

				if (StringHasOffsetInformation(unparsedValue))
				{
					bool dtoSuccess = formatProvider != null ? DateTimeOffset.TryParse(unparsedValue, formatProvider, DateTimeStyles.None, out var dto) : DateTimeOffset.TryParse(unparsedValue, out dto);
					if (dtoSuccess)
					{
						success = true;
						result = new ZDateTimeOffset(dto);
					}
					else
					{
						success = false;
						result = Invalid;
					}
				}
				else
				{
					bool dtSuccess = formatProvider != null ? DateTime.TryParse(unparsedValue, formatProvider, DateTimeStyles.None, out var dt) : DateTime.TryParse(unparsedValue, out dt);
					if (dtSuccess)
					{
						success = true;
						result = new ZDateTimeOffset(dt); //Using offset from current Branch/UNLOCO.
					}
					else
					{
						success = false;
						result = Invalid;
					}
				}
			}

			return success;
		}

		/// <summary>
		/// Does the given string contain time zone information? If so, returns true
		/// </summary>
		/// <param name="value">A DateTime string possibly containing offset information.</param>
		public static bool StringHasOffsetInformation(string value) => HasOffsetInformationRegex.IsMatch(value);

		[ThreadStatic]
		static Regex hasOffsetInformationRegex;

		[SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Is a regex pattern")]
		static Regex HasOffsetInformationRegex
		{
			get
			{
				if (hasOffsetInformationRegex == null)
				{
					hasOffsetInformationRegex = new Regex(@"([+-]\d\d?:\d\d)|([zZ])$");
				}
				return hasOffsetInformationRegex;
			}
		}

		/// <summary>
		/// Use this method when you are updating a ZDateTimeOffset field via XML/CSV/XLS import.
		/// Sometimes we don't know if a string represents a DateTime or a DateTimeOffset.
		/// If it's a DateTime, we want to copy the offset from the previously existing ZDateTimeOffset and re-use it.
		/// If it's a DateTimeOffset, we want to just use the new DateTimeOffset as-is.
		/// </summary>
		/// /// <param name="previousValue">The ZDateTimeOffset previous value. Its offset will be used if unparsedValue is a string representing a DateTime with no offset.</param>
		/// <param name="unparsedValue">A string containing a DateTime or DateTimeOffset string to convert.</param>
		/// <param name="result">Returned as the ZDateTimeOffset equivalent of the given string, else invalid if it cannot be converted.</param>
		/// <param name="formatProvider">Optional culture information. Otherwise, current culture is used.</param>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		public static bool TryParseForUpdate(ZDateTimeOffset previousValue, string unparsedValue, out ZDateTimeOffset result, IFormatProvider formatProvider = null)
		{
			bool success;
			if (string.IsNullOrWhiteSpace(unparsedValue))
			{
				result = Empty;
				success = true;
			}
			else
			{
				unparsedValue = unparsedValue.Trim();

				//1) Use regex to find +\d\d?:\d\d / -\d\d?:\d\d / z / Z at end of string. If there is, it has time zone information - use it as-is.
				//2) If we didn't find any, then parse as DateTime. Compose with Offset of previousValue (if valid), else just use default offset (from current Branch/UNLOCO).

				if (StringHasOffsetInformation(unparsedValue))
				{
					bool dtoSuccess = formatProvider != null ? DateTimeOffset.TryParse(unparsedValue, formatProvider, DateTimeStyles.None, out var dto) : DateTimeOffset.TryParse(unparsedValue, out dto);
					if (dtoSuccess)
					{
						success = true;
						result = new ZDateTimeOffset(dto);
					}
					else
					{
						success = false;
						result = Invalid;
					}
				}
				else
				{
					bool dtSuccess = formatProvider != null ? DateTime.TryParse(unparsedValue, formatProvider, DateTimeStyles.None, out var dt) : DateTime.TryParse(unparsedValue, out dt);
					if (dtSuccess)
					{
						success = true;
						if (previousValue.IsEmpty || !previousValue.IsValid)
						{
							result = new ZDateTimeOffset(dt); //Using offset from current Branch/UNLOCO.
						}
						else
						{
							result = new ZDateTimeOffset(dt, previousValue.Offset); //Composing with offset from previousValue.
						}
					}
					else
					{
						success = false;
						result = Invalid;
					}
				}
			}

			return success;
		}

		/// <summary>
		/// Use this method when you are updating a ZDateTimeOffset field generically via the GUI (e.g. appropriate business logic might not be called)
		/// and you would rather keep the previous value's offset (if any) than override it with the default one (derived from current branch UNLOCO).
		/// </summary>
		public static ZDateTimeOffset ValueForUpdate(ZDateTimeOffset previousValue, ZDateTimeOffset newValue)
		{
			if (previousValue.IsEmpty || !previousValue.IsValid || newValue.IsEmpty || !newValue.IsValid)
			{
				return newValue;
			}
			//Special case here - if newValue's offset is TimeSpan.Zero, we still go with the previous value's offset. Checked in TestValueForUpdate
			return new ZDateTimeOffset(DateTime.SpecifyKind(newValue.ToDateTime(), DateTimeKind.Unspecified), previousValue.Offset);
		}

		/// <summary>
		/// Use this method when you are updating a ZDateTimeOffset field generically via the GUI (e.g. appropriate business logic might not be called)
		/// and you would rather keep the previous value's offset (if any) than override it with the default one (derived from current branch UNLOCO).
		/// </summary>
		public static ZDateTimeOffset ValueForUpdate(ZDateTimeOffset previousValue, ZDateTime newValue)
		{
			if (previousValue.IsEmpty || !previousValue.IsValid || newValue.IsEmpty || !newValue.IsValid)
			{
				return new ZDateTimeOffset(newValue); //Using offset from current Branch/UNLOCO.
			}
			else if (newValue.Kind == DateTimeKind.Utc)
			{
				return new ZDateTimeOffset(newValue); //Will be treated as UTC.
			}
			return new ZDateTimeOffset(newValue, previousValue.Offset);
		}

		/// <summary>
		/// Can the given string in a certain format be converted to a datetimeoffset? If so, Result contains the ZDateTimeOffset resulting from the conversion.
		/// Use this method if you know your string contains the offset. Otherwise, use:
		/// <code>
		///	ZDateTime.TryParseExact(value, out var date, format);
		/// var dateTimeOffset = date.IsValid ? new ZDateTimeOffset(date) : ZDateTimeOffset.Now //or whatever you need;
		/// </code>
		/// This way, it will get the offset from <code>EnvProxy.Instance.CurrentNKUNLOCO</code>.
		/// For information about custom format strings see https://learn.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings
		/// </summary>
		/// <param name="value">A string containing a datetimeoffset string to convert.</param>
		/// <param name="result">Returned as the datetimeoffset equivalent of the given string, else invalid if it cannot be converted.</param>
		/// <param name="format">The format string to apply during parsing</param>
		[SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
		public static bool TryParseExact(string value, out ZDateTimeOffset result, string format)
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
				if (DateTimeOffset.TryParseExact(value, format, ObjectCache.CultureProvider.Culture, DateTimeStyles.None, out var dto))
				{
					result = GetParsedTimeOnlyWithAdjustedDate(dto, format);
					success = true;
				}
			}
			return success;
		}

		/// <summary>
		/// DateTimeOffset.ParseExact uses the local machine date to complete the DateTime in a Time only format parsing.
		/// This date might be different from the date in the current ZDateTimeOffset time zone, Hence it has to be adjusted here.
		/// </summary>
		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "String format")]
		static ZDateTimeOffset GetParsedTimeOnlyWithAdjustedDate(ZDateTimeOffset parsedResult, string format)
		{
			Argument.NotNull(format, nameof(format));

			ZDateTimeOffset result = parsedResult;

			if (!format.Contains("y"))
			{
				if (format.Contains("M") || (format.Contains("d") && format.Trim() != "d"))
				{
					int year = ZDateTimeOffset.Now.Year;
					result = result.AddYears(year - result.Year);
				}
				else if (!format.Contains("d"))
				{
					TimeSpan parsedTime = parsedResult.TimeOfDay;
					result = ZDateTimeOffset.Today.Add(parsedTime);
				}
			}

			return result;
		}

		#endregion

		#region Wrapping the internal DateTimeOffset

		#region ToString

		/// <summary>
		/// The default string representation of this instance.
		/// </summary>
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Always use invariant format for now")]
		public override string ToString()
		{
			return ToString(null);
		}

		public string FormatDateTimeOffset()
		{
			return ObjectCache.DateTimeProvider.FormatDateTimeOffset(ToDateTimeOffset());
		}

		public string FormatDateTimeOffsetWithSeconds()
		{
			return ObjectCache.DateTimeProvider.FormatDateTimeOffsetWithSeconds(ToDateTimeOffset());
		}

		/// <summary>
		/// The string representation of this instance in the specified format.
		/// </summary>
		/// <param name="format">A format string.</param>
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Always use invariant format for now")]
		public string ToString(string format)
		{
			string result;

			if (IsEmpty)
			{
				result = "";
			}
			else if (!IsValid)
			{
				result = ZDateTime.InvalidLiteral;
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
		[SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", Justification = "Always use invariant format for now")]
		public string ToISO8601String()
		{
			return ToString("O");
		}

		public string ToShortDateString() => ToZDateTime().ToShortDateString();

		#endregion

		/// <summary>
		/// The date component of this instance.
		/// </summary>
		[XmlIgnore]
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
					return new ZDate(ValueSafe.Date);
				}
			}
		}

		/// <summary>
		/// The date component of this instance, as a ZDateTimeOffset, with Offset preserved.
		/// </summary>
		[XmlIgnore]
		public ZDateTimeOffset DateAndOffset
		{
			get
			{
				if (IsEmpty)
				{
					return Empty;
				}
				else if (!IsValid)
				{
					return Invalid;
				}
				else
				{
					return new ZDateTimeOffset(ValueSafe.Year, ValueSafe.Month, ValueSafe.Day, 0, 0, 0, ValueSafe.Offset);
				}
			}
		}

		/// <summary>
		/// The date component of a DateTimeOffset, as a DateTimeOffset, with Offset preserved.
		/// </summary>
		public static DateTimeOffset DateAndOffsetHelper(DateTimeOffset dateTimeOffset)
		{
			return new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 0, 0, 0, dateTimeOffset.Offset);
		}

		/// <summary>
		/// The number of ticks that represent the date and time of this instance (in its local time zone).
		/// </summary>
		[XmlIgnore]
		public long Ticks
		{
			get
			{
				var result = ValueSafe.Ticks;
				return result;
			}
		}

		/// <summary>
		/// The number of ticks that represent the date and time of this instance (in UTC).
		/// </summary>
		[XmlIgnore]
		public long UtcTicks
		{
			get
			{
				var result = ValueSafe.UtcTicks;
				return result;
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
		/// The time of day for this instance (as UTC).
		/// </summary>
		[XmlIgnore]
		public TimeSpan UtcTimeOfDay
		{
			get
			{
				return ValueSafe.UtcDateTime.TimeOfDay;
			}
		}

		/// <summary>
		/// The offset for this instance.
		/// </summary>
		[XmlIgnore]
		public TimeSpan Offset
		{
			get
			{
				return ValueSafe.Offset;
			}
		}

		/// <summary>
		/// Return a new ZDateTimeOffset that has the same Utc as this one but the provided offset.
		/// </summary>
		public ZDateTimeOffset KeepUtcChangeOffset(TimeSpan offset)
		{
			if (IsEmpty)
			{
				return Empty;
			}

			if (!IsValid)
			{
				return Invalid;
			}

			DateTime utcTime = ToUtcDateTime();
			return new ZDateTimeOffset(utcTime + offset, DateTimeKind.Unspecified, offset);
		}

		public static string OffsetStringHelper(TimeSpan offset)
		{
			string result = "";
			int hours = offset.Hours;
			int minutes = offset.Minutes;
			if (offset >= TimeSpan.Zero)
			{
				result += "+";
			}
			else
			{
				result += "-";
			}
			result += Math.Abs(hours).ToString("00", CultureInfo.InvariantCulture);
			result += ":";
			result += Math.Abs(minutes).ToString("00", CultureInfo.InvariantCulture);
			return result;
		}

		[XmlIgnore]
		public string OffsetString
		{
			get
			{
				return OffsetStringHelper(ValueSafe.Offset);
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
				var result = ValueSafe.Second;
				return result;
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
				var result = ValueSafe.Minute;
				return result;
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
				var result = ValueSafe.Hour;
				return result;
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
				var result = ValueSafe.Day;
				return result;
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
				var result = ValueSafe.DayOfYear;
				return result;
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
				var result = ValueSafe.Month;
				return result;
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
				var result = ValueSafe.Year;
				return result;
			}
		}

		/// <summary>
		/// The milliseconds component for this instance (as Utc).
		/// </summary>
		[XmlIgnore]
		public int UtcMillisecond
		{
			get
			{
				return ValueSafe.UtcDateTime.Millisecond;
			}
		}

		/// <summary>
		/// The seconds component for this instance.
		/// </summary>
		[XmlIgnore]
		public int UtcSecond
		{
			get
			{
				var result = ValueSafe.UtcDateTime.Second;
				return result;
			}
		}

		/// <summary>
		/// The minutes component for this instance (as Utc).
		/// </summary>
		[XmlIgnore]
		public int UtcMinute
		{
			get
			{
				var result = ValueSafe.UtcDateTime.Minute;
				return result;
			}
		}
		/// <summary>
		/// The hour represented by this instance (as Utc).
		/// </summary>
		[XmlIgnore]
		public int UtcHour
		{
			get
			{
				var result = ValueSafe.UtcDateTime.Hour;
				return result;
			}
		}

		/// <summary>
		/// The day of the month represented by this instance (as Utc).
		/// </summary>
		[XmlIgnore]
		public int UtcDay
		{
			get
			{
				var result = ValueSafe.UtcDateTime.Day;
				return result;
			}
		}

		/// <summary>
		/// The day of the week represented by this instance (as Utc).
		/// </summary>
		[XmlIgnore]
		public DayOfWeek UtcDayOfWeek
		{
			get
			{
				return ValueSafe.UtcDateTime.DayOfWeek;
			}
		}

		/// <summary>
		/// The day of the year represented by this instance (as Utc).
		/// </summary>
		[XmlIgnore]
		public int UtcDayOfYear
		{
			get
			{
				var result = ValueSafe.UtcDateTime.DayOfYear;
				return result;
			}
		}

		/// <summary>
		/// The month component for this instance (as Utc).
		/// </summary>
		[XmlIgnore]
		public int UtcMonth
		{
			get
			{
				var result = ValueSafe.UtcDateTime.Month;
				return result;
			}
		}

		/// <summary>
		/// The year component for this instance (as Utc).
		/// </summary>
		[XmlIgnore]
		public int UtcYear
		{
			get
			{
				var result = ValueSafe.UtcDateTime.Year;
				return result;
			}
		}

		/// <summary>
		/// The value of this instance plus the specified time span.
		/// </summary>
		public ZDateTimeOffset Add(TimeSpan ticks)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add timespans to an invalid or empty ZDateTimeOffset.");
			}

			var returnVal = new ZDateTimeOffset(ValueSafe.Add(ticks));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of ticks added resulted in the minimum DateTimeOffset and hence an invalid ZDateTimeOffset");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of 100-nanosecond ticks.
		/// </summary>
		public ZDateTimeOffset AddTicks(long ticks)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add ticks to an invalid or empty ZDateTimeOffset.");
			}

			var returnVal = new ZDateTimeOffset(ValueSafe.AddTicks(ticks));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of ticks added resulted in the minimum DateTimeOffset and hence an invalid ZDateTimeOffset");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of milliseconds.
		/// </summary>
		public ZDateTimeOffset AddMilliseconds(int milliseconds)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add miliseconds to an invalid or empty ZDateTimeOffset.");
			}

			var returnVal = new ZDateTimeOffset(ValueSafe.AddMilliseconds(milliseconds));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of milliseconds added resulted in the minimum DateTimeOffset and hence an invalid ZDateTimeOffset");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of seconds.
		/// </summary>
		public ZDateTimeOffset AddSeconds(int seconds)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add seconds to an invalid or empty ZDateTimeOffset.");
			}

			var returnVal = new ZDateTimeOffset(ValueSafe.AddSeconds(seconds));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of seconds added resulted in the minimum DateTimeOffset and hence an invalid ZDateTimeOffset");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of minutes.
		/// </summary>
		public ZDateTimeOffset AddMinutes(int minutes)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add minutes to an invalid or empty ZDateTimeOffset.");
			}

			var returnVal = new ZDateTimeOffset(ValueSafe.AddMinutes(minutes));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of minutes added resulted in the minimum DateTimeOffset and hence an invalid ZDateTimeOffset");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of hours.
		/// </summary>
		public ZDateTimeOffset AddHours(int hours)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add hours to an invalid or empty ZDateTimeOffset.");
			}

			var returnVal = new ZDateTimeOffset(ValueSafe.AddHours(hours));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of hours added resulted in the minimum DateTimeOffset and hence an invalid ZDateTimeOffsetOffset");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of hours.
		/// </summary>
		public ZDateTimeOffset AddHours(ZDecimal hours)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add hours to an invalid or empty ZDateTimeOffset. (ZDecimal).");
			}

			decimal decimalHours = hours;
			var returnVal = new ZDateTimeOffset(ValueSafe.AddHours((double)decimalHours));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of hours added resulted in the minimum DateTimeOffset and hence an invalid ZDateTimeOffset");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of days.
		/// </summary>
		public ZDateTimeOffset AddDays(int days)
		{
			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add days to an invalid or empty ZDateTimeOffset.");
			}

			var returnVal = new ZDateTimeOffset(ValueSafe.AddDays(days));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of days added resulted in the minimum DateTimeOffset and hence an invalid ZDateTimeOffset");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of months.
		/// </summary>
		public ZDateTimeOffset AddMonths(int months)
		{
			if (!(months >= -120000 && months <= 120000))
			{
				throw new ArgumentException("An invalid number of months to add.", nameof(months));
			}

			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add months to an invalid or empty ZDateTimeOffset.");
			}

			var returnVal = new ZDateTimeOffset(ValueSafe.AddMonths(months));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of months added resulted in the minimum DateTimeOffset and hence an invalid ZDateTimeOffset");
			}

			return returnVal;
		}

		/// <summary>
		/// The value of this instance plus the specified number of years.
		/// </summary>
		public ZDateTimeOffset AddYears(int years)
		{
			if (!(years >= -10000 && years <= 10000))
			{
				throw new ArgumentException("An invalid numner of years to add", nameof(years));
			}

			if (!IsValid)
			{
				throw new InvalidOperationException("Cannot add years to an invalid or empty ZDateTimeOffset.");
			}

			var returnVal = new ZDateTimeOffset(ValueSafe.AddYears(years));
			if (!returnVal.IsValid)
			{
				throw new InvalidZDateTimeResultException("Number of months added resulted in the minimum DateTimeOffset and hence an invalid ZDateTimeOffset");
			}

			return returnVal;
		}

		public ZDateTimeOffset EndOfDay()
		{
			// Cannot be unit tested as you cannot create an invalid ZDateTimeOffset
			if (!(IsEmpty || IsValid))
			{
				throw new InvalidOperationException();
			}

			if (IsValid)
			{
				return new ZDateTimeOffset(new ZDateTime(ValueSafe.Year, ValueSafe.Month, ValueSafe.Day, 23, 59, 59), DateTimeKind.Local);
			}
			else
			{
				return Empty;
			}
		}

		public bool IsInThePast(double delta = 1E-05)
		{
			return ToUtcZDateTime().IsInThePastUtc(delta);
		}

		public bool IsInTheFuture(double delta = 1E-05)
		{
			return ToUtcZDateTime().IsInTheFutureUtc(delta);
		}

		#endregion

		#region XmlSerializedValue

		[XmlText]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public DateTimeOffset XmlSerializedValue
		{
			get
			{
				if (!IsValid)
				{
					throw new OperationOnInvalidZDateTimeException();
				}

				return ToDateTimeOffset();
			}
			set { this = value; }
		}

		#endregion

		#region IComparable Members

		/// <summary>
		/// Note that comparisons between DateTimeOffset/ZDateTimeOffset use UtcDateTime property.
		/// </summary>
		public int CompareTo(object obj)
		{
			if (obj is IZTypeInternals zTypeInternals)
			{
				obj = zTypeInternals.GetValueForLogicalDataLayer(false);
			}

			if (obj is DBNull)
			{
				if (IsEmpty)
				{
					return 0;
				}
				return 1;
			}
			else if (obj == null)
			{
				return 1;
			}
			else if (obj is DateTimeOffset offset)
			{
				return ValueSafe.CompareTo(offset);
			}
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Can only compare ZDateTimeOffset to DBNull, null, ZDateTimeOffset or DateTimeOffset, but was: {0}", obj.GetType()));
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
			get { return typeof(DateTimeOffset); }
		}

		[XmlIgnore]
		public bool IsDefault
		{
			get { return this == (ZDateTimeOffset)Default; }
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
		/// Is the ZDateTimeOffset empty - that is, not ZDateTimeOffset.Empty?
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
		/// Returns true if the ZDateTimeOffset is valid - that is, not ZDateTimeOffset .Empty and has a valid value
		/// IsValid must be tested before casting a ZDateTimeOffset to a DateTimeOffset as an exception will be throw if the ZDateTimeOffset is not valid.
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

		/// <summary>
		/// Can this date be stored in a smalldatetime field?
		/// </summary>
		[XmlIgnore]
		public bool IsValidSmallDateTime
		{
			get
			{
				return ToZDateTime().IsValidSmallDateTime;
			}
		}

		#endregion

		#region IZDate Members

		IZDate IZDate.Invalid => Invalid;

		IZDate IZDate.Today => Today;

		int IZDate.Year => Year;

		IZDate IZDate.Now => Now;

		string IZDate.SqlFormat => SqlFormat;

		IZDate IZDate.FirstDayOfLastCalendarYear => new ZDateTimeOffset(Now.Year - 1, 1, 1, 0, 0, 0);

		IZDate IZDate.LastDayOfLastCalendarYear => new ZDateTimeOffset(Now.Year - 1, 12, 31, 0, 0, 0);

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
			var sucesss = TryParse(unparsedValue, out var date);
			result = date;
			return sucesss;
		}

		object IZDate.ToBaseDate()
		{
			return this.ToDateTimeOffset();
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

		public string ToString(string format, IFormatProvider formatProvider)
		{
			if (IsEmpty)
			{
				return "";
			}
			else if (!IsValid)
			{
				return ZDateTime.InvalidLiteral;
			}
			else
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
		}

		#endregion

		#region Implementation

		DateTimeOffset ValueSafe
		{
			get
			{
				return fValueUnsafe;
			}
		}
		[SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "If ZDateTime.fValueUnsafe is fine, this is fine too")]
		DateTimeOffset fValueUnsafe;
		readonly bool isNotEmpty;
		readonly bool isValid;

		#endregion

		#region Validation

		static readonly TimeSpan MaxOffsetMagnitude = TimeSpan.FromHours(14);

		public static bool IsValidOffset(TimeSpan value) => IsNotTooLarge(value) && IsWholeNumberOfMinutes(value);

		static bool IsNotTooLarge(TimeSpan value) => Math.Abs(value.TotalMinutes) <= MaxOffsetMagnitude.TotalMinutes;

		static bool IsWholeNumberOfMinutes(TimeSpan value) => value.Ticks % TimeSpan.TicksPerMinute == 0;

		#endregion

		#region ZDateTime Proxies

		public static bool DifferentMinutes(ZDateTimeOffset a, ZDateTimeOffset b) => ZDateTime.DifferentMinutes(a.ToUtcZDateTime(), b.ToUtcZDateTime());

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
			TypeConverter converter = ZDateTimeOffsetTypeConverter.Instance;
			if (converter != null && converter.CanConvertTo(conversionType))
			{
				return converter.ConvertTo(this, conversionType);
			}
			else
			{
				throw new InvalidCastException("Converting from ZDateTimeOffset to " + conversionType + " not supported");
			}
		}

		#endregion

	}
}
