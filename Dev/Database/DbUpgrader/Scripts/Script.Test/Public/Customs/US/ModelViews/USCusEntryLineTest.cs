using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.US.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.US.ModelViews.USCusEntryLine))]
	class USCusEntryLineTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"USCusEntryLine",
				"CusEntryLine",
				new []
				{
					new TestDbViewHelper.DbColumn("CL_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CL_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CL_CL_ParentLine", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CL_HasMPF", Bit, -1),
					new TestDbViewHelper.DbColumn("CL_IJAccepted", Bit, -1),
					new TestDbViewHelper.DbColumn("CL_SupCustomsValue", Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("CL_SupLine", Bit, -1),
					new TestDbViewHelper.DbColumn("CL_DutyRateDesc", VarChar, 50),
					new TestDbViewHelper.DbColumn("CL_ChildLineNum", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("CL_CWOs", VarChar, 30),
					new TestDbViewHelper.DbColumn("CL_SupAdditionalLine", Bit, -1),
					new TestDbViewHelper.DbColumn("CL_SupAdditionalLine2", Bit, -1),
					new TestDbViewHelper.DbColumn("CL_SupAdditionalLine3", Bit, -1),
					new TestDbViewHelper.DbColumn("CL_SupAdditionalLine4", Bit, -1),
					new TestDbViewHelper.DbColumn("CL_SupAdditionalLine5", Bit, -1)
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("USCusEntryLine doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "USCusEntryLine_Idx"));
		}
	}
}
