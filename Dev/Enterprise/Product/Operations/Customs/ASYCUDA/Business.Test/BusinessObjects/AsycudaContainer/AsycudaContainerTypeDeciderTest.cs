using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaContainerTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header1 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header1.AMA_JobReference = "123";
			header1.AMA_ApplicationCode = "BBK";
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header1Container = header1.Containers.AddNew();
			var header2 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header2.AMA_JobReference = "456";
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header2Container = header2.Containers.AddNew();
			var header3 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header3.AMA_JobReference = "789";
			header3.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header3.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header3Container = header3.Containers.AddNew();
			var header4 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header4.AMA_JobReference = "012";
			header4.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header4.AMA_RN_NKCountry = Core.Constants.CountryCodes.Fiji;
			var header4Container = header4.Containers.AddNew();
			Factory.Save();

			Assert(new BusinessObjectFactory().Load(typeof(AsycudaContainer), header1Container.PK) is Integration.Customs.ManifestBase.IAsycudaContainer);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaContainer), header2Container.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaContainer);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaContainer), header3Container.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaContainer);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaContainer), header4Container.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaContainer);
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaContainer>(), new AsycudaContainerTypeDecider().GetTypeForNew());
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaContainer>(), new BusinessObjectFactory().New<AsycudaContainer>().GetType());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaContainer), new AsycudaContainerTypeDecider().GetTypeForBinding());
		}
	}
}
