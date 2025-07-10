using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.NCTS.Business;

public class TIRHeaderEmptyMeansOfTransportCrossingBorderWrapper : SADMeansOfTransportWrapper, IETHeaderMeansOfTransportCrossingBorder
{
	public TIRHeaderEmptyMeansOfTransportCrossingBorderWrapper() : base(nationality: ZString.Empty, identity: ZString.Empty)
	{
	}

	public ZString Type => ZString.Empty;
}
