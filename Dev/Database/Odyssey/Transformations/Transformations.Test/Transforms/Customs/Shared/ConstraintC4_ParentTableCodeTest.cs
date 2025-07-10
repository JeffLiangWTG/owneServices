using System.Collections.Generic;
using System.Text;
using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintC4_ParentTableCode))]
	sealed class ConstraintC4_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintC4_ParentTableCode>
	{
		protected override string TableName => "CusUnderbond";

		protected override string TablePrefix => "C4";

		protected override string[] SupportedParentPrefixes => new[] { "BD", "CA", "CB", "CG", "CJ", "CM", "CN", "CS", "CV", "CX", "JC", "JE", "JS", "PW" };

		protected override bool UseNoCheck => true;

		protected override bool AllowEmptyParentTableCode => true;

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(TableName, "Constraint_C4_ApplicationCode");
			DBTransformationTestHelper.DropIndexIfExists(TableName, "NR_UC__C4_SendersMessageReference");
			base.PrepareTestData();
		}

		protected override void AppendInsertScript(StringBuilder sqlText, Dictionary<string, string> columnValues)
		{
			base.AppendInsertScript(sqlText, columnValues);

			var pk = columnValues[CusUnderbondSchema.Constants.PK];
			sqlText.AppendLine($"INSERT INTO dbo.CusOutturn(C5_PK, C5_C4_Underbond, C5_SystemCreateTimeUtc, C5_SystemCreateUser, C5_SystemLastEditTimeUtc, C5_SystemLastEditUser) VALUES (NEWID(), {pk}, GetUtcDate(), '~BP', GetUtcDate(), '~BP');");
		}

		protected override ConstraintC4_ParentTableCode CreateNewTransformationInstance()
		{
			return new ConstraintC4_ParentTableCode(8);
		}
	}
}
