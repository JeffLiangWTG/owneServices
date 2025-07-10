using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.ComplianceRisk.Integration.ComplianceRiskStatusCodeList;

namespace Enterprise.ComplianceRisk.Business.Test
{
	class RiskStatusHelperTest : TestCaseWithFactory
	{
		public void TestGetOverallCodeDescriptionPairListForFilter()
		{
			var list = RiskStatusHelper.GetOverallCodeDescriptionPairListForFilter();
			AssertContainsExactElementsInAnyOrder(new[]
			{
				Codes.Clear,
				Codes.PotentialRisk,
				Codes.OverrideClear,
				Codes.Held,
				Codes.Blocked,
			}, list.Cast<CodeDescriptionPair>().Select(u => u.Code).ToList());
		}

		public void TestGetPartyCodeDescriptionPairListForFilter()
		{
			var list = RiskStatusHelper.GetPartyCodeDescriptionPairListForFilter();
			AssertContainsExactElementsInAnyOrder(new[]
			{
				Codes.Clear,
				Codes.PotentialRisk,
				Codes.HighRisk,
				Codes.Blocked,
			}, list.Cast<CodeDescriptionPair>().Select(u => u.Code).ToList());
		}

		public void TestGetLocationCodeDescriptionPairListForFilter()
		{
			var list = RiskStatusHelper.GetLocationCodeDescriptionPairListForFilter();
			AssertContainsExactElementsInAnyOrder(new[]
			{
				Codes.Clear,
				Codes.PotentialRisk,
				Codes.Blocked,
			}, list.Cast<CodeDescriptionPair>().Select(u => u.Code).ToList());
		}

		public void TestGetCommodityCodeDescriptionPairListForFilter()
		{
			var list = RiskStatusHelper.GetCommodityCodeDescriptionPairListForFilter();
			AssertContainsExactElementsInAnyOrder(new[]
			{
				Codes.Clear,
				Codes.PotentialRisk,
				Codes.Incomplete,
				Codes.NotApplicable,
				Codes.PossibleRisk,
				Codes.HighRisk,
				Codes.Blocked,
				Codes.Unknown,
				Codes.NotAssessed,
			}, list.Cast<CodeDescriptionPair>().Select(u => u.Code).ToList());
		}

		public void TestGetCommodityRiskStatus()
		{
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();

			var commoditiesArray = new[] {
				GetCommodity(Codes.Blocked),
				GetCommodity(Codes.PotentialRisk),
				GetCommodity(Codes.HighRisk),
				GetCommodity(Codes.NotChecked),
				GetCommodity(Codes.PossibleRisk),
				GetCommodity(Codes.Clear),
				GetCommodity(Codes.Released),
			};

			AssertResult(commoditiesArray, Codes.Blocked);
			AssertResult(commoditiesArray.Skip(1), Codes.HighRisk);
			AssertResult(commoditiesArray.Skip(2), Codes.HighRisk);
			AssertResult(commoditiesArray.Skip(3), Codes.Unknown);
			AssertResult(commoditiesArray.Skip(4), Codes.PossibleRisk);
			AssertResult(commoditiesArray.Skip(5), Codes.Clear);
			AssertResult(commoditiesArray.Skip(6), Codes.Clear);

			complianceRiskStatus.COR_CommodityRisk = Codes.Unknown;
			AssertResult([], Codes.Unknown);

			complianceRiskStatus.COR_CommodityRisk = Codes.Clear;
			AssertResult([], Codes.Unknown);

			complianceRiskStatus.COR_CommodityRisk = Codes.PossibleRisk;
			AssertResult([], Codes.PossibleRisk);

			complianceRiskStatus.COR_ParentID = ZGuid.NewZGuid();
			var eventLog = Factory.NewWithValidTestData<StmComplianceEvent>();
			eventLog.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
			eventLog.SCE_EventSubType = ComplianceEventList.Codes.AssessmentDeclined;
			eventLog.SCE_ParentID = complianceRiskStatus.COR_ParentID;
			AssertResult([], Codes.PossibleRisk);

			eventLog.SCE_EventSubType = ComplianceEventList.Codes.AssessmentInitialized;
			AssertResult([], Codes.Incomplete);

			void AssertResult(IEnumerable<(ZString HarmonizedCode, ZString RiskStatus, CommodityType CommodityType)> commodities, string expectedRisk)
			{
				AssertEquals(expectedRisk, complianceRiskStatus.GetCommodityRiskStatus(commodities.ToList()));
			}

			(ZString HarmonizedCode, ZString RiskStatus, CommodityType CommodityType) GetCommodity(string screeningStatus, CommodityType commodityType = CommodityType.FetchDataEntry)
			{
				return (ZString.Empty, screeningStatus, commodityType);
			}
		}

		public void TestSetCommodityRiskStatus()
		{
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();
			var details = new[] {
				GetDetails(Codes.Blocked),
				GetDetails(Codes.PotentialRisk),
				GetDetails(Codes.HighRisk),
				GetDetails(Codes.NotChecked),
				GetDetails(Codes.PossibleRisk),
				GetDetails(Codes.Clear),
				GetDetails(Codes.Released),
			};

			AssertResult(details, Codes.Blocked);
			AssertResult(details.Skip(1), Codes.HighRisk);
			AssertResult(details.Skip(2), Codes.HighRisk);
			AssertResult(details.Skip(3), Codes.Unknown);
			AssertResult(details.Skip(4), Codes.PossibleRisk);
			AssertResult(details.Skip(5), Codes.Clear);
			AssertResult(details.Skip(6), Codes.Clear);

			complianceRiskStatus.COR_CommodityRisk = Codes.Unknown;
			AssertResult([], Codes.Unknown);

			complianceRiskStatus.COR_CommodityRisk = Codes.Clear;
			AssertResult([], Codes.Unknown);

			complianceRiskStatus.COR_CommodityRisk = Codes.PossibleRisk;
			AssertResult([], Codes.PossibleRisk);

			complianceRiskStatus.COR_ParentID = ZGuid.NewZGuid();
			var eventLog = Factory.NewWithValidTestData<StmComplianceEvent>();
			eventLog.SCE_EventType = AutoEvents.ComplianceRiskInteractionCode;
			eventLog.SCE_EventSubType = ComplianceEventList.Codes.AssessmentDeclined;
			eventLog.SCE_ParentID = complianceRiskStatus.COR_ParentID;
			AssertResult([], Codes.PossibleRisk);

			eventLog.SCE_EventSubType = ComplianceEventList.Codes.AssessmentInitialized;
			AssertResult([], Codes.Incomplete);

			void AssertResult(IEnumerable<ComplianceCommodityDetail> details, string expectedRisk)
			{
				complianceRiskStatus.SetCommodityRiskStatus(details);
				AssertEquals(expectedRisk, complianceRiskStatus.COR_CommodityRisk);
			}

			ComplianceCommodityDetail GetDetails(string riskStatus)
			{
				var details = Factory.New<ComplianceCommodityDetail>();
				details.CCD_RiskStatus = riskStatus;
				return details;
			}
		}

		public void TestGetPartyRiskStatusBasedOnPartiesSnapshot()
		{
			var partiesArray = new[] {
				GetParty(ScreeningStatusesList.Codes.Matched),
				GetParty(ScreeningStatusesList.Codes.NotScreened),
				GetParty(ScreeningStatusesList.Codes.RequiresReview),
				GetParty(ScreeningStatusesList.Codes.Unknown),
				GetParty(ScreeningStatusesList.Codes.Clear),
				GetParty(ScreeningStatusesList.Codes.PermanentClear),
			};

			AssertResult(partiesArray, Codes.Blocked);
			AssertResult(partiesArray.Skip(1), Codes.HighRisk);
			AssertResult(partiesArray.Skip(2), Codes.HighRisk);
			AssertResult(partiesArray.Skip(3), Codes.HighRisk);
			AssertResult(partiesArray.Skip(4), Codes.Clear);
			AssertResult(partiesArray.Skip(5), Codes.Clear);
			AssertResult(partiesArray.Skip(6), Codes.Clear);

			void AssertResult(IEnumerable<(ZGuid PK, ZString ScreeningStatus)> parties, string expectedRisk)
			{
				AssertEquals(expectedRisk, RiskStatusHelper.GetPartyRiskStatusBasedOnPartiesSnapshot(parties.ToList()));
			}

			(ZGuid PK, ZString ScreeningStatus) GetParty(string screeningStatus)
			{
				return (Guid.NewGuid(), screeningStatus);
			}
		}

		public void TestSetOverallRiskStatus()
		{
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();

			complianceRiskStatus.COR_PartyRisk = Codes.Clear;
			complianceRiskStatus.COR_LocationRisk = Codes.Clear;
			complianceRiskStatus.COR_CommodityRisk = Codes.Clear;

			complianceRiskStatus.SetOverallRiskStatus(true);
			AssertEquals(Codes.Clear, complianceRiskStatus.COR_OverallRisk);

			complianceRiskStatus.COR_OverallRisk = Codes.OverrideClear;
			complianceRiskStatus.SetOverallRiskStatus(true);
			AssertEquals(Codes.Clear, complianceRiskStatus.COR_OverallRisk);

			complianceRiskStatus.COR_OverallRisk = Codes.OverrideClear;
			complianceRiskStatus.SetOverallRiskStatus(false);
			AssertEquals(Codes.OverrideClear, complianceRiskStatus.COR_OverallRisk);
		}

		public void TestGetOverallRiskStatus()
		{
			var complianceRiskStatus = Factory.New<ComplianceRiskStatus>();

			AssertResults(Codes.Clear);

			AssertResults(expectRiskStatus: Codes.Held, partyRisk: Codes.PotentialRisk);
			AssertResults(expectRiskStatus: Codes.Blocked, partyRisk: Codes.Blocked);
			AssertResults(expectRiskStatus: Codes.Held, partyRisk: Codes.HighRisk);

			AssertResults(expectRiskStatus: Codes.Held, locationRisk: Codes.PotentialRisk);
			AssertResults(expectRiskStatus: Codes.Blocked, locationRisk: Codes.Blocked);

			AssertResults(expectRiskStatus: Codes.Held, commodityRisk: Codes.HighRisk);
			AssertResults(expectRiskStatus: Codes.Held, commodityRisk: Codes.PotentialRisk);
			AssertResults(expectRiskStatus: Codes.Blocked, commodityRisk: Codes.Blocked);
			AssertResults(expectRiskStatus: Codes.Held, commodityRisk: Codes.Incomplete);
			AssertResults(expectRiskStatus: Codes.Held, commodityRisk: Codes.Unknown);
			AssertResults(expectRiskStatus: Codes.Clear, commodityRisk: Codes.PossibleRisk);
			AssertResults(expectRiskStatus: Codes.Clear, commodityRisk: Codes.NotApplicable);

			AssertResults(Codes.Held, Codes.PotentialRisk, Codes.PotentialRisk, Codes.Incomplete);
			AssertResults(Codes.Held, Codes.HighRisk, Codes.Clear, Codes.HighRisk);
			AssertResults(Codes.Blocked, Codes.HighRisk, Codes.Blocked, Codes.HighRisk);

			void AssertResults(string expectRiskStatus, string partyRisk = Codes.Clear, string locationRisk = Codes.Clear, string commodityRisk = Codes.Clear)
			{
				complianceRiskStatus.COR_PartyRisk = partyRisk;
				complianceRiskStatus.COR_LocationRisk = locationRisk;
				complianceRiskStatus.COR_CommodityRisk = commodityRisk;
				AssertEquals(expectRiskStatus, complianceRiskStatus.GetOverallRiskStatus());
			}
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
