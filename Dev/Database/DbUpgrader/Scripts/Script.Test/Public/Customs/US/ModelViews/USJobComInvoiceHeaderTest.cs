using CargoWise.Data;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.US.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.US.ModelViews.USJobComInvoiceHeader))]
	class USJobComInvoiceHeaderTest : DbCreateScriptTest
	{
		public void TestViewColumns()
		{
			TestDbViewHelper.AssertViewColumns(
				Db.Connection,
				"USJobComInvoiceHeader",
				"JobComInvoiceHeader",
				new []
				{
					new TestDbViewHelper.DbColumn("JZ_PK", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JZ_ClusterKey", Int, -1, 10, 0),
					new TestDbViewHelper.DbColumn("JZ_AESOriginIndicator", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_CH_ReconEntry", UniqueIdentifier, -1),
					new TestDbViewHelper.DbColumn("JZ_DateOfExport", DateTime, -1),
					new TestDbViewHelper.DbColumn("JZ_DateOfExportFromCountryOfOrigin", DateTime, -1),
					new TestDbViewHelper.DbColumn("JZ_DDTCITARExemptionNo", VarChar, 12),
					new TestDbViewHelper.DbColumn("JZ_DDTCMilitaryEquipmentIndicator", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_DDTCPartyCertificationIndicator", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_DDTCRegistrationNo", VarChar, 6),
					new TestDbViewHelper.DbColumn("JZ_DDTCUSMLCategoryCode", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_DeductADDCVDDuty", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_DES", VarChar, 4),
					new TestDbViewHelper.DbColumn("JZ_DestinationState", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_ECCN", VarChar, 5),
					new TestDbViewHelper.DbColumn("JZ_ExportCode", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_FDAContactEmail", VarChar, 254),
					new TestDbViewHelper.DbColumn("JZ_FDAContactName", VarChar, 101),
					new TestDbViewHelper.DbColumn("JZ_FDAContactPhoneNo", VarChar, 15),
					new TestDbViewHelper.DbColumn("JZ_FirstSale", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_ForeignTradeZone", VarChar, 9),
					new TestDbViewHelper.DbColumn("JZ_FSISSignDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JZ_FWSSignDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JZ_GenAIIForSup", Bit, -1),
					new TestDbViewHelper.DbColumn("JZ_HazardousCargo", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_ImportEntryNo", VarChar, 17),
					new TestDbViewHelper.DbColumn("JZ_InbondType", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_InvoiceType", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_IsLineGrouping", Bit, -1),
					new TestDbViewHelper.DbColumn("JZ_JurisdictionNumber", VarChar, 10),
					new TestDbViewHelper.DbColumn("JZ_LACEYACTSignDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JZ_LicenseNo", VarChar, 14),
					new TestDbViewHelper.DbColumn("JZ_LicenseType", VarChar, 3),
					new TestDbViewHelper.DbColumn("JZ_NHTSASignDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JZ_PaymentTerms", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_PaymentTermsDesc", VarChar, 70),
					new TestDbViewHelper.DbColumn("JZ_PrivilegedStatusDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JZ_PSTSignDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JZ_ReleaseEntryNumber", VarChar, 35),
					new TestDbViewHelper.DbColumn("JZ_RoutedTransaction", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_SplitShipmentDetail", VarChar, 20),
					new TestDbViewHelper.DbColumn("JZ_StateOfOrigin", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_TariffType", VarChar, 3),
					new TestDbViewHelper.DbColumn("JZ_TermsOfDeliveryLocation", VarChar, 35),
					new TestDbViewHelper.DbColumn("JZ_TermsOfDeliveryLocationIndicator", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_TermsOfDeliveryLocationQualifier", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_TransactionsRelated", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_TSCASignDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JZ_UC_NKCountryOfExport", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_UC_NKCountryOfOrigin", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_UltimateConsigneeType", VarChar, 1),
					new TestDbViewHelper.DbColumn("JZ_UltimateDestinationCountry", VarChar, 2),
					new TestDbViewHelper.DbColumn("JZ_ValueForDiscount", Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("JZ_ValueForForeignTax", Decimal, -1, 15, 2),
					new TestDbViewHelper.DbColumn("JZ_VNESignDate", DateTime, -1),
					new TestDbViewHelper.DbColumn("JZ_ZoneStatus", VarChar, 1),
				}
			);
		}

		public void TestViewIndexes()
		{
			AssertEquals("USJobComInvoiceHeader doesn't require any indexes.", false, DbObjectCreator.ViewExists(Db.Connection, "USJobComInvoiceHeader_Idx"));
		}
	}
}
