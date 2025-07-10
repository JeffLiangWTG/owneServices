using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.TW.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.TW.ModelViews.TWCusInBondContainer))]
	sealed class TWCusInBondContainerTest : BaseModelViewScriptTest
	{
		#region Overrides of ModelViewScriptTestBase

		protected override string ViewName => "TWCusInBondContainer";

		protected override string UnderlyingTableName => "CusInBondContainer";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("BC_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("BC_IsPart", Bit, -1),
		};

		protected override bool HasIndexes => false;

		#endregion
	}
}
