using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;

namespace Enterprise.UniversalDataBuss.ServiceTasks
{
	class GrEngineMessageProcessor : UniversalMessageProcessor
	{
		public GrEngineMessageProcessor(LoggingInformation logger, IEnumerable<string> messageSubTypes, IFactoryService factoryService) : base(logger, messageSubTypes, factoryService)
		{
		}

		/// <summary>
		/// We are overriding this in GrEngine so that messages normally processed by the USI service task get processed by the GrEngine.
		/// This means we can have a faster query on big systems.
		/// </summary>
		protected override ZQuery AddSubTypesToQuery(ZQuery query) => query;
	}
}
