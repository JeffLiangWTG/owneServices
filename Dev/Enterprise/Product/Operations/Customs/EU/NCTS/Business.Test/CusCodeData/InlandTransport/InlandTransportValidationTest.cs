using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class InlandTransportValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCY_Data_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(inlandTransport.CY_DataInfo);
		}

		public void TestCheckCY_Data_RuleB2101()
		{
			using (var deciderTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(movementHeader);
				deciderTestContext.EnableRule(x => x.IsRuleB2101Active);
				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					inlandTransport.CY_Code = "IT";
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(inlandTransport.CY_DataInfo);
				}
			}
		}

		public void TestCheckCY_Code_RuleB2101()
		{
			const string expectedErrorMessage = "[B2101] You have not entered a Nationality";
			const string errorMessage = "You have not entered a Nationality";
			using (var deciderTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(movementHeader);
				deciderTestContext.EnableRule(x => x.IsRuleB2101Active);
				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					inlandTransport.CY_Data = "Test";
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(inlandTransport.CY_CodeInfo, expectedErrorMessage, "When RuleB2101 is applicable.");
				}

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					inlandTransport.CY_Data = "Test";
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(inlandTransport.CY_CodeInfo, errorMessage, "When RuleB2101 is not applicable.");
				}
			}
		}

		public void TestCheckCY_Code_Mandatory()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(inlandTransport.CY_CodeInfo);
		}

		public void TestCheckCY_Code_ListValidation()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(inlandTransport.CY_CodeInfo, "X1", Core.Constants.CountryCodes.Australia);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType("D");
			header.BH_ApplicationCode = "NC5";
			movementHeader = header.MovementHeader;
			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
			inlandTransport = movementHeader.InlandTransportList.AddNew();
		}
		NctsDepartureMovementHeader movementHeader;
		InlandTransport inlandTransport;

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive)
			=> ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isTransitionPeriodActive);
	}
}
