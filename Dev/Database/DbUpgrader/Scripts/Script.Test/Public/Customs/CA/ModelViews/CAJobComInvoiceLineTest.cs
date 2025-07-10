using CargoWise.Data;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.CA.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA.ModelViews.CAJobComInvoiceLine))]
	internal class CAJobComInvoiceLineTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"CAJobComInvoiceLine",
				"JobComInvoiceLine",
				new[]
				{
					new TestDbViewHelper.DbColumn("JI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_99TariffCode" , VarChar, 4),
					new TestDbViewHelper.DbColumn("JI_ADJCode" , VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_ADJValue" , Decimal, -1, 20, 5),
					new TestDbViewHelper.DbColumn("JI_AirsCode" , VarChar, 6),
					new TestDbViewHelper.DbColumn("JI_AMMVPerUnit" , Decimal, -1, 19, 4),
					new TestDbViewHelper.DbColumn("JI_AMMVPercentage" , Decimal, -1, 9, 5),
					new TestDbViewHelper.DbColumn("JI_ApplyLuxuryTax", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_AuthorityNumber" , VarChar, 16),
					new TestDbViewHelper.DbColumn("JI_B3SubHeaderNumber" , Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_CalculationMethod" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_CasualImportCommodity" , VarChar, 25),
					new TestDbViewHelper.DbColumn("JI_CasualImportDestinationProvince" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_CFIACountryOfSource" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_CFIARegionOfSource" , VarChar, 70),
					new TestDbViewHelper.DbColumn("JI_CFIAStateOfSource" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_CFIAUSStateOfOrigin" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_CompliantCompletion", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_CompliantImportDate", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_ConveyanceIdentificationNumber" , VarChar, 1024),
					new TestDbViewHelper.DbColumn("JI_CustomsValue" , Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("JI_CustomsValueOvr", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_CVforCurrConv" , Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("JI_CVforCurrConvOvr", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_DestinationProvince" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_EndUse" , VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_ExpiryDate" , DateTime, -1),
					new TestDbViewHelper.DbColumn("JI_IIDRegion" , VarChar, 70),
					new TestDbViewHelper.DbColumn("JI_ImportReasonCode" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_IsAutoDummyHSCodeCasualImportLine", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_IsCasualImport", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_IsExempt", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_MiscID" , VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_Model" , VarChar, 25),
					new TestDbViewHelper.DbColumn("JI_ModelNumber" , VarChar, 25),
					new TestDbViewHelper.DbColumn("JI_PageNumber" , Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_PageRelativeLineNumber" , Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_ProductionDate" , DateTime, -1),
					new TestDbViewHelper.DbColumn("JI_RemissionType" , VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_RequirementID" , VarChar, 8),
					new TestDbViewHelper.DbColumn("JI_RequirementVer" , VarChar, 4),
					new TestDbViewHelper.DbColumn("JI_RN_NKCFIAOrigin" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_RN_NKExport" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_TIIN" , VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_TreatmentCode" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_TRSNumber" , VarChar, 13),
					new TestDbViewHelper.DbColumn("JI_TypeSize" , VarChar, 15),
					new TestDbViewHelper.DbColumn("JI_USStateOfExport" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_ValueForDutyCode" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_ValueForTax" , Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("JI_OriginalLineNo" , VarChar, 8),
					new TestDbViewHelper.DbColumn("JI_IsAccountForLine", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_IsSeeded", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_OGDStatus" , VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_PreviousB3LineNo" , Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_PreviousB3SubHeaderNo" , Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_PreviousLineNo" , Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_SIMADumpingNum" , VarChar, 30),
					new TestDbViewHelper.DbColumn("JI_CFIAInd" , VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_CNSCInd" , VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_DFOInd" , VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_ECCCInd" , VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_GACInd" , VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_HCInd" , VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_NRCanInd" , VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_PHACInd" , VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_TCInd" , VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_ManufactureDate" , DateTime, -1),
					new TestDbViewHelper.DbColumn("JI_TradeName" , VarChar, 50),
					new TestDbViewHelper.DbColumn("JI_PackagingQuantity" , Decimal, -1, 8, 0),
					new TestDbViewHelper.DbColumn("JI_RN_NKSource" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_StateOfSource" , VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_VINNumber" , VarChar, 17),
					new TestDbViewHelper.DbColumn("JI_ModelYear" , VarChar, 4),
					new TestDbViewHelper.DbColumn("JI_SIMAmount" , Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("JI_SIMExemptCode" , VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_SIMRateDescription" , VarChar, 1024),
					new TestDbViewHelper.DbColumn("JI_CPTAmount" , Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("JI_CPTExemptCode" , VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_CPTRateDescription" , VarChar, 1024),
					new TestDbViewHelper.DbColumn("JI_CTAAmount" , Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("JI_CTAExemptCode" , VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_CTARateDescription" , VarChar, 1024),
					new TestDbViewHelper.DbColumn("JI_DTYAmount" , Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("JI_DTYExemptCode" , VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_DTYRateDescription" , VarChar, 1024),
					new TestDbViewHelper.DbColumn("JI_GSTAmount" , Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("JI_GSTExemptCode" , VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_GSTRateDescription" , VarChar, 1024),
					new TestDbViewHelper.DbColumn("JI_EXSAmount" , Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("JI_EXSExemptCode" , VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_EXSRateDescription" , VarChar, 1024),
				}
			);
		}

		public void TestViewIndexes()
		{
			TestDbViewHelper.AssertViewIndexes(
				Db.Connection,
				"CAJobComInvoiceLine_Idx",
				new[]
				{
					new TestDbViewHelper.DbIndex("NR_UC__JI_ClusterKey_JI_PK", "JI_ClusterKey,JI_PK"),
					new TestDbViewHelper.DbIndex("NR_UX__JI_B3SubHeaderNumber", "JI_B3SubHeaderNumber"),
					new TestDbViewHelper.DbIndex("NR_UX__JI_CalculationMethod", "JI_CalculationMethod"),
					new TestDbViewHelper.DbIndex("NR_UX__JI_PageNumber", "JI_PageNumber"),
					new TestDbViewHelper.DbIndex("NR_UX__JI_PageRelativeLineNumber", "JI_PageRelativeLineNumber"),
					new TestDbViewHelper.DbIndex("NR_UX__JI_TreatmentCode", "JI_TreatmentCode"),
					new TestDbViewHelper.DbIndex("NR_UX__JI_RN_NKSource", "JI_RN_NKSource"),
					new TestDbViewHelper.DbIndex("NR_UX__JI_StateOfSource", "JI_StateOfSource")
				}
			);
		}
	}
}
