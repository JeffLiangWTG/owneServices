using CargoWise.Customs.DE.MessageContracts;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXTANTMessageHeaderProvider))]
	sealed class EXTANTMessageHeaderProviderTest : ExitMessageHeaderProviderAbstractTest<EXTANTMessageHeaderProvider>
	{
		protected override IExitMessageHeader GetMessageHeaderProvider() => new EXTANTMessageHeaderProvider(exitReport);
	}
}
