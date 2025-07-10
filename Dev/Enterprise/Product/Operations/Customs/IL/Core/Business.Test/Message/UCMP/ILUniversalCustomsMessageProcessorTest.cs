using Enterprise.Messaging.Business.MessageProcessor.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILUniversalCustomsMessageProcessor))]
	sealed class ILUniversalCustomsMessageProcessorTest : UniversalCustomsMessageProcessorTest<ILUniversalCustomsMessageProcessor>
	{
		protected override string ApplicationCode => "ILC";
	}
}
