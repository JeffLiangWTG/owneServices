using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.JP.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.JP.ModelViews.JPCusEntryLine))]
	sealed class JPCusEntryLineTest : BaseModelViewScriptTest
	{
		#region Overrides of ModelViewScriptTestBase

		protected override string ViewName => "JPCusEntryLine";

		protected override string UnderlyingTableName => "CusEntryLine";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("CL_PK", SqlDbType.UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CL_ClusterKey", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CL_MergedCustomsValue", SqlDbType.Decimal, -1, 13, 0),
			new TestDbViewHelper.DbColumn("CL_ParentLineNumber", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("CL_PriceCheck", SqlDbType.VarChar, 1),
		};

		protected override bool HasIndexes => false;

		#endregion
	}
}
