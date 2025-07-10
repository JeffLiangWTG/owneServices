using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.IN.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.IN.ModelViews.INJobDeclaration))]
	class INJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"INJobDeclaration",
				"JobDeclaration",
				new[]
				{
					new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JE_Commissionerate", VarChar, 20),
					new TestDbViewHelper.DbColumn("JE_Division", VarChar, 20),
					new TestDbViewHelper.DbColumn("JE_EPZCode", VarChar, 1),
					new TestDbViewHelper.DbColumn("JE_ExaminationDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_ExaminingOfficerDesignation", VarChar, 30),
					new TestDbViewHelper.DbColumn("JE_ExaminingOfficerName", VarChar, 30),
					new TestDbViewHelper.DbColumn("JE_ExporterType", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_ImporterType", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_Range", VarChar, 20),
					new TestDbViewHelper.DbColumn("JE_RotationDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_RotationNumber", VarChar, 7),
					new TestDbViewHelper.DbColumn("JE_SampleAccompanied", VarChar, 1),
					new TestDbViewHelper.DbColumn("JE_SampleForwarded", VarChar, 1),
					new TestDbViewHelper.DbColumn("JE_SealBy", VarChar, 1),
					new TestDbViewHelper.DbColumn("JE_SealNo", VarChar, 100),
					new TestDbViewHelper.DbColumn("JE_StuffingAt", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_SupervisingOfficerDesignation", VarChar, 30),
					new TestDbViewHelper.DbColumn("JE_SupervisingOfficerName", VarChar, 30),
					new TestDbViewHelper.DbColumn("JE_Verified", VarChar, 1)
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("INJobDeclaration doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "INJobDeclaration_Idx"));
		}
	}
}
