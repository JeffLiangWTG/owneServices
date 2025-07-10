using Enterprise.DbUpgrader.Transformation.DataModification;
using Enterprise.DbUpgrader.Transformations.Transforms.Security;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Transformations.Test.Transforms.Security
{
	[TestedType(typeof(AddMarketIntelligenceGroup))]
	public class AddMarketIntelligenceUserGroupTest : SystemOrgSecurityGroupTransformTest
	{
		protected override bool IsSystemGroup => false;

		protected override string GroupCode => "WEBMARKETINTEL";

		protected override string RoleName => "webmarketintelligenceviewer";

		protected override string GroupDescription => "Web Market Intelligence (View)";

		protected override bool IsNeoDefaultRole => true;

		protected override DataTransformation GetNewTestTransformationInstance() => new AddMarketIntelligenceGroup();
	}

	[TestedType(typeof(AddMarketIntelligenceGroup))]
	public class AddMarketIntelligenceSystemGroupTest : AddMarketIntelligenceUserGroupTest
	{
		protected override bool IsSystemGroup => true;
	}

	[TestedType(typeof(AddMarketIntelligenceGroup))]
	public class AddMarketIntelligenceGroupTestEDI : SystemOrgSecurityGroupTransformTestEDI
	{
		protected override string GroupCode => "WEBMARKETINTEL";

		protected override string RoleName => "webmarketintelligenceviewer";

		protected override DataTransformation GetNewTestTransformationInstance() => new AddMarketIntelligenceGroup();
	}
}
