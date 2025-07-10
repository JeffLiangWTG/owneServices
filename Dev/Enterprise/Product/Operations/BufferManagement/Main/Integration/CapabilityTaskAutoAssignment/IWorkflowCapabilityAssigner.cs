using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IWorkflowCapabilityAssigner
	{
		void AutoAssignWorkflowsImmediatelyOrDelayed(BusinessObjectFactory factory, IEnumerable<Guid> workflowPKs, ILogger logger);
	}
}
