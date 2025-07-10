using CargoWise.Customs.DE.MessageContracts.NCTS;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(DESREMMessageHeaderProvider))]
	sealed class DESREMMessageHeaderProviderTest : NCTSMessageHeaderProviderAbstractTest<DESREMMessageHeaderProvider, DESREMHeaderProvider>
	{
		protected override INCTSMessageHeader GetMessageHeaderProvider() => new DESREMMessageHeaderProvider(nctsHeader);
	}
}
