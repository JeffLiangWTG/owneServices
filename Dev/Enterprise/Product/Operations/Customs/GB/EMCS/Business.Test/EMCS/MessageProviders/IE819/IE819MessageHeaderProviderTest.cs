using System;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE819MessageHeaderProvider))]
	sealed class IE819MessageHeaderProviderTest : MessageHeaderProviderAbstractTest<IE819MessageHeaderProvider, IE819HeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new IE819MessageHeaderProvider(emcsDeclaration, null));
		}

		public void TestRecipient()
		{
			var provider = GetMessageHeaderProvider();
			AssertEquals("NDEA.GB", provider.Recipient);
		}

		public void TestSender()
		{
			var provider = GetMessageHeaderProvider();
			AssertEquals("NDEA.GB", provider.Sender);
		}

		protected override IEMCSMessageHeader GetMessageHeaderProvider() => new IE819MessageHeaderProvider(emcsDeclaration, new AlertOrRejectSendingAction(emcsDeclaration));
	}
}
