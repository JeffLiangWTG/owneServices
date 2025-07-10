using CargoWise.Customs.DE.MessageContracts.EMCS;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED815MessageHeaderProvider))]
	class ED815MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<ED815MessageHeaderProvider, ED815HeaderProvider>
	{
		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new ED815MessageHeaderProvider(emcsDeclaration);
	}
}
