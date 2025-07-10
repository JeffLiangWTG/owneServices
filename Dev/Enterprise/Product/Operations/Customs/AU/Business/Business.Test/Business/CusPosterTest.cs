using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusPosterTest : TestCaseWithFactory
	{
		public void TestPostAndSetSACFlag_SeaCargo()
		{
			ForwardingConsol consol = GetConsol(Core.Constants.TransportModes.Sea, OverseasPort, HomePort, Core.Constants.ContainerModes.Bulk);

			ForwardingShipment shipment1 = GetShipment(Core.Constants.TransportModes.Sea, HomePort);
			shipment1.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			shipment1.Logs.AddNew(Events.DataImport, "AU Declaration Style: SAC");
			consol.Shipments.Add(shipment1);

			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			CommonContainer container = consol.Containers.AddNew();
			container.JC_ContainerNum = "N111100004";

			ForwardingPackLine packLine = shipment1.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;

			Poster.PostAndSetSACFlag(consol);

			CusSCAOceanBill oceanBill = GetCusSCAOceanBillForConsol(consol);
			AssertNotNull(oceanBill);

			CusSCAPivot cusSCAPivot = GetCusSCAPivotForShipment(shipment1);
			AssertNotNull(cusSCAPivot);

			AssertEquals(true, cusSCAPivot.CV_IsSAC);

			cusSCAPivot = GetCusSCAPivotForShipment(shipment2);
			AssertNotNull(cusSCAPivot);
		}

		public void TestPostAndSetSACFlag_AirCargo()
		{
			var consol = GetConsol(Core.Constants.TransportModes.Air, "UAIEV", "AUSYD", Core.Constants.ContainerModes.Bulk);
			var shipment = GetShipment(Core.Constants.TransportModes.Air, HomePort);
			shipment.Logs.AddNew(Events.DataImport, "AU Declaration Style: SAC");
			consol.Shipments.Add(shipment);
			Poster.PostAndSetSACFlag(consol);

			var cusMawb = GetCusMAWBForConsol(consol);
			AssertNotNull(cusMawb);

			var cusHawb = CusHAWB.Load(shipment);
			AssertNotNull(cusHawb);
			AssertEquals(true, cusHawb.CS_IsSelfAssessedClearance);
		}

		public void TestPostNotWorkingForAirImportStandaloneNonAustralianShipment()
		{
			ForwardingConsol consol = GetConsol(Core.Constants.TransportModes.Sea, HomePort, OverseasPort, Core.Constants.ContainerModes.LCL);

			ForwardingShipment shipment = GetShipment(Core.Constants.TransportModes.Sea, OverseasPort);
			consol.Shipments.Add(shipment);

			Poster.PostAndSetSACFlag(consol);
			AssertCusHAWBForShipmentIsNull(shipment, "Sea Export Australian Shipment With Consol");

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.Shipments.RemoveAndDeleteAll();

			shipment = GetShipment(Core.Constants.TransportModes.Air, OverseasPort);
			consol.Shipments.Add(shipment);

			Poster.PostAndSetSACFlag(consol);
			AssertCusHAWBForShipmentIsNull(shipment, "Air Export Australian Shipment");
		}

		void AssertCusHAWBForShipmentIsNull(ForwardingShipment shipment, ZString description)
		{
			AssertNull("No cusHAWB for " + description, CusHAWB.Load(shipment));
		}

		CusSCAPivot GetCusSCAPivotForShipment(ForwardingShipment shipment)
		{
			ZQuery cusSCAHouseFilter = new ZQuery(CusSCAPivotSchema.CV_CA, Factory.LoadTop1<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_JS, shipment.PK)).PK);
			return Factory.LoadTop1<CusSCAPivot>(cusSCAHouseFilter);
		}

		CusMAWB GetCusMAWBForConsol(ForwardingConsol consol)
		{
			ZQuery cusMAWBFilter = new ZQuery(CusMAWBSchema.CM_JK, consol.PK);
			return Factory.LoadTop1<CusMAWB>(cusMAWBFilter);
		}

		CusSCAOceanBill GetCusSCAOceanBillForConsol(ForwardingConsol consol)
		{
			var oceanBillFilter = new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK);
			oceanBillFilter.AddToFilter(CusSCAOceanBillSchema.CB_ParentTableCode, JobConsolSchema.Constants.Prefix);
			return Factory.LoadTop1<CusSCAOceanBill>(oceanBillFilter);
		}

		ForwardingShipment GetShipment(ZString transportMode, ZString port)
		{
			ForwardingShipment result = Factory.New<ForwardingShipment>();
			result.JS_TransportMode = transportMode;
			result.ConsigneePK = GetConsignee(port).PK;

			return result;
		}

		ForwardingConsol GetConsol(ZString transportMode, ZString origin, ZString dest, ZString consolMode)
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = transportMode;
			consol.JK_RL_NKDischargePort = dest;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_ConsolMode = consolMode;

			return consol;
		}

		OrgHeader GetConsignee(ZString port)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_Code = "HAWB";
			result.OH_RL_NKClosestPort = port;
			result.OH_IsConsignee = true;
			return result;
		}

		string HomePort
		{
			get { return (!GlbBranch.CurrentBranch.GB_RL_NKHomePort.IsEmpty) ? GlbBranch.CurrentBranch.GB_RL_NKHomePort.ToString() : "AUSYD"; }
		}

		string OverseasPort
		{
			get { return (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "SGSIN") ? "SGSIN" : "USLAX"; }
		}

		CusPoster Poster
		{
			get { return poster ?? (poster = new CusPoster()); }
		}
		CusPoster poster;
	}
}
