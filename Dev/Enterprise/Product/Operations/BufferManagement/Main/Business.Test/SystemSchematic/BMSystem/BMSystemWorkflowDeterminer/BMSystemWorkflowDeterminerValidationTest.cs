using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	class BMSystemWorkflowDeterminerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNonSupportedBufferManagementType_ShouldAddValidationError()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var workflowType = system.RelatedWorkflowTypes.AddNew();

			var supportedWorkflowDescriptor = WorkflowDescriptors.Instance.Values.First(d => d.SupportsBufferManagement);
			var nonSupportedWorkflowDescriptor = WorkflowDescriptors.Instance.Values.FirstOrDefault(d => !d.SupportsBufferManagement);

			if (nonSupportedWorkflowDescriptor == null)
			{
				Assert("Yaaay everything supports BufferManagement :)", true);
			}
			else
			{
				workflowType.FSW_WorkflowType = nonSupportedWorkflowDescriptor.Code;
				AssertHasError(workflowType.FSW_WorkflowTypeInfo, "This Workflow Type does not support Buffer Management.");

				workflowType.FSW_WorkflowType = supportedWorkflowDescriptor.Code;
				AssertNoErrors(workflowType.FSW_WorkflowTypeInfo);
			}
		}

		public void TestMandatoryValidation()
		{
			var system = BMSTestHelper.CreateSystem(Factory);
			var workflowType = system.RelatedWorkflowTypes.AddNew();

			workflowType.Validation.ValidateAll();
			AssertHasError(workflowType.FSW_WorkflowTypeInfo, "Please enter a Workflow Type.");

			workflowType.FSW_WorkflowType = ":-)";
			AssertHasError(workflowType.FSW_WorkflowTypeInfo, "Enter a valid Workflow Type.");

			workflowType.FSW_WorkflowType = WorkflowDescriptors.WorkItemWorkflowDescriptorCode;
			AssertNoErrors(workflowType.FSW_WorkflowTypeInfo);
		}

		public void TestUniqueValidationOnWorkflowType_WithinSystem()
		{
			var system = BMSTestHelper.CreateSystem(Factory);

			var workflowType1 = system.RelatedWorkflowTypes.AddNew();
			var workflowType2 = system.RelatedWorkflowTypes.AddNew();

			workflowType1.FSW_WorkflowType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			workflowType2.FSW_WorkflowType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			AssertHasError(workflowType1.FSW_WorkflowTypeInfo, "The Workflow Type has been duplicated and must be unique.");
			AssertHasError(workflowType2.FSW_WorkflowTypeInfo, "The Workflow Type has been duplicated and must be unique.");

			AssertExceptionThrown<ZSaveException>(Factory.Save);
		}

		public void TestUniqueValidationOnWorkflowType_AcrossSystems()
		{
			var system1 = Factory.New<BMSystem>();
			var system2 = Factory.New<BMSystem>();

			system1.FS_Name = "SystemOne";
			system2.FS_Name = "SystemTwo";

			var workflowType1 = system1.RelatedWorkflowTypes.AddNew();
			var workflowType2 = system2.RelatedWorkflowTypes.AddNew();

			workflowType1.FSW_WorkflowType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			workflowType2.FSW_WorkflowType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;

			AssertHasError(workflowType2.FSW_WorkflowTypeInfo, "The Workflow Type is in use by SystemOne and must only be used by one system.");

			AssertExceptionThrown<ZSaveException>(Factory.Save);
		}
	}
}
