using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	sealed class AsycudaPackedItemDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestExportPackedItem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee(Core.Constants.Customs.CusEntryFeeTypes.GSTVATAmount, 0.07m, Core.Constants.CountryCodes.Singapore, ZDateTime.Today.AddDays(-5), ZDateTime.Today.AddDays(5), "Goods and Services Tax");

			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "recipient.user@forwarder.com";
			Factory.Save();

			PrepareCusCodeDataForTesting();
			AsycudaBillForRegularBillValidationTest.FindOrMakeRefPackAndCusCodeList("DJC", "AAA", Core.Constants.CountryCodes.Singapore, Factory);
			Factory.Save();

			var source = AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting("SGVLI", "MGI", new BusinessObjectFactory());
			source.Factory.Save();
			source = Factory.Load<AsycudaManifestHeader>(source.PK);
			var bill = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaBill)source.Bills[0];
			bill.ABL_ManifestUQ = "DJC";
			bill.CycleDate = ZDateTime.Today;

			var pack = source.Bills[0].Packs.AddNew();
			pack.APA_PackQty = 10;
			pack.APA_PackUQ = "DJC";
			pack.ContainerPK = source.Containers[0].PK;

			var packedItem = (AsycudaPackedItem)((Integration.Customs.ASYCUDA.SGAccess.IAsycudaPack)pack).PackedItem;
			packedItem.API_Tariff = "9403.2000.30";
			packedItem.API_CustomsQty = 12345.12m;
			packedItem.API_CustomsUQ = "BG";
			packedItem.API_RN_NKGoodsOrigin = "US";
			packedItem.API_GoodsDescription = "Books";
			pack.LinePrice = 1000.2m;
			packedItem.API_DutyAmount = 201.3m;
			packedItem.API_TaxAmount = 100.4m;

			var entryNumber1 = packedItem.CustomsEntryNumbers.AddNew();
			entryNumber1.CE_EntryType = Constants.CustomsEntryType.TradeNetPermit;
			entryNumber1.CE_EntryNum = "ENT0001";

			var entryNumber2 = packedItem.CustomsEntryNumbers.AddNew();
			entryNumber2.CE_EntryType = Constants.CustomsEntryType.ACCESSPermit;
			entryNumber2.CE_EntryNum = "ENT0002";

			var entryNumber3 = packedItem.CustomsEntryNumbers.AddNew();
			entryNumber3.CE_EntryType = "XXX";
			entryNumber3.CE_EntryNum = "ENT0003";

			Factory.Save();

			var headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, source);
			var billData = headerData.SubShipmentCollection[0];
			var packData = billData.PackingLineCollection[0];
			var packedItemData = packData.PackingLineCollection[0];
			AssertEquals("API_RN_NKGoodsOrigin", "US", packedItemData.CountryOfOrigin.Code);
			AssertEquals("API_GoodsDescription", "Books", packedItemData.GoodsDescription);
			AssertEquals("API_Tariff", "9403200030", packedItemData.HarmonisedCode);
			AssertEquals("API_CustomsQty", 12345L, packedItemData.PackQty);
			AssertEquals("API_CustomsUQ", "BG", packedItemData.PackType.Code);
			AssertEquals("API_CustomsUQ Description", "BAG", packedItemData.PackType.Description);

			var addInforGroup = packedItemData.AddInfoGroupCollection[0];
			AssertEquals("PAC", addInforGroup.Type.Code);
			AssertEquals("Asycuda Country Specific Packing Item Details", addInforGroup.Type.Description);
			var countryAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AddInfoConstants.PackedItem.Country);
			var goodsTypeAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == "GoodsType");
			var permitNumberAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == "PermitNumber");
			var entryNumberAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == "EntryNumber");
			var customsValueAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AddInfoConstants.PackedItem.CustomsValue);
			var dutyValueAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AddInfoConstants.PackedItem.DutyValue);
			var taxValueAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AddInfoConstants.PackedItem.TaxValue);
			var countryOfDestinationAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AddInfoConstants.PackedItem.CountryOfDestination);

			var customsQtyAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AddInfoConstants.PackedItem.CustomsQty);
			var customsUQAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AddInfoConstants.PackedItem.CustomsUQ);

			AssertEquals("API_RN_NKCountry", "SG", countryAddInfo.Value);
			AssertEquals("GoodsType", "NT", goodsTypeAddInfo.Value);
			AssertEquals("PermitNumber", "ENT0001", permitNumberAddInfo.Value);
			AssertEquals("EntryNumber", "ENT0002", entryNumberAddInfo.Value);
			AssertEquals("API_CustomsValue", "410.16", customsValueAddInfo.Value);
			AssertEquals("API_DutyAmount", "201.3", dutyValueAddInfo.Value);
			AssertEquals("API_TaxAmount", "42.80", taxValueAddInfo.Value);
			AssertEquals("AsycudaBill.ABL_RL_NKFinalDestination", "SG", countryOfDestinationAddInfo.Value);

			AssertEquals("API_CustomsQty", "12345.12", customsQtyAddInfo.Value);
			AssertEquals("API_CustomsUQ", "BG", customsUQAddInfo.Value);
		}

		public void TestParseSafelyPackQty()
		{
			var staff = Factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "recipient.user@forwarder.com";
			Factory.Save();

			PrepareCusCodeDataForTesting();
			AsycudaBillForRegularBillValidationTest.FindOrMakeRefPackAndCusCodeList("DJC", "AAA", "SG", Factory);
			Factory.Save();

			var source = AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting("SGVLI", "MGI", new BusinessObjectFactory());
			source.Factory.Save();
			source = Factory.Load<AsycudaManifestHeader>(source.PK);
			var bill = source.Bills[0];
			bill.ABL_ManifestUQ = "DJC";

			var pack = source.Bills[0].Packs.AddNew();
			pack.APA_PackUQ = "DJC";
			pack.ContainerPK = source.Containers[0].PK;

			var packedItem = pack.GetSGPackedItemForTesting();
			packedItem.API_Tariff = "9403.2000.30";
			packedItem.API_CustomsQty = 10m;
			packedItem.API_CustomsUQ = "BG";
			packedItem.API_RN_NKGoodsOrigin = "US";
			packedItem.API_GoodsDescription = "Books";

			packedItem.GoodsType = "NT";
			packedItem.API_CustomsValue = 1000.2m;
			packedItem.API_CustomsQty = long.MaxValue + 1.001m;
			packedItem.API_CustomsUQ = "BG";
			packedItem.API_DutyAmount = 201.3m;
			packedItem.API_TaxAmount = 100.4m;

			var headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, source);
			var billData = headerData.SubShipmentCollection[0];
			var packData = billData.PackingLineCollection[0];
			var packedItemData = packData.PackingLineCollection[0];
			AssertEquals("API_RN_NKGoodsOrigin", "US", packedItemData.CountryOfOrigin.Code);
			AssertEquals("API_GoodsDescription", "Books", packedItemData.GoodsDescription);
			AssertEquals("API_Tariff", "9403200030", packedItemData.HarmonisedCode);
			AssertEquals("API_CustomsQty", long.MaxValue, packedItemData.PackQty);
			AssertEquals("API_CustomsUQ", "BG", packedItemData.PackType.Code);
			AssertEquals("API_CustomsUQ Description", "BAG", packedItemData.PackType.Description);

			var addInforGroup = packedItemData.AddInfoGroupCollection[0];
			var customsQtyAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AddInfoConstants.PackedItem.CustomsQty);
			var customsUQAddInfo = addInforGroup.AddInfoCollection.FirstOrDefault(x => x.Key.Value == AddInfoConstants.PackedItem.CustomsUQ);
			AssertEquals("API_CustomsQty", "9223372036854775808.001", customsQtyAddInfo.Value);
			AssertEquals("API_CustomsUQ", "BG", customsUQAddInfo.Value);

			packedItem.API_CustomsQty = long.MinValue - 1.001m;
			headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, source);
			billData = headerData.SubShipmentCollection[0];
			packData = billData.PackingLineCollection[0];
			packedItemData = packData.PackingLineCollection[0];
			AssertEquals("API_CustomsQty", long.MinValue, packedItemData.PackQty);
			AssertEquals("API_CustomsUQ", "BG", packedItemData.PackType.Code);
			AssertEquals("API_CustomsUQ Description", "BAG", packedItemData.PackType.Description);

			packedItem.API_CustomsQty = 12345.12m;
			headerData = (UniversalShipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, source);
			billData = headerData.SubShipmentCollection[0];
			packData = billData.PackingLineCollection[0];
			packedItemData = packData.PackingLineCollection[0];
			AssertEquals("API_CustomsQty", 12345L, packedItemData.PackQty);
			AssertEquals("API_CustomsUQ", "BG", packedItemData.PackType.Code);
			AssertEquals("API_CustomsUQ Description", "BAG", packedItemData.PackType.Description);
		}

		public void PrepareCusCodeDataForTesting()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SG", "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuVAIR = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "VAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVAIR.PK, "AIR", "VUVLI");
			var sbHIRA = helper.CreateNewOrGetExistingCusCodeList("SG", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "HIRA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRA.PK, "AIR", "SGVLI");
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.GoodsType, "GoodsType");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, "DT", "Dutiable Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.GoodsType, "CT", "Controlled Goods", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Eritrea, RefCusCodeListTypes.Codes.GoodsType, "KD", "KD DESC", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.PackageTypes, "BG", "BAG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Singapore, RefCusCodeListTypes.Codes.PackageTypes, "KG", "Keg", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var sgRegistry = ObjectFactory.Get<Integration.Customs.SG.ISGCustomsRegistry>();
			sgRegistry.ACCESSEnable.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			Factory.Save();
		}
	}
}
