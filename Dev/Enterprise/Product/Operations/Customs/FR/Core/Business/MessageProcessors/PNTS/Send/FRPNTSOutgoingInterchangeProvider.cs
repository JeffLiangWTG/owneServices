using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRPNTSOutgoingInterchangeProvider : FRInterchangeProviderBase
	{
		public FRPNTSOutgoingInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
		{
		}

		protected override string GenericMessageInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsPNTS;
	}
}
