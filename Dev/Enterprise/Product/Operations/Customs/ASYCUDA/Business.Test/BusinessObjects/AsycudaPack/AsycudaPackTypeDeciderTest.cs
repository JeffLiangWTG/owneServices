using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaPackTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header1 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header1.AMA_JobReference = "123";
			header1.AMA_ApplicationCode = "BBK";
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header1Bill = header1.Bills.AddNew();
			var header1BillPack = header1Bill.Packs.AddNew();
			var header2 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header2.AMA_JobReference = "456";
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header2Bill = header2.Bills.AddNew();
			var header2BillPack = header2Bill.Packs.AddNew();
			var header3 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header3.AMA_JobReference = "789";
			header3.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header3.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header3Bill = header3.Bills.AddNew();
			var header3BillPack = header3Bill.Packs.AddNew();
			var header4 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header4.AMA_JobReference = "012";
			header4.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header4.AMA_RN_NKCountry = Core.Constants.CountryCodes.Fiji;
			var header4Bill = header4.Bills.AddNew();
			var header4BillPack = header4Bill.Packs.AddNew();
			Factory.Save();

			Assert(new BusinessObjectFactory().Load(typeof(AsycudaPack), header1BillPack.PK) is Integration.Customs.ManifestBase.IAsycudaPack);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaPack), header2BillPack.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaPack);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaPack), header3BillPack.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaPack);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaPack), header4BillPack.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaPack);
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaPack>(), new AsycudaPackTypeDecider().GetTypeForNew());
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaPack>(), new BusinessObjectFactory().New<AsycudaPack>().GetType());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaPack), new AsycudaPackTypeDecider().GetTypeForBinding());
		}
	}
}
