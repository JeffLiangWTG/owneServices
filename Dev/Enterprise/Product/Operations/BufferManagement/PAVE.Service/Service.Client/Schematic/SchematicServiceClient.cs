using System;
using System.Collections.Generic;
using CargoWise.Application;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Service.Client
{
	public class SchematicServiceClient : SimplePAVEServiceClient, ISchematicService
	{
		public void ProcessTransferRules(IEnumerable<Guid> transferablePKs, ILogger logger)
		{
			Process(transferablePKs, logger);
		}

		protected override void ProcessCore(IReadOnlyCollection<Guid> transferablePKs, ILogger logger)
		{
			ObjectFactory.Get<ISchematicService>().ProcessTransferRules(transferablePKs, logger);
		}
	}
}
