using CargoWise.Common;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class ExitControlMessageBuilderManager
	{
		public ExitControlMessageBuilderManager(ExitControlMessageSendingObject exitHeaderMessageSendingObject, ICertificateProvider certificate)
		{
			objectToSend = Argument.NotNull(exitHeaderMessageSendingObject, "exitHeaderMessageSendingObject cannot be null");
			certificateData = Argument.NotNull(certificate, "certificate cannot be null");
		}

		readonly ExitControlMessageSendingObject objectToSend;
		readonly ICertificateProvider certificateData;

		public IMessageBuilderBase NewMessageBuilder()
		{
			var messageType = DeclarationMessageTypeList.Codes.ArrivalAtExitUcc6;
			var messageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
			var exitReport = objectToSend.MessagingObject;

			return new EALAESMessageBuilder(new EALAESSendMessageWrapper(exitReport, certificateData), messageType, messageSubType);
		}
	}
}
