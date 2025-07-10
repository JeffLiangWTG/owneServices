using CargoWise.Customs.DE.MessageContracts;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXTNOTMessageHeaderProvider))]
	sealed class EXTNOTMessageHeaderProviderTest : ExitMessageHeaderProviderAbstractTest<EXTNOTMessageHeaderProvider>
	{
		protected override IExitMessageHeader GetMessageHeaderProvider() => new EXTNOTMessageHeaderProvider(exitReport);
	}
}
