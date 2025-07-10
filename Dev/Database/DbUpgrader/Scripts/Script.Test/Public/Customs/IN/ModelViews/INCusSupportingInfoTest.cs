using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.IN.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.IN.ModelViews.INCusSupportingInfo))]
	class INCusSupportingInfoTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"INCusSupportingInfo",
				"CusSupportingInfo",
				new[]
				{
					new TestDbViewHelper.DbColumn("CSI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CSI_BEDutyPaid", Decimal, -1, 16, 6),
					new TestDbViewHelper.DbColumn("CSI_BEDutyPaymentDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("CSI_BEItemUsed", VarChar, 1),
					new TestDbViewHelper.DbColumn("CSI_CommissionerPermission", VarChar, 1),
					new TestDbViewHelper.DbColumn("CSI_ControlLocation", VarChar, 17),
					new TestDbViewHelper.DbColumn("CSI_InputCredit", VarChar, 1),
					new TestDbViewHelper.DbColumn("CSI_ManualBE", VarChar, 1),
					new TestDbViewHelper.DbColumn("CSI_ModvatAvailed", VarChar, 1),
					new TestDbViewHelper.DbColumn("CSI_ModvatRepaid", VarChar, 1),
					new TestDbViewHelper.DbColumn("CSI_PersonalUsed", VarChar, 1)
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("INCusSupportingInfo doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "INCusSupportingInfo_Idx"));
		}
	}
}
