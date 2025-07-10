using System;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	sealed class TransportEquipmentsProviderForNctsContainerProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentsForNCTSContainerProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new TransportEquipmentsForNCTSContainerProvider(null, 1));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestContainerIdentificationNumber()
		{
			container.BC_ContainerNum = "contnr";
			AssertEquals("contnr", provider.ContainerIdentificationNumber);
		}

		public void TestNumberOfSeals()
		{
			container.BC_Seal1 = "seal1";
			container.BC_Seal2 = "seal2";
			container.Seals.AddNew().BK_SealNumber = "seal3";
			AssertEquals(3, Provider.NumberOfSeals);
		}

		public void TestSeals()
		{
			container.BC_Seal1 = "seal1";
			container.BC_Seal2 = "seal2";
			container.Seals.AddNew().BK_SealNumber = "seal3";
			AssertArrayEqualsByElements(new[] { (1, "seal1"), (2, "seal2"), (3, "seal3") },
				provider.Seals.Select(x => (x.SequenceNumber, x.Identifier)).ToArray());
		}

		public void TestDamagedSealsNotRetained()
		{
			container.BC_Seal1 = "seal1";
			container.BC_Seal2 = "seal2";
			var additionalSeal1 = container.Seals.AddNew();
			additionalSeal1.BK_SealNumber = "ADD1";
			additionalSeal1.BK_UnloadingState = "NEW";
			var additionalSeal2 = container.Seals.AddNew();
			additionalSeal2.BK_SealNumber = "ADD2";
			additionalSeal2.BK_UnloadingState = "DEC";
			var additionalSeal3 = container.Seals.AddNew();
			additionalSeal3.BK_SealNumber = "ADD3";
			additionalSeal3.BK_UnloadingState = "MIS";
			var additionalSeal4 = container.Seals.AddNew();
			additionalSeal4.BK_SealNumber = "ADD4";
			additionalSeal4.BK_UnloadingState = "DAM";

			AssertArrayEqualsByElements(new[] { (1, "seal1"), (2, "seal2"), (3, "ADD1"), (4, "ADD2"), (5, "ADD3"), },
				provider.Seals.Select(x => (x.SequenceNumber, x.Identifier)).ToArray());
		}

		public void TestGoodsReferences()
		{
			var reference1 = container.ItemNumbers.AddNew();
			reference1.CY_Order = (short)1;
			reference1.CY_Code = "ITM";
			var reference2 = container.ItemNumbers.AddNew();
			reference2.CY_Order = (short)2;
			reference2.CY_Code = "ITM";

			var reference3 = container.ItemNumbers.AddNew();
			reference3.CY_Order = (short)3;
			reference3.CY_Code = "ERR";
			AssertEquals(2, provider.GoodsReferences.Count);
		}

		protected override TransportEquipmentsForNCTSContainerProvider GetProvider() => provider;

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			container = header.EnRouteIncidents.AddNew().IncidentContainers.AddNew();
			provider = new TransportEquipmentsForNCTSContainerProvider(container, 1);
		}

		TransportEquipmentsForNCTSContainerProvider provider;
		NctsContainer container;
	}
}
