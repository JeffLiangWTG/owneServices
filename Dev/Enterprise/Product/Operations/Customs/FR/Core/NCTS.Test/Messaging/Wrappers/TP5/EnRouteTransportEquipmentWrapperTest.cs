using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using NctsHeader = Enterprise.Customs.FR.Business.NCTS.NctsHeader;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class EnRouteTransportEquipmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<EnRouteTransportEquipmentWrapper>
	{
		public void TestContainerIdentificationNumber()
		{
			AssertEquals("ContainerIdentificationNumber should be equal to container BC_ContainerNum.", "12345", Provider.ContainerIdentificationNumber);
		}

		public void TestGoodsReference()
		{
			AssertContainsExactElementsInAnyOrder(new string[] { "1", "3", "8" }, Provider.GoodsReference.Select(x => x.DeclarationGoodsItemNumber));
		}

		public void TestNumberOfSeals()
		{
			AssertEquals("NumberOfSeals should reflect container number of seals.", "3", Provider.NumberOfSeals);
		}

		public void TestSeal()
		{
			AssertContainsExactElementsInAnyOrder("Seal should aggregate First, Second and additional seals.", new string[] { "First Seal", "Second Seal", "Third Seal" }, Provider.Seal.Select(x => x.Identifier));
		}

		public void TestNoSeal()
		{
			var container = Factory.New<NctsContainer>();
			var wrapper = EnRouteTransportEquipmentWrapper.New(container);
			AssertEquals("NumberOfSeals should be 0 when container has no seal.", "0", wrapper.NumberOfSeals);
			AssertEquals("Seal should be empty when container has no seal.", 0, wrapper.Seal.Count);
		}

		protected override EnRouteTransportEquipmentWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

			var container = nctsHeader.EnRouteIncidents.AddNew().IncidentContainers.AddNew();
			container.BC_ContainerNum = "12345";
			container.BC_Seal1 = "First Seal";
			container.BC_Seal2 = "Second Seal";
			var additionalSeal =  container.Seals.AddNew();
			additionalSeal.BK_SealNumber = "Third Seal";
			var itemNumber1 = container.ItemNumbers.AddNew();
			itemNumber1.CY_DataNumeric = 1;
			var itemNumber2 = container.ItemNumbers.AddNew();
			itemNumber2.CY_DataNumeric = 3;
			var itemNumber3 = container.ItemNumbers.AddNew();
			itemNumber3.CY_DataNumeric = 8;
			var itemNumber4 = container.ItemNumbers.AddNew();
			itemNumber4.CY_DataNumeric = 0;
			var wrapper = EnRouteTransportEquipmentWrapper.New(container);
			return wrapper;
		}
	}
}
