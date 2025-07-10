using Enterprise.BufferManagement.Business.Test;

namespace Enterprise.BufferManagement.GUI.Test
{
	class WorkflowCapabilityAssignerTest_AutoAssignWorkflowsImmediatelyOrDelayed_SimpleCalculator : WorkflowCapabilityAssignerTest_AutoAssignWorkflowsInBatchesImmediately_OldCalculator
	{
		#region WorkflowCapabilityAssignerTestCase Overrides

		protected override bool UsingSimpleQuery => true;

		protected override void SetUp()
		{
			CapacityCalculatorTestHelper.EnableSimpleCapacityCalculation();
			base.SetUp();
		}
		#endregion
	}
}
