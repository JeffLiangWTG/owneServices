using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintPW_ParentTableCode))]
	sealed class ConstraintPW_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintPW_ParentTableCode>
	{
		protected override string TableName => CusBondDetailSchema.Constants.TableName;

		protected override string TablePrefix => "PW";

		protected override string[] SupportedParentPrefixes => new[] { "ABL", "AMA", "BH", "BM", "CEI", "JE", "OH", "SRH" };

		protected override bool UseNoCheck => true;
		protected override bool AllowEmptyParentTableCode => false;
		protected override string[] ExpectedIndexIncludeColumns => new[] { "[PW_SystemCreateTimeUtc]", "[PW_SystemLastEditTimeUtc]" };
	}
}
