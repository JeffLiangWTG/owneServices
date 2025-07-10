using System.Linq;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using Enterprise.Build.Database.Script.Testing.Public.Customs.EU.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.FR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.FR.ModelViews.FRCusEntryHeader))]
	sealed class FRCusEntryHeaderTest : BaseModelViewScriptTest
	{
		#region Overrides of ModelViewScriptTestBase

		protected override string ViewName => "FRCusEntryHeader";

		protected override string UnderlyingTableName => "CusEntryHeader";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("CH_ConfirmedGuaranteeAmount", Decimal, -1, 19, 4),
			new TestDbViewHelper.DbColumn("CH_TriggeringPointForValidation", VarChar, 3)
		}.Union(EUCusEntryHeaderTest.ExpectedColumns).ToArray();

		protected override TestDbViewHelper.DbColumn[] ExpectedIndexedViewColumns =>
		[
			new TestDbViewHelper.DbColumn("CH_ClusterKey", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CH_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CH_TriggeringPointForValidation", VarChar, 3)
		];

		protected override bool HasIndexes => true;

		#endregion
	}
}
