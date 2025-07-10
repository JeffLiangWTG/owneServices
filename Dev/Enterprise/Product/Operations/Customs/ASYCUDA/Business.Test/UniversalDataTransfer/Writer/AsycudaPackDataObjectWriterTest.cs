using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	sealed class AsycudaPackDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestExportPack()
		{
			PrepareCusCodeDataForTesting();
			AsycudaBillForRegularBillValidationTest.FindOrMakeRefPackAndCusCodeList("BG", "AAA", "SG", Factory);

			Factory.Save();

			var source = AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting("SGVLI", "MGI", new BusinessObjectFactory());
			source.Factory.Save();
			source = Factory.Load<AsycudaManifestHeader>(source.PK);
			var bill = source.Bills[0];
			bill.ABL_ManifestUQ = "BG";

			var pack = bill.Packs.AddNew();
			pack.ContainerPK = source.Containers[0].PK;
			pack.APA_PackQty = 5;
			pack.APA_PackUQ = "BG";
			pack.APA_GoodsDescription = "TestGoods";
			pack.ConsignmentReference = 10001;
			pack.MatchingReference = "HELLO1";
			pack.LinePrice = 110m;
			pack.LinePriceCurrency = Core.Constants.CurrencyCodes.Australia;

			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "KG";
			pack1.RP_Type = RPTypeList.Codes.GlobalManifestLine;
			pack1.RP_CustomsCountry = bill.CountryCode;
			pack1.RP_ConversionFactor = 1;
			pack1.RP_CommercialPack = "BG";

			Factory.Save();
			using (((IExternalFetchHintSupporter)Factory).SetupCreator())
			{
				var manager = (IShipmentDataContextManager)source.GetUniversalDataContextManager();
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, source)));
				var headerData = (UniversalShipment)writer.GetDataObject(source);

				var billData = headerData.SubShipmentCollection[0];
				var packData = billData.PackingLineCollection[0];
				AssertEquals("APA_PackQty", 5L, packData.PackQty);
				AssertEquals("APA_PackUQ", "BG", packData.PackType.Code);
				AssertEquals("APA_PackQty", 5, packData.CustomsOuterPacks);
				AssertEquals("Customs APA_PackUQ", "KG", packData.CustomsPackType.Code);
				AssertEquals("APA_GoodsDescription", "TestGoods", packData.GoodsDescription);
				AssertEquals("LinePrice", 110m, packData.LinePrice);
				AssertEquals("LinePriceCurrency.Code", Core.Constants.CurrencyCodes.Australia, packData.LinePriceCurrency.Code);
				AssertEquals("LinePriceCurrency.Description", "Australian Dollar", packData.LinePriceCurrency.Description);

				var addInforGroup = packData.AddInfoGroupCollection[0];
				AssertEquals("PAC", addInforGroup.Type.Code);
				AssertEquals("Pack Lines", addInforGroup.Type.Description);
				var consignmentReferenceAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AsycudaPack.Schema.ConsignmentReference);
				AssertEquals("10001", consignmentReferenceAddInfo.Value);
				var matchingReferenceAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AsycudaPack.Schema.MatchingReference);
				AssertEquals("HELLO1", matchingReferenceAddInfo.Value);
			}
		}

		public void TestExportMulitplePackedItems()
		{
			PrepareCusCodeDataForTesting();
			AsycudaBillForRegularBillValidationTest.FindOrMakeRefPackAndCusCodeList("BG", "AAA", "SG", Factory);

			Factory.Save();

			var source = AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting("VUVLI", "ASY", new BusinessObjectFactory());
			source.Factory.Save();
			source = Factory.Load<AsycudaManifestHeader>(source.PK);
			var bill = source.Bills[0];
			var pack = bill.Packs.AddNew();
			pack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;
			pack.ContainerPK = source.Containers[0].PK;
			pack.APA_PackQty = 5;
			pack.APA_PackUQ = "BG";
			pack.APA_GoodsDescription = "TestGoods";
			pack.ConsignmentReference = 10001;
			pack.MatchingReference = "HELLO1";
			pack.LinePrice = 110m;
			pack.LinePriceCurrency = Core.Constants.CurrencyCodes.Australia;

			var packedItem1 = pack.CreatePackedItemForTesting();
			packedItem1.API_Tariff = "2010304050";
			packedItem1.API_GoodsDescription = "DESC 1";
			packedItem1.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Bahamas;

			var packedItem2 = pack.CreatePackedItemForTesting();
			packedItem2.API_Tariff = "1010304050";
			packedItem2.API_GoodsDescription = "DESC 3";
			packedItem2.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.SouthAfrica;

			var packedItem3 = pack.CreatePackedItemForTesting();
			packedItem3.API_Tariff = "2010304050";
			packedItem3.API_GoodsDescription = "DESC 4";
			packedItem3.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;

			var packedItem4 = pack.CreatePackedItemForTesting();
			packedItem4.API_Tariff = "3010304050";
			packedItem4.API_GoodsDescription = "DESC 2";
			packedItem4.API_RN_NKGoodsOrigin = Core.Constants.CountryCodes.Australia;

			Factory.Save();
			using (((IExternalFetchHintSupporter)Factory).SetupCreator())
			{
				var manager = (IShipmentDataContextManager)source.GetUniversalDataContextManager();
				var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(null, source)));
				var headerData = (UniversalShipment)writer.GetDataObject(source);

				var billData = headerData.SubShipmentCollection[0];
				var packData = billData.PackingLineCollection[0];
				AssertEquals("packData.PackingLineCollection.Count", 4, packData.PackingLineCollection.Count);
				AssertPackedItemData(packData.PackingLineCollection[0], "1010304050", Core.Constants.CountryCodes.SouthAfrica, "DESC 3");
				AssertPackedItemData(packData.PackingLineCollection[1], "2010304050", Core.Constants.CountryCodes.Australia, "DESC 4");
				AssertPackedItemData(packData.PackingLineCollection[2], "2010304050", Core.Constants.CountryCodes.Bahamas, "DESC 1");
				AssertPackedItemData(packData.PackingLineCollection[3], "3010304050", Core.Constants.CountryCodes.Australia, "DESC 2");
			}
		}

		public void PrepareCusCodeDataForTesting()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SG", "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuVAIR = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "VAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVAIR.PK, "AIR", "VUVLI");
			var sbHIRA = helper.CreateNewOrGetExistingCusCodeList("SG", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "HIRA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRA.PK, "AIR", "SGVLI");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.PackageTypes, "BG", "BAG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.PackageTypes, "KG", "Keg", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();
		}

		void AssertPackedItemData(PackingLine packedItemData, ZString tariff, ZString goodsOrigin, ZString goodsDescription)
		{
			AssertEquals("packedItemData.HarmonisedCode", tariff, packedItemData.HarmonisedCode);
			AssertEquals("packedItemData.CountryOfOrigin.Code", goodsOrigin, packedItemData.CountryOfOrigin.Code);
			AssertEquals("packedItemData.GoodsDescription", goodsDescription, packedItemData.GoodsDescription);
		}
	}
}
