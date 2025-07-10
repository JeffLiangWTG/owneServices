using System.Collections.Generic;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	sealed class AsycudaPackDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportingAsycudaPacksData()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "CON00001";
			Factory.SaveForTesting();

			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var packLine = help.SetupPackingLine("CC", "Goods", "10", 10, 200m, "CON00001", 3);
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var packLineBO = new AsycudaPackDataObjectReader(packLine, Logger, Factory, bill, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(packLineBO);
			AssertEquals("packLineBO.APA_CommodityCode", "CC", packLineBO.APA_CommodityCode);
			AssertEquals("packLineBO.APA_GoodsDescription", "Goods", packLineBO.APA_GoodsDescription);
			AssertEquals("packLineBO.APA_MarksAndNumbers", "10", packLineBO.APA_MarksAndNumbers);
			AssertEquals("packLineBO.APA_PackQty", 10, packLineBO.APA_PackQty);
			AssertEquals("packLineBO.APA_PackUQ", "NO", packLineBO.APA_PackUQ);
			AssertEquals("packLineBO.APA_Weight", 200m, packLineBO.APA_Weight);
			AssertEquals("packLineBO.APA_WeightUQ", "KG", packLineBO.APA_WeightUQ);
			AssertNotNull(packLineBO.Pivot);
		}

		public void TestUpdateAsycudaPacksData()
		{
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var bill = header.Bills.AddNew();
			var oldPack = bill.Packs.AddNew();
			oldPack.APA_CommodityCode = "DD";
			oldPack.APA_GoodsDescription = "OldGoods";
			oldPack.APA_MarksAndNumbers = "OldMark";
			oldPack.ConsignmentReference = 1;
			var oldPackPK = oldPack.PK;
			var packLine = help.SetupPackingLine("CC", "Goods", "10", 10, 200m, "CON00001", 1);
			Factory.SaveForTesting();
			var packLineBO = new AsycudaPackDataObjectReader(packLine, Logger, Factory, bill, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(packLineBO);
			AssertEquals("pack is updated", oldPackPK, packLineBO.PK);
			AssertEquals("packLineBO.APA_CommodityCode", "CC", packLineBO.APA_CommodityCode);
			AssertEquals("packLineBO.APA_GoodsDescription", "Goods", packLineBO.APA_GoodsDescription);
			AssertEquals("packLineBO.APA_MarksAndNumbers", "10", packLineBO.APA_MarksAndNumbers);
			AssertEquals("packLineBO.APA_PackQty", 10, packLineBO.APA_PackQty);
			AssertEquals("packLineBO.APA_PackUQ", "NO", packLineBO.APA_PackUQ);
			AssertEquals("packLineBO.APA_Weight", 200m, packLineBO.APA_Weight);
			AssertEquals("packLineBO.APA_WeightUQ", "KG", packLineBO.APA_WeightUQ);
			AssertEquals("packLineBO.ConsignmentReference", 1, packLineBO.ConsignmentReference);
		}

		public void TestUpdateAsycudaPackByItemNo()
		{
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var bill = header.Bills.AddNew();
			var oldPack = bill.Packs.AddNew();
			oldPack.APA_CommodityCode = "DD";
			oldPack.APA_GoodsDescription = "OldGoods";
			oldPack.APA_MarksAndNumbers = "OldMark";
			oldPack.ConsignmentReference = 1;
			var oldPackPK = oldPack.PK;
			var packLine = help.SetupPackingLine("CC", "Goods", "10", 10, 200m, "CON00001", 0, null, 1);
			Factory.SaveForTesting();
			var packLineBO = new AsycudaPackDataObjectReader(packLine, Logger, Factory, bill, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(packLineBO);
			AssertEquals("pack is updated", oldPackPK, packLineBO.PK);
		}

		public void TestUpdateAsycudaPacksData_UsingMatchingReference()
		{
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			pack1.ConsignmentReference = 1;
			pack1.MatchingReference = "HELLO3";
			var pack2 = bill.Packs.AddNew();
			pack2.ConsignmentReference = 2;
			pack2.MatchingReference = "HELLO2";
			var pack3 = bill.Packs.AddNew();
			pack3.ConsignmentReference = 3;
			pack3.MatchingReference = "HELLO1";
			Factory.SaveForTesting();
			var packLine = help.SetupPackingLine("CC", "Goods", "10", 10, 200m, "CON00001", 0, "HELLO2");
			packLine.LinePrice = 110m;
			packLine.LinePriceCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			var packLineBO = new AsycudaPackDataObjectReader(packLine, Logger, Factory, bill, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(packLineBO);
			AssertEquals("pack is updated", pack2.PK, packLineBO.PK);
			AssertEquals("packLineBO.APA_CommodityCode", "CC", packLineBO.APA_CommodityCode);
			AssertEquals("packLineBO.APA_GoodsDescription", "Goods", packLineBO.APA_GoodsDescription);
			AssertEquals("packLineBO.APA_MarksAndNumbers", "10", packLineBO.APA_MarksAndNumbers);
			AssertEquals("packLineBO.APA_PackQty", 10, packLineBO.APA_PackQty);
			AssertEquals("packLineBO.APA_PackUQ", "NO", packLineBO.APA_PackUQ);
			AssertEquals("packLineBO.APA_Weight", 200m, packLineBO.APA_Weight);
			AssertEquals("packLineBO.APA_WeightUQ", "KG", packLineBO.APA_WeightUQ);
			AssertEquals("packLineBO.ConsignmentReference", 2, packLineBO.ConsignmentReference);
			AssertEquals("packLineBO.MatchingReference", "HELLO2", packLineBO.MatchingReference);
			AssertEquals("packLineBO.LinePrice", 110m, packLineBO.LinePrice);
			AssertEquals("packLineBO.LinePriceCurrency", Core.Constants.CurrencyCodes.Australia, packLineBO.LinePriceCurrency);

			packLine = help.SetupPackingLine("CC", "Goods", "10", 10, 200m, "CON00001", 0, "HELLO1");
			packLine.LinePrice = 110m;
			packLine.LinePriceCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			packLineBO = new AsycudaPackDataObjectReader(packLine, Logger, Factory, bill, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(packLineBO);
			AssertEquals("pack is updated", pack3.PK, packLineBO.PK);
			AssertEquals("packLineBO.APA_CommodityCode", "CC", packLineBO.APA_CommodityCode);
			AssertEquals("packLineBO.APA_GoodsDescription", "Goods", packLineBO.APA_GoodsDescription);
			AssertEquals("packLineBO.APA_MarksAndNumbers", "10", packLineBO.APA_MarksAndNumbers);
			AssertEquals("packLineBO.APA_PackQty", 10, packLineBO.APA_PackQty);
			AssertEquals("packLineBO.APA_PackUQ", "NO", packLineBO.APA_PackUQ);
			AssertEquals("packLineBO.APA_Weight", 200m, packLineBO.APA_Weight);
			AssertEquals("packLineBO.APA_WeightUQ", "KG", packLineBO.APA_WeightUQ);
			AssertEquals("packLineBO.ConsignmentReference", 3, packLineBO.ConsignmentReference);
			AssertEquals("packLineBO.MatchingReference", "HELLO1", packLineBO.MatchingReference);
			AssertEquals("packLineBO.LinePrice", 110m, packLineBO.LinePrice);
			AssertEquals("packLineBO.LinePriceCurrency", Core.Constants.CurrencyCodes.Australia, packLineBO.LinePriceCurrency);

			packLine = help.SetupPackingLine("CC", "Goods", "10", 10, 200m, "CON00001", 0, "HELLO4");
			packLine.LinePrice = 110m;
			packLine.LinePriceCurrency = new Currency() { Code = Core.Constants.CurrencyCodes.Australia };
			packLineBO = new AsycudaPackDataObjectReader(packLine, Logger, Factory, bill, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(packLineBO);
			AssertNotEquals("pack is not matched", pack1.PK, packLineBO.PK);
			AssertNotEquals("pack is not matched", pack2.PK, packLineBO.PK);
			AssertNotEquals("pack is not matched", pack3.PK, packLineBO.PK);
			AssertEquals("packLineBO.APA_CommodityCode", "CC", packLineBO.APA_CommodityCode);
			AssertEquals("packLineBO.APA_GoodsDescription", "Goods", packLineBO.APA_GoodsDescription);
			AssertEquals("packLineBO.APA_MarksAndNumbers", "10", packLineBO.APA_MarksAndNumbers);
			AssertEquals("packLineBO.APA_PackQty", 10, packLineBO.APA_PackQty);
			AssertEquals("packLineBO.APA_PackUQ", "NO", packLineBO.APA_PackUQ);
			AssertEquals("packLineBO.APA_Weight", 200m, packLineBO.APA_Weight);
			AssertEquals("packLineBO.APA_WeightUQ", "KG", packLineBO.APA_WeightUQ);
			AssertEquals("packLineBO.ConsignmentReference", 0, packLineBO.ConsignmentReference);
			AssertEquals("packLineBO.MatchingReference", "HELLO4", packLineBO.MatchingReference);
			AssertEquals("packLineBO.LinePrice", 110m, packLineBO.LinePrice);
			AssertEquals("packLineBO.LinePriceCurrency", Core.Constants.CurrencyCodes.Australia, packLineBO.LinePriceCurrency);
		}

		public void TestCanNotImport()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var readerHelper = new AsycudaManifestDataObjectReaderHelper(Core.Constants.CountryCodes.Eritrea, Factory.BOFactory);
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			var bill = header.Bills.AddNew();
			var packData = help.SetupPackingLine("CC", "Goods", "10", 10, 200m, "CON00001", 1);
			var addInfoGroup1 = new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = AddInfoConstants.Pack.PackAddInfoType, Description = AddInfoConstants.Pack.PackAddInfoTypeDescription },
				AddInfoCollection = new List<AddInfo>(),
			};
			packData.AddInfoGroupCollection.Add(addInfoGroup1);
			var packBO = new AsycudaPackDataObjectReader(packData, Logger, Factory, bill, readerHelper, false).ReadIntoBusinessObject();
			AssertNull(packBO);
			AssertContains("logger.Logs", @"AddInfoGroupCollection cannot have more than one AddInfoGroup which Type is PAC.", Logger.Logs);
		}

		public void TestCanNotImportSubPackingLine()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var helper = new ZZDataTestHelper(Factory.BOFactory);
			Factory.SaveForTesting();
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var readerHelper = header.ApplicationBusinessProvider.GetAsycudaManifestDataObjectReaderHelper(header.AMA_RN_NKCountry);
			var bill = header.Bills.AddNew();
			var packData = help.SetupPackingLine("CC", "Goods", "10", 10, 200m, "CON00001", 1);
			packData.Link = 1;
			var packedItemData = help.SetupPackedItem("US", "9403.2000.30", "SB", 100.01m, 200.01m, 300.01m, "NT", "PT0001", "Goods Desc", 12345678901.1234m, "NMB");
			packData.SetPackingLineCollection(() => new List<PackingLine>(new[] { packedItemData }));
			var packBO = new AsycudaPackDataObjectReader(packData, Logger, Factory, bill, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(packBO);
			AssertContains("logger.Logs", @"Packing Line '1' Sub Line '0' AddInfoGroupCollection.AddInfoGroup.AddInfoCollection.AddInfo.Key.Country is not 'SG'; this data will be ignored.", Logger.Logs);

			packBO = new AsycudaPackDataObjectReader(packData, Logger, Factory, bill, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(packBO);
			packedItemData.SetAddInfoGroupCollection(() => null);
			AssertContains("logger.Logs", @"Packing Line '1' Sub Line '0' AddInfoGroupCollection.AddInfoGroup.AddInfoCollection.AddInfo.Key.Country is not 'SG'; this data will be ignored.", Logger.Logs);

			packedItemData.SetAddInfoGroupCollection(() => new List<AddInfoGroup>());
			var addInfoGroup1 = new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = AddInfoConstants.PackedItem.PackingItemAddInfoType, Description = AddInfoConstants.PackedItem.PackingItemAddInfoTypeDescription },
				AddInfoCollection = new List<AddInfo>(),
			};
			packedItemData.AddInfoGroupCollection?.Add(addInfoGroup1);

			var addInfoGroup2 = new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = AddInfoConstants.PackedItem.PackingItemAddInfoType, Description = AddInfoConstants.PackedItem.PackingItemAddInfoTypeDescription },
				AddInfoCollection = new List<AddInfo>(),
			};
			packedItemData.AddInfoGroupCollection.Add(addInfoGroup2);
			packBO = new AsycudaPackDataObjectReader(packData, Logger, Factory, bill, readerHelper, false).ReadIntoBusinessObject();
			AssertNotNull(packBO);
			AssertContains("logger.Logs", @"Packing Line '1' Sub Line '0' AddInfoGroupCollection cannot have more than one AddInfoGroup which Type is PAC.", Logger.Logs);
		}

		public void TestMultipleAsycudaPacksWithImportingPacklines()
		{
			new ZZDataTestHelper(Factory.BOFactory);
			Factory.SaveForTesting();
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var readerHelper = header.ApplicationBusinessProvider.GetAsycudaManifestDataObjectReaderHelper(header.AMA_RN_NKCountry);
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var bill = header.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			pack1.ConsignmentReference = 1;
			pack1.PackedItem.API_Tariff = "7403.2000.30";
			pack1.PackedItem.API_CustomsQty = 30;
			pack1.PackedItem.API_CustomsUQ = "BOX";

			var pack2 = bill.Packs.AddNew();
			pack2.ConsignmentReference = 2;
			pack2.PackedItem.API_Tariff = "7403.2000.31";
			pack2.PackedItem.API_CustomsQty = 15;
			pack2.PackedItem.API_CustomsUQ = "CAS";

			var packLine1 = help.SetupPackingLine("CC", "Goods", "10", 10, 200m, "CON00001", 0, null, 1);
			var packeLineItem1 = help.SetupPackedItem("AU", "8403.2000.30", "SG", 100.01m, 200.01m, 300.01m, "NT", "PT0001", "Goods Desc 1", 40m, "BAG");
			packLine1.SetPackingLineCollection(() => new List<PackingLine>(new[] { packeLineItem1 }));

			var packLine2 = help.SetupPackingLine("CC", "Goods", "10", 10, 200m, "CON00001", 0, null, 2);
			var packeLineItem2 = help.SetupPackedItem("AU", "8403.2000.31", "SG", 100.01m, 200.01m, 300.01m, "NT", "PT0002", "Goods Desc 2", 20m, "CAN");
			packLine2.SetPackingLineCollection(() => new List<PackingLine>(new[] { packeLineItem2 }));

			var packLine3 = help.SetupPackingLine("CC", "Goods", "10", 10, 200m, "CON00001", 0, null, 3);
			var packeLineItem3 = help.SetupPackedItem("AU", "8403.2000.32", "SG", 100.01m, 200.01m, 300.01m, "NT", "PT0003", "Goods Desc 3", 80m, "BOT");
			packLine3.SetPackingLineCollection(() => new List<PackingLine>(new[] { packeLineItem3 }));

			Factory.SaveForTesting();

			var packLineBO = new AsycudaPackDataObjectReader(packLine1, Logger, Factory, bill, readerHelper, false).ReadIntoBusinessObject();
			AssertEquals("Pack Tariff", "8403.2000.30", packLineBO.PackedItem.API_Tariff);
			AssertEquals("Pack Customs Qty", 40m, packLineBO.PackedItem.API_CustomsQty);
			AssertEquals("Pack Customs Qty Unit", "BAG", packLineBO.PackedItem.API_CustomsUQ);

			var packLineBO2 = new AsycudaPackDataObjectReader(packLine2, Logger, Factory, bill, readerHelper, false).ReadIntoBusinessObject();
			AssertEquals("Pack Tariff", "8403.2000.31", packLineBO2.PackedItem.API_Tariff);
			AssertEquals("Pack Customs Qty", 20m, packLineBO2.PackedItem.API_CustomsQty);
			AssertEquals("Pack Customs Qty Unit", "CAN", packLineBO2.PackedItem.API_CustomsUQ);

			var packLineBO3 = new AsycudaPackDataObjectReader(packLine3, Logger, Factory, bill, readerHelper, false).ReadIntoBusinessObject();
			AssertEquals("Pack Tariff", "8403.2000.32", packLineBO3.PackedItem.API_Tariff);
			AssertEquals("Pack Customs Qty", 80m, packLineBO3.PackedItem.API_CustomsQty);
			AssertEquals("Pack Customs Qty Unit", "BOT", packLineBO3.PackedItem.API_CustomsUQ);
		}
	}
}
