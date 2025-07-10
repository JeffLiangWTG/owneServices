namespace Enterprise.Client.EDI.UserManagement.Business.Testing
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Client.EDI.Billing.Business.Test;
	using Enterprise.Client.EDI.Licencing.Business;
	using Enterprise.MasterFiles.Business.UserAccountReport;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	class CustomerUserAccountSyncHelperTest : TestCaseWithFactory
	{
		public void TestLinkUserAccountsToClientStaff()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			database.LD_DatabaseNumber = 3001;
			Factory.Save();

			var reports = new List<StaffReport>();
			for (var index = 0; index <= 250; index++)
			{
				reports.Add(new StaffReport()
				{
					Code = index.ToString().PadLeft(3, '0'),
					Name = $"User{index}",
					EmailAddress = $"User{index}@.cw1.com",
					IsActive = true,
				});
			}

			var user55 = Factory.New<EdiCustomerUserAccount>();
			user55.EUA_LD = database.PK;
			user55.EUA_UserID = reports[55].Code;

			var clientStaff55 = Factory.New<ClientStaff>();
			clientStaff55.LS_LD = database.PK;
			clientStaff55.LS_Code = reports[55].Code;
			clientStaff55.LS_Email = reports[55].EmailAddress;
			clientStaff55.LS_FullName = reports[55].Name;
			clientStaff55.LS_IsActive = reports[55].IsActive;

			Factory.Save();

			CustomerUserAccountSyncHelper.LinkUserAccountsToClientStaff(database.PK, reports);

			var query = new ZQuery(EdiCustomerUserAccountSchema.EUA_UserID, reports[55].Code);
			var userAccount = new BusinessObjectFactory().LoadTop1<EdiCustomerUserAccount>(query);
			AssertEquals(userAccount.EUA_LS, clientStaff55.PK);
		}

		[TestDate(2020, 01, 05)]
		public void TestImportDeactivationOfUserAccounts()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			database.LD_DatabaseNumber = 3001;
			var org = database.LicEnterprise.Organisation;
			org.Contacts.RemoveAndDeleteAll();

			Factory.Save();

			const string existingUser1Code = "AE1";
			const string existingUser2Code = "IE1";
			const string inactiveExistingUser1Code = "IE2";
			const string inactiveExistingUser2Code = "IE3";

			var reports = new List<StaffReport>();
			reports.Add(new StaffReport
			{
				Code = "AU1",
				Name = "ActiveUser",
				EmailAddress = "activeuser@.cw1.com",
				IsActive = true,
			});
			reports.Add(new StaffReport
			{
				Code = "IU1",
				Name = "InactiveUser",
				EmailAddress = "inactiveuser@.cw1.com",
				IsActive = false,
			});
			reports.Add(new StaffReport
			{
				Code = existingUser1Code,
				Name = "ActiveExistingUser",
				EmailAddress = "activeexistinguser@.cw1.com",
				IsActive = true,
			});
			reports.Add(new StaffReport
			{
				Code = existingUser2Code,
				Name = "InactiveExistingUser",
				EmailAddress = "inactiveexistinguser@.cw1.com",
				IsActive = false,
			});
			reports.Add(new StaffReport
			{
				Code = inactiveExistingUser1Code,
				Name = "ReactivateExistingUser",
				EmailAddress = "inactiveexistinguser2@.cw1.com",
				IsActive = true,
			});
			reports.Add(new StaffReport
			{
				Code = inactiveExistingUser2Code,
				Name = "DeactivateExistingDeactivatedUser",
				EmailAddress = "inactiveexistinguser3@.cw1.com",
				IsActive = false,
			});

			var existingUserAccount1 = Factory.New<EdiCustomerUserAccount>();
			existingUserAccount1.EUA_LD = database.PK;
			existingUserAccount1.EUA_UserID = existingUser1Code;
			existingUserAccount1.EUA_Email = "aaa@aaa.com";
			existingUserAccount1.EUA_FullName = "aaa";
			existingUserAccount1.EUA_IsActive = true;

			var existingUserAccount2 = Factory.New<EdiCustomerUserAccount>();
			existingUserAccount2.EUA_LD = database.PK;
			existingUserAccount2.EUA_UserID = existingUser2Code;
			existingUserAccount2.EUA_Email = "bbb@aaa.com";
			existingUserAccount2.EUA_FullName = "bbb";
			existingUserAccount2.EUA_IsActive = true;

			var inactiveExistingUserAccount1 = Factory.New<EdiCustomerUserAccount>();
			inactiveExistingUserAccount1.EUA_LD = database.PK;
			inactiveExistingUserAccount1.EUA_UserID = inactiveExistingUser1Code;
			inactiveExistingUserAccount1.EUA_Email = "vvv@aaa.com";
			inactiveExistingUserAccount1.EUA_FullName = "ccc";
			inactiveExistingUserAccount1.EUA_IsActive = false;

			var inactiveExistingUserAccount2 = Factory.New<EdiCustomerUserAccount>();
			inactiveExistingUserAccount2.EUA_LD = database.PK;
			inactiveExistingUserAccount2.EUA_UserID = inactiveExistingUser2Code;
			inactiveExistingUserAccount2.EUA_Email = "vva@aaa.com";
			inactiveExistingUserAccount2.EUA_FullName = "cca";
			inactiveExistingUserAccount2.EUA_IsActive = false;

			Factory.Save();

			CustomerUserAccountSyncHelper.ImportDeactivationOfUserAccounts(database.PK, reports);

			var query = new ZQuery();
			query.OrderBy = "EUA_UserID";
			var userAccounts = new BusinessObjectFactory().Load<EdiCustomerUserAccount>(query);

			AssertEquals("New users should not be imported", 4, userAccounts.Length);

			AssertEquals(existingUserAccount1.PK, userAccounts[0].PK);
			AssertEquals("Should remain unchanged", "aaa@aaa.com", userAccounts[0].EUA_Email);
			AssertEquals("Should remain unchanged", "aaa", userAccounts[0].EUA_FullName);
			AssertEquals("Should remain unchanged", true, userAccounts[0].EUA_IsActive);
			AssertEquals("Should remain unchanged", ZDateTime.Empty, userAccounts[0].EUA_SystemVerifiedDateUtc);

			AssertEquals(existingUserAccount2.PK, userAccounts[1].PK);
			AssertEquals("Should remain unchanged", "bbb@aaa.com", userAccounts[1].EUA_Email);
			AssertEquals("Should remain unchanged", "bbb", userAccounts[1].EUA_FullName);
			AssertEquals("Should be updated", false, userAccounts[1].EUA_IsActive);
			AssertEquals("Should be updated", new ZDateTime(2020, 01, 05), userAccounts[1].EUA_SystemVerifiedDateUtc);

			AssertEquals(inactiveExistingUserAccount1.PK, userAccounts[2].PK);
			AssertEquals("Should remain unchanged", "vvv@aaa.com", userAccounts[2].EUA_Email);
			AssertEquals("Should remain unchanged", "ccc", userAccounts[2].EUA_FullName);
			AssertEquals("Should remain unchanged", false, userAccounts[2].EUA_IsActive);
			AssertEquals("Should remain unchanged", ZDateTime.Empty, userAccounts[2].EUA_SystemVerifiedDateUtc);

			AssertEquals(inactiveExistingUserAccount2.PK, userAccounts[3].PK);
			AssertEquals("Should remain unchanged", "vva@aaa.com", userAccounts[3].EUA_Email);
			AssertEquals("Should remain unchanged", "cca", userAccounts[3].EUA_FullName);
			AssertEquals("Should remain unchanged", false, userAccounts[3].EUA_IsActive);
			AssertEquals("Should remain unchanged", ZDateTime.Empty, userAccounts[3].EUA_SystemVerifiedDateUtc);
		}
	}
}
