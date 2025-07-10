using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.ReportFunctions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.ReportFunctions.MasterFiles.Testing
{
	[TestedType(typeof(vw_JobDocAddress))]
	class vw_JobDocAddressEDWTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestView()
		{
			CreateOrganisation("TSTORG", "Test organization", "AUSYD", 1);
			CreateOrgAddress(1, "Address", "123 Main Street1", "Add2", "SYD", "NSW", "1234", 11, "NZAKL");
			CreateDocAddress(11, Guid.NewGuid(), Guid.NewGuid(), "JS", "CRD", 0, 0, "TEST", 21);
			string sql = string.Format(
							@"SELECT * FROM [{0}].[dbo].[vw_JobDocAddress]",
							ScriptDbName);
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals("JS", result.Rows[0]["E2_ParentTableCode"]);
			AssertEquals("CRD", result.Rows[0]["E2_AddressType"]);
			AssertEquals("Address", result.Rows[0]["OA_Code"]);
			AssertEquals("TSTORG", result.Rows[0]["OH_Code"]);
			AssertEquals("Test organization", result.Rows[0]["CompanyName"]);
			AssertEquals("123 Main Street1", result.Rows[0]["Address1"]);
			AssertEquals("Add2", result.Rows[0]["Address2"]);
			AssertEquals("1234", result.Rows[0]["PostCode"]);
			AssertEquals("SYD", result.Rows[0]["City"]);
			AssertEquals("NSW", result.Rows[0]["State"]);
			AssertEquals("NZ", result.Rows[0]["CountryCode"]);
		}

		Guid CreateDocAddress(int orgAddressKey,Guid? docAddressID, Guid? parentID, string parentTableCode, string addressType, int? addressOverride, int? addressSequence, string companyName, int docAddressKey)
		{
			var addressID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[InternationalLogistics].[BAS__DocAddress] (OrganizationAddressKey, DocAddressID, ParentID, ParentTableCode, AddressType, AddressOverride, AddressSequence, CompanyName, DocAddressKey) VALUES ({1}, '{2}', '{3}', '{4}', '{5}', {6}, {7}, '{8}', {9})",
				ScriptDbName, orgAddressKey, docAddressID, parentID, parentTableCode, addressType, addressOverride, addressSequence, companyName, docAddressKey);

			TestConnection.ExecuteNonQuery(sql);
			return addressID;
		}

		void CreateOrgAddress(int orgKey, string code, string address1, string address2, string city, string state, string postCode, int addressKey, string relatedPortCode)
		{
			var addressID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Organization].[BAS__OrganizationAddress] (OrganizationKey, Code, Address1, Address2, City, State, PostCode, OrganizationAddressKey, OrganizationAddressID, RelatedPortCode) VALUES ({1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', {8}, '{9}', '{10}')",
				ScriptDbName, orgKey, code, address1, address2, city, state, postCode, addressKey, addressID, relatedPortCode);

			TestConnection.ExecuteNonQuery(sql);
		}

		void CreateOrganisation(string code, string name, string closestPort, int orgKey)
		{
			var orgID = Guid.NewGuid();

			string sql = string.Format(
				@"INSERT INTO [{0}].[Organization].[BAS__Organization] (Code, FullName, OrganizationKey, OrganizationID, ClosestPort) VALUES ('{1}', '{2}', {3}, '{4}', '{5}')",
				ScriptDbName, code, name, orgKey, orgID, closestPort);

			TestConnection.ExecuteNonQuery(sql);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}
	}
}

