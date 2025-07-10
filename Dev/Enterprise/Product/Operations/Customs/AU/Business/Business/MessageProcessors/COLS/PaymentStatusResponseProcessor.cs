using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	sealed class PaymentStatusResponseProcessor : COLSMessageProcessor<QuarantineColsHeader, PaymentStatusResponse>
	{
		public PaymentStatusResponseProcessor(LoggingInformation logger)
			: base(logger, AUCOLSMessageTypeList.Codes.PaymentStatus, AUCOLSMessageTypeList.Descriptions.PaymentStatus)
		{
		}

		protected override string ProcessCore(QuarantineColsHeader header, PaymentStatusResponse messageData, EDIMessage message, EDIMessage originalMessage)
		{
			header.QCH_MessageStatus = ResponseSuccess.Equals(messageData.result) ? COLSHeaderStatusList.Codes.SuccessfulGetPaymentStatus : COLSHeaderStatusList.Codes.FailedGetPaymentStatus;
			return EDIMessage.Status.Received;
		}
	}
}
