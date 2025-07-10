using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(LandedCostTables_ParentTableCode))]
	sealed class LandCostInput_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<LandedCostTables_ParentTableCode>
	{
		protected override string TableName => "LandCostInput";

		protected override string TablePrefix => "LI";

		protected override string[] SupportedParentPrefixes => ["CO", "JD", "JI", "JO", "JZ"];

		protected override bool UseNoCheck => false;

		protected override bool AllowEmptyParentTableCode => false;

		protected override string[] ExpectedIndexIncludeColumns => ["[LI_SystemCreateTimeUtc]", "[LI_SystemLastEditTimeUtc]", "[LI_AC_ChargeCode]"];

		public override string[] expectedIndex =>
		[
			"NONCLUSTERED INDEX [_WTG__Cleanup data for new constraint on Landed Cost tables XX_ParentTableCode column_1] ON [dbo].[LandCostInput] ([LI_ParentID]) INCLUDE ([LI_AC_ChargeCode], [LI_SystemCreateTimeUtc], [LI_SystemLastEditTimeUtc]) WHERE ([LI_ParentTableCode]<>'CO' AND [LI_ParentTableCode]<>'JD' AND [LI_ParentTableCode]<>'JI' AND [LI_ParentTableCode]<>'JO' AND [LI_ParentTableCode]<>'JZ') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Cleanup data for new constraint on Landed Cost tables XX_ParentTableCode column_2] ON [dbo].[LandedCostHeader] ([LT_ParentID]) INCLUDE ([LT_SystemCreateTimeUtc], [LT_SystemLastEditTimeUtc]) WHERE ([LT_ParentTableCode]<>'JD' AND [LT_ParentTableCode]<>'JE') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Cleanup data for new constraint on Landed Cost tables XX_ParentTableCode column_3] ON [dbo].[LandedCostHistory] ([LH_ParentID]) INCLUDE ([LH_OP], [LH_SystemCreateTimeUtc], [LH_SystemLastEditTimeUtc]) WHERE ([LH_ParentTableCode]<>'JI' AND [LH_ParentTableCode]<>'JO') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		];

		protected override (string column, string tablePrefix)[] ForeignKeyColumns =>
		[
			("LI_LT", "LT")
		];

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(TableName, "Constraint_LI_ClusterKey");
			base.PrepareTestData();
		}
	}
}
