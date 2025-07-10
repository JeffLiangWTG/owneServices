using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public static class ComplianceJobEntityCacheHelperTest
	{
		public static ComplianceJobEntityCache CreateNewComplianceJobEntityCache(this BusinessObjectFactory factory, ComplianceRiskStatus complianceRisk, BusinessObject entity)
		{
			var cache = factory.New<ComplianceJobEntityCache>();
			cache.CJE_COR_ComplianceRisk = complianceRisk.PK;
			cache.CJE_EntityID = entity.PK;
			cache.CJE_EntityTableCode = entity.TablePrefix;
			return cache;
		}

		public static (ComplianceRiskStatus, ForwardingShipment) CreateNewComplianceRiskWithShipment(this BusinessObjectFactory factory)
		{
			var shipment = factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(OrganisationsDataRegistry.Instance.ComplianceJobEndDateLimit.Value);

			var complianceRisk = factory.New<ComplianceRiskStatus>();
			complianceRisk.COR_ParentID = shipment.PK;
			complianceRisk.COR_ParentTableCode = shipment.TablePrefix;
			return (complianceRisk, shipment);
		}

		public static BusinessObject CreateNewJobDocAddress(this BusinessObjectFactory factory, BusinessObject job)
		{
			var jobDocAddress = factory.NewWithValidTestData<JobDocAddress>();
			SetDefaultValueJobDocAddress(jobDocAddress, job);
			return jobDocAddress;
		}

		public static (OrgHeader, OrgHeader) CreateNewOrgConsignorAndConsignee(this BusinessObjectFactory factory)
		{
			var consignor = factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignee = true;
			consignor.OH_Code = "Consignee";

			var consignee = factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignor";

			return (consignor, consignee);
		}

		public static ComplianceJobEntityCache[] LoadComplianceJobEntityCache(this BusinessObjectFactory factory, ComplianceRiskStatus complianceRisk)
		{
			return factory.Load<ComplianceJobEntityCache>(new ZQuery(ComplianceJobEntityCacheSchema.CJE_COR_ComplianceRisk, complianceRisk.PK));
		}

		public static bool IsEntityCacheInDatabase(this BusinessObjectFactory factory, ComplianceJobEntityCache[] caches, BusinessObject entity)
		{
			return caches.Any(c => c.CJE_EntityID == entity.PK && c.CJE_EntityTableCode == entity.TablePrefix);
		}

		public static void SetDefaultValueJobDocAddress(this BusinessObjectFactory factory, JobDocAddress jobDocAddress, BusinessObject job)
		{
			SetDefaultValueJobDocAddress(jobDocAddress, job);
		}

		public static void SetDefaultValueJobDocAddress(JobDocAddress jobDocAddress, BusinessObject job)
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_City = "MELBOURNE";
			jobDocAddress.E2_Address1 = "23 CROWN ST";
			jobDocAddress.E2_Address2 = "PART2OV";
			jobDocAddress.E2_Postcode = "3000";
			jobDocAddress.E2_CompanyName = "TEST OVERRIDE COMPANY";
			jobDocAddress.E2_ParentID = job.PK;
		}

		public static Transport AddTransportLeg(this ForwardingShipment shipment, string loadPort, string discPort, string vessel)
		{
			var transport = shipment.Transports.AddNew();
			transport.JW_TransportMode = shipment.JS_TransportMode;
			transport.JW_RL_NKLoadPort = loadPort;
			transport.JW_RL_NKDiscPort = discPort;
			transport.JW_Vessel = vessel;
			return transport;
		}

		public static (ComplianceRiskStatus, ForwardingConsol, Transport) CreateNewComplianceRiskWithConsol(this BusinessObjectFactory factory)
		{
			var consol = factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "USLAX";

			var transport = consol.Transports.AddNew();
			transport.JW_TransportMode = consol.JK_TransportMode;
			transport.JW_RL_NKLoadPort = consol.JK_RL_NKLoadPort;
			transport.JW_RL_NKDiscPort = consol.JK_RL_NKDischargePort;
			transport.JW_ETD = ZDateTime.Now;
			transport.JW_ETA = ZDateTime.Now.AddDays(OrganisationsDataRegistry.Instance.ComplianceJobEndDateLimit.Value);

			var complianceRisk = factory.New<ComplianceRiskStatus>();
			complianceRisk.COR_ParentID = consol.PK;
			complianceRisk.COR_ParentTableCode = consol.TablePrefix;
			return (complianceRisk, consol, transport);
		}
	}
}
