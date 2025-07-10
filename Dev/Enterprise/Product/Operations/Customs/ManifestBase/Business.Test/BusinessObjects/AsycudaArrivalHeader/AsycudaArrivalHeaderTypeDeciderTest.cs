using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ManifestBase.Testing
{
	class AsycudaArrivalHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var manifest1 = Factory.New<AsycudaManifestHeader>();
			manifest1.AMA_JobReference = "123";
			manifest1.AMA_ApplicationCode = "BAS";
			manifest1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var manifest1ArrivalHeader = Factory.New<AsycudaArrivalHeader>();
			manifest1ArrivalHeader.ATH_AMA_ManifestHeader = manifest1.PK;

			var manifest2 = Factory.New<AsycudaManifestHeader>();
			manifest2.AMA_JobReference = "456";
			manifest2.AMA_ApplicationCode = "NVC";
			manifest2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var manifest2ArrivalHeader = Factory.New<AsycudaArrivalHeader>();
			manifest2ArrivalHeader.ATH_AMA_ManifestHeader = manifest2.PK;
			Factory.Save();

			Assert(new BusinessObjectFactory().Load<AsycudaArrivalHeader>(manifest1ArrivalHeader.PK) is Integration.Customs.ManifestBase.IAsycudaArrivalHeader);
			Assert(new BusinessObjectFactory().Load<AsycudaArrivalHeader>(manifest2ArrivalHeader.PK) is Integration.Customs.ASYCUDA.IAsycudaArrivalHeader);
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(AsycudaArrivalHeader), new AsycudaArrivalHeaderTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaArrivalHeader), new AsycudaArrivalHeaderTypeDecider().GetTypeForBinding());
		}
	}
}
