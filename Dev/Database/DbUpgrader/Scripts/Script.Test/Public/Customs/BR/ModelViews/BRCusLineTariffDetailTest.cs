using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.BR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.BR.ModelViews.BRCusLineTariffDetail))]
	class BRCusLineTariffDetailTest : BaseModelViewScriptTest
	{
		protected override string ViewName => "BRCusLineTariffDetail";

		protected override string UnderlyingTableName => "CusLineTariffDetail";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("BZ_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("BZ_LegalActSubject", NVarChar, 1)
		};

		protected override bool HasIndexes => false;
	}
}
