using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.ZA.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA.ModelViews.ZAJobComInvoiceLine))]
	class ZAJobComInvoiceLineTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"ZAJobComInvoiceLine",
				"JobComInvoiceLine",
				new []
				{
					new TestDbViewHelper.DbColumn("JI_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JI_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JI_ActualPrice", Decimal, -1, 19, 5),
					new TestDbViewHelper.DbColumn("JI_AdvancePaymentNo", VarChar, 35),
					new TestDbViewHelper.DbColumn("JI_CO2Emission", Decimal, -1, 7, 3),
					new TestDbViewHelper.DbColumn("JI_CommissionNumber", VarChar, 35),
					new TestDbViewHelper.DbColumn("JI_ConversionFactor", Decimal, -1, 19, 5),
					new TestDbViewHelper.DbColumn("JI_CustomsValueOverride", Decimal, -1, 19, 4),
					new TestDbViewHelper.DbColumn("JI_DiamondBeneficiaryLicense", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_DiamondDealerLicense", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_DiamondLevyValue", Decimal, -1, 12, 0),
					new TestDbViewHelper.DbColumn("JI_DiamondProducerRegistration", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_DiamondProducerExemption", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_ElectionsExemptionsLevy", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_ImportTariff", VarChar, 10),
					new TestDbViewHelper.DbColumn("JI_ImportCustomsValue", Decimal, -1, 12, 4),
					new TestDbViewHelper.DbColumn("JI_ImportCustomsQty", Decimal, -1, 19, 5),
					new TestDbViewHelper.DbColumn("JI_ImportCustomsQtyUQ", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_ImportCustomsQty2", Decimal, -1, 19, 5),
					new TestDbViewHelper.DbColumn("JI_ImportCustomsQty2UQ", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_ImportCustomsQty3", Decimal, -1, 19, 5),
					new TestDbViewHelper.DbColumn("JI_ImportCustomsQty3UQ", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_ImportDutyPaid", Decimal, -1, 12, 4),
					new TestDbViewHelper.DbColumn("JI_ImportVATPaid", Decimal, -1, 12, 4),
					new TestDbViewHelper.DbColumn("JI_KimberleyCertificate", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_NewOwnerPartAttrib1", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_NewOwnerPartAttrib2", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_NewOwnerPartAttrib3", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_NewOwnerPartNo", VarChar, 35),
					new TestDbViewHelper.DbColumn("JI_NewOwnerSerialNum", VarChar, 50),
					new TestDbViewHelper.DbColumn("JI_NewUsed", VarChar, 1),
					new TestDbViewHelper.DbColumn("JI_OP_NewOwnerProduct", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JI_PermitNumber", VarChar, 35),
					new TestDbViewHelper.DbColumn("JI_ROOCert", VarChar, 35),
					new TestDbViewHelper.DbColumn("JI_RX_NKCustomsValueCurrencyOverride", VarChar, 3),
					new TestDbViewHelper.DbColumn("JI_TakeUpInTradeStatistics", Bit, -1),
					new TestDbViewHelper.DbColumn("JI_TargetEntryLineNumber", SmallInt, -1, 5, 0),
					new TestDbViewHelper.DbColumn("JI_TemporaryBuyersPermit", VarChar, 20),
					new TestDbViewHelper.DbColumn("JI_TemporaryExportExemption", VarChar, 20),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("ZAJobComInvoiceLine doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "ZAJobComInvoiceLine_Idx"));
		}
	}
}
