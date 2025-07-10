using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(PersonProvider))]
	sealed class PersonProviderTest : Customs.Business.Testing.DataProviderTestCase<PersonProvider>
	{
		public void TestName()
		{
			header.ContactFullName = "contactname";
			AssertEquals("contactname", Provider.Name);
		}

		public void TestPhoneNumber()
		{
			header.ContactPhone = "phonenumber";
			AssertEquals("phonenumber", Provider.PhoneNumber);
		}

		public void TestEMailAddress()
		{
			header.ContactEmail = "test@email.com";
			AssertEquals("test@email.com", Provider.EMailAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);

			provider = new PersonProvider(header);
		}

		NctsHeader header;
		PersonProvider provider;

		protected override PersonProvider GetProvider() => provider;
	}
}
