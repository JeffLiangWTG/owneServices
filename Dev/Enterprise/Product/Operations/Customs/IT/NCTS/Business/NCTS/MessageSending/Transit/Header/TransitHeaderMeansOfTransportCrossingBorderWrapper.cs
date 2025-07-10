using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TransitHeaderMeansOfTransportCrossingBorderWrapper : SADMeansOfTransportWrapper, IETHeaderMeansOfTransportCrossingBorder
{
	public TransitHeaderMeansOfTransportCrossingBorderWrapper(ZString nationality, ZString identity) : base(nationality, identity)
	{
	}

	public ZString Type => ZString.Empty;
}
