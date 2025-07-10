using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class ETHeaderMeansOfTransportCrossingBorderWrapper : SADMeansOfTransportWrapper, IETHeaderMeansOfTransportCrossingBorder
{
	public ETHeaderMeansOfTransportCrossingBorderWrapper(ZString nationality, ZString identity) : base(nationality, identity)
	{
	}

	public ZString Type => ZString.Empty;
}
