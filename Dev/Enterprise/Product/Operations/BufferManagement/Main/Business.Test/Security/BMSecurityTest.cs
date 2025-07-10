using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.Testing;

namespace Enterprise.BufferManagement.Business.Test.Security
{
	public class BMSecurityTest : BMSTestCaseWithFactory
	{
		public void TestBMSCheckpoints_ShouldBeDisabledByDefault()
		{
			// This test was moved from SupportedWorkflowTypesForAllDescriptorsTestRunner. Check there for its history.

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var security = new SecurityForTest(null, staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK);
			var operationsProcessManagerSection = security.FindCheckPoint("WorkflowOperationsSection");
			AssertNotNull(operationsProcessManagerSection);
			var bmSystem = security.FindCheckPoint("BMSystems");
			var visualBoardModuleMenu = security.FindCheckPoint("VisualBoards");

			AssertNotNull(bmSystem);
			AssertNotNull(visualBoardModuleMenu);

			AssertEquals(false, bmSystem.IsAllowed);
			AssertEquals(false, bmSystem.Parent.IsAllowed);
			AssertEquals(false, visualBoardModuleMenu.IsAllowed);
		}
	}
}
