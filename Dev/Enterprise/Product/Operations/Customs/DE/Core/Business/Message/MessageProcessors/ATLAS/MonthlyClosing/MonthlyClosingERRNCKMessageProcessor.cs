using System.Text;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.DE.Business.MonthlyClosing
{
	public class MonthlyClosingERRNCKMessageProcessor : MonthlyClosingMessageProcessor<AtlasInboundEDIMessage<IERRNCK>, IERRNCK>
	{
		public MonthlyClosingERRNCKMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("09334A63-4C15-49D8-BD67-145C0B4AF079", "Monthly Closing ERRNCK Message Processor");

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<IERRNCK> message) => GetLinkedObjectFromOriginalMessage(message.Factory, message.DataProvider?.ReferencedMessageIdentifier);

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<IERRNCK> message)
		{
			var declaration = (CusReconDeclaration)message.EM_LinkedObject;
			message.EM_Status = EDIMessage.Status.ProcessedOK;
			declaration.CRD_MessageStatus = EDIMessageStatusList.Codes.Rejected;
			if (declaration.CRD_CustomsStatus.IsEmpty)
			{
				declaration.CRD_CustomsStatus = UniversalReferenceConstants.EntryStatus.ERR;
			}

			var logbookRegistrationNumber = !message.DataProvider.ReferenceNumber.IsNullOrEmpty() ? message.DataProvider.ReferenceNumber : message.DataProvider.ReferencedMessageIdentifier;
			message.SetLogbookRegistrationNumber(logbookRegistrationNumber);

			GenerateHtmlEmailAndSendToOriginalOrGroup(factory
				, declaration
				, Res.GetString("600AE450-4E7E-4461-839C-EF81EF21CF44", "Monthly Closing Declaration Status")
				, GetEmailBody()
				, false
				, message.Branch
				, declaration
				, () => declaration.Messages.LastSentOutgoingMessage);

			ZString GetEmailBody()
			{
				var htmlBody = new StringBuilder();
				htmlBody.Append(Res.GetString("55106B3D-DAF1-4369-8D97-A181A4C74EF9", @"Your Monthly Closing Declaration for Job {0} has been rejected. For details please follow the Link to the Job.", declaration.CRD_JobReferenceNumber));
				htmlBody.Append("<br /><br />");
				htmlBody.Append(GetErrorsEmailTable(message.DataProvider.Errors));
				return htmlBody.ToString();
			}
		}
	}
}
