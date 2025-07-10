using System;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(AddressProvider))]
	sealed class AddressProviderTest : Customs.Business.Testing.DataProviderTestCase<AddressProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new AddressProvider(null));
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

		public void TestStreetAndNumberMaxLength()
		{
			AssertEquals("When not in TransitionPeriod, StreetAndNumberMaxLength should be 70", 70, provider.StreetAndNumberMaxLength);

			var data = Factory.CreateOrgAddress("PartyStreet", "21 ", "2600", "AddressCity", Core.Constants.CountryCodes.Belgium);
			provider = new AddressProvider(Factory.CreateJobDocAddress("CEA", "ConsigneeName", "ContactName", "ContactPhone", "ContactEmail", orgAddress: data), true);
			AssertEquals("When in TransitionPeriod, StreetAndNumberMaxLength should be 35", 35, provider.StreetAndNumberMaxLength);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var data = Factory.CreateOrgAddress("PartyStreet", "21", "2600", "AddressCity", Core.Constants.CountryCodes.Belgium);
			provider = new AddressProvider(Factory.CreateJobDocAddress("CEA", "ConsigneeName", "ContactName", "ContactPhone", "ContactEmail", orgAddress: data));
		}

		AddressProvider provider;

		protected override AddressProvider GetProvider() => provider;
	}
}
