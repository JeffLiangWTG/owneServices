using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.ZA.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA.ModelViews.ZAJobDeclaration))]
	class ZAJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"ZAJobDeclaration",
				"JobDeclaration",
				new []
				{
					new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JE_AGTCode", VarChar, 35),
					new TestDbViewHelper.DbColumn("JE_BOESightDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_BOESightNumber", VarChar, 6),
					new TestDbViewHelper.DbColumn("JE_Carrier", VarChar, 4),
					new TestDbViewHelper.DbColumn("JE_CargoCarrier", VarChar, 35),
					new TestDbViewHelper.DbColumn("JE_IsNonIATAFormatAirWayBill", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_MasterBillIssuedDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_RadioCallSign", VarChar, 10),
					new TestDbViewHelper.DbColumn("JE_RemovalTransportCode", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_ROOCert", VarChar, 35),
					new TestDbViewHelper.DbColumn("JE_ROOType", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_RL_NKMasterBillIssuedAt", VarChar, 5),
					new TestDbViewHelper.DbColumn("JE_Trailer1", VarChar, 10),
					new TestDbViewHelper.DbColumn("JE_Trailer2", VarChar, 10),
					new TestDbViewHelper.DbColumn("JE_VATClaimBackIndicator", VarChar, 1),
					new TestDbViewHelper.DbColumn("JE_VesselAgent", VarChar, 4),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("ZAJobDeclaration doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "ZAJobDeclaration_Idx"));
		}
	}
}
