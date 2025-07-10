using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ComplianceRisk.Business.Test.ComplianceCommodityDetailCollectionTest;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRiskStatusChangeLog))]
	public class ComplianceRiskStatusChangeLogTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInitFromStmComplianceEvent_WithOverrideClearLog()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S123";
			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);
			pluginBizO.RefreshData();
			Factory.Save();

			var complianceRiskStatus = pluginBizO.ComplianceRiskStatus;
			AssertEquals("Pre-Condition", ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);

			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.OverrideClear;
			Factory.Save();

			var query = new ZQuery(StmComplianceEventSchema.SCE_ParentID, shipment.PK);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventType, AutoEvents.StatusUpdatedCode);
			query.AddToFilter(StmComplianceEventSchema.SCE_EventSubType, ComplianceEventList.Codes.OverallRisk);
			query.AddToFilter(StmComplianceEventSchema.SCE_NewValue, ComplianceRiskStatusCodeList.Codes.OverrideClear);

			var overallStatusChangeLog = Factory.Load<StmComplianceEvent>(query).Single();
			var complianceRiskStatusChangeLog = new ComplianceRiskStatusChangeLog(overallStatusChangeLog);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatusChangeLog.OverallRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.OverrideClear, complianceRiskStatusChangeLog.OverallRiskDescription);
			AssertEquals(overallStatusChangeLog.SCE_EventTimeOffset.ToDateTime(), complianceRiskStatusChangeLog.EventDateTime);
			AssertEquals(Environment.Env.CurrentUser.FullName, complianceRiskStatusChangeLog.User);
			AssertEquals(0, complianceRiskStatusChangeLog.PartyRiskLogCollection.Count);
			AssertEquals(0, complianceRiskStatusChangeLog.LocationRiskLogCollection.Count);
			AssertEquals(0, complianceRiskStatusChangeLog.CommodityRiskLogCollection.Count);
			AssertEquals(ZString.Empty, complianceRiskStatusChangeLog.PartyRisk);
			AssertEquals(ZString.Empty, complianceRiskStatusChangeLog.LocationRisk);
			AssertEquals(ZString.Empty, complianceRiskStatusChangeLog.AssessmentRisk);
			AssertEquals(ZString.Empty, complianceRiskStatusChangeLog.CommodityRisk);
			AssertEquals("Not Available", complianceRiskStatusChangeLog.PartyRiskDescription);
			AssertEquals("Not Available", complianceRiskStatusChangeLog.LocationRiskDescription);
			AssertEquals("Not Available", complianceRiskStatusChangeLog.AssessmentRiskDescription);
			AssertEquals("Not Available", complianceRiskStatusChangeLog.CommodityRiskDescription);
			AssertEquals(false, complianceRiskStatusChangeLog.SnapshotExists);
			AssertEquals(string.Empty, complianceRiskStatusChangeLog.ClearedReason);
		}

		public void TestInitFromStmComplianceEvent_WithOverrideClearLogSnapshot()
		{
			#region Init Data

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S123";
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "ORG 1";

			shipment.JS_OH_ExportBroker = org1.PK;
			shipment.JS_RL_NKOrigin = "AUSYD";

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

			pluginBizO.RefreshData();

			var complianceCommodityDetail = pluginBizO.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = pluginBizO.ComplianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = Codes.WorldCustomsOrganisationWCO;
			complianceCommodityDetail.CCD_HarmonizedCode = "123";
			complianceCommodityDetail.Conditions = "CON";
			complianceCommodityDetail.Description = "DES";
			complianceCommodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			var screeningLog_Org1 = Factory.New<IStmEntityScreeningLog>();
			screeningLog_Org1.PJ_ParentID = org1.PK;
			screeningLog_Org1.PJ_ParentTableCode = org1.TablePrefix;
			screeningLog_Org1.PJ_SourceID = shipment.PK;
			screeningLog_Org1.PJ_SourceTableCode = shipment.TablePrefix;
			screeningLog_Org1.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges;

			var screeningLog_CountryAU = Factory.New<IStmEntityScreeningLog>();
			screeningLog_CountryAU.PJ_ParentID = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU").PK;
			screeningLog_CountryAU.PJ_ParentTableCode = "JE";
			screeningLog_CountryAU.PJ_SourceID = shipment.PK;
			screeningLog_CountryAU.PJ_SourceTableCode = shipment.TablePrefix;
			screeningLog_CountryAU.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.UserDecisionsRemoveSanctions;

			Factory.Save();

			#endregion

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Dummy Reason"));

			var query2 = new ZQuery(StmComplianceEventSchema.SCE_ParentID, shipment.PK);
			query2.AddToFilter(StmComplianceEventSchema.SCE_EventType, AutoEvents.StatusUpdatedCode);
			query2.AddToFilter(StmComplianceEventSchema.SCE_EventSubType, ComplianceEventList.Codes.OverallRisk);
			query2.AddToFilter(StmComplianceEventSchema.SCE_NewValue, ComplianceRiskStatusCodeList.Codes.OverrideClear);
			var overrideClearLog = Factory.Load<StmComplianceEvent>(query2).Single();
			var complianceRiskStatusChangeLog = new ComplianceRiskStatusChangeLog(overrideClearLog);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatusChangeLog.OverallRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.OverrideClear, complianceRiskStatusChangeLog.OverallRiskDescription);
			AssertEquals(overrideClearLog.SCE_EventTimeOffset.ToDateTime(), complianceRiskStatusChangeLog.EventDateTime);
			AssertEquals(Environment.Env.CurrentUser.FullName, complianceRiskStatusChangeLog.User);
			AssertEquals(1, complianceRiskStatusChangeLog.CommodityRiskLogCollection.Count);
			AssertEquals(screeningLog_Org1.PK, complianceRiskStatusChangeLog.PartyRiskLogCollection.Cast<CompliancePartyRiskLog>().Single().EntityScreeningLog.PK);
			AssertEquals("Not Screened", complianceRiskStatusChangeLog.PartyRiskLogCollection.Cast<CompliancePartyRiskLog>().Single().StatusDescription);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatusChangeLog.PartyRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatusChangeLog.LocationRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatusChangeLog.CommodityRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.HighRisk, complianceRiskStatusChangeLog.PartyRiskDescription);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.Clear, complianceRiskStatusChangeLog.LocationRiskDescription);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.HighRisk, complianceRiskStatusChangeLog.CommodityRiskDescription);
			AssertEquals(true, complianceRiskStatusChangeLog.SnapshotExists);
			AssertEquals("OTH, Dummy, Dummy Reason", complianceRiskStatusChangeLog.ClearedReason);
		}

		public void TestInitFromStmComplianceEvent_WithOverrideClearLogSnapshot_WithoutCommodityProvider()
		{
			#region Init Data

			var shipment = Factory.NewWithValidTestData<ShipmentWithoutCommodityProvider>();
			shipment.JS_UniqueConsignRef = "S123";
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

			Factory.Save();

			#endregion

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Dummy Reason"));

			var query2 = new ZQuery(StmComplianceEventSchema.SCE_ParentID, shipment.PK);
			query2.AddToFilter(StmComplianceEventSchema.SCE_EventType, AutoEvents.StatusUpdatedCode);
			query2.AddToFilter(StmComplianceEventSchema.SCE_EventSubType, ComplianceEventList.Codes.OverallRisk);
			query2.AddToFilter(StmComplianceEventSchema.SCE_NewValue, ComplianceRiskStatusCodeList.Codes.OverrideClear);
			var overrideClearLog = Factory.Load<StmComplianceEvent>(query2).Single();
			var complianceRiskStatusChangeLog = new ComplianceRiskStatusChangeLog(overrideClearLog);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatusChangeLog.OverallRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.OverrideClear, complianceRiskStatusChangeLog.OverallRiskDescription);
			AssertEquals(overrideClearLog.SCE_EventTimeOffset.ToDateTime(), complianceRiskStatusChangeLog.EventDateTime);
			AssertEquals(Environment.Env.CurrentUser.FullName, complianceRiskStatusChangeLog.User);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatusChangeLog.PartyRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatusChangeLog.LocationRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.NotApplicable, complianceRiskStatusChangeLog.CommodityRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.Clear, complianceRiskStatusChangeLog.PartyRiskDescription);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.Clear, complianceRiskStatusChangeLog.LocationRiskDescription);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.NotApplicable, complianceRiskStatusChangeLog.CommodityRiskDescription);
			AssertEquals(true, complianceRiskStatusChangeLog.SnapshotExists);
			AssertEquals("OTH, Dummy, Dummy Reason", complianceRiskStatusChangeLog.ClearedReason);
		}

		public void TestInitFromStmComplianceEvent_EmptyLog()
		{
			var eventLog = Factory.New<StmComplianceEvent>();
			var complianceRiskStatusChangeLog = new ComplianceRiskStatusChangeLog(eventLog);

			AssertEquals(ZString.Empty, complianceRiskStatusChangeLog.OverallRisk);
			AssertEquals("Not Available", complianceRiskStatusChangeLog.OverallRiskDescription);
			AssertEquals(ZDateTime.Empty, complianceRiskStatusChangeLog.EventDateTime);
			AssertEquals(eventLog.SCE_SystemCreateUser, complianceRiskStatusChangeLog.User);
			AssertEquals(0, complianceRiskStatusChangeLog.PartyRiskLogCollection.Count);
			AssertEquals(0, complianceRiskStatusChangeLog.LocationRiskLogCollection.Count);
			AssertEquals(0, complianceRiskStatusChangeLog.CommodityRiskLogCollection.Count);
			AssertEquals(0, complianceRiskStatusChangeLog.CommodityRiskLogCollection.Count);
			AssertEquals(false, complianceRiskStatusChangeLog.SnapshotExists);
			AssertEquals(ZString.Empty, complianceRiskStatusChangeLog.ClearedReason);
		}

		public void TestInitFromStmComplianceEvent_JobNotInitiatedCommodityShouldObscure_WithOverrideClearLogSnapshot()
		{
			#region Init Data

			var shipment = Factory.NewWithValidTestData<ShipmentWithProvider>();
			shipment.JS_UniqueConsignRef = "S123";
			shipment.ExposedCommodities = new[]
			{
				GetComplianceCommodityWithOriginAndDescription("062483", "WCO", "Source2", shipment.PK)
			};
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "NZAKL";

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

			Factory.Save();

			#endregion

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Dummy Reason"));

			var query2 = new ZQuery(StmComplianceEventSchema.SCE_ParentID, shipment.PK);
			query2.AddToFilter(StmComplianceEventSchema.SCE_EventType, AutoEvents.StatusUpdatedCode);
			query2.AddToFilter(StmComplianceEventSchema.SCE_EventSubType, ComplianceEventList.Codes.OverallRisk);
			query2.AddToFilter(StmComplianceEventSchema.SCE_NewValue, ComplianceRiskStatusCodeList.Codes.OverrideClear);
			var overrideClearLog = Factory.Load<StmComplianceEvent>(query2).Single();
			var complianceRiskStatusChangeLog = new ComplianceRiskStatusChangeLog(overrideClearLog);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatusChangeLog.OverallRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Unknown, complianceRiskStatusChangeLog.CommodityRisk);
			AssertEquals(true, complianceRiskStatusChangeLog.SnapshotExists);
			AssertEquals(1, complianceRiskStatusChangeLog.CommodityRiskLogCollection.Count);
			Assert(complianceRiskStatusChangeLog.CommodityRiskLogCollection[0].RiskStatusDescription == "** INITIATE ASSESSMENT TO VIEW **");
			Assert(complianceRiskStatusChangeLog.CommodityRiskLogCollection[0].RiskStatus.IsEmpty);
		}

		public void TestInitFromStmComplianceEvent_CommodityNomenclatureConditionEnabled()
		{
			#region Init Data

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S123";

			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Iran));
			country.RN_IsSanctioned = true;

			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;
			complianceRiskStatus.COR_PartyRisk = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_FullName = "ORG 1";

			shipment.JS_OH_ExportBroker = org1.PK;
			shipment.JS_RL_NKOrigin = "IRABD";

			var pluginBizO = new ComplianceRiskPlugInBusinessObject(shipment);

			pluginBizO.RefreshData();

			var complianceCommodityDetail = pluginBizO.ComplianceRiskStatus.CommodityDetailCollection.AddNew();
			complianceCommodityDetail.CCD_COR_ComplianceRisk = pluginBizO.ComplianceRiskStatus.PK;
			complianceCommodityDetail.CCD_CountryOrGrouping = Codes.WorldCustomsOrganisationWCO;
			complianceCommodityDetail.CCD_HarmonizedCode = "123";
			complianceCommodityDetail.Conditions = "CON";
			complianceCommodityDetail.Description = "DES";
			complianceCommodityDetail.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.PotentialRisk;

			var screeningLog_Org1 = Factory.New<IStmEntityScreeningLog>();
			screeningLog_Org1.PJ_ParentID = org1.PK;
			screeningLog_Org1.PJ_ParentTableCode = org1.TablePrefix;
			screeningLog_Org1.PJ_SourceID = shipment.PK;
			screeningLog_Org1.PJ_SourceTableCode = shipment.TablePrefix;
			screeningLog_Org1.PJ_Status = DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges;

			Factory.Save();

			#endregion

			complianceRiskStatus.TakeSnapshotAndSetStatusToOverrideClear(pluginBizO, ("OTH", "Dummy", "Dummy Reason"));

			var query2 = new ZQuery(StmComplianceEventSchema.SCE_ParentID, shipment.PK);
			query2.AddToFilter(StmComplianceEventSchema.SCE_EventType, AutoEvents.StatusUpdatedCode);
			query2.AddToFilter(StmComplianceEventSchema.SCE_EventSubType, ComplianceEventList.Codes.OverallRisk);
			query2.AddToFilter(StmComplianceEventSchema.SCE_NewValue, ComplianceRiskStatusCodeList.Codes.OverrideClear);
			var overrideClearLog = Factory.Load<StmComplianceEvent>(query2).Single();
			var complianceRiskStatusChangeLog = new ComplianceRiskStatusChangeLog(overrideClearLog);

			AssertEquals(ComplianceRiskStatusCodeList.Codes.OverrideClear, complianceRiskStatusChangeLog.OverallRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.OverrideClear, complianceRiskStatusChangeLog.OverallRiskDescription);
			AssertEquals(overrideClearLog.SCE_EventTimeOffset.ToDateTime(), complianceRiskStatusChangeLog.EventDateTime);
			AssertEquals(Environment.Env.CurrentUser.FullName, complianceRiskStatusChangeLog.User);
			AssertEquals(1, complianceRiskStatusChangeLog.CommodityRiskLogCollection.Count);
			AssertEquals(screeningLog_Org1.PK, complianceRiskStatusChangeLog.PartyRiskLogCollection.Cast<CompliancePartyRiskLog>().Single().EntityScreeningLog.PK);
			AssertEquals("Not Screened", complianceRiskStatusChangeLog.PartyRiskLogCollection.Cast<CompliancePartyRiskLog>().Single().StatusDescription);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.Blocked, complianceRiskStatusChangeLog.LocationRiskLogCollection.Cast<ComplianceLocationRiskLog>().Single().RiskStatus);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatusChangeLog.PartyRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatusChangeLog.LocationRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Codes.HighRisk, complianceRiskStatusChangeLog.CommodityRisk);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.HighRisk, complianceRiskStatusChangeLog.PartyRiskDescription);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.Blocked, complianceRiskStatusChangeLog.LocationRiskDescription);
			AssertEquals(ComplianceRiskStatusCodeList.Descriptions.HighRisk, complianceRiskStatusChangeLog.CommodityRiskDescription);
			AssertEquals(true, complianceRiskStatusChangeLog.SnapshotExists);
			AssertEquals("OTH, Dummy, Dummy Reason", complianceRiskStatusChangeLog.ClearedReason);
		}

		ComplianceCommodity GetComplianceCommodityWithOriginAndDescription(ZString harmonizedCode, ZString groupingOrCountry, ZString source, ZGuid parentJobID)
		{
			return new ComplianceCommodity(harmonizedCode, groupingOrCountry, source, parentJobID, string.Empty, string.Empty, string.Empty);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ComplianceRiskStatusChangeLog(Factory.New<StmComplianceEvent>());
		}

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
