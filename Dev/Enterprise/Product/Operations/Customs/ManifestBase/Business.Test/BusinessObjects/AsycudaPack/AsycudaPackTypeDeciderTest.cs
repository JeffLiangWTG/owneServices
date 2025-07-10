using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ManifestBase.Testing
{
	class AsycudaPackTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ZA.IAsycudaManifestHeader>();
			header1.AMA_JobReference = "123";
			header1.AMA_ApplicationCode = "OUT";
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			var header1Bill = header1.Bills.AddNew();
			var header1BillPack = header1Bill.Packs.AddNew();
			var header2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
			header2.AMA_JobReference = "456";
			header2.AMA_ApplicationCode = "NVC";
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header2Bill = header2.Bills.AddNew();
			var header2BillPack = header2Bill.Packs.AddNew();
			Factory.Save();

			Assert(new BusinessObjectFactory().Load<AsycudaPack>(header1BillPack.PK) is Integration.Customs.ZA.IAsycudaPack);
			Assert(new BusinessObjectFactory().Load<AsycudaPack>(header2BillPack.PK) is Integration.Customs.ASYCUDA.IAsycudaPack);
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(AsycudaPack), new AsycudaPackTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaPack), new AsycudaPackTypeDecider().GetTypeForBinding());
		}
	}
}
