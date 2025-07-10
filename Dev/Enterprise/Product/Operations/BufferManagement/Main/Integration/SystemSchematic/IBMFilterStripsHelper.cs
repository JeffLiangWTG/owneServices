using System;
using System.Collections.Generic;
using CargoWise.Schema;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMFilterStripsHelper : IFilterStripsHelper
	{
		void SetOverriddenSupportedWorkflowTypes(IEnumerable<string> workflowTypes);
		SchemaColumn SubColumnOverride { get; set; }
		Type BusinessObjectTypeOverride { get; set; }
	}
}
