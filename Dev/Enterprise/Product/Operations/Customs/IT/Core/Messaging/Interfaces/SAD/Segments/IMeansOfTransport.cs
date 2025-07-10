using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IMeansOfTransport
{
	ZString Nationality { get; }

	ZString Identity { get; }
}
