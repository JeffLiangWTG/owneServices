using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.NZ.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.NZ.ModelViews.NZCusContainer))]
	sealed class NZCusContainerTest : BaseModelViewScriptTest
	{
		protected override string ViewName => "NZCusContainer";

		protected override string UnderlyingTableName => "CusContainer";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns =>
		[
			new TestDbViewHelper.DbColumn("CO_PK", SqlDbType.UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CO_ClusterKey", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CO_MAF_ContainerType", SqlDbType.VarChar, 4),
			new TestDbViewHelper.DbColumn("CO_MPIApprovedSystemNumber", SqlDbType.VarChar, 10),
			new TestDbViewHelper.DbColumn("CO_SealingParty", SqlDbType.VarChar, 35)
		];

		protected override bool HasIndexes => false;
	}
}
