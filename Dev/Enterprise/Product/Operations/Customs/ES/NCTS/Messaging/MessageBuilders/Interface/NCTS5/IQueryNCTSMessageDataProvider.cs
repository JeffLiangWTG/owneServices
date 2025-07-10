namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface IQueryNCTSMessageDataProvider : INCTSCommonDataProvider
	{
		INCTSCommonTransitOperationMRN TransitOperation { get; }
	}
}
