using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	class PartyContactPersonJobDocAddressProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyContactPersonJobDocAddressProvider>
	{
		public void TestNew()
		{
			AssertNull(PartyContactPersonJobDocAddressProvider.NewOrNull(null));
		}

		public void TestPosition()
		{
			AssertEquals(ZString.Empty, dataProvider.Position);
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

			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_Contact = "VWG";
			jobDocAddress.E2_Fax = "789";
			jobDocAddress.E2_Email = "123@abc.com";
			jobDocAddress.E2_Phone = "12345";
			dataProvider = PartyContactPersonJobDocAddressProvider.NewOrNull(jobDocAddress);
		}
		IAESPartyContactPerson dataProvider;

		protected override PartyContactPersonJobDocAddressProvider GetProvider() => (PartyContactPersonJobDocAddressProvider)dataProvider;
	}
}
