using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintON_ParentTableCode))]
	sealed class ConstraintON_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintON_ParentTableCode>
	{
		protected override string TableName => "CusEntryCPDec";

		protected override string TablePrefix => "ON";

		protected override string[] SupportedParentPrefixes => new[] { "CC","CH","CI","CL", "CRD", "OH" };

		protected override bool UseNoCheck => false;

		protected override bool AllowEmptyParentTableCode => true;

		protected override string[] ExpectedIndexIncludeColumns => new[] { "[ON_SystemCreateTimeUtc]", "[ON_SystemLastEditTimeUtc]" };
	}
}
