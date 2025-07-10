using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.CH.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.CH.ModelViews.CHCusEntryInstruction))]
	sealed class CHCusEntryInstructionTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"CHCusEntryInstruction",
				"CusEntryInstruction",
				new[]
				{
					new TestDbViewHelper.DbColumn("CEI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CEI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CEI_DeclarationReason", VarChar, 2),
					new TestDbViewHelper.DbColumn("CEI_NextProcedure", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_PartialDelivery", Bit, -1),
					new TestDbViewHelper.DbColumn("CEI_TransportChargesMethodOfPayment", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_WarehouseType", VarChar, 1),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("CHCusEntryInstruction doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "CHCusEntryInstruction_Idx"));
		}
	}
}
