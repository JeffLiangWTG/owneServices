using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;
using Moq;

namespace Enterprise.BufferManagement.Service.Test
{
	public class CapabilityTaskAutoAssignmentServiceTest : SimplePAVEServiceTestCase
	{
		public void TestAutoAssignCapabilityTasks()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var dummyWorkflowPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

			var assignerIsCalled = false;
			var assignerMock = new Mock<IWorkflowCapabilityAssigner>();
			assignerMock.Setup(l => l.AutoAssignWorkflowsImmediatelyOrDelayed(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<ILogger>()))
				.Callback((BusinessObjectFactory factory, IEnumerable<Guid> workflowPKs, ILogger logger) =>
				{
					assignerIsCalled = true;
					AssertContainsExactElementsInAnyOrder(dummyWorkflowPKs, workflowPKs);
				});

			ObjectFactory.Substitute(assignerMock.Object);

			Assert("Precondition: Capacity Calculations should not be disabled in the registry", !BMSRegistry.Instance.DisableCapacityCalculations.Value);

			var service = new CapabilityTaskAutoAssignmentService();
			service.AutoAssignCapabilityTasks(dummyWorkflowPKs, loggerMock.Object);

			Assert(assignerIsCalled);
		}

		public void TestAutoAssignCapabilityTasks_WhenCapacityCalculationDisabled_ShouldNotExecute_WithLogMessage()
		{
			BMSTestHelper.EnableBMSInRegistry();
			BMSRegistry.Instance.EnableResponsivePAVEDataProcessing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			BMSRegistry.Instance.DisableCapacityCalculations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var dummyWorkflowPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };

			var assignerIsCalled = false;

			var assignerMock = new Mock<IWorkflowCapabilityAssigner>();
			assignerMock.Setup(l => l.AutoAssignWorkflowsImmediatelyOrDelayed(It.IsAny<BusinessObjectFactory>(), It.IsAny<IEnumerable<Guid>>(), It.IsAny<ILogger>()))
				.Callback((BusinessObjectFactory factory, IEnumerable<Guid> workflowPKs, ILogger logger) =>
				{
					assignerIsCalled = true;
					AssertContainsExactElementsInAnyOrder(dummyWorkflowPKs, workflowPKs);
				});

			ObjectFactory.Substitute(assignerMock.Object);

			Assert("Precondition: Capacity Calculations should be disabled in the registry", BMSRegistry.Instance.DisableCapacityCalculations.Value);

			var service = new CapabilityTaskAutoAssignmentService();
			service.AutoAssignCapabilityTasks(dummyWorkflowPKs, loggerMock.Object);

			Assert(!assignerIsCalled);
			AssertEquals((LogType.Information, "Capability Task Auto-Assignment not run because the [Disable Capacity Calculations] registry item is enabled."), logs.Single());
			AssertNotEquals("Capacity was calculated even though the DisableCapacityCalculations registry item was enabled.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#region Test Case Overrides

		protected override IPAVEService GetService() => new CapabilityTaskAutoAssignmentService();

		protected override void CallServiceMethod(IPAVEService service, IReadOnlyCollection<Guid> processedPKs, ILogger logger) => ((CapabilityTaskAutoAssignmentService)service).AutoAssignCapabilityTasks(processedPKs, logger);

		protected override string ExpectedClassNameInLogs => "CapabilityTaskAutoAssignmentService";

		protected override string ExpectedMethodNameInLogs => "AutoAssignCapabilityTasks";

		#endregion
	}
}
