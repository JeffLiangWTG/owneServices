using System.Data;
using Enterprise.Build.Database.Script.Testing;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.NO.Testing
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.NO.ModelViews.NOJobComInvoiceLine))]
	sealed class NOJobComInvoiceLineTest : BaseModelViewScriptTest
	{
		protected override string ViewName => "NOJobComInvoiceLine";

		protected override string UnderlyingTableName => "JobComInvoiceLine";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns =>
		[
			new ("JI_PK", SqlDbType.UniqueIdentifier, -1),
			new ("JI_ClusterKey", SqlDbType.Int, -1, 10, 0),
			new ("JI_CustomsRateOverrideType", SqlDbType.VarChar, 5),
			new ("JI_CustomsRateOverrideValue", SqlDbType.Decimal, -1, 12, 4),
			new ("JI_CustomsTransportMode", SqlDbType.VarChar, 2),
			new ("JI_GoodsMarks", SqlDbType.VarChar, 28),
			new ("JI_MergeOverride", SqlDbType.VarChar, 10),
			new ("JI_PackageType", SqlDbType.VarChar, 1),
			new ("JI_ReducedCustomsFlag", SqlDbType.VarChar, 1),
			new ("JI_RTOValue", SqlDbType.Decimal, -1, 12, 4)
		];

		protected override bool HasIndexes => false;
	}
}
