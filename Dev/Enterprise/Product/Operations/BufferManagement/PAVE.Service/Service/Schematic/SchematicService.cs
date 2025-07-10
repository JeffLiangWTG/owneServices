using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Enterprise.BufferManagement.Service.Shared;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Service
{
	public class SchematicService : PAVEService, ISchematicService
	{
		public void ProcessTransferRules(IEnumerable<Guid> transferablePKs, ILogger logger)
		{
			Process(transferablePKs, logger);
		}

		protected override void ProcessCore(IEnumerable<Guid> transferablePKs, ILogger logger)
		{
			var processor = GetNewResponsiveTransferRuleProcessor(logger);
			processor.ProcessTransferables(transferablePKs.ToImmutableArray());
		}

		protected internal virtual ResponsiveTransferRuleProcessor GetNewResponsiveTransferRuleProcessor(ILogger logger)
		{
			return new ResponsiveTransferRuleProcessor(logger);
		}
	}
}
