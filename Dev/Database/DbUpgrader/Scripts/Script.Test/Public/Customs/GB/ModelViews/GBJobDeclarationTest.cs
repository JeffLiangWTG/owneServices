using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.Testing.Public.Customs.EU.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.GB.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.GB.ModelViews.GBJobDeclaration))]
	class GBJobDeclarationTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"GBJobDeclaration",
				"JobDeclaration",
				new []
				{
					new TestDbViewHelper.DbColumn("JE_ApportionByWeight", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_ClaimEuSubsidy", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_DiscAmt", Decimal, -1, 12, 3),
					new TestDbViewHelper.DbColumn("JE_DiscPerc", Decimal, -1, 5, 2),
					new TestDbViewHelper.DbColumn("JE_EidrType", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_FrtChgAmt", Decimal, -1, 12, 3),
					new TestDbViewHelper.DbColumn("JE_HouseSplitReference", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_ImportClearanceStatusICS", VarChar, 2),
					new TestDbViewHelper.DbColumn("JE_InsAmt", Decimal, -1, 12, 3),
					new TestDbViewHelper.DbColumn("JE_IsGvmsPort", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_ManualCalc", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_NiGoodsAtRiskOfMovingToROI", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_NorthernIrelandMode", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_OSAirTransportAmount", Decimal, -1, 12, 3),
					new TestDbViewHelper.DbColumn("JE_OthChgAmt", Decimal, -1, 12, 3),
					new TestDbViewHelper.DbColumn("JE_RX_NKDisc", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_RX_NKFrtChg", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_RX_NKIns", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_RX_NKOthChg", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_RX_NKVATAdj", VarChar, 3),
					new TestDbViewHelper.DbColumn("JE_SuppDecDueDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JE_UsePostponedVatAccounting", Bit, -1),
					new TestDbViewHelper.DbColumn("JE_VATAdjAmt", Decimal, -1, 12, 3),
				}.Union(EUJobDeclarationTest.ExpectedColumns).ToArray()
			);
		}

		public void TestViewIndexes()
		{
			TestDbViewHelper.AssertViewIndexes(
				Db.Connection,
				"GBJobDeclaration_Idx",
				[
					new TestDbViewHelper.DbIndex("NR_UC__JE_ClusterKey_JE_PK", "JE_ClusterKey,JE_PK"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_ClaimEuSubsidy", "JE_ClaimEuSubsidy"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_EidrType", "JE_EidrType"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_NorthernIrelandMode", "JE_NorthernIrelandMode"),
					new TestDbViewHelper.DbIndex("NR_UX__JE_SuppDecDueDate", "JE_SuppDecDueDate"),
				]
			);
		}
	}
}
