using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.KR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR.ModelViews.KRHouseBill))]
	sealed class KRHouseBillTest : BaseModelViewScriptTest
	{
		protected override string ViewName => "KRHouseBill";

		protected override string UnderlyingTableName => "CusDecHouseBill";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("CU_PK", SqlDbType.UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CU_ClusterKey", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CU_BillSeqNo", SqlDbType.VarChar, 4),
			new TestDbViewHelper.DbColumn("CU_HBSplitDecInd", SqlDbType.VarChar, 1),
			new TestDbViewHelper.DbColumn("CU_HBSplitDecReasonCode", SqlDbType.VarChar, 1),
		};

		protected override bool HasIndexes => false;
	}
}
