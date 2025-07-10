using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.BR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.BR.ModelViews.BRJobDeclaration))]
	class BRJobDeclarationTest : BaseModelViewScriptTest
	{
		protected override string ViewName => "BRJobDeclaration";

		protected override string UnderlyingTableName => "JobDeclaration";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JE_CargoArrivalDocumentNumber", VarChar, 15),
			new TestDbViewHelper.DbColumn("JE_CargoArrivalDocumentType", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_CargoArrivalDocumentUtilization", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_DispatchModality", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_IsMultimodal", Bit, -1),
			new TestDbViewHelper.DbColumn("JE_SpecialTransport", VarChar, 4)
		};

		protected override bool HasIndexes => false;
	}
}
