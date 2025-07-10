using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;
using Moq;

namespace Enterprise.BufferManagement.Service.Client.Test
{
	public class CapabilityTaskAutoAssignmentServiceClientTest : PAVEServiceClientTestCase
	{
		public void TestAutoAssignCapabilityTasks_ShouldCallCapabilityTaskAutoAssignmentServiceDirectly()
		{
			var serviceMock = new Mock<ICapabilityTaskAutoAssignmentService>();

			IEnumerable<Guid> workflowPKsProcessed = null;

			serviceMock.Setup(m => m.AutoAssignCapabilityTasks(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<ILogger>()))
				.Callback((IEnumerable<Guid> workflowPKs, ILogger logger) =>
				{
					workflowPKsProcessed = workflowPKs;
					AssertEquals(loggerMock.Object, logger);
				})
				.Verifiable();

			ObjectFactory.Substitute(serviceMock.Object);

			var toProcessPKs = new Guid[] { Guid.NewGuid(), Guid.NewGuid() };
			GetClient().Process(toProcessPKs, loggerMock.Object);

			serviceMock.Verify(m => m.AutoAssignCapabilityTasks(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<ILogger>()), Times.Once);
			AssertContainsExactElementsInAnyOrder(toProcessPKs, workflowPKsProcessed);
		}

		#region Test Case Overrides

		protected override string ExpectedClassNameInLogs => nameof(CapabilityTaskAutoAssignmentServiceClient);

		protected override IPAVEService GetClient() => new CapabilityTaskAutoAssignmentServiceClient();

		#endregion

	}
}
