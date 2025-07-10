using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public sealed class EMCSMessageSenderProvider : ISendGBEMCSMessages
	{
		public EMCSMessageSenderProvider(EMCSJobDeclaration declaration)
		{
			this.declaration = declaration;
		}
		readonly EMCSJobDeclaration declaration;

		void ISendEMCSMessages.SendAlertOrRejectEad(AlertOrRejectSendingAction alertOrReject)
		{
			var dataProvider = new IE819MessageHeaderProvider(declaration, alertOrReject);
			Send(dataProvider, EMCSMessageBuilderLoader.RejectionOfEAD, EMCSGBOutgoingMessageTypeList.Codes.AlertOrRejectionOfAnEAD);
		}

		void ISendEMCSMessages.SendCancellation(CancellationSendingAction cancellation)
		{
			Send(new IE810MessageHeaderProvider(declaration, cancellation), EMCSMessageBuilderLoader.CancellationOfEAD, EMCSGBOutgoingMessageTypeList.Codes.CancellationOfEAD);
		}

		void ISendEMCSMessages.SendChangeOfDestination(EMCSMessageSendingAction action)
		{
			Send(new IE813MessageHeaderProvider(declaration), EMCSMessageBuilderLoader.ChangeOfDestination, EMCSGBOutgoingMessageTypeList.Codes.ChangeOfDestination);
		}

		void ISendEMCSMessages.SendDeliveryDelayExplanation(ExplanationOnDelaySendingAction explanationOnDelay)
		{
			Send(new IE837MessageHeaderProvider(declaration, explanationOnDelay), EMCSMessageBuilderLoader.ExplanationOnDelayForDelivery, EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnDelayForDelivery);
		}

		void ISendEMCSMessages.SendDraftMovementRequest(EMCSMessageSendingAction action)
		{
			Send(new IE815MessageHeaderProvider(declaration), EMCSMessageBuilderLoader.SubmitDraftEAD, EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD);
		}

		void ISendEMCSMessages.SendReasonForShortageExplanation(ReasonForShortageSendingAction generalExplanation)
		{
			Send(new IE871MessageHeaderProvider(declaration, generalExplanation.GeneralExplanation), EMCSMessageBuilderLoader.ExplanationOnReasonForShortage, EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnReasonForShortage);
		}

		void ISendEMCSMessages.SendReportOfReceipt(ReportOfReceiptSendingAction reportOfReceipt)
		{
			Send(new IE818MessageHeaderProvider(declaration, reportOfReceipt), EMCSMessageBuilderLoader.ReportOfReceipt, EMCSGBOutgoingMessageTypeList.Codes.ReportOfReceipt);
		}

		void ISendGBEMCSMessages.SendSplitting()
		{
			Send(new IE825MessageHeaderProvider(declaration), EMCSMessageBuilderLoader.Splitting, EMCSGBOutgoingMessageTypeList.Codes.Splitting);
		}

		void Send(IEMCSMessageHeader dataProvider, string messageName, string messageType)
		{
			var messageBuilder = EMCSMessageBuilderLoader.Instance.GetMessageBuilder(messageName, dataProvider);
			var xmlMessage = messageBuilder.GenerateXmlMessage();
			var message = declaration.Factory.New<EMCSOutboundEDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.SetEM_MessageTextOrDataSource(xmlMessage.GetSerializedStream());
			message.EM_MessageType = messageType;
			message.EM_LinkedObject = declaration;
			message.EM_GB = declaration.JE_GB;
			message.EM_MessageOwner = declaration.JE_CustomsProfile.SubstringSafe(0, EDIMessage.Schema.EM_MessageOwner.Length);

			declaration.JE_MessageStatus = EDIMessage.Status.Sent;
			declaration.Messages.Add(message);
		}
	}
}
