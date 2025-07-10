using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonDepartureTransportMeansWrapper : TransportMediumInfoCommonWrapper, ICommonDepartureTransportMeans
{
	public CommonDepartureTransportMeansWrapper(ZString transportMode, ZString transportId, ZString transportNationality, ZShort seqNum, JobDeclaration declaration = null) : base(transportMode, transportId, transportNationality, declaration)
	{
		SequenceNumber = seqNum.ToString();
	}

	public ZString SequenceNumber { get; }
}
