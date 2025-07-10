using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IE.Business.AES.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class TransportEquipmentWithSealsProviderTest : TransportEquipmentWithSealsBaseProviderTest
	{
		public void TestGetEquipments_EnRouteIncident()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var incident = nctsHeader.EnRouteIncidents.AddNew();

			var container1 = incident.IncidentContainers.AddNew();
			container1.ContainerNumber = "EIRU1909897";
			container1.BC_Seal1 = "1111";
			var seal1 = container1.Seals.AddNew();
			seal1.BK_SealNumber = "3333";

			container1.ItemNumbers.AddNew().CY_DataNumeric = 1;

			var container2 = incident.IncidentContainers.AddNew();
			container2.ContainerNumber = "EIRU1909855";
			container2.BC_Seal2 = "2222";
			var seal2 = container2.Seals.AddNew();
			seal2.BK_SealNumber = "3333";

			container2.ItemNumbers.AddNew().CY_DataNumeric = 1;
			container2.ItemNumbers.AddNew().CY_DataNumeric = 2;

			CombineAssertions(() =>
			{
				var equipments = TransportEquipmentWithSealsProvider.GetEquipments(incident).ToArray();
				AssertEquals("equipments.Length", 2, equipments.Length);
				AssertEquipment(equipments[0], "EIRU1909855", new[] { "2222", "3333" }, new[] { "1", "2" });
				AssertEquipment(equipments[1], "EIRU1909897", new[] { "1111", "3333" }, new[] { "1" });
			});
		}

		public void TestGetEquipments_NctsHeader()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

				AssertEquals("No containers", 0, TransportEquipmentWithSealsProvider.GetEquipments(nctsHeader).Count);

				var container1 = AddContainer(nctsHeader, "EIRU1909897");
				container1.Seal1 = "1111";
				container1.Seal2 = "2222";
				var addSeal = container1.AdditionalSeals.AddNew();
				addSeal.BK_SealNumber = "3333";

				var container2 = AddContainer(nctsHeader, "EIRU1909855");
				container2.Seal1 = "1010";

				var bill1 = nctsHeader.Bills.AddNew();
				var item1 = bill1.GoodsItems.AddNew();
				item1.BY_Description = "Item 1 Container 1";
				item1.BY_LineNo = 1;
				item1.Packages.AddNew().ContainersPivot.AddPivotFor(container1);

				var item2 = bill1.GoodsItems.AddNew();
				item2.BY_Description = "Item 1 Container 2";
				item2.BY_LineNo = 2;
				item2.Packages.AddNew().ContainersPivot.AddPivotFor(container2);

				var item3 = bill1.GoodsItems.AddNew();
				item3.BY_Description = "Item 2 Container 1";
				item3.BY_LineNo = 4;
				item3.Packages.AddNew().ContainersPivot.AddPivotFor(container2);

				var equipments = TransportEquipmentWithSealsProvider.GetEquipments(nctsHeader).ToArray();
				AssertEquals("equipments.Length", 2, equipments.Length);
				AssertEquipment(
					equipment: equipments.FirstOrDefault(equ => equ.ContainerIdentificationNumber == "EIRU1909897"),
					containerNumber: "EIRU1909897",
					seals: new[] { "1111", "2222", "3333" },
					goodsReferences: new[] { "1" }
				);
				AssertEquipment(
					equipment: equipments.FirstOrDefault(equ => equ.ContainerIdentificationNumber == "EIRU1909855"),
					containerNumber: "EIRU1909855",
					seals: new[] { "1010" },
					goodsReferences: new[] { "2", "4" }
				);
			});
		}

		public void TestGetEquipments_SingleContainer()
		{
			CombineAssertions(() =>
			{
				var nctsHeader = Factory.New<NctsHeader>();
				nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);

				AssertEquals("No containers", 0, TransportEquipmentWithSealsProvider.GetEquipments(nctsHeader).Count);

				var container1 = AddContainer(nctsHeader, "EIRU1909897");
				container1.Seal1 = "1111";
				container1.Seal2 = "2222";
				var addSeal = container1.AdditionalSeals.AddNew();
				addSeal.BK_SealNumber = "3333";

				var bill1 = nctsHeader.Bills.AddNew();
				var item1 = bill1.GoodsItems.AddNew();
				item1.BY_Description = "Item 1 Container 1";
				item1.BY_LineNo = 1;
				item1.Packages.AddNew().ContainersPivot.AddPivotFor(container1);

				var item2 = bill1.GoodsItems.AddNew();
				item2.BY_Description = "Item 2 Container 1";
				item2.BY_LineNo = 2;
				item2.Packages.AddNew().ContainersPivot.AddPivotFor(container1);

				var equipments = TransportEquipmentWithSealsProvider.GetEquipments(nctsHeader).ToArray();
				AssertEquals("equipments.Length", 1, equipments.Length);
				AssertEquipment(equipments[0], "EIRU1909897", new[] { "1111", "2222", "3333" }, new[] { "1", "2" });
			});
		}

		internal static NctsDepartureHeaderContainer AddContainer(NctsHeader header, string containerNumber)
		{
			var container = header.DepartureHeaderContainers.AddNew();
			container.BC_ContainerNum = containerNumber;
			return container;
		}

		static void AssertEquipment(ITransportEquipmentWithSeals equipment, string containerNumber, string[] seals, string[] goodsReferences)
		{
			AssertEquals("ContainerIdentificationNumber", containerNumber, equipment.ContainerIdentificationNumber);
			AssertContainsExactElementsInExactOrder("Seals", seals, equipment.Seals);
			AssertContainsExactElementsInExactOrder("GoodsReferences", goodsReferences, equipment.GoodsReferences);
		}
	}
}
