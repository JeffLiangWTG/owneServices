using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.KR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR.ModelViews.KRCusEntryLine))]
	sealed class KRCusEntryLineTest : BaseModelViewScriptTest
	{
		#region Overrides of ModelViewScriptTestBase

		protected override string ViewName => "KRCusEntryLine";

		protected override string UnderlyingTableName => "CusEntryLine";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("CL_PK", SqlDbType.UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CL_ClusterKey", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CL_FTASequenceNumber", SqlDbType.SmallInt, -1, 5, 0),
			new TestDbViewHelper.DbColumn("CL_ValueExemptForVAT", SqlDbType.Decimal, -1, 18, 0),
			new TestDbViewHelper.DbColumn("CL_DutyReductionAmount", SqlDbType.Decimal, -1, 18, 0),
			new TestDbViewHelper.DbColumn("CL_IsGoldOrItsProduct", SqlDbType.Bit, -1),
		};

		protected override bool HasIndexes => false;

		#endregion
	}
}
