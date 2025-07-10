using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

sealed class NctsDepartureMovementHeaderPhase5RuleR0076Test : TestCaseWithFactory
{
	public void TestRequireTransportUpperCaseID_WhenB1811IsDisabled()
	{
		using var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory);
		ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleB1811Active));

		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0076Active));

			departureMovement.BM_ActiveBorderIdentificationType = "10";
			AssertEquals("When NCTS is Phase5 and R0076 is enabled and ActiveBorderIdentificationType = 10, RequireTransportUpperCaseID",
						true, departureMovement.RequireTransportUpperCaseID);

			departureMovement.BM_ActiveBorderIdentificationType = "11";
			AssertEquals("When NCTS is Phase5 and R0076 is enabled and ActiveBorderIdentificationType = 11, RequireTransportUpperCaseID",
						false, departureMovement.RequireTransportUpperCaseID);

			departureMovement.BM_ActiveBorderIdentificationType = "10";
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("When NCTS is Phase4 and R0076 is enabled and ActiveBorderIdentificationType = 10, RequireTransportUpperCaseID",
						false, departureMovement.RequireTransportUpperCaseID);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleR0076Active));
			AssertEquals("When NCTS is Phase5 and ActiveBorderIdentificationType = 10 but R0076 is disabled, RequireTransportUpperCaseID",
						false, departureMovement.RequireTransportUpperCaseID);

			ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0076Active));
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				Factory.InvalidateCachedProperties();

				AssertEquals("When NCTS is Phase5 and R0076 is enabled and ActiveBorderIdentificationType = 10 and is in transition period, RequireTransportUpperCaseID",
							true, departureMovement.RequireTransportUpperCaseID);
			}
		});
	}

	public void TestRequireTransportUpperCaseID_WhenB1811IsEnabled()
	{
		using var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory);
		ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleB1811Active));

		CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0076Active));

			departureMovement.BM_ActiveBorderIdentificationType = "10";
			AssertEquals("When NCTS is Phase5 and both R0076, B1811 are enabled and ActiveBorderIdentificationType = 10, RequireTransportUpperCaseID",
						true, departureMovement.RequireTransportUpperCaseID);

			departureMovement.BM_ActiveBorderIdentificationType = "11";
			AssertEquals("When NCTS is Phase5 and both R0076, B1811 are enabled and ActiveBorderIdentificationType = 11, RequireTransportUpperCaseID",
						false, departureMovement.RequireTransportUpperCaseID);

			departureMovement.BM_ActiveBorderIdentificationType = "10";
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertEquals("When NCTS is Phase4 and both R0076, B1811 are enabled and ActiveBorderIdentificationType = 10, RequireTransportUpperCaseID",
						false, departureMovement.RequireTransportUpperCaseID);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleR0076Active));
			AssertEquals("When NCTS is Phase5 and B1811 is enabled and ActiveBorderIdentificationType = 10 but R0076 is disabled, RequireTransportUpperCaseID",
						false, departureMovement.RequireTransportUpperCaseID);

			ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0076Active));
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true))
			{
				Factory.InvalidateCachedProperties();

				AssertEquals("When NCTS is Phase5 and both R0076, B1811 are enabled and ActiveBorderIdentificationType = 10 and is in transition period, RequireTransportUpperCaseID",
							false, departureMovement.RequireTransportUpperCaseID);
			}
		});
	}

	public void TestTOLCarrierIDIsForcedToUpperCaseWhenRequireTransportUpperCaseID()
	{
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		using var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory);
		ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleR0076Active));

		CombineAssertions(() =>
		{
			departureMovement.BM_ActiveBorderIdentificationType = "10";
			AssertEquals("[PRE-CONDITION] RequireTransportUpperCaseID", true, departureMovement.RequireTransportUpperCaseID);

			departureMovement.BM_TOLCarrierID = "aaaAAbbB";
			AssertEquals(nameof(departureMovement.BM_TOLCarrierID), "AAAAABBB", departureMovement.BM_TOLCarrierID);

			departureMovement.BM_ActiveBorderIdentificationType = "11";
			departureMovement.BM_TOLCarrierID = "zzzYYxx";
			AssertEquals(nameof(departureMovement.BM_TOLCarrierID), "zzzYYxx", departureMovement.BM_TOLCarrierID);

			departureMovement.BM_ActiveBorderIdentificationType = "10";
			AssertEquals(nameof(departureMovement.BM_TOLCarrierID), "ZZZYYXX", departureMovement.BM_TOLCarrierID);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		departureMovement = nctsHeader.MovementHeader;
	}

	NctsHeader nctsHeader;
	NctsDepartureMovementHeader departureMovement;
}
