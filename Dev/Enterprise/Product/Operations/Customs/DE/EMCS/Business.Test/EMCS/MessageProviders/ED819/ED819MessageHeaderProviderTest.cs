using System;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	[TestedType(typeof(ED819MessageHeaderProvider))]
	class ED819MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<ED819MessageHeaderProvider, ED819HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ED819MessageHeaderProvider(emcsDeclaration, null));
		}

		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new ED819MessageHeaderProvider(emcsDeclaration, new AlertOrRejectSendingAction(emcsDeclaration));
	}
}
