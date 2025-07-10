using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class TransportEquipmentWrapperTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentWrapper>
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
			AssertEquals("NumberOfSeals should reflect container number of seals", "3", Provider.NumberOfSeals);

			var provider = GetProviderForArrivalHeaderContainer();
			AssertEquals("NumberOfSeals should reflect container number of seals for Arrival.", "2", provider.NumberOfSeals);
		}

		public void TestSeal()
		{
			AssertContainsExactElementsInAnyOrder("Seal should aggregate First, Second and additional seals.", new string[] { "First Seal", "Second Seal", "Third Seal" }, Provider.Seal.Select(x => x.Identifier));

			var provider = GetProviderForArrivalHeaderContainer();
			AssertContainsExactElementsInAnyOrder("Seal should aggregate First, Second and Seals for ArrivalHeaderContainer.", new string[] { "First Seal", "Second Seal", "Third Seal", "Fourth Seal" }, provider.Seal.Select(x => x.Identifier));
		}

		public void TestNoSeal()
		{
			var container = Factory.New<FRNctsDepartureHeaderContainer>();
			var wrapper = TransportEquipmentWrapper.New(container, new List<string>());
			AssertEquals("NumberOfSeals should be 0 when container has no seal.", "0", wrapper.NumberOfSeals);
			AssertEquals("Seal should be empty when container has no seal.", 0, wrapper.Seal.Count);
		}

		TransportEquipmentWrapper GetProviderForArrivalHeaderContainer()
		{
			var container = Factory.New<NctsArrivalHeaderContainer>();
			container.BC_ContainerNum = "12345";
			container.BC_Seal1 = "First Seal";
			container.BC_Seal2 = "Second Seal";
			var additionalSeal = container.Seals.AddNew();
			additionalSeal.BK_SealNumber = "Third Seal";
			var additionalSeal2 = container.Seals.AddNew();
			additionalSeal2.BK_SealNumber = "Fourth Seal";
			var wrapper = TransportEquipmentWrapper.New(container, new List<string> { "1", "3", "8", "9" });
			return wrapper;
		}

		protected override TransportEquipmentWrapper GetProvider()
		{
			var container = Factory.New<FRNctsDepartureHeaderContainer>();
			container.BC_ContainerNum = "12345";
			container.BC_Seal1 = "First Seal";
			container.BC_Seal2 = "Second Seal";
			var additionalSeal = container.AdditionalSeals.AddNew();
			additionalSeal.BK_SealNumber = "Third Seal";
			var wrapper = TransportEquipmentWrapper.New(container, new List<string> { "1", "3", "8" });
			return wrapper;
		}
	}
}
