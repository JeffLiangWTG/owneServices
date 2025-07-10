using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintC5_ParentTableCode))]
	sealed class ConstraintC5_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintC5_ParentTableCode>
	{
		protected override string TableName => "CusOutturn";

		protected override string TablePrefix => "C5";

		protected override string[] SupportedParentPrefixes => new[] { "APA", "BD", "CG", "CJ", "CM", "CS", "CV", "CX", "JC", "JI", "JS" };

		protected override bool UseNoCheck => true;
		protected override bool AllowEmptyParentTableCode => true;
		protected override string[] ExpectedIndexIncludeColumns => new[] { "[C5_SystemCreateTimeUtc]", "[C5_SystemLastEditTimeUtc]" };
	}
}
