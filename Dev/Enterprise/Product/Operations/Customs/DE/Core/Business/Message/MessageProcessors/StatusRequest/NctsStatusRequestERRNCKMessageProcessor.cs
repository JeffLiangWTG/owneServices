using System.Collections.Generic;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class NctsStatusRequestERRNCKMessageProcessor : DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<IERRNCK>>
	{
		public NctsStatusRequestERRNCKMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"Ncts StatusRequest ERRNCK Message Processor";

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.DECustomsAtlasSystem;

		protected override List<AttachedDocument> GetAttachedDocuments(AtlasInboundEDIMessage<IERRNCK> message) => message.AttachedDocuments;

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject)
		{
			var result = ZGuid.Invalid;
			if (linkedObject is StatusRequest statusRequest)
			{
				result = statusRequest.EM_GB;
			}
			return result;
		}

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IERRNCK> message) => GetOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier ?? ZString.Empty);

		protected override ZString GetMessageIdentifier(AtlasInboundEDIMessage<IERRNCK> message) => message.DataProvider?.MessageIdentifier ?? ZString.Empty;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IERRNCK> message)
		{
			var statusRequest = (StatusRequest)message.EM_LinkedObject;
			statusRequest.EM_Status = EDIMessage.Status.Error;
			var dataProvider = message.DataProvider;
			message.SetLogbookRegistrationNumber(dataProvider.ReferenceNumber ?? dataProvider.ReferencedMessageIdentifier);
			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}
	}
}
