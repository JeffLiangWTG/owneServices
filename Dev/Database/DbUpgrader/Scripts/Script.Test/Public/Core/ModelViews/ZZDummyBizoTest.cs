using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Core.ModelViews.ZZDummyBizo))]
	class ZZDummyBizoTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"ZZDummyBizo",
				"DummyBizo",
				new []
				{
					new TestDbViewHelper.DbColumn("Z0_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("Z0_AddInfoString35", VarChar, 35),
					new TestDbViewHelper.DbColumn("Z0_AddInfoString3", VarChar, 3),
					new TestDbViewHelper.DbColumn("Z0_AddInfoDecimal122", Decimal, -1, 12, 2),
					new TestDbViewHelper.DbColumn("Z0_AddInfoDecimal073", Decimal, -1, 7, 3),
					new TestDbViewHelper.DbColumn("Z0_AddInfoInt16", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("Z0_AddInfoInt32", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("Z0_AddInfoInt32NotNullable", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("Z0_AddInfoBool", Bit, -1),
					new TestDbViewHelper.DbColumn("Z0_AddInfoGuid", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("Z0_AddInfoDateTime", DateTime, -1),
					new TestDbViewHelper.DbColumn("Z0_AddInfoDate", Date, -1),
					new TestDbViewHelper.DbColumn("Z0_NAddInfoString35", NVarChar, 35),
					new TestDbViewHelper.DbColumn("Z0_NAddInfoString3", NVarChar, 3),
					new TestDbViewHelper.DbColumn("Z0_NAddInfoDecimal122", Decimal, -1, 12, 2),
					new TestDbViewHelper.DbColumn("Z0_NAddInfoDecimal073", Decimal, -1, 7, 3),
					new TestDbViewHelper.DbColumn("Z0_NAddInfoInt16", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("Z0_NAddInfoInt32", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("Z0_NAddInfoBool", Bit, -1),
					new TestDbViewHelper.DbColumn("Z0_NAddInfoGuid", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("Z0_NAddInfoDateTime", DateTime, -1),
					new TestDbViewHelper.DbColumn("Z0_NAddInfoDate", Date, -1),
					new TestDbViewHelper.DbColumn("Z0_IAddInfoString35", VarChar, 35),
					new TestDbViewHelper.DbColumn("Z0_IAddInfoDecimal122", Decimal, -1, 12, 2),
					new TestDbViewHelper.DbColumn("Z0_IAddInfoInt16", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("Z0_IAddInfoInt32", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("Z0_IAddInfoBool", Bit, -1),
					new TestDbViewHelper.DbColumn("Z0_IAddInfoGuid", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("Z0_IAddInfoDateTime", DateTime, -1),
					new TestDbViewHelper.DbColumn("Z0_IAddInfoDate", Date, -1),
					new TestDbViewHelper.DbColumn("Z0_INAddInfoString35", NVarChar, 35),
					new TestDbViewHelper.DbColumn("Z0_INAddInfoDecimal122", Decimal, -1, 12, 2),
					new TestDbViewHelper.DbColumn("Z0_INAddInfoInt16", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("Z0_INAddInfoInt32", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("Z0_INAddInfoBool", Bit, -1),
					new TestDbViewHelper.DbColumn("Z0_INAddInfoGuid", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("Z0_INAddInfoDateTime", DateTime, -1),
					new TestDbViewHelper.DbColumn("Z0_INAddInfoDate", Date, -1),
				}
			);
		}

		public void TestViewIndexes()
		{
			TestDbViewHelper.AssertViewIndexes(
				Db.Connection,
				"ZZDummyBizo_Idx",
				new[]
				{
					new TestDbViewHelper.DbIndex("NR_UC__Z0_PK", "Z0_PK"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_IAddInfoString35", "Z0_IAddInfoString35"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_IAddInfoDecimal122", "Z0_IAddInfoDecimal122"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_IAddInfoInt16", "Z0_IAddInfoInt16"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_IAddInfoInt32", "Z0_IAddInfoInt32"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_IAddInfoBool", "Z0_IAddInfoBool"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_IAddInfoGuid", "Z0_IAddInfoGuid"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_IAddInfoDateTime", "Z0_IAddInfoDateTime"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_IAddInfoDate", "Z0_IAddInfoDate"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_INAddInfoString35", "Z0_INAddInfoString35"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_INAddInfoDecimal122", "Z0_INAddInfoDecimal122"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_INAddInfoInt16", "Z0_INAddInfoInt16"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_INAddInfoInt32", "Z0_INAddInfoInt32"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_INAddInfoBool", "Z0_INAddInfoBool"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_INAddInfoGuid", "Z0_INAddInfoGuid"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_INAddInfoDateTime", "Z0_INAddInfoDateTime"),
					new TestDbViewHelper.DbIndex("NR_UX__Z0_INAddInfoDate", "Z0_INAddInfoDate"),
				}
			);
		}
	}
}
