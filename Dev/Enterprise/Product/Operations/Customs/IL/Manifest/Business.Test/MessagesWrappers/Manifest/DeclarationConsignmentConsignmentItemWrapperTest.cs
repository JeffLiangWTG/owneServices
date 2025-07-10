using CargoWise.Customs.IL.MessageDefinitions.MAN.REQ_170;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IL.Manifest.Business.MessagesWrappers;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class DeclarationConsignmentConsignmentItemWrapperTest : DataProviderTestCase<IDeclarationConsignmentConsignmentItem>
	{
		public void TestGoodsStatusCode()
		{
			AssertEquals("GoodsStatusCode should be equal D", "D", Provider.GoodsStatusCode.Value);
			asycudaPackedItem.API_PackStatus = "X";
			AssertEquals("GoodsStatusCode should be equal N", "N", Provider.GoodsStatusCode.Value);
		}

		public void TestAdditionalInformation()
		{
			AssertNotNull("AdditionalInformation", Provider.AdditionalInformation);
			AssertEquals("There should be 2 items", 2, Provider.AdditionalInformation.Count);
		}

		public void TestCommodity()
		{
			AssertNotNull("Commodity", Provider.Commodity);
			AssertEquals("There should be 1 item", 1, Provider.Commodity.Count);
		}

		public void TestGoodsMeasure()
		{
			AssertNotNull("GoodsMeasure", Provider.GoodsMeasure);
			AssertEquals("There should be 2 items", 2, Provider.GoodsMeasure.Count);
		}

		public void TestGovernmentProcedure()
		{
			AssertNotNull("GovernmentProcedure", Provider.GovernmentProcedure);
			AssertEquals("There should be 1 item", 1, Provider.GovernmentProcedure.Count);
		}

		public void TestPackaging()
		{
			AssertNotNull("Packaging", Provider.Packaging);
			AssertEquals("There should be 2 items", 2, Provider.Packaging.Count);
		}

		public void TestTransportEquipment()
		{
			AssertNotNull("TransportEquipment", Provider.TransportEquipment);
			AssertEquals("There should be 1 item", 1, Provider.TransportEquipment.Count);
		}

		public void TestSequenceNumeric()
		{
			AssertEquals("SequenceNumeric should be equal to the expected value", 1m, Provider.SequenceNumeric);
		}

		public void TestUcr()
		{
			AssertNotNull("UCR", Provider.Ucr);
			AssertEquals("UCR should have 0 items", 0, Provider.Ucr.Count);
		}

		public void TestNewOrNull()
		{
			AssertNull(DeclarationConsignmentConsignmentItemWrapper.NewOrNull(null));
			AssertNotNull(DeclarationConsignmentConsignmentItemWrapper.NewOrNull(Factory.New<AsycudaPackedItem>()));
		}

		protected override IDeclarationConsignmentConsignmentItem GetProvider()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var asycudaPack1 = bill.Packs.AddNew();
			var asycudaPack2 = bill.Packs.AddNew();
			var asycudaPack3 = bill.Packs.AddNew();
			var container1 = header.Containers.AddNew();
			var container2 = header.Containers.AddNew();
			asycudaPack1.ContainerPK = container1.PK;
			asycudaPack2.ContainerPK = container2.PK;

			asycudaPackedItem = bill.PackedItems.AddNew();
			asycudaPackedItem.API_LineNo = 1;
			asycudaPackedItem.PackagesPivot.AddPivotFor(asycudaPack1);
			asycudaPackedItem.PackagesPivot.AddPivotFor(asycudaPack3);
			asycudaPackedItem.API_PackStatus = "D";
			asycudaPackedItem.AdditionalInfos.AddNew();
			asycudaPackedItem.AdditionalInfos.AddNew();

			return DeclarationConsignmentConsignmentItemWrapper.NewOrNull(asycudaPackedItem);
		}

		AsycudaPackedItem asycudaPackedItem;
	}
}
