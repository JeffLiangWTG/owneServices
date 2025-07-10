using System.Linq;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	public class ItemWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestItemWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			CreateAndPopulateHouseBill();

			IHouseWaybill wrapper = new HouseWaybillWrapper(header.Bills[0], "ORG");
			IItem item = wrapper.MasterConsignment.IncludedHouseConsignment.IncludedItems.ElementAt(0);

			CombineAssertions(() =>
			{
				AssertEquals((ZDecimal)1, item.SequenceNumber);
				AssertEquals("427127829", item.TypeCode);
				AssertEquals((ZDecimal)129.50, item.GrossWeight);
				AssertEquals("Kgm", item.GrossWeightUQ);
				AssertEquals((ZDecimal)12.50, item.GrossVolume);
				AssertEquals("Cmq", item.GrossVolumeUQ);
				AssertEquals((ZDecimal)12.30, item.TotalChargeAmount);
				AssertEquals("USD", item.TotalChargeAmountCurrencyID);
				AssertEquals((ZInt)8, item.PieceQty);
				AssertEquals("TOOTH PASTE", item.Identification);
				AssertEquals("US", item.OriginID);
			});
		}

		void CreateAndPopulateHouseBill()
		{
			AsycudaBill bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "HOUSELIGADAMASTER0001";
			bill.ABL_GrossWeight = 129.5m;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_ManifestQty = 8;
			bill.ABL_ManifestUQ = "BBK";
			bill.ABL_Volume = 12.5m;
			bill.ABL_VolumeUQ = "CC";
			bill.ABL_RL_NKOrigin = "USLAX";

			CreateAndPopulatePack(bill);
		}

		void CreateAndPopulatePack(AsycudaBill bill)
		{
			AsycudaPack pack = bill.Packs.AddNew();

			pack.LinePrice = 12.3m;
			pack.LinePriceCurrency = "USD";
			pack.APA_GoodsDescription = "TOOTH PASTE";
			pack.APA_CommodityCode = "427127829";
			pack.APA_PackQty = 8;
			pack.APA_PackUQ = "BBK";
			pack.APA_Volume = 12.5m;
			pack.APA_VolumeUQ = "CC";
			pack.APA_Weight = 129.5m;
			pack.APA_WeightUQ = "KG";
		}
	}
}
