using System;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(DedicatedBufferFilter))]
	class DedicatedBufferFilterTest : ComponentFilterTestCase<DedicatedBufferFilter>
	{
		protected override string componentFilterDescription => ProcessHeader.ModuleFilterConstants.DedicatedBuffer;

		protected override void SetComponent(ProcessHeader workflow, ZGuid value)
		{
			workflow.FH_FC_DedicatedBuffer = value;
		}

		public void TestShouldExist_InJobWorkflowsModule_WhenDisplayResponsiveReleaseGateUiSettings_IsEnabled()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			AssertNotNull(filterBizo[componentFilterDescription]);
		}

		public void TestShouldNotExist_InJobWorkflowsModule_WhenDisplayResponsiveReleaseGateUiSettings_IsDisabled()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var filterBizo = new ProcessHeaderFilterBusinessObject();
			AssertNull(filterBizo[componentFilterDescription]);
		}

		public void TestShouldNeverExist_InTransferRules()
		{
			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var filterBizo = new BMFilterRuleFilterBusinessObject();
			AssertNull(filterBizo[componentFilterDescription]);
		}

		protected override void SetUp()
		{
			base.SetUp();

			BMSRegistry.Instance.DisplayResponsiveReleaseGateUiSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}
	}
}
