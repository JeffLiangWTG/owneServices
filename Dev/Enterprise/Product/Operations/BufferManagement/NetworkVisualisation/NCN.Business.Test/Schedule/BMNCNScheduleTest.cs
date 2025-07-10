using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	[TestedType(typeof(BMNCNSchedule))]
	class BMNCNScheduleTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAnnotationsDontGetSchedules()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Wyrm");

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var annotation = networkViewModel.CreateNewAnnotation(diagram).Shape;

			AssertNull(annotation.ScheduleBizo);
		}

		public void TestBranchAndDepartmentReadOnly()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Wyrm");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var schedule = diagram.GetOrCreateSchedule();

			//non-scaled
			Assert("Non-scaled, Branch should be readonly", schedule.BNC_GB_BranchInfo.ReadOnly);
			Assert("Non-scaled, Department should be readonly", schedule.BNC_GE_DepartmentInfo.ReadOnly);

			AssertEquals("Non-scaled, Branch should be current login branch", Env.CurrentBranchPK, schedule.BNC_GB_Branch);
			AssertEquals("Non-scaled, Department should be current login department ", Env.CurrentDepartmentPK, schedule.BNC_GE_Department);

			//scaled
			network.SwitchToScaled();
			Assert("Scaled, Branch should not be readonly", !schedule.BNC_GB_BranchInfo.ReadOnly);
			Assert("Scaled, Department should not be readonly", !schedule.BNC_GE_DepartmentInfo.ReadOnly);

			AssertEquals("Scaled, Branch should be current login branch", Env.CurrentBranchPK, schedule.BNC_GB_Branch);
			AssertEquals("Scaled, Department should be current login department ", Env.CurrentDepartmentPK, schedule.BNC_GE_Department);

			//approved
			networkViewModel.ToggleApproval();
			Assert("Approved, Branch should be readonly", schedule.BNC_GB_BranchInfo.ReadOnly);
			Assert("Approved, Department should be readonly", schedule.BNC_GE_DepartmentInfo.ReadOnly);

			schedule.BNC_GB_Branch = ZGuid.Empty;
			Factory.Save();

			//approved, but only branch is empty
			Assert("Scaled, Branch should not be readonly", !schedule.BNC_GB_BranchInfo.ReadOnly);
			Assert("Scaled, Department should not be readonly", !schedule.BNC_GE_DepartmentInfo.ReadOnly);

			schedule.BNC_GE_Department = ZGuid.Empty;
			Factory.Save();

			//approved, but branch and department is empty
			Assert("Scaled, Branch should not be readonly", !schedule.BNC_GB_BranchInfo.ReadOnly);
			Assert("Scaled, Department should not be readonly", !schedule.BNC_GE_DepartmentInfo.ReadOnly);

			schedule.BNC_GB_Branch = Env.CurrentBranchPK;
			Factory.Save();

			//approved, but only department is empty
			Assert("Scaled, Branch should not be readonly", !schedule.BNC_GB_BranchInfo.ReadOnly);
			Assert("Scaled, Department should not be readonly", !schedule.BNC_GE_DepartmentInfo.ReadOnly);

			schedule.BNC_GE_Department = Env.CurrentDepartmentPK;
			Factory.Save();

			//approved, nor branch or department are empty
			Assert("Approved, Branch should be readonly", schedule.BNC_GB_BranchInfo.ReadOnly);
			Assert("Approved, Department should be readonly", schedule.BNC_GE_DepartmentInfo.ReadOnly);
		}

		public void TestScaledDiagramGetsLoginBranchDepartments()
		{
			var diagram1 = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 1");
			var shape1 = NetworkTestCase.CreateShape(diagram1, "shape1");
			var network1 = NetworkTestCase.CreateNetwork(diagram1);

			var diagram2 = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 2");
			var shape2 = NetworkTestCase.CreateShape(diagram2, "shapeOrphan");
			var network2 = NetworkTestCase.CreateNetwork(diagram2);
			network2.SwitchToScaled();

			//new branch & dep
			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_RL_NKHomePort = "ATGUK";

			GlbDepartment newDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			GlbStaff createUser = Factory.NewWithValidTestData<GlbStaff>();

			Factory.Save();

			using (Env.SetTemporaryUserContext(createUser.GS_LoginName, newBranch.PK.ToGuid(), newDepartment.PK.ToGuid()))
			{
				network1.SwitchToScaled();
				//Diagram1 - scaled under new user
				AssertEquals("Diagram1 Branch should be current login branch", newBranch.PK, diagram1.ScheduleBizo.BNC_GB_Branch);
				AssertEquals("Diagram1 Department should be current login department ", newDepartment.PK, diagram1.ScheduleBizo.BNC_GE_Department);

				//Shape1
				AssertEquals("child shape1 Branch should be current login branch", newBranch.PK, shape1.ScheduleBizo.BNC_GB_Branch);
				AssertEquals("child shape1 Department should be current login department ", newDepartment.PK, shape1.ScheduleBizo.BNC_GE_Department);

				//Diagram2 - scaled under previous user
				AssertNotEquals("Diagram2 Branch should not be current login branch", newBranch.PK, diagram2.ScheduleBizo.BNC_GB_Branch);
				AssertNotEquals("Diagram2 Department should not be current login department ", newDepartment.PK, diagram2.ScheduleBizo.BNC_GE_Department);

				//Shape2
				AssertNotEquals("Shape2 Branch should not be current login branch", newBranch.PK, shape2.ScheduleBizo.BNC_GB_Branch);
				AssertNotEquals("Shape2 Department should not be current login department ", newDepartment.PK, shape2.ScheduleBizo.BNC_GE_Department);
			}
		}

		public void TestApprovingShapeBranchDeptPassesToDescendant()
		{
			var diagram1 = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 1");
			var networkViewModel1 = NetworkTestCase.CreateNetworkViewModel(diagram1);
			var network1 = networkViewModel1.GetJobNetwork();

			var shape1 = networkViewModel1.CreateNewShape(diagram1).Shape;
			var shape2 = networkViewModel1.CreateNewShape(diagram1).Shape;
			var shape21 = networkViewModel1.CreateNewShape(diagram1).Shape;

			network1.SwitchToScaled();

			GlbBranch newBranch = Factory.NewWithValidTestData<GlbBranch>();
			newBranch.GB_RL_NKHomePort = "ATGUK";

			GlbDepartment newDepartment = Factory.NewWithValidTestData<GlbDepartment>();

			Factory.Save();

			diagram1.GetOrCreateSchedule().BNC_GB_Branch = newBranch.PK;
			diagram1.GetOrCreateSchedule().BNC_GE_Department = newDepartment.PK;

			//Ensure shape1's branch/dep is not same as diagram, yet
			AssertNotEquals("child shape1 Branch should not be diagram's branch", newBranch.PK, shape1.GetOrCreateSchedule().BNC_GB_Branch);
			AssertNotEquals("child shape1 Department should not be diagram's department ", newDepartment.PK, shape1.GetOrCreateSchedule().BNC_GE_Department);

			AssertNotEquals("child shape2 Branch should not be diagram's branch", newBranch.PK, shape2.GetOrCreateSchedule().BNC_GB_Branch);
			AssertNotEquals("child shape2 Department should not be diagram's department ", newDepartment.PK, shape2.GetOrCreateSchedule().BNC_GE_Department);

			AssertNotEquals("child shape21 Branch should not be diagram's branch", newBranch.PK, shape21.GetOrCreateSchedule().BNC_GB_Branch);
			AssertNotEquals("child shape21 Department should not be diagram's department ", newDepartment.PK, shape21.GetOrCreateSchedule().BNC_GE_Department);

			//approve it
			diagram1.ScheduledStartTimeUtc = ZDateTime.UtcNow;

			new ApproveDiagramAction(networkViewModel1).ExecuteForEntityWithoutAccessCheck(diagram1);
			Factory.Save();

			Assert("Shape1 should be approved", shape1.IsApproved);
			Assert("Shape2 should be approved", shape2.IsApproved);
			Assert("Shape21 should be approved", shape21.IsApproved);

			//after approval, ensure all children's branch/dep are same as parent
			AssertEquals("child shape1 Branch should be diagram's branch", newBranch.PK, shape1.GetOrCreateSchedule().BNC_GB_Branch);
			AssertEquals("child shape1 Department should be diagram's department ", newDepartment.PK, shape1.GetOrCreateSchedule().BNC_GE_Department);

			AssertEquals("child shape2 Branch should be diagram's branch", newBranch.PK, shape2.GetOrCreateSchedule().BNC_GB_Branch);
			AssertEquals("child shape2 Department should be diagram's department ", newDepartment.PK, shape2.GetOrCreateSchedule().BNC_GE_Department);

			AssertEquals("child shape21 Branch should be diagram's branch", newBranch.PK, shape2.GetOrCreateSchedule().BNC_GB_Branch);
			AssertEquals("child shape21 Department should be diagram's department ", newDepartment.PK, shape2.GetOrCreateSchedule().BNC_GE_Department);

			// a new shape is added after approval
			var shape3 = NetworkTestCase.CreateShape(diagram1, "shape3");

			Assert("Shape3 should not approved yet", !shape3.IsApproved);
			AssertNotEquals("child shape3 Branch should not be diagram's branch", newBranch.PK, shape3.GetOrCreateSchedule().BNC_GB_Branch);
			AssertNotEquals("child shape3 Department should not be diagram's department ", newDepartment.PK, shape3.GetOrCreateSchedule().BNC_GE_Department);

			//approve non-approved shapes
			new ApproveNonApprovedShapesAction(networkViewModel1).ExecuteForEntityWithoutAccessCheck(diagram1);
			Factory.Save();

			//shape 2 should be approved and have the branch/dep as the diagram
			Assert("Shape3 should be approved", shape3.IsApproved);
			AssertEquals("child shape3 Branch should be diagram's branch", newBranch.PK, shape3.GetOrCreateSchedule().BNC_GB_Branch);
			AssertEquals("child shape3 Department should be diagram's department ", newDepartment.PK, shape3.GetOrCreateSchedule().BNC_GE_Department);
		}

		public void TestScheduleScalingFields_ShouldBeSetToProperFormat_WhenSetThroughOldScalingSystem()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Wyrm");

			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			AssertEquals("Default scale set properly", diagram.Scale, new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes());
			AssertEquals("Default resolutionIncrement set properly", new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes(), diagram.ResolutionIncrement);
			AssertNotNull(diagram.ScheduleBizo);

			AssertEquals("Schedule has a converted Scale value set", new ZInt(60 * BMConstants.WorkingHoursPerDay), diagram.ScheduleBizo.BNC_ScaleMagnitude);
			AssertEquals("Schedule has a converted ResolutionIncrement value set", new ZInt(60 * BMConstants.WorkingHoursPerDay), diagram.ScheduleBizo.BNC_ResolutionIncrement);
			AssertEquals("Schedule has the default scaleUnit of Hourset", ScaleUnitList.Codes.Hour, diagram.ScheduleBizo.BNC_ScaleUnit);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			BMSTestHelper.EnableBMSInRegistry();
		}

		#endregion
	}
}
