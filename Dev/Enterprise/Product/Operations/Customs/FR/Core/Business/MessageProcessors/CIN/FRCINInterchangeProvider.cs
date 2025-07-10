using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRCINInterchangeProvider : FRInterchangeProviderBase
	{
		public FRCINInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
		{
		}

		protected override string GenericMessageInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsCIN;
	}
}
