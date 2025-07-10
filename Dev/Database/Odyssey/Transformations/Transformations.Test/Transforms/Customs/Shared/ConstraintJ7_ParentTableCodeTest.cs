using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintJ7_ParentTableCode))]
	sealed class ConstraintJ7_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintJ7_ParentTableCode>
	{
		protected override string TableName => "JobComInvHeaderCharge";

		protected override string TablePrefix => "J7";

		protected override string[] SupportedParentPrefixes => new[] { "JD", "JI", "JO", "JZ" };

		protected override bool UseNoCheck => true;

		protected override string[] ExpectedIndexIncludeColumns => new[] { "[J7_SystemCreateTimeUtc]", "[J7_SystemLastEditTimeUtc]" };

		protected override bool AllowEmptyParentTableCode => false;
	}
}
