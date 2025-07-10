using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeaderSynchroniser))]
	sealed class AsycudaManifestHeaderSynchroniserTest : TestCaseWithFactory
	{
		public void TestMasterBill()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "999";
			consol.JK_CoLoadMasterBill = "123";

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals("999", manifestHeader.AMA_MasterBill);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			consol.JK_CoLoadMasterBill = "123";

			manifestHeader = Factory.New<AsycudaManifestHeader>();
			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals("123", manifestHeader.AMA_MasterBill);

			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_MasterBillNum = "999";
			consol.JK_CoLoadMasterBill = "123";

			manifestHeader = Factory.New<AsycudaManifestHeader>();
			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals("999", manifestHeader.AMA_MasterBill);
		}

		public void TestCarrier()
		{
			var foreignCountry = Factory.NewWithValidTestData<RefCountry>();
			foreignCountry.Code = Constants.CountryCodes.Uruguay;
			var localCountry = Factory.NewWithValidTestData<RefCountry>();
			localCountry.Code = Constants.CountryCodes.Colombia;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierAddress = Factory.NewWithValidTestData<OrgAddress>();
			carrierAddress.OA_RN_NKCountryCode = foreignCountry.Code;
			carrierAddress.OA_OH = carrier.PK;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var creditorAddress = Factory.NewWithValidTestData<OrgAddress>();
			creditorAddress.OA_RN_NKCountryCode = foreignCountry.Code;
			creditorAddress.OA_OH = creditor.PK;

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;

			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(ZGuid.Empty, manifestHeader.AMA_OA_Carrier);

			consol.JK_OA_CreditorAddress = creditorAddress.PK;
			var transport = consol.Transports.AddNew("UYMVD", "COBOG");
			transport.JW_OA_CarrierAddress = carrierAddress.PK;

			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(carrierAddress.PK, manifestHeader.AMA_OA_Carrier);

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(ZGuid.Empty, manifestHeader.AMA_OA_Carrier);

			consol.JK_OA_CreditorAddress = creditorAddress.PK;

			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(ZGuid.Empty, manifestHeader.AMA_OA_Carrier);

			var appointedAgentPorted = creditor.CarrierAppointedAgentPorts_Agency.AddNew();
			appointedAgentPorted.O5_OA_AgentOfficeAddress = carrierAddress.PK;

			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(ZGuid.Empty, manifestHeader.AMA_OA_Carrier);

			appointedAgentPorted.O5_PortOrCountry = localCountry.Code;

			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(carrierAddress.PK, manifestHeader.AMA_OA_Carrier);

			creditorAddress.OA_RN_NKCountryCode = localCountry.Code;

			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(creditorAddress.PK, manifestHeader.AMA_OA_Carrier);

			consol.JK_AgentType = Constants.AgentType.Direct;

			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(ZGuid.Empty, manifestHeader.AMA_OA_Carrier);

			carrierAddress.OA_RN_NKCountryCode = foreignCountry.Code;
			creditorAddress.OA_RN_NKCountryCode = foreignCountry.Code;
			consol.JK_OA_CreditorAddress = creditorAddress.PK;
			transport = consol.Transports.AddNew("UYMVD", "COBOG");
			transport.JW_OA_CarrierAddress = carrierAddress.PK;

			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(ZGuid.Empty, manifestHeader.AMA_OA_Carrier);

			appointedAgentPorted = carrier.CarrierAppointedAgentPorts_Agency.AddNew();
			appointedAgentPorted.O5_OA_AgentOfficeAddress = creditorAddress.PK;

			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(ZGuid.Empty, manifestHeader.AMA_OA_Carrier);

			appointedAgentPorted.O5_PortOrCountry = localCountry.Code;

			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(creditorAddress.PK, manifestHeader.AMA_OA_Carrier);

			carrierAddress.OA_RN_NKCountryCode = localCountry.Code;

			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(carrierAddress.PK, manifestHeader.AMA_OA_Carrier);
		}

		public void TestDeliveryMode()
		{
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = "PCS";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			SynchroniseManifestHeader(manifestHeader, consol);
			Assert(manifestHeader.DeliveryMode.IsEmpty);

			var consolContainer = consol.Containers.AddNew();
			consolContainer.DeliveryModeForBinding = Constants.DeliveryModes.Codes.CY_CY;
			consolContainer.JC_IsNonOperativeReefer = true;
			SynchroniseManifestHeader(manifestHeader, consol);
			Assert(manifestHeader.DeliveryMode.IsEmpty);

			consolContainer.JC_IsNonOperativeReefer = false;
			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(CODeliveryModeList.Codes._1, manifestHeader.DeliveryMode);

			consolContainer.DeliveryModeForBinding = Constants.DeliveryModes.Codes.CY_CFS;
			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(CODeliveryModeList.Codes._2, manifestHeader.DeliveryMode);

			consolContainer.DeliveryModeForBinding = Constants.DeliveryModes.Codes.CFS_CY;
			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(CODeliveryModeList.Codes._3, manifestHeader.DeliveryMode);

			consolContainer.DeliveryModeForBinding = Constants.DeliveryModes.Codes.CFS_CFS;
			SynchroniseManifestHeader(manifestHeader, consol);
			AssertEquals(CODeliveryModeList.Codes._4, manifestHeader.DeliveryMode);
		}

		void SynchroniseManifestHeader(AsycudaManifestHeader manifestHeader, ForwardingConsol consol)
		{
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
		}
	}
}
