using System.Linq;
using CargoWise.Customs.DE.MessageContracts.AES;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class ExportEXQSTAMessageProcessor : ExportMessageProcessor<AesInboundEDIMessage<IEXQSTA>, IEXQSTA>
	{
		public ExportEXQSTAMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("90182B3C-EF12-41B9-96EF-E13C0C3FD4CA", "Export EXQSTA Message Processor");

		protected override bool NeedAttachDocumentsToMessage => true;

		protected override bool NeedAttachDocumentsToLinkedObject => false;

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IEXQSTA> message)
		{
			EDIMessage result = null;
			var provider = message.DataProvider;
			if (provider != null)
			{
				result = GetOriginalMessages(message.Factory, message.DataProvider.ReferencedMessageIdentifier).FirstOrDefault();
				if (result != null)
				{
					result.EM_Status = EDIMessage.Status.Acknowledged;
				}
			}
			return result;
		}

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IEXQSTA> message)
		{
			message.EM_Status = AesEDIMessage.Status.ProcessedOK;
			message.SetLogbookRegistrationNumber(message.DataProvider.MovementReferenceNumber);
		}
	}
}
