using System;
using NUnit.Framework;

namespace CargoWise.Data.Testing
{
	sealed class SqlFormatInfoTest : TestCase
	{
		public void TestFromSqlDate()
		{
			DateTime myBirthday = new DateTime(1971, 9, 18, 12, 30, 31);
			string myBirthdayAsSqlDateFormat = SqlFormatInfo.ToSqlDateString(myBirthday);
			AssertEquals("Format should be yyyy-MM-dd", "1971-09-18", myBirthdayAsSqlDateFormat);
			DateTime myBirthdayDateOnly = SqlFormatInfo.FromSqlDate(myBirthdayAsSqlDateFormat);
			AssertEquals(myBirthday.Date, myBirthdayDateOnly);
		}

		public void TestToSqlDateString()
		{
			DateTime aDateTime = new DateTime(2003, 03, 02, 13, 45, 55);
			AssertEquals("Format should be yyyy-MM-dd", "2003-03-02", SqlFormatInfo.ToSqlDateString(aDateTime));
		}

		public void TestFromSqlDateTime()
		{
			DateTime myBirthday = new DateTime(1971, 9, 18, 12, 30, 31);
			string myBirthdayAsSqlFormat = SqlFormatInfo.ToSqlDateTimeString(myBirthday);
			DateTime myBirthdayAgain = SqlFormatInfo.FromSqlDateTime(myBirthdayAsSqlFormat);
			AssertEquals(myBirthday, myBirthdayAgain);
		}

		public void TestFromSqlTime()
		{
			TimeSpan lunchtime = new TimeSpan(12, 30, 31);
			string lunchtimeAsSqlFormat = SqlFormatInfo.ToSqlTimeString(lunchtime);
			TimeSpan lunchtimeAgain = SqlFormatInfo.FromSqlTime(lunchtimeAsSqlFormat);
			AssertEquals(lunchtime, lunchtimeAgain);
		}

		public void TestToSqlDateTimeString()
		{
			DateTime aDateTime = new DateTime(2003, 03, 02, 13, 45, 55);
			AssertEquals("Format should be yyyy-MM-dd HH:mm:ss", "2003-03-02 13:45:55.000", SqlFormatInfo.ToSqlDateTimeString(aDateTime));
		}

		public void TestToSqlTimeString()
		{
			TimeSpan lunchtime = new TimeSpan(12, 30, 31);
			AssertEquals("Format should be HH:mm:ss", "12:30:31", SqlFormatInfo.ToSqlTimeString(lunchtime));
			TimeSpan preciseLunchtime = new TimeSpan(0, 12, 30, 31, 42);
			AssertEquals("Format should be HH:mm:ss", "12:30:31.0420000", SqlFormatInfo.ToSqlTimeString(preciseLunchtime));
		}

		public void TestToSqlDateTimeOffsetString()
		{
			DateTimeOffset dateTimeOffset1 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(1));
			AssertEquals("Format should be YYYY-MM-DD hh:mm:ss[.fff] [{+|-}hh:mm]", "2003-03-02 13:45:55.0000000 +01:00", SqlFormatInfo.ToSqlDateTimeOffsetString(dateTimeOffset1));
			AssertEquals(dateTimeOffset1, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55.0000000 +01:00"));
			DateTimeOffset dateTimeOffset2 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, TimeSpan.FromHours(-1));
			AssertEquals("Format should be YYYY-MM-DD hh:mm:ss[.fff] [{+|-}hh:mm]", "2003-03-02 13:45:55.0000000 -01:00", SqlFormatInfo.ToSqlDateTimeOffsetString(dateTimeOffset2));
			AssertEquals(dateTimeOffset2, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55.0000000 -01:00"));
			DateTimeOffset dateTimeOffset3 = new DateTimeOffset(2003, 03, 02, 13, 45, 55, new TimeSpan());
			AssertEquals("Format should be YYYY-MM-DD hh:mm:ss[.fff] [{+|-}hh:mm]", "2003-03-02 13:45:55.0000000 +00:00", SqlFormatInfo.ToSqlDateTimeOffsetString(dateTimeOffset3));

			//have checked and +00:00 is the canonical format for no/unspecified utc offset, returned from SQL Server	
			AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55.0000000 +00:00"));
			AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55.0000000 -00:00"));

			//TODO: SQL server returns .0000000 but we expect .000 in other places. Will this be a problem?
			/*AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55.0000000 +00:00"));
			AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55.0000000 -00:00"));*/

			//Other formats we don't currently support, I guess...
			/*AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55 +00:00"));
			AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55 -00:00"));
			AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55.0000000"));
			AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55.000"));
			AssertEquals(dateTimeOffset3, SqlFormatInfo.FromSqlDateTimeOffset("2003-03-02 13:45:55"));*/
		}
	}
}
