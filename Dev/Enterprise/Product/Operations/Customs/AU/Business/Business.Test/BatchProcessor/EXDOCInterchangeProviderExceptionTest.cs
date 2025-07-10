using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDOCInterchangeProviderExceptionTest : SenderReceiverIDExceptionTest
	{
		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection messages) => new EXDOCInterchangeProvider(messages);
	}
}
