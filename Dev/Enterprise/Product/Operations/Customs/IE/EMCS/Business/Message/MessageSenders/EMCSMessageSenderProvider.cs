using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.EMCS.Messaging;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public sealed class EMCSMessageSenderProvider : ISendEMCSMessages
	{
		public EMCSMessageSenderProvider(EMCSJobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly EMCSJobDeclaration declaration;

		void ISendEMCSMessages.SendAlertOrRejectEad(AlertOrRejectSendingAction alertOrReject)
		{
			var dataProvider = new IE819MessageHeaderProvider(declaration, alertOrReject);
			Send(alertOrReject, dataProvider, EMCSMessageBuilderLoader.RejectionOfEAD, EMCSOutgoingMessageTypeList.Codes.AlertOrRejectionOfAnEAD);
		}

		void ISendEMCSMessages.SendCancellation(CancellationSendingAction cancellationAction)
		{
			Send(cancellationAction, new IE810MessageHeaderProvider(declaration, cancellationAction), EMCSMessageBuilderLoader.CancellationOfEAD, EMCSOutgoingMessageTypeList.Codes.CancellationOfEAD);
		}

		void ISendEMCSMessages.SendChangeOfDestination(EMCSMessageSendingAction action)
		{
			Send(action, new IE813MessageHeaderProvider(declaration), EMCSMessageBuilderLoader.ChangeOfDestination, EMCSOutgoingMessageTypeList.Codes.ChangeOfDestination);
		}

		void ISendEMCSMessages.SendDeliveryDelayExplanation(ExplanationOnDelaySendingAction explanationOnDelay)
		{
			Send(explanationOnDelay, new IE837MessageHeaderProvider(declaration, explanationOnDelay), EMCSMessageBuilderLoader.ExplanationOnDelayForDelivery, EMCSOutgoingMessageTypeList.Codes.ExplanationOnDelayForDelivery);
		}

		void ISendEMCSMessages.SendDraftMovementRequest(EMCSMessageSendingAction action)
		{
			Send(action, new IE815MessageHeaderProvider(declaration), EMCSMessageBuilderLoader.SubmitDraftEAD, EMCSOutgoingMessageTypeList.Codes.SubmitDraftEAD);
		}

		void ISendEMCSMessages.SendReasonForShortageExplanation(ReasonForShortageSendingAction generalExplanation)
		{
			Send(generalExplanation, new IE871MessageHeaderProvider(declaration, generalExplanation.GeneralExplanation), EMCSMessageBuilderLoader.ExplanationOnReasonForShortage, EMCSOutgoingMessageTypeList.Codes.ExplanationOnReasonForShortage);
		}

		void ISendEMCSMessages.SendReportOfReceipt(ReportOfReceiptSendingAction reportOfReceipt)
		{
			Send(reportOfReceipt, new IE818MessageHeaderProvider(declaration, reportOfReceipt), EMCSMessageBuilderLoader.ReportOfReceipt, EMCSOutgoingMessageTypeList.Codes.ReportOfReceipt);
		}

		void Send(EMCSMessageSendingAction action, IEMCSMessageHeader dataProvider, string messageName, string messageType)
		{
			var messageBuilder = EMCSMessageBuilderLoader.Instance.GetMessageBuilder(messageName, dataProvider);
			var message = declaration.Factory.New<EMCSOutboundEDIMessage>();
			var xmlMessage = messageBuilder.GenerateXmlMessage();
			message.EM_Status = EDIMessage.Status.Queued;
			message.EM_MessageText = action.MessageCreated(xmlMessage.GetSerializedString());
			message.EM_MessageType = messageType;
			message.EM_GP = declaration.CertificateIdentifier?.PK ?? ZGuid.Empty;
			message.EM_LinkedObject = declaration;

			declaration.JE_MessageStatus = EDIMessage.Status.Sent;
			declaration.Messages.Add(message);
		}
	}
}
