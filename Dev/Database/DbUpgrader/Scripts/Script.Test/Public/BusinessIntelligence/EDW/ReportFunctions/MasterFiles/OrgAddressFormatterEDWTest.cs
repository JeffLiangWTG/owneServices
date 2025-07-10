using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.MasterFiles.Testing
{
	[TestedType(typeof(OrgAddressFormatter))]
	class OrgAddressFormatterEDWTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestOrgAddressFormatter()
		{
			int counter = 0;
			CreateCountryLocation("AUSYD", "AU", "Australia");

			Action<string, string, string, string, string, string> assertJobDocAddress = (address1, address2, city, state, postCode, expectedDocAddress) =>
			{
				counter++;
				CreateOrganisation("DPOrg" + counter, "DP Test organization" + counter, "AUSYD", counter);
				var address = CreateAddress(counter, "Address", address1, address2, city, state, postCode, counter + 10);
				string sql = string.Format(
								@"SELECT OrgAddress FROM [{0}].[dbo].[OrgAddressFormatter]('{1}')",
								ScriptDbName, address);
				var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
				AssertEquals(expectedDocAddress, result.Rows[0]["OrgAddress"]);
			};

			assertJobDocAddress("Add1-1", "Add1-2", "City1", "State1", "1111", "Add1-1\nAdd1-2\nCity1, State1 1111\nAustralia");

			assertJobDocAddress("Add2-1", "", "", "", "", "Add2-1\nAustralia");
			assertJobDocAddress("Add3-1", "Add3-2", "", "", "", "Add3-1\nAdd3-2\nAustralia");

			assertJobDocAddress("Add4-1", "", "", "State2", "", "Add4-1\nState2 \nAustralia");
			assertJobDocAddress("Add5-1", "", "", "", "2222", "Add5-1\n2222\nAustralia");
			assertJobDocAddress("Add6-1", "", "", "State3", "3333", "Add6-1\nState3 3333\nAustralia");
			assertJobDocAddress("Add7-1", "", "City2", "State4", "", "Add7-1\nCity2, State4 \nAustralia");
			assertJobDocAddress("Add8-1", "", "City3", "", "4444", "Add8-1\nCity3, 4444\nAustralia");
			assertJobDocAddress("Add9-1", "", "City4", "State5", "5555", "Add9-1\nCity4, State5 5555\nAustralia");
		}

		void CreateCountryLocation(string locCode, string countryCode, string countryName)
		{
			string sql = string.Format(
				@"INSERT INTO [{0}].[Geography].[BAS__Country] (Code, Country, CountryKey, CountryID) VALUES ('{1}', '{2}', {3}, '{4}')",
				ScriptDbName, countryCode, countryName, 1, Guid.NewGuid());

			TestConnection.ExecuteNonQuery(sql);

			string sql2 = string.Format(
				@"INSERT INTO [{0}].[Geography].[BAS__Location] (Code, CountryCode, LocationKey, LocationID) VALUES ('{1}', '{2}', {3}, '{4}')",
				ScriptDbName, locCode, countryCode, 1, Guid.NewGuid());

			TestConnection.ExecuteNonQuery(sql2);
		}

		void CreateOrganisation(string code, string name, string closestPort, int orgKey)
		{
			var orgID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Organization].[BAS__Organization] (Code, FullName, OrganizationKey, OrganizationID, ClosestPort) VALUES ('{1}', '{2}', {3}, '{4}', '{5}')",
				ScriptDbName, code, name, orgKey, orgID, closestPort);

			TestConnection.ExecuteNonQuery(sql);
		}

		Guid CreateAddress(int orgKey, string code, string address1, string address2, string city, string state, string postCode, int addressKey)
		{
			var addressID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Organization].[BAS__OrganizationAddress] (OrganizationKey, Code, Address1, Address2, City, State, PostCode, OrganizationAddressKey,OrganizationAddressID) VALUES ({1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', {8}, '{9}')",
				ScriptDbName, orgKey, code, address1, address2, city, state, postCode, addressKey, addressID);

			TestConnection.ExecuteNonQuery(sql);
			return addressID;
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}

