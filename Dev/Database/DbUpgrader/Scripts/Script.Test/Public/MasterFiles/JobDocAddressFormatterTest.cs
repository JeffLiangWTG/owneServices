using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles
{
	[TestedType(typeof(JobDocAddressFormatter))]
	class JobDocAddressFormatterTest : DbCreateScriptTest
	{
		public void TestJobDocAddressFormatterForAll()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			int counter = 0;

			Action<string, string, string, string, string, string, int> assertJobDocAddress = (address1, address2, city, state, postCode, expectedDocAddress, clusterKey) =>
			{
				counter++;
				var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00" + counter, "IMP", clusterKey);
				var organisation = TestDataCreator.CreateOrganisation("DPOrg" + counter, "DP Test organization" + counter, "AUSYD");
				var address = TestDataCreator.CreateAddress(organisation, "Address", address1, address2, city, state, postCode);
				TestDataCreator.CreateDocAddress(address, "TC" + counter, declaration, "JJ", "LCI");
				var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT DocAddress FROM dbo.JobDocAddressFormatter('{0}', 'JJ', 'LCI', 'ALL')", declaration));
				AssertEquals(expectedDocAddress, result.Rows[0]["DocAddress"]);
			};

			assertJobDocAddress("Add1-1", "Add1-2", "City1", "State1", "1111", "Add1-1\nAdd1-2\nCity1, State1 1111\nAustralia", 1);

			assertJobDocAddress("Add2-1", "", "", "", "", "Add2-1\nAustralia", 2);
			assertJobDocAddress("Add3-1", "Add3-2", "", "", "", "Add3-1\nAdd3-2\nAustralia", 3);

			assertJobDocAddress("Add4-1", "", "", "State2", "", "Add4-1\nState2 \nAustralia", 4);
			assertJobDocAddress("Add5-1", "", "", "", "2222", "Add5-1\n2222\nAustralia", 5);
			assertJobDocAddress("Add6-1", "", "", "State3", "3333", "Add6-1\nState3 3333\nAustralia", 6);
			assertJobDocAddress("Add7-1", "", "City2", "State4", "", "Add7-1\nCity2, State4 \nAustralia", 7);
			assertJobDocAddress("Add8-1", "", "City3", "", "4444", "Add8-1\nCity3, 4444\nAustralia", 8);
			assertJobDocAddress("Add9-1", "", "City4", "State5", "5555", "Add9-1\nCity4, State5 5555\nAustralia", 9);
		}

		public void TestJobDocAddressFormatterForA1A2()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "AU", "AUD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "TB1", "USCHI");
			int counter = 0;

			var declaration = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00" + counter, "IMP", 1);
			var organisation = TestDataCreator.CreateOrganisation("DPOrg" + counter, "DP Test organization" + counter, "AUSYD");
			var address = TestDataCreator.CreateAddress(organisation, "Address", "Add1-1", "Add1-2", "City1", "State1", "1111");
			TestDataCreator.CreateDocAddress(address, "TC" + counter, declaration, "JJ", "LCI");
			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format("SELECT DocAddress FROM dbo.JobDocAddressFormatter('{0}', 'JJ', 'LCI', 'A1, A2')", declaration));
			AssertEquals("Add1-1\nAdd1-2\n", result.Rows[0]["DocAddress"]);
		}
	}
}

