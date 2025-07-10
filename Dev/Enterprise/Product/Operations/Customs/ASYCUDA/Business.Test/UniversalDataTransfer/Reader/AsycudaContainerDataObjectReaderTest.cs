using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	sealed class AsycudaContainerDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportingAsycudaContainersData()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var help = new AsycudaManifestDataObjectReaderTestHelper();

			var container = help.SetupContainer("CONT00001", "ABC", 200m, "CC1", "STO", "F", "SPN", "SPT", 10);
			Factory.SaveForTesting();
			var containerBO = new AsycudaContainerDataObjectReader(container, Logger, Factory, header).ReadIntoBusinessObject();
			AssertNotNull(containerBO);
			AssertEquals("containerBO.ACN_ContainerNumber", "CONT00001", containerBO.ACN_ContainerNumber);
			AssertEquals("containerBO.ACN_Seal1", "ABC", containerBO.ACN_Seal1);
			AssertEquals("containerBO.ACN_GoodsWeight", 200m, containerBO.ACN_GoodsWeight);
			AssertEquals("containerBO.ACN_CommodityCode", "CC1", containerBO.ACN_CommodityCode);
			AssertEquals("containerBO.ACN_StowageLocation", "STO", containerBO.ACN_StowageLocation);
			AssertEquals("containerBO.ACN_EmptyFullIndicator", "F", containerBO.ACN_EmptyFullIndicator);
			AssertEquals("containerBO.ACN_SealingPartyName", "SPN", containerBO.ACN_SealingPartyName);
			AssertEquals("containerBO.ACN_SealingPartyType", "SPT", containerBO.ACN_SealingPartyType);
			AssertEquals("containerBO.ACN_NumberOfPackages", 10, containerBO.ACN_NumberOfPackages);
		}

		public void TestUpdateAsycudaContainersData()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var oldContainer = header.Containers.AddNew();
			oldContainer.ACN_ContainerNumber = "CONT00001";
			var containerPK = oldContainer.PK;
			Factory.SaveForTesting();
			var container = help.SetupContainer("CONT00001", "ABC", 200m, "CC1", "STO", "F", "SPN", "SPT", 10);
			var containerBO = new AsycudaContainerDataObjectReader(container, Logger, Factory, header).ReadIntoBusinessObject();
			AssertNotNull(containerBO);
			AssertEquals("container is updated", containerPK, containerBO.PK);
			AssertEquals("containerBO.ACN_ContainerNumber", "CONT00001", containerBO.ACN_ContainerNumber);
			AssertEquals("containerBO.ACN_Seal1", "ABC", containerBO.ACN_Seal1);
			AssertEquals("containerBO.ACN_GoodsWeight", 200m, containerBO.ACN_GoodsWeight);
			AssertEquals("containerBO.ACN_CommodityCode", "CC1", containerBO.ACN_CommodityCode);
			AssertEquals("containerBO.ACN_StowageLocation", "STO", containerBO.ACN_StowageLocation);
			AssertEquals("containerBO.ACN_EmptyFullIndicator", "F", containerBO.ACN_EmptyFullIndicator);
			AssertEquals("containerBO.ACN_SealingPartyName", "SPN", containerBO.ACN_SealingPartyName);
			AssertEquals("containerBO.ACN_SealingPartyType", "SPT", containerBO.ACN_SealingPartyType);
			AssertEquals("containerBO.ACN_NumberOfPackages", 10, containerBO.ACN_NumberOfPackages);
		}

		public void TestCanNotImport()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var container = help.SetupContainer("CONT00001", "ABC", 200m, "CC1", "STO", "F", "SPN", "SPT", 10);

			container.ContainerNumber = "";
			var containerBO = new AsycudaContainerDataObjectReader(container, Logger, Factory, header).ReadIntoBusinessObject();
			AssertNull(containerBO);
			AssertContains("logger.Logs", @"ContainerNumber must not be empty.", Logger.Logs);
		}

		public void TestContainerType()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var help = new AsycudaManifestDataObjectReaderTestHelper();

			var container = help.SetupContainer("CONT00001", "ABC", 200m, "CC1", "STO", "F", "SPN", "SPT", 10);
			container.ContainerType = new UniversalDataBuss.DataObjects.Universal.ContainerType();
			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "RC20";
			refContainer.RC_ISOType = "TS20";
			refContainer.RC_Height = 20m;
			refContainer.RC_Width = 20m;
			refContainer.RC_Length = 50m;
			Factory.SaveForTesting();
			var containerBO = new AsycudaContainerDataObjectReader(container, Logger, Factory, header).ReadIntoBusinessObject();
			AssertEquals(ZGuid.Empty, containerBO.ACN_RC_ContainerType);

			container.ContainerType.Code = "RC20";
			Factory.SaveForTesting();
			containerBO = new AsycudaContainerDataObjectReader(container, Logger, Factory, header).ReadIntoBusinessObject();
			AssertEquals(refContainer.PK, containerBO.ACN_RC_ContainerType);

			container.ContainerType.Code = null;
			container.ContainerType.ISOCode = "TS20";
			container.TotalHeight = 20m;
			container.TotalWidth = 20m;
			container.TotalLength = 50m;
			Factory.SaveForTesting();
			containerBO = new AsycudaContainerDataObjectReader(container, Logger, Factory, header).ReadIntoBusinessObject();
			AssertEquals(refContainer.PK, containerBO.ACN_RC_ContainerType);
		}
	}
}
