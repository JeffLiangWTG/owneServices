using System;
using System.Linq.Expressions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPhase5RuleR0473ValidationTest : TestCaseWithFactory
	{
		public void TestCheckBM_TOLCarrierID_R0473() => CombineAssertions(() =>
		{
			var r0473ErrorMessage = "[R0473] Must not contain lower case letters.";

			AssertBM_TOLCarrierIDContainsLowerCaseLetter(c => c.IsRuleR0473Active, ModeOfTransportList.Codes._2_RailTransport, NctsTransportTypeOfIdList.Codes._20, r0473ErrorMessage);
			AssertBM_TOLCarrierIDContainsLowerCaseLetter(c => c.IsRuleR0473Active, ModeOfTransportList.Codes._3_RoadTransport, NctsTransportTypeOfIdList.Codes._31, r0473ErrorMessage);
			AssertBM_TOLCarrierIDContainsLowerCaseLetter(c => c.IsRuleR0473Active, ModeOfTransportList.Codes._4_AirTransport, NctsTransportTypeOfIdList.Codes._81, r0473ErrorMessage);
		});

		public void TestCheckBM_TOLCarrierID_R0473_1() => CombineAssertions(() =>
		{
			var r0473_1ErrorMessage = "[R0473-1] Must not contain lower case letters.";	

			AssertBM_TOLCarrierIDContainsLowerCaseLetter(c => c.IsRuleR0473_1Active, ModeOfTransportList.Codes._2_RailTransport, NctsTransportTypeOfIdList.Codes._20, r0473_1ErrorMessage);
			AssertBM_TOLCarrierIDContainsLowerCaseLetter(c => c.IsRuleR0473_1Active, ModeOfTransportList.Codes._3_RoadTransport, NctsTransportTypeOfIdList.Codes._31, r0473_1ErrorMessage);
			AssertBM_TOLCarrierIDContainsLowerCaseLetter(c => c.IsRuleR0473_1Active, ModeOfTransportList.Codes._4_AirTransport, NctsTransportTypeOfIdList.Codes._81, r0473_1ErrorMessage);
		});

		void AssertBM_TOLCarrierIDContainsLowerCaseLetter(Expression<Func<INctsDepartureMovementHeaderPhase5ValidationDecider, bool>> rule,
			string transportMode, string transportType, string errorMessage)
		{
			using var decider = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);
			decider.ClearCachedValidationDecider(movementHeader);
			decider.DisableRule(c => c.IsRuleR0473Active);
			decider.EnableRule(rule);
			AssertEnabledR0473(transportMode, transportType, errorMessage);

			decider.ClearCachedValidationDecider(movementHeader);
			decider.DisableRule(c => c.IsRuleR0473Active);
			decider.DisableRule(rule);
			movementHeader.Validation.ValidateBM_TOLCarrierID();
			AssertNoMessageErrorContaining($"IsRuleActive = OFF, TransportMode = {transportMode}, BM_TOLCarrierID = dff", movementHeader.BM_TOLCarrierIDInfo, errorMessage);
		}

		void AssertEnabledR0473(string transportMode, string transportType, string errorMessage)
		{
			movementHeader.BM_ExportTransportMode = transportMode;
			movementHeader.BM_ActiveBorderIdentificationType = transportType;

			movementHeader.BM_TOLCarrierID = "DFF";
			movementHeader.Validation.ValidateBM_TOLCarrierID();
			AssertNoMessageErrorContaining($"IsRuleActive = ON, TransportMode = {transportMode} BM_TOLCarrierID = DFF", movementHeader.BM_TOLCarrierIDInfo, errorMessage);

			movementHeader.BM_TOLCarrierID = "dff";
			movementHeader.Validation.ValidateBM_TOLCarrierID();
			AssertHasMessageErrorContaining($"IsRuleActive = ON, TransportMode = {transportMode} BM_TOLCarrierID = dff", movementHeader.BM_TOLCarrierIDInfo, errorMessage);

			using (SetNCTSTransitionPeriod(isActive: true))
			{
				if (movementHeader.ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider { IsRuleR0473Active: true })
				{
					movementHeader.Validation.ValidateBM_TOLCarrierID();
					AssertNoMessageErrorContaining($"IsRuleActive = ON, TP = ON, TransportMode = {transportMode}, BM_TOLCarrierID = dff", movementHeader.BM_TOLCarrierIDInfo, errorMessage);
				}
			}

			IDisposable SetNCTSTransitionPeriod(bool isActive) => ZZCustomsFunctionalityEffectiveDate
				.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isActive);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			movementHeader = header.MovementHeader;
		}
		NctsDepartureMovementHeader movementHeader;
	}
}
