namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using CargoWise.Types;
	using Enterprise.Customs.CA.Messaging;

	public interface IRNSRequest : ICAEDIFACTMessageAttachee, IRNSRequestData
	{
		void RefreshMessagesForDisplay();
	}

	public interface IRNSRequestData
	{
		ZString HouseBillNumber { get; }
		// DTM
		ZDateTime DateOfArrival { get; }

		// RFF
		ZString CargoControlNumber { get; }
		ZString TransactionNumber { get; }

		// LOC
		ZString OfficeCode { get; }
		ZString SubLocationCode { get; }
	}
}
