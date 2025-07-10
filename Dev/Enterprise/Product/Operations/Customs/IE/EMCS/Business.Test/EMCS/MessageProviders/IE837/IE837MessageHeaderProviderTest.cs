using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE837MessageHeaderProvider))]
	public class IE837MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<IE837MessageHeaderProvider, IE837HeaderProvider>
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
