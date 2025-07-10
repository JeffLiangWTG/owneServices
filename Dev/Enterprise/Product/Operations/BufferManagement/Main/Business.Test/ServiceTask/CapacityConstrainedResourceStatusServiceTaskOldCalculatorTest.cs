using System;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(CapacityConstrainedResourceStatusServiceTask))]
	class CapacityConstrainedResourceStatusServiceTaskOldCalculatorTest : CapacityConstrainedResourceStatusServiceTaskTest
	{
		protected override bool UsingNewCalculator => false;

		protected override bool UsingSimpleQuery => false;

		protected override void LocalSetUp()
		{
			BMSRegistry.Instance.EnableSimpleCapacityCalculationForAllSystems.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}
	}
}
