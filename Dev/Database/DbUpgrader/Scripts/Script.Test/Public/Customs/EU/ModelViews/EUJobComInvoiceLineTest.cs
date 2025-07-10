using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.EU.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.EU.ModelViews.EUJobComInvoiceLine))]
	class EUJobComInvoiceLineTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"EUJobComInvoiceLine",
				"JobComInvoiceLine",
				new[]
				{
					new TestDbViewHelper.DbColumn("JI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_AIEMType", VarChar, 6),
					new TestDbViewHelper.DbColumn("JI_CommercialPaymentAmount", Decimal, -1, 12, 3),
					new TestDbViewHelper.DbColumn("JI_CommercialPaymentCode", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_CommercialPaymentDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JI_CommercialPaymentNumber", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_CommercialReference", VarChar, 70),
					new TestDbViewHelper.DbColumn("JI_CountryOfDestination", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_CountryOfDispatch", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_CountryOfSupply", VarChar, 4),
					new TestDbViewHelper.DbColumn("JI_CusNumber", VarChar, 9),
					new TestDbViewHelper.DbColumn("JI_EconomicConditions", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_EntryExitPurposeCode", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_EntryExitPurposeDetail", VarChar, 100),
					new TestDbViewHelper.DbColumn("JI_ExcessStock", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_ExciseCode", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_ExciseExemption", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_ExportUnionAdditionalTariffCode", VarChar, 15),
					new TestDbViewHelper.DbColumn("JI_ExportUnionDeferredInstallment", VarChar, 250),
					new TestDbViewHelper.DbColumn("JI_ExportUnionEcological", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_ExportUnionPackCode", VarChar, 5),
					new TestDbViewHelper.DbColumn("JI_ExportUnionProductionYear", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("JI_ExportUnionThreadCode", VarChar, 5),
					new TestDbViewHelper.DbColumn("JI_FecChallengeDST", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_FecDST", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_GlobalWarmingPotential", Decimal, -1, 12, 2),
					new TestDbViewHelper.DbColumn("JI_GoodsCategory", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_HadErrorInLastResponse", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_HasNonRecycledPlastics", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_IdentificationMeansType", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_InwardProcessingLicenseLineNumber", VarChar, 14),
					new TestDbViewHelper.DbColumn("JI_IsMainPack", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_IsREADirectConsumption", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_MethodOfPayment", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_MethodOfPayment2", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_Meursing", VarChar, 4),
					new TestDbViewHelper.DbColumn("JI_PortTaxRate", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_PriceType", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_PrincipalsRepresentativeName", VarChar, 50),
					new TestDbViewHelper.DbColumn("JI_ProcessingDescription", VarChar, 300),
					new TestDbViewHelper.DbColumn("JI_QuotaQty", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_QuotaUQ", VarChar, 4),
					new TestDbViewHelper.DbColumn("JI_REAProductCode", VarChar, 4),
					new TestDbViewHelper.DbColumn("JI_RegionOfDestination", VarChar, 10),
					new TestDbViewHelper.DbColumn("JI_RelatedIndicator2", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_RelatedIndicator3", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_RelatedIndicator4", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_ReturningGoodsReasonCode", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_ReturningGoodsReasonDetail", VarChar, 100),
					new TestDbViewHelper.DbColumn("JI_ReturnToOrigin", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_RL_NKPrincipalsRepresentativeCity", VarChar, 5),
					new TestDbViewHelper.DbColumn("JI_RW_NKBorderTradeStateCode", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_SecondaryTreatedProduct", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_SecondQuota", VarChar, 15),
					new TestDbViewHelper.DbColumn("JI_StatisticalValue", Decimal, -1, 13, 2),
					new TestDbViewHelper.DbColumn("JI_StatisticalValueManualOverride", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_SteelType", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_T2LItemNumber", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_TotalRetailPrice", Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("JI_TransNature", VarChar, 2),
					new TestDbViewHelper.DbColumn("JI_UCRReference", VarChar, 35),
					new TestDbViewHelper.DbColumn("JI_UsedGoodsCode", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_ValueAdjustmentCode", VarChar, 1),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("EUJobComInvoiceLine doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "EUJobComInvoiceLine_Idx"));
		}
	}
}
