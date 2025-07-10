using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.HK.Business.Testing
{
	class ForwardingShipmentExtensionMethodsTest : TestCaseWithFactory
	{
		public void TestLicenceLength()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00003134";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = "USCHI";
			shipment.JS_RL_NKDestination = "HKHKG";
			Assert(shipment.IsImport());

			var importLicense = shipment.CusEntryNumbers.AddNew();
			importLicense.CE_EntryNum = "1!, 2, AB#C, ed34289";
			importLicense.CE_EntryType = CusEntryNumberTypes.HongKong.ImportLicense;

			AssertContainsExactElementsInAnyOrder(new[] { "1", "2", "ABC", "ED34289" }, shipment.GetTraxonLicenseNumbersFromShipment());
		}

		public void TestSendOtherInfoForShipmentExport()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "USLAX";

			Assert("Should be false when HKDataRegistry.SendOtherCustomsInformation is empty, even if ACAS applies", !shipment.SendOtherInfoForShipment(consol));
			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.Australia }))
			{
				Assert("Should be true when consol discharge port in HKDataRegistry.SendOtherCustomsInformation", shipment.SendOtherInfoForShipment(consol));
			}
		}

		public void TestSendOtherInfoForShipmentImport()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "AUSYD";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "HKHKG";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.HongKong }))
			{
				Assert("Should be false unless HK & discharge port in HKDataRegistry.SendOtherCustomsInformation", !shipment.SendOtherInfoForShipment(consol));
			}

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.HongKong, Core.Constants.CountryGuids.Australia }))
			{
				Assert("Should be true when ACAS should apply", shipment.SendOtherInfoForShipment(consol));
			}
		}

		public void TestSendACASForShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			shipment.JS_RL_NKDestination = "USLAX";
			var transport = shipment.Transports.AddNew();
			transport.JW_RL_NKDiscPort = "USLAX";

			Assert("Should be false if US not in registry", !shipment.SendACASForShipment());
			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.UnitedStates }))
			{
				Assert("Should be true when ACAS should apply to export", shipment.SendACASForShipment());
			}

			shipment.JS_RL_NKDestination = "HKHKG";

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.UnitedStates }))
			{
				Assert("Should be false when HK not specified for import", !shipment.SendACASForShipment());
			}

			using (HKDataRegistry.Instance.SendOtherCustomsInformation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new[] { Core.Constants.CountryGuids.HongKong, Core.Constants.CountryGuids.UnitedStates }))
			{
				Assert("Should be true when ACAS should apply to import", shipment.SendACASForShipment());
			}
		}
	}
}
