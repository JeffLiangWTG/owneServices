using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.HK.ServiceTasks.Testing
{
	class ExceptionTest : SenderReceiverIDExceptionTest
	{
		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection messages) => new TraxonInterchangeProvider(messages);
	}
}
