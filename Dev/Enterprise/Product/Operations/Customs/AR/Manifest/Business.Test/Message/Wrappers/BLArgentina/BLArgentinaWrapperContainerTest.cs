using System.Linq;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	class BLArgentinaWrapperContainerTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestBLArgentinaWrapperContainer()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			CreateAndPopulateContainer();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = header.Containers[0].PK;

			Factory.Save();

			ISeaManifest wrapper = new BLArgentinaWrapperManifest(bill);
			IContainer container = wrapper.Containers.ElementAt(0);

			CombineAssertions(() =>
			{
				AssertEquals("CAIU305178-6", container.ContainerNumber);
				AssertEquals("V", container.ContainerCondition);
				AssertEquals("22G0", container.Description);
				AssertEquals(170m, container.GrossWeight);
				AssertEquals("SEAL1", container.SealNumber);
				AssertEquals("11/11/2021 12:00:00 AM", container.ContainerExpireDate.ToString());
				AssertEquals("ACEP", container.ACEP);
			});
		}

		void CreateAndPopulateContainer()
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "22G0";

			var container = header.Containers.AddNew();

			container.ACN_ACEP = "ACEP";
			container.ACN_ContainerNumber = "CAIU305178-6";
			container.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.EmptyContainer;
			container.ACN_ExpireDate = new ZDate(2021, 11, 11);
			container.ACN_GoodsWeight = 170;
			container.ACN_RC_ContainerType = refContainer.PK;
			container.ACN_Seal1 = "SEAL1";
		}
	}
}
