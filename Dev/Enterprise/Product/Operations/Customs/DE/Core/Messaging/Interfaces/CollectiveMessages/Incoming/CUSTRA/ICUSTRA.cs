using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSTRA : IDataProvider
	{
		ZString ReferenceNumber { get; }
		ZDate ForwardedDate { get; }
		ZString Reason { get; }
		string MRN { get; }
	}
}
