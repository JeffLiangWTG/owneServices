using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC060CHolderOfTheTransitProcedureProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("HolderOfTheTransitProcedureType missing", () => new CC060CHolderOfTheTransitProcedureProvider(null));
			});
		}

		public void TestId()
		{
			AssertEquals("Id", "ID1", provider.Id);
		}

		public void TestName()
		{
			AssertEquals("Name", "BOB THE BUILDER", provider.Name);
		}

		public void TestAddress()
		{
			var emptyProvider = new CC060CHolderOfTheTransitProcedureProvider(new HolderOfTheTransitProcedureType13
			{
				Address = null
			});
			AssertEquals(null, emptyProvider.Contact);

			var address = provider.Address;
			AssertEquals("123 WHERE ST", address.StreetAndNumber);
			AssertEquals("2020", address.Postcode);
			AssertEquals("CITY", address.City);
			AssertEquals("ES", address.Country);
		}

		public void TestContact()
		{
			var emptyProvider = new CC060CHolderOfTheTransitProcedureProvider(new HolderOfTheTransitProcedureType13
			{
				ContactPerson = null
			});
			AssertEquals(null, emptyProvider.Contact);

			var contact = provider.Contact;
			AssertEquals("WENDY", contact.Name);
			AssertEquals("951", contact.PhoneNumber);
			AssertEquals("test@test.com", contact.EmailAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC060CHolderOfTheTransitProcedureProvider(new HolderOfTheTransitProcedureType13
			{
				IdentificationNumber = "ID1",
				Name = "BOB THE BUILDER",
				Address = new AddressType07
				{
					StreetAndNumber = "123 WHERE ST",
					Postcode = "2020",
					City = "CITY",
					Country = "ES",
				},
				ContactPerson = new ContactPersonType04
				{
					Name = "WENDY",
					PhoneNumber = "951",
					EMailAddress = "test@test.com"
				}
			});
		}
		CC060CHolderOfTheTransitProcedureProvider provider;
	}
}
