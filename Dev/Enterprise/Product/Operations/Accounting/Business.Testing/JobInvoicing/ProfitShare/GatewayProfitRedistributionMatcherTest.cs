using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	public class GatewayProfitRedistributionMatcherTest : TestCaseWithFactory
	{
		[TestDate(2022, 10, 10)]
		public void TestGetBestOrgProfitShareDetails_ByDates()
		{
			var consol = profitShareTestHelper.CreateConsol("CN00001");
			var (pickupAddress, deliveryAddress) = profitShareTestHelper.GetPickupAndDeliveryAddress("AUSYD", "SGSIN");
			var shipment = profitShareTestHelper.CreateShipment("SHP1", origin: "AUSYD", "SGSIN", pickupAgent: pickupAddress.Header, forwardingConsols: new[] { consol });
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;

			var ps1 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			var ps2 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			var ps3 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(4), "AUSYD", "SGSIN");
			var ps4 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(2), "AUSYD", "SGSIN");

			var rules = new[] { ps1, ps2, ps3, ps4 };

			consol.Transports[0].JW_ETD = ZDateTime.Today.AddDays(-3);
			consol.Transports[0].JW_ATD = ZDateTime.Today;
			AssertMatcher("should pick ATD if not empty, ps3 > ps1 > ps4 > ps2 because of largest start date, end date sorting", shipment, rules, ps3);

			consol.Transports[0].JW_ATD = ZDateTime.Today.AddDays(-2);
			AssertMatcher("should pick ATD if not empty, should not match as there is no PS for -2 days", shipment, rules, null);

			consol.Transports[0].JW_ETD = ZDateTime.Today;
			consol.Transports[0].JW_ATD = ZDateTime.Empty;
			AssertMatcher("Fallback to pick ETD if ATD is empty", shipment, rules, ps3);

			consol.Transports[0].JW_ETD = ZDateTime.Today.AddDays(-2);
			AssertMatcher("should not match as there is no PS for -2 days", shipment, rules, null);

			SetPSPortFieldsEmpty(rules);
			SetConsignor(shipment);
			SetConsignee(shipment);
			// setting shipment.JS_E_DEP, as after setting Consignor and Consignee, attached consol is not the most interest consol,
			// so system is unable to find the date from consol, and falling back to shipment.JS_E_DEP
			shipment.JS_E_DEP = ZDateTime.Today;
			AssertMatcher("Should pickup date from shipment.JS_E_DEP", shipment, rules, ps3);
			shipment.Job?.Dispose();
		}

		[TestDate(2022, 10, 10)]
		public void TestGetBestOrgProfitShareDetails_ByContainerMode_GrpPriority()
		{
			var consol = profitShareTestHelper.CreateConsol("CN00001");
			var (pickupAddress, deliveryAddress) = profitShareTestHelper.GetPickupAndDeliveryAddress("AUSYD", "SGSIN");
			var shipment = profitShareTestHelper.CreateShipment("SHP1", origin: "AUSYD", "SGSIN", pickupAgent: pickupAddress.Header, forwardingConsols: new[] { consol });
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			shipment.JS_TransportMode = "SEA";

			consol.Transports[0].JW_ETD = ZDateTime.Today.AddDays(-3);
			consol.Transports[0].JW_ATD = ZDateTime.Today;
			consol.JK_ConsolMode = Core.Constants.ContainerModes.Groupage;

			var ps1 = CreateProfitShare(agentRelationship, "GRP", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			var ps2 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			var ps3 = CreateProfitShare(agentRelationship, "SEA", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			var rules = new[] { ps1, ps2, ps3 };

			shipment.JS_PackingMode = "FCL";
			AssertMatcher("No ContainerMode == FCL available. Fallback to SEA", shipment, rules, ps3);

			shipment.JS_PackingMode = "LCL";
			AssertMatcher("ContainerMode == LCL available, but its consol is GRP; so pick GRP", shipment, rules, ps1);

			shipment.JS_PackingMode = "LCL";
			AssertMatcher("ContainerMode == LCL available, but its consol is GRP and GRP not available, pick LCL", shipment, new[] { ps2, ps3 }, ps2);

			shipment.JS_PackingMode = "LCL";
			AssertMatcher("ContainerMode == LCL not available, fallback to SEA", shipment, new[] { ps3 }, ps3);
			shipment.Job?.Dispose();
		}

		[TestDate(2022, 10, 10)]
		public void TestGetBestOrgProfitShareDetails_ByFreightMode()
		{
			var consol = profitShareTestHelper.CreateConsol("CN00001");
			var (pickupAddress, deliveryAddress) = profitShareTestHelper.GetPickupAndDeliveryAddress("AUSYD", "SGSIN");
			var shipment = profitShareTestHelper.CreateShipment("SHP1", origin: "AUSYD", "SGSIN", pickupAgent: pickupAddress.Header, forwardingConsols: new[] { consol });
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;

			consol.Transports[0].JW_ETD = ZDateTime.Today.AddDays(-3);
			consol.Transports[0].JW_ATD = ZDateTime.Today;

			var ps1 = CreateProfitShare(agentRelationship, "AIR", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			var ps2 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			var ps3 = CreateProfitShare(agentRelationship, "SEA", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			var ps4 = CreateProfitShare(agentRelationship, "FCL", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			var rules = new[] { ps1, ps2, ps3, ps4 };

			shipment.JS_PackingMode = "ULD";
			AssertMatcher("ULD PS is not available, p1 > p2 because of transport mode fallback - AIR", shipment, rules, ps1);

			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "FCL";
			AssertMatcher("FCL PS is available, ps4 should be picked because of exact PackingMode matching", shipment, rules, ps4);

			shipment.JS_PackingMode = "LCL";
			AssertMatcher("LCL PS is available, ps2 should be picked because of exact PackingMode matching", shipment, rules, ps2);

			shipment.Job?.Dispose();
		}

		[TestDate(2022, 10, 10)]
		public void TestGetBestOrgProfitShareDetails_ByLocations()
		{
			var consol = profitShareTestHelper.CreateConsol("CN00001");
			var (pickupAddress, deliveryAddress) = profitShareTestHelper.GetPickupAndDeliveryAddress("AUSYD", "SGSIN");
			var shipment = profitShareTestHelper.CreateShipment("SHP1", origin: "AUSYD", "SGSIN", pickupAgent: pickupAddress.Header, forwardingConsols: new[] { consol });
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;

			var ps1 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			var ps2 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(3), "HKHKG", "SGSIN");
			var ps3 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(3), "HKHKG", "DEHAM");
			var rules = new[] { ps1, ps2, ps3 };

			consol.Transports[0].JW_ETD = ZDateTime.Today.AddDays(-3);
			consol.Transports[0].JW_ATD = ZDateTime.Today;

			AssertMatcher("AUSYD-SGSIN rule exists, should be an exact match", shipment, rules, ps1);

			pickupAddress.OA_RL_NKRelatedPortCode = "";
			AssertMatcher("no match as no PSA with blank pickup", shipment, rules, null);

			ps2.O4_SendingPortOrCountry = "";
			AssertMatcher("ps2 became blank-SGSIN, it should be picked", shipment, rules, ps2);

			pickupAddress.OA_RL_NKRelatedPortCode = "HKHKG";
			deliveryAddress.OA_RL_NKRelatedPortCode = "";
			AssertMatcher("no match as no PSA with blank delivery", shipment, rules, null);

			ps3.O4_ReceivingPortOrCountry = "";
			AssertMatcher("ps3 became HKHKG-blank, it should be picked", shipment, rules, ps3);

			var ps4 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(3), "", "");
			rules = new[] { ps1, ps2, ps3, ps4 };

			pickupAddress.OA_RL_NKRelatedPortCode = "INIXE";
			deliveryAddress.OA_RL_NKRelatedPortCode = "SGSIN";
			AssertMatcher("ps2 (blank-SGSIN) should match, ps2 > ps4 because it has matched delivery location", shipment, rules, ps2);

			pickupAddress.OA_RL_NKRelatedPortCode = "HKHKG";
			deliveryAddress.OA_RL_NKRelatedPortCode = "INIXE";
			AssertMatcher("ps3 (HKHKG-blank) should match, ps3 > ps4 because ps3 has matched pickup location", shipment, rules, ps3);

			pickupAddress.OA_RL_NKRelatedPortCode = "USLAX";
			deliveryAddress.OA_RL_NKRelatedPortCode = "INIXE";
			AssertMatcher("Should fallback to match ps4 now", shipment, rules, ps4);

			shipment.Job?.Dispose();
		}

		[TestDate(2022, 10, 10)]
		public void TestGetBestOrgProfitShareDetails_ClientSpecific()
		{
			var consol = profitShareTestHelper.CreateConsol("CN00001");
			var (pickupAddress, deliveryAddress) = profitShareTestHelper.GetPickupAndDeliveryAddress("AUSYD", "SGSIN");
			var shipment = profitShareTestHelper.CreateShipment("SHP1", origin: "AUSYD", "SGSIN", pickupAgent: pickupAddress.Header, forwardingConsols: new[] { consol });
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;

			consol.Transports[0].JW_ETD = ZDateTime.Today.AddDays(-3);
			consol.Transports[0].JW_ATD = ZDateTime.Today;

			var ps1 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(3), "", "");
			var ps2 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(4), "", "");
			var ps3 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(5), "", "");
			var rules = new[] { ps1, ps2, ps3 };

			AssertMatcher("Job's consignor and consignee are empty, ps3 > ps2 > ps1 because of date matching", shipment, rules, ps3);

			SetConsignor(shipment);
			SetConsignee(shipment);
			// setting shipment.JS_E_DEP, as after setting Consignor and Consignee, attached consol is not the most interest consol,
			// so system is unable to find the date from consol, and falling back to shipment.JS_E_DEP
			shipment.JS_E_DEP = ZDateTime.Today;

			ps2.O4_OrgOverrideType = OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNE.Code;
			ps2.O4_OH_OrgOverride = shipment.ConsigneePK;
			AssertMatcher("If there is only specific for Consignee and matched, then system should pick that, ie. ps2", shipment, rules, ps2);

			ps1.O4_OrgOverrideType = OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNR.Code;
			ps1.O4_OH_OrgOverride = shipment.ConsignorPK;
			AssertMatcher("If there are more than one match for Consignor and Consignee, then system should pick Consignee rule, ps2 > ps1 because of that", shipment, rules, ps2);

			ps2.O4_OrgOverrideType = ZString.Empty;
			ps2.O4_OH_OrgOverride = ZGuid.Empty;
			AssertMatcher("If there is only specific for Consignor and matched, then system should pick that, ie. ps1", shipment, rules, ps1);

			ps1.O4_OrgOverrideType = OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNR.Code;
			ps1.O4_OH_OrgOverride = ZGuid.NewZGuid();
			ps2.O4_OrgOverrideType = OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNE.Code;
			ps2.O4_OH_OrgOverride = ZGuid.NewZGuid();
			AssertMatcher("Org override(s) present but don't match job's consignor and consignee. Should fallback to rule with empty values, ie. ps3", shipment, rules, ps3);

			shipment.Job?.Dispose();
		}

		[TestDate(2022, 10, 10)]
		public void TestGetBestOrgProfitShareDetails_ByControllingCustomer()
		{
			var consol = profitShareTestHelper.CreateConsol("CN00001");
			var (pickupAddress, deliveryAddress) = profitShareTestHelper.GetPickupAndDeliveryAddress("AUSYD", "SGSIN");
			var shipment = profitShareTestHelper.CreateShipment("SHP1", origin: "AUSYD", "SGSIN", pickupAgent: pickupAddress.Header, forwardingConsols: new[] { consol });
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			var controllingCustomer = Factory.New<OrgHeader>();

			consol.Transports[0].JW_ETD = ZDateTime.Today.AddDays(-3);
			consol.Transports[0].JW_ATD = ZDateTime.Today;

			var ps1 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			var ps2 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			ps2.O4_OH_ControllingAgent = controllingCustomer.PK;
			var rules = new[] { ps1, ps2 };

			AssertMatcher("Job has no controlling customer. Should pickup rule without controlling customer, ie. ps1", shipment, rules, ps1);

			shipment.DocAddresses.AddNew(controllingCustomer.MainAddress, DocAddressType.ControllingCustomer);
			AssertMatcher("Should pickup rule with matched controlling customer, ie. ps2", shipment, rules, ps2);

			ps2.O4_OH_ControllingAgent = ZGuid.NewZGuid();
			AssertMatcher("Rule with controlling customer presents but doesn't match job's one, should pick the rule with empty value, ie. ps1", shipment, rules, ps1);

			ps1.O4_OH_ControllingAgent = ZGuid.NewZGuid();
			AssertMatcher("No rule matches job's controlling customer", shipment, rules, null);
			shipment.Job?.Dispose();
		}

		[TestDate(2022, 10, 10)]
		public void TestGetBestOrgProfitShareDetails_ByJobTypeAndGatewayAgentType()
		{
			var consol = profitShareTestHelper.CreateConsol("CN00001");
			var (pickupAddress, deliveryAddress) = profitShareTestHelper.GetPickupAndDeliveryAddress("AUSYD", "SGSIN");
			var shipment = profitShareTestHelper.CreateShipment("SHP1", origin: "AUSYD", "SGSIN", pickupAgent: pickupAddress.Header, forwardingConsols: new[] { consol });
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;

			consol.Transports[0].JW_ETD = ZDateTime.Today.AddDays(-3);
			consol.Transports[0].JW_ATD = ZDateTime.Today;

			var ps1 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			ps1.O4_JobType = JobTypesList.Codes.GCN;
			ps1.O4_GatewayAgentType = GatewayAgentTypesList.Codes.BGW;
			var ps2 = CreateProfitShare(agentRelationship, "LCL", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			ps2.O4_JobType = JobTypesList.Codes.GCN;
			ps2.O4_GatewayAgentType = GatewayAgentTypesList.Codes.SGW;
			var rules = new[] { ps1, ps2 };

			AssertMatcher("If there is specific GCN-BGW, then system should pick that, ie ps1", shipment, rules, ps1);

			ps1.O4_JobType = ZString.Empty;
			AssertMatcher("GCN-SGW > blank", shipment, rules, ps2);

			ps1.O4_JobType = JobTypesList.Codes.GCN;
			ps1.O4_GatewayAgentType = GatewayAgentTypesList.Codes.RGW;
			AssertMatcher("No GCN-BGW, GCN-SGW > GCN-RGW", shipment, rules, ps2);

			ps2.O4_JobType = ZString.Empty;
			AssertMatcher("GCN-RGW > blank", shipment, rules, ps1);

			ps2.O4_JobType = JobTypesList.Codes.SHP;
			AssertMatcher("Only match GCN-RGW", shipment, rules, ps1);

			ps1.O4_GatewayAgentType = ZString.Empty;
			AssertMatcher("Should still match GCN-Blank", shipment, rules, ps1);

			ps1.O4_JobType = ZString.Empty;
			ps1.O4_GatewayAgentType = ZString.Empty;
			AssertMatcher("There is no rule with job type GCN. Should not match any.", shipment, rules, null);

			shipment.Job?.Dispose();
		}

		public void TestGetBestOrgProfitShareDetails_ShouldCreateShipmentJobHeaderIfNotPresent()
		{
			var consol = profitShareTestHelper.CreateConsol("CN00001");

			(var pickupAddress, var deliveryAddress) = profitShareTestHelper.GetPickupAndDeliveryAddress("AUSYD", "SGSIN");
			var shipment = profitShareTestHelper.CreateShipment("SHP1", origin: "AUSYD", "SGSIN", pickupAgent: pickupAddress.Header, forwardingConsols: new[] { consol });
			shipment.ConsignorPickupAddress.E2_OA_Address = pickupAddress.PK;
			shipment.ConsigneeDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			SetConsignor(shipment);
			SetConsignee(shipment);
			shipment.JS_E_DEP = ZDateTime.Today;

			var ps1 = CreateProfitShare(agentRelationship, "AIR", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			var ps2 = CreateProfitShare(agentRelationship, "AIR", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");
			var ps3 = CreateProfitShare(agentRelationship, "AIR", ZDate.Today, ZDate.Today.AddDays(3), "AUSYD", "SGSIN");

			var rules = new[] { ps1, ps2, ps3 };

			ps1.O4_OrgOverrideType = OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNR.Code;
			ps1.O4_OH_OrgOverride = shipment.ConsignorPK;

			AssertNull("Pre-condition: Job should be null", shipment.ShipmentJobHeader);
			AssertMatcher("If there is specific for Consignor, then system should pick that, ie ps1", shipment, rules, ps1);
			AssertNotNull("After first matching call, mather should create job", shipment.ShipmentJobHeader);

			ps2.O4_OrgOverrideType = OrgProfitShareDetailsLookups.OrgOverrideTypesList.CNE.Code;
			ps2.O4_OH_OrgOverride = shipment.ConsigneePK;
			AssertMatcher("If there are more than one match for Consignor and Consignee, then system should pick Consignee rule, ie ps2", shipment, rules, ps2);

			using (var job = shipment.ShipmentJobHeader as Job)
			{
				AssertNotNull("Pre-condition: Job is not null", job);

				var localClientAddr = Factory.NewWithValidTestData<OrgAddress>();
				job.JH_OA_LocalChargesAddr = localClientAddr.PK;

				AssertNotNull("Pre-condition: local client is not null", job.LocalCharges);
				ps3.O4_OrgOverrideType = OrgProfitShareDetailsLookups.OrgOverrideTypesList.LOC.Code;
				ps3.O4_OH_OrgOverride = job.LocalChargesPK;
				AssertMatcher("If there is local client, then system should pick Local Client rule, ie ps3", shipment, rules, ps3);
			}
		}

		public void TestGetBestOrgProfitShareDetails_ShouldLogErrorIfUnableToCreateJob()
		{
			var shipment = profitShareTestHelper.CreateShipment("SHP1");
			Factory.Save();

			AssertEquals("", shipment.CreateShipmentJobHeaderWithMutex());

			try
			{
				var anotherFactory = new BusinessObjectFactory();
				var shipmentInAnotherFactory = anotherFactory.Load<ForwardingShipment>(shipment.PK);

				using (var logger = new TestLogger())
				{
					var matcher = new GatewayProfitRedistributionMatcher(anotherFactory, System.Array.Empty<OrgProfitShareDetails>(), logger);
					matcher.GetBestOrgProfitShareDetails(shipmentInAnotherFactory);
					AssertEquals(@"SHP1: You have created the job SHP1 on another form, but haven't saved it yet.
Please close or save other forms that use job SHP1 to continue.", logger.GetLogs(LogType.Error).FirstOrDefault());
				}
			}
			finally
			{
				shipment.ShipmentJobHeader.Dispose();
			}
		}

		public void TestGetBestOrgProfitShareDetails_ShouldNotDisposeShipmentJobHeaderBeforeSaveOrDelete()
		{
			var shipment = profitShareTestHelper.CreateShipment("SHP1");
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var shipmentInAnotherFactory = anotherFactory.Load<ForwardingShipment>(shipment.PK);

			using (var logger = new TestLogger())
			{
				var matcher = new GatewayProfitRedistributionMatcher(anotherFactory, System.Array.Empty<OrgProfitShareDetails>(), logger);
				matcher.GetBestOrgProfitShareDetails(shipmentInAnotherFactory);
				AssertEquals(@"You have created the job SHP1 on another form, but haven't saved it yet.
Please close or save other forms that use job SHP1 to continue.", shipment.CreateShipmentJobHeaderWithMutex());
			}
			shipmentInAnotherFactory.ShipmentJobHeader.Dispose();
		}

		OrgProfitShareDetails CreateProfitShare(
			OrgAgentRelationship agentRelationship,
			ZString transportMode,
			ZDateTime startDate,
			ZDateTime endDate,
			ZString origin,
			ZString destination,
			OrgHeader orgOverride = null,
			string orgType = null,
			string jobType = JobTypesList.Codes.GCN,
			string gatewayAgentType = null,
			string apportionmentMethod = null)
		{
			var profitShare = agentRelationship.ProfitShareDetails.AddNew();
			profitShare.O4_FreightMode = transportMode;
			profitShare.O4_StartDate = startDate;
			profitShare.O4_EndDate = endDate;
			profitShare.O4_SendingPortOrCountry = origin;
			profitShare.O4_ReceivingPortOrCountry = destination;
			if (orgOverride != null)
			{
				profitShare.O4_OH_OrgOverride = orgOverride.PK;
				if (!string.IsNullOrEmpty(orgType))
				{
					profitShare.O4_OrgOverrideType = orgType;
				}
			}

			profitShare.O4_JobType = jobType;
			profitShare.O4_GatewayAgentType = gatewayAgentType;
			profitShare.O4_GatewayProfitApportionmentMethod = apportionmentMethod;

			return profitShare;
		}

		void AssertMatcher(string message, ForwardingShipment shipment, OrgProfitShareDetails[] rules, OrgProfitShareDetails expected)
		{
			using (var logger = new TestLogger())
			{
				var matcher = new GatewayProfitRedistributionMatcher(Factory, rules, logger);
				AssertEquals(message, expected, matcher.GetBestOrgProfitShareDetails(shipment));
			}
		}

		void SetPSPortFieldsEmpty(IEnumerable<OrgProfitShareDetails> orgProfitShareDetails)
		{
			foreach (var ps in orgProfitShareDetails)
			{
				ps.O4_SendingPortOrCountry = "";
				ps.O4_ReceivingPortOrCountry = "";
			}
		}

		void SetConsignor(ForwardingShipment shipment)
		{
			var consignor = Factory.New<OrgHeader>();
			consignor.OH_FullName = "I'm consignor";
			consignor.OH_RL_NKClosestPort = "CNBSX";
			consignor.MainAddress.Address1 = "Unit 200";
			consignor.MainAddress.Address2 = "55 haha Lane";
			consignor.MainAddress.City = "wahaha Ave";
			consignor.MainAddress.Postcode = "10000";
			consignor.MainAddress.OA_RN_NKCountryCode = "CN";

			shipment.ConsignorDocumentaryAddress.E2_OA_Address = consignor.MainAddress.PK;
		}

		void SetConsignee(ForwardingShipment shipment)
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "I'm consignee";
			consignee.OH_RL_NKClosestPort = "AUMEL";
			consignee.MainAddress.Address1 = "Unit 223";
			consignee.MainAddress.Address2 = "553 What Lane";
			consignee.MainAddress.City = "Melbourne";
			consignee.MainAddress.Postcode = "5023";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";

			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.MainAddress.PK;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var testObjectCreator = new TestObjectCreator(Factory);
			profitShareTestHelper = new ProfitShareTestHelper(testObjectCreator);

			agentRelationship = Factory.New<OrgAgentRelationship>();
			agentRelationship.O3_ProfitShareType = "AGY";
		}

		ProfitShareTestHelper profitShareTestHelper;
		OrgAgentRelationship agentRelationship;
	}
}
