using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.TW.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.TW.ModelViews.TWCusEntryInstruction))]
	class TWCusEntryInstructionTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"TWCusEntryInstruction",
				"CusEntryInstruction",
				new[]
				{
					new TestDbViewHelper.DbColumn("CEI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CEI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CEI_BillOfMaterials", Bit, -1),
					new TestDbViewHelper.DbColumn("CEI_BOMPageCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CEI_BoxNumber", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_CustomsOffice", VarChar, 2),
					new TestDbViewHelper.DbColumn("CEI_DaysOfDelayedDeclaration", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CEI_DutyRefund", Bit, -1),
					new TestDbViewHelper.DbColumn("CEI_ExamMode", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_GoodsLocation", VarChar, 8),
					new TestDbViewHelper.DbColumn("CEI_IsCoPackaged", Bit, -1),
					new TestDbViewHelper.DbColumn("CEI_PackageDescription", VarChar, 35),
					new TestDbViewHelper.DbColumn("CEI_PrintDutyMemo", Bit, -1),
					new TestDbViewHelper.DbColumn("CEI_ReasonForDuty", VarChar, 2),
					new TestDbViewHelper.DbColumn("CEI_RORPaymentMethod", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_WaiverOfExemption", Bit, -1),
					new TestDbViewHelper.DbColumn("CEI_WHSMonth", VarChar, 2),
					new TestDbViewHelper.DbColumn("CEI_WHSTradeReferenceNo", VarChar, 14),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("TWCusEntryInstruction doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "TWCusEntryInstruction_Idx"));
		}
	}
}
