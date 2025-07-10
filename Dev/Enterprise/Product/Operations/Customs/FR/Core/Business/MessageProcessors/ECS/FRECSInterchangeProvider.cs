using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRECSInterchangeProvider : FRInterchangeProviderBase
	{
		public FRECSInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
		{
		}

		protected override string GenericMessageInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsECS;
	}
}
