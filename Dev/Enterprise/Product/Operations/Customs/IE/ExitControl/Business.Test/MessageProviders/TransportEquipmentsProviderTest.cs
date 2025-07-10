using System.Linq;
using CargoWise.Customs.IE.MessageContracts.Interfaces;

namespace Enterprise.Customs.IE.ExitControl.Business.AES.Testing
{
	class TransportEquipmentsProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportEquipmentsProvider, ITransportEquipmentWithSeals>
	{
		public void TestSeals()
		{
			container.AllSealNumbers.AddNew().BK_SealNumber = "SEALTEST123";
			var seals = IProvider.Seals.ToArray();
			AssertEquals("No of Seals", 1, seals.Length);
			AssertEquals("Seal data", "SEALTEST123", seals[0]);
		}

		public void TestContainerIdentificationNumber()
		{
			container.CXN_IsEquipment = true;
			container.CXN_ContainerNumber = "XYZ123";
			AssertNullOrEmpty("IsEquipment should not have identification number", GetProvider().ContainerIdentificationNumber);

			container.CXN_IsEquipment = false;
			container.CXN_ContainerNumber = "XYZ123";
			AssertEquals("Container Identification number", "XYZ123", GetProvider().ContainerIdentificationNumber);
		}

		public void TestGoodsReferences()
		{
			goodsReferences = new[] { "1", "5" };
			var references = IProvider.GoodsReferences.ToArray();
			AssertEquals("No of Goods References", 2, references.Length);
			AssertEquals("Goods References", "1", references[0]);
		}

		public void TestContainerIsFull()
		{
			AssertEquals("Container is full", false, Provider.ContainerIsFull);
		}

		protected override TransportEquipmentsProvider GetProvider() => new TransportEquipmentsProvider(container, goodsReferences);

		protected override void SetUp()
		{
			base.SetUp();
			goodsReferences = new[] { "1" };
			header = Factory.New<CusExitHeader>();
			consignment = header.CusExitConsignments.AddNew();
			consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 1;
			pivot = (CusExitConsignmentPivot)consignmentItem.CusExitConsignmentPackagePivots.AddNew();
			container = header.CusExitContainers.AddNew();
			pivot.CNP_CXN_Container = container.PK;
		}
		CusExitHeader header;
		CusExitConsignment consignment;
		CusExitConsignmentItem consignmentItem;
		CusExitConsignmentPivot pivot;
		CusExitContainer container;
		string[] goodsReferences;
	}
}
