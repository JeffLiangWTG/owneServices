using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business.Test
{
	public interface IWorkflowCapabilityAssignerTestCase
	{
		BusinessObjectFactory Factory { get; }

		BMSystem System { get; }

		ILogger RunAutoAssignmentAndGetLog(BMComponent buffer, ZGuid workflowPK, Action<ProcessHeader[]> preassignmentAction = null);

		ILogger RunAutoAssignmentAndGetLog(BMComponent buffer, IEnumerable<ZGuid> workflowPKs, Action<ProcessHeader[]> preassignmentAction = null);
	}
}
