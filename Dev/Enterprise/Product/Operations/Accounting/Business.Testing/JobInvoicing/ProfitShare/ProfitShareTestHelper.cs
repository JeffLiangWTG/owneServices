using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.JobInvoicing.ProfitShare.Testing
{
	class ProfitShareTestHelper
	{
		public ProfitShareTestHelper(TestObjectCreator testObjectCreator)
		{
			this.testObjectCreator = testObjectCreator;
		}

		readonly TestObjectCreator testObjectCreator;

		public ForwardingConsol CreateConsol(string consolNum)
		{
			return testObjectCreator.CreateGatewayConsol("AUSYD", "SGSIN", consolNum, receivingGatewayCompany: GlbCompany.CurrentCompany, transportMode: "AIR");
		}

		public ForwardingConsol CreateConsol(string origin, string destination, string consolNum, decimal profit, string transportMode = "AIR")
		{
			return CreateConsol(origin, destination, consolNum, profit, Env.Registry.FreightChargeCode, transportMode);
		}

		public ForwardingConsol CreateConsol(string origin, string destination, string consolNum, decimal profit, ZGuid chargeCodePK, string transportMode = "AIR")
		{
			var consol = testObjectCreator.CreateGatewayConsol(origin, destination, consolNum, receivingGatewayCompany: GlbCompany.CurrentCompany, transportMode: transportMode);
			CreateProfit(testObjectCreator.Factory, consol, profit, chargeCodePK);

			return consol;
		}

		internal void CreateProfit(BusinessObjectFactory factory, ForwardingConsol consol, decimal profit, ZGuid chargeCodePK)
		{
			var job = testObjectCreator.CreateJob(consol, newFactory: factory);
			job.PlugInData = consol;

			var chargeCode = factory.Load<AccChargeCode>(chargeCodePK);
			testObjectCreator.CreateCharge(job, chargeCode, 100m, profit + 100m);
		}

		internal ForwardingConsol CreateGatewayConsol(OrgAddress sendingAgentAddress, OrgAddress receivingAgentAddress)
		{
			var consol = testObjectCreator.Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_AgentType = Core.Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = sendingAgentAddress.ClosestPort;
			consol.JK_RL_NKDischargePort = receivingAgentAddress.ClosestPort;
			consol.JK_OA_SendingForwarderAddress = sendingAgentAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			consol.JK_OA_ReceivingForwarderAddress = receivingAgentAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			return consol;
		}

		internal GlbBranch CreateBranch(string branchCode, string companyCode, string countryCode, string portCode)
		{
			var org = testObjectCreator.Factory.NewWithValidTestData<OrgHeader>();
			testObjectCreator.SetupPort(org, countryCode, AgentStatusList.Codes.GatewayAgentWithTariff);
			org.MainAddress.OA_RL_NKRelatedPortCode = portCode;

			var branch = testObjectCreator.Factory.NewCompanyAndBranchWith(companyCode, branchCode, countryCode);
			branch.GB_OH_OrgProxy = org.PK;
			branch.Company.GC_OH_OrgProxy = org.PK;

			return branch;
		}

		public (OrgAddress pickupAddress, OrgAddress deliveryAddress) GetPickupAndDeliveryAddress(string pickup, string delivery)
		{
			var pickupAddress = testObjectCreator.Factory.NewWithValidTestData<OrgAddress>();
			pickupAddress.OA_City = "Consignor City";
			pickupAddress.OA_Address1 = "Consignor Address";
			pickupAddress.OA_RL_NKRelatedPortCode = pickup;

			var deliveryAddress = testObjectCreator.Factory.NewWithValidTestData<OrgAddress>();
			deliveryAddress.OA_City = "Consignee City";
			deliveryAddress.OA_Address1 = "Consignee Address";
			deliveryAddress.OA_RL_NKRelatedPortCode = delivery;

			return (pickupAddress, deliveryAddress);
		}

		public ForwardingShipment CreateShipment(
			string shipmentNum,
			string origin = null,
			string destination = null,
			string transportMode = "AIR",
			ZDecimal? chargeable = null,
			ZDecimal? weight = null,
			string weightUnit = "KG",
			ZDecimal? volume = null,
			string volumeUnit = "M3",
			OrgHeader pickupAgent = null,
			OrgHeader deliveryAgent = null,
			ForwardingConsol[] forwardingConsols = null,
			bool changeAgentsAsCreditor = true)
		{
			var shipment = testObjectCreator.CreateShipment(shipmentNum, origin, destination, transportMode: transportMode);

			if (chargeable.HasValue)
			{
				shipment.JS_ActualChargeable = chargeable.Value;
			}

			if (weight.HasValue)
			{
				shipment.JS_ActualWeight = weight.Value;
				shipment.JS_UnitOfWeight = weightUnit;
			}

			if (volume.HasValue)
			{
				shipment.JS_ActualVolume = volume.Value;
				shipment.JS_UnitOfVolume = volumeUnit;
			}

			if (pickupAgent != null)
			{
				if (changeAgentsAsCreditor)
				{
					pickupAgent.OH_IsCreditor = true;
				}
				shipment.PickupAgentPK = pickupAgent.PK;
			}

			if (deliveryAgent != null)
			{
				if (changeAgentsAsCreditor)
				{
					deliveryAgent.OH_IsCreditor = true;
				}
				shipment.JS_OH_DeliveryAgent = deliveryAgent.PK;
			}

			if (forwardingConsols != null)
			{
				shipment.Consols.AddRange(forwardingConsols);
			}

			return shipment;
		}

		public void SetRegistry(ZGuid chargeCodePK)
		{
			SetRegistry(chargeCodePK, Guid.Empty);
		}

		public void SetRegistry(ZGuid chargeCodePK, Guid companyPK)
		{
			var lookups = new OrgProfitSharePartyLookups(null);
			var collection = AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.Value;
			var registryItem = collection[lookups.PartyTypes.GetDescriptionFromCode(OrgProfitSharePartyLookups.PartyTypeCodes.LeadGatewayAgent)];

			if (chargeCodePK.IsEmpty)
			{
				collection.Remove(registryItem);
			}
			else
			{
				registryItem.UseDefaultProfitShareChargeCode = false;
				registryItem.ChargeCode = chargeCodePK;
			}

			AccountingConfigurationRegistry.Instance.ProfitShareChargeCodesPerParty.SetValue(companyPK, Guid.Empty, Guid.Empty, collection);
		}
	}

	class TestLogger : ProfitShareRedistributionLogger
	{
		public List<string> GetLogs(LogType logType)
			=> logs.Where(x => x.Type == logType).Select(x => x.Message).ToList();
	}
}
