using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE837MessageHeaderProvider))]
	sealed class IE837MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<IE837MessageHeaderProvider, IE837HeaderProvider>
	{
		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new IE837MessageHeaderProvider(emcsDeclaration, explanationOnDelay);

		protected override void SetUp()
		{
			base.SetUp();
			explanationOnDelay = new ExplanationOnDelaySendingAction(emcsDeclaration);
		}
		ExplanationOnDelaySendingAction explanationOnDelay;
	}
}
