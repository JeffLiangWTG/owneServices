using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ComplianceReport;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(RankingAddresses))]
	class RankingAddressesTest : BiCreateScriptTest
	{
		public void TestRankingAddressWithGlobalOrgIsTrue()
		{
			SetUpForRankingAddressWithGlobalOrgIsTrue();
			var result = RunScript(true, "AU", "EN", true, false, false, false);
			AssertEquals($"The ARM address Key should be '1'", "1",(result.ToString()));

			result = RunScript(true, "AU", "EN", false, true, false, false);
			AssertEquals($"The ARM address Key should be '3'", "3", result.ToString());

			result = RunScript(true, "AU", "EN", false, false, true, false);
			AssertEquals($"The ARM address Key should be '5'", "5", result.ToString());

			result = RunScript(true, "AU", "EN", false, false, false, true);
			AssertEquals($"The ARM address Key should be '7'", "7", result.ToString());
		}

		public void TestRankingAddressWithGlobalOrgIsFalse()
		{
			SetUpForRankingAddressWithGlobalOrgIsFalse();
			var result = RunScript(false, "AU", "EN", true, false, false, false);
			AssertEquals($"The ARM address Key should be '2'", "2", result.ToString());

			result = RunScript(false, "AU", "EN", false, true, false, false);
			AssertEquals($"The ARM address Key should be '3'", "3", result.ToString());

			result = RunScript(false, "AU", "EN", false, false, true, false);
			AssertEquals($"The ARM address Key should be '5'", "5", result.ToString());

			result = RunScript(false, "AU", "EN", false, false, false, true);
			AssertEquals($"The ARM address Key should be '8'", "8", result.ToString());
		}

		public void TestRankingAddressWithNotSetMainAddress()
		{
			SetUpRankingAddressWithNotSetMainAddress();
			var result = RunScript(true, "AU", "EN", true, false, false, false);
			AssertEquals($"The ARM address Key should be '2'", "2", result.ToString());

			result = RunScript(true, "AU", "EN", false, true, false, false);
			AssertEquals($"The ARM address Key should be '3'", "3", result.ToString());

			result = RunScript(true, "AU", "EN", false, false, true, false);
			AssertEquals($"The ARM address Key should be NULL", DBNull.Value, result);

			result = RunScript(true, "AU", "EN", false, false, false, true);
			AssertEquals($"The ARM address Key should be '8'", "8", result.ToString());
		}

		void SetUpRankingAddressWithNotSetMainAddress()
		{
			Helper.InsertAddress("DXX", 1, "AU", "CNSYD", "EN-US");
			Helper.InsertOrganizationAddressCapability(1, "ARM", 0);
			Helper.InsertAddress("DX2", 1, "AU", "AUSYD", "EN-US");
			Helper.InsertOrganizationAddressCapability(2, "ARM", 1);

			Helper.InsertAddress("DX3", 1, "AU", "CNAAA", "EN-US");
			Helper.InsertOrganizationAddressCapability(3, "APM", 1);
			Helper.InsertAddress("DX4", 1, "AU", "AUAAA", "EN-US");
			Helper.InsertOrganizationAddressCapability(4, "APM", 0);

			Helper.InsertAddress("DX5", 1, "AU", "AUBBB", "EN-US");
			Helper.InsertOrganizationAddressCapability(5, "PST", 0);
			Helper.InsertAddress("DX6", 1, "AU", "AUBBB", "EN-US");
			Helper.InsertOrganizationAddressCapability(6, "PST", 0);

			Helper.InsertAddress("DX7", 1, "AU", "CNSYD", "EN-US");
			Helper.InsertOrganizationAddressCapability(7, "OFC", 0);
			Helper.InsertAddress("DX8", 1, "AU", "AUSYD", "EN-US");
			Helper.InsertOrganizationAddressCapability(8, "OFC", 1);
		}

		void SetUpForRankingAddressWithGlobalOrgIsTrue()
		{
			Helper.InsertAddress("DXX", 1, "AU", "AUSYD", "");
			Helper.InsertOrganizationAddressCapability(1, "ARM", 1);
			Helper.InsertAddress("DX2", 1, "AU", "AUSYD", "");
			Helper.InsertOrganizationAddressCapability(2, "ARM", 0);

			Helper.InsertAddress("DX3", 1, "AU", "AUAAA", "");
			Helper.InsertOrganizationAddressCapability(3, "APM", 1);
			Helper.InsertAddress("DX4", 1, "AU", "AUAAA", "");
			Helper.InsertOrganizationAddressCapability(4, "APM", 0);

			Helper.InsertAddress("DX5", 1, "AU", "AUBBB", "");
			Helper.InsertOrganizationAddressCapability(5, "PST", 1);
			Helper.InsertAddress("DX6", 1, "AU", "AUBBB", "");
			Helper.InsertOrganizationAddressCapability(6, "PST", 0);

			Helper.InsertAddress("DX7", 1, "AU", "AUSYD", "");
			Helper.InsertOrganizationAddressCapability(7, "OFC", 1);
			Helper.InsertAddress("DX8", 1, "AU", "AUSYD", "");
			Helper.InsertOrganizationAddressCapability(8, "OFC", 0);
		}

		void SetUpForRankingAddressWithGlobalOrgIsFalse()
		{
			Helper.InsertAddress("DXX", 1, "AU", "CNSYD", "");
			Helper.InsertOrganizationAddressCapability(1, "ARM", 0);
			Helper.InsertAddress("DX2", 1, "AU", "AUSYD", "");
			Helper.InsertOrganizationAddressCapability(2, "ARM", 1);

			Helper.InsertAddress("DX3", 1, "AU", "AUAAA", "");
			Helper.InsertOrganizationAddressCapability(3, "APM", 1);
			Helper.InsertAddress("DX4", 1, "AU", "AUAAA", "");
			Helper.InsertOrganizationAddressCapability(4, "APM", 0);

			Helper.InsertAddress("DX5", 1, "AU", "AUBBB", "");
			Helper.InsertOrganizationAddressCapability(5, "PST", 1);
			Helper.InsertAddress("DX6", 1, "AU", "AUBBB", "");
			Helper.InsertOrganizationAddressCapability(6, "PST", 0);

			Helper.InsertAddress("DX7", 1, "AU", "CNSYD", "");
			Helper.InsertOrganizationAddressCapability(7, "OFC", 0);
			Helper.InsertAddress("DX8", 1, "AU", "AUSYD", "");
			Helper.InsertOrganizationAddressCapability(8, "OFC", 1);
		}

		object RunScript(bool isGlobalOrg, string currentCountryCode, string orgLanguage, bool isARM, bool isAPM, bool isPST, bool isOFC)
		{
			var sql = $@"SELECT * FROM {ScriptDbName}.dbo.RankingAddresses(1, 1, 1, {(isGlobalOrg ? "1" : "0")}, '{currentCountryCode}', '{orgLanguage}', {(isARM ? 1 : 0)}, {(isAPM ? 1 : 0)}, {(isPST ? 1 : 0)}, {(isOFC ? 1 : 0)})";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("The result should contain 1 rows", 1, result.Rows.Count);
			return result.Rows[0]["Result"];
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			Helper.InsertCompany("AUD", "DAU", countryCode: "AU");
			Helper.CreateBASBranch(1, Guid.NewGuid(), 1, "SYD");
			Helper.InsertOrganization("AUOrg", Guid.NewGuid());
		}

		AccountingFunctionTestingHelper Helper => helper ?? (helper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper helper;
	}
}
