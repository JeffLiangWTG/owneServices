using System;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.Business.AESVersion3_0.Testing
{
	sealed class TransportEquipmentProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentProvider>
	{
		public void TestConstructor_ContainerNull()
		{
			AssertExceptionThrown<ArgumentException>(() => new TransportEquipmentProvider(null, consignment, true));
		}

		public void TestConstructor_ConsignmentNull()
		{
			AssertExceptionThrown<ArgumentException>(() => new TransportEquipmentProvider(container, null, true));
		}

		public void TestContainerIdentificationNumber()
		{
			container.CXN_IsEquipment = false;
			container.CXN_ContainerNumber = "123";
			AssertEquals("123", GetProvider().ContainerIdentificationNumber);
		}

		public void TestContainerIdentificationNumber_IsEquiment()
		{
			container.CXN_IsEquipment = true;
			container.CXN_ContainerNumber = "123";
			AssertEquals(string.Empty, GetProvider().ContainerIdentificationNumber);
		}

		public void TestSealIdentifiers() => CombineAssertions(() =>
		{
			container.AllSealNumbers.AddNew().BK_SealNumber = "N1";
			container.AllSealNumbers.AddNew().BK_SealNumber = "A1";
			container.AllSealNumbers.AddNew().BK_SealNumber = "B1";
			AssertContainsExactElementsInAnyOrder("SealIdentifiers  when mapSeals = true", new[] { "N1", "A1", "B1" }, GetProvider().SealIdentifiers);

			mapNumberOfSeals = false;
			AssertEquals("SealIdentifiers when mapSeals = false", 0, GetProvider().SealIdentifiers.Count);
		});

		public void TestSealIdentifiers_FirstSeal()
		{
			container.AllSealNumbers.AddNew().BK_SealNumber = "N1";
			AssertContainsExactElementsInAnyOrder("SealIdentifiers: first Seal", new[] { "N1" }, GetProvider().SealIdentifiers);
		}

		public void TestSealIdentifiers_NoSeals()
		{
			AssertEquals("SealIdentifiers when no seals", 0, GetProvider().SealIdentifiers.Count);
		}

		public void TestDeclarationGoodsItemNumbers()
		{
			var item1 = consignment.CusExitConsignmentItems.AddNew();
			item1.CCI_LineNumber = 1;
			item1.CusExitConsignmentPackagePivots.AddNew().CNP_CXN_Container = container.PK;
			var item2 = consignment.CusExitConsignmentItems.AddNew();
			item2.CCI_LineNumber = 2;
			item2.CusExitConsignmentPackagePivots.AddNew().CNP_CXN_Container = container.PK;
			var item3 = consignment.CusExitConsignmentItems.AddNew();
			item3.CCI_LineNumber = 3;

			var consignment2 = header.CusExitConsignments.AddNew();
			var item4 = consignment2.CusExitConsignmentItems.AddNew();
			item4.CCI_LineNumber = 4;
			item4.CusExitConsignmentPackagePivots.AddNew().CNP_CXN_Container = container.PK;

			AssertContainsExactElementsInAnyOrder(new[] { 1, 2 }, GetProvider().DeclarationGoodsItemNumbers);
		}

		public void TestNumberOfSeals()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No seals are captured, map condition is true", 0, GetProvider().NumberOfSeals);

				container.AllSealNumbers.AddNew().BK_SealNumber = "N1";

				AssertEquals("First seal is captured, map condition is true", 1, GetProvider().NumberOfSeals);

				container.AllSealNumbers.AddNew().BK_SealNumber = "A1";
				container.AllSealNumbers.AddNew().BK_SealNumber = "B1";

				AssertEquals("AllSealNumbers are captured, map condition is true", 3, GetProvider().NumberOfSeals);

				mapNumberOfSeals = false;
				AssertNull("Map condition is false", GetProvider().NumberOfSeals);
			});
		}

		public void TestNumberOfSeals_NoContainer()
		{
			CombineAssertions(() =>
			{
				var provider = new TransportEquipmentProvider(consignment, mapNumberOfSeals: true);
				AssertEquals("No container/equipment, mapNumberofSeals true", 0, provider.NumberOfSeals);

				provider = new TransportEquipmentProvider(consignment, mapNumberOfSeals: false);
				AssertNull("No container/equipment, mapNumberofSeals false", provider.NumberOfSeals);
			});
		}

		public void TestDeclarationGoodsItemNumbers_NoContainer()
		{
			var item1 = consignment.CusExitConsignmentItems.AddNew();
			item1.CCI_LineNumber = 1;
			var item2 = consignment.CusExitConsignmentItems.AddNew();
			item2.CCI_LineNumber = 2;

			var provider = new TransportEquipmentProvider(consignment, false);

			AssertContainsExactElementsInAnyOrder("All line numbers from Consignment Items are mapped when there is no container", new[] { 1, 2 }, provider.DeclarationGoodsItemNumbers);
		}

		protected override TransportEquipmentProvider GetProvider() => new TransportEquipmentProvider(container, consignment, mapNumberOfSeals);

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusExitHeader>();
			container = header.CusExitContainers.AddNew();
			consignment = header.CusExitConsignments.AddNew();
			mapNumberOfSeals = true;
		}
		CusExitHeader header;
		CusExitContainer container;
		CusExitConsignment consignment;
		bool mapNumberOfSeals;
	}
}
