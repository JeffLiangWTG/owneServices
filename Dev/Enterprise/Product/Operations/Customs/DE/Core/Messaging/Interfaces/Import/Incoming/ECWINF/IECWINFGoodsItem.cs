using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.Messaging
{
	public interface IECWINFGoodsItem : IInboundProvider
	{
		ZString ReferencedRegistrationNumber { get; }

		string MRN { get; }
	}
}
