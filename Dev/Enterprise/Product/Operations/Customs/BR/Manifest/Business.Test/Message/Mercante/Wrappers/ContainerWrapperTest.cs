using System.Linq;
using CargoWise.Customs.BR.MessageContracts.Mercante.Outgoing;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	class ContainerWrapperTest : TestCaseWithFactory
	{
		public void TestContainerWrapper()
		{
			var manifestHeader = CreateAndPopulateManifestHeader();
			IManifest wrapper = new ManifestWrapper(manifestHeader);

			AssertEquals(1, wrapper.Bills.ElementAt(0).Containers.Count);

			var container = wrapper.Bills.ElementAt(0).Containers.ElementAt(0);

			CombineAssertions(() =>
			{
				AssertEquals("CAIU305178-6", container.Number);
				AssertEquals("1891", container.Type);

				AssertEquals("SEAL1", container.Seals.ElementAt(0));
				AssertEquals("SEAL2", container.Seals.ElementAt(1));
				AssertEquals("SEAL3", container.Seals.ElementAt(2));
			});

			manifestHeader.AMA_ContainerMode = Core.Constants.ContainerModes.BreakBulk;
			wrapper = new ManifestWrapper(manifestHeader);

			AssertEquals(0, wrapper.Bills.ElementAt(0).Containers.Count);
		}

		AsycudaManifestHeader CreateAndPopulateManifestHeader()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ContainerMode = Core.Constants.ContainerModes.Containerised;

			var bill = header.Bills.AddNew();

			var container = CreateAndPopulateContainer(header);
			_ = header.Containers.AddNew();

			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;

			return header;
		}

		AsycudaContainer CreateAndPopulateContainer(AsycudaManifestHeader header)
		{
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "22G0";
			refContainer.RC_ContainerType = Core.Constants.ContainerTypes.FlatRack;

			var containerMap = Factory.New<RefContainerCodeMap>();
			containerMap.RCM_RC_Container = refContainer.PK;
			containerMap.RCM_RN_NKCountry = Core.Constants.CountryCodes.Brazil;
			containerMap.RCM_Code = "1891";

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "CAIU305178-6";
			container.ACN_Seal1 = "SEAL1";
			container.ACN_Seal2 = "SEAL2";
			container.ACN_Seal3 = "SEAL3";
			container.ACN_RC_ContainerType = refContainer.PK;

			return container;
		}
	}
}
