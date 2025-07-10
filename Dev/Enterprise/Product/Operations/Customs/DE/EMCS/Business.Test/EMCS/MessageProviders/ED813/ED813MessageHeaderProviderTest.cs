using CargoWise.Customs.DE.MessageContracts.EMCS;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED813MessageHeaderProvider))]
	class ED813MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<ED813MessageHeaderProvider, ED813HeaderProvider>
	{
		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new ED813MessageHeaderProvider(emcsDeclaration);
	}
}
