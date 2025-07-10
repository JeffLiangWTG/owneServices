using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintCY_ParentTableCode))]
	sealed class ConstraintCY_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintCY_ParentTableCode>
	{
		protected override string TableName => "CusCodeData";

		protected override string TablePrefix => "CY";

		protected override string[] SupportedParentPrefixes => new[] { "ABL", "AMA", "B0", "B7", "B9", "BC", "BH", "BJ", "BK", "BM", "BY", "C5", "CC", "CEI", "CER", "CGC", "CH", "CI", "CL", "CPH", "CSI", "CU", "CY", "DL", "EE", "EM", "HVC", "JE", "JI", "JK", "JPB", "JPH", "JS", "JZ", "OH", "OV", "PF", "QH", "QL", "STH", "XX" };

		protected override bool UseNoCheck => true;

		protected override bool AllowEmptyParentTableCode => false;

		protected override string[] ExpectedIndexIncludeColumns => new[] { "[CY_SystemCreateTimeUtc]", "[CY_SystemLastEditTimeUtc]" };
	}
}
