using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.NZ.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.NZ.ModelViews.NZCusClassification))]
	sealed class NZCusClassificationTest : BaseModelViewScriptTest
	{
		protected override string ViewName => "NZCusClassification";

		protected override string UnderlyingTableName => "CusClassification";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns =>
		[
			new TestDbViewHelper.DbColumn("CC_PK", SqlDbType.UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CC_ConcessionCode", SqlDbType.VarChar, 7),
			new TestDbViewHelper.DbColumn("CC_OtherInfos", SqlDbType.VarChar, 500),
			new TestDbViewHelper.DbColumn("CC_PartsOfClassification", SqlDbType.VarChar, 14),
			new TestDbViewHelper.DbColumn("CC_PermitCodes", SqlDbType.VarChar, 500),
			new TestDbViewHelper.DbColumn("CC_ProhibitedCodes", SqlDbType.VarChar, 50),
		];

		protected override bool HasIndexes => false;
	}
}
