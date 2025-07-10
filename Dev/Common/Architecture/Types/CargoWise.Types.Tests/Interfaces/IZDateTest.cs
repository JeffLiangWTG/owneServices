namespace CargoWise.Types.Tests
{
	using System;
	using NUnit.Framework;

	public class IZDateTest : TestCase
	{
		public void TestNewIZDate()
		{
			AssertEquals(ZDateTime.Empty, new ZDateTime());
			AssertEquals(ZDateTimeOffset.Empty, new ZDateTimeOffset());
		}

		public void TestInvalid()
		{
			foreach (var value in zDateTimeValues)
			{
				AssertEquals(ZDateTime.Invalid, value.Invalid);
			}

			foreach (var value in zDateTimeOffsetValues)
			{
				AssertEquals(ZDateTimeOffset.Invalid, value.Invalid);
			}
		}

		public void TestDefault()
		{
			foreach (var value in zDateTimeValues)
			{
				AssertEquals(ZDateTime.Empty, value.Default);
			}

			foreach (var value in zDateTimeOffsetValues)
			{
				AssertEquals(ZDateTimeOffset.Empty, value.Default);
			}
		}

		[TestDate(2024, 7, 29, 10, 0, 0)]
		[TestUtcOffset(8, 0, 0)]
		public void TestNow()
		{
			foreach (var value in zDateTimeValues)
			{
				AssertEquals(new ZDateTime(2024, 7, 29, 18, 0, 0, DateTimeKind.Local), value.Now);
			}

			foreach (var value in zDateTimeOffsetValues)
			{
				AssertEquals(new ZDateTimeOffset(2024, 7, 29, 18, 0, 0, TimeSpan.FromHours(8)), value.Now);

				AssertEquals(TimeSpan.FromHours(8), (value.Now as ZDateTimeOffset?).Value.Offset);
			}
		}

		[TestDate(2024, 7, 29, 18, 0, 0)]
		[TestUtcOffset(8, 0, 0)]
		public void TestToday()
		{
			foreach (var value in zDateTimeValues)
			{
				AssertEquals(new ZDateTime(2024, 7, 30, 0, 0, 0, DateTimeKind.Local), value.Today);
			}

			foreach (var value in zDateTimeOffsetValues)
			{
				AssertEquals(new ZDateTimeOffset(2024, 7, 30, 0, 0, 0, TimeSpan.FromHours(8)), value.Today);

				AssertEquals(TimeSpan.FromHours(8), (value.Now as ZDateTimeOffset?).Value.Offset);
			}
		}

		public void TestSqlFormat()
		{
			AssertEquals("2024-07-15 12:30:30.000", (new ZDateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local) as IZDate).SqlFormat);
			AssertEquals("2024-07-15 12:30:30.0000000 +08:00", (new ZDateTimeOffset(2024, 7, 15, 12, 30, 30, TimeSpan.FromHours(8)) as IZDate).SqlFormat);
		}

		public void TestAddDays()
		{
			AssertEquals(new ZDateTime(2024, 7, 2, 0, 0, 0, DateTimeKind.Local), (new ZDateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local) as IZDate).AddDays(1));
			AssertEquals(new ZDateTimeOffset(2024, 7, 2, 0, 0, 0, TimeSpan.FromHours(8)), (new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)) as IZDate).AddDays(1));

			AssertEquals(new ZDateTime(2024, 6, 30, 0, 0, 0, DateTimeKind.Local), (new ZDateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local) as IZDate).AddDays(-1));
			AssertEquals(new ZDateTimeOffset(2024, 6, 30, 0, 0, 0, TimeSpan.FromHours(8)), (new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)) as IZDate).AddDays(-1));
		}

		public void TestAddMonths()
		{
			AssertEquals(new ZDateTime(2024, 8, 1, 0, 0, 0, DateTimeKind.Local), (new ZDateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local) as IZDate).AddMonths(1));
			AssertEquals(new ZDateTimeOffset(2024, 8, 1, 0, 0, 0, TimeSpan.FromHours(8)), (new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)) as IZDate).AddMonths(1));

			AssertEquals(new ZDateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Local), (new ZDateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local) as IZDate).AddMonths(-1));
			AssertEquals(new ZDateTimeOffset(2024, 6, 1, 0, 0, 0, TimeSpan.FromHours(8)), (new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)) as IZDate).AddMonths(-1));
		}

		public void TestAddYears()
		{
			AssertEquals(new ZDateTime(2025, 7, 1, 0, 0, 0, DateTimeKind.Local), (new ZDateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local) as IZDate).AddYears(1));
			AssertEquals(new ZDateTimeOffset(2025, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), (new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)) as IZDate).AddYears(1));

			AssertEquals(new ZDateTime(2023, 7, 1, 0, 0, 0, DateTimeKind.Local), (new ZDateTime(2024, 7, 1, 0, 0, 0, DateTimeKind.Local) as IZDate).AddYears(-1));
			AssertEquals(new ZDateTimeOffset(2023, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)), (new ZDateTimeOffset(2024, 7, 1, 0, 0, 0, TimeSpan.FromHours(8)) as IZDate).AddYears(-1));
		}

		public void TestToISO8601String()
		{
			AssertEquals("2024-07-15T12:30:30", (new ZDateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local) as IZDate).ToISO8601String());
			AssertEquals("2024-07-15T12:30:30.0000000+08:00", (new ZDateTimeOffset(2024, 7, 15, 12, 30, 30, TimeSpan.FromHours(8)) as IZDate).ToISO8601String());
		}
		public void TestToString()
		{
			AssertEquals("15-Jul-24 12:30", (new ZDateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local) as IZDate).ToString("dd-MMM-yy HH:mm", null));
			AssertEquals("15-Jul-24", (new ZDateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local) as IZDate).ToString("dd-MMM-yy", null));
			AssertEquals("15-Jul-24 12:30 +08:00", (new ZDateTimeOffset(2024, 7, 15, 12, 30, 30, TimeSpan.FromHours(8)) as IZDate).ToString("dd-MMM-yy HH:mm zzz", null));
		}

		public void TestTryParse()
		{
			foreach (var value in zDateTimeValues)
			{
				AssertEquals(true, value.TryParse("2024-07-15T12:30:30", out var valueZDateTime));
				AssertEquals(new ZDateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local), valueZDateTime);
			}
			foreach (var value in zDateTimeOffsetValues)
			{
				AssertEquals(true, value.TryParse("2024-07-15T12:30:30.0000000+08:00", out var valueZDateTimeOffset));
				AssertEquals(new ZDateTimeOffset(2024, 7, 15, 12, 30, 30, TimeSpan.FromHours(8)), valueZDateTimeOffset);
			}
		}

		[TestDate(2024, 7, 15, 12, 30, 30)]
		[TestUtcOffset(8, 0, 0)]
		public void TestFirstDayOfLastCalendarYear()
		{
			foreach (var value in zDateTimeValues)
			{
				AssertEquals(new ZDateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Local), value.FirstDayOfLastCalendarYear);
			}

			foreach (var value in zDateTimeOffsetValues)
			{
				AssertEquals(new ZDateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.FromHours(8)), value.FirstDayOfLastCalendarYear);

				AssertEquals(TimeSpan.FromHours(8), (value.FirstDayOfLastCalendarYear as ZDateTimeOffset?).Value.Offset);
			}
		}

		[TestDate(2024, 7, 15, 12, 30, 30)]
		[TestUtcOffset(8, 0, 0)]
		public void TestLastDayOfLastCalendarYear()
		{
			foreach (var value in zDateTimeValues)
			{
				AssertEquals(new ZDateTime(2023, 12, 31, 0, 0, 0, DateTimeKind.Local), value.LastDayOfLastCalendarYear);
			}

			foreach (var value in zDateTimeOffsetValues)
			{
				AssertEquals(new ZDateTimeOffset(2023, 12, 31, 0, 0, 0, TimeSpan.FromHours(8)), value.LastDayOfLastCalendarYear);

				AssertEquals(TimeSpan.FromHours(8), (value.LastDayOfLastCalendarYear as ZDateTimeOffset?).Value.Offset);
			}
		}

		[TestUtcOffset(8, 0, 0)]
		public void TestEndOfDay()
		{
			AssertEquals(new ZDateTime(2024, 7, 15, 23, 59, 59, DateTimeKind.Local), (new ZDateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local) as IZDate).EndOfDay());
			AssertEquals(new ZDateTimeOffset(2024, 7, 15, 23, 59, 59, TimeSpan.FromHours(8)), (new ZDateTimeOffset(2024, 7, 15, 12, 30, 30, TimeSpan.FromHours(8)) as IZDate).EndOfDay());
		}

		public void TestToBaseDate()
		{
			AssertEquals(new DateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local), (new ZDateTime(2024, 7, 15, 12, 30, 30, DateTimeKind.Local) as IZDate).ToBaseDate());
			AssertEquals(new DateTimeOffset(2024, 7, 15, 12, 30, 30, TimeSpan.FromHours(8)), (new ZDateTimeOffset(2024, 7, 15, 12, 30, 30, TimeSpan.FromHours(8)) as IZDate).ToBaseDate());
		}

		readonly IZDate[] zDateTimeValues = { new ZDateTime(), ZDateTime.Empty, ZDateTime.Invalid, new ZDateTime(2024, 7, 29, 10, 10, 10) };
		readonly IZDate[] zDateTimeOffsetValues = { new ZDateTimeOffset(), ZDateTimeOffset.Empty, ZDateTimeOffset.Invalid, new ZDateTimeOffset(2024, 7, 29, 10, 10, 10, TimeSpan.FromHours(8)) };
	}
}
