using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CMRInterchangeProviderExceptionTest : SenderReceiverIDExceptionTest
	{
		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection messages) => new CMRInterchangeProvider(messages);
	}
}
