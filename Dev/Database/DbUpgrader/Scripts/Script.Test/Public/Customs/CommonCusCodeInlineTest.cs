using System;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs
{
	[TestedType(typeof(CommonCusCodeInline))]
	class CommonCusCodeInlineTest : DbCreateScriptTest
	{
		public void TestCommonCusCodeInline()
		{
			var sqlString = "select CusCodes.Result from dbo.CommonCusCodeInline('{0}', '{1}', '{2}', '{3}') AS CusCodes";
			var org1PK = Guid.NewGuid();
			TestDataCreator.CreateOrganisation("org1", "Organisation 1", orgHeaderPK: org1PK);
			TestDataCreator.CreateOrgCusCode(org1PK, "BRM", "00002", "US");
			TestDataCreator.CreateOrgCusCode(org1PK, "BRM", "00001", "CA");
			var sql = string.Format(sqlString, org1PK, "CA", "BRM", "");
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("BRM: 00001", (string)result.Rows[0]["Result"]);

			var org2PK = Guid.NewGuid();
			TestDataCreator.CreateOrganisation("org2", "Organisation 2", orgHeaderPK: org2PK);
			TestDataCreator.CreateOrgCusCode(org2PK, "BRM", "", "CA");
			sql = string.Format(sqlString, org2PK, "CA", "BRM", "");
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(0, result.Rows.Count);

			var org3PK = Guid.NewGuid();
			TestDataCreator.CreateOrganisation("org3", "Organisation 3", orgHeaderPK: org3PK);
			TestDataCreator.CreateOrgCusCode(org3PK, "BRM", "123123", "US");
			TestDataCreator.CreateOrgCusCode(org3PK, "ABC", "0002", "US");
			sql = string.Format(sqlString, org3PK, "CA", "BRM", "");
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(0, result.Rows.Count);

			sql = string.Format(sqlString, org3PK, "US", "BRM", "ABC");
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("BRM: 123123", (string)result.Rows[0]["Result"]);

			sql = string.Format(sqlString, org3PK, "US", "ABC", "BRM");
			result = DataUtils.GetDataTableFromQuery(TestConnection, sql);
			AssertEquals(1, result.Rows.Count);
			AssertEquals("ABC: 0002", (string)result.Rows[0]["Result"]);
		}
	}
}
