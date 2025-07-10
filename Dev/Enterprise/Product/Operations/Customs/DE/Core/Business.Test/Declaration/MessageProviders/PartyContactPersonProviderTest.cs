using CargoWise.Customs.DE.MessageContracts;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	class PartyContactPersonProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyContactPersonProvider>
	{
		public void TestNew()
		{
			AssertNull(PartyContactPersonProvider.NewOrNull(null));
		}

		public void TestPosition()
		{
			AssertEquals("DEV", dataProvider.Position);
		}

		public void TestPersonName()
		{
			AssertEquals("VWG", dataProvider.PersonName);
		}

		public void TestPhoneNumber()
		{
			AssertEquals("12345", dataProvider.PhoneNumber);
		}

		public void TestFacsimileNumber()
		{
			AssertEquals("789", dataProvider.FacsimileNumber);
		}

		public void TestMailAddress()
		{
			AssertEquals("123@abc.com", dataProvider.MailAddress);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var orgContact = Factory.New<OrgContact>();
			orgContact.OC_Title = "DEV";
			orgContact.OC_ContactName = "VWG";
			orgContact.OC_Phone = "12345";
			orgContact.OC_Fax = "789";
			orgContact.OC_Email = "123@abc.com";
			dataProvider = PartyContactPersonProvider.NewOrNull(orgContact);
		}
		IAESPartyContactPerson dataProvider;

		protected override PartyContactPersonProvider GetProvider() => (PartyContactPersonProvider)dataProvider;
	}
}
