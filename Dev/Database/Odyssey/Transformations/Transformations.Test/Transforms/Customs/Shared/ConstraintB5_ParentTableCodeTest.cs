using System.Collections.Generic;
using System.Text;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintB5_ParentTableCode))]
	sealed class ConstraintB5_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintB5_ParentTableCode>
	{
		protected override string TableName => "CusInvPack";

		protected override string TablePrefix => "B5";

		protected override string[] SupportedParentPrefixes => new[] { "B7", "B9", "BY", "CXI", "JE", "JI", "JZ" };

		protected override bool UseNoCheck => false;

		protected override bool AllowEmptyParentTableCode => false;

		protected override string[] ExpectedIndexIncludeColumns => new[] { "[B5_SystemCreateTimeUtc]", "[B5_SystemLastEditTimeUtc]" };

		protected override void AppendInsertScript(StringBuilder sqlText, Dictionary<string, string> columnValues)
		{
			base.AppendInsertScript(sqlText, columnValues);

			// Insert pack difference with a valid ParentTableCode
			var pk = columnValues[CusInvPackSchema.Constants.PK];
			sqlText.AppendLine($"INSERT INTO dbo.CusInvPack(B5_PK, B5_ParentID, B5_ParentTableCode, B5_B5_ParentPackage, B5_SystemCreateTimeUtc, B5_SystemCreateUser, B5_SystemLastEditTimeUtc, B5_SystemLastEditUser) VALUES (NEWID(), NEWID(), 'JE', {pk}, GetUtcDate(), '~BP', GetUtcDate(), '~BP');");
		}
	}
}
