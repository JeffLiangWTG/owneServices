using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	public class CapacityAllocationTestingEnvironment : Assertion
	{
		public CapacityAllocationTestingEnvironment(BusinessObjectFactory factory, BMSystem system)
		{
			var helper = new WorkflowCapabilityAssignerTestHelper(factory);

			Buffer = BMSTestHelper.CreateBuffer(system, "Buffer");
			Buffer.FC_AutoAssignTasksAge = new ZInt(60).GetDateTimeFromMinutes();

			var capability1 = factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "COD";
			capability1.G4_AllowTaskAutoAssignment = true;
			var capability2 = factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "RVW";
			capability2.G4_AllowTaskAutoAssignment = true;

			Resource1 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "FRO", "Frodo Baggins", capability1);
			Resource2 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "SAM", "Samwise Gamgee", capability2);
			Resource3 = BMSTestHelper.CreateStaffInCurrentBranchDept(factory, "GAN", "Gandalf the Grey");

			Workflow1 = helper.CreateJobAndWorkflow(true, Buffer, ZDateTime.Today.AddDays(-1));

			var task1_1 = helper.CreateTask(Workflow1, capability1, null, 60);
			Assert(task1_1.RequiresResourceWithCapability);

			var task1_2 = helper.CreateTask(Workflow1, capability1, null, 60);
			Assert(task1_2.RequiresResourceWithCapability);

			var task1_3 = helper.CreateTask(Workflow1, capability2, null, 60);
			Assert(task1_3.RequiresResourceWithCapability);

			var task1_4 = helper.CreateTask(Workflow1, capability2, Resource3, 60);
			Assert(!task1_4.RequiresResourceWithCapability);

			Workflow2 = BMSTestHelper.CreateJobHeader<OrgHeader>(factory).ProcessHeaders[0];
			Workflow2.FH_FC_CurrentComponent = Buffer.PK;
			Task2_1 = BMSTestHelper.CreateTask(Workflow2, Resource1.GS_Code, Buffer.FC_BufferTimespanInMinutes);
			Task2_2 = BMSTestHelper.CreateTask(Workflow2, Resource2.GS_Code, Buffer.FC_BufferTimespanInMinutes);
			Task2_3 = BMSTestHelper.CreateTask(Workflow2, Resource3.GS_Code, Buffer.FC_BufferTimespanInMinutes);

			factory.Save();
		}

		public BMComponent Buffer;
		public ProcessHeader Workflow1;
		public ProcessHeader Workflow2;
		public GlbStaff Resource1;
		public GlbStaff Resource2;
		public GlbStaff Resource3;
		public ProcessTask Task2_1;
		public ProcessTask Task2_2;
		public ProcessTask Task2_3;
	}
}
