using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	sealed class TransportEquipmentWithSealsProviderTest : TransportEquipmentWithSealsBaseProviderTest
	{
		public void TestGetEquipments()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Null parameter", 0, TransportEquipmentWithSealsProvider.GetEquipments(null).Count);

				var declaration = Factory.New<JobDeclaration>();
				var package1 = declaration.Packages.AddNew();
				var package2 = declaration.Packages.AddNew();
				var package3 = declaration.Packages.AddNew();
				var package4 = declaration.Packages.AddNew();
				var package5 = declaration.Packages.AddNew();
				var package6 = declaration.Packages.AddNew();

				var entry1 = declaration.CustomsEntryHeaders.AddNew();
				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				var entry1Line1 = entry1.MergedLines.AddNew();
				var entry1Line2 = entry1.MergedLines.AddNew();
				entry1Line2.CL_LineNumber = 1;
				entry1Line1.CL_LineNumber = 2;
				var entry2Line1 = entry2.MergedLines.AddNew();
				entry2Line1.CL_LineNumber = 1;
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine1.JI_CL = entry1Line1.PK;
				var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine2.JI_CL = entry1Line2.PK;
				var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
				invoiceLine3.JI_CL = entry2Line1.PK;

				AssertEquals("No containers", 0, TransportEquipmentWithSealsProvider.GetEquipments(entry1).Count);

				var container1 = declaration.CusContainers.AddNew();
				container1.CO_ContainerNumber = "CONT3";
				container1.CO_Seal = "SLA1";
				var container2 = declaration.CusContainers.AddNew();
				container2.CO_ContainerNumber = "CONT2";
				container2.CO_SecondSeal = "SLB1";
				var container3 = declaration.CusContainers.AddNew();
				container3.CO_ContainerNumber = "CONT1";
				container3.CO_Seal = "SLC2";
				container3.CO_SecondSeal = "SLC1";

				package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
				package2.CW_ContainerNoOrEquipmentNo = container2.CO_ContainerNumber;
				package3.CW_ContainerNoOrEquipmentNo = container3.CO_ContainerNumber;
				invoiceLine1.PackagesPivot.AddPivotFor(package1);
				invoiceLine1.PackagesPivot.AddPivotFor(package3);
				invoiceLine2.PackagesPivot.AddPivotFor(package2);
				invoiceLine2.PackagesPivot.AddPivotFor(package3);

				var equipments = TransportEquipmentWithSealsProvider.GetEquipments(entry1).ToArray();
				AssertEquals("equipments.Length", 3, equipments.Length);
				AssertEquipment(equipments[0], "CONT1", new[] { "SLC2", "SLC1" }, new[] { "1", "2" });
				AssertEquipment(equipments[1], "CONT2", new[] { "SLB1" }, new[] { "1" });
				AssertEquipment(equipments[2], "CONT3", new[] { "SLA1" }, new[] { "2" });

				var container4 = declaration.CusContainers.AddNew();
				container4.CO_ContainerNumber = "CONT4";
				container4.CO_Seal = "SLD2";
				container4.CO_SecondSeal = "SLD1";
				var equipment1 = declaration.Equipments.AddNew();
				equipment1.CEQ_IdentificationNumber = "E1";
				equipment1.Seals.AddNew().BK_SealNumber = "SLE1";
				equipment1.Seals.AddNew().BK_SealNumber = "SLE3";
				equipment1.Seals.AddNew().BK_SealNumber = "SLE2";
				equipment1.Seals.ApplySort(EU.Business.Declaration.CusSeal.Schema.BK_SealNumber, System.ComponentModel.ListSortDirection.Ascending);
				var equipment2 = declaration.Equipments.AddNew();
				equipment2.CEQ_IdentificationNumber = "E2";

				package4.CW_ContainerNoOrEquipmentNo = container4.CO_ContainerNumber;
				package5.CW_ContainerNoOrEquipmentNo = equipment1.CEQ_IdentificationNumber;
				package6.CW_ContainerNoOrEquipmentNo = equipment2.CEQ_IdentificationNumber;
				invoiceLine3.PackagesPivot.AddPivotFor(package4);
				invoiceLine3.PackagesPivot.AddPivotFor(package5);
				invoiceLine3.PackagesPivot.AddPivotFor(package6);

				equipments = TransportEquipmentWithSealsProvider.GetEquipments(entry2).ToArray();
				AssertEquals("equipments.Length", 2, equipments.Length);
				AssertEquipment(equipments[0], "CONT4", new[] { "SLD2", "SLD1" }, new[] { "1" });
				AssertEquipment(equipments[1], string.Empty, new[] { "SLE1", "SLE3", "SLE2" }, new[] { "1" });
			});
		}

		public void TestAdditionalSeals()
		{
			var declaration = Factory.New<JobDeclaration>();
			var package = declaration.Packages.AddNew();
			var container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT3";
			container.CO_Seal = "SLA3";
			container.CO_SecondSeal = "SLA2";
			container.AdditionalSeals.AddNew().BK_SealNumber = "SLA2";
			container.AdditionalSeals.AddNew().BK_SealNumber = "SLA1";
			container.AdditionalSeals.ApplySort(EU.Business.Declaration.CusSeal.Schema.BK_SealNumber, System.ComponentModel.ListSortDirection.Ascending);
			package.CW_ContainerNoOrEquipmentNo = container.CO_ContainerNumber;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.PackagesPivot.AddPivotFor(package);

			var equipment = TransportEquipmentWithSealsProvider.GetEquipments(entry).First();
			AssertContainsExactElementsInExactOrder(
				"Seals should return elements with correct order.",
				new[] { "SLA3", "SLA2", "SLA2", "SLA1" },
				equipment.Seals
			);
		}

		static void AssertEquipment(ITransportEquipmentWithSeals equipment, string containerNumber, string[] seals, string[] goodsReferences)
		{
			AssertEquals("ContainerIdentificationNumber", containerNumber, equipment.ContainerIdentificationNumber);
			AssertContainsExactElementsInExactOrder("Seals", seals, equipment.Seals);
			AssertContainsExactElementsInExactOrder("GoodsReferences", goodsReferences, equipment.GoodsReferences);
		}
	}
}
