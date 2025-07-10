using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.UserAccountReport;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Business.Test
{
	[TestedType(typeof(ClientBranch))]
	class ClientBranchTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSync()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var clientCompany = licence.ClientCompany;
			clientCompany.LCC_Code = "AAA";

			var org = database.LicEnterprise.Organisation;

			var clientBranch1 = Factory.New<ClientBranch>();
			clientBranch1.LCB_LD = database.PK;
			clientBranch1.LCB_Code = "BN1";
			clientBranch1.LCB_Name = "Branch Sydney";
			clientBranch1.LCB_ClientPK = ZGuid.NewZGuid();

			var address1 = org.Addresses.AddNew();
			address1.FillWithValidTestData();

			Factory.Save();

			var branchReport1 = new BranchReport()
			{
				PK = clientBranch1.LCB_ClientPK.ToGuid(),
				Code = "BN1",
				BranchName = "Branch Sydney",
				CompanyName = "Company AAA",
				CompanyCode = "AAA",
				IsActive = true,
				Address1 = "1 Branch Rd",
				Address2 = "Building A",
				City = "Sydney",
				State = "NSW",
				PostCode = "2000",
				Unloco = "AUSYD",
				CountryCode = "AU",
				ValidationStatus = AddressValidationStatus.Invalid
			};

			var branchReport2 = new BranchReport()
			{
				PK = Guid.NewGuid(),
				Code = "BN2",
				BranchName = "Branch Melbourne",
				CompanyName = "Company AAA",
				IsActive = true,
				Address1 = "2 Branch Rd",
				Address2 = "Building B",
				City = "Melbourne",
				State = "VIC",
				PostCode = "3000",
				Unloco = "AUMEL",
				CountryCode = "AU",
				ValidationStatus = AddressValidationStatus.Verified
			};

			var branchReport3 = new BranchReport()
			{
				PK = Guid.NewGuid(),
				Code = "BN3",
				BranchName = "Branch Brisbane",
				CompanyName = "Company AAA",
				IsActive = true,
				Address1 = "3 Branch Rd",
				Address2 = "Building C",
				City = "Brisbane",
				State = "QLD",
				PostCode = "4000",
				Unloco = "AUBNE",
				CountryCode = "AU",
				ValidationStatus = AddressValidationStatus.ToBeVerified
			};

			var branchReports = new List<BranchReport>() { branchReport1, branchReport2, branchReport3 };

			ClientBranch.Sync(Factory, database, branchReports);
			Factory.Save();
			AssertClientBranch(branchReport1);
			AssertClientBranch(branchReport2);
			AssertClientBranch(branchReport3);

			branchReport1.Address1 = "1 New Branch St";
			branchReport2.PostCode = "3001";
			branchReport3.ValidationStatus = AddressValidationStatus.Verified;

			var importFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			ClientBranch.Sync(Factory, database, branchReports);
			importFactory.Save();
			AssertClientBranch(branchReport1);
			AssertClientBranch(branchReport2);
			AssertClientBranch(branchReport3);
		}

		public void TestSync_SameExistingAddress()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var clientCompany = licence.ClientCompany;
			clientCompany.LCC_Code = "AAA";

			var org = database.LicEnterprise.Organisation;
			var address1 = org.Addresses.AddNew();

			address1.OA_Address1 = "1 Branch Rd";
			address1.OA_Address2 = "Building A";
			address1.OA_City = "Sydney";
			address1.OA_State = "NSW";
			address1.OA_PostCode = "2000";
			address1.OA_RL_NKRelatedPortCode = "AUSYD";
			address1.OA_RN_NKCountryCode = "AU";
			address1.OA_ValidationStatus = AddressValidationStatus.Invalid;

			Factory.Save();

			var branchReport1 = new BranchReport()
			{
				PK = Guid.NewGuid(),
				Code = "BN1",
				BranchName = "Branch Sydney",
				CompanyName = "Company AAA",
				IsActive = true,
				Address1 = "1 Branch Rd",
				Address2 = "Building A",
				City = "Sydney",
				State = "NSW",
				PostCode = "2000",
				Unloco = "AUSYD",
				CountryCode = "AU",
				ValidationStatus = AddressValidationStatus.Verified
			};

			var branchReports = new List<BranchReport>() { branchReport1 };

			ClientBranch.Sync(Factory, database, branchReports);
			Factory.Save();
			AssertClientBranch(branchReport1);

			var clientBranch1 = Factory.LoadTop1<ClientBranch>(new ZQuery(ClientBranchSchema.LCB_ClientPK, branchReport1.PK));
			AssertEquals("Linked to existing address", address1.PK, clientBranch1.LCB_OA);

			var branchReport2 = new BranchReport()
			{
				PK = Guid.NewGuid(),
				Code = "BN2",
				BranchName = "Branch Melbourne",
				CompanyName = "Company AAA",
				IsActive = true,
				Address1 = "1 Branch Rd",
				Address2 = "Building A",
				City = "Sydney",
				State = "NSW",
				PostCode = "2000",
				Unloco = "AUSYD",
				CountryCode = "AU",
				ValidationStatus = AddressValidationStatus.Verified
			};

			branchReports = new List<BranchReport>() { branchReport1, branchReport2 };
			ClientBranch.Sync(Factory, database, branchReports);
			Factory.Save();

			var clientBranch2 = Factory.LoadTop1<ClientBranch>(new ZQuery(ClientBranchSchema.LCB_ClientPK, branchReport2.PK));
			AssertEquals("Still linked to address", address1.PK, clientBranch1.LCB_OA);
			AssertNotEquals("Should not link to existing address which has another linked branch", address1.PK, clientBranch2.LCB_OA);
		}

		public void TestSync_SameBranchAddress()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var clientCompany = licence.ClientCompany;
			clientCompany.LCC_Code = "AAA";

			var org = database.LicEnterprise.Organisation;
			var address1 = org.Addresses.AddNew();

			address1.OA_Address1 = "1 Branch Rd";
			address1.OA_Address2 = "Building A";
			address1.OA_City = "Sydney";
			address1.OA_State = "NSW";
			address1.OA_PostCode = "2000";
			address1.OA_RL_NKRelatedPortCode = "AUSYD";
			address1.OA_RN_NKCountryCode = "AU";
			address1.OA_ValidationStatus = AddressValidationStatus.Invalid;

			Factory.Save();

			var branchReport1 = new BranchReport()
			{
				PK = Guid.NewGuid(),
				Code = "BN1",
				BranchName = "Branch Sydney",
				CompanyName = "Company AAA",
				IsActive = true,
				Address1 = address1.Address1,
				Address2 = address1.OA_Address2,
				City = address1.OA_City,
				State = address1.OA_State,
				PostCode = address1.OA_PostCode,
				Unloco = address1.OA_RL_NKRelatedPortCode,
				CountryCode = address1.OA_RN_NKCountryCode,
				ValidationStatus = AddressValidationStatus.Verified
			};

			var branchReport2 = new BranchReport()
			{
				PK = Guid.NewGuid(),
				Code = "BN2",
				BranchName = "Branch Melbourne",
				CompanyName = "Company AAA",
				IsActive = true,
				Address1 = address1.Address1,
				Address2 = address1.OA_Address2,
				City = address1.OA_City,
				State = address1.OA_State,
				PostCode = address1.OA_PostCode,
				Unloco = address1.OA_RL_NKRelatedPortCode,
				CountryCode = address1.OA_RN_NKCountryCode,
				ValidationStatus = AddressValidationStatus.Invalid
			};

			var branchReports = new List<BranchReport>() { branchReport1, branchReport2 };

			ClientBranch.Sync(Factory, database, branchReports);
			Factory.Save();

			var clientBranch1 = Factory.LoadTop1<ClientBranch>(new ZQuery(ClientBranchSchema.LCB_ClientPK, branchReport1.PK));
			AssertEquals("Linked to existing address", address1.PK, clientBranch1.LCB_OA);
			AssertEquals("Address validation updated", address1.OA_ValidationStatus, AddressValidationStatus.Verified);

			var clientBranch2 = Factory.LoadTop1<ClientBranch>(new ZQuery(ClientBranchSchema.LCB_ClientPK, branchReport2.PK));
			AssertNotEquals("Should not link to existing address which has another linked branch", address1.PK, clientBranch2.LCB_OA);
		}

		public void TestSync_NotUseMainAddress()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var clientCompany = licence.ClientCompany;
			clientCompany.LCC_Code = "AAA";

			var org = database.LicEnterprise.Organisation;

			var address1 = org.MainAddress;
			address1.OA_RL_NKRelatedPortCode = "";
			address1.OA_Address1 = "1 Branch Rd";
			address1.OA_Address2 = "Building A";
			address1.OA_City = "Sydney";
			address1.OA_State = "NSW";
			address1.OA_PostCode = "2000";
			address1.OA_RN_NKCountryCode = "AU";

			var address2 = org.Addresses.AddNew();
			address2.OA_RL_NKRelatedPortCode = "AUSYD";
			address2.OA_Address1 = "1 Branch Rd";
			address2.OA_Address2 = "Building A";
			address2.OA_City = "Sydney";
			address2.OA_State = "NSW";
			address2.OA_PostCode = "2000";
			address2.OA_RN_NKCountryCode = "AU";

			Factory.Save();

			var branchReport1 = new BranchReport()
			{
				PK = Guid.NewGuid(),
				Code = "BN1",
				BranchName = "Branch Sydney",
				CompanyName = "Company AAA",
				IsActive = true,
				Address1 = "1 Branch Rd",
				Address2 = "Building A",
				City = "Sydney",
				State = "NSW",
				PostCode = "2000",
				Unloco = "AUSYD",
				CountryCode = "AU",
				ValidationStatus = AddressValidationStatus.Verified
			};

			var branchReports = new List<BranchReport>() { branchReport1 };

			ClientBranch.Sync(Factory, database, branchReports);
			Factory.Save();
			AssertClientBranch(branchReport1);

			var clientBranch1 = Factory.LoadTop1<ClientBranch>(new ZQuery(ClientBranchSchema.LCB_ClientPK, branchReport1.PK));
			AssertEquals("Linked to non main address", address2.PK, clientBranch1.LCB_OA);

			clientBranch1.LCB_OA = address1.PK;
			Factory.Save();
			ClientBranch.Sync(Factory, database, branchReports);
			Factory.Save();
			clientBranch1.Reload();
			AssertEquals("Should update link to non main address", address2.PK, clientBranch1.LCB_OA);
		}

		public void TestSync_Deactivation()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var clientCompany = licence.ClientCompany;
			clientCompany.LCC_Code = "AAA";

			var org = database.LicEnterprise.Organisation;
			var address1 = org.Addresses.AddNew();
			address1.FillWithValidTestData();

			var clientBranch1 = Factory.New<ClientBranch>();
			clientBranch1.LCB_LD = database.PK;
			clientBranch1.LCB_Code = "BN1";
			clientBranch1.LCB_Name = "Branch Sydney";
			clientBranch1.LCB_ClientPK = ZGuid.NewZGuid();
			clientBranch1.LCB_OA = address1.PK;

			Factory.Save();

			var branchReport1 = new BranchReport()
			{
				PK = clientBranch1.LCB_ClientPK.ToGuid(),
				Code = "BN1",
				BranchName = "Branch Sydney",
				CompanyName = "Company AAA",
				IsActive = false,
				Address1 = "1 Branch Rd",
				Address2 = "Building A",
				City = "Sydney",
				State = "NSW",
				PostCode = "2000",
				Unloco = "AUSYD",
				CountryCode = "AU",
				ValidationStatus = AddressValidationStatus.Verified
			};

			var branchReports = new List<BranchReport>() { branchReport1 };

			ClientBranch.Sync(Factory, database, branchReports);
			Factory.Save();

			address1.Reload();
			AssertEquals("Should be deactivated", false, address1.OA_IsActive);
		}

		public void TestSync_ChangedDatabaseWebAccessOrg()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");

			var database = licence.Database;
			var clientCompany = licence.ClientCompany;
			clientCompany.LCC_Code = "AAA";

			var org = database.LicEnterprise.Organisation;
			var address1 = org.Addresses.AddNew();
			address1.FillWithValidTestData();

			var clientBranch1 = Factory.New<ClientBranch>();
			clientBranch1.LCB_LD = database.PK;
			clientBranch1.LCB_Code = "BN1";
			clientBranch1.LCB_Name = "Branch Sydney";
			clientBranch1.LCB_ClientPK = ZGuid.NewZGuid();
			clientBranch1.LCB_OA = address1.PK;

			Factory.Save();

			var licence2 = BillingTestHelper.CreateAnotherLicence(database, "DEF");
			var newOrg = licence2.Company.Header;
			database.LD_OH_WebAccessOrg = newOrg.PK;

			Factory.Save();

			var branchReport1 = new BranchReport()
			{
				PK = clientBranch1.LCB_ClientPK.ToGuid(),
				Code = "BN1",
				BranchName = "Branch Sydney",
				CompanyName = "Company AAA",
				IsActive = true,
				Address1 = "1 Branch Rd",
				Address2 = "Building A",
				City = "Sydney",
				State = "NSW",
				PostCode = "2000",
				Unloco = "AUSYD",
				CountryCode = "AU",
				ValidationStatus = AddressValidationStatus.Verified
			};

			var branchReports = new List<BranchReport>() { branchReport1 };

			ClientBranch.Sync(Factory, database, branchReports);
			Factory.Save();

			address1.Reload();
			clientBranch1.Reload();
			var importedAddress = newOrg.Addresses.Cast<OrgAddress>().First(x => x.OA_CompanyNameOverride == "Company AAA");

			AssertNotEquals("New address should be imported", address1.PK, importedAddress.PK);
			AssertEquals("Branch should be linked to new address", importedAddress.PK, clientBranch1.LCB_OA);
			AssertEquals("Address should be imported to correct org", newOrg.PK, importedAddress.OA_OH);
			AssertEquals("Old address should be deactivated", false, address1.OA_IsActive);
		}

		public void TestSync_AddressCode()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var clientBranch1 = Factory.New<ClientBranch>();
			clientBranch1.LCB_LD = database.PK;
			clientBranch1.LCB_Code = "BN1";
			clientBranch1.LCB_Name = "Branch Sydney";
			clientBranch1.LCB_ClientPK = ZGuid.NewZGuid();

			var address1 = org.Addresses.AddNew();
			address1.FillWithValidTestData();
			address1.OA_Code = "Branch Sydney (BN1)";

			Factory.Save();

			var branchReport1 = new BranchReport()
			{
				PK = clientBranch1.LCB_ClientPK.ToGuid(),
				Code = "BN1",
				BranchName = "Branch Sydney",
				CompanyName = "Company AAA",
				IsActive = true,
				Address1 = "1 Branch Rd",
				Address2 = "Building A",
				City = "Sydney",
				State = "NSW",
				PostCode = "2000",
				Unloco = "AUSYD",
				CountryCode = "AU",
				ValidationStatus = AddressValidationStatus.Verified
			};

			var branchReports = new List<BranchReport>() { branchReport1 };

			ClientBranch.Sync(Factory, database, branchReports);
			Factory.Save();

			var clientBranch = Factory.LoadTop1<ClientBranch>(new ZQuery(ClientBranchSchema.LCB_ClientPK, branchReport1.PK));
			var address = clientBranch.Address;
			AssertNotNull("Related OrgAddress", address);

			AssertEquals("Address Short Code", "Branch Sydney_1 (BN1)", address.OA_Code);
		}

		public void TestSync_AddressCodeUpdate()
		{
			var licence = BillingTestHelper.CreateLicence(Factory, "DDD", "ABC", "SYD");
			var database = licence.Database;
			var org = database.LicEnterprise.Organisation;

			var address1 = Factory.New<OrgAddress>();
			address1.OA_OH = org.PK;
			address1.OA_Address1 = "Am Genter Ufer 7";
			address1.OA_RN_NKCountryCode = "DE";
			address1.OA_Code = "Hamburg Cargo Cente (HA5)";

			var address2 = Factory.New<OrgAddress>();
			address2.OA_OH = org.PK;
			address2.OA_Address1 = "Am Genter Ufer 7";
			address2.OA_RN_NKCountryCode = "DE";
			address2.OA_Code = "Hamburg Cargo Cen_1 (HA5)";

			var branch1 = Factory.New<ClientBranch>();
			branch1.LCB_LD = database.PK;
			branch1.LCB_Code = "HA5";
			branch1.LCB_ClientPK = Guid.NewGuid();
			branch1.LCB_OA = address1.PK;

			var branch2 = Factory.New<ClientBranch>();
			branch2.LCB_LD = database.PK;
			branch2.LCB_Code = "HA5";
			branch2.LCB_ClientPK = Guid.NewGuid();
			branch2.LCB_OA = address2.PK;

			Factory.Save();

			var branchReport1 = new BranchReport()
			{
				PK = branch1.LCB_ClientPK.ToGuid(),
				Code = "H95",
				BranchName = "Hamburg Cargo Center",
				CompanyName = "Company AAA",
				CompanyCode = "DE2",
				IsActive = true,
				Address1 = "Am Genter Ufer 7",
				Address2 = "",
				City = "",
				State = "",
				PostCode = "",
				Unloco = "",
				CountryCode = "DE",
				ValidationStatus = AddressValidationStatus.ToBeVerified
			};

			var branchReport2 = new BranchReport()
			{
				PK = branch2.LCB_ClientPK.ToGuid(),
				Code = "HA5",
				BranchName = "Hamburg Cargo Center",
				CompanyName = "Company AAA",
				CompanyCode = "DE1",
				IsActive = true,
				Address1 = "Am Genter Ufer 7",
				Address2 = "",
				City = "",
				State = "",
				PostCode = "",
				Unloco = "",
				CountryCode = "DE",
				ValidationStatus = AddressValidationStatus.ToBeVerified
			};

			var branchReports = new List<BranchReport>() { branchReport1, branchReport2 };

			ClientBranch.Sync(Factory, database, branchReports);
			Factory.Save();

			branch1.Reload();
			branch2.Reload();
			AssertEquals(branchReport1.CompanyCode, branch1.LCB_LCC_Code);
			AssertEquals(branchReport2.CompanyCode, branch2.LCB_LCC_Code);
		}

		void AssertClientBranch(BranchReport report)
		{
			var factory = new BusinessObjectFactory();
			var clientBranch = factory.LoadTop1<ClientBranch>(new ZQuery(ClientBranchSchema.LCB_ClientPK, report.PK));
			AssertNotNull("Client Branch", clientBranch);
			AssertEquals("LCB_Code", report.Code, clientBranch.LCB_Code);
			AssertEquals("LCB_Name", report.BranchName, clientBranch.LCB_Name);

			var address = clientBranch.Address;
			AssertNotNull("Related OrgAddress", address);

			AssertEquals("Address Short Code", $"{report.BranchName} ({report.Code})", address.OA_Code);
			AssertEquals("Address Company Name", report.CompanyName, address.OA_CompanyNameOverride);
			AssertEquals("Address 1", report.Address1 ?? string.Empty, address.OA_Address1);
			AssertEquals("Address 2", report.Address2 ?? string.Empty, address.OA_Address2);
			AssertEquals("City", report.City ?? string.Empty, address.OA_City);
			AssertEquals("State", report.State ?? string.Empty, address.OA_State);
			AssertEquals("Postcode", report.PostCode ?? string.Empty, address.OA_PostCode);
			AssertEquals("Unloco", report.Unloco ?? string.Empty, address.OA_RL_NKRelatedPortCode);
			AssertEquals("Country", report.CountryCode ?? string.Empty, address.OA_RN_NKCountryCode);

			var validationStatus = report.ValidationStatus != null ?
				(
					report.ValidationStatus == AddressValidationStatus.Invalid ||
					report.ValidationStatus == AddressValidationStatus.ToBeVerified
					? AddressValidationStatus.ManuallyVerified
					: report.ValidationStatus
				) : string.Empty;
			AssertEquals("ValidationStatus", validationStatus, address.OA_ValidationStatus);

			if (string.IsNullOrEmpty(report.CompanyCode))
			{
				AssertEquals(true, clientBranch.LCB_LCC_Code.IsEmpty);
			}
			else
			{
				var companyQuery = new ZQuery(ClientCompanySchema.LCC_Code, report.CompanyCode);
				companyQuery.AddToFilter(ClientCompanySchema.LCC_LD, clientBranch.LCB_LD);
				var clientCompany = factory.LoadTop1<ClientCompany>(companyQuery);
				AssertNotNull(clientCompany);
				AssertEquals(clientCompany.LCC_Code, clientBranch.LCB_LCC_Code);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			var branch = Factory.New<ClientBranch>();
			branch.LCB_LD = database.PK;
			return branch;
		}
	}
}
