using CargoWise.Types;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonActiveBorderTransportMeansWrapper : CommonDepartureTransportMeansWrapper, INCTSCommonActiveBorderTransportMeans
	{
		public NCTS5CommonActiveBorderTransportMeansWrapper(ZString transportMode, ZString transportId, ZString transportNationality, ZString conveyanceReference, ZShort seqNum) : base(transportMode, transportId, transportNationality, seqNum)
		{
			ConveyanceReferenceNumber = conveyanceReference;
		}

		public ZString ConveyanceReferenceNumber { get; }
	}
}
