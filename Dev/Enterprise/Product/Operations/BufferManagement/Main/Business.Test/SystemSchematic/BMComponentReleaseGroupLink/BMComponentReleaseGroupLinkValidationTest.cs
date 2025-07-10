using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMComponentReleaseGroupLinkValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckFO_TaskAssignTaskAgeInvalidFormat()
		{
			var component = Factory.New<BMComponent>();

			var glbGroup = Factory.New<GlbGroup>();

			var link = Factory.New<BMComponentReleaseGroupLink>();

			link.FO_GG_ReleaseGroup = glbGroup.PK;
			link.FO_FC_Component = component.PK;

			link.FO_AutoAssignTasksAge = ZDateTime.Invalid;
			link.Validation.ValidateAll();
			AssertHasError(link.FO_AutoAssignTasksAgeInfo, "Enter a valid Auto Assign Tasks Age. Correct format should be 000:00.");
		}

		public void TestCheckFO_ReleaseGateMode()
		{
			var link = Factory.New<BMComponentReleaseGroupLink>();

			link.FO_ReleaseGateMode = "";
			AssertHasError(link.FO_ReleaseGateModeInfo, "Please enter a Release Gate Mode.");

			link.FO_ReleaseGateMode = "ZZZ";
			AssertHasError(link.FO_ReleaseGateModeInfo, "Enter a valid Release Gate Mode.");

			link.FO_ReleaseGateMode = ReleaseGateModeList.Codes.Legacy;
			AssertNoErrors(link.FO_ReleaseGateModeInfo);
		}
	}
}
