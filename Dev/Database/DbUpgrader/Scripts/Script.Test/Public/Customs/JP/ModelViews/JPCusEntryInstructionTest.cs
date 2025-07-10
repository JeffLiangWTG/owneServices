using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.JP.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.JP.ModelViews.JPCusEntryInstruction))]
	sealed class JPCusEntryInstructionTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"JPCusEntryInstruction",
				"CusEntryInstruction",
				[
					new TestDbViewHelper.DbColumn("CEI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("CEI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CEI_LoadingConfirmationIsRequired", Bit, -1),
					new TestDbViewHelper.DbColumn("CEI_PreInspectedCargoType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_BeforePermitApplicationReason", VarChar, 2),
					new TestDbViewHelper.DbColumn("CEI_BillNumberType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_CommonControlNumber", VarChar, 35),
					new TestDbViewHelper.DbColumn("CEI_ContainerCount", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("CEI_CustomsInspectionCode", VarChar, 5),
					new TestDbViewHelper.DbColumn("CEI_TradeType", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_CargoType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_CDB01CargoType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_CDB01PermitNo", VarChar, 11),
					new TestDbViewHelper.DbColumn("CEI_ContentInspectionResult", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_DutyDrawback", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_DeclarationCargoType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_DeclarationCondition", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_ECRCargoType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_FoodHygieneCertificateType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_PlantProtectionCertificateType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_AnimalQuarantineCertificateType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_CommercialValueType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_CustomsOfficeForSpecialDeclarations", VarChar, 2),
					new TestDbViewHelper.DbColumn("CEI_CustomsOfficeDepartmentForSpecialDeclarations", VarChar, 2),
					new TestDbViewHelper.DbColumn("CEI_CargoQuantity", Decimal, -1,8,0),
					new TestDbViewHelper.DbColumn("CEI_CargoQuantityUnit", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_GoodsDescription", VarChar, 70),
					new TestDbViewHelper.DbColumn("CEI_GrossWeight", Decimal, -1,9,3),
					new TestDbViewHelper.DbColumn("CEI_GrossWeightUnit", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_TradeControlOrder", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_ApprovalCertificateCategory", VarChar, 2),
					new TestDbViewHelper.DbColumn("CEI_BondedLocationCode", VarChar, 5),
					new TestDbViewHelper.DbColumn("CEI_BondedLocationName", VarChar, 70),
					new TestDbViewHelper.DbColumn("CEI_SpecialCargoCode", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_ValueType", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_Volume", Decimal, -1,9,3),
					new TestDbViewHelper.DbColumn("CEI_VolumeUnit", VarChar, 3),
					new TestDbViewHelper.DbColumn("CEI_PreviousBillNumber", VarChar, 35),
					new TestDbViewHelper.DbColumn("CEI_RCRAction", VarChar, 1),
					new TestDbViewHelper.DbColumn("CEI_ViaLocation", VarChar, 5),
				]
			);
		}
	}
}
