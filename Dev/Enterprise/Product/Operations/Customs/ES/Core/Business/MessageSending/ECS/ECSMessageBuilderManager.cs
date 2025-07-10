using CargoWise.Common;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageSending
{
	public class ECSMessageBuilderManager
	{
		public ECSMessageBuilderManager(ECSExitHeaderMessageSendingObject exitHeaderMessageSendingObject, ICertificateProvider certificate)
		{
			objectToSend = Argument.NotNull(exitHeaderMessageSendingObject, "exitHeaderMessageSendingObject cannot be null");
			certificateData = Argument.NotNull(certificate, "certificate cannot be null");
		}
		readonly ECSExitHeaderMessageSendingObject objectToSend;
		readonly ICertificateProvider certificateData;

		public IMessageBuilderBase NewMessageBuilder()
		{
			var messageType = DeclarationMessageTypeList.Codes.ArrivalAtExit;
			var messageSubType = DeclarationMessageSubTypeList.Codes.OriginalDeclaration;
			var exitDetail = objectToSend.ExitDetail;

			return new ArrivalAtExitExportMessageBuilder(new ArrivalAtExitSendMessageWrapper(exitDetail, certificateData), messageType, messageSubType);
		}
	}
}
