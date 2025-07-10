using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(PartyProvider))]
	sealed class PartyProviderBaseOnlyTest : PartyProviderAbstractTest<PartyProvider>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("BEPartyID", Provider.IdentificationNumber);
		}

		public void TestNameMaxlength()
		{
			var provider = new PartyProvider(address);
			AssertEquals("When not in TransitionPeriod, StreetAndNumberMaxLength should be 70", 70, provider.NameMaxlength);

			provider = new PartyProvider(address, true);
			AssertEquals("When in TransitionPeriod, StreetAndNumberMaxLength should be 35", 35, provider.NameMaxlength);
		}
	}
}
