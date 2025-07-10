using CargoWise.Common;
using CargoWise.Customs.CN.MessageDefinitions.ACDA;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Business
{
	public class InboundMessageCreator : IInboundMessageCreator
	{
		public InboundMessageCreator(ILoggingInformation logger)
		{
			LoggingInformation = logger;
		}

		ILoggingInformation LoggingInformation { get; }

		void IInboundMessageCreator.CreateMessagesForInterchange(EDIInterchange interchange)
		{
			var interchangeText = interchange.EI_BodyText;
			if (!interchangeText.IsEmpty && XmlUtils.IsValidXml(interchangeText, out var document) && document.DocumentElement.Name == nameof(ImportAgrResponse))
			{
				var newMessage = interchange.ContainedMessages.AddNew();
				newMessage.EM_ApplicationCode = GenericMessageDeliveryInterchangeTypeList.Codes.CNSingleWindow;
				newMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				newMessage.EM_MessageNum = interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength);
				newMessage.EM_MessageType = MessageTypeList.Codes.ImportAgreementResponse;
				newMessage.EM_MessageText = interchange.EI_BodyText;
				newMessage.EM_Status = EDIMessage.Status.Queued;
			}
			else
			{
				var invalidXmlMessage = (NoResString)"Invalid xml content or root element.";
				LoggingInformation.LogWarning(invalidXmlMessage);
				interchange.Logs.AddNew(Events.ErrorReport, invalidXmlMessage);
				interchange.EI_Status = EDIInterchangeStatusList.Codes.Error;
			}
		}
	}
}
