using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Business.Messaging;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceRiskStatusSynchronizerTest : TestCaseWithFactory
	{
		#region Synchronize

		public void TestIsJobInternational()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USORD";
			AssertEquals("Job International", true, (shipment as IComplianceJobDirectionProvider).IsInternational);

			shipment.JS_RL_NKDestination = "AUMEL";
			AssertEquals("Job Domestic", false, (shipment as IComplianceJobDirectionProvider).IsInternational);

			var dummyBizO = Factory.New<DummyBaseBusinessObject>();
			AssertEquals(false, dummyBizO is IComplianceJobDirectionProvider);

			AssertJob("AUSYD", "USORD", Enterprise.Core.Constants.ShipmentTypes.HighVolumeLowValue, "Explicit: ShipmentTypes.HighVolumeLowValue", expectedResult: false);
			AssertJob("AUSYD", "AUSYD", Enterprise.Core.Constants.ShipmentTypes.HighVolumeLowValueMaster, "Explicit: ShipmentTypes.HighVolumeLowValueMaster", expectedResult: false);

			void AssertJob(string origin, string destination, string type, string message, bool expectedResult)
			{
				shipment.JS_RL_NKOrigin = origin;
				shipment.JS_RL_NKDestination = destination;
				shipment.JS_ShipmentType = type;
				AssertEquals(message, expectedResult, shipment.IsCommodityRiskAssessableWithoutScreeningEnabledCheck());
			}
		}

		public void TestSynchronize_WhenShipmentTypeHighVolumeLowValue()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USORD";
			shipment.JS_ShipmentType = Enterprise.Core.Constants.ShipmentTypes.HighVolumeLowValue;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			Factory.Save();

			CombineAssertions("Precondition", () =>
			{
				AssertEquals("Job international", false, shipment.IsCommodityRiskAssessableWithoutScreeningEnabledCheck());
				AssertEquals("Overall risk status", ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
				AssertEquals("Parties risk status", ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_PartyRisk);
				AssertEquals("Locations risk status", ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_LocationRisk);
				AssertEquals("Commodities risk status", ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
			});

			Db.Connection.ExecuteNonQuery($@"
UPDATE dbo.OrgHeader SET OH_ScreeningStatus = 'CLR' WHERE OH_PK = '{consignor.PK}'
UPDATE dbo.OrgHeader SET OH_ScreeningStatus = 'CLR' WHERE OH_PK = '{consignee.PK}'");

			_ = ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			CombineAssertions(() =>
			{
				AssertEquals("Overall risk status", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
				AssertEquals("Parties risk status", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_PartyRisk);
				AssertEquals("Locations risk status", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_LocationRisk);
				AssertEquals("Commodities risk status", ComplianceRiskStatusCodeList.Codes.NotApplicable, complianceRiskStatus.COR_CommodityRisk);
			});
		}

		public void TestSynchronize_WhenPartiesHaveChanged_ResetOverallRisk()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			Factory.Save();

			var consignee2 = Factory.NewWithValidTestData<OrgHeader>();
			consignee2.OH_IsConsignor = true;
			consignee2.OH_Code = "Consignee2";

			shipment.ConsigneePK = consignee2.PK;

			CombineAssertions("Should reset party and overall risk status:", () =>
			{
				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_PartyRisk);
			});
		}

		public void TestSynchronize_WhenPartiesDeleted_KeepOverallRiskOverrideClear()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			Factory.Save();

			shipment.ConsigneePK = ZGuid.Empty;

			CombineAssertions("Should reset party but keep overall risk status:", () =>
			{
				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_PartyRisk);
			});
		}

		public void TestSynchronize_WhenLocationsHaveChangedWithSanctionedCountry_ResetOverallRisk()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";
			consignor.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consignor.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUMEL";
			consignee.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Iran));
			country.RN_IsSanctioned = true;

			Factory.Save();

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "IRABD";

			CombineAssertions("Should reset location and overall risk status:", () =>
			{
				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_LocationRisk);
			});
		}

		public void TestSynchronize_WhenLocationsDeleted_KeepOverallRiskOverrideClear()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";
			consignor.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consignor.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "USORD";
			consignee.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "IRABD";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Clear;

			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Iran));
			country.RN_IsSanctioned = true;

			Factory.Save();

			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consignee.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			CombineAssertions("Should reset location but keep overall risk status:", () =>
			{
				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_LocationRisk);
			});
		}

		public void TestSynchronize_WhenCommodityHaveChanged_ResetOverallRisk()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);

			var commodity = complianceRiskStatus.PlugInParent.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.Conditions = "Some conditions applied";
			commodity.CCD_HarmonizedCode = "369852";

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;

			Factory.Save();

			CombineAssertions("Should reset commodity and overall risk status:", () =>
			{
				commodity.CCD_HarmonizedCode = "469852";

				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
			});
		}

		public void TestSynchronize_WhenCommodityDeleted_KeepOverallRiskOverrideClear()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);

			var commodity = complianceRiskStatus.PlugInParent.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.Conditions = "Some conditions applied";
			commodity.CCD_HarmonizedCode = "369852";

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;

			Factory.Save();

			complianceRiskStatus.CommodityDetailCollection.Remove(commodity);

			CombineAssertions("Should reset commodity but keep overall risk status:", () =>
			{
				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Incomplete, complianceRiskStatus.COR_CommodityRisk);
			});
		}

		public void TestMasterShipmentParentJobResetWhenSubShipmentUpdateOrAddCommodity()
		{
			var masterShipment = Factory.New<ForwardingShipment>();
			masterShipment.JS_ShipmentType = "CLD";

			var masterShipmentComplianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			masterShipmentComplianceRiskStatus.COR_ParentID = masterShipment.PK;
			masterShipmentComplianceRiskStatus.COR_ParentTableCode = masterShipment.TablePrefix;

			var subShipmentComplianceRiskStatus = CreateTestSubShipment(masterShipment);

			var commodity = subShipmentComplianceRiskStatus.PlugInParent.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.Conditions = "Some conditions applied";
			commodity.CCD_HarmonizedCode = "123456";

			ComplianceRiskStatusSynchronizer.Synchronize(subShipmentComplianceRiskStatus.PlugInParent);
			masterShipmentComplianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, masterShipmentComplianceRiskStatus.COR_OverallRisk);

			var commodity1 = subShipmentComplianceRiskStatus.PlugInParent.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.Conditions = "Some conditions applied";
			commodity1.CCD_HarmonizedCode = "654321";

			ComplianceRiskStatusSynchronizer.Synchronize(subShipmentComplianceRiskStatus.PlugInParent);
			AssertNotEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, masterShipmentComplianceRiskStatus.COR_OverallRisk);

			masterShipmentComplianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, masterShipmentComplianceRiskStatus.COR_OverallRisk);

			commodity1.CCD_HarmonizedCode = "321321";
			ComplianceRiskStatusSynchronizer.Synchronize(subShipmentComplianceRiskStatus.PlugInParent);
			AssertNotEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, masterShipmentComplianceRiskStatus.COR_OverallRisk);
		}

		public void TestConsolComplianceRiskStatusResetWhenSubShipmentUpdateOrAddCommodity()
		{
			var (consolidation, complianceRiskStatus) = CreateTestConsolidation();
			var shipmentStatus = CreateTestConsolidationShipment(consolidation);

			var commodity = shipmentStatus.PlugInParent.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.Conditions = "Some conditions applied";
			commodity.CCD_HarmonizedCode = "123456";

			ComplianceRiskStatusSynchronizer.Synchronize(shipmentStatus.PlugInParent);
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);

			var commodity1 = shipmentStatus.PlugInParent.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.Conditions = "Some conditions applied";
			commodity1.CCD_HarmonizedCode = "654321";

			ComplianceRiskStatusSynchronizer.Synchronize(shipmentStatus.PlugInParent);
			AssertNotEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);

			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);

			commodity1.CCD_HarmonizedCode = "321321";
			ComplianceRiskStatusSynchronizer.Synchronize(shipmentStatus.PlugInParent);
			AssertNotEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestMultipleParentComplianceRiskStatusResetWhenSubShipmentUpdateOrAddCommodity()
		{
			var (consol, consolComplianceRiskStatus) = CreateTestConsolidation();
			var (masterShipment, masterShipmentComplianceRiskStatus) = CreateTestConsolidationShipmentAndCPW(consol);

			var subShipmentComplianceRiskStatus = CreateTestSubShipment(masterShipment);

			var commodity = subShipmentComplianceRiskStatus.PlugInParent.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.Conditions = "Some conditions applied";
			commodity.CCD_HarmonizedCode = "123456";

			ComplianceRiskStatusSynchronizer.Synchronize(subShipmentComplianceRiskStatus.PlugInParent);
			consolComplianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			masterShipmentComplianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, consolComplianceRiskStatus.COR_OverallRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, masterShipmentComplianceRiskStatus.COR_OverallRisk);

			var commodity1 = subShipmentComplianceRiskStatus.PlugInParent.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.Conditions = "Some conditions applied";
			commodity1.CCD_HarmonizedCode = "654321";

			ComplianceRiskStatusSynchronizer.Synchronize(subShipmentComplianceRiskStatus.PlugInParent);
			AssertNotEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, consolComplianceRiskStatus.COR_OverallRisk);
			AssertNotEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, masterShipmentComplianceRiskStatus.COR_OverallRisk);

			consolComplianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			masterShipmentComplianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, consolComplianceRiskStatus.COR_OverallRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, masterShipmentComplianceRiskStatus.COR_OverallRisk);

			commodity1.CCD_HarmonizedCode = "321321";
			ComplianceRiskStatusSynchronizer.Synchronize(subShipmentComplianceRiskStatus.PlugInParent);
			AssertNotEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, consolComplianceRiskStatus.COR_OverallRisk);
			AssertNotEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, masterShipmentComplianceRiskStatus.COR_OverallRisk);
		}

		public void TestSynchronize_TakeSnapShot()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";

			var countryAU = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia));
			countryAU.RN_IsSanctioned = false;

			var countryIR = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Iran));
			countryIR.RN_IsSanctioned = true;

			shipment.ConsigneePK = consignee.PK;
			shipment.ConsignorPK = consignor.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "IRABD";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			AssertNull(complianceRiskStatus.PlugInParent.SnapShot);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertNotNull(complianceRiskStatus.PlugInParent.SnapShot);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				(consignor.PK, consignor.OH_ScreeningStatus),
				(consignee.PK, consignee.OH_ScreeningStatus)
			}, complianceRiskStatus.PlugInParent.SnapShot.Parties);
			AssertContainsExactElementsInAnyOrder(new[]
			{
				(countryAU.PK, false),
				(countryIR.PK, true)
			}, complianceRiskStatus.PlugInParent.SnapShot.Locations);
		}

		public void TestSynchronizeCommodityRiskStatusChange()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USORD";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "369852";
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;

			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
			AssertNull(complianceRiskStatus.PlugInParent.SnapShot);

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);
			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertNotNull(complianceRiskStatus.PlugInParent.SnapShot);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestSynchronizeJobEndDateShouldHasValue()
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USORD";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;

			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertDateTimeWithinOneSecond("COR_JobEndDate should has correct value", ZDateTime.Now.AddMonths(OrganisationsDataRegistry.Instance.JobUpdatePeriod.Value).UtcToDateTimeOffset().ToDateTime(), complianceRiskStatus.COR_JobEndDate.ToDateTime());
		}

		public void TestSynchronizeCommodityRiskStatusChange_WithBorderWiseAPIIntegration()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USORD";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "369852";
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);
			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);

			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Clear;
			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_CommodityRisk);

			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_CommodityRisk);

			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_CommodityRisk);

			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk;
			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_CommodityRisk);

			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PossibleRisk;
			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.PossibleRisk, complianceRiskStatus.COR_CommodityRisk);
		}

		public void TestSynchronize_UseNewStatus()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Iran));
			country.RN_IsSanctioned = false;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "Consignee";
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "IRABD";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity = complianceRiskStatus.PlugInParent.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "369852";
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Clear;

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_PartyRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_LocationRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_CommodityRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

			country.RN_IsSanctioned = true;
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.HighRisk;
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_PartyRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_LocationRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_CommodityRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestSynchronize_InternationalChangeToDomestic()
		{
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates));
			country.RN_IsSanctioned = false;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignee = true;
			consignee.OH_Code = "Consignee";
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "US32M";
			consignee.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = consignee.PK;
			shipment.JS_RL_NKOrigin = "AU2CO";
			shipment.JS_RL_NKDestination = "US32M";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_PartyRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_LocationRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);

			var complianceJobDirection = (shipment as IComplianceJobDirectionProvider);
			AssertEquals(true, complianceJobDirection.IsInternational);
			shipment.JS_RL_NKDestination = "AU32S";
			AssertEquals(false, complianceJobDirection.IsInternational);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_PartyRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_LocationRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.NotApplicable, complianceRiskStatus.COR_CommodityRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestSynchronize_WithCommodityScreeningFromEnableToDisable()
		{
			AssertSynchronize_WithCommodityScreeningFromEnableToDisable(false, ComplianceRiskStatusCodeList.Codes.Unknown, ComplianceRiskStatusCodeList.Codes.NotAssessed);
			AssertSynchronize_WithCommodityScreeningFromEnableToDisable(true, ComplianceRiskStatusCodeList.Codes.Incomplete, ComplianceRiskStatusCodeList.Codes.Incomplete);

			void AssertSynchronize_WithCommodityScreeningFromEnableToDisable(bool isAssessmentInitialized, string enableFeatureCommodityRisk, string disableFeatureCommodityRisk)
			{
				var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
				shipment.JS_RL_NKOrigin = "AU2CO";
				shipment.JS_RL_NKDestination = "US32M";

				var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
				complianceRiskStatus.COR_ParentID = shipment.PK;
				complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
				complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

				if (isAssessmentInitialized)
				{
					StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);
				}

				using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(true))
				{
					ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
					AssertEquals(enableFeatureCommodityRisk, complianceRiskStatus.COR_CommodityRisk);
				}

				using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(false))
				{
					ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
					AssertEquals(disableFeatureCommodityRisk, complianceRiskStatus.COR_CommodityRisk);
				}
			}
		}

		public void TestConsolSynchronize_WithCommodityScreeningDisable()
		{
			var consolBusinessObject = (BusinessObject)Factory.New<IForwardingConsol>();
			var consol = (ForwardingConsol)consolBusinessObject;
			consol.JK_RL_NKLoadPort = "AU2CO";
			consol.JK_RL_NKDischargePort = "US32M";
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = consol.PK;
			complianceRiskStatus.COR_ParentTableCode = consol.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(consol);

			var shipment = (BusinessObject)Factory.New<IForwardingShipment>();
			var shipmentRiskStatus = Factory.New<ComplianceRiskStatus>();
			shipmentRiskStatus.COR_ParentID = shipment.PK;
			shipmentRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			consol.Shipments.Add(shipment);

			using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
			using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(false))
			{
				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotAssessed, complianceRiskStatus.COR_CommodityRisk);

				shipmentRiskStatus.InitializeAssessmentWorkflow();

				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Incomplete, complianceRiskStatus.COR_CommodityRisk);

				var commodity = shipmentRiskStatus.CommodityDetailCollection.AddNew();
				commodity.CCD_HarmonizedCode = "369852";
				commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Clear;
				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_CommodityRisk);
			}
		}

		#endregion

		#region Synchronize And Save If Needed

		public void TestSynchronizeAndSaveIfNeeded_NoSaveConcurrencyExceptionThrown()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;

			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			AssertEquals("Precondition:", false, complianceRiskStatus.IsInDatabase);
			AssertNoExceptionThrown("Save concurrency exception not thrown", () =>
			{
				ComplianceRiskStatusSynchronizer.SynchronizeAndSaveIfNeeded(complianceRiskStatus.PlugInParent, synchronizeEnforceOnFormLoad: false);
				AssertEquals(true, complianceRiskStatus.IsInDatabase);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_PartyRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotApplicable, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_LocationRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			});
		}

		public void TestSynchronizeAndSaveIfNeeded_ConcurrencyException_NoErrorReport()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_IsConsignor = true;
			consignee.OH_Code = "Consignee";
			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();

			BusinessObjectFactory.SetOnFactorySaveHookForTest(factory => throw new Exception("Simulated concurrency exception"));

			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var result = ComplianceRiskStatusSynchronizer.SynchronizeAndSaveIfNeeded(complianceRiskStatus.PlugInParent, synchronizeEnforceOnFormLoad: true);

			AssertNotNull(result);
			AssertNoExceptionThrown("ErrorReporter should not be called", () =>
			{
				AssertEquals(0, ErrorReporter.TotalErrorCount);
			});
		}

		public void TestSynchronizeAndSaveIfNeeded_NoSaveConcurrencyExceptionWhenDataChangedInDB_WithParties()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsConsignor = true;
			orgHeader.OH_Code = "Consignor";

			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia));
			country.RN_IsSanctioned = false;

			shipment.ConsignorPK = orgHeader.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);

			var tariff = ComplianceRiskTariffTestDataHelper.CreateNewOrLoadTariff(Factory, "123456");
			var commodity = complianceRiskStatus.PlugInParent.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = tariff.ZZ1_TariffCode;

			AssertNull(complianceRiskStatus.PlugInParent.SnapShot);

			Factory.Save();

			Db.Connection.ExecuteNonQuery($@"
UPDATE dbo.RefCountry set RN_IsSanctioned = 0 where RN_PK = '{country.PK}'
UPDATE dbo.OrgHeader set OH_ScreeningStatus = 'CLR' where OH_PK = '{orgHeader.PK}'");

			AssertNoExceptionThrown("Reload safe should refresh compliance risk and parties status", () =>
			{
				ComplianceRiskStatusSynchronizer.SynchronizeAndSaveIfNeeded(complianceRiskStatus.PlugInParent, synchronizeEnforceOnFormLoad: false);
				AssertNotNull(complianceRiskStatus.PlugInParent.SnapShot);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
			});
		}

		public void TestSynchronizeAndSaveIfNeeded_NoSaveConcurrencyExceptionWhenDataChangedInDB_WithPartiesComplianceRisk()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsConsignor = true;
			orgHeader.OH_Code = "Consignor";

			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia));
			country.RN_IsSanctioned = false;

			shipment.ConsignorPK = orgHeader.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);

			var tariff = ComplianceRiskTariffTestDataHelper.CreateTariffWithConditions(Factory, "486219", "Dummy Conditions");
			var commodity = complianceRiskStatus.PlugInParent.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = tariff.ZZ1_TariffCode;

			Factory.Save();

			CombineAssertions("Pre-Conditions:", () =>
			{
				AssertEquals("COR_CommodityRisk is Unknown", ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals("CCD_RiskStatus is Potential Risk", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity.CCD_RiskStatus);
			});

			Db.Connection.ExecuteNonQuery($@"
UPDATE dbo.OrgHeader SET OH_ScreeningStatus = 'CLR' WHERE OH_PK = '{orgHeader.PK}'
UPDATE dbo.ComplianceRiskStatus SET COR_CommodityRisk = 'CLR' WHERE COR_PK = '{complianceRiskStatus.PK}'");

			CombineAssertions("Reload safe should refresh compliance risk status", () =>
			{
				var result = false;
				AssertNoExceptionThrown(() =>
				{
					result = ComplianceRiskStatusSynchronizer.SynchronizeAndSaveIfNeeded(complianceRiskStatus.PlugInParent, synchronizeEnforceOnFormLoad: false);
				});
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
				AssertEquals(true, result);
			});
		}

		public void TestSynchronizeForConvertShipmentFirstLoadIfNeeded()
		{
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			consignor.OH_IsConsignor = true;
			consignor.OH_Code = "Consignor";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.ConsigneePK = new ZGuid();
			shipment.ConsignorPK = consignor.PK;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_LocationRisk = ComplianceRiskStatusCodeList.Codes.Blocked;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			complianceRiskStatus.CopyFromBooking = true;
			complianceRiskStatus.CopyFromBookingFirstLoaded = true;

			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
			AssertEquals("Precondition:", false, complianceRiskStatus.IsInDatabase);

			CombineAssertions(() =>
			{
				ComplianceRiskStatusSynchronizer.SynchronizeForConvertShipmentFirstLoadIfNeeded(complianceRiskStatus.PlugInParent, synchronizeEnforceOnFormLoad: true);
				AssertEquals(false, complianceRiskStatus.IsInDatabase);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatus.COR_PartyRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_LocationRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
			});
		}

		public void TestSynchoniseAndSave_WhenChildShipmentRemovesCommodity()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = "ASM";
			shipment.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USORD";
			var complianceRisk = new ComplianceRiskPlugInBusinessObject(shipment);
			complianceRisk.ComplianceRiskStatus.COR_CommodityRisk = "PRS";

			Factory.Save();

			AssertEquals(ComplianceRiskStatusCodeList.Codes.PossibleRisk, complianceRisk.ComplianceRiskStatus.COR_CommodityRisk);

			var childShipment = shipment.CoLoadShipments.AddNew();
			var childComplianceRisk = new ComplianceRiskPlugInBusinessObject(childShipment);

			var commodityDetail = childComplianceRisk.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			commodityDetail.CCD_HarmonizedCode = "123456";
			commodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			childComplianceRisk.ComplianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			childComplianceRisk.ComplianceRiskStatus.Factory.Save();
			AssertEquals("PSK", childComplianceRisk.ComplianceRiskStatus.COR_CommodityRisk);

			ComplianceRiskStatusSynchronizer.SynchronizeOnUserControlShownAndSaveIfNeeded(complianceRisk);

			AssertNotEquals(ComplianceRiskStatusCodeList.Codes.PossibleRisk, complianceRisk.ComplianceRiskStatus.COR_CommodityRisk);
			AssertEquals(false, complianceRisk.ComplianceRiskStatus.HasChanges);
		}

		#endregion

		public void TestSynchronizeOverallRisk_WhenJobIsDomestic()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUSYD";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			Factory.Save();

			CombineAssertions("Precondition: Compliance Risk Status", () =>
			{
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);

				AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatus.COR_CommodityRisk);
			});

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertEquals("Overall Risk should be CLR", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);
			AssertEquals("Commodity Risk should be NAP", ComplianceRiskStatusCodeList.Codes.NotApplicable, complianceRiskStatus.COR_CommodityRisk);
		}

		public void TestCommodityRisk_WhenSynchronizeWithRuleMatchNoHarmonizedCode()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Empty;
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_JobNum = "ABC123";
			job.JH_Status = JobHeaderStatus.Working.Code;

			var complianceRule = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule.CRU_Origin = "AU";
			complianceRule.CRU_Destination = "US";
			complianceRule.CRU_HarmonizedCode = "";
			complianceRule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "12345";

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "23456";

			var commodity3 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity3.CCD_HarmonizedCode = "34567";

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);

				StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);
				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

				Assert(complianceRiskStatus.IsAssessmentInitialized);
				Assert(complianceRiskStatus.CommodityDetailCollection.All(x => ((ComplianceCommodityDetail)x).CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.Blocked));
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestCommodityRisk_WhenSynchronizeWithRuleMatchHarmonizedCode()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Empty;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_JobNum = "ABC123";
			job.JH_Status = JobHeaderStatus.Working.Code;

			var complianceRule = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule.CRU_Origin = "AU";
			complianceRule.CRU_Destination = "US";
			complianceRule.CRU_HarmonizedCode = "12345";
			complianceRule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "12345";

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "23456";

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);

				StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);
				ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

				Assert(complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity1.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity2.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestCommodityRisk_WhenSynchronizeOnUserControlShown()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = ZDateTime.Now;
			shipment.JS_E_ARV = ZDateTime.Empty;

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_JobNum = "ABC123";
			job.JH_Status = JobHeaderStatus.Working.Code;

			var complianceRule = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule.CRU_Origin = "AU";
			complianceRule.CRU_Destination = "US";
			complianceRule.CRU_HarmonizedCode = "12345";
			complianceRule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "12345";
			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "23456";

			Factory.Save();

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				ComplianceRiskStatusSynchronizer.SynchronizeOnUserControlShownAndSaveIfNeeded(complianceRiskStatus.PlugInParent);
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity2.CCD_RiskStatus);
			}
		}

		public void TestCraEdiMessage_ShouldPublishEdiMessageWhenAnyCommodityNotChecked()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "12345";
			commodity.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.NotChecked;

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CPWRequestMessage)
				.AddToFilter(EDIMessageSchema.EM_MessageType, ComplianceRiskAssessmentMessagePublisher.MaterialChange)
				.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.ComplianceRiskAssessment)
				.AddToFilter(EDIMessageSchema.EM_LinkTable, shipment.TableName)
				.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, shipment.PK)
				.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Queued);

			query.TableIndexHints.Add(new TableIndexHint("NR_RX__EM_ApplicationCode_EM_ApplicationReference"));
			var exists = Factory.Exists(typeof(ComplianceRiskAssessmentEdiMessage), query);
			Assert("Commodity Material Change EDI Message", exists);
		}

		public void TestCommodityMaterialChanges_ShouldNotPublishEdiMessageWhenNoCommodities()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.CPWRequestMessage)
				.AddToFilter(EDIMessageSchema.EM_MessageType, ComplianceRiskAssessmentMessagePublisher.MaterialChange)
				.AddToFilter(EDIMessageSchema.EM_MessageSubType, EDIMessageSubTypeList.Codes.ComplianceRiskAssessment)
				.AddToFilter(EDIMessageSchema.EM_LinkTable, shipment.TableName)
				.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, shipment.PK)
				.AddToFilter(EDIMessageSchema.EM_Status, EDIMessageStatusList.Codes.Queued);

			query.TableIndexHints.Add(new TableIndexHint("NR_RX__EM_ApplicationCode_EM_ApplicationReference"));
			var exists = Factory.Exists(typeof(ComplianceRiskAssessmentEdiMessage), query);
			Assert("Commodity Material Change Do Not Create EDI Message", !exists);
		}

		#region Job or Commodity Material Change

		public void TestJobMaterialChanges_ShouldResetAllCommodityRiskStatus()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "12345";
			commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "456789";
			commodity2.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Clear;

			complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot = ComplianceCheckRequestModelBuilder.GetRequestModel(complianceRiskStatus.PlugInParent, complianceRiskStatus.PlugInParent.ComplianceCommodityRiskStatusProvider.AssessmentPointPairInfo).RequestModel;

			shipment.JS_RL_NKOrigin = "NZAKL";
			AssertPointPairs("Material change Origin: AUSYD to NZAKL");
			Factory.Save();

			shipment.JS_RL_NKDestination = "AUSYD";
			complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().ToArray().ForEach(c => c.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked);
			AssertPointPairs("Material change Destination: USLAX to AUSYD");

			void AssertPointPairs(string message)
			{
				CombineAssertions(message, () =>
				{
					ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

					AssertEquals(2, complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Count(c => c.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.NotChecked));
					AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
				});
			}
		}

		public void TestCommodityMaterialChanges_ShouldResetCommodityRiskStatus()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			StmComplianceEventHelperTest.CreateAssessmentEvent(shipment, ComplianceEventList.Codes.AssessmentInitialized);

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "12345";
			commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "23456";
			commodity2.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;

			complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot = ComplianceCheckRequestModelBuilder.GetRequestModel(complianceRiskStatus.PlugInParent, complianceRiskStatus.PlugInParent.ComplianceCommodityRiskStatusProvider.AssessmentPointPairInfo).RequestModel;

			complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot.Commodities = complianceRiskStatus.PlugInParent.ComplianceMaterialChangesSnapshot.Commodities
				.Select(u => new ComplianceCheckRequestCommodityModel { Origin = new[] { "US" }, HsCode = u.HsCode, HsCodeDescription = u.HsCodeDescription })
				.ToArray();

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertEquals(2, complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>().Count(c => c.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.NotChecked));
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Held, complianceRiskStatus.COR_OverallRisk);
		}

		#endregion

		#region AddCommodityAdditionAndDeletionLogIfNeeded

		public void TestAddLogsForHistoryAndBilling()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = "JS";
			complianceRiskStatus.COR_OverallRisk = "CLR";
			complianceRiskStatus.COR_PartyRisk = "CLR";
			complianceRiskStatus.COR_LocationRisk = "CLR";
			complianceRiskStatus.COR_CommodityRisk = "CLR";

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "123";

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "456";
			commodity2.CCD_SystemCreateTimeUtc = new ZDateTime(2024, 9, 6, 1, 2, 3);

			var commodity3 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity3.CCD_HarmonizedCode = "789";
			Factory.Save();

			ComplianceRiskStatusWorkflowInitializer.InitializeAssessmentWorkflow(complianceRiskStatus);

			Assert(complianceRiskStatus.IsAssessmentInitialized);
			commodity2.Delete();
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			var additionLog = complianceRiskStatus.GetEventLogs().Single(s => s.SCE_EventType == "CVO");
			AssertEquals("CPC", additionLog.SCE_EventSubType);
			AssertEquals(3, additionLog.SCE_ItemsCount);

			var deletionLog = complianceRiskStatus.GetEventLogs().Single(s => s.SCE_EventType == "CCI");
			AssertEquals("CLD", deletionLog.SCE_EventSubType);
			AssertEquals(1, deletionLog.SCE_ItemsCount);
			AssertEquals("{\"Commodities\":[{\"Code\":\"456\",\"Conditions\":\"\",\"HsCodeDescription\":\"\",\"RiskStatus\":\"NCH\",\"NomenclatureCondition\":\"\",\"SpecificCondition\":\"\",\"Source\":\"\",\"CommoditySource\":\"Compliance\",\"Notes\":\"\",\"GoodsDescription\":\"\",\"OriginOfGoods\":\"\",\"IsAssessmentInitiated\":true,\"DateAddedUtc\":\"2024-09-06T01:02:03\"}]}", deletionLog.SCE_Snapshot);
		}

		#endregion

		#region Consolidation Commodity Risk Status With No Commodity

		public void TestConsolidationCommodityRiskStatusShouldBeNotApplicable_WhenNoShipmentAttached()
		{
			var (consolidation, complianceRiskStatus) = CreateTestConsolidation();

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertEquals(
				$"Commodity risk status should be {ComplianceRiskStatusCodeList.Codes.NotApplicable}",
				ComplianceRiskStatusCodeList.Codes.NotApplicable,
				consolidation.ComplianceRiskStatus.CommodityRisk);
		}

		public void TestConsolidationCommodityRiskStatusShouldBeUnknown_WhenShipmentWithoutCommodityAttached()
		{
			var (consolidation, complianceRiskStatus) = CreateTestConsolidation();
			var shipmentStatus = CreateTestConsolidationShipment(consolidation);

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertEquals(
				$"Shipment without commodities should have status {ComplianceRiskStatusCodeList.Codes.Unknown}",
				ComplianceRiskStatusCodeList.Codes.Unknown,
				shipmentStatus.COR_CommodityRisk);

			AssertEquals(
				$"Commodity risk status should be {ComplianceRiskStatusCodeList.Codes.Unknown}",
				ComplianceRiskStatusCodeList.Codes.Unknown,
				consolidation.ComplianceRiskStatus.CommodityRisk);
		}

		public void TestConsolidationCommodityRiskStatusShouldBePossibleRisk_WhenDeclinedShipmentWithoutCommodityAttached()
		{
			var (consolidation, complianceRiskStatus) = CreateTestConsolidation();
			var shipmentStatus = CreateTestConsolidationShipment(consolidation);
			shipmentStatus.DeclinedAssessmentWorkflow();

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertEquals(
				$"Declined shipment without commodities should have status {ComplianceRiskStatusCodeList.Codes.PossibleRisk}",
				ComplianceRiskStatusCodeList.Codes.PossibleRisk,
				shipmentStatus.COR_CommodityRisk);

			AssertEquals(
				$"Commodity risk status should be {ComplianceRiskStatusCodeList.Codes.PossibleRisk}",
				ComplianceRiskStatusCodeList.Codes.PossibleRisk,
				consolidation.ComplianceRiskStatus.CommodityRisk);
		}

		public void TestConsolidationCommodityRiskStatusShouldBeUnknown_WhenShipmentWithoutCommodityAndDeclinedShipmentWithoutCommodityAttached()
		{
			var (consolidation, complianceRiskStatus) = CreateTestConsolidation();
			var declinedShipmentStatus = CreateTestConsolidationShipment(consolidation, "AUSYD", "NZAKL");
			declinedShipmentStatus.DeclinedAssessmentWorkflow();
			var notDeclinedShipmentStatus = CreateTestConsolidationShipment(consolidation, "NZAKL");

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertEquals(
				$"Declined shipment without commodities should have status {ComplianceRiskStatusCodeList.Codes.PossibleRisk}",
				ComplianceRiskStatusCodeList.Codes.PossibleRisk,
				declinedShipmentStatus.COR_CommodityRisk);

			AssertEquals(
				$"Shipment without commodities should have status {ComplianceRiskStatusCodeList.Codes.Unknown}",
				ComplianceRiskStatusCodeList.Codes.Unknown,
				notDeclinedShipmentStatus.COR_CommodityRisk);

			AssertEquals(
				$"Commodity risk status should be {ComplianceRiskStatusCodeList.Codes.Unknown}",
				ComplianceRiskStatusCodeList.Codes.Unknown,
				consolidation.ComplianceRiskStatus.CommodityRisk);
		}

		public void TestConsolidationCommodityRiskStatusShouldBeIncomplete_WhenInitializedShipmentWithoutCommodityAndShipmentWithoutCommodityAttached()
		{
			var (consolidation, complianceRiskStatus) = CreateTestConsolidation();
			var notInitializedShipmentStatus = CreateTestConsolidationShipment(consolidation, "AUSYD", "NZAKL");
			var initializedShipmentStatus = CreateTestConsolidationShipment(consolidation, "NZAKL");
			initializedShipmentStatus.InitializeAssessmentWorkflow();

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertEquals(
				$"Shipment without commodities should have status {ComplianceRiskStatusCodeList.Codes.Unknown}",
				ComplianceRiskStatusCodeList.Codes.Unknown,
				notInitializedShipmentStatus.COR_CommodityRisk);

			AssertEquals(
				$"Initialized shipment without commodities should have status {ComplianceRiskStatusCodeList.Codes.Incomplete}",
				ComplianceRiskStatusCodeList.Codes.Incomplete,
				initializedShipmentStatus.COR_CommodityRisk);

			AssertEquals(
				$"Commodity risk status should be {ComplianceRiskStatusCodeList.Codes.Incomplete}",
				ComplianceRiskStatusCodeList.Codes.Incomplete,
				consolidation.ComplianceRiskStatus.CommodityRisk);
		}

		public void TestConsolidationCommodityRiskStatusShouldBeCorrectWhenDetachingShipments()
		{
			var (consolidation, complianceRiskStatus) = CreateTestConsolidation();
			var declinedShipmentStatus = CreateTestConsolidationShipment(consolidation, "AUSYD", "NZAKL");
			declinedShipmentStatus.DeclinedAssessmentWorkflow();
			var notDeclinedShipmentStatus = CreateTestConsolidationShipment(consolidation, "NZAKL");

			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertEquals(
				$"Declined shipment without commodities should have status {ComplianceRiskStatusCodeList.Codes.PossibleRisk}",
				ComplianceRiskStatusCodeList.Codes.PossibleRisk,
				declinedShipmentStatus.COR_CommodityRisk);

			AssertEquals(
				$"Shipment without commodities should have status {ComplianceRiskStatusCodeList.Codes.Unknown}",
				ComplianceRiskStatusCodeList.Codes.Unknown,
				notDeclinedShipmentStatus.COR_CommodityRisk);

			// Detach the shipment that was not declined.
			consolidation.Shipments.Remove(consolidation.Shipments[1]);
			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertEquals(
				$"Commodity risk status should be {ComplianceRiskStatusCodeList.Codes.PossibleRisk}",
				ComplianceRiskStatusCodeList.Codes.PossibleRisk,
				consolidation.ComplianceRiskStatus.CommodityRisk);

			// Detach the shipment that was declined.
			consolidation.Shipments.RemoveAll();
			ComplianceRiskStatusSynchronizer.Synchronize(complianceRiskStatus.PlugInParent);

			AssertEquals(
				$"Commodity risk status should be {ComplianceRiskStatusCodeList.Codes.NotApplicable}",
				ComplianceRiskStatusCodeList.Codes.NotApplicable,
				consolidation.ComplianceRiskStatus.CommodityRisk);
		}

		(ForwardingConsol consolidation, ComplianceRiskStatus complianceRiskStatus) CreateTestConsolidation(string loadPort = "AUSYD", string dischargePort = "USLAX")
		{
			var consolidation = (ForwardingConsol)Factory.New<IForwardingConsol>();
			consolidation.JK_RL_NKLoadPort = loadPort;
			consolidation.JK_RL_NKDischargePort = dischargePort;

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = consolidation.PK;
			complianceRiskStatus.COR_ParentTableCode = consolidation.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(consolidation);

			return (consolidation, complianceRiskStatus);
		}

		ComplianceRiskStatus CreateTestConsolidationShipment(ForwardingConsol consolidation, string origin = "AUSYD", string destination = "USLAX")
		{
			var shipment = consolidation.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;

			var shipmentComplianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			shipmentComplianceRiskStatus.COR_ParentID = shipment.PK;
			shipmentComplianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			shipmentComplianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			return shipmentComplianceRiskStatus;
		}

		(ForwardingShipment shipment, ComplianceRiskStatus complianceRiskStatus) CreateTestConsolidationShipmentAndCPW(ForwardingConsol consolidation, string origin = "AUSYD", string destination = "USLAX")
		{
			var shipment = consolidation.Shipments.AddNew();
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;

			var shipmentComplianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			shipmentComplianceRiskStatus.COR_ParentID = shipment.PK;
			shipmentComplianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			shipmentComplianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			return (shipment, shipmentComplianceRiskStatus);
		}

		ComplianceRiskStatus CreateTestSubShipment(ForwardingShipment masterShipment, string origin = "AUSYD", string destination = "USLAX")
		{
			var shipment = masterShipment.CoLoadShipments.AddNew();
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;

			var shipmentComplianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			shipmentComplianceRiskStatus.COR_ParentID = shipment.PK;
			shipmentComplianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			shipmentComplianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			return shipmentComplianceRiskStatus;
		}

		#endregion

		IDisposable setAllowComplianceCommodityRiskAssessmentToTrue;
		protected override void SetUp()
		{
			base.SetUp();
			setAllowComplianceCommodityRiskAssessmentToTrue = OrganisationsDataRegistry.Instance.AllowComplianceCommodityRiskAssessment.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
		}

		protected override void TearDown()
		{
			base.TearDown();
			setAllowComplianceCommodityRiskAssessmentToTrue.Dispose();
		}
	}
}
