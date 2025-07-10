using System;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE810MessageHeaderProvider))]
	class IE810MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<IE810MessageHeaderProvider, IE810HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE810MessageHeaderProvider(emcsDeclaration, null));
		}

		protected override void SetUp()
		{
			base.SetUp();
			cancellationOfEad = new CancellationSendingAction(emcsDeclaration);
		}
		CancellationSendingAction cancellationOfEad;

		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new IE810MessageHeaderProvider(emcsDeclaration, cancellationOfEad);
	}
}
