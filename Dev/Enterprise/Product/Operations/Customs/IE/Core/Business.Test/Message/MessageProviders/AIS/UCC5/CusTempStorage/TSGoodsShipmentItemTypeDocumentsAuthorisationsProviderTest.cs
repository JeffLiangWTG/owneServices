using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage.Testing
{
	sealed class TSGoodsShipmentItemTypeDocumentsAuthorisationsProviderTest : DataProviderTestCase<TSGoodsShipmentItemTypeDocumentsAuthorisationsProvider>
	{
		public void TestSimplifiedDeclarationDocuments()
		{
			packedItem.PreviousDocuments.AddNew().CSI_Code = "PRE";
			var documents = GetProvider().SimplifiedDeclarationDocuments.ToArray();
			AssertEquals("PRE", documents[0].PreviousDocumentType);
		}

		public void TestAdditionalInformations()
		{
			var info1 = packedItem.AdditionalInfos.AddNew();
			info1.CSI_SubType = "INF";
			info1.CSI_Code = "111";
			var info2 = packedItem.AdditionalInfos.AddNew();
			info2.CSI_SubType = "TRA";
			info2.CSI_Code = "222";
			var infos = GetProvider().AdditionalInformations.ToArray();
			AssertEquals("Only take INF value", 1, infos.Length);
			AssertEquals("111", infos[0].Code);
		}

		public void TestProducedDocuments()
		{
			packedItem.SupportingDocuments.AddNew().CSI_Code = "SUP";
			var documents = GetProvider().ProducedDocuments.ToArray();
			AssertEquals("SUP", documents[0].Type);
		}

		protected override TSGoodsShipmentItemTypeDocumentsAuthorisationsProvider GetProvider() => new (packedItem);

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<TemporaryStorageHeader>();
			bill = header.Bills.AddNew();
			packedItem = bill.PackedItems.AddNew();
		}
		TemporaryStorageBill bill;
		TemporaryStoragePackedItem packedItem;
	}
}
