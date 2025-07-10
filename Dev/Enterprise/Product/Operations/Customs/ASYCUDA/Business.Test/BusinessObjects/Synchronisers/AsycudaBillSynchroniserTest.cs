using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	[TestedType(typeof(AsycudaBillSynchroniser))]
	sealed class AsycudaBillSynchroniserTest : ManifestBillSynchroniserTest
	{
		public void TestSynchroniseManifestUQ()
		{
			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "NO";
			pack1.RP_Type = "GMB";
			pack1.RP_CustomsCountry = "AU";
			pack1.RP_ConversionFactor = 10;
			pack1.RP_CommercialPack = "DIZ";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = "UT";

			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Precondition", 1, manifestHeader.Bills.Count);
			var manifestBill = manifestHeader.Bills[0];
			manifestBill.Header.AMA_RN_NKCountry = "AU";

			AssertEquals("", "UT", manifestBill.ABL_ManifestUQ);
			AssertEquals("", 10, manifestBill.ABL_ManifestQty);

			shipment.JS_F3_NKPackType = "DIZ";

			AssertEquals("", "NO", manifestBill.ABL_ManifestUQ);
			AssertEquals("", 100, manifestBill.ABL_ManifestQty);
		}

		public void TestCrossFactoryAsycudaBillGoodsDescriptionSynchroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "GBSOU";
			shipment.JS_RL_NKDestination = "LKCMB";
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			shipment.Consols.Add(consol);
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			var manifestBill = manifestHeader.Bills.AddNew();
			AssertEquals("Prereq", "", manifestBill.ABL_GoodsDescription);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			shipment.JS_GoodsDescription = "Short description";
			AssertEquals("Short description", "Short description", manifestBill.ABL_GoodsDescription);
			shipment.DetailedGoodsDescriptionNoteText = "Long notes description";
			AssertEquals("Long notes description", "Long notes description", manifestBill.ABL_GoodsDescription);
		}

		public void TestNoSyncroPaymentTerms()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "GBSOU";
			shipment.JS_RL_NKDestination = "LKCMB";
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			shipment.Consols.Add(consol);
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			var manifestBill = manifestHeader.Bills.AddNew();
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			shipment.JS_PaymentTermAutoratingOverride = "CCX";
			AssertEquals("Payment Terms", "", manifestBill.ABL_PrepaidCollect);
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

		protected override ZString GetPortOfLading(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;

		protected override ZString GetPlaceOfReceipt(ForwardingShipment shipment) => shipment.JS_RL_NKDestination;

		protected override ZString GetLastForeignPort(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;

		protected override void AssertSpecficPropertiesSynchronisedResult(IManifestBillForSynchroniser bill, ForwardingShipment shipment)
		{
			base.AssertSpecficPropertiesSynchronisedResult(bill, shipment);

			var asycudaBill = bill as AsycudaBill;
			AssertEquals("ABL_MarksAndNumbers", shipment.JS_MarksAndNumbers, asycudaBill.ABL_MarksAndNumbers);
			AssertEquals("ABL_FreightValue", shipment.JS_GoodsValue, asycudaBill.ABL_FreightValue);
			AssertEquals("ABL_RX_NKFreightValueCurrency", shipment.JS_RX_NKGoodsValueCurr, asycudaBill.ABL_RX_NKFreightValueCurrency);
			AssertEquals("ABL_InsuranceValue", shipment.JS_InsuranceValue, asycudaBill.ABL_InsuranceValue);
			AssertEquals("ABL_RX_NKInsuranceValueCurrency", shipment.JS_RX_NKInsuranceCurrency, asycudaBill.ABL_RX_NKInsuranceValueCurrency);
			AssertEquals("ABL_GoodsDescription", shipment.JS_GoodsDescription, asycudaBill.ABL_GoodsDescription);
			AssertEquals("ABL_JS_Shipment", shipment.PK, asycudaBill.ABL_JS_Shipment);
			AssertEquals("ABL_BolType", shipment.JS_ShipmentType == Core.Constants.ShipmentTypes.CoLoadMaster ? Core.Constants.ShipmentTypes.CoLoadMaster : Core.Constants.ShipmentTypes.StandardHouse, asycudaBill.ABL_BolType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			Shipment.JS_RL_NKOrigin = "AUSYD";
			Shipment.JS_RL_NKDestination = "ZAJNB";
			Shipment.JS_MarksAndNumbers = "Mark1";
			Shipment.JS_GoodsValue = 10000.10;
			Shipment.JS_RX_NKGoodsValueCurr = "JN";
			Shipment.JS_InsuranceValue = 100.5;
			Shipment.JS_RX_NKInsuranceCurrency = "JN";
			Shipment.JS_GoodsDescription = "JN";
			Shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;
			Factory.Save();
		}
	}
}
