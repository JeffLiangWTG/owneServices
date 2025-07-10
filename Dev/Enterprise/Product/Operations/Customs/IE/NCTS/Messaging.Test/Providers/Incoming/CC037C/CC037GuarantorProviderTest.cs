using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	public class CC037GuarantorProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("GuarantorType missing", () => new CC037GuarantorProvider(null));
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
			var address = provider.Address;
			AssertEquals("123 WHERE ST", address.StreetAndNumber);
			AssertEquals("2020", address.Postcode);
			AssertEquals("CITY", address.City);
			AssertEquals("IE", address.Country);
		}

		public void TestContact()
		{
			var contact = provider.Contact;
			AssertEquals("WENDY", contact.Name);
			AssertEquals("951", contact.PhoneNumber);
			AssertEquals("test@test.com", contact.EmailAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC037GuarantorProvider(new GuarantorType01
			{
				IdentificationNumber = "ID1",
				Name = "BOB THE BUILDER",
				Address = new AddressType13
				{
					StreetAndNumber = "123 WHERE ST",
					Postcode = "2020",
					City = "CITY",
					Country = CountryCodesCustomsOfficeLists.Ie,
				},
				ContactPerson = new ContactPersonType01
				{
					Name = "WENDY",
					PhoneNumber = "951",
					EMailAddress = "test@test.com"
				}
			});
		}
		CC037GuarantorProvider provider;
	}
}
