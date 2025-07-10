using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NctsCustomsTransitOfficeWrapper : INctsCustomsTransitOfficeProvider
	{
		public NctsCustomsTransitOfficeWrapper(ZString destinationCustomsOfficeCode)
		{
			CustomsTransitOfficeState = destinationCustomsOfficeCode.Left(2);
			CustomsTransitOfficeCode = destinationCustomsOfficeCode.SubstringSafe(2);
		}

		public ZString CustomsTransitOfficeState { get; }

		public ZString CustomsTransitOfficeCode { get; }
	}
}
