using CargoWise.Customs.GB.MessageContracts.EMCS;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE815MessageHeaderProvider))]
	sealed class IE815MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<IE815MessageHeaderProvider, IE815HeaderProvider>
	{
		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new IE815MessageHeaderProvider(emcsDeclaration);
	}
}
