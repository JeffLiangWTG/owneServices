using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.CN.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.CN.ModelViews.CNJobComInvoiceLine))]
	sealed class CNJobComInvoiceLineTest : BaseModelViewScriptTest
	{
		#region Overrides of BaseModelViewScriptTest

		protected override string ViewName => "CNJobComInvoiceLine";
		protected override string UnderlyingTableName => "JobComInvoiceLine";
		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("JI_PK", SqlDbType.UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("JI_ClusterKey", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JI_CIQEndUse", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("JI_CIQExpiryDate", SqlDbType.DateTime, -1),
			new TestDbViewHelper.DbColumn("JI_CIQOriginState", SqlDbType.VarChar, 6),
			new TestDbViewHelper.DbColumn("JI_CIQQualityGuaranteePeriod", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JI_CIQTariff", SqlDbType.VarChar, 13),
			new TestDbViewHelper.DbColumn("JI_DestinationDistrict", SqlDbType.VarChar, 5),
			new TestDbViewHelper.DbColumn("JI_DestinationRegion", SqlDbType.VarChar, 6),
			new TestDbViewHelper.DbColumn("JI_DutyMode", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JI_NameOfGoods", SqlDbType.NVarChar, 50),
			new TestDbViewHelper.DbColumn("JI_NameOfGoods2", SqlDbType.NVarChar, 50),
			new TestDbViewHelper.DbColumn("JI_NonDangerousChemicalFlag", SqlDbType.Bit, -1),
			new TestDbViewHelper.DbColumn("JI_OrigContainerFlag", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JI_OriginDistrict", SqlDbType.VarChar, 5),
			new TestDbViewHelper.DbColumn("JI_OriginRegion", SqlDbType.VarChar, 6),
			new TestDbViewHelper.DbColumn("JI_PackageTypeOfUNDG", SqlDbType.VarChar, 36),
			new TestDbViewHelper.DbColumn("JI_ProductManualNo", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JI_ProductManualNo2", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JI_ProductVersion", SqlDbType.VarChar, 32),
			new TestDbViewHelper.DbColumn("JI_TradeQuantity", SqlDbType.Decimal, -1, 19,5),
			new TestDbViewHelper.DbColumn("JI_TradeUnitQty", SqlDbType.VarChar, 3),
		};

		protected override bool HasIndexes => false;

		#endregion
	}
}
