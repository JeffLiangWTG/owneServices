using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.BR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.BR.ModelViews.BRJobComInvoiceHeader))]
	class BRJobComInvoiceHeaderTest : BaseModelViewScriptTest
	{
		protected override string ViewName => "BRJobComInvoiceHeader";

		protected override string UnderlyingTableName => "JobComInvoiceHeader";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("JZ_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("JZ_ClusterKey", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JZ_SupplierAuthorityIdentifier", VarChar, 35),
			new TestDbViewHelper.DbColumn("JZ_SupplierAuthorityVersion", VarChar, 8)
		};

		protected override bool HasIndexes => false;
	}
}
