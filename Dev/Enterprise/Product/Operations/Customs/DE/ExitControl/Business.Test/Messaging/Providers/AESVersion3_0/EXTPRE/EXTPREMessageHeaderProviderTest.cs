using CargoWise.Customs.DE.MessageContracts;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	[TestedType(typeof(EXTPREMessageHeaderProvider))]
	sealed class EXTPREMessageHeaderProviderTest : ExitMessageHeaderProviderAbstractTest<EXTPREMessageHeaderProvider>
	{
		protected override IExitMessageHeader GetMessageHeaderProvider() => new EXTPREMessageHeaderProvider(exitReport);
	}
}
