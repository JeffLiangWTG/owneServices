using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	sealed class BrokerWrapperTest : Customs.Business.Testing.DataProviderTestCase<BrokerWrapper>
	{
		public void TestType_EmailHasValue()
		{
			AssertEquals("Type should be equivalent to EM when broker.GS_EmailAddress has value.", "EM", Provider.Type);
		}

		public void TestType_EmailIsEmpty()
		{
			var provider = GetAlternativeWrapper_EmailIsEmpty();
			AssertEquals("Type should be Empty when broker.GS_EmailAddress is empty.", ZString.Empty, provider.Type);
		}

		public void TestIdentifier_EmailHasValue()
		{
			AssertEquals("Identifier should be equivalent to broker.GS_EmailAddress when broker.GS_EmailAddress has value.", "Satoshi@palletTown.com", Provider.Identifier);
		}

		public void TestIdentifier_EmailIsEmpty()
		{
			var provider = GetAlternativeWrapper_EmailIsEmpty();
			AssertEquals("Identifier should be empty when broker.GS_EmailAddress is empty.", string.Empty, provider.Identifier);
		}

		BrokerWrapper GetAlternativeWrapper_EmailIsEmpty()
		{
			var contact = Factory.New<GlbStaff>();
			contact.GS_EmailAddress = ZString.Empty;
			return BrokerWrapper.New(contact);
		}

		protected override BrokerWrapper GetProvider()
		{
			var contact = Factory.New<GlbStaff>();
			contact.GS_EmailAddress = "Satoshi@palletTown.com";
			return BrokerWrapper.New(contact);
		}

		internal static void AssertBrokerWrapper(BrokerWrapper broker, string expectedType, string expectedIdentifier)
		{
			CombineAssertions("BrokerWrapper properties.", () =>
			{
				AssertEquals("BrokerWrapper.Type", expectedType, broker.Type);
				AssertEquals("BrokerWrapper.Identifier", expectedIdentifier, broker.Identifier);
			});
		}
	}
}
