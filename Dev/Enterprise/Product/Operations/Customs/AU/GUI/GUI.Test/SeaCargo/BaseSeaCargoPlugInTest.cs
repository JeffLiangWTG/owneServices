using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.SeaCargo.GUI.Testing
{
	abstract class BaseSeaCargoPlugInTest : SeaCargoTestCase
	{
		protected const string CoLoadMasterHouseBillNumber1 = "CLM93938020";
		protected const string SubHouseBillNumber1 = "SHB7838392";
		protected const string CoLoaderClientID = "C002293920";

		protected OrgHeader GetOrganisation(ZString name, ZString uNLOCO)
		{
			var result = Factory.New<OrgHeader>();
			result.OH_FullName = name;
			result.MainAddress.OA_Address1 = "Address " + name;
			result.OH_RL_NKClosestPort = uNLOCO;
			return result;
		}

		protected OrgHeader GetConsignee(ZString name)
		{
			var result = GetOrganisation(name, "AUSYD");
			result.OH_IsConsignee = true;
			return result;
		}

		protected OrgHeader GetConsignor(ZString name)
		{
			var result = GetOrganisation(name, "SGSIN");
			result.OH_IsConsignor = true;
			return result;
		}

		protected OrgHeader GetReceivingForwarder(ZString name)
		{
			var result = GetOrganisation(name, "AUSYD");
			result.OH_IsForwarder = true;
			result.LocalManifestID = "C123456789";
			return result;
		}

		protected ForwardingShipment CreateShipmentToSynchronise()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "USLAX";
			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "2391";
			transport.JW_Vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, SQLComparisonOperator.NotEqual, ZString.Empty)).RV_Code;
			transport.JW_ETA = ZDateTime.Now.AddDays(1);
			AssertNotNull("Failed to create Sailing", consol.Schedule);
			CommonContainer container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = ContainerNumber1;
			container1.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			consol.SetDefaultReceivingForwarderAddress(GetReceivingForwarder("Forwarder"));
			var result = consol.Shipments.AddNew();
			result.JS_TransportMode = Core.Constants.TransportModes.Sea;
			result.JS_RL_NKDestination = "AUSYD";
			result.JS_OuterPacks = 10;
			result.JS_F3_NKPackType = "PKG";
			return result;
		}

		protected virtual SeaCargoSynchroniser GetSeaCargoSynchroniser(CommonConsol consol) => new CMRSeaCargoSynchroniser(consol);
	}
}
