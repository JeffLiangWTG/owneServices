using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.JP.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.JP.ModelViews.JPCusClassPartPivot))]
	sealed class JPCusClassPartPivotTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"JPCusClassPartPivot",
				"CusClassPartPivot",
				new[]
				{
					new TestDbViewHelper.DbColumn("CI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CI_AdvanceRulingOnClassification", VarChar, 9),
					new TestDbViewHelper.DbColumn("CI_AdvanceRulingOnOrigin", VarChar, 7),
					new TestDbViewHelper.DbColumn("CI_DomesticConsumptionTaxExemptionCode", VarChar, 1),
					new TestDbViewHelper.DbColumn("CI_DomesticConsumptionTaxExemptionIsPartial", Bit, -1),
					new TestDbViewHelper.DbColumn("CI_DutyReductionAmount", Decimal, -1, 11, 0),
					new TestDbViewHelper.DbColumn("CI_DutyReductionExemptionRefundCode", VarChar, 5),
					new TestDbViewHelper.DbColumn("CI_FEFTAArticle48", VarChar, 1),
					new TestDbViewHelper.DbColumn("CI_StorageType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CI_TradeControlOrderAppendix", VarChar, 5),
				}
			);
		}
	}
}
