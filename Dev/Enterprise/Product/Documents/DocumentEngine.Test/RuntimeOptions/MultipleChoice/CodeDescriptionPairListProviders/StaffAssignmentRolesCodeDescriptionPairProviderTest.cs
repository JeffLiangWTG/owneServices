using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class StaffAssignmentRolesCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new StaffAssignmentRolesCodeDescriptionPairProvider();
		}

		public override void TestIsReturningCorrectCollection()
		{
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), DataRegistry.Instance.OrgStaffMemberAssignmentRoles);
		}
	}
}
