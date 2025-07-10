using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE815MessageHeaderProvider))]
	public class IE815MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<IE815MessageHeaderProvider, IE815HeaderProvider>
	{
		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new IE815MessageHeaderProvider(emcsDeclaration);
	}
}
