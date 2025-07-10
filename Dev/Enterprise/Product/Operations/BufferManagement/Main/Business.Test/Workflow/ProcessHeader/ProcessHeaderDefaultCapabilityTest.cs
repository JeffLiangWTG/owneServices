using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class ProcessHeaderDefaultCapabilityTest : BusinessObjectValidationTestCase
	{
		public void TestDefaultCapabilityIsAssignedAndWarningIsAttached()
		{
			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "GAV", capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "GAV");

			Factory.Save();

			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "MEH");
			var task4 = BMSTestHelper.CreateTask(workflow, taskType: "MEH");

			Factory.Save();

			AssertEquals(capability.PK, task1.P9_G4_RequiredCapability);
			AssertEquals(ZGuid.Empty, task2.P9_G4_RequiredCapability);
			AssertEquals(capability.PK, task3.P9_G4_RequiredCapability);
			AssertEquals(capability.PK, task4.P9_G4_RequiredCapability);
			AssertNoWarnings(task1.P9_G4_RequiredCapabilityInfo);
			AssertNoWarnings(task2.P9_G4_RequiredCapabilityInfo);
			AssertHasWarning(task3.P9_G4_RequiredCapabilityInfo, "The default capability has been applied.");
			AssertHasWarning(task4.P9_G4_RequiredCapabilityInfo, "The default capability has been applied.");
		}

		public void TestDefaultCapabilityIsNotAssigned_WhenAllTasksAreNew()
		{
			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "GAV", capability: capability, staffCode: "ABC");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "GAV");
			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "MEH");
			var task4 = BMSTestHelper.CreateTask(workflow, taskType: "MEH");

			Factory.Save();

			AssertEquals(capability.PK, task1.P9_G4_RequiredCapability);
			AssertEquals(ZGuid.Empty, task2.P9_G4_RequiredCapability);
			AssertEquals(ZGuid.Empty, task3.P9_G4_RequiredCapability);
			AssertEquals(ZGuid.Empty, task4.P9_G4_RequiredCapability);
			AssertNoWarnings(task1.P9_G4_RequiredCapabilityInfo);
			AssertNoWarnings(task2.P9_G4_RequiredCapabilityInfo);
			AssertNoWarnings(task3.P9_G4_RequiredCapabilityInfo);
			AssertNoWarnings(task4.P9_G4_RequiredCapabilityInfo);
		}

		public void TestDefaultCapabilityIsNotAssigned_WhenFirstTaskHasStaff()
		{
			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "GAV", capability: capability, staffCode: "ABC");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "GAV");

			Factory.Save();

			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "MEH");
			var task4 = BMSTestHelper.CreateTask(workflow, taskType: "MEH");

			Factory.Save();

			AssertEquals(capability.PK, task1.P9_G4_RequiredCapability);
			AssertEquals(ZGuid.Empty, task2.P9_G4_RequiredCapability);
			AssertEquals(ZGuid.Empty, task3.P9_G4_RequiredCapability);
			AssertEquals(ZGuid.Empty, task4.P9_G4_RequiredCapability);
			AssertNoWarnings(task1.P9_G4_RequiredCapabilityInfo);
			AssertNoWarnings(task2.P9_G4_RequiredCapabilityInfo);
			AssertNoWarnings(task3.P9_G4_RequiredCapabilityInfo);
			AssertNoWarnings(task4.P9_G4_RequiredCapabilityInfo);
		}

		public void TestDefaultCapabilityIsNotAssigned_WhenFirstTaskHasNoCapability()
		{
			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "GAV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "GAV", capability: capability);

			Factory.Save();

			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "MEH");
			var task4 = BMSTestHelper.CreateTask(workflow, taskType: "MEH");

			Factory.Save();

			AssertEquals(ZGuid.Empty, task1.P9_G4_RequiredCapability);
			AssertEquals(capability.PK, task2.P9_G4_RequiredCapability);
			AssertEquals(ZGuid.Empty, task3.P9_G4_RequiredCapability);
			AssertEquals(ZGuid.Empty, task4.P9_G4_RequiredCapability);
			AssertNoWarnings(task1.P9_G4_RequiredCapabilityInfo);
			AssertNoWarnings(task2.P9_G4_RequiredCapabilityInfo);
			AssertNoWarnings(task3.P9_G4_RequiredCapabilityInfo);
			AssertNoWarnings(task4.P9_G4_RequiredCapabilityInfo);
		}

		public void TestDefaultCapabilityIsNotAssigned_WhenThereIsNoDefault()
		{
			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "GAV");
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "GAV", capability: capability);

			Factory.Save();

			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "MEH");
			var task4 = BMSTestHelper.CreateTask(workflow, taskType: "MEH");

			Factory.Save();

			AssertEquals(ZGuid.Empty, task1.P9_G4_RequiredCapability);
			AssertEquals(capability.PK, task2.P9_G4_RequiredCapability);
			AssertEquals(ZGuid.Empty, task3.P9_G4_RequiredCapability);
			AssertEquals(ZGuid.Empty, task4.P9_G4_RequiredCapability);
			AssertNoWarnings(task1.P9_G4_RequiredCapabilityInfo);
			AssertNoWarnings(task2.P9_G4_RequiredCapabilityInfo);
			AssertNoWarnings(task3.P9_G4_RequiredCapabilityInfo);
			AssertNoWarnings(task4.P9_G4_RequiredCapabilityInfo);
		}

		public void TestDefaultCapabilityGeneratesErrorAndWarning_WhenAssignedAndCapabilityIsNotActive()
		{
			capability.G4_IsActive = false;
			Factory.Save();

			var task1 = BMSTestHelper.CreateTask(workflow, taskType: "GAV", capability: capability);
			var task2 = BMSTestHelper.CreateTask(workflow, taskType: "GAV");

			Factory.Save();

			var task3 = BMSTestHelper.CreateTask(workflow, taskType: "MEH");
			var task4 = BMSTestHelper.CreateTask(workflow, taskType: "MEH");

			Factory.Save();

			AssertEquals(capability.PK, task1.P9_G4_RequiredCapability);
			AssertEquals(ZGuid.Empty, task2.P9_G4_RequiredCapability);
			AssertEquals(capability.PK, task3.P9_G4_RequiredCapability);
			AssertEquals(capability.PK, task4.P9_G4_RequiredCapability);
			AssertHasError(task1.P9_G4_RequiredCapabilityInfo, "This Required Capability is inactive - it may not be used.");
			AssertNoWarnings(task2.P9_G4_RequiredCapabilityInfo);
			AssertHasError(task3.P9_G4_RequiredCapabilityInfo, "This Required Capability is inactive - it may not be used.");
			AssertHasWarning(task3.P9_G4_RequiredCapabilityInfo, "The default capability has been applied.");
			AssertHasError(task4.P9_G4_RequiredCapabilityInfo, "This Required Capability is inactive - it may not be used.");
			AssertHasWarning(task4.P9_G4_RequiredCapabilityInfo, "The default capability has been applied.");
		}

		GlbCapability capability;
		ProcessHeader workflow;
		GlbStaff staff;

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();

			capability = BMSTestHelper.CreateCapability(Factory);
			staff = BMSTestHelper.CreateStaff(Factory, "AAA");
			staff.Capabilities.Add(capability); // so we do not get warning that the capability has no staff
			Factory.Save();

			workflow = BMSTestHelper.CreateWorkflowAndParents<DummyWithWorkflow>(Factory, "Workflow");

			var collection = new CategorisedWorkflowTaskTypesCollection();
			var typeCategory = collection.AddNew();
			typeCategory.Code = workflow.FH_WorkflowType;
			var type = typeCategory.TaskTypes.AddNew();
			type.Code = "MEH";
			type.DefaultCapability = capability.PK;
			WorkflowDataRegistry.Instance.TaskTypes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, collection);
		}
	}
}
