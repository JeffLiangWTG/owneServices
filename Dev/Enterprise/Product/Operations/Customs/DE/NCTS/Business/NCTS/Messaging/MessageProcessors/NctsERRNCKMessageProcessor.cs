using System.Collections.Generic;
using System.Text;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsERRNCKMessageProcessor : NctsMessageProcessor<AtlasInboundEDIMessage<IERRNCK>, IERRNCK>
	{
		public NctsERRNCKMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("715AA85C-F09B-4174-8527-B8B261D941B5", "NCTS ERRNCK Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IERRNCK> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IERRNCK> message)
		{
			var dataProvider = message.DataProvider;

			message.EM_Status = EDIMessage.Status.ProcessedOK;
			var logbookRegistrationNumber = !dataProvider.ReferenceNumber.IsNullOrEmpty() ? dataProvider.ReferenceNumber : dataProvider.ReferencedMessageIdentifier;
			message.SetLogbookRegistrationNumber(logbookRegistrationNumber);
			message.SetLogbookLocalReferenceNumber(dataProvider.LocalReferenceNumber);

			NctsHeader header;
			if (message.EM_LinkedObject is NctsDepartureMovementHeader movementHeader)
			{
				header = movementHeader.Header;
				movementHeader.BM_Phase = NctsMovementHeaderTransactionStatusList.Codes.Declaration;
				movementHeader.BM_MessageStatus = LogicalStatusList.Codes.Error;

				movementHeader.GuaranteeTransactionCoordinator.DeleteTransactions();
			}
			else
			{
				header = (NctsHeader)message.EM_LinkedObject;
				header.EffectiveMessageStatus = LogicalStatusList.Codes.Error;
			}

			var emailBody = GetErrorEmailBody(header.BH_JobReference, dataProvider.Errors);
			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory, header,
				Res.GetString("E8EC1B83-6956-4E68-BBF1-4CAE5F1522EF", "NCTS Declaration Message Status"),
				emailBody, isFailure: false, message.Branch, header, dataProvider.ReferencedMessageIdentifier);
		}

		ZString GetErrorEmailBody(ZString messageReference, IEnumerable<IERRNCKError> errors)
		{
			var emailBody = new StringBuilder();
			emailBody.Append(ZString.Format(Res.GetString("9176AEEF-45D5-4417-B072-A7B20DA54D0A", @"Your NCTS Declaration Message for Job {0} has been rejected.
For details please follow the Link to the Job 
Shown below is a summary of relevant information received in the message
", messageReference)) + " <br />");
			emailBody.Append("<br />");
			emailBody.Append(GetErrorsEmailTable(errors));
			return emailBody.ToString();
		}
	}
}
