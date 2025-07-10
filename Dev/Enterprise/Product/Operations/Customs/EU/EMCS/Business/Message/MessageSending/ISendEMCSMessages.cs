namespace Enterprise.Customs.EU.EMCS.Business
{
	public interface ISendEMCSMessages
	{
		void SendDraftMovementRequest(EMCSMessageSendingAction action);

		void SendCancellation(CancellationSendingAction cancellation);

		void SendChangeOfDestination(EMCSMessageSendingAction action);

		void SendDeliveryDelayExplanation(ExplanationOnDelaySendingAction explanationOnDelay);

		void SendReasonForShortageExplanation(ReasonForShortageSendingAction generalExplanation);

		void SendReportOfReceipt(ReportOfReceiptSendingAction reportOfReceipt);

		void SendAlertOrRejectEad(AlertOrRejectSendingAction alertOrReject);
	}
}
