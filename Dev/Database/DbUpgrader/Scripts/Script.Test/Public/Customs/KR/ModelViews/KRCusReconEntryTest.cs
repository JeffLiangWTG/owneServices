using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.KR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR.ModelViews.KRCusReconEntry))]
	sealed class KRCusReconEntryTest : BaseModelViewScriptTest
	{
		protected override string ViewName => "KRCusReconEntry";

		protected override string UnderlyingTableName => "CusReconEntry";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("CRE_PK", SqlDbType.UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CRE_Amendment5WNVersionNumber", SqlDbType.SmallInt, -1, 5, 0),
			new TestDbViewHelper.DbColumn("CRE_CustomsBillNumber", SqlDbType.VarChar, 15),
			new TestDbViewHelper.DbColumn("CRE_SequenceNumber", SqlDbType.SmallInt, -1, 5, 0),
		};

		protected override bool HasIndexes => false;
	}
}
