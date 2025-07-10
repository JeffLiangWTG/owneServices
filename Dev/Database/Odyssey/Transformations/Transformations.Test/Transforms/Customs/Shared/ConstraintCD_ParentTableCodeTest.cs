using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintCD_ParentTableCode))]
	sealed class ConstraintCD_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintCD_ParentTableCode>
	{
		protected override string TableName => "CusUSClassification";

		protected override string TablePrefix => "CD";

		protected override string[] SupportedParentPrefixes => new[] { "CI" };

		protected override bool UseNoCheck => false;
		protected override bool AllowEmptyParentTableCode => false;

		protected override string[] ExpectedIndexIncludeColumns => new[] { "[CD_SystemCreateTimeUtc]", "[CD_SystemLastEditTimeUtc]" };
	}
}
