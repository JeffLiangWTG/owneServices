using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaPackageContainerLinkTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header1 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header1.AMA_JobReference = "123";
			header1.AMA_ApplicationCode = "BBK";
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header1Bill = header1.Bills.AddNew();
			var header1BillPack = header1Bill.Packs.AddNew();
			var header1Container = header1.Containers.AddNew();
			header1BillPack.ContainerPK = header1Container.PK;
			var header2 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header2.AMA_JobReference = "456";
			header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header2Bill = header2.Bills.AddNew();
			var header2BillPack = header2Bill.Packs.AddNew();
			var header2Container = header2.Containers.AddNew();
			header2BillPack.ContainerPK = header2Container.PK;
			var header3 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header3.AMA_JobReference = "789";
			header3.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
			header3.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header3Bill = header3.Bills.AddNew();
			var header3BillPack = header3Bill.Packs.AddNew();
			var header3Container = header3.Containers.AddNew();
			header3BillPack.ContainerPK = header3Container.PK;
			var header4 = Factory.New<ManifestBase.AsycudaManifestHeader>();
			header4.AMA_JobReference = "012";
			header4.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
			header4.AMA_RN_NKCountry = Core.Constants.CountryCodes.Fiji;
			var header4Bill = header4.Bills.AddNew();
			var header4BillPack = header4Bill.Packs.AddNew();
			var header4Container = header4.Containers.AddNew();
			header4BillPack.ContainerPK = header4Container.PK;
			Factory.Save();

			Assert(new BusinessObjectFactory().Load(typeof(AsycudaContainerBillOrPackageLink), header1BillPack.Pivot.PK) is Integration.Customs.ManifestBase.IAsycudaContainerBillOrPackageLink);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaContainerBillOrPackageLink), header2BillPack.Pivot.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaContainerBillOrPackageLink);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaContainerBillOrPackageLink), header3BillPack.Pivot.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaContainerBillOrPackageLink);
			Assert(new BusinessObjectFactory().Load(typeof(AsycudaContainerBillOrPackageLink), header4BillPack.Pivot.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaContainerBillOrPackageLink);
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaContainerBillOrPackageLink>(), new AsycudaContainerBillOrPackageLinkDecider().GetTypeForNew());
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaContainerBillOrPackageLink>(), new BusinessObjectFactory().New<AsycudaContainerBillOrPackageLink>().GetType());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaContainerBillOrPackageLink), new AsycudaContainerBillOrPackageLinkDecider().GetTypeForBinding());
		}
	}
}
