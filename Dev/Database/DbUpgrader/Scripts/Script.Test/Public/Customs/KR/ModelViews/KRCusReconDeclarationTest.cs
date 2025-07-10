using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.KR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR.ModelViews.KRCusReconDeclaration))]
	sealed class KRCusReconDeclarationTest : BaseModelViewScriptTest
	{
		protected override string ViewName => "KRCusReconDeclaration";

		protected override string UnderlyingTableName => "CusReconDeclaration";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("CRD_PK", SqlDbType.UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CRD_RefundCauseCode", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("CRD_RefundReasonCode", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("CRD_CustomsDivision", SqlDbType.VarChar, 2),
			new TestDbViewHelper.DbColumn("CRD_TaxOffice", SqlDbType.VarChar, 3),
		};

		protected override bool HasIndexes => false;
	}
}
