using System.Linq;
using CargoWise.Customs.AR.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	public class ItemWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestItemWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Argentina;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			CreateAndPopulateHouseBill();

			IHouseWaybill wrapper = new HouseWaybillWrapper(header.Bills[0], MessageSubTypeCodes.Codes.Original);
			IItem item = wrapper.MasterConsignment.IncludedHouseConsignment.IncludedItems.ElementAt(0);

			CombineAssertions(() =>
			{
				AssertEquals((decimal)1, item.SequenceNumber);
				AssertEquals("427127829", item.TypeCode);
				AssertEquals((decimal)129.50, item.GrossWeight);
				AssertEquals("Kgm", item.GrossWeightUQ);
				AssertEquals((decimal)12.50, item.GrossVolume);
				AssertEquals("Cmq", item.GrossVolumeUQ);
				AssertEquals((decimal)12.30, item.TotalChargeAmount);
				AssertEquals("USD", item.TotalChargeAmountCurrencyID);
				AssertEquals(8, item.PieceQty);
				AssertEquals("TOOTH PASTE", item.Identification);
				AssertEquals("US", item.OriginID);
			});
		}

		void CreateAndPopulateHouseBill()
		{
			var bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "HOUSELIGADAMASTER0001";
			bill.ABL_GrossWeight = 129.5m;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_ManifestQty = 8;
			bill.ABL_ManifestUQ = "BBK";
			bill.ABL_Volume = 12.5m;
			bill.ABL_VolumeUQ = "CC";

			CreateAndPopulatePack(bill);
		}

		void CreateAndPopulatePack(AsycudaBill bill)
		{
			var pack = bill.Packs.AddNew();

			pack.APA_CommodityCode = "427127829";
			pack.APA_PackQty = 8;
			pack.APA_PackUQ = "BBK";
			pack.APA_Volume = 12.5m;
			pack.APA_VolumeUQ = "CC";
			pack.APA_Weight = 129.5m;
			pack.APA_WeightUQ = "KG";

			pack.CreatePackedItemForTesting();

			pack.PackedItem.API_GoodsValue = 12.3m;
			pack.PackedItem.API_RX_NKGoodsValueCurrency = "USD";
			pack.PackedItem.API_GoodsDescription = "TOOTH PASTE";
			pack.PackedItem.API_RN_NKGoodsOrigin = "US";
		}
	}
}
