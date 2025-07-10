using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.NO.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.NO.ModelViews.NOCusClassPartPivot))]
	sealed class NOCusClassPartPivotTest : BaseModelViewScriptTest
	{
		protected override string ViewName => "NOCusClassPartPivot";

		protected override string UnderlyingTableName => "CusClassPartPivot";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns =>
		[
			new ("CI_PK", SqlDbType.UniqueIdentifier, -1),
			new ("CI_ReducedCustomsFlag", SqlDbType.VarChar, 1)
		];

		protected override bool HasIndexes => false;
	}
}
