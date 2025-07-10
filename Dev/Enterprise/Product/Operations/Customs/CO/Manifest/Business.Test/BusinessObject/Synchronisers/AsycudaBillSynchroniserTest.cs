using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillSynchroniser))]
	sealed class AsycudaBillSynchroniserTest : ManifestBillSynchroniserTest
	{
		public void TestABL_PrepaidCollect()
		{
			var shipment = CreateAndPopulateShipment();
			var manifestHeader = CreateManifestHeader(shipment);
			var manifestBill = manifestHeader.Bills[0];

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_PaymentTermAutoratingOverride = "CCX";
			AssertEquals("Collect", "CLT", manifestBill.ABL_PrepaidCollect);

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_PaymentTermAutoratingOverride = "PPD";
			AssertEquals("Prepaid", "PPD", manifestBill.ABL_PrepaidCollect);
		}

		public void TestManifestUQIsNotConverted()
		{
			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "BT";
			pack1.RP_Type = "GMB";
			pack1.RP_CustomsCountry = "CO";
			pack1.RP_ConversionFactor = 2;
			pack1.RP_CommercialPack = "PCS";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = "PCS";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			var bill = manifestHeader.Bills[0];

			CombineAssertions(() =>
			{
				AssertEquals("ABL_ManifestQty is not converted", 10, bill.ABL_ManifestQty);
				AssertEquals("ABL_ManifestUQ is not converted", "PCS", bill.ABL_ManifestUQ);
			});
		}

		public void TestFreightAndGoodsValue()
		{
			var shipment = CreateAndPopulateShipment();
			var manifestHeader = CreateManifestHeader(shipment);
			var manifestBill = manifestHeader.Bills[0];

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_GoodsValue = 100;
			AssertEquals("Goods value is 100", 100m, manifestBill.ABL_GoodsValue);
			AssertEquals("Freight value is 0", 0m, manifestBill.ABL_FreightValue);

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_GoodsValue = 0;
			AssertEquals("Goods value is 0", 0m, manifestBill.ABL_GoodsValue);
			AssertEquals("Freight value is 0", 0m, manifestBill.ABL_FreightValue);
		}

		public void TestFreightAndGoodsCurrenciesValue()
		{
			var shipment = CreateAndPopulateShipment();
			var manifestHeader = CreateManifestHeader(shipment);
			var manifestBill = manifestHeader.Bills[0];

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_RX_NKGoodsValueCurr = "COP";
			AssertEquals("Goods value currency is COP", "COP", manifestBill.ABL_RX_NKGoodsValueCurrency);
			AssertEquals("Freight value currency is USD", "USD", manifestBill.ABL_RX_NKFreightValueCurrency);

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_RX_NKGoodsValueCurr = ZString.Empty;
			AssertEquals("Goods value currency is USD", "USD", manifestBill.ABL_RX_NKGoodsValueCurrency);
			AssertEquals("Freight value currency is USD", "USD", manifestBill.ABL_RX_NKFreightValueCurrency);
		}

		public void TestGoodsLocation()
		{
			var shipment = CreateAndPopulateShipment();
			var manifestHeader = CreateManifestHeader(shipment);
			var manifestBill = manifestHeader.Bills[0];

			SynchroniserManifestHeader(manifestHeader);

			var org = Factory.New<OrgHeader>();
			var orgPK = org.MainAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = orgPK;
			AssertEquals("Location of goods is ", orgPK, manifestBill.ABL_OA_GoodsLocation);
		}

		protected override IManifestBillForSynchroniser GetManifestBill(ForwardingConsol consol)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ParentId = Consol.PK;
			header.AMA_ParentTableCode = "JK";
			var bill = header.Bills.AddNew();
			return bill;
		}

		ForwardingShipment CreateAndPopulateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "UYMVD";
			shipment.JS_RL_NKDestination = "COBOG";
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			shipment.Consols.Add(consol);
			return shipment;
		}

		AsycudaManifestHeader CreateManifestHeader(ForwardingShipment shipment)
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(shipment.Consols[0]);
			_ = manifestHeader.Bills.AddNew();
			return manifestHeader;
		}

		void SynchroniserManifestHeader(AsycudaManifestHeader manifestHeader)
		{
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
		}

		protected override BusinessObjectSynchroniser GetManifestBillSynchroniser(IManifestBillForSynchroniser bill, ForwardingShipment shipment) => new AsycudaBillSynchroniser((AsycudaBill)bill, shipment);

		protected override ZString GetPortOfLading(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;

		protected override ZString GetPlaceOfReceipt(ForwardingShipment shipment) => shipment.JS_RL_NKDestination;

		protected override ZString GetLastForeignPort(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;
	}
}
