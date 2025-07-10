using CargoWise.Customs.DE.MessageContracts.NCTS;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(DEPDATMessageHeaderProvider))]
	sealed class DEPDATMessageHeaderProviderTest : NCTSMessageHeaderProviderAbstractTest<DEPDATMessageHeaderProvider, DEPDATHeaderProvider>
	{
		protected override INCTSMessageHeader GetMessageHeaderProvider() => new DEPDATMessageHeaderProvider(nctsHeader);
	}
}
