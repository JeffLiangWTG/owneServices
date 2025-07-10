using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.BR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.BR.ModelViews.BRJobComInvoiceLine))]
	class BRJobComInvoiceLineTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"BRJobComInvoiceLine",
				"JobComInvoiceLine",
				new[]
				{
					new TestDbViewHelper.DbColumn("JI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_AgentCommissionPercentage", Decimal, -1, 5, 2),
					new TestDbViewHelper.DbColumn("JI_CargoPriority", VarChar, 4),
					new TestDbViewHelper.DbColumn("JI_CatalogAuthorityIdentifier", VarChar, 50),
					new TestDbViewHelper.DbColumn("JI_CatalogAuthorityVersion", VarChar, 8),
					new TestDbViewHelper.DbColumn("JI_ComplementaryNote", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_DigitalServiceDossier", VarChar, 17),
					new TestDbViewHelper.DbColumn("JI_ExportJustificationInfo", VarChar, 100),
					new TestDbViewHelper.DbColumn("JI_FinancedValue", Decimal, -1, 19, 4),
					new TestDbViewHelper.DbColumn("JI_FourthCPC", VarChar, 5),
					new TestDbViewHelper.DbColumn("JI_IntendedTermDays", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("JI_GoodsApplication", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_GoodsCondition", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_ICMSBaseValueReductionPercentage", Decimal, -1, 8, 5),
					new TestDbViewHelper.DbColumn("JI_ICMSFormula", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_ICMSRate", Decimal, -1, 4, 2),
					new TestDbViewHelper.DbColumn("JI_ICMSTotalAmountReductionPercentage", Decimal, -1, 5, 2),
					new TestDbViewHelper.DbColumn("JI_ManufacturerIndicator", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_ManufacturerAuthorityIdentifier", VarChar, 35),
					new TestDbViewHelper.DbColumn("JI_ManufacturerAuthorityVersion", VarChar, 8),
					new TestDbViewHelper.DbColumn("JI_NFeItemNumber", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_NFeLinePrice", Decimal, -1, 19, 4),
					new TestDbViewHelper.DbColumn("JI_NFeNumber", VarChar, 44),
					new TestDbViewHelper.DbColumn("JI_RequiresImportLicense", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_SecondCPC", VarChar, 5),
					new TestDbViewHelper.DbColumn("JI_TemporaryAdmissionReason", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_ThirdCPC", VarChar, 5),
					new TestDbViewHelper.DbColumn("JI_UsedMaterialManufactureYear", VarChar, 4),
					new TestDbViewHelper.DbColumn("JI_UsedMaterialOperationType", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_UsedMaterialRegime", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_UsedMaterialSerialNumber", VarChar, 20)
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("BRJobComInvoiceLine doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "BRJobComInvoiceLine_Idx"));
		}
	}
}
