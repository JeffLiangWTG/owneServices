using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ManifestBase.Testing
{
	class AsycudaBillTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ZA.IAsycudaManifestHeader>();
			header1.AMA_JobReference = "123";
			header1.AMA_ApplicationCode = "OUT";
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			var header1Bill = header1.Bills.AddNew();
			var header2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
			header2.AMA_JobReference = "456";
			header2.AMA_ApplicationCode = "NVC";
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header2Bill = header2.Bills.AddNew();
			Factory.Save();

			Assert(new BusinessObjectFactory().Load<AsycudaBill>(header1Bill.PK) is Integration.Customs.ZA.IAsycudaBill);
			Assert(new BusinessObjectFactory().Load<AsycudaBill>(header2Bill.PK) is Integration.Customs.ASYCUDA.IAsycudaBill);
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(AsycudaBill), new AsycudaBillTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaBill), new AsycudaBillTypeDecider().GetTypeForBinding());
		}
	}
}
