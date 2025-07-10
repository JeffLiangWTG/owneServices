using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromConsol))]
	sealed class FreightWrapperFromConsolTest : FreightWrapperTest
	{
		[TestDate(2017, 03, 12)]
		public void TestTransportReference()
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			forwardingConsol.FillWithValidTestData();
			forwardingConsol.JK_TransportMode = Constants.TransportModes.Air;

			var transport = forwardingConsol.Transports.MostInterestingTransport;
			transport.JW_IsLinked = true;
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_VoyageFlight = "AAA";
			transport.JW_TransportType = "AAA";

			var wrapper = new FreightWrapperFromConsol(forwardingConsol, Factory);
			AssertEquals("Should Not Empty", "AAA / AUSYD / 12-Mar-17", wrapper.TransportReference);

			var secondTransport = (Transport)transport.Clone();
			secondTransport.JW_IsLinked = true;
			secondTransport.JW_RL_NKLoadPort = "AUSYD";
			secondTransport.JW_RL_NKDiscPort = "NZAKL";
			secondTransport.JW_ETD = ZDateTime.Now.AddDays(1);
			secondTransport.JW_VoyageFlight = "BBB";
			secondTransport.JW_TransportType = "BBB";

			forwardingConsol.Transports.Add(secondTransport);

			wrapper = new FreightWrapperFromConsol(forwardingConsol, Factory);
			AssertEquals("AAA / AUSYD / 12-Mar-17 -> BBB / NZAKL / 13-Mar-17", wrapper.TransportReference);
		}

		public override void TestArrivalCFSTransport()
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "Arrival";

			forwardingConsol.JK_OA_ArrivalUnpackCFSTransportAddress = header.MainAddress.PK;

			var wrapper = new FreightWrapperFromConsol(forwardingConsol, Factory);
			AssertNotNull(wrapper.ArrivalCFSTransport);
			AssertEquals("ARRIVAL", wrapper.ArrivalCFSTransport.CompanyName);
		}

		public override void TestDepartureCFSTransport()
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			var header = Factory.New<OrgHeader>();
			header.OH_FullName = "Departure";

			forwardingConsol.JK_OA_DeparturePackCFSTransportAddress = header.MainAddress.PK;

			var wrapper = new FreightWrapperFromConsol(forwardingConsol, Factory);
			AssertNotNull(wrapper.DepartureCFSTransport);
			AssertEquals("DEPARTURE", wrapper.DepartureCFSTransport.CompanyName);
		}

		protected override CommonContainer AddContainer(BusinessObject parent)
		{
			return ((ForwardingConsol)parent).Containers.AddNew();
		}

		public void TestFullHandlingInstructions()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handle with no care");
			var wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Handle with no care", wrapper.FullHandlingInstructions);
		}

		public void TestFullCartageInstructions()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.PickupInstructionsNote.Description, "Pick it up");
			var wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Pick it up", wrapper.FullCartageInstructions);
		}

		public void TestDeliveryAgent()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			DeliveryAgentOrgHeader deliveryAgent = Factory.New<DeliveryAgentOrgHeader>();
			wrapper.SetDeliveryAgent(deliveryAgent);
			AssertEquals("wrapper.DeliveryAgent.WrappedObjectPK", deliveryAgent.PK, wrapper.DeliveryAgent.WrappedObjectPK);
		}

		public void TestTotalAgentAndLocalClientCharges()
		{
			GlbBranch.CurrentBranch.Country.RN_RX_NKLocalCurrency = "AUD";

			ZString homePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			if (homePort.IsEmpty)
			{
				homePort = "AUSYD";
			}

			ZString overseasPort = homePort.StartsWith("GB") ? "NLAMS" : "GBLON";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = homePort;
			consol.JK_RL_NKDischargePort = overseasPort;

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			shipment1.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment1.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Job header1 = Factory.NewJobForTesting<Job>();
			header1.JH_ParentID = shipment1.PK;
			header1.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header1.Parent = shipment1;
			header1.LocalChargesPK = shipment1.ConsignorPK;
			header1.AgentCollectPK = shipment1.ConsigneePK;
			AddCharge(header1, "OWHARF", 1101m); // prepaid
			AddCharge(header1, "DWHARF", "AUD", 1100m).JR_OH_SellAccount = header1.AgentCollectPK; // collect
			AddCharge(header1, "FRT", "USD", 15100m); // collect

			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			shipment2.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment2.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Job header2 = Factory.NewJobForTesting<Job>();
			header2.JH_ParentID = shipment2.PK;
			header2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header2.Parent = shipment2;
			header2.LocalChargesPK = shipment2.ConsignorPK;
			header2.AgentCollectPK = shipment2.ConsigneePK;
			AddCharge(header2, "OWHARF", 1202m); // prepaid
			AddCharge(header2, "DWHARF", "USD", 1200m).JR_OH_SellAccount = header2.AgentCollectPK; // collect
			AddCharge(header2, "FRT", "USD", 15200m); // collect

			ForwardingShipment shipment3 = consol.Shipments.AddNew();
			shipment3.JS_INCO = Constants.IncoTerms.FreeOnBoard;
			shipment3.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment3.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;

			Job header3 = Factory.NewJobForTesting<Job>();
			header3.JH_ParentID = shipment3.PK;
			header3.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			header3.Parent = shipment3;
			header3.LocalChargesPK = shipment3.ConsignorPK;
			header3.AgentCollectPK = shipment3.ConsigneePK;
			AddCharge(header3, "OWHARF", 1404m); // prepaid
			AddCharge(header3, "DWHARF", "HKD", 1400m).JR_OH_SellAccount = header3.AgentCollectPK; // collect
			AddCharge(header3, "FRT", "USD", 15400); // collect

			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);

			AssertEquals("Collect Totals",
				new FreightWrapperFromShipment(shipment1, Factory).JobHeader.JobChargesForAgentCollect.TotalCharges
				+ new FreightWrapperFromShipment(shipment2, Factory).JobHeader.JobChargesForAgentCollect.TotalCharges
				+ new FreightWrapperFromShipment(shipment3, Factory).JobHeader.JobChargesForAgentCollect.TotalCharges,
				wrapper.FreightJobsChargesForOverseasAgent.TotalCharges);

			AssertEquals("TotalJobChargesForLocalClient", 3707m, wrapper.FreightJobsChargesForLocalClient.TotalCharges);
		}

		JobCharge AddCharge(Job header, ZString chargeCode, ZDecimal localSellAmt)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			JobCharge charge = header.Charges.AddNew();
			charge.JR_AC = Factory.LoadTop1<AccChargeCode>(filter).PK;
			charge.JR_LocalSellAmt = localSellAmt;

			return charge;
		}

		JobCharge AddCharge(Job header, ZString chargeCode, ZString osSellCurrency, ZDecimal osSellAmt)
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccChargeCodeSchema.AC_Code, chargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);

			JobCharge charge = header.Charges.AddNew();
			charge.JR_AC = Factory.LoadTop1<AccChargeCode>(filter).PK;
			charge.JR_RX_NKSellCurrency = osSellCurrency;
			charge.JR_OSSellAmt = osSellAmt;

			return charge;
		}

		public void TestOriginAndDestination()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Transport transport1 = consol.Transports.AddNew();
			transport1.JW_RL_NKLoadPort = "NZAKL";
			transport1.JW_RL_NKDiscPort = "AUSYD";
			transport1.JW_ETD = new ZDateTime(2011, 6, 1);
			transport1.JW_ETA = new ZDateTime(2011, 6, 10);
			Transport transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "MYBAG";
			transport2.JW_ETD = new ZDateTime(2011, 6, 11);
			transport2.JW_ETA = new ZDateTime(2011, 6, 18);
			Transport transport3 = consol.Transports.AddNew();
			transport3.JW_RL_NKLoadPort = "MYBAG";
			transport3.JW_RL_NKDiscPort = "CAQUE";
			transport3.JW_ETD = new ZDateTime(2011, 6, 20);
			transport3.JW_ETA = new ZDateTime(2011, 6, 26);
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CAQUE";

			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Origin port is load port defined by consol", "AUSYD", wrapper.Origin.Location.UNLOCO);
			AssertEquals("Destination port is discharge port defined by consol", "CAQUE", wrapper.Destination.Location.UNLOCO);
			AssertEquals("Origin ETD is transport's ETD with same load port as consol", new ZDateTime(2011, 6, 11), wrapper.Origin.EstimatedDate);
			AssertEquals("Destination ETA is transport's ETA with same discharge port as consol", new ZDateTime(2011, 6, 26), wrapper.Destination.EstimatedDate);

			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_RL_NKDischargePort = "INDEL";

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Origin port is load port defined by consol", "USCHI", wrapper.Origin.Location.UNLOCO);
			AssertEquals("Destination port is discharge port defined by consol", "INDEL", wrapper.Destination.Location.UNLOCO);
			AssertEquals("Origin ETD is empty", ZDateTime.Empty, wrapper.Origin.EstimatedDate);
			AssertEquals("Destination ETA is empty", ZDateTime.Empty, wrapper.Destination.EstimatedDate);
		}

		public void TestFreightDepotType()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			AssertEquals("FreightDepotType should be CTO", "CTO", wrapper.FreightDepotType);
			consol.JK_ConsolMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("FreightDepotType should be CFS", "CFS", wrapper.FreightDepotType);
		}

		public void TestPickupLocation()
		{
			var consolPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var shipmentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUMEL"));
			var anotherPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUBNE"));
			LocationWrapper emptyLocationWrapper = new LocationWrapper(null, Factory);

			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Fallback to FreightWrapper PickupLocation", emptyLocationWrapper.UNLOCO, wrapper.PickupLocation.UNLOCO);

			consol.JK_RL_NKLoadPort = consolPort.RL_Code;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "";

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Shipments.Count", 0, consol.Shipments.Count);
			AssertEquals("PickupLocation should be Consol Orgin", consolPort.Code, wrapper.PickupLocation.UNLOCO);

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = shipmentPort.RL_Code;
			consol.JK_AgentType = Constants.AgentType.Direct;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("PickupLocation should be Shipments Orgin", shipmentPort.Code, wrapper.PickupLocation.UNLOCO);

			shipment = consol.Shipments.AddNew();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			shipment.JS_RL_NKOrigin = anotherPort.RL_Code;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("PickupLocation should be Consol Orgin", consolPort.Code, wrapper.PickupLocation.UNLOCO);
		}

		#region TestFreightWrapperCollectionForMultiAWB

		public void TestFreightWrapperCollectionForMultiAWB()
		{
			var masterConsol = Factory.New<ForwardingConsol>();
			masterConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			masterConsol.JK_AgentType = Core.Constants.AgentType.AWBMaster;

			CommonContainer masterContainer = masterConsol.Containers.AddNew();
			masterContainer.JC_ContainerNum = "ASDF098765343";

			var subConsol1 = masterConsol.ColoadConsols.AddNew();
			var subConsol2 = masterConsol.ColoadConsols.AddNew();

			var subShipment1 = subConsol1.Shipments.AddNew();
			masterConsol.Shipments.Add(subShipment1);
			var packline1 = subShipment1.OuterPackLines.AddNew();
			packline1.SetContainer(masterConsol, masterContainer);

			var subShipment2 = subConsol1.Shipments.AddNew();

			var subShipment3 = subConsol2.Shipments.AddNew();
			masterConsol.Shipments.Add(subShipment3);
			var packline2 = subShipment3.OuterPackLines.AddNew();
			var packline3 = subShipment3.OuterPackLines.AddNew();
			packline3.SetContainer(masterConsol, masterContainer);

			Factory.Save();

			var wrapper = new FreightWrapperFromConsol(masterConsol, Factory);
			AssertEquals("FreightWrapperCollection will have subshipments", 2, wrapper.FreightJobs.Count);
			AssertEquals("shipment1 will be in collection.", subShipment1, wrapper.FreightJobs[0].WrappedObject);
			AssertEquals("shipment3 will be in collection.", subShipment3, wrapper.FreightJobs[1].WrappedObject);

			wrapper = new FreightWrapperFromConsol(masterConsol, Factory);
			AssertEquals("FreightWrapperCollection will have subConsols", 2, wrapper.FreightConsolidations.Count);
			AssertEquals("sub consol1 will be in collection.", subConsol1, wrapper.FreightConsolidations[0].WrappedObject);
			AssertEquals("sub consol2 will be in collection.", subConsol2, wrapper.FreightConsolidations[1].WrappedObject);
		}

		#endregion

		#region TestPackageWrapperCollectionForMasterConsol

		public void TestPackageWrapperCollectionForMasterConsol()
		{
			var masterConsol = Factory.New<ForwardingConsol>();
			masterConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			masterConsol.JK_AgentType = Core.Constants.AgentType.AWBMaster;

			CommonContainer masterContainer = masterConsol.Containers.AddNew();
			masterContainer.JC_ContainerNum = "ASDF098765343";

			var subConsol1 = masterConsol.ColoadConsols.AddNew();
			var subConsol2 = masterConsol.ColoadConsols.AddNew();

			var subShipment1 = subConsol1.Shipments.AddNew();
			masterConsol.Shipments.Add(subShipment1);
			subShipment1.JS_UniqueConsignRef = "S000001001";
			var packline1 = subShipment1.OuterPackLines.AddNew();
			packline1.SetContainer(masterConsol, masterContainer);

			var subShipment2 = subConsol2.Shipments.AddNew();
			masterConsol.Shipments.Add(subShipment2);
			subShipment2.JS_UniqueConsignRef = "S000001002";
			var packline2 = subShipment2.OuterPackLines.AddNew();
			var packline3 = subShipment2.OuterPackLines.AddNew();
			packline3.SetContainer(masterConsol, masterContainer);

			Factory.Save();

			var wrapper = new FreightWrapperFromConsol(masterConsol, Factory);
			AssertEquals("Collection count should be 2", 2, wrapper.Packages.Count);
			AssertEquals("collection[0].ContainerNo", "ASDF098765343", wrapper.Packages[0].ContainerNo);
			AssertEquals("collection[1].ContainerNo", "ASDF098765343", wrapper.Packages[1].ContainerNo);

			AssertContainsExactElementsInAnyOrder("Parent JobNumbers", new ZString[] { "S000001001", "S000001002" }, wrapper.Packages.Select(item => ((PackageWrapper)item).Parent.JobNumber).Distinct());
		}

		public void TestPacklinesForBCNShipments()
		{
			var masterConsol = Factory.New<ForwardingConsol>();
			masterConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			masterConsol.JK_AgentType = Core.Constants.AgentType.AWBMaster;
			masterConsol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.SubHouseBillsOnly;

			var masterContainer = masterConsol.Containers.AddNew();
			masterContainer.JC_ContainerNum = "ASDF098765343";

			var buyersConsolLeadShipment1 = masterConsol.Shipments.AddNew();
			masterConsol.Shipments.Add(buyersConsolLeadShipment1);
			var packline1 = buyersConsolLeadShipment1.OuterPackLines.AddNew();

			buyersConsolLeadShipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			packline1.SetContainer(masterConsol, masterContainer);

			var wrapper = new FreightWrapperFromConsol(masterConsol, Factory);
			wrapper.SetReportNameForTesting("Cargo Manifest");
			AssertEquals("For Cargo Manifest, SubHouseBillsOnly option is not applicable for BCN shipments without sub shipments", 1, wrapper.Packages.Count);

			masterConsol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.MastersOnly;

			wrapper = new FreightWrapperFromConsol(masterConsol, Factory);
			wrapper.SetReportNameForTesting("Cargo Manifest");
			AssertEquals("For Cargo Manifest, MastersOnly option is not applicable for BCN shipments without sub shipments", 1, wrapper.Packages.Count);

			masterConsol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.All;

			wrapper = new FreightWrapperFromConsol(masterConsol, Factory);
			wrapper.SetReportNameForTesting("Cargo Manifest");
			AssertEquals("For Cargo Manifest, All option is not applicable for BCN shipments without sub shipments", 1, wrapper.Packages.Count);

			var buyersConsolLeadShipment2 = masterConsol.Shipments.AddNew();
			var packline2 = buyersConsolLeadShipment2.OuterPackLines.AddNew();
			buyersConsolLeadShipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.BuyersConsolLead;
			packline2.SetContainer(masterConsol, masterContainer);

			var subShipmentA = Factory.New<ForwardingShipment>();
			subShipmentA.JS_GoodsDescription = "shipment A";
			subShipmentA.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			subShipmentA.JS_JS_ColoadMasterShipment = buyersConsolLeadShipment2.PK;

			var subShipmentB = Factory.New<ForwardingShipment>();
			subShipmentB.JS_GoodsDescription = "shipment B";
			subShipmentB.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;
			subShipmentB.JS_JS_ColoadMasterShipment = buyersConsolLeadShipment2.PK;

			masterConsol.Shipments.Add(buyersConsolLeadShipment2);
			masterConsol.Shipments.Add(subShipmentA);
			masterConsol.Shipments.Add(subShipmentB);

			wrapper = new FreightWrapperFromConsol(masterConsol, Factory);
			wrapper.SetReportNameForTesting("Cargo Manifest");
			AssertEquals("For Cargo Manifest, SubHouseBillsOnly option is not applicable for BCN shipments", 4, wrapper.Packages.Count);

			masterConsol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.MastersOnly;

			wrapper = new FreightWrapperFromConsol(masterConsol, Factory);
			wrapper.SetReportNameForTesting("Cargo Manifest");
			AssertEquals("For Cargo Manifest, MastersOnly option is not applicable for BCN shipments", 4, wrapper.Packages.Count);

			masterConsol.JK_PrintOptionForColoadsOnManifest = FreightConstants.PrintOptionForCoLoads.All;

			wrapper = new FreightWrapperFromConsol(masterConsol, Factory);
			wrapper.SetReportNameForTesting("Cargo Manifest");
			AssertEquals("For Cargo Manifest, All option is not applicable for BCN shipments", 4, wrapper.Packages.Count);
		}

		#endregion

		public void TestDeliveryLocation()
		{
			var consolPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD"));
			var shipmentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUMEL"));
			var anotherPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUBNE"));
			LocationWrapper emptyLocationWrapper = new LocationWrapper(null, Factory);

			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Should fallback to FreightWrapper DeliveryLocation", emptyLocationWrapper.UNLOCO, wrapper.DeliveryLocation.UNLOCO);

			consol.JK_RL_NKDischargePort = consolPort.RL_Code;
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "";

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Shipments.Count should be zero", 0, consol.Shipments.Count);
			AssertEquals("DeliveryLocation should be Consol Destination", consolPort.Code, wrapper.DeliveryLocation.UNLOCO);

			CommonShipment shipment = consol.Shipments.AddNew();
			shipment.JS_RL_NKDestination = shipmentPort.RL_Code;
			consol.JK_AgentType = Constants.AgentType.Direct;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("DeliveryLocation Shipments Destination", shipmentPort.Code, wrapper.DeliveryLocation.UNLOCO);

			shipment = consol.Shipments.AddNew();
			consol.JK_AgentType = Constants.AgentType.CoLoad;
			shipment.JS_RL_NKDestination = anotherPort.RL_Code;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("DeliveryLocation should be Consol Destination", consolPort.Code, wrapper.DeliveryLocation.UNLOCO);
		}

		public void TestCollectAmount()
		{
			JobConsolCost consolCost = Factory.New<JobConsolCost>();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost.E6_ParentID = consol.PK;
				consolCost.E6_ParentTableCode = "JK";
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			consolCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			consolCost.E6_RX_NKCurrency = "";
			consolCost.E6_OSCostAmount = 3000m;
			consolCost.E6_LocalCostAmount = 3000m;
			consolCost.E6_GC = GlbCompany.CurrentCompany.PK;

			ApportionSplitCharge charge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			consolCost.ApportionmentCharges.Add(charge);
			charge.JR_OSCostAmt = 3000m;
			charge.JR_LocalCostAmt = 3000m;

			Factory.Save();

			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("CollectAmount should be empty", new MoneyWrapper(Money.Empty, Factory).Amount, wrapper.CollectAmount.Amount);

			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("CollectAmount should be empty", new MoneyWrapper(Money.Empty, Factory).Amount, wrapper.CollectAmount.Amount);

			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Collect;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("CollectAmount.Amount should loaded MoneyWrapper", 3000m, wrapper.CollectAmount.Amount);
			AssertEquals("CollectAmount.Currency should loaded MoneyWrapper", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, wrapper.CollectAmount.Currency.Code);
			consolCost.E6_RX_NKCurrency = "INR";
			charge.JR_LocalCostAmt = 3000m; // Restore as it was reset by changing Currency
			Factory.Save();
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("CollectAmount.Currency should loaded MoneyWrapper", "INR", wrapper.CollectAmount.Currency.Code);
		}

		public void TestServices()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingContainer container1 = consol.Containers.AddNew();
			ForwardingContainer container2 = consol.Containers.AddNew();
			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Services for the Consol without any services yet added should be zero", 0, wrapper.Services.Count);

			container1.Services.AddNew();
			container1.Services.AddNew();
			container1.Services.AddNew();
			container2.Services.AddNew();
			container2.Services.AddNew();

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Services for the Consol should be the total of Services in the Consols Containers", 5, wrapper.Services.Count);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestConsignor()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			OrgHeader sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "SENDING FORWARDER";
			sendingForwarder.MainAddress.OA_Address1 = "SF MAIN ADDRESS";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgAddress sfAddr2 = sendingForwarder.Addresses.AddNew();
			sfAddr2.OA_Address1 = "SF ADDRESS";
			sfAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_SendingForwarderAddress = sfAddr2.PK;

			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.Consignor.CompanyNameAndAddress", "SENDING FORWARDER\nSF ADDRESS\nAustralia", wrapper.Consignor.CompanyNameAndAddress);

			consol.JK_AgentType = Constants.AgentType.Direct;

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.CompanyNameAndAddress", "SENDING FORWARDER\nSF ADDRESS\nAustralia", wrapper.Consignor.CompanyNameAndAddress);

			ForwardingShipment shipment = consol.Shipments.AddNew();
			OrgHeader consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "SHIPMENT'S CONSIGNOR";
			consignor.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsignorPK = consignor.PK;

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.Consignor should be the single Shipment's Consignor", "SHIPMENT'S CONSIGNOR", wrapper.Consignor.CompanyName);

			DocumentEngine.DocumentNote docNote = DocumentEngine.DocumentNote.LoadNote(shipment);
			docNote.SetFieldValue("Consignor - Shipper", "hello");
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.Consignor should be set from shipment UDF", "hello", wrapper.Consignor.CompanyName);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestConsignee()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			OrgHeader receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "RECEIVING FORWARDER";
			receivingForwarder.MainAddress.OA_Address1 = "RF MAIN ADDRESS";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgAddress rfAddr2 = receivingForwarder.Addresses.AddNew();
			rfAddr2.OA_Address1 = "RF ADDRESS";
			rfAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ReceivingForwarderAddress = rfAddr2.PK;

			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.Consignee.CompanyNameAndAddress should be RECEIVING FORWARDER", "RECEIVING FORWARDER\nRF ADDRESS\nAustralia", wrapper.Consignee.CompanyNameAndAddress);

			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_OA_ReceivingForwarderAddress = rfAddr2.PK;

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.Consignee.CompanyNameAndAddress should be RECEIVING FORWARDER", "RECEIVING FORWARDER\nRF ADDRESS\nAustralia", wrapper.Consignee.CompanyNameAndAddress);

			ForwardingShipment shipment = consol.Shipments.AddNew();
			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "SHIPMENT'S CONSIGNEE";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";
			shipment.ConsigneePK = consignee.PK;

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.Consignee should be the single Shipment's Consignor", "SHIPMENT'S CONSIGNEE", wrapper.Consignee.CompanyName);

			DocumentEngine.DocumentNote docNote = DocumentEngine.DocumentNote.LoadNote(shipment);
			docNote.SetFieldValue("Consignee - Importer", "hello");
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.Consignee should be set from shipment UDF", "hello", wrapper.Consignee.CompanyName);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestNotifyParty()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.NotifyParty should be ZString.Empty", ZString.Empty, wrapper.NotifyParty.CompanyName);

			consol.JK_AgentType = Constants.AgentType.Direct;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.NotifyParty should be ZString.Empty", ZString.Empty, wrapper.NotifyParty.CompanyName);

			OrgHeader notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_FullName = "SHIPMENT'S NOTIFY PARTY";
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notifyParty.PK;
			consol.JK_AgentType = Constants.AgentType.Other;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.NotifyParty should be ZString.Empty", ZString.Empty, wrapper.NotifyParty.CompanyName);

			consol.JK_AgentType = Constants.AgentType.Direct;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.NotifyParty should be the single Shipment's NotifyParty", "SHIPMENT'S NOTIFY PARTY", wrapper.NotifyParty.CompanyName);

			DocumentEngine.DocumentNote docNote = DocumentEngine.DocumentNote.LoadNote(shipment);
			docNote.SetFieldValue("Notify Party", "Shipment UDF");
			Factory.Save();
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.NotifyParty should be shipment's UDF", "Shipment UDF", wrapper.NotifyParty.ContactName);
			AssertEquals(ZString.Empty, wrapper.NotifyParty.CompanyName);
			AssertEquals(ZString.Empty, wrapper.NotifyParty.CompanyNameAndAddress);

			consol.JK_AgentType = Constants.AgentType.Agent;

			OrgHeader receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "RECEIVING FORWARDER";
			OrgAddress rfAddr2 = receivingForwarder.Addresses.AddNew();
			rfAddr2.OA_Address1 = "RF ADDRESS";
			rfAddr2.OA_Address1 = "Receiving Forwarder Address";
			rfAddr2.OA_Phone = "12121212";
			rfAddr2.OA_Fax = "21212121";
			consol.JK_OA_ReceivingForwarderAddress = rfAddr2.PK;

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("RECEIVING FORWARDER", wrapper.NotifyParty.CompanyName);
			AssertEquals("Notify Party phone", "12121212", wrapper.NotifyParty.ContactPhone);
			AssertEquals("Notify Party fax", "21212121", wrapper.NotifyParty.ContactFax);

			consol.NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
			consol.NotifyPartyDocumentaryAddress.E2_CompanyName = "Test Notify Party";
			consol.NotifyPartyDocumentaryAddress.E2_Phone = "232323";
			consol.NotifyPartyDocumentaryAddress.E2_Fax = "343434";

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Test Notify Party", wrapper.NotifyParty.CompanyName);
			AssertEquals("Notify Party phone", "232323", wrapper.NotifyParty.ContactPhone);
			AssertEquals("Notify Party fax", "343434", wrapper.NotifyParty.ContactFax);
		}

		public void TestCarrierBookingAgent()
		{
			var consol = Factory.New<ForwardingConsol>();
			var wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.CarrierBookingAgent should be ZString.Empty", ZString.Empty, wrapper.CarrierBookingAgent.CompanyName);

			var carrierBookingAgent = Factory.NewWithValidTestData<OrgHeader>();
			carrierBookingAgent.OH_FullName = "CARRIER BOOKING AGENT FULL";
			carrierBookingAgent.MainAddress.OA_Address1 = "CBA Address 1";
			carrierBookingAgent.MainAddress.OA_Address2 = "CBA Address 2";
			carrierBookingAgent.MainAddress.OA_PostCode = "2000";

			consol.CarrierBookingAgentDocumentaryAddress.OrganisationPK = carrierBookingAgent.PK;

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("CARRIER BOOKING AGENT FULL", wrapper.CarrierBookingAgent.CompanyName);
			AssertEquals("CBA ADDRESS 1\nCBA ADDRESS 2\n2000", wrapper.CarrierBookingAgent.CompanyAddress.ToString());
		}

		public void TestCarrierHandlingAgent()
		{
			var consol = Factory.New<ForwardingConsol>();
			var wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.CarrierHandlingAgent should be ZString.Empty", ZString.Empty, wrapper.CarrierHandlingAgent.CompanyName);

			var carrierHandlingAgent = Factory.NewWithValidTestData<OrgHeader>();
			carrierHandlingAgent.OH_FullName = "CARRIER Handling AGENT FULL";
			carrierHandlingAgent.MainAddress.OA_Address1 = "CHA Address 1";
			carrierHandlingAgent.MainAddress.OA_Address2 = "CHA Address 2";
			carrierHandlingAgent.MainAddress.OA_PostCode = "2000";

			consol.CarrierHandlingAgentDocumentaryAddress.OrganisationPK = carrierHandlingAgent.PK;

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("CARRIER HANDLING AGENT FULL", wrapper.CarrierHandlingAgent.CompanyName);
			AssertEquals("CHA ADDRESS 1\nCHA ADDRESS 2\n2000", wrapper.CarrierHandlingAgent.CompanyAddress.ToString());
		}

		public void TestNotifyParty2()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_AgentType = Constants.AgentType.Direct;
			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.NotifyParty2 should be ZString.Empty", ZString.Empty, wrapper.NotifyParty2.CompanyName);

			ForwardingShipment shipment = consol.Shipments.AddNew();
			OrgHeader notifyParty = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty.OH_FullName = "SHIPMENT'S NOTIFY PARTY TWO";
			shipment.NotifyParty2DocumentaryAddress.OrganisationPK = notifyParty.PK;
			consol.JK_AgentType = Constants.AgentType.Other;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.NotifyParty3 should be ZString.Empty", ZString.Empty, wrapper.NotifyParty3.CompanyName);

			consol.JK_AgentType = Constants.AgentType.Direct;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.NotifyParty should be the single Shipment's NotifyParty", "SHIPMENT'S NOTIFY PARTY TWO", wrapper.NotifyParty2.CompanyName);

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_AgentType = Constants.AgentType.Other;
			OrgHeader notifyParty2 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty2.OH_FullName = "CONSOL NOTIFY PARTY";

			consol2.NotifyParty2DocumentaryAddress.OrganisationPK = notifyParty2.PK;
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();

			wrapper = new FreightWrapperFromConsol(consol2, Factory);
			AssertEquals("wrapper.NotifyParty should be the consol's NotifyParty", "CONSOL NOTIFY PARTY", wrapper.NotifyParty2.CompanyName);
		}

		public void TestNotifyParty3()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			consol.JK_AgentType = Constants.AgentType.Direct;
			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.NotifyParty3 should be ZString.Empty", ZString.Empty, wrapper.NotifyParty3.CompanyName);

			ForwardingShipment shipment = consol.Shipments.AddNew();
			OrgHeader notifyParty3 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty3.OH_FullName = "SHIPMENT'S NOTIFY PARTY THREE";
			shipment.NotifyParty3DocumentaryAddress.OrganisationPK = notifyParty3.PK;
			consol.JK_AgentType = Constants.AgentType.Other;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.NotifyParty3 should be ZString.Empty", ZString.Empty, wrapper.NotifyParty3.CompanyName);

			consol.JK_AgentType = Constants.AgentType.Direct;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("wrapper.NotifyParty should be the single Shipment's NotifyParty", "SHIPMENT'S NOTIFY PARTY THREE", wrapper.NotifyParty3.CompanyName);

			ForwardingConsol consol2 = Factory.NewWithValidTestData<ForwardingConsol>();
			consol2.JK_AgentType = Constants.AgentType.Agent;
			OrgHeader notifyParty2 = Factory.NewWithValidTestData<OrgHeader>();
			notifyParty2.OH_FullName = "CONSOL NOTIFY PARTY";
			consol2.NotifyParty3DocumentaryAddress.OrganisationPK = notifyParty2.PK;
			var shipment2 = consol2.Shipments.AddNew();

			wrapper = new FreightWrapperFromConsol(consol2, Factory);
			AssertEquals("wrapper.NotifyParty should be the consol's NotifyParty", "CONSOL NOTIFY PARTY", wrapper.NotifyParty3.CompanyName);
		}

		public void TestExportReceivingDepotAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			var wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingDepotAddress.Address);

			var packCFS = Factory.New<OrgHeader>();

			var consolPackDepotAddress = packCFS.MainAddress;
			consolPackDepotAddress.OA_Address1 = "CONSOLBO DEPARTUREDEPOTADDRESS 55";
			consolPackDepotAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_PackDepotAddress = consolPackDepotAddress.PK;

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTUREDEPOTADDRESS 55\nAUSTRALIA", wrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("Organization is from packCFS", packCFS, wrapper.ExportReceivingDepotAddress.Organization.WrappedObject);
		}

		public void TestExportReceivingCTOAddress()
		{
			var consol = Factory.New<ForwardingConsol>();
			var wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingCTOAddress.Address);

			var departureCTO = Factory.New<OrgHeader>();

			var consolDepartureCTOAddress = departureCTO.MainAddress;
			consolDepartureCTOAddress.OA_Address1 = "CONSOLBO DEPARTURECTOADDRESS 77";
			consolDepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_DepartureCTOAddress = consolDepartureCTOAddress.PK;

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTURECTOADDRESS 77\nAUSTRALIA", wrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("Organization is from departureCTO", departureCTO, wrapper.ExportReceivingCTOAddress.Organization.WrappedObject);
		}

		public void TestExportReceivalAddress()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivalAddress.Address);

			OrgAddress consolPackDepotAddress = Factory.New<OrgAddress>();
			consolPackDepotAddress.OA_Address1 = "CONSOLBO PACKDEPOTADDRESS 88";
			consolPackDepotAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_PackDepotAddress = consolPackDepotAddress.PK;

			OrgAddress consolDepartureCTOAddress = Factory.New<OrgAddress>();
			consolDepartureCTOAddress.OA_Address1 = "CONSOLBO DEPARTURECTOADDRESS 77";
			consolDepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_DepartureCTOAddress = consolDepartureCTOAddress.PK;

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.PackDepotAddress", "CONSOLBO PACKDEPOTADDRESS 88\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTURECTOADDRESS 77\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);
		}

		public void TestExportReceivalAddress_Organization()
		{
			var consol = Factory.New<ForwardingConsol>();
			var wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivalAddress.Address);

			var packCFS = Factory.New<OrgHeader>();
			var departureCTO = Factory.New<OrgHeader>();

			consol.JK_OA_PackDepotAddress = packCFS.MainAddress.PK;
			consol.JK_OA_DepartureCTOAddress = departureCTO.MainAddress.PK;

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Organization is from packCFS", packCFS, wrapper.ExportReceivalAddress.Organization.WrappedObject);

			consol.JK_ConsolMode = Constants.ContainerModes.FCL;

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Organization is from departureCTO", departureCTO, wrapper.ExportReceivalAddress.Organization.WrappedObject);
		}

		public void TestPickupDeliveryConfirmations()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol1.Shipments.AddNew();
			ForwardingShipment shipment2 = consol2.Shipments.AddNew();
			PackLine packline1 = shipment1.OuterPackLines.AddNew();
			PackLine packline2 = shipment2.OuterPackLines.AddNew();
			ForwardingContainer container1 = consol1.Containers.AddNew();
			ForwardingContainer container2 = consol2.Containers.AddNew();
			packline1.SetContainer(consol1, container1);
			packline2.SetContainer(consol2, container2);

			CommonPickupDeliveryConfirm pickupLooseConfirmation = shipment1.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm deliveryLooseConfirmation = shipment1.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm pickupContainer1Confirmation = container1.OriginConfirm;
			CommonPickupDeliveryConfirm deliveryContainer1Confirmation = container1.DestinationConfirm;
			CommonPickupDeliveryConfirm pickupContainer2ConfirmationShouldntBeInCollection = container2.OriginConfirm;
			CommonPickupDeliveryConfirm deliveryContainer2ConfirmationShouldntBeInCollection = container2.DestinationConfirm;

			consol1.JK_ConsolMode = Constants.ContainerModes.LCL;

			List<CommonPickupDeliveryConfirm> expectedLCL = new List<CommonPickupDeliveryConfirm>();
			expectedLCL.Add(pickupLooseConfirmation);
			expectedLCL.Add(deliveryLooseConfirmation);

			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol1, Factory);
			AssertContainsExactElementsInAnyOrder("wrapper.PickupDeliveryConfirmations",
				c => c.EU_GoodsSignForBy, expectedLCL, Array.ConvertAll(wrapper.PickupDeliveryConfirmations.ToArray<PickupDeliveryConfirmationsWrapper>(),
				(w) => (CommonPickupDeliveryConfirm)w.WrappedObject));

			consol1.JK_ConsolMode = Constants.ContainerModes.FCL;

			List<CommonPickupDeliveryConfirm> expectedFCL = new List<CommonPickupDeliveryConfirm>();
			expectedFCL.Add(pickupContainer1Confirmation);
			expectedFCL.Add(deliveryContainer1Confirmation);

			wrapper = new FreightWrapperFromConsol(consol1, Factory);
			AssertContainsExactElementsInAnyOrder("wrapper.PickupDeliveryConfirmations",
				c => c.EU_GoodsSignForBy, expectedFCL, Array.ConvertAll(wrapper.PickupDeliveryConfirmations.ToArray<PickupDeliveryConfirmationsWrapper>(),
				(w) => (CommonPickupDeliveryConfirm)w.WrappedObject));
		}

		public void TestMasterBillHeading()
		{
			ForwardingConsol consol1 = Factory.New<ForwardingConsol>();
			FreightWrapper wrapper1 = FreightWrapper.New(consol1, Factory)[0];

			ForwardingConsol consol2 = Factory.New<ForwardingConsol>();
			FreightWrapper wrapper2 = FreightWrapper.New(consol2, Factory)[0];

			ForwardingConsol consol3 = Factory.New<ForwardingConsol>();
			FreightWrapper wrapper3 = FreightWrapper.New(consol3, Factory)[0];

			consol1.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("wrapper.MasterBillHeading", "MAWB", wrapper1.MasterBillHeading);

			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("wrapper.MasterBillHeading", "Ocean Bill Of Lading", wrapper2.MasterBillHeading);

			consol3.JK_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("wrapper.MasterBillHeading", "Master Bill", wrapper3.MasterBillHeading);
		}

		public void TestUNDGS()
		{
			var consol = Factory.New<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var shipment3 = consol.Shipments.AddNew();

			var packline1 = shipment1.OuterPackLines.AddNew();
			var packline2 = shipment2.OuterPackLines.AddNew();
			var packline3 = shipment3.OuterPackLines.AddNew();
			var packline4 = shipment3.OuterPackLines.AddNew();

			var undg1 = packline1.UNDGs.AddNew();
			var undg2 = packline1.UNDGs.AddNew();
			var undg3 = packline4.UNDGs.AddNew();

			undg1.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "3005", "a", "IMO").First().PK;
			undg2.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "1002", "", "IMO").First().PK;
			undg3.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "2004", "", "IMO").First().PK;

			FreightWrapper wrapper = FreightWrapper.New(consol, Factory)[0];
			AssertEquals("3 UNDG substances", 3, wrapper.UNDGs.Count);

			var undgs = wrapper.UNDGs.Select(x => ((UNDGSubstanceWrapper)x).UNNumberWithVariant);
			AssertCollectionContains("3005a", undgs);
			AssertCollectionContains("2004", undgs);
			AssertCollectionContains("1002", undgs);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<ForwardingConsol>();
		}

		[TestDate(2007, 1, 1)]
		public void TestFullConsolWrapper()
		{
			#region Setup

			OrgHeader shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "SHIPPING LINE";
			shippingLine.MainAddress.OA_Address1 = "SL MAIN ADDRESS";
			shippingLine.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgAddress slAddr2 = shippingLine.Addresses.AddNew();
			slAddr2.OA_Address1 = "SL ADDRESS";
			slAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ReceivingForwarderAddress = slAddr2.PK;

			OrgHeader creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "CREDITOR";
			creditor.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "SENDING FORWARDER";
			sendingForwarder.MainAddress.OA_Address1 = "SF MAIN ADDRESS";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgAddress sfAddr2 = sendingForwarder.Addresses.AddNew();
			sfAddr2.OA_Address1 = "SF ADDRESS";
			sfAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ReceivingForwarderAddress = sfAddr2.PK;

			OrgHeader receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "RECEIVING FORWARDER";
			receivingForwarder.MainAddress.OA_Address1 = "RF MAIN ADDRESS";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";
			OrgAddress rfAddr2 = receivingForwarder.Addresses.AddNew();
			rfAddr2.OA_Address1 = "RF ADDRESS";
			rfAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ReceivingForwarderAddress = rfAddr2.PK;

			OrgHeader cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "CTO ANOHTER COMPANY";
			cTO.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress unpackdepotAddress = Factory.New<OrgAddress>();
			unpackdepotAddress.OA_Address1 = "TEST WHAT ADDRESS";
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress containerParkEmptyPickupAddress = Factory.New<OrgAddress>();
			containerParkEmptyPickupAddress.OA_Address1 = "CONTAINER YARD EMPTY PICKUP ADDRESS WITH MAYO";
			containerParkEmptyPickupAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress containerParkEmptyReturnAddress = Factory.New<OrgAddress>();
			containerParkEmptyReturnAddress.OA_Address1 = "CONTAINER YARD EMPTY RETURN ADDRESS WITH PICKLES";
			containerParkEmptyReturnAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_AgentsReference = "AGENTS REF";
			consol.JK_ShippedOnBoardDate = new ZDateTime(2007, 6, 15);
			consol.JK_BookingReference = "BOOKING REF";
			consol.JK_UniqueConsignRef = "C09328403-3";
			consol.JK_MasterBillNum = "MBL342ADSF";
			consol.JK_MasterBillIssueDate = new ZDateTime(2007, 12, 15);
			consol.JK_PrepaidCollect = "CCX";
			consol.JK_ConsolStatus = "ASF";
			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.ExpressBofL;
			consol.JK_NoOriginalBills = 5;
			consol.JK_NoCopyBills = 12;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = slAddr2.PK;
			consol.JK_OA_SendingForwarderAddress = sfAddr2.PK;
			consol.JK_OA_ReceivingForwarderAddress = rfAddr2.PK;
			consol.JK_OA_ArrivalCTOAddress = cTO.Addresses[0].PK;
			consol.JK_OA_UnpackDepotAddress = unpackdepotAddress.PK;
			consol.JK_OA_ContainerYardEmptyPickupAddress = containerParkEmptyPickupAddress.PK;
			consol.JK_OA_ContainerYardEmptyReturnAddress = containerParkEmptyReturnAddress.PK;
			consol.JK_TotalShipmentChargableCheck = 8997.3m;
			consol.JK_RL_NKFirstForeignPort = "USLAX";
			consol.JK_RL_NKLastForeignPort = "USMEM";
			consol.JK_RL_NKPortOfFirstArrival = "USNYC";
			consol.JK_CRN = "CRNCRN";
			consol.JK_AWBServiceLevel = "STD";

			JobSailing sailing = Factory.New<JobSailing>();
			JobVoyage voyage = Factory.New<JobVoyage>();
			VoyageDestination voyageDestination = Factory.New<VoyageDestination>();
			voyageDestination.JB_ArrivalReference = "ARRIVAL REFERENCE";
			voyageDestination.JB_Berth = "BOOTH A23";
			voyageDestination.JB_JV = voyage.PK;
			VoyageOrigin voyageOrigin = Factory.New<VoyageOrigin>();
			voyageOrigin.JA_JV = voyage.PK;
			sailing.JX_JB = voyageDestination.PK;
			sailing.JX_JA = voyageOrigin.PK;

			Transport transport = consol.Transports.MostInterestingTransport;
			transport.JW_JX = sailing.PK;
			transport.JW_TerminalAvailabilityDate = new ZDateTime(2007, 4, 28);
			transport.JW_DepotAvailabilityDate = new ZDateTime(2006, 5, 18);
			transport.JW_TerminalStorageDate = new ZDateTime(2006, 5, 13);
			transport.JW_DepotStorageDate = new ZDateTime(2007, 8, 24);
			transport.JW_TerminalCutOff = new ZDateTime(2007, 7, 24);
			transport.JW_DepotCutOff = new ZDateTime(2006, 8, 29);

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			shipment1.JS_ActualWeight = 123.4m;
			shipment1.JS_ActualVolume = 50.5m;

			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			shipment2.JS_ActualWeight = 111.1m;
			shipment2.JS_ActualVolume = 106.4m;

			consol.Containers.AddNew();
			consol.Containers.AddNew();

			shipment1.JS_OuterPacks = 2;
			shipment1.JS_F3_NKPackType = "ART";
			shipment2.JS_OuterPacks = 3;
			shipment2.JS_F3_NKPackType = "ART";

			shipment1.JS_TotalPackageCount = 6;
			shipment2.JS_TotalPackageCount = 2;

			shipment1.JS_F3_NKTotalCountPackType = "MIN";
			shipment2.JS_F3_NKTotalCountPackType = "MIN";

			consol.RequiredDocuments.AddNew();

			shipment1.Services.AddNew();
			shipment1.Services.AddNew();

			shipment1.JS_ActualWeight = 134.5m;
			shipment1.JS_ActualVolume = 56.9m;
			shipment1.JS_ActualChargeable = 7997.3m;
			shipment1.JS_LoadingMeters = 10.24m;

			shipment2.JS_ActualWeight = 100m;
			shipment2.JS_ActualVolume = 100m;
			shipment2.JS_ActualChargeable = 1000m;
			shipment2.JS_LoadingMeters = 100m;

			#endregion

			FreightWrapperFromConsol fullWrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("fullWrapper.IncoTerm.Code", "CCX", fullWrapper.IncoTerm.Code);
			AssertEquals("fullWrapper.IncoTerm.PaymentType", "CCX - Collect", fullWrapper.IncoTerm.PaymentType.CodeAndDescription);
			AssertEquals("fullWrapper.GoodsDescription", "Consolidated Cargo", fullWrapper.GoodsDescription);
			AssertEquals("fullWrapper.ConsolDateCreated", ZDateTime.Empty, fullWrapper.ConsolDateCreated);
			AssertEquals("fullWrapper.ExportAgentsReference", "AGENTS REF", fullWrapper.ExportAgentsReference);
			AssertEquals("fullWrapper.ImportAgentsReference", "AGENTS REF", fullWrapper.ImportAgentsReference);
			AssertEquals("fullWrapper.LocalForwarderReference", "AGENTS REF", fullWrapper.LocalForwarderReference);
			AssertEquals("fullWrapper.ShippedOnBoardDate", new ZDateTime(2007, 6, 15), fullWrapper.ShippedOnBoardDate);
			AssertEquals("fullWrapper.OwnerReference", "AGENTS REF", fullWrapper.OwnerReference);
			AssertEquals("fullWrapper.NoOriginalBills", 5, fullWrapper.NoOriginalBills);
			AssertEquals("fullWrapper.NoCopyBills", 12, fullWrapper.NoCopyBills);
			AssertEquals("fullWrapper.ShippersReference", ZString.Empty, fullWrapper.ShippersReference);
			AssertEquals("fullWrapper.JobNumber", "C09328403-3", fullWrapper.JobNumber);
			AssertEquals("fullWrapper.ArrivalReference", "ARRIVAL REFERENCE", fullWrapper.ArrivalReference);
			AssertEquals("fullWrapper.CTOArrivalBerth", "BOOTH A23", fullWrapper.CTOArrivalBerth);
			AssertEquals("fullWrapper.BookingReference", "BOOKING REF", fullWrapper.BookingReference);
			AssertEquals("fullWrapper.MasterBill", "MBL342ADSF", fullWrapper.MasterBill);
			AssertEquals("fullWrapper.MasterBillHeading", "MAWB", fullWrapper.MasterBillHeading);
			AssertEquals("fullWrapper.HouseBillHeading", "HAWB", fullWrapper.HouseBillHeading);
			AssertEquals("fullWrapper.MasterBillIssue", new ZDateTime(2007, 12, 15), fullWrapper.MasterBillIssue);
			AssertEquals("fullWrapper.ConsolPaymentType", "CCX", fullWrapper.ConsolPaymentType);
			AssertEquals("fullWrapper.ConsolNumber", "C09328403-3", fullWrapper.ConsolNumber);
			AssertEquals("fullWrapper.ConsolType.Code", "AGT", fullWrapper.ConsolType.Code);
			AssertEquals("fullWrapper.ConsolContainerMode.Code", "LSE", fullWrapper.ConsolContainerMode.Code);
			AssertEquals("fullWrapper.ConsolTransportMode.Code", "AIR", fullWrapper.ConsolTransportMode.Code);
			AssertEquals("fullWrapper.ShipmentStatus.Code", "ASF", fullWrapper.ShipmentStatus.Code);
			AssertEquals("fullWrapper.ShipmentContainerMode.Code", "LSE", fullWrapper.ShipmentContainerMode.Code);
			AssertEquals("fullWrapper.ShipmentTransportMode.Code", "AIR", fullWrapper.ShipmentTransportMode.Code);
			AssertEquals("fullWrapper.OrderTransportMode.Code", ZString.Empty, fullWrapper.OrderTransportMode.Code);
			AssertEquals("fullWrapper.ReleaseType.Code", Constants.ShipmentReleaseTypes.ExpressBofL, fullWrapper.ReleaseType.Code);
			AssertEquals("fullWrapper.ConsolCreditor.CompanyName", "CREDITOR", fullWrapper.ConsolCreditor.CompanyName);

			AssertEquals("fullWrapper.Carrier.CompanyNameAndAddress", "SHIPPING LINE\nSL ADDRESS\nAUSTRALIA", fullWrapper.Carrier.CompanyNameAndAddress);
			AssertEquals("fullWrapper.ExportAgent.CompanyNameAndAddress", "SENDING FORWARDER\r\nSF ADDRESS\nAUSTRALIA", fullWrapper.ExportAgent.CompanyNameAndAddress);
			AssertEquals("fullWrapper.ImportAgent.CompanyNameAndAddress", "RECEIVING FORWARDER\nRF ADDRESS\nAUSTRALIA", fullWrapper.ImportAgent.CompanyNameAndAddress);
			AssertEquals("fullWrapper.LocalForwarder.CompanyNameAndAddress", "RECEIVING FORWARDER\r\nRF ADDRESS\nAUSTRALIA", fullWrapper.LocalForwarder.CompanyNameAndAddress);

			AssertEquals("fullWrapper.CTOArrival.CompanyName", "CTO ANOHTER COMPANY", fullWrapper.CTOArrival.CompanyName);
			AssertEquals("fullWrapper.UnpackCFSAddress.CompanyName", "TEST WHAT ADDRESS\nAUSTRALIA", fullWrapper.UnpackCFSAddress.Address);
			AssertEquals("fullWrapper.GoodsAvailableAt.CompanyName", "TEST WHAT ADDRESS\nAUSTRALIA", fullWrapper.GoodsAvailableAt.Address);
			AssertEquals("fullWrapper.ContainerYardEmptyPickupAddress.Address", "CONTAINER YARD EMPTY PICKUP ADDRESS WITH MAYO\nAUSTRALIA", fullWrapper.ContainerYardEmptyPickupAddress.Address);
			AssertEquals("fullWrapper.ContainerYardEmptyReturnAddress.Address", "CONTAINER YARD EMPTY RETURN ADDRESS WITH PICKLES\nAUSTRALIA", fullWrapper.ContainerYardEmptyReturnAddress.Address);
			AssertEquals("fullWrapper.Weight.Value", 234.5m, fullWrapper.Weight.Value);
			AssertEquals("fullWrapper.Volume.Value", 156.9m, fullWrapper.Volume.Value);
			AssertEquals("fullWrapper.LoadingMeters", 110.24m, fullWrapper.LoadingMeters);
			AssertEquals("fullWrapper.ChargeableWeight.Value", 26150m, fullWrapper.ChargeableWeight.Value);
			AssertEquals("fullWrapper.ConsolRoutes.Count", 1, fullWrapper.ConsolRoutes.Count);
			AssertEquals("fullWrapper.Containers.Count", 2, fullWrapper.Containers.Count);
			AssertEquals("fullWrapper.FreightJobs.Count", 2, fullWrapper.FreightJobs.Count);
			AssertEquals("fullWrapper.FreightConsolidations.Count", 0, fullWrapper.FreightConsolidations.Count);
			AssertEquals("fullWrapper.RequiredDocuments.Count", 1, fullWrapper.RequiredDocuments.Count);
			AssertEquals("fullWrapper.Services.Count", 0, fullWrapper.Services.Count);
			AssertEquals("fullWrapper.ShipmentOuterPacksQty", 5, fullWrapper.ShipmentOuterPacksQty.Value.ToZInt());
			AssertEquals("fullWrapper.ShipmentInnerPacksQty", 8, fullWrapper.ShipmentInnerPacksQty.Value.ToZInt());
			AssertEquals("fullWrapper.ShipmentOuterPacksQty", "ART", fullWrapper.ShipmentOuterPacksQty.Unit.ToString());
			AssertEquals("fullWrapper.ShipmentOuterPacksQty", "MIN", fullWrapper.ShipmentInnerPacksQty.Unit.ToString());
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingCTOAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("fullWrapper.ExportReceivalAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivalAddress.Address);
			AssertEquals("fullWrapper.FirstForeignPort", "USLAX", fullWrapper.FirstForeignPort.Location.UNLOCO);
			AssertEquals("fullWrapper.LastForeignPort", "USMEM", fullWrapper.LastForeignPort.Location.UNLOCO);
			AssertEquals("fullWrapper.PortOfFirstArrival", "USNYC", fullWrapper.PortOfFirstArrival.Location.UNLOCO);
			AssertEquals("fullWrapper.CustomsEntryNumber", "CRNCRN", fullWrapper.CustomsEntryNumber);
			AssertEquals("fullWrapper.ServiceLevel", "STD - Standard", fullWrapper.ServiceLevel.ToString());

			AssertEquals("fullWrapper.MainShipToParty.CompanyName", ZString.Empty, fullWrapper.MainShipToParty.CompanyName);
			AssertEquals("fullWrapper.SellingParty.CompanyName", ZString.Empty, fullWrapper.SellingParty.CompanyName);
			AssertEquals("fullWrapper.Consolidator.CompanyName", ZString.Empty, fullWrapper.Consolidator.CompanyName);
			AssertEquals("fullWrapper.StuffingLocation.CompanyName", ZString.Empty, fullWrapper.StuffingLocation.CompanyName);

			AssertEquals("fullWrapper.SendingForwarder.CompanyNameAndAddress", "SENDING FORWARDER\r\nSF ADDRESS\nAUSTRALIA", fullWrapper.SendingForwarder.CompanyNameAndAddress);
			AssertEquals("fullWrapper.ReceivingForwarder.CompanyNameAndAddress", "RECEIVING FORWARDER\nRF ADDRESS\nAUSTRALIA", fullWrapper.ReceivingForwarder.CompanyNameAndAddress);

			consol.JK_RL_NKDischargePort = "USLAX";
			fullWrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("fullWrapper.FreightPayableAt", "USLAX", fullWrapper.FreightPayableAt.UNLOCO);
		}

		public override void TestWrapperNotes()
		{
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "Some dangerous goods crap");
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "Just certificate of origin for testing");
			consol.Notes.AddNew(false, PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description, "Pre alert arrival notice remarks as nothing ever arrives why have this");

			FreightWrapperFromConsol noteWrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("noteWrapper.DangerousGoodsAdditionalHandlingInformation", "Some dangerous goods crap", noteWrapper.Notes[PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description].Text);
			AssertEquals("noteWrapper.CertificateOfOriginNotes", "Just certificate of origin for testing", noteWrapper.Notes[PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description].Text);
			AssertEquals("noteWrapper.PreAlertArrivalNoticeRemarks", "Pre alert arrival notice remarks as nothing ever arrives why have this", noteWrapper.Notes[PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description].Text);
		}

		public void TestPorts()
		{
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "ERZZZ";

			FreightWrapperFromConsol portWrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("portWrapper.ConsolRoutes[First].Origin.UNLOCO", "ERZZZ", portWrapper.ConsolRoutes["First"].Origin.UNLOCO);
			AssertEquals("portWrapper.ConsolRoutes[Last].Destination.UNLOCO", "AUSYD", portWrapper.ConsolRoutes["Last"].Destination.UNLOCO);
			AssertEquals("portWrapper.ShipmentType.Code", "EXP", portWrapper.ShipmentType.Code);
		}

		public void TestWeight()
		{
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 8.00m;
			shipment.JS_DocumentedWeight = 10.5m;
			shipment.JS_ManifestedWeight = 5.1m;

			FreightWrapperFromConsol wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual);
			AssertEquals("Should be the actual weight", "8.000 KG", wrapper.Weight.ValueAndUnitCodeBlankIfZero);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("Should be the Documented weight", "10.500 KG", wrapper.Weight.ValueAndUnitCodeBlankIfZero);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Carrier);
			AssertEquals("Should be the Carrier weight", "5.100 KG", wrapper.Weight.ValueAndUnitCodeBlankIfZero);
		}

		public void TestVolume()
		{
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualVolume = 8.00m;
			shipment.JS_DocumentedVolume = 10.5m;
			shipment.JS_ManifestedVolume = 5.1m;

			FreightWrapperFromConsol wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual);
			AssertEquals("Should be the actual Volume", "8.000 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("Should be the Documented Volume", "10.500 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Carrier);
			AssertEquals("Should be the Carrier Volume", "5.100 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);
		}

		public void TestVolumeForManifestConsolExportDisplayVolumewhenAir()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var shipment = consol.Shipments.AddNew();
				shipment.JS_ActualVolume = 8.00m;
				shipment.JS_DocumentedVolume = 10.5m;
				shipment.JS_ManifestedVolume = 5.1m;
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "DEHAM";
				shipment.JS_TransportMode = "AIR";
				consol.JK_TransportMode = Core.Constants.TransportModes.Air;
				consol.JK_RL_NKLoadPort = "AUSYD";
				consol.JK_RL_NKDischargePort = "DEHAM";

				Env.Registry.ConsolManifestConsolExportDisplayVolumewhenAir = false;
				var wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.ManifestLandscapeDetailed);
				AssertEquals("Should be the actual Volume", "", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.Manifest);
				AssertEquals("Should be the Documented Volume", "", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.ColoadMasterManifest);
				AssertEquals("Should be the Carrier Volume", "", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.SummaryManifest);
				AssertEquals("Should be the Carrier Volume", "", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.ManifestLandscape);
				AssertEquals("Should be the Carrier Volume", "", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				Env.Registry.ConsolManifestConsolExportDisplayVolumewhenAir = true;
				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.ManifestLandscapeDetailed);
				AssertEquals("Should be the actual Volume", "8.000 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.Manifest);
				AssertEquals("Should be the Documented Volume", "8.000 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.ColoadMasterManifest);
				AssertEquals("Should be the Carrier Volume", "8.000 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.SummaryManifest);
				AssertEquals("Should be the Carrier Volume", "8.000 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);

				wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual, DocBaseWrapper.ManifestLandscape);
				AssertEquals("Should be the Carrier Volume", "8.000 M3", wrapper.Volume.ValueAndUnitCodeBlankIfZero);
			}
		}

		public void TestLoadingMeters()
		{
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_LoadingMeters = 10m;
			shipment.JS_DocumentedLoadingMeters = 11m;
			shipment.JS_ManifestedLoadingMeters = 12m;

			FreightWrapperFromConsol wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual);
			AssertEquals("Should be the actual loading meters", 10m, wrapper.LoadingMeters);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("Should be the Documented loading meters", 11m, wrapper.LoadingMeters);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Carrier);
			AssertEquals("Should be the Carrier loading meters", 12m, wrapper.LoadingMeters);
		}

		public void TestChargeable()
		{
			consol.JK_TransportMode = Constants.TransportModes.Sea;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = Constants.TransportModes.Sea;
			shipment.JS_ActualVolume = 8m;
			shipment.JS_DocumentedVolume = 11m;
			shipment.JS_ManifestedVolume = 5m;

			FreightWrapperFromConsol wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Actual);
			AssertEquals("Should be the actual Chargeable", "8.000 M3", wrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Client);
			AssertEquals("Should be the Documented Chargeable", "11.000 M3", wrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero);

			wrapper = CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(consol, WeightAndVolumeDisplayTypes.Codes.Carrier);
			AssertEquals("Should be the Carrier Chargeable", "5.000 M3", wrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero);
		}

		FreightWrapperFromConsol CreateWrapperAndSetupEnvironmentForWeightAndVolumeDisplay(ForwardingConsol consol, string displayType, string reportName = null)
		{
			Env.Registry.ConsolForwardingInstructionWeightAndVolumeDisplay = displayType;

			FreightWrapperFromConsol result = new FreightWrapperFromConsol(consol, Factory);
			result.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			if (!string.IsNullOrEmpty(reportName))
			{
				result.SetReportNameForTesting(reportName);
			}
			else
			{
				result.SetReportNameForTesting(DocBaseWrapper.ForwardingInstruction);
			}
			return result;
		}

		public void TestShippersReference()
		{
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			var wrapper = new FreightWrapperFromConsol(consol, Factory);

			AssertEquals(false, consol.IsDirect);
			AssertEquals("Consol is NOT Direct", ZString.Empty, wrapper.ShippersReference);

			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals(true, consol.IsDirect);
			AssertEquals("Direct Shipment is NULL", ZString.Empty, wrapper.ShippersReference);

			var shipment = consol.Shipments.AddNew();
			AssertEquals(ZString.Empty, shipment.JS_BookingReference);
			AssertEquals(ZString.Empty, shipment.JS_UniqueConsignRef);
			AssertEquals("Direct Shipment has empty reference and empty number", ZString.Empty, wrapper.ShippersReference);

			shipment.JS_UniqueConsignRef = "S0001";
			AssertEquals(ZString.Empty, shipment.JS_BookingReference);
			AssertEquals("Direct Shipment has empty reference", "S0001", wrapper.ShippersReference);

			shipment.JS_BookingReference = "BOOKING REF";
			AssertEquals("Direct Shipment has reference", "BOOKING REF", wrapper.ShippersReference);
		}

		public void TestGetBookingReference()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals(ZString.Empty, wrapper.BookingReference);

			consol.JK_BookingReference = "AAA, BBB";
			AssertEquals("AAA, BBB", wrapper.BookingReference);

			consol.JK_BookingReference = ZString.Empty;

			CusEntryNumber number1 = consol.Numbers.AddNew();
			number1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			number1.CE_EntryNum = "XXX";

			CusEntryNumber number2 = consol.Numbers.AddNew();
			number2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			number2.CE_EntryNum = "YYY";

			CusEntryNumber number3 = consol.Numbers.AddNew();
			number3.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			number3.CE_EntryNum = "  ";

			AssertEquals("YYY", wrapper.BookingReference);

			CusEntryNumber number4 = consol.Numbers.AddNew();
			number4.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			number4.CE_EntryNum = " ZZZ ";

			AssertEquals("YYY, ZZZ", wrapper.BookingReference);

			consol.JK_BookingReference = "CCC";
			AssertEquals("CCC, YYY, ZZZ", wrapper.BookingReference);
		}

		public void TestGetCarrierContractNumber()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals(ZString.Empty, wrapper.CarrierContractNumber);

			consol.JK_CarrierContractNumber = "123456";
			AssertEquals("123456", wrapper.CarrierContractNumber);

			consol.JK_CarrierContractNumber = ZString.Empty;

			CusEntryNumber number1 = consol.Numbers.AddNew();
			number1.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.CON;
			number1.CE_EntryNum = "1234";

			CusEntryNumber number2 = consol.Numbers.AddNew();
			number2.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			number2.CE_EntryNum = "YYY";

			AssertEquals("1234", wrapper.CarrierContractNumber);
		}

		public void TestGoodsAvailableAt()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Other;

			FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertNull(wrapper.GoodsAvailableAt.WrappedObject);

			OrgHeader arrivalCTO = Factory.New<OrgHeader>();
			OrgHeader unpackCFS = Factory.New<OrgHeader>();

			consol.JK_OA_ArrivalCTOAddress = arrivalCTO.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = unpackCFS.MainAddress.PK;

			consol.JK_ConsolMode = Core.Constants.ContainerModes.BreakBulk;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals(arrivalCTO.MainAddress, wrapper.GoodsAvailableAt.WrappedObject);
			AssertEquals(arrivalCTO, wrapper.GoodsAvailableAt.Organization.WrappedObject);

			consol.JK_ConsolMode = Core.Constants.ContainerModes.RollOnRollOff;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals(arrivalCTO.MainAddress, wrapper.GoodsAvailableAt.WrappedObject);
			AssertEquals(arrivalCTO, wrapper.GoodsAvailableAt.Organization.WrappedObject);

			consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals(unpackCFS.MainAddress, wrapper.GoodsAvailableAt.WrappedObject);
			AssertEquals(unpackCFS, wrapper.GoodsAvailableAt.Organization.WrappedObject);

			consol.JK_ConsolMode = Core.Constants.ContainerModes.FCL;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals(unpackCFS.MainAddress, wrapper.GoodsAvailableAt.WrappedObject);
			AssertEquals(unpackCFS, wrapper.GoodsAvailableAt.Organization.WrappedObject);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals(arrivalCTO.MainAddress, wrapper.GoodsAvailableAt.WrappedObject);
			AssertEquals(arrivalCTO, wrapper.GoodsAvailableAt.Organization.WrappedObject);

			consol.JK_ConsolMode = Core.Constants.ContainerModes.BuyersConsol;
			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals(arrivalCTO.MainAddress, wrapper.GoodsAvailableAt.WrappedObject);
			AssertEquals(arrivalCTO, wrapper.GoodsAvailableAt.Organization.WrappedObject);
		}

		public void TestShipmentModeDescriptionChangesWhenLanguageChanges()
		{
			using (var itlMockData = Res.GetLanguageInstance(Core.SharedConstants.Languages.Italian).UseMockData())
			{
				itlMockData.Put("2dae04d5-1614-4c80-9ee3-1a2e2f2a72c6", new ResourceStringData("2dae04d5-1614-4c80-9ee3-1a2e2f2a72c6", string.Empty, string.Empty, "Thgierf Daor", string.Empty));
				ForwardingConsol consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Road;
				consol.JK_ConsolMode = Core.Constants.ContainerModes.Other;
				FreightWrapperFromConsol wrapper = new FreightWrapperFromConsol(consol, Factory);
				AssertEquals("Road Freight", wrapper.ShipmentTransportMode.Description);
				using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Italian))
				{
					AssertEquals("Thgierf Daor", wrapper.ShipmentTransportMode.Description);
				}
				AssertEquals("Road Freight", wrapper.ShipmentTransportMode.Description);
			}
		}

		public void TestWeightVolumeDecimalPlaces()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_OverrideConsolChargeable = true;
			consol.JK_CorrectedConsolWeightUnit = Core.Constants.Weight.Kilograms;
			consol.JK_CorrectedConsolVolumeUnit = Core.Constants.Volume.CubicMetres;
			consol.JK_CorrectedConsolWeight = 1365.264m;
			consol.JK_CorrectedConsolVolume = 16.168m;

			var wrapper = new FreightWrapperFromConsol(consol, Factory);

			AssertEquals(1365.27m, wrapper.Weight.Value);
			AssertEquals("KG", wrapper.Weight.Unit.Code);
			AssertEquals("1365.27 KG", wrapper.Weight.ValueAndUnitCode);

			AssertEquals(16.16m, wrapper.Volume.Value);
			AssertEquals("M3", wrapper.Volume.Unit.Code);
			AssertEquals("16.16 M3", wrapper.Volume.ValueAndUnitCode);

			AssertEquals(16.16m, wrapper.ChargeableWeight.Value);
			AssertEquals("M3", wrapper.ChargeableWeight.Unit.Code);
			AssertEquals("16.16 M3", wrapper.ChargeableWeight.ValueAndUnitCode);
		}

		public void TestImportContainerPenalties()
		{
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("No Import Penalties added yet", 0, wrapper.ImportContainerPenalties.Count);

			container1.ImportPenalties.AddNew();
			container1.ExportPenalties.AddNew();
			container2.ImportPenalties.AddNew();
			container2.ExportPenalties.AddNew();
			container2.ExportPenalties.AddNew();

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Should find 2 Import penalties across the containers", 2, wrapper.ImportContainerPenalties.Count);
		}

		public void TestExportContainerPenalties()
		{
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("No Export Penalties added yet", 0, wrapper.ImportContainerPenalties.Count);

			container1.ImportPenalties.AddNew();
			container1.ExportPenalties.AddNew();
			container2.ImportPenalties.AddNew();
			container2.ExportPenalties.AddNew();
			container2.ExportPenalties.AddNew();

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals("Should find 3 Export penalties across the containers", 3, wrapper.ExportContainerPenalties.Count);
		}

		public void TestContainerPenalties()
		{
			var container1 = consol.Containers.AddNew();
			var container2 = consol.Containers.AddNew();
			var wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals(0, wrapper.ContainerPenalties.Count);

			container1.ImportPenalties.AddNew();
			container1.ExportPenalties.AddNew();
			container2.ImportPenalties.AddNew();
			container2.ExportPenalties.AddNew();

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			AssertEquals(4, wrapper.ContainerPenalties.Count);
		}

		public void TestCO2eEmissions()
		{
			var wrapper = new FreightWrapperFromConsol(consol, Factory);
			var legs = ((ICO2eLegBasedSupporter)consol).Legs;
			AssertEquals(legs.Count, wrapper.CO2eEmissions.Count);

			((IRoutingSupport)consol).TransportsIncludingRelated.AddNew();
			((IRoutingSupport)consol).TransportsIncludingRelated.AddNew();

			wrapper = new FreightWrapperFromConsol(consol, Factory);
			legs = ((ICO2eLegBasedSupporter)consol).Legs;
			AssertEquals(legs.Count, wrapper.CO2eEmissions.Count);
		}

		public override void TestFormattedTotalCO2e()
		{
			var shipment = consol.Shipments.AddNew();
			consol.SetTotalCO2e(500.555555m);
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);

			Factory.Save();

			var consolWrapper = new FreightWrapperFromConsol(consol, Factory);

			AssertEquals("500.556", consolWrapper.FormattedTotalCO2e);

			consol.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			consolWrapper = new FreightWrapperFromConsol(consol, Factory);

			AssertEquals(ZString.Empty, consolWrapper.FormattedTotalCO2e);
		}

		public override void TestCO2eCalculationDate()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.SetCO2ePerTonneInKg(100.1111111m);

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 5m;
			shipment.JS_UnitOfWeight = Constants.Weight.Tonnes;

			Factory.Save();

			var consolWrapper = new FreightWrapperFromConsol(consol, Factory);

			AssertEquals((consol.GetOrCreateJobCO2e() as JobCO2e).JCO_SystemLastEditTimeUtc, consolWrapper.CO2eCalculationDate);
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "FreightDepotType", "CFS" },
					{ "GoodsDescription", "Consolidated Cargo" },
					{ "HouseBillHeading", "HAWB" },
					{ "JobNumberHeading", "Consol" },
					{ "MasterBillHeading", "MAWB" },
					{ "NoCopyBills", "1" },
					{ "TransportReference", "     / AUSYD /    " }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
ArrivalCFSTransport : 
Carrier : SHIPPING LINE\nAUSTRALIA
Consignee : RECEIVING FORWARDER\nAUSTRALIA
Consignor : SENDING FORWARDER\nAUSTRALIA
ConsolContainerMode : LSE - Loose
ConsolCreditor : CREDITOR\nAUSTRALIA
ConsolTransportMode : AIR - Air Freight
ConsolType : AGT - Agent
CTOArrival : CTO ANOHTER COMPANY\nAUSTRALIA
DeliveryLocation : AUSYD - Sydney
DepartureCFSTransport : 
Destination : AUSYD - Sydney
ExportAgent : SENDING FORWARDER\nAUSTRALIA
ExportReceivalAddress : TEST PACK ADDRESS\nAUSTRALIA
ExportReceivingDepotAddress : TEST PACK ADDRESS\nAUSTRALIA
GoodsAvailableAt : TEST WHAT ADDRESS\nAUSTRALIA
ImportAgent : RECEIVING FORWARDER\nAUSTRALIA
ImportArrivalCTOAddress : CTO ANOHTER COMPANY\nAUSTRALIA
LocalForwarder : SENDING FORWARDER\nAUSTRALIA
NotifyParty : RECEIVING FORWARDER\nAUSTRALIA
Origin : ERZZZ
PickupCFSAddress : TEST PACK ADDRESS\nAUSTRALIA
ReceivingForwarder : RECEIVING FORWARDER\nAUSTRALIA
ReleaseType : EBL - Express Bill of Lading
SendingForwarder : SENDING FORWARDER\nAUSTRALIA
ServiceLevel : STD - Standard
ShipmentContainerMode : LSE - Loose
ShipmentStatus : ASF
ShipmentTransportMode : AIR - Air Freight
ShipmentType : EXP - Export
UnpackCFSAddress : TEST WHAT ADDRESS\nAUSTRALIA";
			}
		}

		protected override FreightWrapper GetNewDocumentWrapperWithCarrier()
		{
			((ForwardingConsol)WrappedBO).JK_OA_ShippingLineAddress = Factory.LoadTop1<OrgAddress>(new ZQuery()).PK;
			return new FreightWrapperFromConsol((ForwardingConsol)WrappedBO, Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
		}
		ForwardingConsol consol;

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_FullName = "SHIPPING LINE";
			shippingLine.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader creditor = Factory.New<OrgHeader>();
			creditor.OH_FullName = "CREDITOR";
			creditor.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader sendingForwarder = Factory.New<OrgHeader>();
			sendingForwarder.OH_FullName = "SENDING FORWARDER";
			sendingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader receivingForwarder = Factory.New<OrgHeader>();
			receivingForwarder.OH_FullName = "RECEIVING FORWARDER";
			receivingForwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "CTO ANOHTER COMPANY";
			cTO.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress unpackdepotAddress = Factory.New<OrgAddress>();
			unpackdepotAddress.OA_Address1 = "TEST WHAT ADDRESS";
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress packdepotAddress = Factory.New<OrgAddress>();
			packdepotAddress.OA_Address1 = "TEST PACK ADDRESS";
			packdepotAddress.OA_RN_NKCountryCode = "AU";

			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_RL_NKLoadPort = "ERZZZ";
			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolStatus = "ASF";
			consol.JK_ReleaseType = Constants.ShipmentReleaseTypes.ExpressBofL;
			consol.SetDefaultShippingLineAddress(shippingLine);
			consol.SetDefaultSendingForwarderAddress(sendingForwarder);
			consol.SetDefaultReceivingForwarderAddress(receivingForwarder);
			consol.JK_OA_ArrivalCTOAddress = cTO.Addresses[0].PK;
			consol.JK_OA_UnpackDepotAddress = unpackdepotAddress.PK;
			consol.JK_OA_PackDepotAddress = packdepotAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			return new FreightWrapperFromConsol(consol, Factory);
		}
	}
}
