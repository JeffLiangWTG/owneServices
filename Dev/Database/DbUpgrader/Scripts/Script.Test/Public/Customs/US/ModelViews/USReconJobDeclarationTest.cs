using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.US.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.US.ModelViews.USReconJobDeclaration))]
	class USReconJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"USReconJobDeclaration",
				"JobDeclaration",
				new []
				{
					new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JE_ALDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_ALDuty", Decimal, -1, 12, 4),
					new TestDbViewHelper.DbColumn("JE_ClaimDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_ClaimID", VarChar, 20),
					new TestDbViewHelper.DbColumn("JE_ClientBranchDesignation", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_Comment", VarChar, 150),
					new TestDbViewHelper.DbColumn("JE_DocProvidedDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_ENSAction", VarChar, 10),
					new TestDbViewHelper.DbColumn("JE_EntryFilerCode", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_EstimatedEntryDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_IsAggregate", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_IssueCode", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_Paid", VarChar, 1),
					new TestDbViewHelper.DbColumn("JE_PaymentDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_PaymentType", VarChar, 1),
					new TestDbViewHelper.DbColumn("JE_PreliminaryStatementPrintDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_SchDEntry", VarChar, 5),
					new TestDbViewHelper.DbColumn("JE_SuretyCode", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_TeamNo", VarChar, 3),
				}
			);
		}

		public void TestViewIndexes()
		{
			TestDbViewHelper.AssertViewIndexes(
				Db.Connection,
				"USReconJobDeclaration_IDx",
				new[]
				{
					new TestDbViewHelper.DbIndex("NR_UX__JE_ALDate", "JE_ALDate"),
					new TestDbViewHelper.DbIndex("NR_UC__JE_ClusterKey_JE_PK", "JE_ClusterKey,JE_PK"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_ENSAction", "JE_ENSAction"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_EntryFilerCode", "JE_EntryFilerCode"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_EstimatedEntryDate", "JE_EstimatedEntryDate"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_IsAggregate", "JE_IsAggregate"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_IssueCode", "JE_IssueCode"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_PaymentType", "JE_PaymentType"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_PreliminaryStatementPrintDate", "JE_PreliminaryStatementPrintDate"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_SchDEntry", "JE_SchDEntry"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_SuretyCode", "JE_SuretyCode"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_TeamNo", "JE_TeamNo"),
				}
			);
		}
	}
}
