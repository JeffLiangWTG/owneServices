using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.CN.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.CN.ModelViews.CNCusEntryInstruction))]
	sealed class CNCusEntryInstructionTest : BaseModelViewScriptTest
	{
		#region Overrides of ModelViewScriptTestBase

		protected override string ViewName => "CNCusEntryInstruction";

		protected override string UnderlyingTableName => "CusEntryInstruction";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("CEI_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CEI_ClusterKey", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CEI_CEI_Parent", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("CEI_CIQRelatedNum", VarChar, 18),
			new TestDbViewHelper.DbColumn("CEI_CIQRelatedReason", VarChar, 1),
			new TestDbViewHelper.DbColumn("CEI_CIQRequires", Bit, -1),
			new TestDbViewHelper.DbColumn("CEI_DocumentSubmissionType", VarChar, 1),
			new TestDbViewHelper.DbColumn("CEI_EnterprisePromised", Bit, -1),
			new TestDbViewHelper.DbColumn("CEI_LevyType", VarChar, 3),
			new TestDbViewHelper.DbColumn("CEI_ManualNo", VarChar, 12),
			new TestDbViewHelper.DbColumn("CEI_Packages", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("CEI_PackageUQ", VarChar, 2),
			new TestDbViewHelper.DbColumn("CEI_RelatedManualNo", VarChar, 12),
			new TestDbViewHelper.DbColumn("CEI_RelatedMRN", VarChar, 18),
		};

		protected override bool HasIndexes => false;

		#endregion
	}
}
