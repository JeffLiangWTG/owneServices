using CargoWise.Customs.DE.MessageContracts.NCTS;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(DESNOTMessageHeaderProvider))]
	sealed class DESNOTMessageHeaderProviderTest : NCTSMessageHeaderProviderAbstractTest<DESNOTMessageHeaderProvider, DESNOTHeaderProvider>
	{
		protected override INCTSMessageHeader GetMessageHeaderProvider() => new DESNOTMessageHeaderProvider(nctsHeader);
	}
}
