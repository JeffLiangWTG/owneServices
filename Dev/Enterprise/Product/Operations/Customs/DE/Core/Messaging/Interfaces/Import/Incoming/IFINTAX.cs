using CargoWise.Customs.DE.MessageContracts;

namespace Enterprise.Customs.DE.Messaging
{
	public interface IFINTAX : IDataProvider
	{
		string ReferenceNumber { get; }

		string MRN { get; }

		string LocalReferenceNumber { get; }
	}
}
