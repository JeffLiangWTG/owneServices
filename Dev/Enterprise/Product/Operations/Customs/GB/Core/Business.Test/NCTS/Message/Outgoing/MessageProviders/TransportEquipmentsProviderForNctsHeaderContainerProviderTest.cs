using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class TransportEquipmentsProviderForNctsHeaderContainerProviderTest : DataProviderTestCase<TransportEquipmentsForNCTSHeaderContainerProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TransportEquipmentsForNCTSHeaderContainerProvider(null, 1));
		}

		public void TestGoodsReferences()
		{
			var bill = container.Header.Bills.AddNew();

			var cusInBondCargoDesc1 = bill.GoodsItems.AddNew();
			var package1 = cusInBondCargoDesc1.Packages.AddNew();
			cusInBondCargoDesc1.BY_DeclarationGoodsItemNumber = 1;
			package1.ContainersPivot.AddPivotFor(container);

			var cusInBondCargoDesc2 = bill.GoodsItems.AddNew();
			var package2 = cusInBondCargoDesc2.Packages.AddNew();
			cusInBondCargoDesc2.BY_DeclarationGoodsItemNumber = 2;
			package2.ContainersPivot.AddPivotFor(container);

			AssertContainsExactElementsInAnyOrder(new[] { (1, 1), (2, 2) }, Provider.GoodsReferences.Select(x => (x.SequenceNumber, x.DeclarationGoodsItemNumber)));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestContainerIdentificationNumber()
		{
			container.BC_ContainerNum = "contnr";
			AssertEquals("contnr", Provider.ContainerIdentificationNumber);
		}

		public void TestNumberOfSeals()
		{
			container.BC_Seal1 = "seal1";
			container.BC_Seal2 = "seal2";
			container.AdditionalSeals.AddNew().BK_SealNumber = "seal3";
			AssertEquals(3, Provider.NumberOfSeals);
		}

		public void TestSeals()
		{
			container.BC_Seal1 = "seal1";
			container.BC_Seal2 = "seal2";
			container.AdditionalSeals.AddNew().BK_SealNumber = "seal3";
			AssertArrayEqualsByElements(new[] { (1, "seal1"), (2, "seal2"), (3, "seal3") },
				provider.Seals.Select(x => (x.SequenceNumber, x.Identifier)).ToArray());
		}

		protected override TransportEquipmentsForNCTSHeaderContainerProvider GetProvider() => provider;

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			container = header.DepartureHeaderContainers.AddNew();
			provider = new TransportEquipmentsForNCTSHeaderContainerProvider(container, 1);
		}

		TransportEquipmentsForNCTSHeaderContainerProvider provider;
		NctsDepartureHeaderContainer container;
	}
}
