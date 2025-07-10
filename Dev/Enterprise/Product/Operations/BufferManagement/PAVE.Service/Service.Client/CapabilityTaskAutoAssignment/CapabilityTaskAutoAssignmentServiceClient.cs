using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Service.Client
{
	public class CapabilityTaskAutoAssignmentServiceClient : SimplePAVEServiceClient, ICapabilityTaskAutoAssignmentService
	{
		public void AutoAssignCapabilityTasks(IEnumerable<Guid> workflowPKs, ILogger logger)
		{
			Process(workflowPKs, logger);
		}

		protected override void ProcessCore(IReadOnlyCollection<Guid> workflowPKs, ILogger logger)
		{
			ObjectFactory.Get<ICapabilityTaskAutoAssignmentService>().AutoAssignCapabilityTasks(workflowPKs, logger);
		}
	}
}
