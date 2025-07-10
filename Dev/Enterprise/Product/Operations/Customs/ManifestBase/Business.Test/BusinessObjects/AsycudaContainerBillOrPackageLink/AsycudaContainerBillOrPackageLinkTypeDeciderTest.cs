using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ManifestBase.Testing
{
	class AsycudaContainerBillOrPackageLinkTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var header1 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ZA.IAsycudaManifestHeader>();
			header1.AMA_JobReference = "123";
			header1.AMA_ApplicationCode = "OUT";
			header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
			var header1Bill = header1.Bills.AddNew();
			var header1Container = header1.Containers.AddNew();
			var header1Pack = header1Bill.Packs.AddNew();
			header1Pack.ContainerPK = header1Container.PK;
			var header1PackageContainerLink = header1Pack.Pivot;
			var header2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
			header2.AMA_JobReference = "456";
			header2.AMA_ApplicationCode = "NVC";
			header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header2Bill = header2.Bills.AddNew();
			var header2Container = header2.Containers.AddNew();
			var header2Pack = header2Bill.Packs.AddNew();
			header2Pack.ContainerPK = header2Container.PK;
			var header2PackageContainerLink = header2Pack.Pivot;
			var header3 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
			header3.AMA_JobReference = "789";
			header3.AMA_ApplicationCode = "NVC";
			header3.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var header3Bill = header3.Bills.AddNew();
			var header3Container = header3.Containers.AddNew();
			header3Bill.ContainerPK = header3Container.PK;
			var header3PackageContainerLink = header3Bill.Pivot;
			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("header1PackageContainerLink type", new BusinessObjectFactory().Load<AsycudaContainerBillOrPackageLink>(header1PackageContainerLink.PK) is Integration.Customs.ZA.IAsycudaContainerBillOrPackageLink);
				Assert("header2PackageContainerLink type", new BusinessObjectFactory().Load<AsycudaContainerBillOrPackageLink>(header2PackageContainerLink.PK) is Integration.Customs.ASYCUDA.IAsycudaContainerBillOrPackageLink);
				Assert("header3PackageContainerLink type", new BusinessObjectFactory().Load<AsycudaContainerBillOrPackageLink>(header3PackageContainerLink.PK) is Integration.Customs.ASYCUDA.IAsycudaContainerBillOrPackageLink);
			});
		}

		public void TestGetTypeForNew()
		{
			AssertEquals(typeof(AsycudaContainerBillOrPackageLink), new AsycudaContainerBillOrPackageLinkTypeDecider().GetTypeForNew());
		}

		public void TestGetTypeForBinding()
		{
			AssertEquals(typeof(AsycudaContainerBillOrPackageLink), new AsycudaContainerBillOrPackageLinkTypeDecider().GetTypeForBinding());
		}
	}
}
