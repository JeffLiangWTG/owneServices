using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class BLArgentinaWrapperAuthTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestBLArgentinaWrapperAuth()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			Factory.Save();

			ISeaManifest wrapper = new BLArgentinaWrapperManifest(bill);
			IAuth authentication = wrapper.Authentication;

			CombineAssertions(() =>
			{
				AssertEquals("", authentication.CompanyCUIT);
				AssertEquals("", authentication.CompanyRol);
				AssertEquals("", authentication.AgentType);
			});
		}
	}
}
