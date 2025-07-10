using CargoWise.Customs.GB.MessageContracts.EMCS;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE813MessageHeaderProvider))]
	sealed class IE813MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<IE813MessageHeaderProvider, IE813HeaderProvider>
	{
		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new IE813MessageHeaderProvider(emcsDeclaration);
	}
}
