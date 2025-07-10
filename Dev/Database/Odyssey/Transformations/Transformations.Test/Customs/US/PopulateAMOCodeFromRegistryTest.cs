using System;
using CargoWise.Data;
using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformation.DataModification.Testing;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.US;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Customs.US
{
	[TestedType(typeof(PopulateAMOCodeFromRegistry))]
	class PopulateAMOCodeFromRegistryTest : DataTransformationTestCase
	{
		protected override void AssertTransformationResults()
		{
			AssertOrgCusCode("Org1 should have AMO code from the system", org1, "AVVVCAS", org1MainAddress);
			AssertOrgCusCode("Org2 should have AMO code from the company2", org2, "1112333", org2MainAddress);
			AssertOrgCusCode("Org3 should have AMO code from the system", org3, "AVVVCAS", org3MainAddress);
			AssertOrgCusCode("Org4 should have AMO code from the system", org4, "AVVVCAS", org4MainAddress);
		}

		void AssertOrgCusCode(string message, Guid orgPK, string expectedAMOCode, Guid expectedAddressPK)
		{
			var amoCode = Db.Connection.ExecuteScalar($"SELECT OK_CustomsRegNo FROM dbo.OrgCusCode WHERE OK_OH = '{orgPK}' AND OK_CodeType = 'AMO' AND OK_RN_NKCodeCountry = 'US'");
			AssertEquals(message, expectedAMOCode, amoCode);

			var premisesAddress = Db.Connection.ExecuteScalar($"SELECT OK_OA_PremisesAddress FROM dbo.OrgCusCode WHERE OK_OH = '{orgPK}' AND OK_CodeType = 'AMO' AND OK_RN_NKCodeCountry = 'US'");
			AssertEquals(expectedAddressPK, premisesAddress);
		}

		protected override DataTransformation GetNewTestTransformationInstance() => new PopulateAMOCodeFromRegistry();

		protected override void PrepareTestData()
		{
			var testDataCreator = new TransformationTestDataCreator();
			org1 = testDataCreator.CreateOrgHeader("Org1", "Org Header 1");
			org1MainAddress = testDataCreator.CreateOfficeAddress(org1, "Test1", "Test 1");
			var company1 = testDataCreator.CreateGlbCompany("DUS", "US", org1);

			org2 = testDataCreator.CreateOrgHeader("Org2", "Org Header 2");
			org2MainAddress = testDataCreator.CreateOfficeAddress(org2, "Test2", "Test 2");
			var company2 = testDataCreator.CreateGlbCompany("DCA", "CA", org2);
			var company3 = testDataCreator.CreateGlbCompany("DC1", "CA", org2);

			org3 = testDataCreator.CreateOrgHeader("Org3", "Org Header 3");
			org3MainAddress = testDataCreator.CreateOfficeAddress(org3, "Test3", "Test 3");
			var company4 = testDataCreator.CreateGlbCompany("DPR", "PR", org3);
			var company5 = testDataCreator.CreateGlbCompany("DP1", "PR", org3);

			org4 = testDataCreator.CreateOrgHeader("Org4", "Org Header 4");
			org4MainAddress = testDataCreator.CreateOfficeAddress(org4, "Test4", "Test4");
			var company6 = testDataCreator.CreateGlbCompany("DUK", "UK", org4);

			var sql = $@"INSERT dbo.StmData(SD_PK, SD_Name, SD_Type, SD_IsLogged, SD_BinaryValue, SD_Owner) VALUES
	(NEWID(), 'USAirAMSOriginatorCode', 'STR', 1, 0x4100560056005600430041005300, null),
	(NEWID(), 'USAirAMSOriginatorCode', 'STR', 1, 0x3100310031003200330033003300, '{company2}'),
	(NEWID(), 'USAirAMSOriginatorCode', 'STR', 0, 0x3100310031003200330033003300, '{company6}');";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}
		Guid org1;
		Guid org2;
		Guid org3;
		Guid org4;
		Guid org1MainAddress;
		Guid org2MainAddress;
		Guid org3MainAddress;
		Guid org4MainAddress;
	}
}
