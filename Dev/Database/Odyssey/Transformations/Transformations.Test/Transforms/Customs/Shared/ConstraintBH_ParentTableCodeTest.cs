using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintBH_ParentTableCode))]
	sealed class ConstraintBH_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintBH_ParentTableCode>
	{
		protected override string TableName => "CusInBondHeader";

		protected override string TablePrefix => "BH";

		protected override string[] SupportedParentPrefixes => new[] { "CEI", "JE", "JK", "JS", "JX" };

		protected override bool UseNoCheck => false;

		protected override bool AllowEmptyParentTableCode => true;

		protected override string[] ExpectedIndexIncludeColumns => new[] { "[BH_SystemCreateTimeUtc]", "[BH_SystemLastEditTimeUtc]" };

		protected override (string column, string tablePrefix)[] ForeignKeyColumns => new[]
		{
			("BH_GB", "GB")
		};
	}
}
