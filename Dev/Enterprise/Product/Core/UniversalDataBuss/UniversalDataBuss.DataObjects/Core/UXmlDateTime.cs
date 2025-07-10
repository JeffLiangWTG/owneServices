using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core
{
	public struct UXmlDateTime : IComparable
	{
		public UXmlDateTime(ZDateTime? value)
		{
			date = value;
		}

		public UXmlDateTime(ZDateTimeOffset? value)
		{
			date = value;
		}

		public UXmlDateTime(ZDate? value)
		{
			date = new ZDateTime(value);
		}

		public UXmlDateTime(DateTime value)
		{
			date = new ZDateTime(value);
		}

		/// <summary>
		/// Converts the UXmlDateTime to a ZDateTime:
		/// If it was set with a ZDateTime, returns the underlying ZDateTime.
		/// If it was set with a ZDateTimeOffset, returns a new ZDateTime without the offset.
		/// If the underlying date is null, returns ZDateTime.Empty.
		/// </summary>
		public ZDateTime ToZDateTime()
		{
			if (date is ZDateTime dt)
			{
				return dt;
			}
			else if (date is ZDateTimeOffset offset)
			{
				return new ZDateTime(offset.ToZDateTime(), DateTimeKind.Unspecified);
			}

			return ZDateTime.Empty;
		}

		/// <summary>
		/// Converts the UXmlDateTime to a ZDateTimeOffset:
		/// If it was set with a ZDateTime, calculates and returns a ZDateTimeOffset based on the current branch's time zone.
		/// If it was set with a ZDateTimeOffset, returns the underlying ZDateTimeOffset.
		/// If the underlying date is null, returns ZDateTimeOffset.Empty.
		/// </summary>
		public ZDateTimeOffset ToZDateTimeOffset()
		{
			if (date is ZDateTime dt)
			{
				return new ZDateTimeOffset(dt);
			}
			else if (date is ZDateTimeOffset offset)
			{
				return offset;
			}

			return ZDateTimeOffset.Empty;
		}

		/// <summary>
		/// Gets the Current datetime value and if there is no present offset then add the offset of the inputted refUNLOCO
		/// If there is an existing offset it returns the existing offset
		/// </summary>
		public ZDateTimeOffset ToZDateTimeOffsetWithTheGivenOffsetIfNoneIsPresent(IRefUNLOCO refUNLOCO)
		{
			var dt = ToZDateTimeOffset();
			var utcOffset = refUNLOCO.StandardZoneUTCOffset;
			if (dt.IsValid)
			{
				return dt;
			}
			else
			{
				return new ZDateTimeOffset(ToZDateTime(), TimeSpan.FromHours((double)utcOffset));
			}
		}

		/// <summary>
		/// Gets the Current datetime value and if there is no present offset then add the offset of the inputted TimeSpan
		/// If there is an existing offset it returns the existing offset
		/// </summary>
		public ZDateTimeOffset ToZDateTimeOffsetWithTheGivenOffsetIfNoneIsPresent(TimeSpan timeSpan)
		{
			var dt = ToZDateTimeOffset();
			if (dt.IsValid)
			{
				return dt;
			}
			else
			{
				return new ZDateTimeOffset(ToZDateTime(), timeSpan);
			}
		}

		/// <summary>
		/// Converts the UXmlDateTime to a ZDateTimeOffset with the offset being the timezone of RefUNLOCO:
		/// The Moment in time will remain the same and be adjusted to the offset value
		/// </summary>
		public ZDateTimeOffset ToZDateTimeOffset(IRefUNLOCO refUNLOCO)
		{
			var dt = ToZDateTimeOffset();
			var utcOffset = refUNLOCO.StandardZoneUTCOffset;
			var offset = new TimeSpan((int)utcOffset, (int)(60 * ((double)utcOffset - (int)utcOffset)), 0);
			if (dt is ZDateTimeOffset dtOffset)
			{
				var timeDifference = offset - dtOffset.Offset;
				return new ZDateTimeOffset(ToZDateTime().Add(timeDifference), offset);
			}
			else
			{
				return new ZDateTimeOffset(ToZDateTime().Add(offset), offset);
			}
		}

		/// <summary>
		/// Converts the UXmlDateTime to a ZDateTimeOffset with the offset being the timeSpan:
		/// The Moment in time will remain the same and be adjusted to make the timespan the offset value
		/// </summary>
		public ZDateTimeOffset ToZDateTimeOffset(TimeSpan timeSpan)
		{
			var dt = ToZDateTimeOffset();
			if (dt is ZDateTimeOffset dtOffset)
			{
				var timeDifference = timeSpan - dtOffset.Offset;
				return new ZDateTimeOffset(ToZDateTime().Add(timeDifference), timeSpan);
			}
			else
			{
				return new ZDateTimeOffset(ToZDateTime().Add(timeSpan), timeSpan);
			}
		}

		/// <summary>
		/// Returns the underlying date type this UXmlDateTime was set with (ZDateTime or ZDateTimeOffset) or null if it was null.
		/// </summary>
		public Type GetDateType()
		{
			if (date is ZDateTime)
			{
				return typeof(ZDateTime);
			}
			else if (date is ZDateTimeOffset)
			{
				return typeof(ZDateTimeOffset);
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Return a ZDateTime Local time:
		/// If it was set with a ZDateTime, assume it is the correct Local time and return.
		/// If it was set with a ZDateTimeOffset, returns a ZDateTime by converting offset TimeSpan to input TimeSpan.
		/// If the underlying date is null, returns ZDateTime.Empty.
		/// </summary>
		public ZDateTime ToLocalTime(TimeSpan timeSpan)
		{
			if (date is ZDateTime dt)
			{
				return dt;
			}
			else if (date is ZDateTimeOffset offset)
			{
				return new ZDateTime(offset.ToUtcZDateTime() + timeSpan, DateTimeKind.Local);
			}

			return ZDateTime.Empty;
		}

		/// <summary>
		/// Return a ZDateTime Local time:
		/// If it was set with a ZDateTime, assume it is the correct Local time and return.
		/// If it was set with a ZDateTimeOffset, returns a ZDateTime by converting offset TimeSpan based on input UNLOCO.
		/// If the underlying date is null, returns ZDateTime.Empty.
		/// </summary>
		public ZDateTime ToLocalTime(IRefUNLOCO refUNLOCO)
		{
			if (date is ZDateTime dt)
			{
				return dt;
			}
			else if (date is ZDateTimeOffset offset)
			{
				ZDecimal utcOffset = refUNLOCO.StandardZoneUTCOffset;
				return new ZDateTime(offset.ToUtcZDateTime() + new TimeSpan((int)utcOffset, (int)(60 * ((double)utcOffset - (int)utcOffset)), 0), DateTimeKind.Local);
			}

			return ZDateTime.Empty;
		}

		/// <summary>
		/// Return a ZDateTime UTC time:
		/// If it was set with a ZDateTime, assume it is the correct Local time, calculate UTC time by subtract TimeSpan.
		/// If it was set with a ZDateTimeOffset, ignore TimeSpan and calculate UTC time by its own offset.
		/// If the underlying date is null, returns ZDateTime.Empty.
		/// </summary>
		public ZDateTime ToUTCTime(TimeSpan timeSpan)
		{
			if (!date.IsValid)
			{
				return ZDateTime.Empty;
			}

			if (date is ZDateTime dt)
			{
				return dt - timeSpan;
			}
			else if (date is ZDateTimeOffset offset)
			{
				return new ZDateTime(offset.ToUtcZDateTime(), DateTimeKind.Utc);
			}

			return ZDateTime.Empty;
		}

		/// <summary>
		/// Return a ZDateTime UTC time:
		/// If it was set with a ZDateTime, assume it is the correct Local time and return.
		/// If it was set with a ZDateTimeOffset, returns a ZDateTime by converting offset TimeSpan based on input UNLOCO.
		/// If the underlying date is null, returns ZDateTime.Empty.
		/// </summary>
		public ZDateTime ToUTCTime(IRefUNLOCO refUNLOCO)
		{
			if (!date.IsValid)
			{
				return ZDateTime.Empty;
			}

			if (date is ZDateTime dt)
			{
				ZDecimal utcOffset = refUNLOCO.StandardZoneUTCOffset;
				return dt - new TimeSpan((int)utcOffset, (int)(60 * ((double)utcOffset - (int)utcOffset)), 0);
			}
			else if (date is ZDateTimeOffset offset)
			{
				return new ZDateTime(offset.ToUtcZDateTime(), DateTimeKind.Utc);
			}

			return ZDateTime.Empty;
		}

		public ZDateTime ToUtcZDateTime() => date is ZDateTimeOffset offset ? offset.ToUtcZDateTime() : ((ZDateTime)date);

		public bool IsValid
		{
			get
			{
				if (!date.IsValid)
				{
					return false;
				}

				return date is ZDateTime || date is ZDateTimeOffset;
			}
		}

		public bool IsEmpty => date switch
		{
			ZDateTime dt => dt.IsEmpty,
			ZDateTimeOffset offset => offset.IsEmpty,
			_ => true
		};

		public int Year => date switch
		{
			ZDateTime dt => dt.Year,
			ZDateTimeOffset offset => offset.Year,
			_ => -1
		};

		public int Month => date switch
		{
			ZDateTime dt => dt.Month,
			ZDateTimeOffset offset => offset.Month,
			_ => -1
		};
		public int Day => date switch
		{
			ZDateTime dt => dt.Day,
			ZDateTimeOffset offset => offset.Day,
			_ => -1
		};

		public int Hour => date switch
		{
			ZDateTime dt => dt.Hour,
			ZDateTimeOffset offset => offset.Hour,
			_ => -1
		};

		public int Minute => date switch
		{
			ZDateTime dt => dt.Minute,
			ZDateTimeOffset offset => offset.Minute,
			_ => -1
		};

		public int Second => date switch
		{
			ZDateTime dt => dt.Second,
			ZDateTimeOffset offset => offset.Second,
			_ => -1
		};

		public int Millisecond => date switch
		{
			ZDateTime dt => dt.Millisecond,
			ZDateTimeOffset offset => offset.Millisecond,
			_ => -1
		};

		public TimeSpan Offset => date switch
		{
			ZDateTimeOffset offset => offset.Offset,
			_ => TimeSpan.Zero
		};

		public bool IsZDateTimeOffset => date is ZDateTimeOffset;
		public bool IsZDateTime => date is ZDateTime;

		#region Object Overrides

		public override bool Equals(object obj)
		{
			return base.Equals(obj) ||
				(obj is ZDateTime dateTime && dateTime == this.ToZDateTime()) ||
				(obj is ZDateTimeOffset dateTimeOffset && dateTimeOffset == this.ToZDateTimeOffset()) ||
				(obj is ZDate date && date == this.ToZDateTime()) ||
				(obj is DateTime date2 && date2 == this.ToZDateTime());
		}

		public override int GetHashCode() => date != null ? date.GetHashCode() : 0;

		public override string ToString() => date != null ? date.ToString() : "";

		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if (obj is IZTypeInternals zTypeInternals)
			{
				obj = zTypeInternals.GetValueForLogicalDataLayer(false);
			}

			if (obj == null || obj is DBNull || IsEmpty)
			{
				return 0;
			}
			else if (obj is UXmlDateTime dt)
			{
				if (dt.IsZDateTime)
				{
					return date.CompareTo(dt.ToZDateTime());
				}
				else if (dt.IsZDateTimeOffset)
				{
					return date.CompareTo(dt.ToZDateTimeOffset());
				}
			}
			throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Can only compare UXmlDateTime to DBNull, null and UXmlDateTime, but was: {0}", obj.GetType()));
		}

		#endregion

		#region Comparison Operator

		public static bool operator >(UXmlDateTime lhs, UXmlDateTime rhs)
		{
			return lhs.ToZDateTimeOffset() > rhs.ToZDateTimeOffset();
		}

		public static bool operator <(UXmlDateTime lhs, UXmlDateTime rhs)
		{
			return lhs.ToZDateTimeOffset() < rhs.ToZDateTimeOffset();
		}
		public static bool operator >=(UXmlDateTime lhs, UXmlDateTime rhs)
		{
			return lhs.ToZDateTimeOffset() >= rhs.ToZDateTimeOffset();
		}

		public static bool operator <=(UXmlDateTime lhs, UXmlDateTime rhs)
		{
			return lhs.ToZDateTimeOffset() <= rhs.ToZDateTimeOffset();
		}

		#endregion

		#region Implicit Conversions

		public static implicit operator UXmlDateTime(ZDateTime? value) => new (value);
		public static implicit operator UXmlDateTime(ZDateTimeOffset? value) => new (value);
		public static implicit operator UXmlDateTime(ZDate? value) => new (value);
		public static implicit operator UXmlDateTime(DateTime value) => new (value);

		public static implicit operator ZDateTime(UXmlDateTime value) => value.ToZDateTime();
		public static implicit operator ZDateTimeOffset(UXmlDateTime value) => value.ToZDateTimeOffset();
		public static implicit operator ZDate(UXmlDateTime value) => (ZDate)value.ToZDateTime();

		#endregion

		#region Implementation

		readonly IZType date;

		public IZType ZDate => date;

		#endregion
	}
}
