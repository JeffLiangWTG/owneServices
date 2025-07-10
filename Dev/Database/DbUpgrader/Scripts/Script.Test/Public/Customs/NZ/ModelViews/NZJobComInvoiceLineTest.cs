using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.NZ.ModelViews;

[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.NZ.ModelViews.NZJobComInvoiceLine))]
sealed class NZJobComInvoiceLineTest : BaseModelViewScriptTest
{
	protected override string ViewName => "NZJobComInvoiceLine";

	protected override string UnderlyingTableName => "JobComInvoiceLine";

	protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns =>
	[
		new TestDbViewHelper.DbColumn("JI_PK", SqlDbType.UniqueIdentifier, -1),
		new TestDbViewHelper.DbColumn("JI_ClusterKey", SqlDbType.Int, -1, 10, 0),
		new TestDbViewHelper.DbColumn("JI_AntiDumpingDuty", SqlDbType.Decimal, -1, 10, 2),
		new TestDbViewHelper.DbColumn("JI_BrandName", SqlDbType.VarChar, 35),
		new TestDbViewHelper.DbColumn("JI_CommonName", SqlDbType.VarChar, 35),
		new TestDbViewHelper.DbColumn("JI_ConcessionCode", SqlDbType.VarChar, 7),
		new TestDbViewHelper.DbColumn("JI_CountervailingDuty", SqlDbType.Decimal, -1, 10, 2),
		new TestDbViewHelper.DbColumn("JI_DateMarking", SqlDbType.DateTime, -1),
		new TestDbViewHelper.DbColumn("JI_DepositRefund", SqlDbType.Decimal, -1, 10, 2),
		new TestDbViewHelper.DbColumn("JI_DTY", SqlDbType.Decimal, -1, 10, 2),
		new TestDbViewHelper.DbColumn("JI_DutyCredit", SqlDbType.Decimal, -1, 10, 2),
		new TestDbViewHelper.DbColumn("JI_EFG", SqlDbType.Decimal, -1, 10, 2),
		new TestDbViewHelper.DbColumn("JI_ENF", SqlDbType.Decimal, -1, 10, 2),
		new TestDbViewHelper.DbColumn("JI_ExciseDutyCredit", SqlDbType.Decimal, -1, 10, 2),
		new TestDbViewHelper.DbColumn("JI_GeneticallyModified", SqlDbType.Bit, -1),
		new TestDbViewHelper.DbColumn("JI_GST", SqlDbType.Decimal, -1, 10, 2),
		new TestDbViewHelper.DbColumn("JI_GSTCredit", SqlDbType.Decimal, -1, 10, 2),
		new TestDbViewHelper.DbColumn("JI_HadErrorInLastResponse", SqlDbType.Bit, -1),
		new TestDbViewHelper.DbColumn("JI_IntendedUse", SqlDbType.VarChar, 256),
		new TestDbViewHelper.DbColumn("JI_IntendedUseCode", SqlDbType.VarChar, 3),
		new TestDbViewHelper.DbColumn("JI_IsApportioned", SqlDbType.Bit, -1),
		new TestDbViewHelper.DbColumn("JI_IsZeroRatedDuty", SqlDbType.VarChar, 1),
		new TestDbViewHelper.DbColumn("JI_IsZeroRatedExcise", SqlDbType.VarChar, 1),
		new TestDbViewHelper.DbColumn("JI_IsZeroRatedGST", SqlDbType.VarChar, 1),
		new TestDbViewHelper.DbColumn("JI_IsZeroRatedLevies", SqlDbType.VarChar, 1),
		new TestDbViewHelper.DbColumn("JI_Itinerary", SqlDbType.VarChar, 30),
		new TestDbViewHelper.DbColumn("JI_JI_ParentLine", SqlDbType.UniqueIdentifier, -1),
		new TestDbViewHelper.DbColumn("JI_LevyForExport", SqlDbType.Decimal, -1, 10, 2),
		new TestDbViewHelper.DbColumn("JI_LevyForExportCode", SqlDbType.VarChar, 4),
		new TestDbViewHelper.DbColumn("JI_LotNumber", SqlDbType.VarChar, 80),
		new TestDbViewHelper.DbColumn("JI_LVY", SqlDbType.Decimal, -1, 10, 2),
		new TestDbViewHelper.DbColumn("JI_MAF_GoodsType", SqlDbType.VarChar, 3),
		new TestDbViewHelper.DbColumn("JI_MAF_MeasurementUQ", SqlDbType.VarChar, 3),
		new TestDbViewHelper.DbColumn("JI_MAF_MeasurementValue", SqlDbType.Int, -1, 10, 0),
		new TestDbViewHelper.DbColumn("JI_MAF_NewGoods", SqlDbType.VarChar, 1),
		new TestDbViewHelper.DbColumn("JI_MaxTemp", SqlDbType.Decimal, -1, 15, 3),
		new TestDbViewHelper.DbColumn("JI_MinTemp", SqlDbType.Decimal, -1, 15, 3),
		new TestDbViewHelper.DbColumn("JI_OriginRegion", SqlDbType.VarChar, 9),
		new TestDbViewHelper.DbColumn("JI_OtherInfos", SqlDbType.VarChar, 500),
		new TestDbViewHelper.DbColumn("JI_PartsOfClassification", SqlDbType.VarChar, 14),
		new TestDbViewHelper.DbColumn("JI_PermitCodes", SqlDbType.VarChar, 500),
		new TestDbViewHelper.DbColumn("JI_PreferentialCountryGroup", SqlDbType.VarChar, 3),
		new TestDbViewHelper.DbColumn("JI_ProhibitedCodes", SqlDbType.VarChar, 50),
		new TestDbViewHelper.DbColumn("JI_QualifiesForPreferentialDuty", SqlDbType.VarChar, 1),
		new TestDbViewHelper.DbColumn("JI_RegisteredName", SqlDbType.VarChar, 35),
		new TestDbViewHelper.DbColumn("JI_RN_NKCountryOfExport", SqlDbType.VarChar, 2),
		new TestDbViewHelper.DbColumn("JI_StorageTemp", SqlDbType.Decimal, -1, 15, 3),
		new TestDbViewHelper.DbColumn("JI_SupplementaryQty", SqlDbType.Decimal, -1, 10, 3),
		new TestDbViewHelper.DbColumn("JI_SupplementaryUQ", SqlDbType.VarChar, 3),
		new TestDbViewHelper.DbColumn("JI_TemperatureDetailsToBeSent", SqlDbType.Bit, -1),
		new TestDbViewHelper.DbColumn("JI_TradeName", SqlDbType.VarChar, 35),
		new TestDbViewHelper.DbColumn("JI_UsedGoods", SqlDbType.Bit, -1)
	];

	protected override bool HasIndexes => false;
}
