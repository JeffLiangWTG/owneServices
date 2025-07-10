namespace Enterprise.Messaging.Business.MessageProcessor
{
	public interface IUniversalCustomsMessageProcessorWithAdditionalErrorReportDetails : IUniversalCustomsMessageProcessor
	{
		string GetAdditionalErrorReportDetails(EDIMessage message);
	}
}
