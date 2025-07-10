using System.Linq;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsTRQSTAMessageProcessor : NctsMessageProcessor<AtlasInboundEDIMessage<ITRQSTA>, ITRQSTA>
	{
		public NctsTRQSTAMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("19B3D4CA-5D48-4CF6-BA16-EDE03CE56236", "NCTS TRQSTA Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ITRQSTA> message)
		{
			EDIMessage result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				result = GetOriginalMessages(message.Factory, provider.ReferencedMessageIdentifier).FirstOrDefault();
				if (result != null)
				{
					result.EM_Status = EDIMessage.Status.Acknowledged;
				}
			}
			return result;
		}

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject)
		{
			var result = ZGuid.Invalid;
			if (linkedObject is EDIMessage message)
			{
				result = message.EM_GB;
			}
			return result;
		}

		protected override bool NeedAttachDocumentsToMessage => true;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ITRQSTA> message)
		{
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			message.SetLogbookRegistrationNumber(message.DataProvider.MovementReferenceNumber);
		}

		protected override bool NeedAttachDocumentsToLinkedObject => false;
	}
}
