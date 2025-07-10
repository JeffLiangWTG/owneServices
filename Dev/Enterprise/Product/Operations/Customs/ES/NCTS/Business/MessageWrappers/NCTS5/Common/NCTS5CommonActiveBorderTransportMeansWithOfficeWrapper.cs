using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonActiveBorderTransportMeansWithOfficeWrapper : NCTS5CommonActiveBorderTransportMeansWrapper, INCTSCommonActiveBorderTransportMeansWithOffice
	{
		public NCTS5CommonActiveBorderTransportMeansWithOfficeWrapper(ZString transportMode, ZString transportId, ZString transportNationality, ZString customsOffice, ZString conveyanceReference, ZShort seqNum) : base(transportMode, transportId, transportNationality, conveyanceReference, seqNum)
		{
			CustomsOfficeAtBorderReferenceNumber = customsOffice;
		}

		public ZString CustomsOfficeAtBorderReferenceNumber { get; }
	}
}
