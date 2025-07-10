using System;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.DbHealth.Check
{
	[TestedType(typeof(RestoreChecker))]
	sealed class RestoreCheckerTest : CheckerTestCaseBase
	{
		[UseSnapshotProtection]
		[TestDate(2022, 3, 14, 14, 00, 00)]
		public void TestWarningListDetail()
		{
			SetUpEnvironment(ZDateTime.UtcNow.ToDateTime(), ZDateTime.UtcNow.AddHours(1).ToDateTime(), false);

			var warningList = GetRestoreCheckerWarningList();
			AssertEquals(2, warningList.Count);

			AssertEquals($"Database [{Db.DatabaseName}] has been restored on 03/14/2022 14:00:00.", warningList[0].Description);
			AssertEquals($"{BrandingFactory.Instance.ProductName} will disable compliance Books in all Portugal Login Companies", warningList[0].Action);

			AssertEquals("Due to a Database Restore, all compliance sequences in all Portugal Login Companies have been disabled on 03/14/2022 15:00:00. This is to cope with Portugal local compliance laws. Please create new compliance books for Portugal companies if necessary.", warningList[1].Description);
			AssertEquals($"{BrandingFactory.Instance.ProductName} will disable compliance Books in all Portugal Login Companies", warningList[1].Action);
		}

		[UseSnapshotProtection]
		[TestDate(2022, 3, 14, 14, 00, 00)]
		public void TestRestoreChecker_IsHostedWithCargowise()
		{
			SetUpEnvironment(ZDateTime.UtcNow.ToDateTime(), ZDateTime.UtcNow.ToDateTime(), true);
			AssertEquals(0, GetRestoreCheckerWarningList().Count);

			SetUpEnvironment(ZDateTime.UtcNow.ToDateTime(), ZDateTime.UtcNow.ToDateTime(), false);
			AssertEquals(2, GetRestoreCheckerWarningList().Count);
		}

		[UseSnapshotProtection]
		[TestDate(2022, 3, 14, 14, 00, 00)]
		public void TestRestoreChecker_WhenRegistryIsDateTimeMinValue()
		{
			SetUpEnvironment(DateTime.MinValue, DateTime.MinValue, false);
			AssertEquals(0, GetRestoreCheckerWarningList().Count);

			SetUpEnvironment(ZDateTime.UtcNow.ToDateTime(), ZDateTime.UtcNow.ToDateTime(), false);
			AssertEquals(2, GetRestoreCheckerWarningList().Count);
		}

		[UseSnapshotProtection]
		[TestDate(2022, 3, 14, 14, 00, 00)]
		public void TestRestoreChecker_TheDateTimeShouldBeWithinOneMonth()
		{
			var testDate1 = ZDateTime.UtcNow.ToDateTime().AddMonths(-1);
			SetUpEnvironment(testDate1, testDate1);
			AssertEquals(0, GetRestoreCheckerWarningList().Count);

			var testDate2 = testDate1.AddSeconds(1);
			SetUpEnvironment(testDate2, testDate2);
			AssertEquals(2, GetRestoreCheckerWarningList().Count);
		}

		[UseSnapshotProtection]
		[TestDate(2022, 3, 14, 14, 00, 00)]
		public void TestRestoreChecker_WhenActivePortugalCompanyDoNotExist()
		{
			SetUpEnvironment(ZDateTime.UtcNow.ToDateTime(), ZDateTime.UtcNow.ToDateTime(), false, CountryCodes.China);
			AssertEquals(0, GetRestoreCheckerWarningList().Count);

			SetUpEnvironment(ZDateTime.UtcNow.ToDateTime(), ZDateTime.UtcNow.ToDateTime(), false, CountryCodes.Portugal);

			Db.Connection.ExecuteNonQuery($"Update dbo.GlbCompany SET GC_IsActive = 0, GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GETUTCDATE()");
			AssertEquals(0, GetRestoreCheckerWarningList().Count);

			Db.Connection.ExecuteNonQuery($"Update dbo.GlbCompany SET GC_IsActive = 1, GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GETUTCDATE()");
			AssertEquals(2, GetRestoreCheckerWarningList().Count);
		}

		DbHealthWarningList GetRestoreCheckerWarningList()
		{
			var checker = GetNewCheckerInstance();
			var warningList = new DbHealthWarningList();
			checker.Check(Db.Connection, warningList, new DummyLogger());
			return warningList;
		}

		void SetUpEnvironment(
			DateTime lastDatabaseRestoreDate,
			DateTime lastUTCDateToDisableComplianceBookAfterDbRestored,
			bool isHostedWithCargowise = false,
			string countryCode = CountryCodes.Portugal)
		{
			EnvProxy.SetHostedLocationForTest(isHostedWithCargowise ? "SYD" : "");
			AssertEquals("Precondition", isHostedWithCargowise, EnvProxy.IsHostedWithCargowise);

			Db.Connection.ExecuteNonQuery($"Update dbo.GlbCompany SET GC_RN_NKCountryCode = '{countryCode}', GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GETUTCDATE()");

			SystemDataRegistryForTest.Get().LastDatabaseRestoreDate = lastDatabaseRestoreDate;
			ObjectFactory.Get<IAccountingRegistryProvider>().LastUTCDateToDisableComplianceBookAfterDbRestored = lastUTCDateToDisableComplianceBookAfterDbRestored;
		}

		protected override IChecker GetNewCheckerInstance()
		{
			return new RestoreChecker();
		}
	}
}
