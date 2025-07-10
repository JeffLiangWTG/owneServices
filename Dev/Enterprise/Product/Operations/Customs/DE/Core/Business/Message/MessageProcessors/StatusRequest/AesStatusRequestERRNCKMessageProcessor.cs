using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business
{
	public class AesStatusRequestERRNCKMessageProcessor : DEBranchCustomsApplicationTypeMessageProcessor<AesInboundEDIMessage<IERRNCK>>
	{
		public AesStatusRequestERRNCKMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => (NoResString)"AES StatusRequest ERRNCK Message Processor";

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.DECustomsAesSystem;

		protected override List<AttachedDocument> GetAttachedDocuments(AesInboundEDIMessage<IERRNCK> message) => message.AttachedDocuments;

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject)
		{
			var result = ZGuid.Invalid;
			if (linkedObject is StatusRequest statusRequest)
			{
				result = statusRequest.EM_GB;
			}
			return result;
		}

		protected override BusinessObject GetLinkedObject(AesInboundEDIMessage<IERRNCK> message) => GetOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier ?? ZString.Empty);

		protected override ZString GetMessageIdentifier(AesInboundEDIMessage<IERRNCK> message) => message.DataProvider?.MessageIdentifier ?? ZString.Empty;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AesInboundEDIMessage<IERRNCK> message)
		{
			var statusRequest = (StatusRequest)message.EM_LinkedObject;
			statusRequest.EM_Status = EDIMessage.Status.Error;
			var dataProvider = message.DataProvider;

			var logbookRegistrationNumber = !dataProvider.ReferenceNumber.IsNullOrEmpty() ? dataProvider.ReferenceNumber : dataProvider.ReferencedMessageIdentifier;
			message.SetLogbookRegistrationNumber(logbookRegistrationNumber);
			message.EM_Status = EDIMessage.Status.ProcessedOK;
		}
	}
}
