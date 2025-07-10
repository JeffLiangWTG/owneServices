using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	class TP5OutgoingInterchangeProvider : FRInterchangeProviderBase
	{
		public TP5OutgoingInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
		{
		}

		protected override string GenericMessageInterchangeType => GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsTP5;
	}
}
