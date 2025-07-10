using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting.ReportingBooks;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing
{
	[TestedType(typeof(GetOrganizationAttribute))]
	class GetOrganizationAttributeTest : BiCreateScriptTest
	{
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		public void TestOrganizationAttribute()
		{
			PrepareTestData();

			var result = Excute(1, 1);
			AssertEquals("XX is not EUN", 1, result.Rows.Count);
			AssertEquals("Attribute_LFO = LOC", 1, result.Select("Attribute_LFE = 'LOC' and Attribute_LFO = 'LOC' and countryKey = 2 and OrganizationAddressKey = 1").Length);

			result = Excute(1, 2);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Attribute_LFO = FOR", 1, result.Select("Attribute_LFE = 'OEU' and Attribute_LFO = 'FOR' and countryKey = 2 and OrganizationAddressKey = 1").Length);

			result = Excute(2, 1);
			AssertEquals("YY is EUN", 1, result.Rows.Count);
			AssertEquals("Attribute_LFO = FOR", 1, result.Select("Attribute_LFE = 'WEU' and Attribute_LFO = 'FOR' and countryKey = 1 and OrganizationAddressKey = 2").Length);

			result = Excute(2, 2);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("Attribute_LFO = LOC", 1, result.Select("Attribute_LFE = 'LOC' and Attribute_LFO = 'LOC' and countryKey = 1 and OrganizationAddressKey = 2").Length);

			result = Excute(3, 2);
			AssertEquals("No data of there's no mainAddress", 0, result.Rows.Count);

			result = Excute(4, 2);
			AssertEquals("No data if there's no address", 0, result.Rows.Count);
		}

		DataTable Excute(int organizationKey, int companyKey)
		{
			var sql = $@"SELECT * FROM [{ScriptDbName}].[dbo].[GetOrganizationAttribute]('{organizationKey}', {companyKey})";
			return DataUtils.GetDataTableFromQuery(TestConnection, sql);
		}

		void PrepareTestData()
		{
			var sql = $@"DELETE FROM [{ScriptDbName}].[Organization].[BAS__OrganizationAddress];
						DELETE FROM [{ScriptDbName}].[Organization].[BAS__Company];
						DELETE FROM [{ScriptDbName}].[Geography].[BAS__Country]";
			TestConnection.ExecuteNonQuery(sql);

			TestHelper.InsertCompany("DXX", "DCN", 1, 1, "XX");
			TestHelper.InsertCompany("DYY", "DYY", 1, 1, "YY");
			TestHelper.InsertCountry("YY");
			TestHelper.InsertCountry("XX", economicGrouping: "DDD");
			TestHelper.InsertAddress("DXX", countryCode: "XX");
			TestHelper.InsertAddress("DYY", 2, "YY");
			TestHelper.InsertAddress("DYY", 3, "YY");
			TestHelper.InsertOrganizationAddressCapability(1);
			TestHelper.InsertOrganizationAddressCapability(2);
			TestHelper.InsertOrganizationAddressCapability(1, "JJJ");
			TestHelper.InsertOrganizationAddressCapability(2, "JJJ");
			TestHelper.InsertOrganizationAddressCapability(1, isMainAddress: 0);
			TestHelper.InsertOrganizationAddressCapability(2, isMainAddress: 0);
		}

		AccountingFunctionTestingHelper TestHelper => fTestHelper ?? (fTestHelper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName));
		AccountingFunctionTestingHelper fTestHelper;
	}
}
