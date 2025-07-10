using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.EMCS.Business
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
			Send(new ED819MessageHeaderProvider(declaration, alertOrReject), EmcsMessageBuilderLoader.RejectionOfEAD, Messaging.EmcsMessageSubTypeList.Codes.Emb);
		}

		void ISendEMCSMessages.SendCancellation(CancellationSendingAction cancellation)
		{
			Send(new ED810MessageHeaderProvider(declaration, cancellation), EmcsMessageBuilderLoader.CancellationOfEAD, Messaging.EmcsMessageSubTypeList.Codes.Eme);
		}

		void ISendEMCSMessages.SendChangeOfDestination(EMCSMessageSendingAction action)
		{
			Send(new ED813MessageHeaderProvider(declaration), EmcsMessageBuilderLoader.ChangeOfDestination, Messaging.EmcsMessageSubTypeList.Codes.Eme);
		}

		void ISendEMCSMessages.SendDeliveryDelayExplanation(ExplanationOnDelaySendingAction explanationOnDelay)
		{
			var messageSubType = declaration.IsConsignor ? Messaging.EmcsMessageSubTypeList.Codes.Eme : Messaging.EmcsMessageSubTypeList.Codes.Emb;
			Send(new ED837MessageHeaderProvider(declaration, explanationOnDelay), EmcsMessageBuilderLoader.ExplanationOnDelay, messageSubType);
		}

		void ISendEMCSMessages.SendDraftMovementRequest(EMCSMessageSendingAction action)
		{
			var dataProvider = new ED815MessageHeaderProvider(declaration);
			Send(dataProvider, EmcsMessageBuilderLoader.SubmittedDraftEAD, Messaging.EmcsMessageSubTypeList.Codes.Eme, ((IED815Header)dataProvider.Header).LocalReferenceNumber);
		}

		void ISendEMCSMessages.SendReasonForShortageExplanation(ReasonForShortageSendingAction reasonForShortage)
		{
			var messageSubType = declaration.IsConsignor ? Messaging.EmcsMessageSubTypeList.Codes.Eme : Messaging.EmcsMessageSubTypeList.Codes.Emb;
			Send(new ED871MessageHeaderProvider(declaration, reasonForShortage.GeneralExplanation), EmcsMessageBuilderLoader.ExplanationForShortage, messageSubType);
		}

		void ISendEMCSMessages.SendReportOfReceipt(ReportOfReceiptSendingAction reportOfReceipt)
		{
			Send(new ED818MessageHeaderProvider(declaration, reportOfReceipt), EmcsMessageBuilderLoader.ReportOfReceipt, Messaging.EmcsMessageSubTypeList.Codes.Emb);
		}

		void Send(IEMCSMessageHeader dataProvider, string messageName, string messageSubType, string logbookLocalReferenceNumber = "")
		{
			var messageBuilder = EmcsMessageBuilderLoader.Instance.GetMessageBuilder(messageName, dataProvider);
			var message = declaration.Factory.New<EmcsEDIMessage>();
			message.EM_Status = EDIMessage.Status.Queued;
			message.SetEM_MessageTextOrDataSource(messageBuilder.GetXMLMessage());
			message.EM_ApplicationReference = messageBuilder.MessageTechnicalName;
			message.EM_MessageSubType = messageSubType;

			message.SetLogbookLocalReferenceNumber(logbookLocalReferenceNumber);
			message.SetLogbookRegistrationNumber(dataProvider.Header.AdministrativeReferenceCode);

			declaration.JE_MessageStatus = EDIMessage.Status.Sent;
			declaration.Messages.Add(message);
		}
	}
}
