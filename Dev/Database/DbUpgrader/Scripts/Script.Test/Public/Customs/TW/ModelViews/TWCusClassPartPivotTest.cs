using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.TW.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.TW.ModelViews.TWCusClassPartPivot))]
	sealed class TWCusClassPartPivotTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"TWCusClassPartPivot",
				"CusClassPartPivot",
				new[]
				{
					new TestDbViewHelper.DbColumn("CI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CI_CarType", VarChar, 2),
					new TestDbViewHelper.DbColumn("CI_Transmission", VarChar, 1),
					new TestDbViewHelper.DbColumn("CI_EngineType", VarChar, 2),
					new TestDbViewHelper.DbColumn("CI_LHD", VarChar, 1),
					new TestDbViewHelper.DbColumn("CI_HasCatalystConverter", VarChar, 1),
					new TestDbViewHelper.DbColumn("CI_EquipmentPrintMode", VarChar, 3),
					new TestDbViewHelper.DbColumn("CI_CarCondition", VarChar, 1),
					new TestDbViewHelper.DbColumn("CI_ModelYear", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("CI_Displacement", VarChar, 9),
					new TestDbViewHelper.DbColumn("CI_NumberOfDoor", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("CI_Seats", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("CI_Cylinders", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("CI_Gears",  SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("CI_CustomsSupplierPartNo", VarChar, 30),
					new TestDbViewHelper.DbColumn("CI_CustomsOwnerPartNo", VarChar, 30),
					new TestDbViewHelper.DbColumn("CI_ModeOfStatistics", VarChar, 2),
					new TestDbViewHelper.DbColumn("CI_DutyTreatment", VarChar, 2),
					new TestDbViewHelper.DbColumn("CI_Price", Decimal, -1, 19, 6),
					new TestDbViewHelper.DbColumn("CI_PriceCurr", VarChar, 3),
					new TestDbViewHelper.DbColumn("CI_AlcoholPercentage", Decimal, -1, 6, 3),
					new TestDbViewHelper.DbColumn("CI_TariffAdditionalCode", VarChar, 5),
					new TestDbViewHelper.DbColumn("CI_Compositions", VarChar, 256),
					new TestDbViewHelper.DbColumn("CI_EPTDigit1", VarChar, 1),
					new TestDbViewHelper.DbColumn("CI_EPTDigit2", VarChar, 1),
					new TestDbViewHelper.DbColumn("CI_EPTDigit3", VarChar, 1),
					new TestDbViewHelper.DbColumn("CI_DeclGoodsDescMode", VarChar, 3),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("TWCusClassPartPivot doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "TWCusClassPartPivot_Idx"));
		}
	}
}
