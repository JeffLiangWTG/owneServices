namespace Enterprise.Customs.DE.Messaging
{
	public interface IInterchangeCustomsDataProvider
	{
		string MessageSubType { get; }

		string MessageType { get; }

		string InterchangeRecipientEORIBranch { get; }

		string LocalReferenceNumber { get; }
	}
}
