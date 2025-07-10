using Enterprise.Build.Database.Script.Testing.Public.Core.ModelViews;
using NUnit.Framework;
using static System.Data.SqlDbType;

namespace Enterprise.Build.Database.Script.Testing.Public.Customs.CN.ModelViews
{
	[TestedType(typeof(CargoWise.DbUpgrader.Scripts.Definitions.Customs.CN.ModelViews.CNJobDeclaration))]
	sealed class CNJobDeclarationTest : BaseModelViewScriptTest
	{
		#region Overrides of ModelViewScriptTestBase

		protected override string ViewName => "CNJobDeclaration";

		protected override string UnderlyingTableName => "JobDeclaration";

		protected override TestDbViewHelper.DbColumn[] ExpectedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JE_CIQOfficeOfEntryExit", VarChar, 6),
			new TestDbViewHelper.DbColumn("JE_ClearanceMode", VarChar, 3),
			new TestDbViewHelper.DbColumn("JE_CNLastPortBeforeEntry", VarChar, 6),
			new TestDbViewHelper.DbColumn("JE_CNPortOfDestination", VarChar, 6),
			new TestDbViewHelper.DbColumn("JE_CNPortOfOrigin", VarChar, 6),
			new TestDbViewHelper.DbColumn("JE_CNTransportMode", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_DateOfUnloadComplete", DateTime, -1),
			new TestDbViewHelper.DbColumn("JE_InspectionInvolved", Bit, -1),
			new TestDbViewHelper.DbColumn("JE_LicenseInvolved", Bit, -1),
			new TestDbViewHelper.DbColumn("JE_OfficeOfEntryExit", VarChar, 4),
			new TestDbViewHelper.DbColumn("JE_RN_NKCountryOfTrade", VarChar, 2),
			new TestDbViewHelper.DbColumn("JE_TaxInvolved", Bit, -1),
			new TestDbViewHelper.DbColumn("JE_TransitMode", VarChar, 1),
			new TestDbViewHelper.DbColumn("JE_VesselInland", NVarChar, 50),
			new TestDbViewHelper.DbColumn("JE_VoyageInland", NVarChar, 32)
		};

		protected override bool HasIndexes => true;

		protected override TestDbViewHelper.DbColumn[] ExpectedIndexedViewColumns => new[]
		{
			new TestDbViewHelper.DbColumn("JE_PK", UniqueIdentifier, -1),
			new TestDbViewHelper.DbColumn("JE_ClusterKey", Int, -1, 10, 0),
			new TestDbViewHelper.DbColumn("JE_OfficeOfEntryExit", VarChar, 4)
		};

		#endregion
	}
}
