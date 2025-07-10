using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.PNTS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	public class ConsignmentItemWrapperTest : ConsignmentDataProviderTestCase<ConsignmentItemWrapper>
	{
		public void TestAdditionalInformation()
		{
			AssertType<Collection<IAdditionalInformation>>("AdditionalInformation type", Provider.AdditionalInformation);
			AssertContainsExactElementsInAnyOrder("There should be 2 distinct (filtered on code) elements in AdditionalInformation, matching entry additional infos of type INF. ", new string[] { "INF1", "INF2" }, Provider.AdditionalInformation.Select(x => x.Code));

			var wrapper = ConsignmentHouseLevelWrapper.New(Factory.New<TemporaryStorageBill>());
			AssertEquals("There should be 0 AdditionalInformation.", 0, wrapper.AdditionalInformation.Count);
		}

		public void TestAdditionalReference()
		{
			AssertType<Collection<IAdditionalReference>>("AdditionalReference type", Provider.AdditionalReference);
			AssertContainsExactElementsInAnyOrder("There should be 3 distinct (filtered on type and reference number) elements in AdditionalReference, matching entry additional infos of type REF. ", new string[] { "CD1|REF1", "CD1|REF1", "CD2|REF1", "CD2|REF2" }, Provider.AdditionalReference.Select(x => x.Type + "|" + x.ReferenceNumber));

			var wrapper = ConsignmentHouseLevelWrapper.New(Factory.New<TemporaryStorageBill>());
			AssertEquals("There should be 0 AdditionalReference.", 0, wrapper.AdditionalReference.Count);
		}

		public void TestAdditionalSupplyChainActor()
		{
			AssertType<Collection<IAdditionalSupplyChainActor>>("AdditionalSupplyChainActor type", Provider.AdditionalSupplyChainActor);
			AssertContainsExactElementsInAnyOrder("There should be 3 distinct (filtered on role and reference number) elements in AdditionalSupplyChainActor, matching entry instruction.", new string[] { "ABC|SCREF1", "ABC|SCREF1", "ABC|SCREF2", "DEF|SCREF2" }, Provider.AdditionalSupplyChainActor.Select(x => x.Role + "|" + x.IdentificationNumber));

			var wrapper = ConsignmentHouseLevelWrapper.New(Factory.New<TemporaryStorageBill>());
			AssertEquals("There should be 0 AdditionalSupplyChainActor.", 0, wrapper.AdditionalSupplyChainActor.Count);
		}

		public void TestCommodity()
		{
			AssertEquals("Wrapper Commodity should use CommodityWrapper as sub wrapper.", "GoodsDescription1", Provider.Commodity.DescriptionOfGoods);
		}

		public void TestGoodsItemNumber()
		{
			AssertEquals("Wrapper GoodsItemNumber should be equal to item.API_LineNo.", "1", Provider.GoodsItemNumber);
		}

		public void TestPackaging()
		{
			CombineAssertions("Wrapper Packaging property should use PackagingWrapper sub wrapper", () =>
			{
				AssertContainsExactElementsInExactOrder("TypeOfPackages", new string[] { "CTN", "CTN", "CTN" }, Provider.Packaging.Select(x => x.TypeOfPackages));
				AssertContainsExactElementsInExactOrder("ShippingMarks", new string[] { "Marks & numbers 1", "Marks & numbers 2", "Marks & numbers 3" }, Provider.Packaging.Select(x => x.ShippingMarks));
				AssertContainsExactElementsInExactOrder("NumberOfPackages", new string[] { "3", "5", "7" }, Provider.Packaging.Select(x => x.NumberOfPackages));
			});

			var pack = Factory.New<TemporaryStoragePackedItem>();
			var wrapper = ConsignmentItemWrapper.New(pack);
			AssertEquals("Wrapper Packaging count should be equal 0.", 0, wrapper.Packaging.Count);
		}

		public void TestPreviousDocument()
		{
			AssertEquals("Wrapper PreviousDocument is null if no previous document.", null, Provider.PreviousDocument);

			var previousDocument = item.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "N355";
			previousDocument.CSI_LineNo = 1;
			previousDocument.CSI_ReferenceNumber = "10";

			var provider = ConsignmentItemWrapper.New(item);
			AssertType<PreviousDocumentWrapper>("Wrapper PreviousDocument is not null if there is a previous document.", provider.PreviousDocument);
			AssertEquals("1", provider.PreviousDocument.GoodsItemIdentifier);
			AssertEquals("10", provider.PreviousDocument.ReferenceNumber);
			AssertEquals("N355", provider.PreviousDocument.Type);
		}

		public void TestSupportingDocument()
		{
			AssertType<Collection<ISupportingDocument>>("SupportingDocument type", Provider.SupportingDocument);
			AssertContainsExactElementsInAnyOrder("There should be 4 elements in SupportingDocument, matching all entry supporting documents.", new string[] { "N380|SUP1", "N380|SUP1", "N380|SUP2", "N270|SUP1" }, Provider.SupportingDocument.Select(x => x.Type + "|" + x.ReferenceNumber));

			var wrapper = GoodsShipmentWrapper.New(Factory.New<Declaration.CusEntryHeader>());
			AssertEquals("There should be 0 SupportingDocument.", 0, wrapper.SupportingDocument.Count);
		}

		public void TestTransportEquipment()
		{
			CombineAssertions("Wrapper TransportEquipment property should use TransportEquipmentWrapper sub wrapper", () =>
			{
				AssertContainsExactElementsInExactOrder("NumberOfSeals", new string[] { "3", "2" }, Provider.TransportEquipment.Select(x => x.NumberOfSeals));
				AssertContainsExactElementsInExactOrder("ContainerPackedStatus", new string[] { "FUL", "FUL" }, Provider.TransportEquipment.Select(x => x.ContainerPackedStatus));
				AssertContainsExactElementsInExactOrder("ContainerIdentificationNumber", new string[] { "ContainerNumber1", "ContainerNumber3" }, Provider.TransportEquipment.Select(x => x.ContainerIdentificationNumber));
			});

			var pack = Factory.New<TemporaryStoragePackedItem>();
			var packagesPivot = pack.PackagesPivot.AddNew();
			var packagesPivotPack = Factory.New<AsycudaPack>();
			packagesPivot.APP_APA_Pack = packagesPivotPack.PK;
			var wrapper = ConsignmentItemWrapper.New(pack);
			AssertEquals("Wrapper TransportEquipment count should be equal 0.", 0, wrapper.TransportEquipment.Count);
		}

		public void TestWeight()
		{
			AssertEquals("Wrapper Weight should use WeightWrapper as sub wrapper.", 55.33m, Provider.Weight.GrossMass);
		}

		protected TemporaryStoragePackedItem GetTestItem()
		{
			return GetPackedItem();
		}

		TemporaryStoragePackedItem GetPackedItem()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			var bill = temporaryStorageHeader.Bills.AddNew();

			var container1 = CreateContainer(temporaryStorageHeader, "ContainerNumber1", "FUL", "SEAL1", "SEAL2", "SEAL3");
			var container2 = CreateContainer(temporaryStorageHeader, "ContainerNumber2", "FUL", "SEAL1");
			var container3 = CreateContainer(temporaryStorageHeader, "ContainerNumber3", "FUL", "SEAL1", "SEAL2");
			var container4 = CreateContainer(temporaryStorageHeader, "ContainerNumber4", "FUL", "SEAL1");

			var pack1 = CreatePack(bill, container1, 3, "Marks & numbers 1", "CTN");
			var pack2 = CreatePack(bill, container1, 5, "Marks & numbers 2", "CTN");
			var pack3 = CreatePack(bill, container3, 7, "Marks & numbers 3", "CTN");
			var pack4 = CreatePack(bill, container4, 9, "Marks & numbers 4", "CTN");

			var packedItem1 = (TemporaryStoragePackedItem)pack1.PackedItems.AddNewPackedItem();
			packedItem1.API_GoodsDescription = "GoodsDescription1";
			packedItem1.API_Tariff = "2345167890";
			packedItem1.API_GrossWeight = 55.33m;

			packedItem1.AsycudaPackPackedItemPivots.AddPivotFor(pack2);
			packedItem1.AsycudaPackPackedItemPivots.AddPivotFor(pack3);

			var informationAdditionalInfo1 = packedItem1.AdditionalInfos.AddNew();
			informationAdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo1.CSI_Code = "INF1";
			informationAdditionalInfo1.CSI_Description = "INFDescription1";

			var informationAdditionalInfo2 = packedItem1.AdditionalInfos.AddNew();
			informationAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			informationAdditionalInfo2.CSI_Code = "INF2";
			informationAdditionalInfo2.CSI_Description = "INFDescription2";

			var referenceAdditionalInfo1 = packedItem1.AdditionalInfos.AddNew();
			referenceAdditionalInfo1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo1.CSI_Code = "CD1";
			referenceAdditionalInfo1.CSI_ReferenceNumber = "REF1";

			var referenceAdditionalInfo2 = packedItem1.AdditionalInfos.AddNew();
			referenceAdditionalInfo2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo2.CSI_Code = "CD1";
			referenceAdditionalInfo2.CSI_ReferenceNumber = "REF1";

			var referenceAdditionalInfo3 = packedItem1.AdditionalInfos.AddNew();
			referenceAdditionalInfo3.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo3.CSI_Code = "CD2";
			referenceAdditionalInfo3.CSI_ReferenceNumber = "REF1";

			var referenceAdditionalInfo4 = packedItem1.AdditionalInfos.AddNew();
			referenceAdditionalInfo4.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			referenceAdditionalInfo4.CSI_Code = "CD2";
			referenceAdditionalInfo4.CSI_ReferenceNumber = "REF2";

			var cusSupplyChainActorReferences1 = packedItem1.SupplyChainActors.AddNew();
			cusSupplyChainActorReferences1.CFR_Code = "ABC";
			cusSupplyChainActorReferences1.CFR_Reference = "SCREF1";
			var cusSupplyChainActorReferences2 = packedItem1.SupplyChainActors.AddNew();
			cusSupplyChainActorReferences2.CFR_Code = "ABC";
			cusSupplyChainActorReferences2.CFR_Reference = "SCREF1";
			var cusSupplyChainActorReferences3 = packedItem1.SupplyChainActors.AddNew();
			cusSupplyChainActorReferences3.CFR_Code = "ABC";
			cusSupplyChainActorReferences3.CFR_Reference = "SCREF2";
			var cusSupplyChainActorReferences4 = packedItem1.SupplyChainActors.AddNew();
			cusSupplyChainActorReferences4.CFR_Code = "DEF";
			cusSupplyChainActorReferences4.CFR_Reference = "SCREF2";

			var supportingDocument1 = packedItem1.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "N380";
			supportingDocument1.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument1.CSI_ReferenceNumber = "SUP1";

			var supportingDocument3 = packedItem1.SupportingDocuments.AddNew();
			supportingDocument3.CSI_Code = "N380";
			supportingDocument3.CSI_DateOfExpiry = new ZDateTime(2023, 01, 02);
			supportingDocument3.CSI_ReferenceNumber = "SUP1";

			var supportingDocument4 = packedItem1.SupportingDocuments.AddNew();
			supportingDocument4.CSI_Code = "N380";
			supportingDocument4.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument4.CSI_ReferenceNumber = "SUP2";

			var supportingDocument5 = packedItem1.SupportingDocuments.AddNew();
			supportingDocument5.CSI_Code = "N270";
			supportingDocument5.CSI_DateOfExpiry = new ZDateTime(2023, 01, 01);
			supportingDocument5.CSI_ReferenceNumber = "SUP1";

			return packedItem1;
		}

		TemporaryStoragePackedItem item;

		protected override ConsignmentItemWrapper GetProvider()
		{
			item = GetTestItem();
			return ConsignmentItemWrapper.New(item);
		}
	}
}
