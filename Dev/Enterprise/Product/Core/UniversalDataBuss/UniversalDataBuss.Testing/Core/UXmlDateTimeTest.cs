using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Core.Testing
{
	class UXmlDateTimeTest : TestCaseWithFactory
	{
		public void TestHoldsDateTime()
		{
			var dt = new ZDateTime(2024, 1, 1);
			var xmlDate = new UXmlDateTime(dt);
			AssertEquals("Can convert back to ZDateTime", dt, xmlDate.ToZDateTime());

			var empty = new UXmlDateTime(ZDateTime.Empty);
			AssertEquals("Holds empty ZDateTime", ZDateTime.Empty, empty.ToZDateTime());

			var invalid = new UXmlDateTime(ZDateTime.Invalid);
			AssertEquals("Holds invalid ZDateTime", ZDateTime.Invalid, invalid.ToZDateTime());
		}

		public void TestHoldsDateTimeOffset()
		{
			var dt = ZDateTimeOffset.UtcNow;
			var xmlDate = new UXmlDateTime(dt);
			AssertEquals("Can convert back to ZDateTimeOffset", dt, xmlDate.ToZDateTimeOffset());

			var empty = new UXmlDateTime(ZDateTimeOffset.Empty);
			AssertEquals("Holds empty ZDateTimeOffset", ZDateTimeOffset.Empty, empty.ToZDateTimeOffset());

			var invalid = new UXmlDateTime(ZDateTimeOffset.Invalid);
			AssertEquals("Holds invalid ZDateTimeOffset", ZDateTimeOffset.Invalid, invalid.ToZDateTimeOffset());
		}

		[TestUtcOffset(-10,0,0)]
		public void TestDateTimeToOffset()
		{
			var utc = new ZDateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var xmlDate = new UXmlDateTime(utc);
			AssertEquals("Utc ZDateTime converts to the same ZDateTimeOffset", new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, 0, TimeSpan.Zero), xmlDate.ToZDateTimeOffset());

			var local = new ZDateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
			var xmlLocalDate = new UXmlDateTime(local);
			AssertEquals("ZDateTimeOffset calculated from ZDateTime based on local offset", new ZDateTimeOffset(2024, 1, 1, 10, 0, 0, 0, TimeSpan.Zero), xmlLocalDate.ToZDateTimeOffset());

			var unspecified = new ZDateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified);
			var xmlOtherDate = new UXmlDateTime(unspecified);
			AssertEquals("ZDateTimeOffset calculated from ZDateTime based on local offset if we don't know what kind of date was used to create the UXmlDateTime", new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, 0, TimeSpan.FromHours(-10)), xmlOtherDate.ToZDateTimeOffset());
		}

		public void TestToDateTimeOffsetWithParameters()
		{
			var refUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			var timeSpan = new TimeSpan(6,0,0);

			var unspecified = new ZDateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Unspecified);
			var xmlOtherDate = new UXmlDateTime(unspecified);

			AssertEquals("ZDateTimeOFfset is calculated by the TimeSpan offset(+6) and time is adjusted to be the same moment in time", new ZDateTimeOffset(2024, 1, 1, 8, 0, 0, 0, TimeSpan.FromHours(6)), xmlOtherDate.ToZDateTimeOffset(timeSpan));
			AssertEquals("ZDateTimeOffset is calculated by the refUNLOCO offset(+10) and time is adjusted to be the same moment in time", new ZDateTimeOffset(2024, 1, 1, 12, 0, 0, 0, TimeSpan.FromHours(10)), xmlOtherDate.ToZDateTimeOffset(refUNLOCO));
		}

		public void TestToDateTimeOffsetWithParameters_AndPresentOffset()
		{
			var refUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			var timeSpan = new TimeSpan(6, 0, 0);

			var offset = new TimeSpan(-4, 0, 0);
			var dTOffset = new ZDateTimeOffset(2024, 1, 1, 4, 0, 0, 0, offset);
			var uxmlOffset = new UXmlDateTime(dTOffset);

			AssertEquals("ZDateTimeOffset is calculated by the TimeSpan offset(+6) and time is adjusted to be the same moment in time", new ZDateTimeOffset(2024, 1, 1, 14, 0, 0, 0, TimeSpan.FromHours(6)), uxmlOffset.ToZDateTimeOffset(timeSpan));
			AssertEquals("ZDateTimeOffset is calculated by the refUNLOCO offset(+10) and time is adjusted to be the same moment in time", new ZDateTimeOffset(2024, 1, 1, 18, 0, 0, 0, TimeSpan.FromHours(10)), uxmlOffset.ToZDateTimeOffset(refUNLOCO));
		}

		public void TestToZDateTimeOffsetWithTheGivenOffsetIfNoneIsPresent()
		{
			var refUNLOCO = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUMEL");
			var timeSpan = new TimeSpan(6, 0, 0);

			var unspecified = new ZDateTime(2024, 1, 1, 12, 0, 0, 0, DateTimeKind.Unspecified);
			var xmlDate1 = new UXmlDateTime(unspecified);

			var dtOffset = new ZDateTimeOffset(2024, 1, 1, 12, 0, 0, 0, new TimeSpan(-4,0,0));
			var xmlDate3 = new UXmlDateTime(dtOffset);

			AssertEquals("Unspecified should add the offset and not change the time", new ZDateTimeOffset(2024, 1, 1, 12, 0, 0, 0, new TimeSpan(10, 0, 0)), xmlDate1.ToZDateTimeOffsetWithTheGivenOffsetIfNoneIsPresent(refUNLOCO));
			AssertEquals("Unspecified should add the offset and not change the time", new ZDateTimeOffset(2024, 1, 1, 12, 0, 0, 0, new TimeSpan(10, 0, 0)), xmlDate1.ToZDateTimeOffsetWithTheGivenOffsetIfNoneIsPresent(timeSpan));

			AssertEquals("DateTimeOffset should not be changed", dtOffset, xmlDate3.ToZDateTimeOffsetWithTheGivenOffsetIfNoneIsPresent(refUNLOCO));
			AssertEquals("DateTimeOffset should not be changed", dtOffset, xmlDate3.ToZDateTimeOffsetWithTheGivenOffsetIfNoneIsPresent(timeSpan));
		}

		public void TestOffsetToDateTime()
		{
			var offset = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, 0, TimeSpan.FromHours(10));
			var xmlDate = new UXmlDateTime(offset);
			var dt = xmlDate.ToZDateTime();

			AssertEquals("Converted to ZDateTime with offset ignored", new ZDateTime(2024, 1, 1, 0, 0, 0, 0), dt);
			AssertEquals("ZDateTime type is unspecified", DateTimeKind.Unspecified, dt.Kind);
		}

		public void TestImplicitConversions()
		{
			var dt = new ZDateTime(2024, 1, 1, DateTimeKind.Utc);
			UXmlDateTime xmlDate = dt;
			ZDateTimeOffset offset = xmlDate;
			UXmlDateTime newXmlDate = offset;
			ZDateTime newDate = newXmlDate;

			AssertEquals("Date is still the same", dt, newDate);
		}

		public void TestEquals()
		{
			AssertEquals("Compare UXmlDateTime", new UXmlDateTime(new ZDateTime(2015, 2, 3)), new UXmlDateTime(new ZDateTime(2015, 2, 3)));
			AssertEquals("Compare with ZDateTime", new ZDateTime(2015, 2, 3), new UXmlDateTime(new ZDateTime(2015, 2, 3)));
			AssertEquals("Compare with ZDateTimeOffset", new ZDateTimeOffset(2024, 2, 13, 0, 0, 0, TimeSpan.FromHours(12)), new UXmlDateTime(new ZDateTimeOffset(2024, 2, 13, 0, 0, 0, TimeSpan.FromHours(12))));
			AssertEquals("Compare with ZDate", new ZDate(1999, 4, 4), new UXmlDateTime(new ZDate(1999, 4, 4)));
			AssertEquals("Compare with DateTime", new DateTime(2024, 2, 13, 10, 30, 0), new UXmlDateTime(new DateTime(2024, 2, 13, 10, 30, 0)));
		}

		public void TestToLocalTime()
		{
			var dt = new ZDateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var xmlDate1 = new UXmlDateTime(dt);
			var offset = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, 0, TimeSpan.FromHours(10));
			var xmlDate2 = new UXmlDateTime(offset);

			ZDateTime localDate1 = xmlDate1.ToLocalTime(new TimeSpan(10, 1, 1));
			AssertEquals("Converted to Local Time with TimeSpan", new ZDateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) , localDate1);

			ZDateTime localDate2 = xmlDate2.ToLocalTime(new TimeSpan(10, 1, 1));
			AssertEquals("Converted to Local Time with TimeSpan", new ZDateTime(2024, 1, 1, 0, 1, 1, 0, DateTimeKind.Local), localDate2);

			var unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			localDate1 = xmlDate1.ToLocalTime(unloco);
			AssertEquals("Converted to Local Time with UNLOCO", new ZDateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), localDate1);
			localDate2 = xmlDate2.ToLocalTime(unloco);
			AssertEquals("Converted to Local Time with UNLOCO", new ZDateTime(2023, 12, 31, 15, 0, 0, 0, DateTimeKind.Local), localDate2);

			var unlocoWithNonIntegerUTCOffset = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_IATARegionCode, "BGL"));
			localDate2 = xmlDate2.ToLocalTime(unlocoWithNonIntegerUTCOffset);
			AssertEquals("Converted to Local Time with UNLOCO", new ZDateTime(2023, 12, 31, 19, 45, 0, 0, DateTimeKind.Local), localDate2);
		}

		public void TestIsValid()
		{
			var dt = new ZDateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var xmlDate1 = new UXmlDateTime(dt);
			var offset = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, 0, TimeSpan.FromHours(10));
			var xmlDate2 = new UXmlDateTime(offset);
			Assert(xmlDate1.IsValid);
			Assert(xmlDate2.IsValid);
			Assert(!new UXmlDateTime(ZDateTime.Empty).IsValid);
			Assert(!new UXmlDateTime(ZDateTimeOffset.Empty).IsValid);
			Assert(!new UXmlDateTime(ZDateTime.Invalid).IsValid);
			Assert(!new UXmlDateTime(ZDateTimeOffset.Invalid).IsValid);
		}

		public void TestToUTCTime()
		{
			var dt = new ZDateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var xmlDate1 = new UXmlDateTime(dt);
			var offset = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, 0, TimeSpan.FromHours(10));
			var xmlDate2 = new UXmlDateTime(offset);

			ZDateTime utcDate1 = xmlDate1.ToUTCTime(new TimeSpan(10, 0, 0));
			AssertEquals("Converted to UTC Time with TimeSpan", new ZDateTime(2023, 12, 31, 14, 0, 0, 0, DateTimeKind.Utc), utcDate1);

			ZDateTime utcDate2 = xmlDate2.ToUTCTime(new TimeSpan(10, 1, 1));
			AssertEquals("Converted to UTC Time with TimeSpan", new ZDateTime(2023, 12, 31, 14, 0, 0, 0, DateTimeKind.Utc), utcDate2);

			var unloco = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			utcDate1 = xmlDate1.ToUTCTime(unloco);
			AssertEquals("Converted to UTC Time with UNLOCO", new ZDateTime(2023, 12, 31, 23, 0, 0, 0, DateTimeKind.Utc), utcDate1);
			utcDate2 = xmlDate2.ToUTCTime(unloco);
			AssertEquals("Converted to UTC Time with UNLOCO", new ZDateTime(2023, 12, 31, 14, 0, 0, 0, DateTimeKind.Utc), utcDate2);
			AssertEquals(ZDateTime.Empty, new UXmlDateTime(ZDateTime.Empty).ToUTCTime(unloco));
			AssertEquals(ZDateTime.Empty, new UXmlDateTime(ZDateTimeOffset.Empty).ToUTCTime(unloco));

			var unlocoWithNonIntegerUTCOffset = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_IATARegionCode, "BGL"));
			utcDate1 = xmlDate1.ToUTCTime(unlocoWithNonIntegerUTCOffset);
			AssertEquals("Converted to UTC Time with UNLOCO", new ZDateTime(2023, 12, 31, 18, 15, 0, 0, DateTimeKind.Utc), utcDate1);
			AssertEquals(ZDateTime.Empty, new UXmlDateTime(ZDateTime.Empty).ToUTCTime(new TimeSpan(10, 0, 0)));
			AssertEquals(ZDateTime.Empty, new UXmlDateTime(ZDateTimeOffset.Empty).ToUTCTime(new TimeSpan(10, 0, 0)));
		}

		public void TestComparisonOperators()
		{
			var dt1 = new ZDateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var dt2 = new ZDateTime(2024, 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var offset1 = new ZDateTimeOffset(2024, 1, 1, 4, 0, 0, 0, TimeSpan.FromHours(2));
			var offset2 = new ZDateTimeOffset(2024, 1, 1, 13, 0, 0, 0, TimeSpan.FromHours(10));

			var xmlDate1 = new UXmlDateTime(dt1);
			var xmlDate2 = new UXmlDateTime(dt2);
			var xmlOffset1 = new UXmlDateTime(offset1);
			var xmlOffset2 = new UXmlDateTime(offset2);

			Assert("Operator < ZDateTime and ZDateTime", xmlDate1 < xmlDate2);
			Assert("Operator > ZDateTime and ZDateTime", xmlDate2 > xmlDate1);
			Assert("Operator <= ZDateTime and ZDateTime", xmlDate1 <= xmlDate2);
			Assert("Operator >= ZDateTime and ZDateTime", xmlDate2 >= xmlDate1);

			Assert("Operator < ZDateTime and ZDateTimeOffset", xmlDate1 < xmlOffset1);
			Assert("Operator > ZDateTimeOffset and ZDateTime", xmlOffset1 > xmlDate1);
			Assert("Operator <= ZDateTime and ZDateTimeOffset", xmlDate1 <= xmlOffset1);
			Assert("Operator >= ZDateTimeOffset and ZDateTime", xmlOffset1 >= xmlDate1);

			Assert("Operator < ZDateTimeOffset and ZDateTimeOffset", xmlOffset1 < xmlOffset2);
			Assert("Operator > ZDateTimeOffset and ZDateTimeOffset", xmlOffset2 > xmlOffset1);
			Assert("Operator <= ZDateTimeOffset and ZDateTimeOffset", xmlOffset1 <= xmlOffset2);
			Assert("Operator >= ZDateTimeOffset and ZDateTimeOffset", xmlOffset2 >= xmlOffset1);

			AssertEquals(-1, xmlDate1.CompareTo(xmlDate2));
			AssertEquals(1, xmlDate2.CompareTo(xmlDate1));
			AssertEquals(-1, xmlOffset1.CompareTo(xmlOffset2));
			AssertEquals(1, xmlOffset2.CompareTo(xmlOffset1));

			xmlDate1 = new UXmlDateTime(dt1.AddHours(1));
			Assert("Operator <= ZDateTime and ZDateTime", xmlDate1 <= xmlDate2);
			Assert("Operator >= ZDateTime and ZDateTime", xmlDate2 >= xmlDate1);
			Assert("Operator <= ZDateTime and ZDateTimeOffset", xmlDate1 <= xmlOffset1);
			Assert("Operator >= ZDateTimeOffset and ZDateTime", xmlOffset1 >= xmlDate1);

			xmlOffset2 = new UXmlDateTime(offset2.AddHours(-1));
			Assert("Operator <= ZDateTimeOffset and ZDateTimeOffset", xmlOffset1 <= xmlOffset2);
			Assert("Operator >= ZDateTimeOffset and ZDateTimeOffset", xmlOffset2 >= xmlOffset1);

			AssertEquals(0, xmlDate1.CompareTo(xmlDate2));
			AssertEquals(0, xmlDate2.CompareTo(xmlDate1));
			AssertEquals(0, xmlOffset1.CompareTo(xmlOffset2));
			AssertEquals(0, xmlOffset2.CompareTo(xmlOffset1));
		}

		public void TestDatePart()
		{
			var dt1 = new ZDateTimeOffset(2024, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(2));
			var xmlDate1 = new UXmlDateTime(dt1);

			AssertEquals("Year", 2024, xmlDate1.Year);
			AssertEquals("Month", 1, xmlDate1.Month);
			AssertEquals("Day", 2, xmlDate1.Day);
			AssertEquals("Hour", 3, xmlDate1.Hour);
			AssertEquals("Minute", 4, xmlDate1.Minute);
			AssertEquals("Second", 5, xmlDate1.Second);
			AssertEquals("Millisecond", 6, xmlDate1.Millisecond);
			AssertEquals("Offset", TimeSpan.FromHours(2), xmlDate1.Offset);

			var dt2 = new ZDateTime(2024, 1, 2, 3, 4, 5, 6);
			var xmlDate2 = new UXmlDateTime(dt2);

			AssertEquals("Year", 2024, xmlDate2.Year);
			AssertEquals("Month", 1, xmlDate2.Month);
			AssertEquals("Day", 2, xmlDate2.Day);
			AssertEquals("Hour", 3, xmlDate2.Hour);
			AssertEquals("Minute", 4, xmlDate2.Minute);
			AssertEquals("Second", 5, xmlDate2.Second);
			AssertEquals("Millisecond", 6, xmlDate2.Millisecond);
		}

		public void TestToUtcZDateTime()
		{
			var dt = new ZDateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
			var xmlDate1 = new UXmlDateTime(dt);
			var offset = new ZDateTimeOffset(2024, 1, 1, 0, 0, 0, 0, TimeSpan.FromHours(10));
			var xmlDate2 = new UXmlDateTime(offset);

			var utcDate1 = xmlDate1.ToUtcZDateTime();
			AssertEquals("Converted ZDateTime to UtcZDateTime with TimeSpan", new ZDateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), utcDate1);

			var utcDate2 = xmlDate2.ToUtcZDateTime();
			AssertEquals("Converted ZDateTimeOffset to UtcZDateTime with TimeSpan", new ZDateTime(2023, 12, 31, 14, 0, 0, 0, DateTimeKind.Utc), utcDate2);
		}
	}
}
