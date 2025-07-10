using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED837MessageHeaderProvider))]
	class ED837MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<ED837MessageHeaderProvider, ED837HeaderProvider>
	{
		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new ED837MessageHeaderProvider(emcsDeclaration, explanationOnDelay);

		protected override void SetUp()
		{
			base.SetUp();
			explanationOnDelay = new ExplanationOnDelaySendingAction(emcsDeclaration);
		}
		ExplanationOnDelaySendingAction explanationOnDelay;
	}
}
