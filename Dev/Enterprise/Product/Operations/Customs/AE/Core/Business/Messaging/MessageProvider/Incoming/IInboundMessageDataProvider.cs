using CargoWise.Types;

namespace Enterprise.Customs.AE.Business;

public interface IInboundMessageDataProvider
{
	ZString OutgoingAccessReference { get; }
}
