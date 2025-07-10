using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaBillTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header1 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header1.AMA_JobReference = "123";
			header1.AMA_ApplicationCode = "BBK";
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header1Bill = header1.Bills.AddNew();
			var header2 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header2.AMA_JobReference = "456";
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header2Bill = header2.Bills.AddNew();
			var header3 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header3.AMA_JobReference = "789";
			header3.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header3.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header3Bill = header3.Bills.AddNew();
			var header4 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header4.AMA_JobReference = "012";
			header4.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header4.AMA_RN_NKCountry = Core.Constants.CountryCodes.Fiji;
			var header4Bill = header4.Bills.AddNew();
			Factory.Save();

			Assert(new BusinessObjectFactory().Load(typeof(AsycudaBill), header1Bill.PK) is Integration.Customs.ManifestBase.IAsycudaBill);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaBill), header2Bill.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaBill);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaBill), header3Bill.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaBill);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaBill), header4Bill.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaBill);
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaBill>(), new AsycudaBillTypeDecider().GetTypeForNew());
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaBill>(), new BusinessObjectFactory().New<AsycudaBill>().GetType());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaBill), new AsycudaBillTypeDecider().GetTypeForBinding());
		}
	}
}
