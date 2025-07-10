using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE813MessageHeaderProvider))]
	class IE813MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<IE813MessageHeaderProvider, IE813HeaderProvider>
	{
		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new IE813MessageHeaderProvider(emcsDeclaration);
	}
}
