using System;
using System.Collections.Generic;
using System.Text;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintBW_ParentTableCode))]
	sealed class ConstraintBW_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintBW_ParentTableCode>
	{
		protected override string TableName => "CusCAeMHHouse";

		protected override string TablePrefix => "BW";

		protected override string[] SupportedParentPrefixes => new[] { "HVC", "JS" };

		protected override bool UseNoCheck => false;

		protected override bool AllowEmptyParentTableCode => true;

		protected override string[] ExpectedIndexIncludeColumns => new[] { "[BW_SystemCreateTimeUtc]", "[BW_SystemLastEditTimeUtc]" };

		protected override (string column, string tablePrefix)[] ForeignKeyColumns => new []
		{
			(CusCAeMHHouseSchema.Constants.BW_BP_Master, "BP")
		};

		protected override void AppendInsertScript(StringBuilder sqlText, Dictionary<string, string> columnValues)
		{
			columnValues.Add(CusCAeMHHouseSchema.Constants.BW_MessageReference, $"'{Guid.NewGuid().ToString().Substring(0, CusCAeMHHouseSchema.BW_MessageReference.MaxLength)}'");
			base.AppendInsertScript(sqlText, columnValues);

			var pk = columnValues[CusCAeMHHouseSchema.Constants.PK];
			AddTableToScript(sqlText, "BQ");
			sqlText.AppendLine($"INSERT INTO dbo.CusCAeMHHouseContainerPivot (BPA_PK, BPA_SystemCreateTimeUtc, BPA_SystemCreateUser, BPA_SystemLastEditTimeUtc, BPA_SystemLastEditUser, BPA_BW_House, BPA_BQ_Container) VALUES (NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', {pk}, @CusCAeMHContainerPK);");
			sqlText.AppendLine($"INSERT INTO dbo.CusCAeMHItem (BX_PK, BX_SystemCreateTimeUtc, BX_SystemCreateUser, BX_SystemLastEditTimeUtc, BX_SystemLastEditUser, BX_BW_House, BX_LineNumber) VALUES (NEWID(), GetUtcDate(), '~BP', GetUtcDate(), '~BP', {pk}, 1);");
		}
	}
}
