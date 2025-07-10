using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.TW.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.TW.ModelViews.TWJobComInvoiceLine))]
	sealed class TWJobComInvoiceLineTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"TWJobComInvoiceLine",
				"JobComInvoiceLine",
				new[]
				{
					new TestDbViewHelper.DbColumn("JI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_AdditionalDutyRate", Decimal, -1, 10, 5),
					new TestDbViewHelper.DbColumn("JI_AlcoholAge", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_AlcoholCountryRegion", NVarChar, 100),
					new TestDbViewHelper.DbColumn("JI_AlcoholEndOfShelfLife", DateTime, -1 ),
					new TestDbViewHelper.DbColumn("JI_AlcoholPercentage", Decimal, -1, 6, 3),
					new TestDbViewHelper.DbColumn("JI_AlcoholYear", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_AlteredLotNoAmt", Decimal, -1, 15, 4),
					new TestDbViewHelper.DbColumn("JI_AnimalAgeMonth", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_AnimalAgeYear", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_AnimalFemaleQty", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_AnimalMaleQty", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_AntiDumpingDutyRate", Decimal, -1, 10, 5),
					new TestDbViewHelper.DbColumn("JI_BarCode", VarChar, 13),
					new TestDbViewHelper.DbColumn("JI_BondedGoodsCode", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_BottledDate", DateTime, -1 ),
					new TestDbViewHelper.DbColumn("JI_CarCondition", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_CarType", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_Compositions", NVarChar, 256),
					new TestDbViewHelper.DbColumn("JI_CountervailingDutyRate", Decimal, -1, 10, 5),
					new TestDbViewHelper.DbColumn("JI_CustomPermitUQ", VarChar, 32),
					new TestDbViewHelper.DbColumn("JI_CustomsOwnerPartNo", VarChar, 30),
					new TestDbViewHelper.DbColumn("JI_CustomsSupplierPartNo", VarChar, 30),
					new TestDbViewHelper.DbColumn("JI_CusValueConvRatio", Decimal, -1, 16, 4),
					new TestDbViewHelper.DbColumn("JI_CVAfterRecon", Decimal, -1, 19, 0),
					new TestDbViewHelper.DbColumn("JI_Cylinders", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("JI_Displacement", VarChar, 9),
					new TestDbViewHelper.DbColumn("JI_DtyPymntMthd", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_EngineType", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_EnteredUnitPrice", Decimal, -1, 19, 6),
					new TestDbViewHelper.DbColumn("JI_EPTDigit1", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_EPTDigit2", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_EPTDigit3", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_EquipmentPrintMode", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_ExpirationDate", DateTime, -1 ),
					new TestDbViewHelper.DbColumn("JI_Gears", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("JI_GoodsType", VarChar, 5),
					new TestDbViewHelper.DbColumn("JI_Group", NVarChar, 512),
					new TestDbViewHelper.DbColumn("JI_HasCatalystConverter", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_IMPTariff", VarChar, 18),
					new TestDbViewHelper.DbColumn("JI_InnerPackDescription", NVarChar, 200),
					new TestDbViewHelper.DbColumn("JI_InnerPackingMaterial", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_InnerPackType", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_LHD", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_ManufacturerRelationship", VarChar, 4),
					new TestDbViewHelper.DbColumn("JI_MicrochipID", VarChar, 18),
					new TestDbViewHelper.DbColumn("JI_ModelYear", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("JI_NewOwnerPartNo", VarChar, 35),
					new TestDbViewHelper.DbColumn("JI_NewPartAttribute1", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_NewPartAttribute2", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_NewPartAttribute3", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_NewSerialNumber", VarChar, 50),
					new TestDbViewHelper.DbColumn("JI_NoOriginalLotNoAmt", Decimal, -1, 15, 4),
					new TestDbViewHelper.DbColumn("JI_NumberOfDoor", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("JI_OriginCriteria", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_OwnerProduct", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JI_PermitQty", Decimal, -1, 19, 6),
					new TestDbViewHelper.DbColumn("JI_PermitUnitPrice", Decimal, -1, 19, 6),
					new TestDbViewHelper.DbColumn("JI_PermitUQ", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_PHValue", VarChar, 4),
					new TestDbViewHelper.DbColumn("JI_ProductGrade", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_ProductThickness", VarChar, 10),
					new TestDbViewHelper.DbColumn("JI_PTCriteria", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_PTCriteria2", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_QuarantineFeatures", NVarChar, 40),
					new TestDbViewHelper.DbColumn("JI_QuarantineTreatment", NVarChar, 120),
					new TestDbViewHelper.DbColumn("JI_RAPCurr", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_RAPPrice", Decimal, -1, 18, 6),
					new TestDbViewHelper.DbColumn("JI_RemovedLotNoAmt", Decimal, -1, 15, 4),
					new TestDbViewHelper.DbColumn("JI_RetaliatoryDutyRate", Decimal, -1, 10, 5),
					new TestDbViewHelper.DbColumn("JI_Seats", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("JI_SterilizationValue", VarChar, 4),
					new TestDbViewHelper.DbColumn("JI_TariffAdditionalCode", VarChar, 5),
					new TestDbViewHelper.DbColumn("JI_TariffExtensionCode", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_TariffPrintLength", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_TextileWidth", Decimal, -1, 18, 6),
					new TestDbViewHelper.DbColumn("JI_TextileWidthUQ", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_TpfPymntMthd", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_Transmission", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_UseOneTenthCV", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_VaccinationTypeDate", NVarChar, 60),
					new TestDbViewHelper.DbColumn("JI_VatPymntMthd", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_PackagingQTY", Decimal, -1, 8, 0),
					new TestDbViewHelper.DbColumn("JI_PackagingUQ", VarChar, 3),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("TWJobComInvoiceLine doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "TWJobComInvoiceLine_Idx"));
		}
	}
}
