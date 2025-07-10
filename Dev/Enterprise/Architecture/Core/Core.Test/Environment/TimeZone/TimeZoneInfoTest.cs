using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeZoneInfoTest : TestCase
	{
		public void TestToLocalTimeFromUtc_TimeZoneAheadOfUtc()
		{
			Guid sydDstZonePk = GetDstZonePkFromPort("AUSYD");
			TimeZoneInfoForTesting testZone = new TimeZoneInfoForTesting("Australia/Sydney", 10m, 11m, sydDstZonePk);

			// DST START IN 2006 - Last Sunday of October @2am in Standard time => 29/10/2006 @2am
			// UTC = 28/10/2006 @4pm

			DateTime utcDateTime = new DateTime(2006, 10, 28, 15, 0, 0);
			AssertEquals("UTC 28/10/2006 @3pm - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @3pm - Should be standard offset", new TimeSpan(0, 0, 10 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @3pm - Local Time:", new DateTime(2006, 10, 29, 1, 0, 0), testZone.ToLocalTime(utcDateTime));
			AssertEquals("UTC 28/10/2006 @3pm - Local Time Offset:", new DateTimeOffset(2006, 10, 29, 1, 0, 0, TimeSpan.FromHours(10)), testZone.ToLocalTimeOffset(utcDateTime));

			utcDateTime = new DateTime(2006, 10, 28, 15, 1, 0);
			AssertEquals("UTC 28/10/2006 @3:01pm - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @3:01pm - Should be standard offset", new TimeSpan(0, 0, 10 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @3:01pm - Local Time:", new DateTime(2006, 10, 29, 1, 1, 0), testZone.ToLocalTime(utcDateTime));
			AssertEquals("UTC 28/10/2006 @3pm - Local Time Offset:", new DateTimeOffset(2006, 10, 29, 1, 1, 0, TimeSpan.FromHours(10)), testZone.ToLocalTimeOffset(utcDateTime));

			utcDateTime = new DateTime(2006, 10, 28, 15, 59, 0);
			AssertEquals("UTC 28/10/2006 @3:59pm - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @3:59pm - Should be standard offset", new TimeSpan(0, 0, 10 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @3:59pm - Local Time:", new DateTime(2006, 10, 29, 1, 59, 0), testZone.ToLocalTime(utcDateTime));
			AssertEquals("UTC 28/10/2006 @3pm - Local Time Offset:", new DateTimeOffset(2006, 10, 29, 1, 59, 0, TimeSpan.FromHours(10)), testZone.ToLocalTimeOffset(utcDateTime));

			utcDateTime = new DateTime(2006, 10, 28, 16, 0, 0);
			AssertEquals("UTC 28/10/2006 @4pm - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @4pm - Should be DST offset", new TimeSpan(0, 0, 11 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @4pm - Local Time:", new DateTime(2006, 10, 29, 3, 0, 0), testZone.ToLocalTime(utcDateTime));
			AssertEquals("UTC 28/10/2006 @3pm - Local Time Offset:", new DateTimeOffset(2006, 10, 29, 3, 0, 0, TimeSpan.FromHours(11)), testZone.ToLocalTimeOffset(utcDateTime));

			utcDateTime = new DateTime(2006, 10, 28, 16, 1, 0);
			AssertEquals("UTC 28/10/2006 @4:01pm - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @4:01pm - Should be DST offset", new TimeSpan(0, 0, 11 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @4:01pm - Local Time:", new DateTime(2006, 10, 29, 3, 1, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2006, 10, 28, 17, 0, 0);
			AssertEquals("UTC 28/10/2006 @5pm - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @5pm - Should be DST offset", new TimeSpan(0, 0, 11 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @5pm - Local Time:", new DateTime(2006, 10, 29, 4, 0, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2006, 10, 28, 17, 1, 0);
			AssertEquals("UTC 28/10/2006 @5:01pm - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @5:01pm - Should be DST offset", new TimeSpan(0, 0, 11 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 28/10/2006 @5:01pm - Local Time:", new DateTime(2006, 10, 29, 4, 1, 0), testZone.ToLocalTime(utcDateTime));

			// DST END IN 2006 - 1st Sunday of April @2am in Standard time => 02/04/2006 @2am
			// UTC = 01/04/2006 @4pm

			utcDateTime = new DateTime(2006, 4, 1, 15, 0, 0);
			AssertEquals("UTC 01/04/2006 @3pm - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @3pm - Should be DST offset", new TimeSpan(0, 0, 11 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @3pm - Local Time:", new DateTime(2006, 4, 2, 2, 0, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2006, 4, 1, 15, 1, 0);
			AssertEquals("UTC 01/04/2006 @3:01pm - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @3:01pm - Should be DST offset", new TimeSpan(0, 0, 11 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @3:01pm - Local Time:", new DateTime(2006, 4, 2, 2, 1, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2006, 4, 1, 15, 59, 0);
			AssertEquals("UTC 01/04/2006 @3:59pm - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @3:59pm - Should be DST offset", new TimeSpan(0, 0, 11 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @3:59pm - Local Time:", new DateTime(2006, 4, 2, 2, 59, 0), testZone.ToLocalTime(utcDateTime));
			AssertEquals("UTC 28/10/2006 @3pm - Local Time Offset:", new DateTimeOffset(2006, 4, 2, 2, 59, 0, TimeSpan.FromHours(11)), testZone.ToLocalTimeOffset(utcDateTime));

			utcDateTime = new DateTime(2006, 4, 1, 16, 0, 0);
			AssertEquals("UTC 01/04/2006 @4pm - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @4pm - Should be standard offset", new TimeSpan(0, 0, 10 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @4pm - Local Time:", new DateTime(2006, 4, 2, 2, 0, 0), testZone.ToLocalTime(utcDateTime));
			AssertEquals("UTC 28/10/2006 @3pm - Local Time Offset:", new DateTimeOffset(2006, 4, 2, 2, 0, 0, TimeSpan.FromHours(10)), testZone.ToLocalTimeOffset(utcDateTime));

			utcDateTime = new DateTime(2006, 4, 1, 16, 1, 0);
			AssertEquals("UTC 01/04/2006 @4:01pm - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @4:01pm - Should be standard offset", new TimeSpan(0, 0, 10 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @4:01pm - Local Time:", new DateTime(2006, 4, 2, 2, 1, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2006, 4, 1, 17, 0, 0);
			AssertEquals("UTC 01/04/2006 @5pm - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @5pm - Should be standard offset", new TimeSpan(0, 0, 10 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @5pm - Local Time:", new DateTime(2006, 4, 2, 3, 0, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2006, 4, 1, 17, 1, 0);
			AssertEquals("UTC 01/04/2006 @5:01pm - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @5:01pm - Should be standard offset", new TimeSpan(0, 0, 10 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 01/04/2006 @5:01pm - Local Time:", new DateTime(2006, 4, 2, 3, 1, 0), testZone.ToLocalTime(utcDateTime));
		}

		public void TestToLocalTimeFromUtc_TimeZoneBehindUtc()
		{
			Guid saoPauloDstZonePk = GetDstZonePkFromPort("BRSAO");
			TimeZoneInfoForTesting testZone = new TimeZoneInfoForTesting("America/Sao_Paulo", -3m, -2m, saoPauloDstZonePk);

			// DST START IN 2006 - 3rd Sunday of October @12am(0:00) in Local time => 15/10/2006 @12am
			// UTC = 15/10/2006 @3am

			DateTime utcDateTime = new DateTime(2006, 10, 15, 2, 0, 0);
			AssertEquals("UTC 15/10/2006 @2am - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @2am - Should be standard offset", new TimeSpan(0, 0, -3 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @2am - Local Time:", new DateTime(2006, 10, 14, 23, 0, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2006, 10, 15, 2, 1, 0);
			AssertEquals("UTC 15/10/2006 @2:01am - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @2:01am - Should be standard offset", new TimeSpan(0, 0, -3 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @2:01am - Local Time:", new DateTime(2006, 10, 14, 23, 1, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2006, 10, 15, 2, 59, 0);
			AssertEquals("UTC 15/10/2006 @2:59am - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @2:59am - Should be standard offset", new TimeSpan(0, 0, -3 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @2:59am - Local Time:", new DateTime(2006, 10, 14, 23, 59, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2006, 10, 15, 3, 0, 0);
			AssertEquals("UTC 15/10/2006 @3am - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @3am - Should be DST offset", new TimeSpan(0, 0, -2 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @3am - Local Time:", new DateTime(2006, 10, 15, 1, 0, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2006, 10, 15, 3, 1, 0);
			AssertEquals("UTC 15/10/2006 @3:01am - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @3:01am - Should be DST offset", new TimeSpan(0, 0, -2 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @3:01am - Local Time:", new DateTime(2006, 10, 15, 1, 1, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2006, 10, 15, 4, 0, 0);
			AssertEquals("UTC 15/10/2006 @4am - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @4am - Should be DST offset", new TimeSpan(0, 0, -2 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @4am - Local Time:", new DateTime(2006, 10, 15, 2, 0, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2006, 10, 15, 4, 1, 0);
			AssertEquals("UTC 15/10/2006 @4:01am - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @4:01am - Should be DST offset", new TimeSpan(0, 0, -2 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 15/10/2006 @4:01am - Local Time:", new DateTime(2006, 10, 15, 2, 1, 0), testZone.ToLocalTime(utcDateTime));

			// DST END IN 2007 - 3rd Sunday of February @12am(0:00) in Local time => 18/02/2007 @12am
			// UTC = 18/02/2007 @2am

			utcDateTime = new DateTime(2007, 2, 18, 1, 0, 0);
			AssertEquals("UTC 18/02/2007 @1am - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @1am - Should be DST offset", new TimeSpan(0, 0, -2 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @1am - Local Time:", new DateTime(2007, 2, 17, 23, 0, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2007, 2, 18, 1, 1, 0);
			AssertEquals("UTC 18/02/2007 @1:01am - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @1:01am - Should be DST offset", new TimeSpan(0, 0, -2 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @1:01am - Local Time:", new DateTime(2007, 2, 17, 23, 1, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2007, 2, 18, 1, 59, 0);
			AssertEquals("UTC 18/02/2007 @1:59am - Daylight Saving:", true, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @1:59am - Should be DST offset", new TimeSpan(0, 0, -2 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @1:59am - Local Time:", new DateTime(2007, 2, 17, 23, 59, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2007, 2, 18, 2, 0, 0);
			AssertEquals("UTC 18/02/2007 @2am - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @2am - Should be standard offset", new TimeSpan(0, 0, -3 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @2am - Local Time:", new DateTime(2007, 2, 17, 23, 0, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2007, 2, 18, 2, 1, 0);
			AssertEquals("UTC 18/02/2007 @2:01am - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @2:01am - Should be standard offset", new TimeSpan(0, 0, -3 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @2:01am - Local Time:", new DateTime(2007, 2, 17, 23, 1, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2007, 2, 18, 3, 0, 0);
			AssertEquals("UTC 18/02/2007 @3am - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @3am - Should be standard offset", new TimeSpan(0, 0, -3 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @3am - Local Time:", new DateTime(2007, 2, 18, 0, 0, 0), testZone.ToLocalTime(utcDateTime));

			utcDateTime = new DateTime(2007, 2, 18, 3, 1, 0);
			AssertEquals("UTC 18/02/2007 @3:01am - Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @3:01am - Should be standard offset", new TimeSpan(0, 0, -3 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("UTC 18/02/2007 @3:01am - Local Time:", new DateTime(2007, 2, 18, 0, 1, 0), testZone.ToLocalTime(utcDateTime));
		}

		public void TestToLocalTimeFromUtc_NoDstTimeZone()
		{
			TimeZoneInfoForTesting testZone = new TimeZoneInfoForTesting("TestZone/NoDaylightSaving", 3m, 3m, Guid.Empty);

			DateTime utcDateTime = DateTime.UtcNow; // Getting a ramdon UTC from client PC for tests
			AssertEquals("Daylight Saving:", false, testZone.IsDaylightSavingBasedOnUtc(utcDateTime));
			AssertEquals("UTC Offset", new TimeSpan(0, 0, 3 * 3600), testZone.GetUtcOffsetBasedOnUtc(utcDateTime));
			AssertEquals("Local Time:", utcDateTime.AddHours(3), testZone.ToLocalTime(utcDateTime));
		}

		public void TestToUtcFromCurrentBranchLocalTime_TimeZoneAheadOfUtc()
		{
			// Sydney - AUSYD
			Guid dstZonePk = GetDstZonePkFromPort("AUSYD");
			TimeZoneInfoForTesting testZone = new TimeZoneInfoForTesting("Australia/Sydney", 10m, 11m, dstZonePk);

			// DST START IN 2006 - Last Sunday of October @2am in Standard time => 29/10/2006 @2am
			// Local Time = 29/10/2006 @2am

			// Before DST Start
			DateTime localDateTime = new DateTime(2006, 10, 29, 1, 30, 0);
			TimeSpan tentativeUtcOffset = testZone.GetUtcOffsetBasedOnUtc(localDateTime);
			DateTime tentativeUtc = localDateTime.Subtract(tentativeUtcOffset);
			TimeSpan checkUtcOffset = testZone.GetUtcOffsetBasedOnUtc(tentativeUtc);
			DateTime actualUtc = localDateTime.Subtract(checkUtcOffset);
			AssertEquals("Local Time = 29/10/2006 @1:30am => Tentative UTC:", new DateTime(2006, 10, 28, 14, 30, 0), tentativeUtc);
			AssertEquals("Local Time = 29/10/2006 @1:30am => Actual UTC:", new DateTime(2006, 10, 28, 15, 30, 0), actualUtc);

			// After DST Start
			localDateTime = new DateTime(2006, 10, 29, 3, 0, 0);
			tentativeUtcOffset = testZone.GetUtcOffsetBasedOnUtc(localDateTime);
			tentativeUtc = localDateTime.Subtract(tentativeUtcOffset);
			checkUtcOffset = testZone.GetUtcOffsetBasedOnUtc(tentativeUtc);
			actualUtc = localDateTime.Subtract(checkUtcOffset);
			AssertEquals("Local Time = 29/10/2006 @3am => Tentative UTC:", new DateTime(2006, 10, 28, 16, 0, 0), tentativeUtc);
			AssertEquals("Local Time = 29/10/2006 @3am => Actual UTC:", new DateTime(2006, 10, 28, 16, 0, 0), actualUtc);

			// DST END IN 2006 - 1st Sunday of April @2am in Standard time => 02/04/2006 @2am
			// Local Time = 02/04/2006 @3am

			// Before DST End
			localDateTime = new DateTime(2006, 4, 2, 1, 0, 0);
			tentativeUtcOffset = testZone.GetUtcOffsetBasedOnUtc(localDateTime);
			tentativeUtc = localDateTime.Subtract(tentativeUtcOffset);
			checkUtcOffset = testZone.GetUtcOffsetBasedOnUtc(tentativeUtc);
			actualUtc = localDateTime.Subtract(checkUtcOffset);
			AssertEquals("Local Time = 02/04/2006 @1am => Tentative UTC:", new DateTime(2006, 4, 1, 15, 0, 0), tentativeUtc);
			AssertEquals("Local Time = 02/04/2006 @1am => Actual UTC:", new DateTime(2006, 4, 1, 14, 0, 0), actualUtc);

			// After DST End
			localDateTime = new DateTime(2006, 4, 2, 2, 30, 0);
			tentativeUtcOffset = testZone.GetUtcOffsetBasedOnUtc(localDateTime);
			tentativeUtc = localDateTime.Subtract(tentativeUtcOffset);
			checkUtcOffset = testZone.GetUtcOffsetBasedOnUtc(tentativeUtc);
			actualUtc = localDateTime.Subtract(checkUtcOffset);
			AssertEquals("Local Time = 02/04/2006 @2:30am => Tentative UTC:", new DateTime(2006, 4, 1, 16, 30, 0), tentativeUtc);
			AssertEquals("Local Time = 02/04/2006 @2:30am => Actual UTC:", new DateTime(2006, 4, 1, 16, 30, 0), actualUtc);

			// After DST End
			localDateTime = new DateTime(2006, 4, 2, 3, 0, 0);
			tentativeUtcOffset = testZone.GetUtcOffsetBasedOnUtc(localDateTime);
			tentativeUtc = localDateTime.Subtract(tentativeUtcOffset);
			checkUtcOffset = testZone.GetUtcOffsetBasedOnUtc(tentativeUtc);
			actualUtc = localDateTime.Subtract(checkUtcOffset);
			AssertEquals("Local Time = 02/04/2006 @3am => Tentative UTC:", new DateTime(2006, 4, 1, 17, 0, 0), tentativeUtc);
			AssertEquals("Local Time = 02/04/2006 @3am => Actual UTC:", new DateTime(2006, 4, 1, 17, 0, 0), actualUtc);
		}

		public void TestToUtcFromCurrentBranchLocalTime_TimeZoneBehindUtc()
		{
			// Porto Alegre - BRPOA
			Guid dstZonePk = GetDstZonePkFromPort("BRPOA");
			TimeZoneInfoForTesting testZone = new TimeZoneInfoForTesting("America/Sao_Paulo", -3m, -2m, dstZonePk);

			// DST START IN 2005 - 3rd Sunday of October @12am(0:00)
			// Local time => 16/10/2005 @12am

			// Before DST Start
			DateTime localDateTime = new DateTime(2005, 10, 15, 23, 30, 0);
			TimeSpan tentativeUtcOffset = testZone.GetUtcOffsetBasedOnUtc(localDateTime);
			DateTime tentativeUtc = localDateTime.Subtract(tentativeUtcOffset);
			TimeSpan checkUtcOffset = testZone.GetUtcOffsetBasedOnUtc(tentativeUtc);
			DateTime actualUtc = localDateTime.Subtract(checkUtcOffset);
			AssertEquals("Local Time = 15/10/2005 @23:30 => Tentative UTC:", new DateTime(2005, 10, 16, 2, 30, 0), tentativeUtc);
			AssertEquals("Local Time = 15/10/2005 @23:30 => Actual UTC:", new DateTime(2005, 10, 16, 2, 30, 0), actualUtc);

			// After DST Start
			localDateTime = new DateTime(2005, 10, 16, 1, 0, 0);
			tentativeUtcOffset = testZone.GetUtcOffsetBasedOnUtc(localDateTime);
			tentativeUtc = localDateTime.Subtract(tentativeUtcOffset);
			checkUtcOffset = testZone.GetUtcOffsetBasedOnUtc(tentativeUtc);
			actualUtc = localDateTime.Subtract(checkUtcOffset);
			AssertEquals("Local Time = 16/10/2005 @1am => Tentative UTC:", new DateTime(2005, 10, 16, 4, 0, 0), tentativeUtc);
			AssertEquals("Local Time = 16/10/2005 @1am => Actual UTC:", new DateTime(2005, 10, 16, 3, 0, 0), actualUtc);

			// DST END IN 2005 - 3rd Sunday of February @12am(0:00)
			// Local time => 20/02/2005 @12am

			// Before DST End
			localDateTime = new DateTime(2005, 2, 19, 22, 30, 0);
			tentativeUtcOffset = testZone.GetUtcOffsetBasedOnUtc(localDateTime);
			tentativeUtc = localDateTime.Subtract(tentativeUtcOffset);
			checkUtcOffset = testZone.GetUtcOffsetBasedOnUtc(tentativeUtc);
			actualUtc = localDateTime.Subtract(checkUtcOffset);
			AssertEquals("Local Time = 19/02/2005 @22:30 => Tentative UTC:", new DateTime(2005, 2, 20, 0, 30, 0), tentativeUtc);
			AssertEquals("Local Time = 19/02/2005 @22:30 => Actual UTC:", new DateTime(2005, 2, 20, 0, 30, 0), actualUtc);

			// Before DST End
			localDateTime = new DateTime(2005, 2, 19, 23, 30, 0);
			tentativeUtcOffset = testZone.GetUtcOffsetBasedOnUtc(localDateTime);
			tentativeUtc = localDateTime.Subtract(tentativeUtcOffset);
			checkUtcOffset = testZone.GetUtcOffsetBasedOnUtc(tentativeUtc);
			actualUtc = localDateTime.Subtract(checkUtcOffset);
			AssertEquals("Local Time = 19/02/2005 @23:30 => Tentative UTC:", new DateTime(2005, 2, 20, 1, 30, 0), tentativeUtc);
			AssertEquals("Local Time = 19/02/2005 @23:30 => Actual UTC:", new DateTime(2005, 2, 20, 1, 30, 0), actualUtc);

			// After DST End
			localDateTime = new DateTime(2005, 2, 20, 0, 0, 0);
			tentativeUtcOffset = testZone.GetUtcOffsetBasedOnUtc(localDateTime);
			tentativeUtc = localDateTime.Subtract(tentativeUtcOffset);
			checkUtcOffset = testZone.GetUtcOffsetBasedOnUtc(tentativeUtc);
			actualUtc = localDateTime.Subtract(checkUtcOffset);
			AssertEquals("Local Time = 20/02/2005 @0:00 => Tentative UTC:", new DateTime(2005, 2, 20, 2, 0, 0), tentativeUtc);
			AssertEquals("Local Time = 20/02/2005 @0:00 => Actual UTC:", new DateTime(2005, 2, 20, 3, 0, 0), actualUtc);
		}

		public void TestToUtcFromCurrentBranchLocalTime_NoDstTimeZone()
		{
			// Bombay - INBOM
			TimeZoneInfoForTesting testZone = new TimeZoneInfoForTesting("Asia/Calcutta", 5.5m, 5.5m, Guid.Empty);

			DateTime localDateTime = DateTime.Now; // Getting a ramdon time from client PC for tests
			TimeSpan tentativeUtcOffset = testZone.GetUtcOffsetBasedOnUtc(localDateTime);
			DateTime tentativeUtc = localDateTime.Subtract(tentativeUtcOffset);
			TimeSpan checkUtcOffset = testZone.GetUtcOffsetBasedOnUtc(tentativeUtc);
			DateTime actualUtc = localDateTime.Subtract(checkUtcOffset);
			AssertEquals("LocalTime-TentativeUTC", new TimeSpan(5, 30, 0), localDateTime.Subtract(tentativeUtc));
			AssertEquals("LocalTime-ActualUTC", new TimeSpan(5, 30, 0), localDateTime.Subtract(actualUtc));
		}

		public void TestThrowsExceptionIfDstZoneDoesNotExist()
		{
			string timeZoneSetName = "DoesNotMatter";

			try
			{
				TimeZoneInfoForTesting testZone = new TimeZoneInfoForTesting(timeZoneSetName, 0m, 0m, Guid.NewGuid());
				Fail("Should have thrown exception");
			}
			catch (TimeZoneException ex)
			{
				string expectedError = string.Format("Unable to obtain date time information: Inexisting Daylight Saving zone for [{0}] time zone.", timeZoneSetName);
				AssertEquals("Caught Exception: ", expectedError, ex.Message);
			}
		}

		public void TestIsDaylightSavingWithDataReaderOpen_ShouldTryOnNewConnection()
		{
			var info = new TimeZoneInfo("Australia/Sydney", 10m, 11m, GetDstZonePkFromPort("AUSYD"));

			using (var command = Db.Connection.Command("SELECT COUNT(*) FROM dbo.OrgHeader"))
			using (var reader = command.ExecuteReader())
			{
				AssertNoExceptionThrown(() => info.IsDaylightSavingBasedOnUtc(DateTime.UtcNow));
			}
		}

		Guid GetDstZonePkFromPort(string portCode)
		{
			string sqlText = @"
				SELECT R3_R2_DaylightSavingZone
				FROM dbo.RefUNLOCO INNER JOIN dbo.RefTimeZoneSet ON R3_PK = RL_R3
				WHERE RL_Code = @code";

			return (Guid)Db.Connection.ExecuteScalar(sqlText, cmd => cmd.AddParameterBasedOnDbColumn("@code", portCode, RefUNLOCOSchema.RL_Code));
		}
	}
}
