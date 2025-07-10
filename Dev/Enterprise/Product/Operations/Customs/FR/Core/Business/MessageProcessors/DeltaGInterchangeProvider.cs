using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class DeltaGInterchangeProvider : FRInterchangeProviderBase
	{
		public DeltaGInterchangeProvider(NonDependentEDIMessageCollection messages)
			: base(messages)
		{
		}

		protected override string GenericMessageInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustoms;
	}
}
