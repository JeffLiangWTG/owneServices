using System;
using System.Collections.Generic;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface ISchematicService
	{
		void ProcessTransferRules(IEnumerable<Guid> transferablePKs, ILogger logger);
	}
}
