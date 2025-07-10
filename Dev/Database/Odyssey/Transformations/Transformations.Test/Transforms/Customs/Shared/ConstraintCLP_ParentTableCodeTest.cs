using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintCLP_ParentTableCode))]
	sealed class ConstraintCLP_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintCLP_ParentTableCode>
	{
		protected override string TableName => "CusCALPCO";

		protected override string TablePrefix => "CLP";

		protected override string[] SupportedParentPrefixes => new[] { "B7", "JE" };

		protected override bool UseNoCheck => false;

		protected override bool AllowEmptyParentTableCode => false;

		protected override string[] ExpectedIndexIncludeColumns => new[] { "[CLP_OA_Applicant]", "[CLP_OA_Holder]", "[CLP_SystemCreateTimeUtc]", "[CLP_SystemLastEditTimeUtc]" };
	}
}
