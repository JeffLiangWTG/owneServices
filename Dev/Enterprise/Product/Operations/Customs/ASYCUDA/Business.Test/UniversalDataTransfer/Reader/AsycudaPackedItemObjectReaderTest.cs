using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	sealed class AsycudaPackedItemObjectReaderTest : DataObjectReaderTest
	{
		public void TestImportingAsycudaPackedItemData()
		{
			new ZZDataTestHelper(Factory.BOFactory);
			Factory.SaveForTesting();
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var readerHelper = header.ApplicationBusinessProvider.GetAsycudaManifestDataObjectReaderHelper(header.AMA_RN_NKCountry);
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItemData = help.SetupPackedItem("US", "9403.2000.30", "SG", 100.01m, 200.01m, 300.01m, "NT", "PT0001", "Goods Desc", 12345678901.1234m, "NMB");

			Factory.SaveForTesting();
			var packedItemBO = new AsycudaPackedItemObjectReader(packedItemData, packedItemData.AddInfoGroupCollection[0].AddInfoCollection, Logger, Factory, pack, readerHelper).ReadIntoBusinessObject();
			var sgPackedItemBO = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaPackedItem)packedItemBO;
			AssertNotNull(packedItemBO);
			AssertEquals("packedItemBO.API_RN_NKGoodsOrigin", "US", packedItemBO.API_RN_NKGoodsOrigin);
			AssertEquals("packedItemBO.API_Tariff", "9403.2000.30", packedItemBO.API_Tariff);
			AssertEquals("packedItemBO.API_CustomsValue", 100.01m, packedItemBO.API_CustomsValue);
			AssertEquals("packedItemBO.API_DutyAmount", 200.01m, packedItemBO.API_DutyAmount);
			AssertEquals("packedItemBO.API_TaxAmount", 300.01m, packedItemBO.API_TaxAmount);
			AssertEquals("packedItemBO.GoodsType", "NT", sgPackedItemBO.GoodsType);
			AssertEquals("packedItemBO.API_GoodsDescription", "Goods Desc", packedItemBO.API_GoodsDescription);
			AssertEquals("packedItemBO.API_CustomsQty", 12345678901.1234m, packedItemBO.API_CustomsQty);
			AssertEquals("packedItemBO.API_CustomsUQ", "NMB", packedItemBO.API_CustomsUQ);
			packedItemBO.CustomsEntryNumbers.Load();
			var customsEntryNumbers = packedItemBO.CustomsEntryNumbers;
			customsEntryNumbers.Load();
			AssertEquals("packedItemBO.CustomsEntryNumber", "PT0001", customsEntryNumbers[0].CE_EntryNum);
			AssertEquals("packedItemBO.CustomsEntryNumberType", Constants.CustomsEntryType.TradeNetPermit, customsEntryNumbers[0].CE_EntryType);
		}

		public void TestUpdateAsycudaPackedItemData()
		{
			var help = new AsycudaManifestDataObjectReaderTestHelper();
			var helper = new ZZDataTestHelper(Factory.BOFactory);
			Factory.SaveForTesting();
			var header = (AsycudaManifestHeader)Factory.BOFactory.New<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>();
			header.AMA_ManifestType = "MGI";
			var readerHelper = header.ApplicationBusinessProvider.GetAsycudaManifestDataObjectReaderHelper(header.AMA_RN_NKCountry);
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = (AsycudaPackedItem)((Integration.Customs.ASYCUDA.SGAccess.IAsycudaPack)pack).PackedItem;
			var packedItemPk = packedItem.PK;
			var entryNumber = packedItem.CustomsEntryNumbers.AddNew();
			entryNumber.CE_EntryNum = "5555";
			entryNumber.CE_EntryType = Constants.CustomsEntryType.TradeNetPermit;
			Factory.SaveForTesting();
			var packedItemData = help.SetupPackedItem("US", "9403.2000.30", "SG", 100.01m, 200.01m, 300.01m, "NT", "6666", "Goods Desc", 12345678901.1234m, "NMB");

			var packedItemBO = (Integration.Customs.ASYCUDA.SGAccess.IAsycudaPackedItem)new AsycudaPackedItemObjectReader(packedItemData, packedItemData.AddInfoGroupCollection[0].AddInfoCollection, Logger, Factory, pack, readerHelper).ReadIntoBusinessObject();
			AssertNotNull(packedItemBO);
			AssertEquals("country is updated", packedItemPk, packedItemBO.PK);
			AssertEquals("packedItemBO.API_RN_NKGoodsOrigin", "US", packedItemBO.API_RN_NKGoodsOrigin);
			AssertEquals("packedItemBO.API_Tariff", "9403.2000.30", packedItemBO.API_Tariff);
			AssertEquals("packedItemBO.API_CustomsValue", 100.01m, packedItemBO.API_CustomsValue);
			AssertEquals("packedItemBO.API_DutyAmount", 200.01m, packedItemBO.API_DutyAmount);
			AssertEquals("packedItemBO.API_TaxAmount", 300.01m, packedItemBO.API_TaxAmount);
			AssertEquals("packedItemBO.GoodsType", "NT", packedItemBO.GoodsType);
			AssertEquals("packedItemBO.API_GoodsDescription", "Goods Desc", packedItemBO.API_GoodsDescription);
			AssertEquals("packedItemBO.API_CustomsQty", 12345678901.1234m, packedItemBO.API_CustomsQty);
			AssertEquals("packedItemBO.API_CustomsUQ", "NMB", packedItemBO.API_CustomsUQ);

			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, packedItemPk);
			var entryNumbers = Factory.Load<CusEntryNumber>(query);
			AssertEquals("1 entry numbers", 1, entryNumbers.Length);
			var permitNumber = entryNumbers.First(x => x.CE_EntryType == Constants.CustomsEntryType.TradeNetPermit);
			AssertEquals("permit number is updated", "6666", permitNumber.CE_EntryNum);
		}
	}
}
