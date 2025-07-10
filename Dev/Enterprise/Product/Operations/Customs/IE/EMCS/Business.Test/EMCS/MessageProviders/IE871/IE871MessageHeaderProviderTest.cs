using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE871MessageHeaderProvider))]
	public class IE871MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<IE871MessageHeaderProvider, IE871HeaderProvider>
	{
		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new IE871MessageHeaderProvider(emcsDeclaration, string.Empty);
	}
}
