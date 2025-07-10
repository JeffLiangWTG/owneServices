using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.NZ.ModelViews;

[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.NZ.ModelViews.NZJobComInvoiceHeader))]
class NZJobComInvoiceHeaderTest : BaseModelViewScriptTest
{
	protected override string ViewName => "NZJobComInvoiceHeader";

	protected override string UnderlyingTableName => "JobComInvoiceHeader";

	protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns =>
	[
		new TestDbViewHelper.DbColumn("JZ_PK", SqlDbType.UniqueIdentifier, -1),
		new TestDbViewHelper.DbColumn("JZ_ClusterKey", SqlDbType.Int, -1, 10, 0),
		new TestDbViewHelper.DbColumn("JZ_IsGSTPrePaid", SqlDbType.VarChar, 1),
		new TestDbViewHelper.DbColumn("JZ_SupplierGSTNumber", SqlDbType.VarChar, 15),
		new TestDbViewHelper.DbColumn("JZ_IsZeroRatedDuty", SqlDbType.VarChar, 1),
		new TestDbViewHelper.DbColumn("JZ_IsZeroRatedExcise", SqlDbType.VarChar, 1),
		new TestDbViewHelper.DbColumn("JZ_IsZeroRatedLevies", SqlDbType.VarChar, 1),
		new TestDbViewHelper.DbColumn("JZ_IsZeroRatedGST", SqlDbType.VarChar, 1),
		new TestDbViewHelper.DbColumn("JZ_RelationshipIndicator", SqlDbType.VarChar, 1),
		new TestDbViewHelper.DbColumn("JZ_OriginRegion", SqlDbType.VarChar, 9),
		new TestDbViewHelper.DbColumn("JZ_QualifiesForPreferentialDuty", SqlDbType.VarChar, 1),
		new TestDbViewHelper.DbColumn("JZ_PreferentialCountryGroup", SqlDbType.VarChar, 3),
		new TestDbViewHelper.DbColumn("JZ_SupplierName", SqlDbType.VarChar, 100)
	];

	protected override bool HasIndexes => false;
}
