using System;
using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface ICapabilityTaskAutoAssignmentService
	{
		void AutoAssignCapabilityTasks(IEnumerable<Guid> workflowPKs, ILogger logger);
	}
}
