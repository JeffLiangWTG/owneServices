using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintBP_ParentTableCode))]
	sealed class ConstraintBP_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintBP_ParentTableCode>
	{
		protected override string TableName => "CusCAeMHMaster";

		protected override string TablePrefix => "BP";

		protected override string[] SupportedParentPrefixes => new[] { "JK" };

		protected override bool UseNoCheck => false;

		protected override bool AllowEmptyParentTableCode => true;

		protected override string[] ExpectedIndexIncludeColumns => new[] { "[BP_PK]", "[BP_SystemCreateTimeUtc]", "[BP_SystemLastEditTimeUtc]" };

		protected override (string column, string tablePrefix)[] ForeignKeyColumns => new (string column, string tablePrefix)[]
		{
			( "BP_GB_Branch", "GB" )
		};

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(TableName, "Constraint_BP_ParentTableCodeAllowEmpty");
			base.PrepareTestData();
		}

		protected override void AppendInsertScript(StringBuilder sqlText, Dictionary<string, string> columnValues)
		{
			base.AppendInsertScript(sqlText, columnValues);

			if (columnValues["BP_ParentTableCode"] == "'!!'")
			{
				var bpPK = columnValues["BP_PK"];
				var cusCAeMHContainerPK = Guid.NewGuid();
				var cusCAeMHHousePK = Guid.NewGuid();
				var cusCAeMHHouseContainerPivotPK = Guid.NewGuid();
				var cusCAeMHItemPK = Guid.NewGuid();
				sqlText.AppendLine($"INSERT INTO dbo.CusCAeMHContainer (BQ_PK, BQ_BP_Master, BQ_SystemCreateTimeUtc, BQ_SystemCreateUser, BQ_SystemLastEditTimeUtc, BQ_SystemLastEditUser) VALUES ('{cusCAeMHContainerPK}', {bpPK}, GetUtcDate(), '~BP', GetUtcDate(), '~BP');");
				sqlText.AppendLine($"INSERT INTO dbo.CusCAeMHHouse (BW_PK, BW_BP_Master, BW_MessageReference, BW_SystemCreateTimeUtc, BW_SystemCreateUser, BW_SystemLastEditTimeUtc, BW_SystemLastEditUser) VALUES ('{cusCAeMHHousePK}', {bpPK}, 'BWMsgRef2', GetUtcDate(), '~BP', GetUtcDate(), '~BP');");
				sqlText.AppendLine($"INSERT INTO dbo.CusCAeMHHouseContainerPivot(BPA_PK, BPA_BW_House, BPA_BQ_Container, BPA_SystemCreateTimeUtc, BPA_SystemCreateUser, BPA_SystemLastEditTimeUtc, BPA_SystemLastEditUser) VALUES ('{cusCAeMHHouseContainerPivotPK}', '{cusCAeMHHousePK}', '{cusCAeMHContainerPK}', GetUtcDate(), '~BP', GetUtcDate(), '~BP');");
				sqlText.AppendLine($"INSERT INTO dbo.CusCAeMHItem(BX_PK, BX_BW_House, BX_LineNumber, BX_SystemCreateTimeUtc, BX_SystemCreateUser, BX_SystemLastEditTimeUtc, BX_SystemLastEditUser) VALUES ('{cusCAeMHItemPK}', '{cusCAeMHHousePK}', 1, GetUtcDate(), '~BP', GetUtcDate(), '~BP');");
			}
		}

		public override string[] expectedIndex =>
		[
			"NONCLUSTERED INDEX [_WTG__Cleanup data for new constraint on column BP_ParentTableCode_1] ON [dbo].[CusCAeMHMaster] ([BP_ParentID]) INCLUDE ([BP_PK], [BP_SystemCreateTimeUtc], [BP_SystemLastEditTimeUtc]) WHERE ([BP_ParentTableCode]<>'' AND [BP_ParentTableCode]<>'JK') WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Cleanup data for new constraint on column BP_ParentTableCode_2] ON [dbo].[CusCAeMHHouseContainerPivot] ([BPA_BW_House]) INCLUDE ([BPA_BQ_Container], [BPA_SystemCreateTimeUtc], [BPA_SystemLastEditTimeUtc]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
			"NONCLUSTERED INDEX [_WTG__Cleanup data for new constraint on column BP_ParentTableCode_3] ON [dbo].[CusCAeMHHouseContainerPivot] ([BPA_BQ_Container]) INCLUDE ([BPA_BW_House], [BPA_SystemCreateTimeUtc], [BPA_SystemLastEditTimeUtc]) WITH (ALLOW_PAGE_LOCKS = OFF, ONLINE = ON)",
		];
	}
}
