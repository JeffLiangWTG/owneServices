using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class BMNCNScheduleValidationTest : BusinessObjectValidationTestCase
	{
		public void TestBranch_ForScaledDiagram_ShouldBeMandatory()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 1");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();

			var schedule = diagram.GetOrCreateSchedule();
			AssertEquals(GlbBranch.CurrentBranch.PK, schedule.BNC_GB_Branch);

			schedule.Validation.ValidateAll();
			AssertNoErrors(schedule.BNC_GB_BranchInfo);

			schedule.BNC_GB_Branch = ZGuid.Empty;
			AssertHasError(schedule.BNC_GB_BranchInfo, "Please enter a Branch.");
		}

		public void TestDepartment_ForScaledDiagram_ShouldBeMandatory()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 1");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();

			var schedule = diagram.GetOrCreateSchedule();
			AssertEquals(GlbDepartment.CurrentDepartment.PK, schedule.BNC_GE_Department);

			schedule.Validation.ValidateAll();
			AssertNoErrors(schedule.BNC_GE_DepartmentInfo);

			schedule.BNC_GE_Department = ZGuid.Empty;
			AssertHasError(schedule.BNC_GE_DepartmentInfo, "Please enter a Department.");
		}

		public void TestBranch_ForNonScaledDiagram_ShouldNotBeMandatory()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 1");
			var schedule = diagram.GetOrCreateSchedule();
			AssertEquals(GlbBranch.CurrentBranch.PK, schedule.BNC_GB_Branch);

			schedule.Validation.ValidateAll();
			AssertNoErrors(schedule.BNC_GB_BranchInfo);

			schedule.BNC_GB_Branch = ZGuid.Empty;
			AssertNoErrors(schedule.BNC_GB_BranchInfo);
		}

		public void TestDepartment_ForNonScaledDiagram_ShouldNotBeMandatory()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 1");
			var schedule = diagram.GetOrCreateSchedule();
			AssertEquals(GlbDepartment.CurrentDepartment.PK, schedule.BNC_GE_Department);

			schedule.Validation.ValidateAll();
			AssertNoErrors(schedule.BNC_GE_DepartmentInfo);

			schedule.BNC_GE_Department = ZGuid.Empty;
			AssertNoErrors(schedule.BNC_GE_DepartmentInfo);
		}

		public void TestBranch_ForShapeOnScaledDiagram_ShouldNotBeMandatory()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 1");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();

			var shape = NetworkTestCase.CreateShape(diagram);
			var schedule = shape.GetOrCreateSchedule();
			AssertEquals(GlbBranch.CurrentBranch.PK, schedule.BNC_GB_Branch);

			schedule.Validation.ValidateAll();
			AssertNoErrors(schedule.BNC_GB_BranchInfo);

			schedule.BNC_GB_Branch = ZGuid.Empty;
			AssertNoErrors(schedule.BNC_GB_BranchInfo);
		}

		public void TestDepartment_ForShapeOnScaledDiagram_ShouldNotBeMandatory()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 1");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();

			var shape = NetworkTestCase.CreateShape(diagram);
			var schedule = shape.GetOrCreateSchedule();
			AssertEquals(GlbDepartment.CurrentDepartment.PK, schedule.BNC_GE_Department);

			schedule.Validation.ValidateAll();
			AssertNoErrors(schedule.BNC_GE_DepartmentInfo);

			schedule.BNC_GE_Department = ZGuid.Empty;
			AssertNoErrors(schedule.BNC_GE_DepartmentInfo);
		}

		public void TestIsBranchAndDepartmentValid()
		{
			var diagram = NetworkTestCase.CreateDiagram(Factory, name: "Diagram 1");
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			network.SwitchToScaled();

			var shape = NetworkTestCase.CreateShape(diagram).RootShape;
			var schedule = shape.GetOrCreateSchedule();
			AssertEquals(GlbDepartment.CurrentDepartment.PK, schedule.BNC_GE_Department);

			schedule.Validation.ValidateAll();

			Assert(schedule.Validation.IsBranchAndDepartmentValid);

			schedule.BNC_GB_Branch = ZGuid.Empty;
			schedule.Validation.ValidateAll();

			Assert(!schedule.Validation.IsBranchAndDepartmentValid);

			schedule.BNC_GE_Department = ZGuid.Empty;
			schedule.Validation.ValidateAll();

			Assert(!schedule.Validation.IsBranchAndDepartmentValid);

			schedule.BNC_GB_Branch = Env.CurrentBranchPK;
			schedule.Validation.ValidateAll();

			Assert(!schedule.Validation.IsBranchAndDepartmentValid);

			schedule.BNC_GE_Department = Env.CurrentDepartmentPK;
			schedule.Validation.ValidateAll();

			Assert(schedule.Validation.IsBranchAndDepartmentValid);
		}
	}
}
