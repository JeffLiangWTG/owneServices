using System;
using System.Linq;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.CusTempStorage;
using TemporaryStorageContainer = Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStorageContainer;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	class TransportEquipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("Should throw ArgumentException with null container.", () => TransportEquipmentProvider.New(null));
		}

		public void TestContainerPackedStatus()
		{
			container.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.NotEmpty;
			AssertEquals("ContainerPackedStatus: ACN_EmptyFullIndicator", EmptyFullIndicatorList.Codes.NotEmpty, GetProvider().ContainerPackedStatus);
		}

		public void TestContainerId()
		{
			container.ACN_ContainerNumber = "CNT001";
			AssertEquals("ContainerIdentificationNumber: ACN_ContainerNumber", "CNT001", GetProvider().ContainerId);
		}

		public void TestNumberOfSeals()
		{
			AssertEquals("NumberOfSeals: Count of Seals", "0", GetProvider().NumberOfSeals);

			container.ACN_ContainerNumber = "TEST123";
			container.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.NotEmpty;
			container.ACN_Seal1 = "SEAL1";
			container.ACN_Seal2 = "SEAL2";
			container.ACN_Seal3 = "SEAL3";
			var seal4 = container.AdditionalSeals.AddNew();
			seal4.BK_SealNumber = "SEAL4";
			AssertEquals("NumberOfSeals: Count of Seals", "4", GetProvider().NumberOfSeals);
		}

		public void TestSeal()
		{
			container.ACN_ContainerNumber = "TEST123";
			container.ACN_EmptyFullIndicator = EmptyFullIndicatorList.Codes.NotEmpty;
			container.ACN_Seal1 = "SEAL1";

			var provider = GetProvider();
			AssertEquals("Seal Count (single)", 1, provider.Seal.Count);
			AssertEquals("Seal 1", "SEAL1", provider.Seal.First());

			container.ACN_Seal2 = "SEAL2";
			container.ACN_Seal3 = "SEAL3";
			var seal4 = container.AdditionalSeals.AddNew();
			seal4.BK_SealNumber = "SEAL4";

			provider = GetProvider();
			AssertEquals("Seal Count with additional seals", 4, provider.Seal.Count);
			AssertSequencesEqual(new[] { "SEAL1", "SEAL2", "SEAL3", "SEAL4" }, provider.Seal);
		}

		public void TestGoodsReference()
		{
			AssertEquals("GoodsReference", 0, Provider.GoodsReference.Count);
		}

		protected override TransportEquipmentProvider GetProvider() => TransportEquipmentProvider.New(container);

		TemporaryStorageHeader header;
		TemporaryStorageContainer container;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			container = header.Containers.AddNew();
		}
	}
}
