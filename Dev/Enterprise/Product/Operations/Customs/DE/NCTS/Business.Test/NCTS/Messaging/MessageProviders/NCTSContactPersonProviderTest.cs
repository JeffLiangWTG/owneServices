using CargoWise.Customs.DE.MessageContracts.NCTS;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class NCTSContactPersonProviderTest : Customs.Business.Testing.DataProviderTestCase<INCTSContactPerson>
	{
		public void TestName()
		{
			AssertEquals("Fritz", Provider.Name);
		}

		public void TestMailAddress()
		{
			AssertEquals("fritz@domain.org", Provider.MailAddress);
		}

		public void TestPhoneNumber()
		{
			AssertEquals("+491234567", Provider.PhoneNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			docAddress = Factory.NewWithValidTestData<JobDocAddress>();
			docAddress.E2_Contact = "Fritz";
			docAddress.E2_Phone = "+491234567";
			docAddress.E2_Email = "fritz@domain.org";
		}

		JobDocAddress docAddress;

		protected override INCTSContactPerson GetProvider() => NCTSContactPersonProvider.NewOrNull(docAddress);
	}
}
