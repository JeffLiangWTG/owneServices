using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.CN.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.CN.ModelViews.CNJobComInvoiceHeader))]
	sealed class CNJobComInvoiceHeaderTest : BaseModelViewScriptTest
	{
		#region Overrides of ModelViewScriptTestBase

		protected override string ViewName => "CNJobComInvoiceHeader";

		protected override string UnderlyingTableName => "JobComInvoiceHeader";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("JZ_PK", SqlDbType.UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("JZ_ClusterKey", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JZ_PriceAffectConfirm", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JZ_PaymentOfRoyaltyConfirm", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JZ_SpecialRelationshipConfirm", SqlDbType.VarChar, 1)
		};

		protected override bool HasIndexes => false;

		#endregion
	}
}
