using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ManifestBase.Testing
{
	class AsycudaContainerTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ZA.IAsycudaManifestHeader>();
			header1.AMA_JobReference = "123";
			header1.AMA_ApplicationCode = "OUT";
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			var header1Container = (AsycudaContainer)Factory.New<Integration.Customs.ZA.IAsycudaContainer>();
			header1Container.ACN_AMA_Manifest = header1.PK;
			var header2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
			header2.AMA_JobReference = "456";
			header2.AMA_ApplicationCode = "NVC";
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header2Container = (AsycudaContainer)Factory.New<Integration.Customs.ASYCUDA.IAsycudaContainer>();
			header2Container.ACN_AMA_Manifest = header2.PK;
			Factory.Save();

			Assert(new BusinessObjectFactory().Load<AsycudaContainer>(header1Container.PK) is Integration.Customs.ZA.IAsycudaContainer);
			Assert(new BusinessObjectFactory().Load<AsycudaContainer>(header2Container.PK) is Integration.Customs.ASYCUDA.IAsycudaContainer);
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(AsycudaContainer), new AsycudaContainerTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaContainer), new AsycudaContainerTypeDecider().GetTypeForBinding());
		}
	}
}
