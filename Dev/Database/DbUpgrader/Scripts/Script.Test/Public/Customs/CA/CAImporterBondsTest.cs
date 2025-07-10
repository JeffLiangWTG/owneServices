using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(CAImporterBonds))]
	class CAImporterBondsTest : DbCreateScriptTest
	{
		public void TestCAImporterBonds()
		{
			var org1PK = Guid.NewGuid();
			TestDataCreator.CreateOrganisation("org1", "Organisation 1", orgHeaderPK: org1PK);
			TestDataCreator.CreateOrgCusCode(org1PK, "BRM", "00001", "CA");
			TestDataCreator.CreateCusBondDetail(org1PK, "CAC", new DateTime(2024, 01, 01), new DateTime(2024, 03, 01), "0123456789", "8", "ABC");
			TestDataCreator.CreateCusBondDetail(org1PK, "CUS", new DateTime(2024, 01, 01), new DateTime(2024, 03, 01), "0123456789", "8", "ABC");
			TestDataCreator.CreateCusBondDetail(org1PK, "CAC", new DateTime(2024, 05, 01), new DateTime(2024, 10, 01), "0123456780", "9", "BCD");
			var sql = "select * from dbo.CAImporterBonds()";
			var modesDataTable = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(2, modesDataTable.Rows.Count);
			AssertCAImporterBonds(modesDataTable.Rows[0], "org1", "Organisation 1", "BRM: 00001", "8", "0123456789", "ABC", 1);
			AssertCAImporterBonds(modesDataTable.Rows[1], "org1", "Organisation 1", "BRM: 00001", "9", "0123456780", "BCD", 5);
		}

		void AssertCAImporterBonds(DataRow row, string orgCode, string orgName, string businessNumber, string bondType, string bondNumber, string suretyCode, int bondEffectiveMonth)
		{
			AssertEquals("OH_Code", orgCode, (string)row["OH_Code"]);
			AssertEquals("OH_FullName", orgName, (string)row["OH_FullName"]);
			AssertEquals("BusinessNumber", businessNumber, (string)row["BusinessNumber"]);
			AssertEquals("PW_BondType", bondType, (string)row["PW_BondType"]);
			AssertEquals("PW_BondNumber", bondNumber, (string)row["PW_BondNumber"]);
			AssertEquals("PW_SuretyCode", suretyCode, (string)row["PW_SuretyCode"]);
			AssertEquals("BondEffectiveMonth", bondEffectiveMonth, (int)row["BondEffectiveMonth"]);
		}
	}
}
