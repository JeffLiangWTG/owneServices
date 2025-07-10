using System;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(CapabilityTaskAutoAssignmentServiceTask))]
	class CapabilityTaskAutoAssignmentServiceTaskOldCalculatorTest : CapabilityTaskAutoAssignmentServiceTaskTest
	{
		protected override void SetUpCore()
		{
			base.SetUpCore();
			BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
