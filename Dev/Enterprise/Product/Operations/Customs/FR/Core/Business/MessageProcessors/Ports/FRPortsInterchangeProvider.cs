using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRPortsInterchangeProvider : FRInterchangeProviderBase
	{
		public FRPortsInterchangeProvider(NonDependentEDIMessageCollection messages) : base(messages)
		{
		}

		protected override string GenericMessageInterchangeType => FREDIMessage.ApplicationCodes.FRPortMessage;
	}
}
