using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IETHeaderMeansOfTransportCrossingBorder : IMeansOfTransport
{
	ZString Type { get; }
}
