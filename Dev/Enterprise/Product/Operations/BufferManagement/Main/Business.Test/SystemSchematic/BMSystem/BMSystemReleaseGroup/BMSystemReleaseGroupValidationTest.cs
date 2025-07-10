using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMSystemReleaseGroupValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateReleaseGroupPK()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var releaseGroup1 = system.ReleaseGroups.AddNew();

			releaseGroup1.Validation.ValidateAll();
			AssertHasError(releaseGroup1.FSG_GG_GroupInfo, "Please enter a Group.");

			releaseGroup1.FSG_GG_Group = ZGuid.NewZGuid();
			AssertHasError(releaseGroup1.FSG_GG_GroupInfo, "Enter a valid Group.");

			var group = Factory.NewWithValidTestData<GlbGroup>();
			releaseGroup1.FSG_GG_Group = group.PK;
			AssertNoErrors(releaseGroup1.FSG_GG_GroupInfo);

			var releaseGroup2 = system.ReleaseGroups.AddNew();
			releaseGroup2.FSG_GG_Group = group.PK;
			AssertHasError(releaseGroup2.FSG_GG_GroupInfo, "The Group has been duplicated and must be unique.");
		}
	}
}
