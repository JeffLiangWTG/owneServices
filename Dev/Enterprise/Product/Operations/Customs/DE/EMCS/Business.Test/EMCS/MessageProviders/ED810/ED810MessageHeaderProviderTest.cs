using System;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED810MessageHeaderProvider))]
	class ED810MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<ED810MessageHeaderProvider, ED810HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED810MessageHeaderProvider(emcsDeclaration, null));
		}

		protected override void SetUp()
		{
			base.SetUp();
			cancellationOfEad = new CancellationSendingAction(emcsDeclaration);
		}
		CancellationSendingAction cancellationOfEad;

		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new ED810MessageHeaderProvider(emcsDeclaration, cancellationOfEad);
	}
}
