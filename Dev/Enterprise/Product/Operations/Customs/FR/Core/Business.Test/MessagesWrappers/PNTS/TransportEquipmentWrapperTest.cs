using System.Linq;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class TransportEquipmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentWrapper>
	{
		public void TestContainerIdentificationNumber()
		{
			AssertEquals("Wrapper ContainerIdentificationNumber should equal ACN_ContainerNumber.", "ContainerNumber", Provider.ContainerIdentificationNumber);
		}

		public void TestContainerPackedStatus()
		{
			AssertEquals("Wrapper ContainerPackedStatus should equal ACN_EmptyFullIndicator.", "FUL", Provider.ContainerPackedStatus);
		}

		public void TestNumberOfSeals()
		{
			AssertEquals("Wrapper ContainerIdentificationNumber should count non empty seals.", "5", Provider.NumberOfSeals);
		}

		public void TestSeal()
		{
			AssertContainsExactElementsInAnyOrder("Wrapper ContainerIdentificationNumber count.", new string[] { "SEAL1", "SEAL2", "SEAL3", "SEAL4", "SEAL5" }, Provider.Seal.Select(x => x.Identifier).ToArray());
		}

		public void TestContainerWithoutSeal()
		{
			var container = Factory.New<TemporaryStorageContainer>();
			container.ACN_ContainerNumber = "ContainerNumber";
			container.ACN_EmptyFullIndicator = "FUL";
			var wrapper = TransportEquipmentWrapper.New(container);
			AssertEquals("Wrapper should return an empty collection of Seals.", 0, wrapper.Seal.Count);
		}

		protected override TransportEquipmentWrapper GetProvider()
		{
			var container = Factory.New<TemporaryStorageContainer>();
			container.ACN_ContainerNumber = "ContainerNumber";
			container.ACN_EmptyFullIndicator = "FUL";
			container.ACN_Seal1 = "SEAL1";
			container.ACN_Seal2 = "SEAL2";
			container.ACN_Seal3 = "SEAL3";

			var cusSeal1 = container.AdditionalSeals.AddNew();
			cusSeal1.BK_SealNumber = "SEAL4";

			var cusSeal2 = container.AdditionalSeals.AddNew();
			cusSeal2.BK_SealNumber = "SEAL5";

			return TransportEquipmentWrapper.New(container);
		}
	}
}
