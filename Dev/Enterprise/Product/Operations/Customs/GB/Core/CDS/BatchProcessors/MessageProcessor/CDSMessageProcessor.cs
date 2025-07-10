using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.CDS
{
	public abstract class CDSMessageProcessor<TCDSEDIMessage> : ApplicationTypeMessageProcessor
	where TCDSEDIMessage : CDSEDIMessage
	{
		protected CDSMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected sealed override void ProcessMessageCore(EDIMessage message)
		{
			if (message is TCDSEDIMessage cdsEDIMessage)
			{
				cdsEDIMessage.EM_Status = ProcessMessageCore(cdsEDIMessage, cdsEDIMessage.Factory);
			}
		}

		protected abstract ZString ProcessMessageCore(TCDSEDIMessage cdsEDIMessage, BusinessObjectFactory factory);

		protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;

		protected sealed override IReadOnlyList<ZString> MessageTypesToIncludeCore => new[] { MessageType };

		protected abstract ZString MessageType { get; }
	}
}
