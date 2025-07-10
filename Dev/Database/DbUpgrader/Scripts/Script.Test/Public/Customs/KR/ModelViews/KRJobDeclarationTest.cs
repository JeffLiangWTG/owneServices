using System.Data;
using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.KR.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR.ModelViews.KRJobDeclaration))]
	sealed class KRJobDeclarationTest : BaseModelViewScriptTest
	{
		protected override string ViewName => "KRJobDeclaration";

		protected override string UnderlyingTableName => "JobDeclaration";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JE_AuditorJobTitle", NVarChar, 30),
			new TestDbViewHelper.DbColumn("JE_AuditorName", NVarChar, 12),
			new TestDbViewHelper.DbColumn("JE_AuditorPhone", VarChar, 40),
			new TestDbViewHelper.DbColumn("JE_AuthorJobTitle", NVarChar, 30),
			new TestDbViewHelper.DbColumn("JE_AuthorName", NVarChar, 12),
			new TestDbViewHelper.DbColumn("JE_AuthorPhone", VarChar, 40),
			new TestDbViewHelper.DbColumn("JE_ContainerPackMode", VarChar, 3),
			new TestDbViewHelper.DbColumn("JE_CustomsDivision", VarChar, 2),
			new TestDbViewHelper.DbColumn("JE_DeclarationPlan", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_ExporterType", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_GoldTrade", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_GoodsCondition", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_IsBlanketDeclaration", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_LateDecPenaltyDateCode", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_LocationIDInBondedArea", VarChar, 15),
			new TestDbViewHelper.DbColumn("JE_MissedDecPenaltyRate", SqlDbType.SmallInt, -1, 5, 0),
			new TestDbViewHelper.DbColumn("JE_MRNType", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_NoOfCrew", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JE_OutOfHoursDecInd", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_ProcedureType", VarChar, 2),
			new TestDbViewHelper.DbColumn("JE_ReturnReason", VarChar, 2),
			new TestDbViewHelper.DbColumn("JE_ReturnType", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_SimpleDRWApp", VarChar, 2),
			new TestDbViewHelper.DbColumn("JE_TaxOffice", VarChar, 3),
			new TestDbViewHelper.DbColumn("JE_TradeIDWithKP", VarChar, 2),
			new TestDbViewHelper.DbColumn("JE_TradeIndicatorWithKP", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_TradeType", VarChar, 3),
			new TestDbViewHelper.DbColumn("JE_TransshipmentDate", DateTime, -1),
			new TestDbViewHelper.DbColumn("JE_TransshipmentPort", NVarChar, 5),
			new TestDbViewHelper.DbColumn("JE_VoyageDuration", Int, -1, 10, 0)
		};

		protected override bool HasIndexes => false;
	}
}
