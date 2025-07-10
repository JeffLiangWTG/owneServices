using Enterprise.DbUpgrader.Shared;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintCSI_ParentTableCode))]
	sealed class ConstraintCSI_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintCSI_ParentTableCode>
	{
		protected override string TableName => "CusSupportingInfo";

		protected override string TablePrefix => "CSI";

		protected override string[] SupportedParentPrefixes => new[] { "ABL", "AMA", "APA", "API", "ASR", "B0", "B3", "B9", "BH", "BM", "BY", "CCI", "CED", "CEI", "CER", "CI", "CL", "CSI", "CXC", "CY", "ERI", "JE", "JI", "JZ", "QH", "STH", "TSL", "TW1" };

		protected override bool UseNoCheck => true;

		protected override bool AllowEmptyParentTableCode => false;

		protected override void PrepareTestData()
		{
			DBTransformationTestHelper.DropConstraintIfExists(TableName, "Constraint_CSI_Type");
			DBTransformationTestHelper.DropConstraintIfExists(TableName, "Constraint_CSI_DataModel");
			base.PrepareTestData();
		}

		protected override string[] ExpectedIndexIncludeColumns => new[] { "[CSI_CSI_SupportingInfo]", "[CSI_SystemCreateTimeUtc]", "[CSI_SystemLastEditTimeUtc]", "[CSI_Type]" };
	}
}
