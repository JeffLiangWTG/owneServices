using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC060CRepresentativeProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("RepresentativeType missing", () => new CC060CRepresentativeProvider(null));
			});
		}

		public void TestId()
		{
			AssertEquals("Id", "IDREPRES123450000", provider.Id);
		}

		public void TestContact()
		{
			var emptyProvider = new CC060CRepresentativeProvider(new RepresentativeType04
			{
				ContactPerson = null
			});
			AssertEquals(null, emptyProvider.Contact);

			var contact = provider.Contact;
			AssertEquals("WENDY", contact.Name);
			AssertEquals("+001951", contact.PhoneNumber);
			AssertEquals("test@test.com", contact.EmailAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC060CRepresentativeProvider(new RepresentativeType04
			{
				IdentificationNumber = "IDREPRES123450000",
				Status = "2",
				ContactPerson = new ContactPersonType04
				{
					Name = "WENDY",
					PhoneNumber = "+001951",
					EMailAddress = "test@test.com"
				}
			});
		}
		CC060CRepresentativeProvider provider;
	}
}
