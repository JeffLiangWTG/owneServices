using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.KR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR.ModelViews.KRCusEntryHeader))]
	sealed class KRCusEntryHeaderTest : BaseModelViewScriptTest
	{
		#region Overrides of ModelViewScriptTestBase

		protected override string ViewName => "KRCusEntryHeader";

		protected override string UnderlyingTableName => "CusEntryHeader";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("CH_PK", SqlDbType.UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CH_ClusterKey", SqlDbType.Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CH_DutyPenaltyExemptionAmount", SqlDbType.Decimal, -1, 18, 0),
			new TestDbViewHelper.DbColumn("CH_HighestContainerNumber", SqlDbType.SmallInt, -1, 5, 0),
			new TestDbViewHelper.DbColumn("CH_HighestFTASequenceNumber", SqlDbType.SmallInt, -1, 5, 0),
			new TestDbViewHelper.DbColumn("CH_HighestTransportMeansNo", SqlDbType.SmallInt, -1, 5, 0),
		};

		protected override bool HasIndexes => false;

		#endregion
	}
}
