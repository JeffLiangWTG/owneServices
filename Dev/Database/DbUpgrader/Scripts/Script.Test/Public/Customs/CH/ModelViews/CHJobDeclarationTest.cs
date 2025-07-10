using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.CH.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.CH.ModelViews.CHJobDeclaration))]
	sealed class CHJobDeclarationTest : BaseModelViewScriptTest
	{
		protected override string ViewName => "CHJobDeclaration";

		protected override string UnderlyingTableName => "JobDeclaration";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("JE_PK", SqlDbType.UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("JE_ClusterKey", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JE_AdditionalDecisionInfo", SqlDbType.Bit, -1),
			new TestDbViewHelper.DbColumn("JE_ClearanceLocation", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_DispatchCountryConfirmation", SqlDbType.Bit, -1),
			new TestDbViewHelper.DbColumn("JE_SpecificCircumstanceIndicator", SqlDbType.VarChar, 3),
			new TestDbViewHelper.DbColumn("JE_VATPaidBy", SqlDbType.VarChar, 3),
			new TestDbViewHelper.DbColumn("JE_VehicleType", SqlDbType.VarChar, 2)
		};

		protected override bool HasIndexes => false;
	}
}
