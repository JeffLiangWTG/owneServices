using System;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE810MessageHeaderProvider))]
	sealed class IE810MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<IE810MessageHeaderProvider, IE810HeaderProvider>
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
