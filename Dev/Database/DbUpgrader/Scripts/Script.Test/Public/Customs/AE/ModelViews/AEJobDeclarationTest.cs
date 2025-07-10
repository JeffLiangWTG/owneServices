using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.AE.ModelViews;

[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.AE.ModelViews.AEJobDeclaration))]
sealed class AEJobDeclarationTest : BaseModelViewScriptTest
{
	protected override string ViewName => "AEJobDeclaration";

	protected override string UnderlyingTableName => "JobDeclaration";

	protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
	{
		new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
		new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
		new TestDbViewHelper.DbColumn("JE_ClearanceLocation", VarChar, 2),
		new TestDbViewHelper.DbColumn("JE_ExitPoint", VarChar, 3),
		new TestDbViewHelper.DbColumn("JE_PlaceOfDischarge", VarChar, 1),
		new TestDbViewHelper.DbColumn("JE_TypeOfGoods", VarChar, 1)
	};

	protected override bool HasIndexes => false;
}
