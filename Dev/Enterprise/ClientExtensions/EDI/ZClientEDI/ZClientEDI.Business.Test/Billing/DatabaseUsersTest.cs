using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class DatabaseUsersTest : TestCaseWithFactory
	{
		public void TestDatabaseMonthlyUserCount()
		{
			var lic1a = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic1b = BillingTestHelper.CreateAnotherDatabase(lic1a, "CCC");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1a, "BBB");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "DDD");

			BillingTestHelper.CreateEdiLicenceUsage(lic1a, new ZDateTime(2013, 8, 30), "August Only");
			BillingTestHelper.CreateEdiLicenceUsage(lic1a, new ZDateTime(2013, 10, 1), "October Only");
			BillingTestHelper.CreateEdiLicenceUsage(lic1a, new ZDateTime(2013, 9, 1), "Jim");

			BillingTestHelper.CreateEdiLicenceUsage(lic2, new ZDateTime(2013, 8, 30), "Jim");
			BillingTestHelper.CreateEdiLicenceUsage(lic2, new ZDateTime(2013, 10, 1), "Jim");

			BillingTestHelper.CreateEdiLicenceUsage(lic1a, new ZDateTime(2013, 9, 1), "Albert");
			BillingTestHelper.CreateEdiLicenceUsage(lic2, new ZDateTime(2013, 9, 1), "Betty");

			BillingTestHelper.CreateEdiLicenceUsage(lic1b, new ZDateTime(2013, 9, 1), "Db Two");

			BillingTestHelper.CreateEdiLicenceUsage(lic3, new ZDateTime(2013, 9, 1), "Jim");
			BillingTestHelper.CreateEdiLicenceUsage(lic3, new ZDateTime(2013, 9, 1), "Another One");
			BillingTestHelper.CreateEdiLicenceUsage(lic3, new ZDateTime(2013, 8, 1), "Another Two");
			BillingTestHelper.CreateEdiLicenceUsage(lic3, new ZDateTime(2013, 10, 1), "Another Three");

			Factory.Save();

			var dbUsers = new DatabaseUsers(forSingleOrg: false);
			AssertEquals("Guid.Empty", 0, dbUsers.DatabaseMonthlyUserCount(Guid.Empty, new DateTime(2013, 9, 1)));
			AssertEquals("Guid.New", 0, dbUsers.DatabaseMonthlyUserCount(Guid.NewGuid(), new DateTime(2013, 9, 1)));

			AssertEquals("Jim, Albert, Betty", 3, dbUsers.DatabaseMonthlyUserCount(lic1a.LA_LD, new DateTime(2013, 9, 1)));
			AssertEquals("Jim, Another One", 2, dbUsers.DatabaseMonthlyUserCount(lic3.LA_LD, new DateTime(2013, 9, 1)));
			AssertEquals("Db Two", 1, dbUsers.DatabaseMonthlyUserCount(lic1b.LA_LD, new DateTime(2013, 9, 1)));

			AssertEquals("August Only, Jim", 2, dbUsers.DatabaseMonthlyUserCount(lic1a.LA_LD, new DateTime(2013, 8, 1)));
			AssertEquals("October Only, Jim", 2, dbUsers.DatabaseMonthlyUserCount(lic1a.LA_LD, new DateTime(2013, 10, 1)));

			int callsToLoadAll = dbUsers.CallsToLoadAll;
			// repeat some calls to verify caching
			AssertEquals("August Only, Jim", 2, dbUsers.DatabaseMonthlyUserCount(lic1a.LA_LD, new DateTime(2013, 8, 1)));
			AssertEquals("August Only, Jim", 2, dbUsers.DatabaseMonthlyUserCount(lic1a.LA_LD, new DateTime(2013, 8, 1)));
			AssertEquals("October Only, Jim", 2, dbUsers.DatabaseMonthlyUserCount(lic1a.LA_LD, new DateTime(2013, 10, 1)));
			AssertEquals("October Only, Jim", 2, dbUsers.DatabaseMonthlyUserCount(lic1a.LA_LD, new DateTime(2013, 10, 1)));
			AssertEquals("results are cached", callsToLoadAll, dbUsers.CallsToLoadAll);

			dbUsers = new DatabaseUsers(forSingleOrg: true);
			AssertEquals("August Only, Jim", 2, dbUsers.DatabaseMonthlyUserCount(lic1a.LA_LD, new DateTime(2013, 8, 1)));
			AssertEquals("October Only, Jim", 2, dbUsers.DatabaseMonthlyUserCount(lic1a.LA_LD, new DateTime(2013, 10, 1)));
			AssertEquals("August Only, Jim", 2, dbUsers.DatabaseMonthlyUserCount(lic1a.LA_LD, new DateTime(2013, 8, 1)));
			AssertEquals("October Only, Jim", 2, dbUsers.DatabaseMonthlyUserCount(lic1a.LA_LD, new DateTime(2013, 10, 1)));
		}

		public void TestDatabaseMonthlyUserList()
		{
			var lic1a = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic1b = BillingTestHelper.CreateAnotherDatabase(lic1a, "CCC");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1a, "BBB");
			var lic3 = BillingTestHelper.CreateLicence(Factory, "DDD");

			var staffJim = BillingTestHelper.FindOrCreateDatabaseStaffByName(lic1a.Database, "Jim");
			staffJim.LS_Code = "JJ";
			Factory.Save();

			BillingTestHelper.CreateEdiLicenceUsage(lic1a, new ZDateTime(2013, 8, 30), "August Only");
			BillingTestHelper.CreateEdiLicenceUsage(lic1a, new ZDateTime(2013, 10, 1), "October Only");
			BillingTestHelper.CreateEdiLicenceUsage(lic1a, new ZDateTime(2013, 9, 1), "Jim");

			BillingTestHelper.CreateEdiLicenceUsage(lic2, new ZDateTime(2013, 8, 30), "Jim");
			BillingTestHelper.CreateEdiLicenceUsage(lic2, new ZDateTime(2013, 10, 1), "Jim");

			BillingTestHelper.CreateEdiLicenceUsage(lic1a, new ZDateTime(2013, 9, 1), "Albert");
			BillingTestHelper.CreateEdiLicenceUsage(lic2, new ZDateTime(2013, 9, 1), "Betty");

			BillingTestHelper.CreateEdiLicenceUsage(lic1b, new ZDateTime(2013, 9, 1), "Db Two");

			BillingTestHelper.CreateEdiLicenceUsage(lic3, new ZDateTime(2013, 9, 1), "Jim");
			BillingTestHelper.CreateEdiLicenceUsage(lic3, new ZDateTime(2013, 9, 1), "Another One");
			BillingTestHelper.CreateEdiLicenceUsage(lic3, new ZDateTime(2013, 8, 1), "Another Two");
			BillingTestHelper.CreateEdiLicenceUsage(lic3, new ZDateTime(2013, 10, 1), "Another Three");

			Factory.Save();

			var dbUsers = new DatabaseUsers(true);
			AssertArrayEqualsByElements("Guid.Empty", Array.Empty<string>(), dbUsers.DatabaseMonthlyUserList(Guid.Empty, new DateTime(2013, 9, 1)).ToArray());
			AssertArrayEqualsByElements("Guid.New", Array.Empty<string>(), dbUsers.DatabaseMonthlyUserList(Guid.NewGuid(), new DateTime(2013, 9, 1)).ToArray());

			AssertArrayEqualsByElements(new string[] { "Albert", "Betty", "Jim (JJ)" }, dbUsers.DatabaseMonthlyUserList(lic1a.LA_LD, new DateTime(2013, 9, 1)).ToArray());
			AssertArrayEqualsByElements(new string[] { "Another One", "Jim" }, dbUsers.DatabaseMonthlyUserList(lic3.LA_LD, new DateTime(2013, 9, 1)).ToArray());
			AssertArrayEqualsByElements(new string[] { "Db Two" }, dbUsers.DatabaseMonthlyUserList(lic1b.LA_LD, new DateTime(2013, 9, 1)).ToArray());

			AssertArrayEqualsByElements(new string[] { "August Only", "Jim (JJ)" }, dbUsers.DatabaseMonthlyUserList(lic1a.LA_LD, new DateTime(2013, 8, 1)).ToArray());
			AssertArrayEqualsByElements(new string[] { "Jim (JJ)", "October Only" }, dbUsers.DatabaseMonthlyUserList(lic1a.LA_LD, new DateTime(2013, 10, 1)).ToArray());

			// repeat some calls
			AssertArrayEqualsByElements(new string[] { "August Only", "Jim (JJ)" }, dbUsers.DatabaseMonthlyUserList(lic1a.LA_LD, new DateTime(2013, 8, 1)).ToArray());
			AssertArrayEqualsByElements(new string[] { "Jim (JJ)", "October Only" }, dbUsers.DatabaseMonthlyUserList(lic1a.LA_LD, new DateTime(2013, 10, 1)).ToArray());
			AssertArrayEqualsByElements(new string[] { "August Only", "Jim (JJ)" }, dbUsers.DatabaseMonthlyUserList(lic1a.LA_LD, new DateTime(2013, 8, 1)).ToArray());
			AssertArrayEqualsByElements(new string[] { "Jim (JJ)", "October Only" }, dbUsers.DatabaseMonthlyUserList(lic1a.LA_LD, new DateTime(2013, 10, 1)).ToArray());
		}
	}
}