using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration;

namespace Enterprise.BufferManagement.Business
{
	public abstract class ReleaseGateServiceTaskProcessor : BMServiceTaskProcessor
	{
		protected ReleaseGateServiceTaskProcessor(ILogger logger, ServiceTaskFactoryProviderWrapper factoryProvider)
			: base(logger, factoryProvider)
		{
		}

		public IEnumerable<ZGuid> WorkflowPKs { get; set; }
	}
}
