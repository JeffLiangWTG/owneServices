using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusSealPhase5ValidationTest : TestCaseWithFactory
	{
		public void TestCheckBK_SealNumber()
		{
			var expectedWarning = "[TR0045] Duplicate Seal Number entered.";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var container1 = nctsHeader.DepartureHeaderContainers.AddNew();
			var container2 = nctsHeader.DepartureHeaderContainers.AddNew();
			var seal1 = container1.AdditionalSeals.AddNew();
			var seal2 = container2.AdditionalSeals.AddNew();

			container1.Seal1 = "123";
			container1.Seal2 = "456";
			seal1.BK_SealNumber = "789";

			container2.Seal1 = "321";
			container2.Seal2 = "654";
			seal2.BK_SealNumber = "987";

			CombineAssertions(() =>
			{
				using var ruleContext = new CusSealValidationDeciderTestContext<ICusSealPhase5ValidationDecider>(Factory);
				ruleContext.EnableRule(x => x.IsRuleTR0045Active);

				var seal = container1.AdditionalSeals.AddNew();
				seal.BK_SealNumber = "123";
				AssertHasWarning("Duplicate container1.Seal1", seal.BK_SealNumberInfo, expectedWarning);

				seal.BK_SealNumber = "789";
				AssertHasWarning("Duplicate in container1.AdditionalSeals", seal.BK_SealNumberInfo, expectedWarning);

				seal.BK_SealNumber = "654";
				AssertHasWarning("Duplicate container2.Seal2", seal.BK_SealNumberInfo, expectedWarning);

				seal.BK_SealNumber = "987";
				AssertHasWarning("Duplicate in container2.AdditionalSeals", seal.BK_SealNumberInfo, expectedWarning);

				seal.BK_SealNumber = "ABC";
				AssertNoWarning("Unique seal number", seal.BK_SealNumberInfo, expectedWarning);

				ruleContext.DisableRule(x => x.IsRuleTR0045Active);
				seal.BK_SealNumber = "123";
				AssertNoWarning("Duplicate container1.Seal1 but rule TR0045 is inactive", seal.BK_SealNumberInfo, expectedWarning);
			});
		}

		public void TestCheckBK_SealNumber_RuleN0003()
		{
			const string expectedError = "[N0003] Supporting Document Code 67YY has been entered, therefore Seals must not be entered.";

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);

			var container1 = header.DepartureHeaderContainers.AddNew();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.MovementHeader.SupportingDocuments.AddNew().CSI_Code = NctsTypeOfSupportingDocument.Codes._67YY;
			
			using (var ruleTestContext = new CusSealValidationDeciderTestContext<ICusSealPhase5ValidationDecider>(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					CombineAssertions("When RuleN0003 is active", () =>
					{
						ruleTestContext.EnableRule(x => x.IsRuleN0003Active);

						var additionalSeal = container1.AdditionalSeals.AddNew();
						additionalSeal.BK_SealNumber = "789";

						additionalSeal.Validation.ValidateAll();
						AssertHasRowMessageError(additionalSeal, expectedError);
					});

					CombineAssertions("When RuleN0003 is not active", () =>
					{
						ruleTestContext.DisableRule(x => x.IsRuleN0003Active);
						var additionalSeal = container1.AdditionalSeals.AddNew();
						additionalSeal.BK_SealNumber = "789";

						additionalSeal.Validation.ValidateAll();
						AssertNoRowMessageError(additionalSeal, expectedError);
					});
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					CombineAssertions("When RuleN0003 is active", () =>
					{
						ruleTestContext.EnableRule(x => x.IsRuleN0003Active);

						var additionalSeal = container1.AdditionalSeals.AddNew();
						additionalSeal.BK_SealNumber = "789";

						additionalSeal.Validation.ValidateAll();
						AssertNoRowMessageError(additionalSeal, expectedError);
					});

					CombineAssertions("When RuleN0003 is not active", () =>
					{
						ruleTestContext.DisableRule(x => x.IsRuleN0003Active);
						var additionalSeal = container1.AdditionalSeals.AddNew();
						additionalSeal.BK_SealNumber = "789";

						additionalSeal.Validation.ValidateAll();
						AssertNoRowMessageError(additionalSeal, expectedError);
					});
				}
			}
		}

		public void TestCheckBK_SealNumber_For_Incident()
		{
			var expectedWarning = "[TR0045] Duplicate Seal Number entered.";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var container = nctsHeader.EnRouteIncidents.AddNew().IncidentContainers.AddNew();
			container.BC_Seal1 = "111";
			var seal = container.Seals.AddNew();

			CombineAssertions(() =>
			{
				using var ruleContext = new CusSealValidationDeciderTestContext<ICusSealPhase5ValidationDecider>(Factory);
				ruleContext.EnableRule(x => x.IsRuleTR0045Active);
				seal.BK_SealNumber = "111";
				AssertHasWarning("The seal number duplicates Seal1", seal.BK_SealNumberInfo, expectedWarning);

				seal.BK_SealNumber = "222";
				AssertNoWarning("The Seal number is unique", seal.BK_SealNumberInfo, expectedWarning);

				ruleContext.DisableRule(x => x.IsRuleTR0045Active);
				seal.BK_SealNumber = "111";
				AssertNoWarning("The seal number duplicates Seal1 but rule TR0045 is inactive", seal.BK_SealNumberInfo, expectedWarning);
			});
		}

		public void TestCheckBK_SealNumber_For_NctsArrivalHeaderContainer()
		{
			var expectedWarning = "[TR0045] Duplicate Seal Number entered.";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var headerContainer = nctsHeader.ArrivalHeaderContainers.AddNew();
			headerContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;
			var seal1 = headerContainer.Seals.AddNew();
			var seal2 = headerContainer.Seals.AddNew();

			CombineAssertions(() =>
			{
				using var ruleContext = new CusSealValidationDeciderTestContext<ICusSealPhase5ValidationDecider>(Factory);
				ruleContext.EnableRule(x => x.IsRuleTR0045Active);
				seal1.BK_SealNumber = "111";
				seal2.BK_SealNumber = "111";
				AssertHasWarning("The seal number duplicates Seal1", seal2.BK_SealNumberInfo, expectedWarning);

				seal2.BK_SealNumber = "112";
				AssertNoWarning("The Seal number is unique", seal2.BK_SealNumberInfo, expectedWarning);

				ruleContext.DisableRule(x => x.IsRuleTR0045Active);
				seal2.BK_SealNumber = "111";
				AssertNoWarning("The seal number duplicates Seal1 but rule TR0045 is inactive", seal2.BK_SealNumberInfo, expectedWarning);
			});
		}

		public void TestCheckBK_UnloadedState_For_NctsArrivalHeaderContainer()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var headerContainer = nctsHeader.ArrivalHeaderContainers.AddNew();
			headerContainer.BC_Mode = Core.Constants.ContainerModes.Containerised;
			var seal = headerContainer.Seals.AddNew();

			CombineAssertions(() =>
			{
				seal.BK_UnloadingState = ZString.Empty;
				AssertHasErrorContaining("Unloading State is empty", seal.BK_UnloadingStateInfo, MandatoryValidation.MustBeEntered);

				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.NEW;
				AssertNoErrorContaining("Unloading State is entered (NEW)", seal.BK_UnloadingStateInfo, MandatoryValidation.MustBeEntered);
			});
		}
	}
}
