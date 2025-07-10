using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface ICUSREV : IDataProvider
	{
		ZString CancelledReferenceNumber { get; }

		ZString ReferenceNumber { get; }

		ZString Reason { get; }

		string MRN { get; }

		string CancelledMRN { get; }
	}
}
