using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceJobEntityCacheSynchronizerTest : TestCaseWithFactory
	{
		public void TestShipmentOrgHeaderAndRefVesselEntityCache()
		{
			var (orgConsignor, orgConsignee) = Factory.CreateNewOrgConsignorAndConsignee();
			var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
			shipment.ConsigneePK = orgConsignee.PK;
			shipment.ConsignorPK = orgConsignor.PK;

			var refVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var transport = shipment.AddTransportLeg("AUSYD", "SGSIN", refVessel.RV_Code);
			var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRisk);

			AssertEquals("Precondition Memory entity cache count 0", 0, complianceBizO.JobEntityCacheCollection.Count);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);

			Factory.Save();

			var entityCache = (new BusinessObjectFactory()).LoadComplianceJobEntityCache(complianceRisk);

			CombineAssertions("Cache should exist in database", () =>
			{
				Assert($"Consignee: {orgConsignee.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgConsignee));
				Assert($"Consignor: {orgConsignor.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgConsignor));
				Assert($"Vessel: {refVessel.PK}", Factory.IsEntityCacheInDatabase(entityCache, refVessel));
			});
		}

		public void TestShipmentWithFreeTextVesselEntityCache()
		{
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			orgConsignee.OH_IsConsignee = true;

			var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
			shipment.ConsigneePK = orgConsignee.PK;

			var transport = shipment.AddTransportLeg("AUSYD", "SGSIN", "FREE TEXT VESSEL");
			var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRisk);

			AssertEquals("Precondition Memory entity cache count 0", 0, complianceBizO.JobEntityCacheCollection.Count);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);

			Factory.Save();

			var entityCache = (new BusinessObjectFactory()).LoadComplianceJobEntityCache(complianceRisk);

			CombineAssertions("Cache should exist in database", () =>
			{
				Assert($"Consignee: {orgConsignee.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgConsignee));
				Assert($"Free text vessel: {transport.PK}", Factory.IsEntityCacheInDatabase(entityCache, transport));
			});
		}

		public void TestShipmentWithOverrideJobDocAddressEntityCache()
		{
			var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
			orgConsignee.OH_IsConsignee = true;

			var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
			shipment.ConsigneePK = orgConsignee.PK;

			var jobDocAddress = shipment.DocAddresses
				.Where(x => x.E2_AddressType == AutoDocAddressTypes.Codes.ConsignorDocumentaryAddress)
				.FirstOrDefault();

			Factory.SetDefaultValueJobDocAddress(jobDocAddress, shipment);

			var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRisk);
			AssertEquals("Precondition Memory entity cache count 0", 0, complianceBizO.JobEntityCacheCollection.Count);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);

			Factory.Save();

			var entityCache = (new BusinessObjectFactory()).LoadComplianceJobEntityCache(complianceRisk);

			CombineAssertions("Cache should exist in database", () =>
			{
				Assert($"Consignee: {orgConsignee.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgConsignee));
				Assert($"Override job doc address: {jobDocAddress.PK}", Factory.IsEntityCacheInDatabase(entityCache, jobDocAddress));
			});
		}

		public void TestCreateAddRemoveEntityCache()
		{
			var (orgConsignor, orgConsignee) = Factory.CreateNewOrgConsignorAndConsignee();

			var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
			shipment.ConsigneePK = orgConsignee.PK;
			shipment.ConsignorPK = orgConsignor.PK;

			var refVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var transport = shipment.AddTransportLeg("AUSYD", "SGSIN", refVessel.RV_Code);
			var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRisk);

			AssertEquals("Precondition Memory entity cache count 0", 0, complianceBizO.JobEntityCacheCollection.Count);
			AssertEntityCacheCount("Created entities, the expected cache count 3", 3);

			// Remove the consignee from the shipment
			shipment.ConsigneePK = ZGuid.Empty;
			AssertEntityCacheCount("Removed consignee, the expected cache count 2", 2);

			// Reinstate the consignee from the shipment
			shipment.ConsigneePK = orgConsignee.PK;

			// Add the pick agent from the shipment
			var orgPickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			shipment.PickupAgentPK = orgPickupAgent.PK;

			AssertEntityCacheCount("The expected cache count 4", 4);

			void AssertEntityCacheCount(string message, int expectedCount)
			{
				ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);

				Factory.Save();

				var entityCache = (new BusinessObjectFactory()).LoadComplianceJobEntityCache(complianceRisk);

				CombineAssertions(message, () =>
				{
					AssertEquals($"Cache collection is updated: {message}", expectedCount, complianceBizO.JobEntityCacheCollection.Count);
					AssertEquals($"Cache exist in database: {message}", expectedCount, entityCache.Length);
				});
			}
		}

		public void TestShipmentWithMasterShipmentEntityCache()
		{
			var (masterConsignor, masterConsignee) = Factory.CreateNewOrgConsignorAndConsignee();
			masterConsignor.OH_Code = "MConsignor";
			masterConsignee.OH_Code = "MConsignee";

			var (childConsignor, childConsignee) = Factory.CreateNewOrgConsignorAndConsignee();
			childConsignor.OH_Code = "CConsignor";
			childConsignee.OH_Code = "CConsignee";

			var (childComplianceRisk, childShipment) = Factory.CreateNewComplianceRiskWithShipment();
			childShipment.ConsigneePK = childConsignee.PK;
			childShipment.ConsignorPK = childConsignor.PK;
			childShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			var complianceBizO = new ComplianceRiskBusinessObject(childShipment, childComplianceRisk);

			AssertEntityCacheCount("Child Shipment created entities, the expected cache count 2", 2, childComplianceRisk);

			var (masterComplianceRisk, masterShipment) = Factory.CreateNewComplianceRiskWithShipment();
			masterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			childShipment.JS_JS_ColoadMasterShipment = masterShipment.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			masterComplianceRisk = newFactory.Load<ComplianceRiskStatus>(masterComplianceRisk.PK);
			masterShipment = newFactory.Load<ForwardingShipment>(masterShipment.PK);
			masterShipment.ConsigneePK = masterConsignee.PK;
			masterShipment.ConsignorPK = masterConsignor.PK;

			complianceBizO = new ComplianceRiskBusinessObject(masterShipment, masterComplianceRisk);

			AssertEntityCacheCount("Master Shipment created entities, the expected cache count 2", 2, masterComplianceRisk);

			void AssertEntityCacheCount(string message, int expectedCount, ComplianceRiskStatus expectedComplianceRisk)
			{
				ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);
				complianceBizO.HostBusinessEntity.Factory.Save();

				var entityCache = (new BusinessObjectFactory()).LoadComplianceJobEntityCache(expectedComplianceRisk);

				CombineAssertions(message, () =>
				{
					AssertEquals($"Cache collection is updated: {message}", expectedCount, complianceBizO.JobEntityCacheCollection.Count);
					AssertEquals($"Cache exist in database: {message}", expectedCount, entityCache.Length);
				});
			}
		}

		public void TestRegisrtyDisabled()
		{
			using (OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment
				.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetJobEntitiesCaching(false)))
			{
				var orgConsignee = Factory.NewWithValidTestData<OrgHeader>();
				orgConsignee.OH_IsConsignee = true;

				var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
				shipment.ConsigneePK = orgConsignee.PK;
				complianceRisk.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

				Factory.Save();

				ComplianceRiskStatusSynchronizer.Synchronize(complianceRisk.PlugInParent);
				var entityCache = Factory.LoadComplianceJobEntityCache(complianceRisk);
				AssertEquals("Cache should not exist in database", 0, entityCache.Length);
			}
		}

		public void TestShipmentWithDeclarationEntityCache()
		{
			var (orgConsignor, orgForwarder) = Factory.CreateNewOrgConsignorAndConsignee();
			orgForwarder.OH_IsForwarder = true;

			var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
			shipment.ConsignorPK = orgConsignor.PK;

			var refVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			var transport = shipment.AddTransportLeg("AUSYD", "SGSIN", "FREE TEXT VESSEL");
			var declaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration[JobDeclarationSchema.JE_JS] = shipment.PK;
			declaration[JobDeclarationSchema.JE_RL_NKOrigin] = shipment.JS_RL_NKOrigin;
			declaration[JobDeclarationSchema.JE_RL_NKFinalDestination] = shipment.JS_RL_NKDestination;
			declaration[JobDeclarationSchema.JE_TransportMode] = shipment.JS_TransportMode;
			declaration[JobDeclarationSchema.JE_VesselName] = refVessel.RV_Code;

			var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRisk);
			ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);

			Factory.Save();

			var entityCache = Factory.LoadComplianceJobEntityCache(complianceRisk);

			CombineAssertions("The entity cache is saved in the database", () =>
			{
				AssertEquals("Cache collection count 3", 3, complianceBizO.JobEntityCacheCollection.Count);
				Assert($"Free Text Vessel: {transport.PK}", Factory.IsEntityCacheInDatabase(entityCache, transport));
				Assert($"Consignor: {orgConsignor.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgConsignor));
				Assert($"Vessel: {refVessel.PK}", Factory.IsEntityCacheInDatabase(entityCache, refVessel));
			});
		}

		public void TestShipmentWithHVLVConsignmentEntityCache()
		{
			using (HVLVDataRegistry.Instance.HVLVEnablePartyScreening.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new HVLVEnablePartyScreening() { EnableHVLVPartyScreening = true }))
			{
				var (orgShpConsignee, orgHVLVConsignee) = Factory.CreateNewOrgConsignorAndConsignee();
				orgHVLVConsignee.OH_Code = "HVLVCONS";
				orgHVLVConsignee.OH_FullName = "HVLV Consignee";

				var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;

				var consignmentHeader = Factory.New<HVLVConsignmentHeader>();
				consignmentHeader.HCH_JS_Shipment = shipment.PK;

				var consignment = consignmentHeader.Consignments.AddNew();
				consignment.HVC_WeightUQ = Weight.Grams;
				consignment.HVC_ConsigneeName = orgHVLVConsignee.OH_FullName;

				var item = consignment.Items.AddNew();
				item.HVI_ActualWeight = 1000;

				var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRisk);

				AssertEquals("Precondition Memory entity cache count 0", 0, complianceBizO.JobEntityCacheCollection.Count);

				ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);
				AssertEquals("HVLV Consignment: Cache collection count 1", 1, complianceBizO.JobEntityCacheCollection.Count);

				shipment.ConsigneePK = orgShpConsignee.PK;

				ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);
				AssertEquals("HVLV + Shipment: Cache collection count 2", 2, complianceBizO.JobEntityCacheCollection.Count);
			}
		}

		public void TestShipmentWithHVLVConsignmentOrganizationFreeTextEntityCache()
		{
			using (HVLVDataRegistry.Instance.HVLVEnablePartyScreening.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				new HVLVEnablePartyScreening() { EnableHVLVPartyScreening = true }))
			{
				var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
				shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
				shipment.JS_TransportMode = TransportModes.Sea;

				var consignmentHeader = Factory.New<HVLVConsignmentHeader>();
				consignmentHeader.HCH_JS_Shipment = shipment.PK;

				var consignment = consignmentHeader.Consignments.AddNew();
				consignment.HVC_WeightUQ = Weight.Grams;
				consignment.HVC_ConsigneeName = "ORG FREE TEXT";

				var item = consignment.Items.AddNew();
				item.HVI_ActualWeight = 1000;

				var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRisk);

				AssertEquals("Precondition Memory entity cache count 0", 0, complianceBizO.JobEntityCacheCollection.Count);

				ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);

				var cache = (ComplianceJobEntityCache)complianceBizO.JobEntityCacheCollection.FirstOrDefault();

				CombineAssertions("The entity cache should in the collection", () =>
				{
					AssertEquals("HVLV Consignment: Cache collection count 1 ", 1, complianceBizO.JobEntityCacheCollection.Count);
					AssertEquals("Cache table code", "HVC", cache.CJE_EntityTableCode);
					AssertEquals("Cache HVLC consigment PK", consignment.PK, cache.CJE_EntityID);
				});
			}
		}

		public void TestShipmentBookingWithQuoteEntityCache()
		{
			var (orgConsignor, orgConsignee) = Factory.CreateNewOrgConsignorAndConsignee();
			var bookingClient = Factory.NewWithValidTestData<OrgHeader>();
			var quotedBooking = QuotedBooking.New(QuoteBookingType.BookingWithQuote, Factory);
			quotedBooking.ClientPK = bookingClient.PK;

			var shipment = quotedBooking.Booking;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRisk = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRisk.COR_ParentID = shipment.JS_TH_OneTimeQuote;
			complianceRisk.COR_ParentTableCode = RatingHeaderSchema.Constants.Prefix;

			CombineAssertions("Precondition", () =>
			{
				Assert("IsBooking", shipment.JS_IsBooking);
				Assert("IsForwardRegistered", !shipment.JS_IsForwardRegistered);
			});

			Factory.Save();

			shipment.ConsigneePK = orgConsignee.PK;
			shipment.ConsignorPK = orgConsignor.PK;

			var complianceBizO = new ComplianceRiskBusinessObject(quotedBooking, complianceRisk);
			ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);

			Factory.Save();

			var entityCache = Factory.LoadComplianceJobEntityCache(complianceRisk);

			CombineAssertions("The entity cache is saved in the database", () =>
			{
				AssertEquals("Cache collection count 3 ", 3, complianceBizO.JobEntityCacheCollection.Count);
				Assert($"Client: {bookingClient.PK}", Factory.IsEntityCacheInDatabase(entityCache, bookingClient));
				Assert($"Consignor: {orgConsignor.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgConsignor));
				Assert($"Consignee: {orgConsignee.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgConsignee));
			});
		}

		public void TestConsolidationEntityCache()
		{
			var (orgSendingForwarder, orgReceivingForwarder) = Factory.CreateNewOrgConsignorAndConsignee();
			var (complianceRisk, consol, transport) = Factory.CreateNewComplianceRiskWithConsol();
			consol.JK_OA_SendingForwarderAddress = orgSendingForwarder.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = orgReceivingForwarder.MainAddress.PK;

			var complianceBizO = new ComplianceRiskBusinessObject(consol, complianceRisk);
			ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);

			Factory.Save();

			var entityCache = Factory.LoadComplianceJobEntityCache(complianceRisk);

			CombineAssertions("The entity cache is saved in the database", () =>
			{
				AssertEquals("Cache collection count 2", 2, complianceBizO.JobEntityCacheCollection.Count);
				AssertEquals("Entity cache count 2", 2, entityCache.Length);
				Assert($"Sending Forwarder: {orgSendingForwarder.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgSendingForwarder));
				Assert($"Receiving Forwarder: {orgReceivingForwarder.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgReceivingForwarder));
			});
		}

		public void TestShipmentJobEndDateIsExpiredDeleteEntityCache()
		{
			var (orgConsignor, orgConsignee) = Factory.CreateNewOrgConsignorAndConsignee();
			var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
			shipment.ConsigneePK = orgConsignee.PK;
			shipment.ConsignorPK = orgConsignor.PK;

			var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRisk);

			AssertEquals("Precondition Memory entity cache count 0", 0, complianceBizO.JobEntityCacheCollection.Count);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);

			Factory.Save();

			var entityCache = (new BusinessObjectFactory()).LoadComplianceJobEntityCache(complianceRisk);

			CombineAssertions("Cache should exist in database", () =>
			{
				Assert($"Consignee: {orgConsignee.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgConsignee));
				Assert($"Consignor: {orgConsignor.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgConsignor));
			});

			shipment.JS_E_DEP = ZDateTime.Now.AddDays(-20);
			shipment.JS_E_ARV = ZDateTime.Empty;

			ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);

			Factory.Save();

			entityCache = (new BusinessObjectFactory()).LoadComplianceJobEntityCache(complianceRisk);

			CombineAssertions("Shipment is no longer active", () =>
			{
				Assert($"Consignee: {orgConsignee.PK}", !Factory.IsEntityCacheInDatabase(entityCache, orgConsignee));
				Assert($"Consignor: {orgConsignor.PK}", !Factory.IsEntityCacheInDatabase(entityCache, orgConsignor));
			});
		}

		public void TestShipmentJobEndDateIsReactivatedCreateEntityCache()
		{
			var (orgConsignor, orgConsignee) = Factory.CreateNewOrgConsignorAndConsignee();
			var (complianceRisk, shipment) = Factory.CreateNewComplianceRiskWithShipment();
			shipment.ConsigneePK = orgConsignee.PK;
			shipment.ConsignorPK = orgConsignor.PK;
			shipment.JS_E_DEP = ZDateTime.Now.AddDays(-30);
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(-15);

			var complianceBizO = new ComplianceRiskBusinessObject(shipment, complianceRisk);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);

			Factory.Save();

			var entityCache = (new BusinessObjectFactory()).LoadComplianceJobEntityCache(complianceRisk);

			CombineAssertions("Shipment is no longer active", () =>
			{
				Assert($"Consignee: {orgConsignee.PK}", !Factory.IsEntityCacheInDatabase(entityCache, orgConsignee));
				Assert($"Consignor: {orgConsignor.PK}", !Factory.IsEntityCacheInDatabase(entityCache, orgConsignor));
			});

			shipment.JS_E_DEP = ZDateTime.Now.AddDays(+7);
			shipment.JS_E_ARV = ZDateTime.Now.AddDays(+14);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceBizO);

			Factory.Save();

			entityCache = (new BusinessObjectFactory()).LoadComplianceJobEntityCache(complianceRisk);

			CombineAssertions("Shipment is reactivated", () =>
			{
				Assert($"Consignee: {orgConsignee.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgConsignee));
				Assert($"Consignor: {orgConsignor.PK}", Factory.IsEntityCacheInDatabase(entityCache, orgConsignor));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment
				.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetJobEntitiesCaching(true));
		}

		protected override void TearDown()
		{
			OrganisationsDataRegistry.Instance.ComplianceWiseFeatureDevelopment
				.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ComplianceWiseRegistryHelper.GetJobEntitiesCaching(false));
			base.TearDown();
		}
	}
}
