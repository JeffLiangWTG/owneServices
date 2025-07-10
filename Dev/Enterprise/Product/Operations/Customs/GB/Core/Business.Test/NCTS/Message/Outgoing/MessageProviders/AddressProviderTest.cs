using System;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class AddressProviderTest : DataProviderTestCase<AddressProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new AddressProvider(null, false));
		}

		public void TestStreetAndNumber()
		{
			AssertEquals("PartyStreet 21", provider.StreetAndNumber);
		}

		public void TestPostcode()
		{
			AssertEquals("2600", provider.Postcode);
		}

		public void TestCity()
		{
			AssertEquals("AddressCity", provider.City);
		}

		public void TestCountry()
		{
			AssertEquals(Core.Constants.CountryCodes.Belgium, provider.Country);
		}

		public void TestAddressStreetAndNumberMaxLength_ForPhase5TransitionPeriod()
		{
			AssertEquals("Outside Phase 5 Transition Period", MessageSchema.AddressStreetAndNumberMaxLength, Provider.AddressStreetAndNumberMaxLength);
			AssertEquals("In Phase 5 Transition Period", MessageSchemaInTransitionPeriod.AddressStreetAndNumberMaxLength, new AddressProvider(jobDocAddress, true).AddressStreetAndNumberMaxLength);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var data = Factory.CreateOrgAddress("PartyStreet", "21", "2600", "AddressCity", Core.Constants.CountryCodes.Belgium);
			jobDocAddress = Factory.CreateJobDocAddress("CEA", "ConsigneeName", "ContactName", "ContactPhone", "ContactEmail", orgAddress: data);
			provider = new AddressProvider(jobDocAddress, false);
		}

		JobDocAddress jobDocAddress;
		AddressProvider provider;

		protected override AddressProvider GetProvider() => provider;
	}
}
