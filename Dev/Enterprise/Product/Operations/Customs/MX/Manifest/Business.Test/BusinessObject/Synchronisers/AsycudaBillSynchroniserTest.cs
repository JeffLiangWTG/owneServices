using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
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

		public void TestFreightAndGoodsValue()
		{
			var shipment = CreateAndPopulateShipment();
			var manifestHeader = CreateManifestHeader(shipment);
			var manifestBill = manifestHeader.Bills[0];

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_GoodsValue = 100;
			AssertEquals("Goods value is 100", 100m, manifestBill.ABL_TransportValue);
			AssertEquals("Freight value is 0", 0m, manifestBill.ABL_FreightValue);

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_GoodsValue = 0;
			AssertEquals("Goods value is 0", 0m, manifestBill.ABL_TransportValue);
			AssertEquals("Freight value is 0", 0m, manifestBill.ABL_FreightValue);
		}

		public void TestFreightAndGoodsCurrenciesValue()
		{
			var shipment = CreateAndPopulateShipment();
			var manifestHeader = CreateManifestHeader(shipment);
			var manifestBill = manifestHeader.Bills[0];

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_RX_NKGoodsValueCurr = "MXN";
			AssertEquals("Goods value currency is MXN", "MXN", manifestBill.ABL_RX_NKTransportValueCurrency);
			AssertEquals("Freight value currency is USD by default", "USD", manifestBill.ABL_RX_NKFreightValueCurrency);

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_RX_NKGoodsValueCurr = ZString.Empty;
			AssertEquals("Goods value currency is empty", "", manifestBill.ABL_RX_NKTransportValueCurrency);
			AssertEquals("Freight value currency is USD by default", "USD", manifestBill.ABL_RX_NKFreightValueCurrency);
		}

		protected override IManifestBillForSynchroniser GetManifestBill(ForwardingConsol consol)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ParentId = Consol.PK;
			header.AMA_ParentTableCode = "JK";
			var bill = header.Bills.AddNew();
			return bill;
		}

		protected override BusinessObjectSynchroniser GetManifestBillSynchroniser(IManifestBillForSynchroniser bill, ForwardingShipment shipment) => new AsycudaBillSynchroniser((AsycudaBill)bill, shipment);

		ForwardingShipment CreateAndPopulateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "UYMVD";
			shipment.JS_RL_NKDestination = "MXACA";
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
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

		protected override ZString GetPortOfLading(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;

		protected override ZString GetPlaceOfReceipt(ForwardingShipment shipment) => shipment.JS_RL_NKDestination;

		protected override ZString GetLastForeignPort(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;
	}
}
