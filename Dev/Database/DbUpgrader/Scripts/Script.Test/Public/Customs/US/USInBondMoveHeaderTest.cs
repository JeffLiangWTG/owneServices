using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.US;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.US
{
	[TestedType(typeof(USInBondMoveHeader))]
	class USInBondMoveHeaderTest : DbCreateScriptTest
	{
		public void TestUSInBondMoveHeaderView()
		{
			var companyPK = TestDataCreator.CreateCompany("USA", "US", "USD");
			var branchPK = TestDataCreator.CreateBranch(companyPK, "PHL", "USPHL", "US");

			var inbondPK = TestDataCreator.CreateCusInbondHeader("INB000001", branchPK, "INB");
			var moveHeaderPk = TestDataCreator.CreateCusInBondMoveHeader(inbondPK);
			TestConnection.ExecuteScalar($"UPDATE dbo.CusInBondMoveHeader SET BM_MessageStatus = 'AWO', BM_SystemLastEditTimeUtc = GETUTCDATE(), BM_SystemLastEditUser = 'E' WHERE BM_PK = '{moveHeaderPk}'");

			var amsPK = TestDataCreator.CreateCusInbondHeader("AMS0000001", branchPK, "AMS");
			TestDataCreator.CreateCusInBondMoveHeader(amsPK);

			var result = DataUtils.GetDataTableFromQuery(TestConnection, "select * from dbo.USInBondMoveHeader");
			AssertEquals("Result should have 1 row", 1, result.Rows.Count);
			AssertEquals("AWO", result.Rows[0]["BMH_MessageStatus"]);
		}
	}
}
