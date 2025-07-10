using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CategorisedWorkflowTaskTypes))]
	sealed class CategorisedWorkflowTaskTypesTest : RegistryBusinessObjectTest
	{
		public void TestTaskTypes()
		{
			var newTaskTypes = new WorkflowTaskTypeCollection();
			var oldTaskTypes = BizObj.TaskTypes;
			Assert(BizObj.IsRegisteredEditableChildObject(oldTaskTypes));

			BizObj.SetTaskTypes(newTaskTypes);
			Assert(!BizObj.IsRegisteredEditableChildObject(oldTaskTypes));
			Assert(BizObj.IsRegisteredEditableChildObject(newTaskTypes));
			AssertEquals(newTaskTypes, BizObj.TaskTypes);
		}

		public void TestReservedTaskTypes()
		{
			var workflowType = new CategorisedWorkflowTaskTypes();
			workflowType.Code = "AAA";
			workflowType.Description = (NoResString)"Some Workflow Type";

			var taskType1 = workflowType.TaskTypes.AddNew();
			taskType1.Code = "AAA";
			taskType1.Description = (NoResString)"Not failing code";
			Assert(!workflowType.HasErrors);

			var taskType2 = workflowType.TaskTypes.AddNew();
			taskType2.Code = "TRG";
			taskType2.Description = (NoResString)"Failing code";
			Assert(workflowType.HasErrors);
			AssertHasError(taskType2.CodeInfo, "The Code must not be in the list of reserved codes 'EXC, MIL, TRG'.");
		}

		public override void TestMaxDescriptionLength()
		{
			AssertEquals("Custom MaxDescriptionLength", 256, BizObj.DescriptionInfo.MaxLength);
		}

		protected override void AssertCloneValues(RegistryBusinessObject clone)
		{
			var clonedTypes = (CategorisedWorkflowTaskTypes)clone;
			AssertEquals(5, clonedTypes.CodeMaxLength);
			AssertEquals("WRK", clonedTypes.Code);
			AssertEquals("Work Item", clonedTypes.Description);

			AssertEquals(2, clonedTypes.TaskTypes.Count);

			AssertEquals("CVW", clonedTypes.TaskTypes[0].Code);
			AssertEquals("Code Review", clonedTypes.TaskTypes[0].Description);
			AssertEquals(true, clonedTypes.TaskTypes[0].CreatesAppointment);
			AssertEquals(false, clonedTypes.TaskTypes[0].CanCloseTaskNotAssignedToSelf);
			AssertEquals(true, clonedTypes.TaskTypes[0].CanCancelTask);

			AssertEquals("FVW", clonedTypes.TaskTypes[1].Code);
			AssertEquals("Functional Review", clonedTypes.TaskTypes[1].Description);
			AssertEquals(false, clonedTypes.TaskTypes[1].CreatesAppointment);
			AssertEquals(true, clonedTypes.TaskTypes[1].CanCloseTaskNotAssignedToSelf);
			AssertEquals(false, clonedTypes.TaskTypes[1].CanCancelTask);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = (CategorisedWorkflowTaskTypes)GetNewBusinessObject();

			result.CodeMaxLength = 5;
			result.Code = "WRK";
			result.Description = (NoResString)"Work Item";

			var taskType1 = result.TaskTypes.AddNew();
			taskType1.Code = "CVW";
			taskType1.Description = (NoResString)"Code Review";
			taskType1.CreatesAppointment = true;
			taskType1.CanCloseTaskNotAssignedToSelf = false;
			taskType1.CanCancelTask = true;

			var taskType2 = result.TaskTypes.AddNew();
			taskType2.Code = "FVW";
			taskType2.Description = (NoResString)"Functional Review";
			taskType2.CreatesAppointment = false;
			taskType2.CanCloseTaskNotAssignedToSelf = true;
			taskType2.CanCancelTask = false;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		new CategorisedWorkflowTaskTypes BizObj
		{
			get { return (CategorisedWorkflowTaskTypes)base.BizObj; }
		}
	}
}
