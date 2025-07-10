using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ComplianceRisk.Business.Test.ComplianceCommodityDetailCollectionTest;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestDate(2024, 08, 01)]
	public class ComplianceRuleMatcherTest : TestCaseWithFactory
	{
		public void TestComplianceRuleMatchWithNoHarmonizedCode()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

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

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(complianceRiskStatus.IsAssessmentInitialized);
				Assert(complianceRiskStatus.CommodityDetailCollection.All(x => ((ComplianceCommodityDetail)x).CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.Blocked));
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestComplianceRuleMatchWithNoHarmonizedCode_PersistUserDecision()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			var complianceRule = Factory.New<ComplianceRule>();
			complianceRule.CRU_Origin = "AU";
			complianceRule.CRU_Destination = "US";
			complianceRule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "12345";
			AssertEquals("Pre-Condition", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				Assert(!commodity1.BlockedByComplianceRule);

				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals("Set to blocked by rule", ComplianceRiskStatusCodeList.Codes.Blocked, commodity1.CCD_RiskStatus);
				Assert(commodity1.BlockedByComplianceRule);

				commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				AssertEquals("Keep the user decision when status is REL", ComplianceRiskStatusCodeList.Codes.Released, commodity1.CCD_RiskStatus);
				Assert(!commodity1.BlockedByComplianceRule);

				commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Clear;
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				AssertEquals("Apply the rule when status is not REL", ComplianceRiskStatusCodeList.Codes.Blocked, commodity1.CCD_RiskStatus);
				Assert(commodity1.BlockedByComplianceRule);
			}
		}

		public void TestComplianceRuleMatchWithHarmonizedCode()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			var complianceRule = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule.CRU_Origin = "AU";
			complianceRule.CRU_Destination = "US";
			complianceRule.CRU_HarmonizedCode = "123456";
			complianceRule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var complianceRule2 = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule2.CRU_Origin = "AU";
			complianceRule2.CRU_Destination = "US";
			complianceRule2.CRU_HarmonizedCode = "654321";
			complianceRule2.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "123456";
			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "111222";
			var commodity3 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity3.CCD_HarmonizedCode = "654321";

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity1.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity2.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity3.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestComplianceRuleMatchWithCommodityScreeningFeature()
		{
			AssertComplianceRuleMatchWithCommodityScreeningFeature(false);
			AssertComplianceRuleMatchWithCommodityScreeningFeature(true);

			void AssertComplianceRuleMatchWithCommodityScreeningFeature(bool enableCommodityScreening)
			{
				using (ComplianceRiskFeatureControlHelper.GetIngoreComplianceWiseCommodityScreeningEnableForTest())
				using (ComplianceRiskFeatureControlHelperTest.SetupComplianceWiseCommodityScreeningMocksForTest(enableCommodityScreening))
				using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
					shipment.JS_RL_NKOrigin = "AUSYD";
					shipment.JS_RL_NKDestination = "USLAX";
					shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
					shipment.JS_E_ARV = ZDateTime.Empty;
					SetJobHeaderToShipment(shipment);

					var complianceRule = Factory.NewWithValidTestData<ComplianceRule>();
					complianceRule.CRU_Origin = "AU";
					complianceRule.CRU_Destination = "US";
					complianceRule.CRU_HarmonizedCode = "123456";
					complianceRule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

					var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
					complianceRiskStatus.COR_ParentID = shipment.PK;
					complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
					complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
					var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
					commodity1.CCD_HarmonizedCode = "123456";

					ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
					AssertEquals(enableCommodityScreening, complianceRiskStatus.IsAssessmentInitialized);
				}
			}
		}

		public void TestComplianceRuleMatchWithHarmonizedCode_PersistUserDecision()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			var complianceRule = Factory.New<ComplianceRule>();
			complianceRule.CRU_Origin = "AU";
			complianceRule.CRU_Destination = "US";
			complianceRule.CRU_HarmonizedCode = "123456";
			complianceRule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "123456";
			AssertEquals("Pre-Condition", ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				Assert(!commodity1.BlockedByComplianceRule);

				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals("Set to blocked by rule", ComplianceRiskStatusCodeList.Codes.Blocked, commodity1.CCD_RiskStatus);
				Assert(commodity1.BlockedByComplianceRule);

				commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Released;
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				AssertEquals("Keep the user decision when status is REL", ComplianceRiskStatusCodeList.Codes.Released, commodity1.CCD_RiskStatus);
				Assert(!commodity1.BlockedByComplianceRule);

				commodity1.CCD_RiskStatus = ComplianceRiskStatusCodeList.Codes.Clear;
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				AssertEquals("Apply the rule when status is not REL", ComplianceRiskStatusCodeList.Codes.Blocked, commodity1.CCD_RiskStatus);
				Assert(commodity1.BlockedByComplianceRule);
			}
		}

		public void TestComplianceRuleMatchWhenRuleUpdateAfterAssessmentInitialized()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

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

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(1, GetRiskInteractionEventLogs().Length);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity1.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity2.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);

				complianceRule.CRU_HarmonizedCode = "23456";

				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals("Only initialized once", 1, GetRiskInteractionEventLogs().Length);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity1.CCD_RiskStatus);
				AssertEquals("23456 meets the compliance rule and has been set to block, no matter Assessment Initialized or Not", ComplianceRiskStatusCodeList.Codes.Blocked, commodity2.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			}

			StmComplianceEvent[] GetRiskInteractionEventLogs()
			{
				var query = new ZQuery(StmComplianceEventSchema.SCE_ParentID, complianceRiskStatus.COR_ParentID);
				query.AddToFilter(StmComplianceEventSchema.SCE_EventType, AutoEvents.ComplianceRiskInteractionCode);
				return Factory.Load<StmComplianceEvent>(query);
			}
		}

		public void TestComplianceRuleMatchWhenJobIsNotInternational()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			var complianceRule = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule.CRU_Origin = "AU";
			complianceRule.CRU_Destination = "";
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

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity2.CCD_RiskStatus);
			}
		}

		public void TestComplianceRuleMatchWithMultiRules()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			var complianceRule1 = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule1.CRU_Origin = "AU";
			complianceRule1.CRU_Destination = "US";
			complianceRule1.CRU_HarmonizedCode = "";
			complianceRule1.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			var complianceRule2 = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule2.CRU_Origin = "AU";
			complianceRule2.CRU_Destination = "US";
			complianceRule2.CRU_HarmonizedCode = "23456";
			complianceRule2.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

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

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(complianceRiskStatus.IsAssessmentInitialized);
				Assert(complianceRiskStatus.CommodityDetailCollection.All(x => ((ComplianceCommodityDetail)x).CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.Blocked));
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestComplianceRuleMatchWithOnlyOriginRule()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			var complianceRule1 = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule1.CRU_Origin = "AU";
			complianceRule1.CRU_Destination = "";
			complianceRule1.CRU_HarmonizedCode = "";
			complianceRule1.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

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

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(complianceRiskStatus.IsAssessmentInitialized);
				Assert(complianceRiskStatus.CommodityDetailCollection.All(x => ((ComplianceCommodityDetail)x).CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.Blocked));
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestComplianceRuleMatchWithOnlyDestinationRule()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			var complianceRule1 = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule1.CRU_Origin = "";
			complianceRule1.CRU_Destination = "US";
			complianceRule1.CRU_HarmonizedCode = "";
			complianceRule1.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

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

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(complianceRiskStatus.IsAssessmentInitialized);
				Assert(complianceRiskStatus.CommodityDetailCollection.Cast<ComplianceCommodityDetail>()
					.All(x => x.CCD_RiskStatus == ComplianceRiskStatusCodeList.Codes.Blocked && x.RowWarnings
					.Any(u => u.Message == ComplianceCommodityDetailValidationReal.GetMessages.ComplianceRuleAdministratorMessage)));
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestComplianceRuleMatchWithOnlyOriginAndHarmonizedCodeRule()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			var complianceRule1 = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule1.CRU_Origin = "AU";
			complianceRule1.CRU_Destination = "";
			complianceRule1.CRU_HarmonizedCode = "23456";
			complianceRule1.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

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

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity2.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity3.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestComplianceRuleMatchWithOnlyDestinationAndHarmonizedCodeRule()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			var complianceRule1 = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule1.CRU_Origin = "";
			complianceRule1.CRU_Destination = "US";
			complianceRule1.CRU_HarmonizedCode = "23456";
			complianceRule1.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

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

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity2.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity3.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestComplianceRuleMatchWithMultiplePointPairs()
		{
			var shipment = Factory.New<ShipmentWithProvider>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			var poIntPairs = new List<ComplianceCheckRequestPointPair>
			{
				new ComplianceCheckRequestPointPair
				{
					OriginPoint = new ComplianceCheckRequestPointPairLocation
					{
						Country = "AU",
						UNLOCO = "AUSYD",
						MovementDescription = "Origin"
					},
					DestinationPoint = new ComplianceCheckRequestPointPairLocation
					{
						Country = "NZ",
						UNLOCO = "NZAKL",
						MovementDescription = "Destination"
					},
					EstimatedTimeOfArrival = new ZDateTime(2024, 1, 1),
					EstimatedTimeOfDeparture = new ZDateTime(2024, 2, 1),
					Mode = "SEA"
				},
				new ComplianceCheckRequestPointPair
				{
					OriginPoint = new ComplianceCheckRequestPointPairLocation
					{
						Country = "CN",
						UNLOCO = "CNSHA",
						MovementDescription = "Origin"
					},
					DestinationPoint = new ComplianceCheckRequestPointPairLocation
					{
						Country = "US",
						UNLOCO = "USORD",
						MovementDescription = "Destination"
					},
					EstimatedTimeOfArrival = new ZDateTime(2024, 1, 1),
					EstimatedTimeOfDeparture = new ZDateTime(2024, 2, 1),
					Mode = "SEA"
				}
			};

			shipment.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(poIntPairs);

			var complianceRule = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule.CRU_Origin = "CN";
			complianceRule.CRU_Destination = "US";
			complianceRule.CRU_HarmonizedCode = "23456";
			complianceRule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "12345";

			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "23456";

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				complianceRiskStatus.COR_CommodityRisk = ComplianceRiskStatusCodeList.Codes.Clear;
				complianceRiskStatus.COR_OverallRisk = ComplianceRiskStatusCodeList.Codes.Clear;
				AssertEquals("Pre-Condition", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals("Pre-Condition", ComplianceRiskStatusCodeList.Codes.Clear, complianceRiskStatus.COR_OverallRisk);

				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity2.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_CommodityRisk);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			}
		}

		public void TestComplianceRuleMatch_CommodityHsCodeStartsWithComplianceRuleHsCode_SetToBlocked()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			var complianceRule1 = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule1.CRU_Origin = "AU";
			complianceRule1.CRU_Destination = "US";
			complianceRule1.CRU_HarmonizedCode = "12345678";
			complianceRule1.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			var complianceRule2 = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule2.CRU_Origin = "AU";
			complianceRule2.CRU_Destination = "US";
			complianceRule2.CRU_HarmonizedCode = "654321";
			complianceRule2.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "1234567802";
			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "6543210001";
			var commodity3 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity3.CCD_HarmonizedCode = "12345";

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);

				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity1.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity2.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity3.CCD_RiskStatus);
			}
		}

		public void TestComplianceRuleMatch_CommodityHsCodeNumericalValueStartsWithComplianceRuleHsCode_SetToBlocked()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			var complianceRule1 = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule1.CRU_Origin = "AU";
			complianceRule1.CRU_Destination = "US";
			complianceRule1.CRU_HarmonizedCode = "87654321";
			complianceRule1.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			var complianceRule2 = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule2.CRU_Origin = "AU";
			complianceRule2.CRU_Destination = "US";
			complianceRule2.CRU_HarmonizedCode = "2345678910";
			complianceRule2.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;
			var complianceRule3 = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule3.CRU_Origin = "AU";
			complianceRule3.CRU_Destination = "US";
			complianceRule3.CRU_HarmonizedCode = "123456";
			complianceRule3.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "12 34.56";
			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "8765.4321.0001";
			var commodity3 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity3.CCD_HarmonizedCode = "2345.678901";

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);

				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity1.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity2.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity3.CCD_RiskStatus);
			}
		}

		public void TestComplianceRule_WithoutCommodityProvider()
		{
			var pointPairs = new List<ComplianceCheckRequestPointPair> {
				new ComplianceCheckRequestPointPair
				{
					OriginPoint = new ComplianceCheckRequestPointPairLocation
					{
						Country = "AU",
						UNLOCO = "AUSYD",
						MovementDescription = "Origin"
					},
					DestinationPoint = new ComplianceCheckRequestPointPairLocation
					{
						Country = "US",
						UNLOCO = "USK99",
						MovementDescription = "Destination"
					},
					EstimatedTimeOfArrival = new ZDateTime(2024, 1, 1),
					EstimatedTimeOfDeparture = new ZDateTime(2024, 2, 1),
					Mode = "SEA"
				}
			};

			var shipment = Factory.NewWithValidTestData<ShipmentWithoutCommodityProvider>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			shipment.AssessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(pointPairs);
			SetJobHeaderToShipment(shipment);

			var complianceRule = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule.CRU_Origin = "AU";
			complianceRule.CRU_Destination = "US";
			complianceRule.CRU_HarmonizedCode = "123456";
			complianceRule.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "123456";

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity1.CCD_RiskStatus);
			}
		}

		public void TestComplianceRuleMatch_MatchedToComplianceRules_ShowWarningMessage()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			var complianceRule1 = Factory.NewWithValidTestData<ComplianceRule>();
			complianceRule1.CRU_Origin = "AU";
			complianceRule1.CRU_Destination = "US";
			complianceRule1.CRU_HarmonizedCode = "123456";
			complianceRule1.CRU_RiskStatus = ComplianceRiskStatusCodeList.Codes.Blocked;

			var complianceRiskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			complianceRiskStatus.COR_ParentID = shipment.PK;
			complianceRiskStatus.COR_ParentTableCode = shipment.TablePrefix;
			complianceRiskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);
			var commodity1 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity1.CCD_HarmonizedCode = "123456";
			var commodity2 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity2.CCD_HarmonizedCode = "1234560001";
			var commodity3 = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity3.CCD_HarmonizedCode = "222111";
			var ruleWarningMessage = ComplianceCommodityDetailValidationReal.GetMessages.ComplianceRuleAdministratorMessage;

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);

				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity1.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity2.CCD_RiskStatus);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity3.CCD_RiskStatus);

				AssertHasRowWarning(commodity1, ruleWarningMessage);
				AssertHasRowWarning(commodity2, ruleWarningMessage);
				AssertNoRowWarningContaining(commodity3, ruleWarningMessage);
			}
		}

		public void TestIgnoreReleaseRules()
		{
			var riskStatus = MockRiskStatus();
			var commodity = MockCommodityWithRule("111111", "111111", ComplianceRiskStatusCodeList.Codes.Released, riskStatus);

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ComplianceRuleMatcher.MatchComplianceRule(riskStatus.ParentJob);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.NotChecked, commodity.CCD_RiskStatus);
			}
		}

		public void TestBlockCommodityWhenReleasedRuleWithEmptyCodeWhileBlockedRuleWithExactCodeAndEmptyOrigin()
		{
			var riskStatus = MockRiskStatus();
			var commodity = MockCommodity("111111", riskStatus);
			MockRule("", ComplianceRiskStatusCodeList.Codes.Clear);
			MockRule("111111", ComplianceRiskStatusCodeList.Codes.Blocked, "");

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ComplianceRuleMatcher.MatchComplianceRule(riskStatus.ParentJob);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity.CCD_RiskStatus);
			}
		}

		public void TestAssessmentInitializedByRule()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var riskStatus = MockRiskStatus(shipment);
			var commodity = MockCommodity("111111", riskStatus);
			MockRule("111111", ComplianceRiskStatusCodeList.Codes.Blocked, "");

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ComplianceRuleMatcher.MatchComplianceRule(riskStatus.ParentJob);
				AssertEquals(true, riskStatus.IsAssessmentInitialized);
				AssertEquals("Set to true when assessment initialized by rule", true, riskStatus.AssessmentInitializedByRule);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity.CCD_RiskStatus);

				riskStatus.AssessmentInitializedByRule = false;
				ComplianceRuleMatcher.MatchComplianceRule(riskStatus.ParentJob);
				AssertEquals("Remain false when assessment initialized", false, riskStatus.AssessmentInitializedByRule);
			}
		}

		public void TestBlockCommodityWhenReleasedRuleWithEmptyDestinationAndBlockedRuleWithEmptyOrigin()
		{
			var riskStatus = MockRiskStatus();
			var commodity = MockCommodity("111111", riskStatus);
			MockRule("", ComplianceRiskStatusCodeList.Codes.Clear, destination: "");
			MockRule("", ComplianceRiskStatusCodeList.Codes.Blocked, "");

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ComplianceRuleMatcher.MatchComplianceRule(riskStatus.ParentJob);
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, commodity.CCD_RiskStatus);
			}
		}

		public void TestComplianceRuleShouldNotMatchWithExpiredJob()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 16);
			shipment.JS_E_ARV = ZDateTime.Empty;

			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			localClient.OH_RL_NKClosestPort = "BEBRU";
			var overseasAgent = Factory.NewWithValidTestData<OrgHeader>();
			overseasAgent.OH_RL_NKClosestPort = "FIATS";

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GC = GlbCompany.CurrentCompany.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.JH_ParentID = shipment.PK;
			job.JH_OA_LocalChargesAddr = localClient.MainAddress.PK;
			job.JH_OA_AgentCollectAddr = overseasAgent.MainAddress.PK;
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
			var commodity = complianceRiskStatus.CommodityDetailCollection.AddNew();
			commodity.CCD_HarmonizedCode = "12345";

			Factory.Save();

			var relatedBizOs = ((IStmALogParent)shipment).BusinessObjectsWithRelatedEvents.Append(shipment);
			foreach (var bizO in relatedBizOs.Where(item => item is IAuditDetails))
			{
				if (bizO is AutoJobDocsAndCartage autoJobDocsAndCartage)
				{
					autoJobDocsAndCartage.JP_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
				}
				else if (bizO is AutoJobHeader autoJobHeader)
				{
					autoJobHeader.JH_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
				}
				else if (bizO is AutoJobShipment autoJobShipment)
				{
					autoJobShipment.JS_SystemCreateTimeUtc = ZDateTime.BrettsBirthday;
				}
			}

			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				ComplianceRuleMatcher.MatchComplianceRule(complianceRiskStatus.ParentJob);
				Assert(!complianceRiskStatus.IsAssessmentInitialized);
				Assert(complianceRiskStatus.CommodityDetailCollection.All(x => ((ComplianceCommodityDetail)x).CCD_RiskStatus != ComplianceRiskStatusCodeList.Codes.Blocked));
				AssertEquals(ComplianceRiskStatusCodeList.Codes.Blocked, complianceRiskStatus.COR_OverallRisk);
			}
		}

		#region Mock helpers

		/// <summary>
		/// </summary>
		/// <param name="shipment">Creates new "AUSYD"-"USLAX" Shipment if not provided</param>
		/// <returns></returns>
		ComplianceRiskStatus MockRiskStatus(ForwardingShipment shipment = null)
		{
			if (shipment == null)
			{
				shipment = MockShipment();
			}

			var riskStatus = Factory.NewWithValidTestData<ComplianceRiskStatus>();
			riskStatus.COR_ParentID = shipment.PK;
			riskStatus.COR_ParentTableCode = shipment.TablePrefix;
			riskStatus.PlugInParent = new ComplianceRiskPlugInBusinessObject(shipment);

			return riskStatus;
		}

		ForwardingShipment MockShipment(string origin = "AUSYD", string destination = "USLAX")
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = origin;
			shipment.JS_RL_NKDestination = destination;
			shipment.JS_E_DEP = new ZDateTime(2024, 7, 26);
			shipment.JS_E_ARV = ZDateTime.Empty;
			SetJobHeaderToShipment(shipment);

			return shipment;
		}

		ComplianceCommodityDetail MockCommodityWithRule(string commodityCode, string ruleCode, string ruleRiskStatusCode, ComplianceRiskStatus riskStatus)
		{
			MockRule(ruleCode, ruleRiskStatusCode);
			var commodity = MockCommodity(commodityCode, riskStatus);
			return commodity;
		}

		ComplianceCommodityDetail MockCommodity(string commodityCode, ComplianceRiskStatus riskStatus)
		{
			var newCommodity = riskStatus.CommodityDetailCollection.AddNew();
			newCommodity.CCD_HarmonizedCode = commodityCode;
			return newCommodity;
		}

		ComplianceRule MockRule(string ruleCode, string ruleRiskStatusCode, string origin = "AU", string destination = "US")
		{
			var rule = Factory.NewWithValidTestData<ComplianceRule>();
			rule.CRU_Origin = origin;
			rule.CRU_Destination = destination;
			rule.CRU_HarmonizedCode = ruleCode;
			rule.CRU_RiskStatus = ruleRiskStatusCode;
			return rule;
		}

		void SetJobHeaderToShipment(CommonShipment shipment)
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_ParentID = shipment.PK;
			job.JH_JobNum = "ABC123";
			job.JH_Status = JobHeaderStatus.Working.Code;
		}

		#endregion
	}
}
