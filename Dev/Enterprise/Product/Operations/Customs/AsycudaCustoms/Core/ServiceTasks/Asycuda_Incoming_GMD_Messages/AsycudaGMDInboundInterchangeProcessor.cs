using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AsycudaCustoms.ServiceTasks
{
	class AsycudaGMDInboundInterchangeProcessor : GMDInboundInterchangeProcessor
	{
		public AsycudaGMDInboundInterchangeProcessor(IEnumerable<ZString> interchangeTypes, ILogger serviceTaskLogger)
			: base(interchangeTypes)
		{
			serviceLogger = serviceTaskLogger;
		}

		protected override bool AddNoteOnException => true;

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return new AsycudaInboundMessageCreator(serviceLogger);
		}

		readonly ILogger serviceLogger;
	}
}
