namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface IAmendmentNCTSMessageDataProvider : IDepartureNCTSCommonMessageDataProvider
	{
		IAmendmentNCTSTransitOperation TransitOperation { get; }
	}

	public interface IAmendmentNCTSTransitOperation : INCTSCommonTransitOperationMRN
	{
		INCTSCommonCompleteTransitOperation CommonTransitOperation { get; }
	}
}
