using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADMeansOfTransportWrapper : IMeansOfTransport
{
	public SADMeansOfTransportWrapper(ZString nationality, ZString identity)
	{
		Nationality = nationality;
		Identity = identity;
	}

	public ZString Nationality { get; }

	public ZString Identity { get; }
}
