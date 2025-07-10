using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using TemporaryStoragePackedItem = Enterprise.Customs.EU.Business.CusTempStorage.TemporaryStoragePackedItem;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class ConsignmentItemProviderTest : DataProviderTestCase<ConsignmentItemProvider>
	{
		public void TestIConsignmentItem()
		{
			Assert("Should implement IConsignmentItem", Provider is IConsignmentItem);
		}

		public void TestPackagings()
		{
			Assert("Should be IReadOnlyCollection<IPackaging>", Provider.Packagings is IReadOnlyCollection<IPackaging>);
			AssertEquals("One Packaging for each pack", 1, Provider.Packagings.Count);

			var packItem = Factory.New<TemporaryStoragePackedItem>();
			var provider = new ConsignmentItemProvider(packItem);
			AssertEquals("Should be empty when pack is null", 0, provider.Packagings.Count);
		}

		public void TestGoodsItemNumber()
		{
			AssertEquals("GoodsItemNumber", "1", Provider.GoodsItemNumber);
		}

		public void TestAdditionalSupplyChainActors()
		{
			var actor = PackItem.SupplyChainActors.AddNew();
			actor.CFR_Reference = "IdentificationNo";
			actor.CFR_Code = "CS";
			Assert("Should be IReadOnlyCollection<AdditionalSupplyChainActorProvider>", Provider.AdditionalSupplyChainActors is IReadOnlyCollection<AdditionalSupplyChainActorProvider>);
			AssertEquals("AdditionalSupplyChainActor number", 1, Provider.AdditionalSupplyChainActors.Count);
			AssertEquals("AdditionalSupplyChainActor ID", "IdentificationNo", Provider.AdditionalSupplyChainActors.FirstOrDefault().ID);
			AssertEquals("AdditionalSupplyChainActor Role", "CS", Provider.AdditionalSupplyChainActors.FirstOrDefault().Role);

			var packItem = Factory.New<TemporaryStoragePackedItem>();
			var provider = new ConsignmentItemProvider(packItem);
			AssertEquals("Should be empty when AdditionalSupplyChainActor is null", 0, provider.AdditionalSupplyChainActors.Count);
		}

		public void TestCommodity()
		{
			Assert("Should be ICommodity09", Provider.Commodity is ICommodity09);

			PackItem.API_GoodsDescription = "description";
			PackItem.API_ChemicalSubstanceCode = "code1";
			AssertEquals("Commodity DescriptionOfGoods", "description", Provider.Commodity.DescriptionOfGoods);
			AssertEquals("Commodity CusCode", "code1", Provider.Commodity.CusCode);
		}

		public void TestGoodsMeasure()
		{
			PackItem.API_GrossWeight = 1000m;
			PackItem.API_GrossWeightUQ = "KG";
			AssertEquals("GoodsMeasure GrossMass", 1000m, Provider.GoodsMeasure);
		}

		public void TestPreviousDocuments()
		{
			PackItem.PreviousDocuments.AddNew();
			PackItem.PreviousDocuments.AddNew();
			var previousDocuments = Provider.PreviousDocuments;
			AssertEquals("Count", 2, previousDocuments.Count);
			Assert("Should be IReadOnlyCollection<PreviousDocumentProvider>", previousDocuments is IReadOnlyCollection<PreviousDocumentProvider>);
		}

		public void TestSupportingDocuments()
		{
			PackItem.SupportingDocuments.AddNew();
			PackItem.SupportingDocuments.AddNew();
			var supportingDocuments = Provider.SupportingDocuments;
			AssertEquals("Count", 2, supportingDocuments.Count);
			Assert("Should be IReadOnlyCollection<DocumentProvider>", supportingDocuments is IReadOnlyCollection<DocumentProvider>);
		}

		public void TestTransportEquipments()
		{
			AssertEquals("TransportEquipment should have 1 item.", 1, Provider.TransportEquipments.Count);
			AssertType<TransportEquipmentProvider>("TransportEquipment should return an object of TransportEquipment06Provider.", Provider.TransportEquipments.First());
		}

		public void TestAdditionalInformations()
		{
			PackItem.AdditionalInfos.AddNew().CSI_SubType = "INF";
			PackItem.AdditionalInfos.AddNew().CSI_SubType = "INF";
			var additionalInformations = Provider.AdditionalInformations;
			AssertEquals("Count", 2, additionalInformations.Count);
			Assert("Should be IReadOnlyCollection<AdditionalInformationProvider>", additionalInformations is IReadOnlyCollection<AdditionalInformationProvider>);
		}

		public void TestAdditionalReferences()
		{
			PackItem.AdditionalInfos.AddNew().CSI_SubType = "REF";
			PackItem.AdditionalInfos.AddNew().CSI_SubType = "REF";
			var additionalReferences = Provider.AdditionalReferences;
			AssertEquals("Count", 2, additionalReferences.Count);
			Assert("Should be IReadOnlyCollection<DocumentProvider>", additionalReferences is IReadOnlyCollection<DocumentProvider>);
		}

		protected override ConsignmentItemProvider GetProvider() => new ConsignmentItemProvider(PackItem);

		TemporaryStoragePackedItem PackItem
		{
			get
			{
				if (packItem == null)
				{
					var header = Factory.New<TemporaryStorageHeader>();
					var bill = header.Bills.AddNew();
					var pack = bill.Packs.AddNew();
					packItem = bill.PackedItems.AddNew();
					var container = header.Containers.AddNew();
					pack.ContainerPK = container.PK;
					var pivot = packItem.AsycudaPackPackedItemPivots.AddNew();
					pivot.APP_APA_Pack = pack.PK;
				}
				return packItem;
			}
		}
		TemporaryStoragePackedItem packItem;
	}
}
