using System.Collections.Generic;
using Enterprise.DbUpgrader.Transformations.Transforms.Customs.Shared;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Customs.Shared
{
	[TestedType(typeof(ConstraintB7_ParentTableCode))]
	sealed class ConstraintB7_ParentTableCodeTest : ConstraintBase_ParentTableCodeTest<ConstraintB7_ParentTableCode>
	{
		protected override string TableName => "CusAddInfo";

		protected override string TablePrefix => "B7";

		protected override Dictionary<string, string> AdditionalColumnsAndValuesForChild => new Dictionary<string, string>
		{
			{ "B7_Type", "'USA'" }
		};

		protected override string[] SupportedParentPrefixes => new[] { "B0", "B7", "B9", "BC", "BH", "BY", "CC", "CEI", "CH", "CI", "CL", "CO", "CS", "CU", "CY", "JE", "JI", "JK", "JPB", "JPH", "JZ", "OH", "QH", "ULB", "WB", "WE" };

		protected override bool UseNoCheck => true;
		protected override bool AllowEmptyParentTableCode => false;
		protected override string[] ExpectedIndexIncludeColumns => new[] { "[B7_SystemCreateTimeUtc]", "[B7_SystemLastEditTimeUtc]" };
	}
}
