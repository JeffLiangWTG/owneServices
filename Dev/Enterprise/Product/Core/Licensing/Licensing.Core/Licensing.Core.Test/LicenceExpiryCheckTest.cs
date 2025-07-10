using System;
using CargoWise.Application;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Licensing.Testing
{
	sealed class LicenceExpiryCheckTest : TransactionedTestCase
	{
		[TestDate(2005, 12, 06, 9, 22, 0)]
		public void TestDayAfterExpired()
		{
			Assert(LicenceExpiryCheck.Create().SystemHasExpired);
		}

		[TestDate(2005, 12, 05, 16, 01, 0)]
		public void TestDayOfExpiryAfterSixteenHours()
		{
			Assert(LicenceExpiryCheck.Create().SystemHasExpired);
		}

		[TestDate(2005, 12, 05, 15, 30, 0)]
		public void TestDayOfExpiryBeforeSixteenHours()
		{
			Assert(!LicenceExpiryCheck.Create().SystemHasExpired);
		}

		[TestDate(2005, 12, 04, 17, 28, 0)]
		public void TestDayBeforeExpiryAfterSixteenHours()
		{
			var expiryCheck = LicenceExpiryCheck.Create();
			Assert(!expiryCheck.SystemHasExpired);
			Assert(expiryCheck.DaysToExpiry < 1);
		}

		[TestDate(2005, 12, 04, 8, 45, 0)]
		public void TestDayBeforeExpiryInTheMorning()
		{
			var expiryCheck = LicenceExpiryCheck.Create();
			Assert(!expiryCheck.SystemHasExpired);
			AssertEquals(1, (int)expiryCheck.DaysToExpiry);
		}

		[TestDate(2005, 12, 03, 12, 15, 0)]
		public void TestTwoDaysBeforeExpiry()
		{
			var expiryCheck = LicenceExpiryCheck.Create();
			Assert(!expiryCheck.SystemHasExpired);
			AssertEquals(2, (int)expiryCheck.DaysToExpiry);
		}

		[TestDate(2005, 11, 28, 14, 44, 0)]
		public void TestSevenBeforeExpiry()
		{
			var expiryCheck = LicenceExpiryCheck.Create();
			Assert(!expiryCheck.SystemHasExpired);
			AssertEquals(7, (int)expiryCheck.DaysToExpiry);
		}

		[TestDate(2005, 11, 26, 18, 18, 0)]
		public void TestEightDaysBeforeExpiry()
		{
			var expiryCheck = LicenceExpiryCheck.Create();
			Assert(!expiryCheck.SystemHasExpired);
			AssertEquals(8, (int)expiryCheck.DaysToExpiry);
		}

		[TestDate(2005, 12, 5, 15, 0, 0)]
		[TestUtcOffset(0, 0, 0)]
		public void TestSystemHasExpired()
		{
			var expiryCheck = LicenceExpiryCheck.Create();
			// Raw expiry date is 5/12/2005 (a Monday)
			AssertEquals("pre", new DateTime(2005, 12, 5, 16, 0, 0), expiryCheck.ExpiryDateTime);
			Assert(!expiryCheck.SystemHasExpired);

			TestUtcOffsetAttribute.Time = new TimeSpan(1, 0, 0);
			expiryCheck = LicenceExpiryCheck.Create();
			Assert("expiry is in local time", expiryCheck.SystemHasExpired);
		}

		[TestDate(2005, 12, 05, 16, 0, 0)] // Expiry data and time
		public void TestNonProductionSystem()
		{
			DateTime expiryDateTime = new DateTime(2005, 12, 05, 16, 0, 0);

			LicenceCheckpointTest.SetSystemExpiryDate(new DateTime(2005, 12, 3, 0, 0, 0));
			LicenceCheckpointTest.SetDatabaseType(DatabaseTypes.Codes.Training);

			// Training system
			var expiryCheck = LicenceExpiryCheck.Create();
			Assert(!expiryCheck.SystemHasExpired);
			AssertEquals(double.MaxValue, expiryCheck.DaysToExpiry);

			// Demo system
			LicenceCheckpointTest.SetDatabaseType(DatabaseTypes.Codes.Demo);
			expiryCheck = LicenceExpiryCheck.Create();
			Assert(!expiryCheck.SystemHasExpired);
			AssertEquals(double.MaxValue, expiryCheck.DaysToExpiry);

			// Education system
			LicenceCheckpointTest.SetDatabaseType(DatabaseTypes.Codes.Education);
			expiryCheck = LicenceExpiryCheck.Create();
			Assert(!expiryCheck.SystemHasExpired);
			AssertEquals(double.MaxValue, expiryCheck.DaysToExpiry);
		}

		[TestDate(2005, 12, 05, 16, 0, 0)] // Expiry data and time
		public void TestStabilityErrorMessage_Custom()
		{
			DateTime expiryDateTime = new DateTime(2005, 12, 05, 16, 0, 0);
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.SystemExpiryDateForTest = new DateTime(2005, 12, 3, 0, 0, 0);
			registrationKey.ExpiredMessageForTest = "MSG1";
			registrationKey.ExpiryWeekMessageForTest = "MSG2";
			registrationKey.ExpiryMonthMessageForTest = "MSG3";

			var expiryCheck = LicenceExpiryCheck.Create();
			AssertEquals("MSG1", expiryCheck.StabilityErrorMessage);

			TestDateAttribute.Date = expiryDateTime.AddMinutes(-1);
			expiryCheck = LicenceExpiryCheck.Create();
			AssertEquals("MSG2", expiryCheck.StabilityErrorMessage);

			TestDateAttribute.Date = expiryDateTime.AddDays(-7);
			expiryCheck = LicenceExpiryCheck.Create();
			AssertEquals("MSG2", expiryCheck.StabilityErrorMessage);

			TestDateAttribute.Date = expiryDateTime.AddDays(-7).AddMinutes(-1);
			expiryCheck = LicenceExpiryCheck.Create();
			AssertEquals("MSG3", expiryCheck.StabilityErrorMessage);
		}
	}
}
