 using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using static Enterprise.Core.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateRuleNR0079_IsTransitionPeriod()
		{
			const string expectedWarning = "[NR0079] Previous Documents will not be included in pre-declarations.";
			using var ruleContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
			CombineAssertions(() =>
			{
				ruleContext.EnableRule(c => c.IsRuleNR0079Active);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: true))
				{
					AssertNoRowWarningContaining(nctsBill, expectedWarning);
					var prevdoc = nctsBill.PreviousDocuments.AddNew();
					nctsBill.Validation.ValidateRuleNR0079();
					AssertNoRowWarningContaining(nctsBill, expectedWarning);
				}
			});
		}

		public void TestValidateRuleNR0079_NotTransitionPeriod()
		{
			const string expectedWarning = "[NR0079] Previous Documents will not be included in pre-declarations.";
			using var ruleContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
			CombineAssertions(() =>
			{
				ruleContext.EnableRule(c => c.IsRuleNR0079Active);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality("NC5TP", "EUN", ZDate.Today, value: false))
				{
					AssertNoRowWarningContaining(nctsBill, expectedWarning);
					var prevdoc = nctsBill.PreviousDocuments.AddNew();
					nctsBill.Validation.ValidateRuleNR0079();
					AssertHasRowWarningContaining(nctsBill, expectedWarning);

					ruleContext.DisableRule(c => c.IsRuleNR0079Active);
					nctsBill.ClearAllNotifications();
					nctsBill.Validation.ValidateRuleNR0079();
					AssertNoRowWarningContaining(nctsBill, expectedWarning);
				}
			});
		}

		public void TestValidateRuleNR0062()
		{
			const string expectedWarning = "[NR0062] The sum of Gross Weight in packages (";

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem1 = nctsBill.ArrivalGoodsItems.AddNew();
			var goodsItem2 = nctsBill.ArrivalGoodsItems.AddNew();

			var package1 = goodsItem1.Packages.AddNew();
			var package2 = goodsItem1.Packages.AddNew();

			using var ruleContext = new NctsBillValidationDeciderTestContext<INctsBillArrivalPhase5ValidationDecider>(Factory);
			CombineAssertions(() =>
			{
				ruleContext.EnableRule(c => c.IsRuleNR0062Active);

				goodsItem1.BY_GrossWeight = 2;
				goodsItem2.BY_GrossWeight = 3;
				nctsBill.Validation.ValidateRuleNR0062();
				AssertNoRowWarningContaining("(items sum > 0) && (packages sum = 0)", nctsBill, expectedWarning);

				package1.B5_GrossWeight = 2;
				package2.B5_GrossWeight = 3;
				nctsBill.Validation.ValidateRuleNR0062();
				AssertNoRowWarningContaining("(items sum) == (packages sum)", nctsBill, expectedWarning);

				package1.B5_GrossWeight = 3;
				nctsBill.Validation.ValidateRuleNR0062();
				var completeExpectedWarning = expectedWarning + "6.000000 KG) does not match with the sum of Gross Weight (5.000000 KG) in Goods Items";
				AssertHasRowWarningContaining("(items sum) != (packages sum)", nctsBill, completeExpectedWarning);

				goodsItem1.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				goodsItem1.UnloadedGoodsItem.BY_GrossWeight = 3;
				nctsBill.ClearAllNotifications();
				nctsBill.Validation.ValidateRuleNR0062();
				AssertNoRowWarningContaining("with DIF (items sum) == (packages sum)", nctsBill, expectedWarning);

				goodsItem1.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				nctsBill.Validation.ValidateRuleNR0062();
				AssertHasRowWarningContaining("(items sum) != (packages sum)", nctsBill, expectedWarning);

				ruleContext.DisableRule(c => c.IsRuleNR0062Active);
				nctsBill.ClearAllNotifications();
				nctsBill.Validation.ValidateRuleNR0062();
				AssertNoRowWarningContaining("Rule Inactive and (items sum) != (packages sum)", nctsBill, expectedWarning);
			});
		}

		public void TestValidateRuleB1896_IsTransitionPeriod()
		{
			var errorMessage = "[B1896] Transport Document is required when Security is ENT or BTH, UCR is EMPTY and Declaration type is not 'TIR'.";
			var additionalDocument = nctsBill.AdditionalDocuments.AddNew();
			var movementHeader = nctsBill.Header.MovementHeader;
			using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
			{
				deciderTestContext.EnableRule(c => c.IsRuleB1896Active);
				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
				additionalDocument.CSI_SubType = "TRA";
				nctsBill.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(nctsBill, errorMessage);

				additionalDocument.CSI_Code = "T1";
				nctsBill.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(nctsBill, errorMessage);

				additionalDocument.CSI_SubType = ZString.Empty;
				nctsBill.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(nctsBill, errorMessage);

				movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				nctsBill.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(nctsBill, errorMessage);

				movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
				nctsBill.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(nctsBill, errorMessage);

				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				nctsBill.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(nctsBill, errorMessage);
			}

			var newFactory = new BusinessObjectFactory();
			var nctsHeader = newFactory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var newFactoryHeader = nctsHeader.MovementHeader;
			var newFactoryBill = nctsHeader.Bills.AddNew();
			var newFactoryDocument = newFactoryBill.AdditionalDocuments.AddNew();
			newFactoryDocument.CSI_SubType = "INF";

			using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
			{
				deciderTestContext.DisableRule(x => x.IsRuleB1896Active);
				newFactoryHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				newFactoryHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
				newFactoryDocument.CSI_SubType = "TRA";
				newFactoryBill.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(nctsBill, errorMessage);

				additionalDocument.CSI_Code = "T1";
				newFactoryBill.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(nctsBill, errorMessage);

				newFactoryDocument.CSI_SubType = ZString.Empty;
				newFactoryBill.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(nctsBill, errorMessage);

				newFactoryHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				newFactoryBill.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(nctsBill, errorMessage);

				newFactoryHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
				newFactoryHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
				newFactoryBill.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(nctsBill, errorMessage);

				newFactoryHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				newFactoryBill.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(nctsBill, errorMessage);
			}
		}

		public void TestValidateRuleB1896_NotTransitionPeriod()
		{
			var errorMessage = "[B1896] Transport Document is required when Security is ENT or BTH, UCR is EMPTY and Declaration type is not 'TIR'.";
			var additionalDocument = nctsBill.AdditionalDocuments.AddNew();
			var movementHeader = nctsBill.Header.MovementHeader;
			using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
			{
				deciderTestContext.EnableRule(c => c.IsRuleB1896Active);
				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.ENT;
				movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
				additionalDocument.CSI_SubType = "TRA";
				nctsBill.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(nctsBill, errorMessage);
			}
		}

		public void TestCheckB0_Weight_CusInBondMoveDetailNew()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsBill = nctsHeader.Bills.AddNew();
			nctsBill.MovementDetail.B9_UnloadedState = "NEW";
			nctsBill.B0_Weight = 0;
			ValidationTestHelper.AssertErrorIfNotEnteredWhenOtherPropertyHasValue(nctsBill.B0_WeightInfo, nctsBill.MovementDetail.B9_UnloadedStateInfo, (ZString)"NEW", combineAssertions: true);
		}

		public void TestCheckB0_Weight_LessThanSumOfUnderlyingGoodsItemsGrossWeights_WhenActive()
		{
			var messageError = "[R0983] Gross Weight for a House Consignment can't be less than sum of Gross Weights of its underlying Goods Items.";
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem1 = nctsBill.GoodsItems.AddNew();
			var goodsItem2 = nctsBill.GoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 50m;
			goodsItem1.BY_GrossWeightUnit = "KG";
			goodsItem2.BY_GrossWeight = 50000m;
			goodsItem2.BY_GrossWeightUnit = "G";

			var nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsBillArrival = nctsHeaderArrival.Bills.AddNew();
			var goodsItem3 = nctsBillArrival.ArrivalGoodsItems.AddNew();
			var goodsItem4 = nctsBillArrival.ArrivalGoodsItems.AddNew();
			goodsItem3.BY_GrossWeight = 50m;
			goodsItem3.BY_GrossWeightUnit = "KG";
			goodsItem4.BY_GrossWeight = 50000m;
			goodsItem4.BY_GrossWeightUnit = "G";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(c => c.IsRuleR0983Active);
					nctsBill.B0_Weight = 100m;
					nctsBillArrival.B0_Weight = 100m;
					AssertNoMessageErrorContaining("B0_Weight is not less than Sum of Gross Weights of Underlying Goods Items - Departure.", nctsBill.B0_WeightInfo, messageError);
					AssertNoMessageErrorContaining("B0_Weight is not less than Sum of Gross Weights of Underlying Goods Items - Arrival.", nctsBillArrival.B0_WeightInfo, messageError);

					nctsBill.B0_Weight = 99m;
					nctsBillArrival.B0_Weight = 99m;
					AssertHasMessageErrorContaining("B0_Weight is less than Sum of Gross Weights of Underlying Goods Items.", nctsBill.B0_WeightInfo, messageError);
					AssertNoMessageErrorContaining("B0_Weight is less than Sum of Gross Weights of Underlying Goods Items, but the error does not appear for arrival movement.", nctsBillArrival.B0_WeightInfo, messageError);

					nctsBill.B0_Weight = 0m;
					nctsBillArrival.B0_Weight = 0m;
					AssertHasMessageErrorContaining("If B0_Weight equals 0, it should be seen as less than Sum of Gross Weights of Underlying Goods Items.", nctsBill.B0_WeightInfo, messageError);
					AssertNoMessageErrorContaining("if B0_Weight equals 0, it should be seen as less than Sum of Gross Weights of Underlying Goods Items, but the error does not appear for arrival movement.", nctsBillArrival.B0_WeightInfo, messageError);
				}
			});
		}

		public void TestCheckB0_Weight_ConditionRuleTR0077_WhenActive()
		{
			var messageError = "[TR0077] Gross Weight for a House Consignment can't be less than sum of Gross Weights of its underlying Goods Items.";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem1 = nctsBill.GoodsItems.AddNew();
			var goodsItem2 = nctsBill.GoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 50m;
			goodsItem1.BY_GrossWeightUnit = "KG";
			goodsItem2.BY_GrossWeight = 50000m;
			goodsItem2.BY_GrossWeightUnit = "G";

			var nctsHeaderArrival = Factory.New<NctsHeader>();
			nctsHeaderArrival.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderArrival.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsBillArrival = nctsHeaderArrival.Bills.AddNew();
			var goodsItem3 = nctsBillArrival.ArrivalGoodsItems.AddNew();
			var goodsItem4 = nctsBillArrival.ArrivalGoodsItems.AddNew();
			goodsItem3.BY_GrossWeight = 50m;
			goodsItem3.BY_GrossWeightUnit = "KG";
			goodsItem4.BY_GrossWeight = 50000m;
			goodsItem4.BY_GrossWeightUnit = "G";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.ClearCachedValidationDecider(nctsBill);
					deciderTestContext.EnableRule(c => c.IsRuleTR0077Active);
					nctsBill.B0_Weight = 100m;
					AssertNoMessageErrorContaining("B0_Weight is not less than Sum of Gross Weights of Underlying Goods Items - Departure.", nctsBill.B0_WeightInfo, messageError);
					AssertNoMessageErrorContaining("B0_Weight is not less than Sum of Gross Weights of Underlying Goods Items - Arrival", nctsBillArrival.B0_WeightInfo, messageError);

					nctsBill.B0_Weight = 99m;
					AssertHasMessageErrorContaining("B0_Weight is less than Sum of Gross Weights of Underlying Goods Items. - Departure", nctsBill.B0_WeightInfo, messageError);
					AssertNoMessageErrorContaining("B0_Weight is less than Sum of Gross Weights of Underlying Goods Items, but the error message should not appear on an arrival movement.", nctsBillArrival.B0_WeightInfo, messageError);

					nctsBill.B0_Weight = 0m;
					AssertNoMessageErrorContaining("If B0_Weight 0, then no message error should be displayed. - Departure", nctsBill.B0_WeightInfo, messageError);
					AssertNoMessageErrorContaining("If B0_Weight 0, then no message error should be displayed. - Arrival", nctsBillArrival.B0_WeightInfo, messageError);
				}
			});
		}

		public void TestCheckB0_Weight_ConditionRuleTR0077_WhenInactive()
		{
			var messageError = "[TR0077] Gross Weight for a House Consignment can't be less than sum of Gross Weights of its underlying Goods Items.";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem1 = nctsBill.GoodsItems.AddNew();
			var goodsItem2 = nctsBill.GoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 50m;
			goodsItem1.BY_GrossWeightUnit = "KG";
			goodsItem2.BY_GrossWeight = 50000m;
			goodsItem2.BY_GrossWeightUnit = "G";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.DisableRule(c => c.IsRuleTR0077Active);
					nctsBill.B0_Weight = 100m;
					AssertNoRowMessageError("B0_Weight is not less than Sum of Gross Weights of Underlying Goods Items - Departure.", nctsBill, messageError);

					nctsBill.B0_Weight = 99m;
					AssertNoRowMessageError("B0_Weight is less than Sum of Gross Weights of Underlying Goods Items. - Departure", nctsBill, messageError);

					nctsBill.B0_Weight = 0m;
					AssertNoRowMessageError("If B0_Weight 0, then no message error should be displayed. - Departure", nctsBill, messageError);
				}
			});
		}

		public void TestCheckB0_Weight_LessThanSumOfUnderlyingGoodsItemsGrossWeights_WhenInactive()
		{
			const string errorMessage = "[R0983] Gross Weight for a House Consignment can't be less than sum of Gross Weights of its underlying Goods Items.";

			using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			{
				var goodsItem1 = nctsBill.GoodsItems.AddNew();
				var goodsItem2 = nctsBill.GoodsItems.AddNew();
				CombineAssertions("RuleInactive", () =>
				{
					deciderTestContext.ClearCachedValidationDecider(nctsBill);
					deciderTestContext.EnableRule(x => x.IsRuleR0983Active);
					goodsItem1.BY_GrossWeight = 50m;
					goodsItem1.BY_GrossWeightUnit = "KG";
					goodsItem2.BY_GrossWeight = 50001m;
					goodsItem2.BY_GrossWeightUnit = "G";
					nctsBill.B0_Weight = 100m;

					AssertHasMessageErrorContaining("B0_Weight is less than Sum of Gross Weights of Underlying Goods Items, Given BY_GrossWeightUnit is in G and rule R0983 disabled enabled", nctsBill.B0_WeightInfo, errorMessage);

					deciderTestContext.DisableRule(x => x.IsRuleR0983Active);
					nctsBill.Validation.ValidateB0_Weight();

					AssertNoMessageErrorContaining("No Error related to B0_Weight is less than Sum of Gross Weights of Underlying Goods Items, Given BY_GrossWeightUnit is in G and rule R0983 disabled ", nctsBill.B0_WeightInfo, errorMessage);

					deciderTestContext.EnableRule(x => x.IsRuleR0983Active);
					goodsItem1.BY_GrossWeight = 50m;
					goodsItem1.BY_GrossWeightUnit = "KG";
					goodsItem2.BY_GrossWeight = 51m;
					goodsItem2.BY_GrossWeightUnit = "KG";
					nctsBill.B0_Weight = 100m;

					AssertHasMessageErrorContaining("B0_Weight is less than Sum of Gross Weights of Underlying Goods Items, Given BY_GrossWeightUnit is in KG and rule R0983 enabled", nctsBill.B0_WeightInfo, errorMessage);

					deciderTestContext.DisableRule(x => x.IsRuleR0983Active);
					nctsBill.Validation.ValidateB0_Weight();

					AssertNoMessageErrorContaining("No Error related to B0_Weight is less than Sum of Gross Weights of Underlying Goods Items, Given BY_GrossWeightUnit is in KG and rule R0983 disabled", nctsBill.B0_WeightInfo, errorMessage);
				});
			}
		}

		public void TestCheckB0_Weight_NationalRuleNR0078()
		{
			var goodsItem1 = nctsBill.GoodsItems.AddNew();
			var goodsItem2 = nctsBill.GoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 50m;
			goodsItem1.BY_GrossWeightUnit = "KG";
			goodsItem2.BY_GrossWeight = 50000m;
			goodsItem2.BY_GrossWeightUnit = "G";

			var message = "[NR0078] Total Gross Weight for House Consignment is different from the total Gross Weight of its Goods Items (100 Kg)";
			using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(nctsBill);
				deciderTestContext.EnableRule(c => c.IsRuleNR0078Active);
				CombineAssertions("Enable Rule NR0078", () =>
				{
					nctsBill.B0_Weight = 200m;
					AssertHasWarning("B0_Weight is greater than the Sum of Gross Weights of Underlying Goods Items.", nctsBill.B0_WeightInfo, message);

					nctsBill.B0_Weight = 100m;
					AssertNoWarning("B0_Weight is equal to the Sum of Gross Weights of Underlying Goods Items.", nctsBill.B0_WeightInfo, message);

					nctsBill.B0_Weight = 0m;
					AssertHasWarning("B0_Weight is less than the Sum of Gross Weights of Underlying Goods Items.", nctsBill.B0_WeightInfo, message);
				});

				deciderTestContext.DisableRule(c => c.IsRuleNR0078Active);
				nctsBill.B0_Weight = 200m;
				AssertNoWarning("Disable Rule NR0078", nctsBill.B0_WeightInfo, message);
			}
		}

		public void TestCheckB0_Weight_ConditionRuleR0983_1()
		{
			var message = "[R0983-1] Total Gross Weight on House Consignment should be equal or greater than the sum of Gross Weight of its underlying Goods Items (100 kg)";

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem1 = nctsBill.GoodsItems.AddNew();
			var goodsItem2 = nctsBill.GoodsItems.AddNew();
			goodsItem1.BY_GrossWeight = 50m;
			goodsItem1.BY_GrossWeightUnit = "KG";
			goodsItem2.BY_GrossWeight = 50000m;
			goodsItem2.BY_GrossWeightUnit = "G";

			var nctsHeaderWithConversion = Factory.New<NctsHeader>();
			nctsHeaderWithConversion.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeaderWithConversion.BH_HeaderType = NctsMovementType.Codes.Departure;
			var nctsBillInGrams = nctsHeaderWithConversion.Bills.AddNew();
			nctsBillInGrams.B0_WeightUQ = "G";
			var goodsItemWithConversion1 = nctsBillInGrams.GoodsItems.AddNew();
			goodsItemWithConversion1.BY_GrossWeight = 100m;
			goodsItemWithConversion1.BY_GrossWeightUnit = "KG";

			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				deciderTestContext.ClearCachedValidationDecider(nctsBill);
				deciderTestContext.ClearCachedValidationDecider(nctsBillInGrams);
				deciderTestContext.EnableRule(c => c.IsRuleR0983_1Active);
				nctsBill.B0_Weight = 101m;
				nctsBillInGrams.B0_Weight = 101000m;
				AssertHasWarning("B0_Weight is more than Sum of Gross Weights of Underlying Goods Items. - Departure", nctsBill.B0_WeightInfo, message);
				AssertHasWarning("B0_Weight is more than Sum of Gross Weights of Underlying Goods Items. - Convert from Grams", nctsBillInGrams.B0_WeightInfo, message);

				nctsBill.B0_Weight = 100m;
				nctsBillInGrams.B0_Weight = 100000m;
				AssertNoWarning("B0_Weight equal to Sum of Gross Weights of Underlying Goods Items - Departure.", nctsBill.B0_WeightInfo, message);
				AssertNoWarning("B0_Weight equal to Sum of Gross Weights of Underlying Goods Items - Convert from Grams", nctsBillInGrams.B0_WeightInfo, message);

				nctsBill.B0_Weight = 99m;
				nctsBillInGrams.B0_Weight = 99000m;
				AssertHasWarning("B0_Weight is less than Sum of Gross Weights of Underlying Goods Items. - Departure", nctsBill.B0_WeightInfo, message);
				AssertHasWarning("B0_Weight is less than Sum of Gross Weights of Underlying Goods Items. - Convert from Grams", nctsBillInGrams.B0_WeightInfo, message);

				deciderTestContext.DisableRule(c => c.IsRuleR0983_1Active);
				nctsBill.B0_Weight = 101m;
				nctsBillInGrams.B0_Weight = 101000m;
				AssertNoWarning("R0983_1 is inactive, B0_Weight is more than Sum of Gross Weights of Underlying Goods Items. - Departure", nctsBill.B0_WeightInfo, message);
				AssertNoWarning("R0983_1 is inactive, B0_Weight is more than Sum of Gross Weights of Underlying Goods Items. - Convert from Grams", nctsBillInGrams.B0_WeightInfo, message);
			});
		}

		public void TestCheckGrossMassRule_R0221_WhenActive_NotInPhase5TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
			{
				var errorMessage = "[R0221] Gross Weight must be greater than 0 for at least one goods item.";
				var bulkType = Factory.SetupBulkCusCode();

				CombineAssertions(() =>
				{
					using var ruleContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
					ruleContext.EnableRule(c => c.IsRuleR0221Active);
					nctsBill.Validation.ValidateGrossMassRule_R0221();
					AssertNoRowMessageError("There is no message error when no goods items.", nctsBill, errorMessage);

					var goodItem = nctsBill.GoodsItems.AddNew();
					goodItem.BY_GrossWeight = ZDecimal.Zero;
					var goodItem2 = nctsBill.GoodsItems.AddNew();
					goodItem2.BY_GrossWeight = ZDecimal.Zero;
					nctsBill.Validation.ValidateGrossMassRule_R0221();
					AssertHasRowMessageError("There is no package number in goods items and all of the goods items Gross Weight equal to 0.", nctsBill, errorMessage);
					nctsBill.ClearAllNotifications();

					var package = goodItem.Packages.AddNew();
					package.B5_UnitCount = ZLong.Zero;
					var package2 = goodItem2.Packages.AddNew();
					package2.B5_UnitCount = ZLong.Zero;
					nctsBill.Validation.ValidateGrossMassRule_R0221();
					AssertHasRowMessageError("All of the package number in goods items is 0 and all of the goods items Gross Weight equal to 0.", nctsBill, errorMessage);
					nctsBill.ClearAllNotifications();

					package.B5_UnitType = bulkType;
					package.B5_UnitCount = 1;
					nctsBill.Validation.ValidateGrossMassRule_R0221();
					AssertHasRowMessageError("All of the package number which type is no bulk in goods items is 0 and all of the goods items Gross Weight equal to 0.", nctsBill, errorMessage);
					nctsBill.ClearAllNotifications();

					package2.B5_UnitCount = 1;
					nctsBill.Validation.ValidateGrossMassRule_R0221();
					AssertNoRowMessageError("At least one goods item has package number greater than 0.", nctsBill, errorMessage);

					package2.B5_UnitCount = ZLong.Zero;
					goodItem2.BY_GrossWeight = 1.2m;
					nctsBill.Validation.ValidateGrossMassRule_R0221();
					AssertNoRowMessageError("At least one goods item Gross Weight greater than 0.", nctsBill, errorMessage);
				});
			}
		}

		public void TestCheckGrossMassRule_R0221_WhenActive_InPhase5TransitionPeriod()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
			{
				var errorMessage = "[R0221] Gross Weight must be greater than 0 for at least one goods item.";
				var goodItem = nctsBill.GoodsItems.AddNew();
				goodItem.BY_GrossWeight = ZDecimal.Zero;

				var package = goodItem.Packages.AddNew();
				package.B5_UnitCount = ZLong.Zero;
				var goodItem2 = nctsBill.GoodsItems.AddNew();
				goodItem2.BY_GrossWeight = ZDecimal.Zero;
				var package2 = goodItem2.Packages.AddNew();
				package2.B5_UnitCount = ZLong.Zero;
				CombineAssertions(() =>
				{
					using var ruleContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
					ruleContext.EnableRule(c => c.IsRuleR0221Active);
					nctsBill.Validation.ValidateAll();
					AssertNoRowMessageError("No message error when InPhase5TransitionPeriod", nctsBill, errorMessage);
				});
			}
		}

		public void TestCheckGrossMassRule_R0221_WhenInactive()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
			{
				var errorMessage = "[R0221] Gross Weight must be greater than 0 for at least one goods item.";
				var goodItem = nctsBill.GoodsItems.AddNew();
				goodItem.BY_GrossWeight = ZDecimal.Zero;

				var package = goodItem.Packages.AddNew();
				package.B5_UnitCount = ZLong.Zero;
				var goodItem2 = nctsBill.GoodsItems.AddNew();
				goodItem2.BY_GrossWeight = ZDecimal.Zero;
				var package2 = goodItem2.Packages.AddNew();
				package2.B5_UnitCount = ZLong.Zero;
				CombineAssertions(() =>
				{
					using var ruleContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
					ruleContext.ClearCachedValidationDecider(nctsBill);
					ruleContext.DisableRule(c => c.IsRuleR0221Active);
					nctsBill.Validation.ValidateAll();
					AssertNoRowMessageError("No message error when Configuration is inactive", nctsBill, errorMessage);
				});
			}
		}

		public void TestCheckB0_RN_NKCountryOfDestination()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsBill.B0_RN_NKCountryOfDestinationInfo, "XX", CountryCodes.Germany);
		}

		public void TestCheckB0_RN_NKCountryOfDestination_RuleC0343_2()
		{
			const string errorMessage = "[C0343-2] Destination Country/Region must be filled either on Declaration, House Consignment or Goods Item Level.";

			var departureMovementHeader = nctsBill.Header.MovementHeader;
			var goodsItem = nctsBill.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.ClearCachedValidationDecider(nctsBill);
					deciderTestContext.EnableRule(c => c.IsRuleC0343_2Active);

					departureMovementHeader.BM_RL_NKDestinationPort = ZString.Empty;
					nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
					goodsItem.BY_RN_NKCountryOfDestination = ZString.Empty;
					AssertHasMessageError("All the three fields are empty", nctsBill.B0_RN_NKCountryOfDestinationInfo, errorMessage);

					departureMovementHeader.BM_RL_NKDestinationPort = ZString.Empty;
					nctsBill.B0_RN_NKCountryOfDestination = CountryCodes.Georgia;
					goodsItem.BY_RN_NKCountryOfDestination = CountryCodes.France;
					nctsBill.Validation.ValidateB0_RN_NKCountryOfDestination();
					AssertHasMessageError("Details tab empty and Country of Destination values in HouseConsignments and Goods Items.", nctsBill.B0_RN_NKCountryOfDestinationInfo, errorMessage);

					departureMovementHeader.BM_RL_NKDestinationPort = CountryCodes.Germany;
					goodsItem.BY_RN_NKCountryOfDestination = ZString.Empty;
					nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
					nctsBill.Validation.ValidateB0_RN_NKCountryOfDestination();
					AssertNoMessageError("Country of destination only filled in details tab", nctsBill.B0_RN_NKCountryOfDestinationInfo, errorMessage);

					departureMovementHeader.BM_RL_NKDestinationPort = ZString.Empty;
					goodsItem.BY_RN_NKCountryOfDestination = CountryCodes.Estonia;
					nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
					nctsBill.Validation.ValidateB0_RN_NKCountryOfDestination();
					AssertNoMessageError("Country of Destination only filled in Goods Item level", nctsBill.B0_RN_NKCountryOfDestinationInfo, errorMessage);

					nctsBill.B0_RN_NKCountryOfDestination = CountryCodes.Georgia;
					departureMovementHeader.BM_RL_NKDestinationPort = ZString.Empty;
					goodsItem.BY_RN_NKCountryOfDestination = ZString.Empty;
					nctsBill.Validation.ValidateB0_RN_NKCountryOfDestination();
					AssertNoMessageError("Country of Destination only filled in HouseConsignment level", nctsBill.B0_RN_NKCountryOfDestinationInfo, errorMessage);

					deciderTestContext.DisableRule(c => c.IsRuleC0343_2Active);
					departureMovementHeader.BM_RL_NKDestinationPort = ZString.Empty;
					nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
					goodsItem.BY_RN_NKCountryOfDestination = ZString.Empty;
					AssertNoMessageError("No message error should show when rule is not active.", nctsBill.B0_RN_NKCountryOfDestinationInfo, errorMessage);
				}
			});
		}

		public void TestCheckB0_RN_NKCountryOfDestination_RuleC0343_2_ForMultipleGoodsItems()
		{
			const string errorMessage = "[C0343-2] Destination Country/Region must be filled either on Declaration, House Consignment or Goods Item Level.";

			var departureMovementHeader = nctsBill.Header.MovementHeader;
			var goodsItem1 = nctsBill.GoodsItems.AddNew();
			var goodsItem2 = nctsBill.GoodsItems.AddNew();
			departureMovementHeader.BM_RL_NKDestinationPort = ZString.Empty;

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.ClearCachedValidationDecider(nctsBill);
					deciderTestContext.EnableRule(c => c.IsRuleC0343_2Active);

					goodsItem1.BY_RN_NKCountryOfDestination = CountryCodes.France;
					goodsItem2.BY_RN_NKCountryOfDestination = CountryCodes.Germany;
					nctsBill.B0_RN_NKCountryOfDestination = ZString.Empty;
					AssertNoMessageError("Not empty BY_RN_NKCountryOfDestination for All Goods Items", nctsBill.B0_RN_NKCountryOfDestinationInfo, errorMessage);

					goodsItem1.BY_RN_NKCountryOfDestination = ZString.Empty;
					goodsItem2.BY_RN_NKCountryOfDestination = ZString.Empty;
					nctsBill.Validation.ValidateB0_RN_NKCountryOfDestination();
					AssertHasMessageError("Empty BY_RN_NKCountryOfDestination for All Goods Items", nctsBill.B0_RN_NKCountryOfDestinationInfo, errorMessage);

					goodsItem1.BY_RN_NKCountryOfDestination = CountryCodes.France;
					goodsItem2.BY_RN_NKCountryOfDestination = ZString.Empty;
					nctsBill.Validation.ValidateB0_RN_NKCountryOfDestination();
					AssertHasMessageError("Empty BY_RN_NKCountryOfDestination for Some Goods Items", nctsBill.B0_RN_NKCountryOfDestinationInfo, errorMessage);

					deciderTestContext.DisableRule(c => c.IsRuleC0343_2Active);
					nctsBill.Validation.ValidateB0_RN_NKCountryOfDestination();
					AssertNoMessageError("No message error should show when rule is not active.", nctsBill.B0_RN_NKCountryOfDestinationInfo, errorMessage);
				}
			});
		}

		public void TestCheckB0_RN_NKCountryOfExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_NC008);
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(nctsBill.B0_RN_NKCountryOfExportInfo, "XX", "AU");
		}

		public void Test_CheckB0_RN_NKCountryOfExport_C0909_WhenActive()
		{
			const string message = "[C0909] You have not entered Country Of Dispatch. It is required either on Declaration or House Consignment or House Consignment Item.";
			var nctsHeader = nctsBill.Header;
			var goodsItem = nctsBill.GoodsItems.AddNew();
			var movementHeader = nctsHeader.MovementHeader;
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillPhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(c => c.IsRuleC0909Active);
					movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T2;
					movementHeader.BM_RN_NKCountryOfDispatch = CountryCodes.China;
					goodsItem.BY_RN_NKCountryOfDispatch = CountryCodes.China;
					nctsBill.B0_RN_NKCountryOfExport = CountryCodes.China;
					AssertNoMessageError("MovementHeader's BM_InBondEntryType is not TIR", nctsBill.B0_RN_NKCountryOfExportInfo, message);

					movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
					nctsBill.Validation.ValidateB0_RN_NKCountryOfExport();
					AssertNoMessageError("MovementHeader's BM_InBondEntryType is TIR", nctsBill.B0_RN_NKCountryOfExportInfo, message);

					movementHeader.BM_RN_NKCountryOfDispatch = ZString.Empty;
					nctsBill.Validation.ValidateB0_RN_NKCountryOfExport();
					AssertNoMessageError("Dispatch Country haven't specified on declaration level", nctsBill.B0_RN_NKCountryOfExportInfo, message);

					movementHeader.BM_RN_NKCountryOfDispatch = CountryCodes.China;
					nctsBill.B0_RN_NKCountryOfExport = ZString.Empty;
					AssertNoMessageError("Dispatch Country haven't specified on house consignment level", nctsBill.B0_RN_NKCountryOfExportInfo, message);

					goodsItem.BY_RN_NKCountryOfDispatch = ZString.Empty;
					nctsBill.B0_RN_NKCountryOfExport = CountryCodes.China;
					AssertNoMessageError("Dispatch Country haven't specified on goods items level", nctsBill.B0_RN_NKCountryOfExportInfo, message);

					nctsBill.B0_RN_NKCountryOfExport = ZString.Empty;
					AssertNoMessageError("Dispatch Country haven't specified on goods items or house consignment level", nctsBill.B0_RN_NKCountryOfExportInfo, message);

					movementHeader.BM_RN_NKCountryOfDispatch = ZString.Empty;
					nctsBill.B0_RN_NKCountryOfExport = CountryCodes.China;
					AssertNoMessageError("Dispatch Country haven't specified on goods items or declaration level", nctsBill.B0_RN_NKCountryOfExportInfo, message);

					goodsItem.BY_RN_NKCountryOfDispatch = CountryCodes.China;
					nctsBill.B0_RN_NKCountryOfExport = ZString.Empty;
					AssertNoMessageError("Dispatch Country haven't specified on house consignment or declaration level", nctsBill.B0_RN_NKCountryOfExportInfo, message);

					goodsItem.BY_RN_NKCountryOfDispatch = ZString.Empty;
					nctsBill.Validation.ValidateB0_RN_NKCountryOfExport();
					AssertHasMessageError("Dispatch Country haven't specified on goods items, house consignment or declaration level", nctsBill.B0_RN_NKCountryOfExportInfo, message);
				}
			});
		}

		public void Test_CheckB0_RN_NKCountryOfExport_C0909_WhenInactive()
		{
			const string message = "[C0909] You have not entered Country Of Dispatch. It is required either on Declaration or House Consignment or House Consignment Item.";
			var nctsHeader = nctsBill.Header;
			var goodsItem = nctsBill.GoodsItems.AddNew();

			using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(nctsBill);
				deciderTestContext.DisableRule(c => c.IsRuleC0909Active);
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
				nctsBill.Validation.ValidateB0_RN_NKCountryOfExport();

				AssertCollectionNotContains($"Expected notifications would not contain {message}", nctsBill.B0_RN_NKCountryOfExportInfo.Notifications.Select(e => e.Message), x => x.Contains(message));
			}
		}

		public void TestCheckB0_RN_NKCountryOfExport_WhenRuleE1301Active()
		{
			var expectedError = "[E1301] In transition period, which is now, Country/Region of Dispatch must be empty";

			CombineAssertions(() =>
			{
				using (var ruleContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
					Constants.FunctionalityTypes.NCTSTransitionPeriod,
					RefDataGroupingCodes.EuropeanUnionEUN,
					ZDate.Today,
					value: true))
				{
					ruleContext.EnableRule(x => x.IsRuleE1301Active);
					nctsBill.B0_RN_NKCountryOfExport = ZString.Empty;
					AssertNoMessageError("When Dispatch Ctry is empty", nctsBill.B0_RN_NKCountryOfExportInfo, expectedError);

					nctsBill.B0_RN_NKCountryOfExport = CountryCodes.India;
					AssertHasMessageError("When Dispatch Ctry is filled", nctsBill.B0_RN_NKCountryOfExportInfo, expectedError);
				}
			});
		}

		public void TestCheckB0_RN_NKCountryOfExport_WhenRuleE1301Inactive()
		{
			var expectedError = "[E1301] In transition period, which is now, Country/Region of Dispatch must be empty";

			using (var ruleContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
				Constants.FunctionalityTypes.NCTSTransitionPeriod,
				RefDataGroupingCodes.EuropeanUnionEUN,
				ZDate.Today,
				value: true))
			{
				ruleContext.EnableRule(x => x.IsRuleE1301Active);
				nctsBill.B0_RN_NKCountryOfExport = CountryCodes.India;
				AssertHasMessageError("When Dispatch Ctry is filled", nctsBill.B0_RN_NKCountryOfExportInfo, expectedError);

				ruleContext.DisableRule(x => x.IsRuleE1301Active);
				nctsBill.Validation.ValidateB0_RN_NKCountryOfExport();
				AssertNoMessageError("When Dispatch Ctry is filled but rule E1301_1 is not active", nctsBill.B0_RN_NKCountryOfExportInfo, expectedError);
			}
		}

		public void TestCheckB0_RN_NKCountryOfExport_R0506()
		{
			var expectedError = "[R0506] Country/Region of Dispatch must be different for at least one of the house consignment.";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.B0_RN_NKCountryOfExport = CountryCodes.Poland;

			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				deciderTestContext.EnableRule(c => c.IsRuleR0506Active);

				AssertNoMessageError("No error when only one bill", bill1.B0_RN_NKCountryOfExportInfo, expectedError);

				var bill2 = nctsHeader.Bills.AddNew();
				bill1.B0_RN_NKCountryOfExport = ZString.Empty;
				bill2.B0_RN_NKCountryOfExport = ZString.Empty;
				AssertNoMessageError("No error when Country of Dispatch is empty", bill2.B0_RN_NKCountryOfExportInfo, expectedError);

				bill1.B0_RN_NKCountryOfExport = CountryCodes.Poland;
				bill2.B0_RN_NKCountryOfExport = CountryCodes.France;
				AssertNoMessageError("All Bills do not have same Country of Dispatch", bill2.B0_RN_NKCountryOfExportInfo, expectedError);

				bill2.B0_RN_NKCountryOfExport = CountryCodes.Poland;
				AssertHasMessageError("All bills have same Country of Dispatch", bill2.B0_RN_NKCountryOfExportInfo, expectedError);

				deciderTestContext.DisableRule(c => c.IsRuleR0506Active);

				bill2.Validation.ValidateB0_RN_NKCountryOfExport();
				AssertNoMessageError("All bills have same Country of Dispatch", bill2.B0_RN_NKCountryOfExportInfo, expectedError);
			});
		}

		public void TestCheckB0_Weight()
		{
			var messageError = "You have not entered a Total Gross Weight.";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var nctsBillArrival = nctsHeader.Bills.AddNew();
			nctsBillArrival.MovementDetail.B9_UnloadedState = "NEW";
			nctsBillArrival.B0_Weight = 0;
			AssertNoMessageError("When B0_Weight have similar errors, the message Error should not be displayed.", nctsBillArrival.B0_WeightInfo, messageError);

			nctsBill.B0_Weight = 0;
			AssertHasMessageError("When there are no similar errors in the B0_Weight, the message Error should be displayed.", nctsBill.B0_WeightInfo, messageError);
		}

		public void TestCheckB0_WeightUQ() => CombineAssertions(() =>
		{
			string[] movementTypes = [NctsMovementType.Codes.Arrival, NctsMovementType.Codes.Departure];

			foreach (var movementType in movementTypes)
			{
				nctsHeader.SetMovementType(movementType);
				AssertNoErrorContaining($"{movementType}, B0_WeightUQ: Default", nctsBill.B0_WeightUQInfo, MandatoryValidation.MustBeEntered);
				AssertNoErrorContaining($"{movementType}, B0_WeightUQ: Default", nctsBill.B0_WeightUQInfo, ListValidation.InvalidCodeError);

				nctsBill.B0_WeightUQ = ZString.Empty;
				AssertHasErrorContaining($"{movementType}, B0_WeightUQ: Empty", nctsBill.B0_WeightUQInfo, MandatoryValidation.MustBeEntered);

				ValidationTestHelper.AssertErrorIfInvalidCode(nctsBill.B0_WeightUQInfo, invalidCode: "XX", validCode: "KG");

				foreach (var weightUnit in Weight.Codes)
				{
					nctsBill.B0_WeightUQ = weightUnit;
					AssertNoErrorContaining($"{movementType}, B0_WeightUQ: {weightUnit}", nctsBill.B0_WeightUQInfo, ListValidation.InvalidCodeError);
				}
			}
		});

		public void TestCheckB0_ReferenceID_WhenRuleE1301Active()
		{
			var expectedError = "[E1301] In transition period, which is now, Reference Number / UCR must be empty";
			CombineAssertions(() =>
			{
				using (var ruleContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
					Constants.FunctionalityTypes.NCTSTransitionPeriod,
					RefDataGroupingCodes.EuropeanUnionEUN,
					ZDate.Today,
					value: true))
				{
					ruleContext.EnableRule(x => x.IsRuleE1301Active);
					nctsBill.B0_ReferenceID = ZString.Empty;
					AssertNoMessageError("When Reference Number is empty", nctsBill.B0_ReferenceIDInfo, expectedError);

					nctsBill.B0_ReferenceID = "HBL101010";
					AssertHasMessageError("When Reference Number is filled", nctsBill.B0_ReferenceIDInfo, expectedError);
				}
			});
		}

		public void TestCheckB0_ReferenceID_WhenRuleE1301Inactive()
		{
			var expectedError = "[E1301] In transition period, which is now, Reference Number / UCR must be empty";
			CombineAssertions(() =>
			{
				using (var ruleContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
					Constants.FunctionalityTypes.NCTSTransitionPeriod,
					RefDataGroupingCodes.EuropeanUnionEUN,
					ZDate.Today,
					value: true))
				{
					ruleContext.EnableRule(x => x.IsRuleE1301Active);
					nctsBill.B0_ReferenceID = "HBL101010";
					AssertHasMessageError("When Reference Number is filled", nctsBill.B0_ReferenceIDInfo, expectedError);

					ruleContext.DisableRule(x => x.IsRuleE1301Active);
					nctsBill.Validation.ValidateB0_ReferenceID();
					AssertNoMessageError("When Reference Number is filled but rule E1301_1 is not active", nctsBill.B0_ReferenceIDInfo, expectedError);
				}
			});
		}

		public void TestCheckB0_ReferenceID_R0506()
		{
			var expectedError = "[R0506] House Consignment ID must be different for at least one of the house consignment.";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.B0_ReferenceID = "HBL101010";

			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				deciderTestContext.EnableRule(c => c.IsRuleR0506Active);

				AssertNoMessageError("No error for single bill", bill1.B0_ReferenceIDInfo, expectedError);

				var bill2 = nctsHeader.Bills.AddNew();
				bill1.B0_ReferenceID = ZString.Empty;
				bill2.B0_ReferenceID = ZString.Empty;
				AssertNoMessageError("No error when ReferenceID is empty", bill2.B0_ReferenceIDInfo, expectedError);

				bill1.B0_ReferenceID = "HBL101010";
				bill2.B0_ReferenceID = "HBL645312";
				AssertNoMessageError("All Bills do not have same ReferenceID", bill2.B0_ReferenceIDInfo, expectedError);

				bill2.B0_ReferenceID = "HBL101010";
				AssertHasMessageError("All bills have same ReferenceID", bill2.B0_ReferenceIDInfo, expectedError);

				deciderTestContext.DisableRule(c => c.IsRuleR0506Active);

				bill2.Validation.ValidateB0_ReferenceID();
				AssertNoMessageError("All bills have same ReferenceID", bill2.B0_ReferenceIDInfo, expectedError);
			});
		}

		public void TestCheckB0_TransportPaymentMethod()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsBill.B0_TransportPaymentMethodInfo, "X", TransportChargesModeOfPayment.Codes.AccountHolderWithCarrier);
		}

		public void TestCheckB0_TransportPaymentMethod_R0506()
		{
			var expectedError = "[R0506] Transport MoP must be different for at least one of the house consignment.";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill1 = nctsHeader.Bills.AddNew();
			bill1.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;

			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				deciderTestContext.EnableRule(c => c.IsRuleR0506Active);

				AssertNoMessageError("No error for single bill", bill1.B0_TransportPaymentMethodInfo, expectedError);

				var bill2 = nctsHeader.Bills.AddNew();
				bill1.B0_TransportPaymentMethod = ZString.Empty;
				bill2.B0_TransportPaymentMethod = ZString.Empty;
				AssertNoMessageError("No error when Transport Charges is empty", bill2.B0_TransportPaymentMethodInfo, expectedError);

				bill1.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;
				bill2.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cheque;
				AssertNoMessageError("All Bills do not have same Transport Charges", bill2.B0_TransportPaymentMethodInfo, expectedError);

				bill2.B0_TransportPaymentMethod = TransportChargesModeOfPayment.Codes.Cash;
				AssertHasMessageError("All bills have same Transport Charges", bill2.B0_TransportPaymentMethodInfo, expectedError);

				deciderTestContext.DisableRule(c => c.IsRuleR0506Active);

				bill2.Validation.ValidateB0_TransportPaymentMethod();
				AssertNoMessageError("All bills have same Transport Charges", bill2.B0_TransportPaymentMethodInfo, expectedError);
			});
		}

		public void TestCheckFirstDepartureTransportMeansID_WhenTR0057Active()
		{
			var expectedError = "[TR0057] You have not entered";
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				deciderTestContext.EnableRule(decider => decider.IsRuleTR0057Active);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._10;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Lloyds", bill.FirstDepartureTransportMeansIDInfo, expectedError);
					bill.TransportAtDeparture = "TI123";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Lloyds", bill.FirstDepartureTransportMeansIDInfo, expectedError);

					bill.TransportAtDeparture = ZString.Empty;
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._11;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Vessel", bill.FirstDepartureTransportMeansIDInfo, expectedError);
					bill.TransportAtDeparture = "TI123";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Vessel", bill.FirstDepartureTransportMeansIDInfo, expectedError);

					bill.TransportAtDeparture = ZString.Empty;
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._20;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Wagon", bill.FirstDepartureTransportMeansIDInfo, expectedError);
					bill.TransportAtDeparture = "TI123";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Wagon", bill.FirstDepartureTransportMeansIDInfo, expectedError);

					bill.TransportAtDeparture = ZString.Empty;
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._21;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Train", bill.FirstDepartureTransportMeansIDInfo, expectedError);
					bill.TransportAtDeparture = "TI123";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Train", bill.FirstDepartureTransportMeansIDInfo, expectedError);

					bill.TransportAtDeparture = ZString.Empty;
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._40;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Flight", bill.FirstDepartureTransportMeansIDInfo, expectedError);
					bill.TransportAtDeparture = "TI123";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Flight", bill.FirstDepartureTransportMeansIDInfo, expectedError);

					bill.TransportAtDeparture = ZString.Empty;
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._41;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Registration", bill.FirstDepartureTransportMeansIDInfo, expectedError);
					bill.TransportAtDeparture = "TI123";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Registration", bill.FirstDepartureTransportMeansIDInfo, expectedError);

					bill.TransportAtDeparture = ZString.Empty;
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._80;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("ENI Code", bill.FirstDepartureTransportMeansIDInfo, expectedError);
					bill.TransportAtDeparture = "TI123";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("ENI Code", bill.FirstDepartureTransportMeansIDInfo, expectedError);

					bill.TransportAtDeparture = ZString.Empty;
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._81;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Vessel", bill.FirstDepartureTransportMeansIDInfo, expectedError);
					bill.TransportAtDeparture = "TI123";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Vesel", bill.FirstDepartureTransportMeansIDInfo, expectedError);

					bill.TransportAtDeparture = ZString.Empty;
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._30;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Transport ID", bill.FirstDepartureTransportMeansIDInfo, expectedError);
					bill.TransportAtDeparture = "TI123";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Transport ID", bill.FirstDepartureTransportMeansIDInfo, expectedError);

					header.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
					bill.TransportAtDeparture = ZString.Empty;
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._99;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Transport ID", bill.FirstDepartureTransportMeansIDInfo, expectedError);
					bill.TransportAtDeparture = "TI123";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Transport ID", bill.FirstDepartureTransportMeansIDInfo, expectedError);
					bill.TransportTypeAtDeparture = ZString.Empty;
					bill.TransportAtDeparture = ZString.Empty;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Transport ID", bill.FirstDepartureTransportMeansIDInfo, expectedError);

					header.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
					bill.TransportAtDeparture = ZString.Empty;
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._99;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageErrorContaining("Transport ID", bill.FirstDepartureTransportMeansIDInfo, expectedError);
					bill.TransportAtDeparture = "TI123";
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Transport ID", bill.FirstDepartureTransportMeansIDInfo, expectedError);
					bill.TransportTypeAtDeparture = ZString.Empty;
					bill.TransportAtDeparture = ZString.Empty;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Transport ID", bill.FirstDepartureTransportMeansIDInfo, expectedError);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._10;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoMessageErrorContaining("Lloyds", bill.FirstDepartureTransportMeansIDInfo, expectedError);
				}
			});
		}

		public void TestCheckFirstDepartureTransportMeansID_WhenTR0057NotActive()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				deciderTestContext.DisableRule(decider => decider.IsRuleTR0057Active);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._10;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("Lloyds", bill.FirstDepartureTransportMeansIDInfo);

					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._11;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("Vessel", bill.FirstDepartureTransportMeansIDInfo);

					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._20;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("Wagon", bill.FirstDepartureTransportMeansIDInfo);

					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._21;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("Train", bill.FirstDepartureTransportMeansIDInfo);

					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._40;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("Flight", bill.FirstDepartureTransportMeansIDInfo);

					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._41;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("Registration", bill.FirstDepartureTransportMeansIDInfo);

					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._80;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("ENI Code", bill.FirstDepartureTransportMeansIDInfo);

					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._81;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("Vessel", bill.FirstDepartureTransportMeansIDInfo);

					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._30;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("Transport ID", bill.FirstDepartureTransportMeansIDInfo);

					header.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._7_FixedTransportInstallations;
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._99;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("Transport ID", bill.FirstDepartureTransportMeansIDInfo);

					header.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._9_OwnPropulsion;
					bill.TransportTypeAtDeparture = NctsTransportTypeOfIdList.Codes._99;
					bill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("Transport ID", bill.FirstDepartureTransportMeansIDInfo);
				}
			});
		}

		public void TestCheckFirstDepartureTransportMeansID_TR0059()
		{
			const string message = "[TR0059] Please capture first Transport ID in field Wagon No./Train No. before using grid Additional Wagon Numbers.";

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();

			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				deciderTestContext.EnableRule(c => c.IsRuleTR0059Active);

				header.MovementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._2_RailTransport;
				bill.FirstDepartureTransportMeansID = "Train ID";
				bill.Validation.ValidateFirstDepartureTransportMeansID();
				AssertNoMessageError("No Additional Wagons", bill.FirstDepartureTransportMeansIDInfo, message);

				bill.TransportDepartureAdditionalWagonNumbers.AddNew();
				bill.TransportDepartureAdditionalWagonNumbers.AddNew();
				bill.FirstDepartureTransportMeansID = ZString.Empty;
				bill.Validation.ValidateFirstDepartureTransportMeansID();
				AssertHasMessageError("Wagon ID cannot be empty if additional wagons are entered", bill.FirstDepartureTransportMeansIDInfo, message);

				bill.FirstDepartureTransportMeansID = "Wagon ID";
				bill.Validation.ValidateFirstDepartureTransportMeansID();
				AssertNoMessageError("Wagon ID is entered", bill.FirstDepartureTransportMeansIDInfo, message);

				deciderTestContext.DisableRule(c => c.IsRuleTR0059Active);

				bill.FirstDepartureTransportMeansID = ZString.Empty;
				bill.Validation.ValidateFirstDepartureTransportMeansID();
				AssertNoMessageError("Wagon ID cannot be empty if additional wagons are entered, but it is ignored for inactive rule", bill.FirstDepartureTransportMeansIDInfo, message);
			});
		}

		public void TestCheckTransportCountryAtDepartureMeansNationality_ValidCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR("NCNAT", "EUN");
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(nctsBill.TransportCountryAtDepartureInfo, "XX", "DE");
		}

		public void TestCheckFirstDepartureTransportMeansNationality_WhenTR0058Active()
		{
			var expectedError = "[TR0058] You have not entered";
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					deciderTestContext.EnableRule(c => c.IsRuleTR0058Active);
					bill.TransportAtDeparture = "ID123";
					bill.Validation.ValidateFirstDepartureTransportMeansNationality();
					AssertHasMessageErrorContaining("Nationality empty", bill.FirstDepartureTransportMeansNationalityInfo, expectedError);
					bill.FirstDepartureTransportMeansNationality = "DE";
					bill.Validation.ValidateFirstDepartureTransportMeansNationality();
					AssertNoMessageErrorContaining("Nationality not empty", bill.FirstDepartureTransportMeansNationalityInfo, expectedError);
					bill.TransportAtDeparture = ZString.Empty;
					bill.FirstDepartureTransportMeansNationality = ZString.Empty;
					bill.Validation.ValidateFirstDepartureTransportMeansNationality();
					AssertNoMessageErrorContaining("Nationality And Transport empty", bill.FirstDepartureTransportMeansNationalityInfo, expectedError);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					bill.TransportAtDeparture = "ID123";
					bill.Validation.ValidateFirstDepartureTransportMeansNationality();
					AssertNoMessageErrorContaining("Nationality empty", bill.FirstDepartureTransportMeansNationalityInfo, expectedError);
				}
			});
		}

		public void TestCheckFirstDepartureTransportMeansNationality_WhenTR0058NotActive()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					deciderTestContext.DisableRule(c => c.IsRuleTR0058Active);
					bill.TransportAtDeparture = "ID123";
					bill.Validation.ValidateFirstDepartureTransportMeansNationality();
					AssertNoNotifications("Nationality", bill.FirstDepartureTransportMeansNationalityInfo);
				}
			});
		}

		public void TestCheckRuleR0364()
		{
			const string r0364Text = "[R0364] At least one consignment item must have Package line with Package Quantity greater than 0 for the same Shipping Marks when Package Type is not bulk or unpackaged.";

			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				deciderTestContext.DisableRule(decider => decider.IsRuleR0364Active);
				deciderTestContext.EnableRule(decider => decider.IsRuleB1964Active);
				using var nctsTransitionPeriodActivate = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
					code: Constants.FunctionalityTypes.NCTSTransitionPeriod,
					dataGroupingCode: RefDataGroupingCodes.EuropeanUnionEUN,
					effectiveDate: ZDate.Today,
					value: true
				);

				var bulkType = Factory.SetupBulkCusCode();
				var unPackedType = Factory.SetupUnpackCusCode();
				nctsBill.GoodsItems.DeleteAll();

				var goodsItem = nctsBill.GoodsItems.AddNew();

				var normalPackage = goodsItem.Packages.AddNew();
				normalPackage.B5_UnitType = "1A";
				normalPackage.B5_MarksAndNumbers = "MARK001";
				normalPackage.B5_UnitCount = 0;
				nctsBill.Validation.ValidateAll();
				AssertNoRowMessageError("R0364-, B1964+, NC5TP+, expecting: R0364 INACTIVATE.", nctsBill, r0364Text);

				using var nctsTransitionPeriodInactivate = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
					code: Constants.FunctionalityTypes.NCTSTransitionPeriod,
					dataGroupingCode: RefDataGroupingCodes.EuropeanUnionEUN,
					effectiveDate: ZDate.Today,
					value: false
				);
				nctsBill.Validation.ValidateAll();
				AssertNoRowMessageError("R0364-, B1964+, NC5TP-, expecting: R0364 INACTIVATE.", nctsBill, r0364Text);

				deciderTestContext.EnableRule(decider => decider.IsRuleR0364Active);
				nctsBill.Validation.ValidateAll();
				AssertHasRowMessageError("R0364+, B1964+, NC5TP-, expecting: R0364 ACTIVATE.", nctsBill, r0364Text);

				using var nctsTransitionPeriodReActivate = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
					code: Constants.FunctionalityTypes.NCTSTransitionPeriod,
					dataGroupingCode: RefDataGroupingCodes.EuropeanUnionEUN,
					effectiveDate: ZDate.Today,
					value: true
				);
				deciderTestContext.DisableRule(decider => decider.IsRuleB1964Active);
				AssertHasRowMessageError("R0364+, B1964-, NC5TP+, expecting: R0364 ACTIVATE.", nctsBill, r0364Text);

				var normalPackage2 = goodsItem.Packages.AddNew();
				normalPackage2.B5_UnitType = "1x";
				normalPackage2.B5_MarksAndNumbers = "MARK001";
				normalPackage2.B5_UnitCount = 1;
				nctsBill.Validation.ValidateAll();
				AssertNoRowMessageError("R0364: non-empty package with same MarksAndNumbers, validation passes.", nctsBill, r0364Text);

				normalPackage2.B5_MarksAndNumbers = "MARK002";
				nctsBill.Validation.ValidateAll();
				AssertHasRowMessageError("R0364: non-empty package with different MarksAndNumbers, validation fails.", nctsBill, r0364Text);

				var bulkPackage = goodsItem.Packages.AddNew();
				bulkPackage.B5_UnitType = bulkType;
				bulkPackage.B5_MarksAndNumbers = "MARK001";
				bulkPackage.B5_UnitCount = 1;
				nctsBill.Validation.ValidateAll();
				AssertHasRowMessageError("R0364: BULK packages does not effect result", nctsBill, r0364Text);

				var unPackedPackage = goodsItem.Packages.AddNew();
				unPackedPackage.B5_UnitType = unPackedType;
				unPackedPackage.B5_MarksAndNumbers = "MARK001";
				unPackedPackage.B5_UnitCount = 1;
				nctsBill.Validation.ValidateAll();
				AssertHasRowMessageError("R0364: UNPACKED packages does not effect result.", nctsBill, r0364Text);
			});
		}

		public void TestCheckSecondDepartureTransportMeansNationality_ValidCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR("NCNAT", "EUN");
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(nctsBill.SecondDepartureTransportMeansNationalityInfo, "XX", "DE");
		}

		public void TestCheckSecondDepartureTransportMeansNationality_WhenTR0058Active()
		{
			var expectedError = "[TR0058] You have not entered";
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					deciderTestContext.EnableRule(c => c.IsRuleTR0058Active);
					bill.Trailer1IDAtDeparture = "ID123";
					bill.Validation.ValidateSecondDepartureTransportMeansNationality();
					AssertHasMessageErrorContaining("Nationality empty", bill.SecondDepartureTransportMeansNationalityInfo, expectedError);
					bill.SecondDepartureTransportMeansNationality = "DE";
					bill.Validation.ValidateSecondDepartureTransportMeansNationality();
					AssertNoMessageErrorContaining("Nationality not empty", bill.SecondDepartureTransportMeansNationalityInfo, expectedError);
					bill.Trailer1IDAtDeparture = ZString.Empty;
					bill.SecondDepartureTransportMeansNationality = ZString.Empty;
					bill.Validation.ValidateSecondDepartureTransportMeansNationality();
					AssertNoMessageErrorContaining("Nationality And Transport empty", bill.SecondDepartureTransportMeansNationalityInfo, expectedError);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					bill.Trailer1IDAtDeparture = "ID123";
					bill.Validation.ValidateSecondDepartureTransportMeansNationality();
					AssertNoMessageErrorContaining("Nationality empty", bill.SecondDepartureTransportMeansNationalityInfo, expectedError);
				}
			});
		}

		public void TestCheckSecondDepartureTransportMeansNationality_WhenTR0058NotActive()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					deciderTestContext.DisableRule(c => c.IsRuleTR0058Active);
					bill.Trailer1IDAtDeparture = "ID123";
					bill.Validation.ValidateSecondDepartureTransportMeansNationality();
					AssertNoNotifications("Nationality", bill.SecondDepartureTransportMeansNationalityInfo);
				}
			});
		}

		public void TestCheckSecondDepartureTransportMeansNationality_WhenR0474Active()
		{
			const string expectedError = "[R0474] This field can be filled only if 'Transport ID' is present";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(c => c.IsRuleR0474Active);

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
					{
						nctsBill.SecondDepartureTransportMeansNationality = "DE";
						nctsBill.Validation.ValidateSecondDepartureTransportMeansNationality();
						AssertHasMessageErrorContaining("Trailer1 Nationality is filled but Transport ID, Type, Nationality is empty", nctsBill.SecondDepartureTransportMeansNationalityInfo, expectedError);

						nctsBill.TransportTypeAtDeparture = "30";
						AssertHasMessageErrorContaining("Trailer1 Nationality is filled, Transport Type is filled but Transport ID, Nationality is empty", nctsBill.SecondDepartureTransportMeansNationalityInfo, expectedError);

						nctsBill.TransportAtDeparture = "123";
						AssertHasMessageErrorContaining("Trailer1 Nationality is filled, Transport Type and Transport ID is filled but Transport Nationality is empty", nctsBill.SecondDepartureTransportMeansNationalityInfo, expectedError);

						nctsBill.FirstDepartureTransportMeansNationality = "DE";
						nctsBill.Validation.ValidateSecondDepartureTransportMeansNationality();
						AssertNoMessageErrorContaining("Trailer1 Nationality is filled and Transport ID, Type and Nationality is filled", nctsBill.SecondDepartureTransportMeansNationalityInfo, expectedError);
					}

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
					{
						nctsBill.SecondDepartureTransportMeansNationality = "DE";
						nctsBill.Validation.ValidateSecondDepartureTransportMeansNationality();
						AssertNoMessageErrorContaining("Trailer1 Nationality is filled but Transport ID, Type, Nationality is empty", nctsBill.SecondDepartureTransportMeansNationalityInfo, expectedError);
					}
				}
			});
		}

		public void TestCheckSecondDepartureTransportMeansNationality_WhenR0474NotActive()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR("NCNAT", "EUN");
			Factory.Save();

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.DisableRule(c => c.IsRuleR0474Active);
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
					{
						nctsBill.SecondDepartureTransportMeansNationality = "DE";
						nctsBill.Validation.ValidateSecondDepartureTransportMeansNationality();
						AssertNoNotifications("Trailer1 Nationality is filled but Transport ID, Type, Nationality is empty", nctsBill.SecondDepartureTransportMeansNationalityInfo);
					}
				}
			});
		}

		public void TestCheckSecondDepartureTransportMeansID_WhenRuleR0474Active()
		{
			const string expectedError = "[R0474] This field can be filled only if 'Transport ID' is present";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(c => c.IsRuleR0474Active);

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
					{
						nctsBill.SecondDepartureTransportMeansID = "123";
						nctsBill.Validation.ValidateSecondDepartureTransportMeansID();
						AssertHasMessageErrorContaining("Trailer1 ID is filled but Transport ID, Type, Nationality is empty", nctsBill.SecondDepartureTransportMeansIDInfo, expectedError);

						nctsBill.TransportTypeAtDeparture = "30";
						nctsBill.Validation.ValidateSecondDepartureTransportMeansID();
						AssertNoMessageError("Trailer1 ID is filled and Transport Type is filled", nctsBill.SecondDepartureTransportMeansIDInfo, expectedError);

						nctsBill.TransportTypeAtDeparture = ZString.Empty;
						nctsBill.TransportAtDeparture = "123";
						nctsBill.Validation.ValidateSecondDepartureTransportMeansID();
						AssertNoMessageError("Trailer1 ID is filled and Transport ID is filled", nctsBill.SecondDepartureTransportMeansIDInfo, expectedError);

						nctsBill.TransportAtDeparture = ZString.Empty;
						nctsBill.FirstDepartureTransportMeansNationality = "AU";
						nctsBill.Validation.ValidateSecondDepartureTransportMeansID();
						AssertNoMessageError("Trailer1 ID is filled and Nationality is filled", nctsBill.SecondDepartureTransportMeansIDInfo, expectedError);
					}

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
					{
						nctsBill.SecondDepartureTransportMeansID = "123";
						nctsBill.Validation.ValidateSecondDepartureTransportMeansID();
						AssertNoMessageError("Trailer1 ID is filled but Transport ID, Type, Nationality is empty", nctsBill.SecondDepartureTransportMeansIDInfo, expectedError);
					}
				}
			});
		}

		public void TestCheckSecondDepartureTransportMeansID_WhenRuleR0474NotActive()
		{
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.DisableRule(c => c.IsRuleR0474Active);
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
					{
						nctsBill.SecondDepartureTransportMeansID = "123";
						nctsBill.Validation.ValidateSecondDepartureTransportMeansID();
						AssertNoNotifications("Trailer1 ID is filled but Transport ID, Type, Nationality is empty", nctsBill.SecondDepartureTransportMeansIDInfo);
					}
				}
			});
		}

		public void TestCheckThirdDepartureTransportMeansNationality_ValidCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR("NCNAT", "EUN");
			Factory.Save();

			ValidationTestHelper.AssertInvalidCodeMessageError(nctsBill.ThirdDepartureTransportMeansNationalityInfo, "XX", "DE");
		}

		public void TestCheckThirdDepartureTransportMeansNationality_WhenTR0058Active()
		{
			var expectedError = "[TR0058] You have not entered";
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					deciderTestContext.EnableRule(c => c.IsRuleTR0058Active);
					bill.Trailer2IDAtDeparture = "ID123";
					bill.Validation.ValidateThirdDepartureTransportMeansNationality();
					AssertHasMessageErrorContaining("Nationality empty", bill.ThirdDepartureTransportMeansNationalityInfo, expectedError);
					bill.ThirdDepartureTransportMeansNationality = "DE";
					bill.Validation.ValidateThirdDepartureTransportMeansNationality();
					AssertNoMessageErrorContaining("Nationality not empty", bill.ThirdDepartureTransportMeansNationalityInfo, expectedError);
					bill.Trailer2IDAtDeparture = ZString.Empty;
					bill.ThirdDepartureTransportMeansNationality = ZString.Empty;
					bill.Validation.ValidateThirdDepartureTransportMeansNationality();
					AssertNoMessageErrorContaining("Nationality And Transport empty", bill.ThirdDepartureTransportMeansNationalityInfo, expectedError);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					bill.Trailer2IDAtDeparture = "ID123";
					bill.Validation.ValidateThirdDepartureTransportMeansNationality();
					AssertNoMessageErrorContaining("Nationality empty", bill.ThirdDepartureTransportMeansNationalityInfo, expectedError);
				}
			});
		}

		public void TestCheckThirdDepartureTransportMeansNationality_WhenTR0058NotActive()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = header.Bills.AddNew();
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					deciderTestContext.DisableRule(c => c.IsRuleTR0058Active);
					bill.Trailer2IDAtDeparture = "ID123";
					bill.Validation.ValidateThirdDepartureTransportMeansNationality();
					AssertNoNotifications("Nationality", bill.ThirdDepartureTransportMeansNationalityInfo);
				}
			});
		}

		public void TestCheckThirdDepartureTransportMeansNationality_WhenR0474Active()
		{
			const string expectedError = "[R0474] This field can be filled only if 'Transport ID' is present";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(c => c.IsRuleR0474Active);
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
					{
						nctsBill.ThirdDepartureTransportMeansNationality = "DE";
						nctsBill.Validation.ValidateThirdDepartureTransportMeansNationality();
						AssertHasMessageErrorContaining("Trailer2 Nationality is filled but Transport ID, Type, Nationality is empty", nctsBill.ThirdDepartureTransportMeansNationalityInfo, expectedError);

						nctsBill.TransportTypeAtDeparture = "30";
						AssertHasMessageErrorContaining("Trailer2 Nationality is filled, Transport Type is filled but Transport ID, Nationality is empty", nctsBill.ThirdDepartureTransportMeansNationalityInfo, expectedError);

						nctsBill.TransportAtDeparture = "123";
						AssertHasMessageErrorContaining("Trailer2 Nationality is filled, Transport Type and Transport ID is filled but Transport Nationality is empty", nctsBill.ThirdDepartureTransportMeansNationalityInfo, expectedError);

						nctsBill.FirstDepartureTransportMeansNationality = "DE";
						nctsBill.Validation.ValidateThirdDepartureTransportMeansNationality();
						AssertNoMessageErrorContaining("Trailer2 Nationality is filled and Transport ID, Type and Nationality is filled", nctsBill.ThirdDepartureTransportMeansNationalityInfo, expectedError);
					}

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
					{
						nctsBill.ThirdDepartureTransportMeansNationality = "DE";
						nctsBill.Validation.ValidateSecondDepartureTransportMeansNationality();
						AssertNoMessageErrorContaining("Trailer2 Nationality is filled but Transport ID, Type, Nationality is empty", nctsBill.ThirdDepartureTransportMeansNationalityInfo, expectedError);
					}
				}
			});
		}

		public void TestCheckThirdDepartureTransportMeansNationality_WhenR0474NotActive()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCountryListOfAU_DE_FR("NCNAT", "EUN");
			Factory.Save();

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.DisableRule(c => c.IsRuleR0474Active);
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
					{
						nctsBill.ThirdDepartureTransportMeansNationality = "DE";
						nctsBill.Validation.ValidateThirdDepartureTransportMeansNationality();
						AssertNoNotifications("Trailer2 Nationality is filled but Transport ID, Type, Nationality is empty", nctsBill.ThirdDepartureTransportMeansNationalityInfo);
					}
				}
			});
		}

		public void TestCheckThirdDepartureTransportMeansID_WhenRuleR0474Active()
		{
			const string expectedError = "[R0474] This field can be filled only if 'Transport ID' is present";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(c => c.IsRuleR0474Active);
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
					{
						nctsBill.ThirdDepartureTransportMeansID = "123";
						nctsBill.Validation.ValidateThirdDepartureTransportMeansID();
						AssertHasMessageErrorContaining("Trailer2 ID is filled but Transport ID, Type, Nationality is empty", nctsBill.ThirdDepartureTransportMeansIDInfo, expectedError);

						nctsBill.TransportTypeAtDeparture = "30";
						nctsBill.Validation.ValidateThirdDepartureTransportMeansID();
						AssertNoMessageError("Trailer2 ID is filled and Transport Type is filled", nctsBill.ThirdDepartureTransportMeansIDInfo, expectedError);

						nctsBill.TransportTypeAtDeparture = ZString.Empty;
						nctsBill.TransportAtDeparture = "123";
						nctsBill.Validation.ValidateThirdDepartureTransportMeansID();
						AssertNoMessageError("Trailer2 ID is filled and Transport ID is filled", nctsBill.ThirdDepartureTransportMeansIDInfo, expectedError);

						nctsBill.TransportAtDeparture = ZString.Empty;
						nctsBill.FirstDepartureTransportMeansNationality = "DE";
						nctsBill.Validation.ValidateThirdDepartureTransportMeansID();
						AssertNoMessageError("Trailer2 ID is filled and Nationality is filled", nctsBill.ThirdDepartureTransportMeansIDInfo, expectedError);
					}

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
					{
						nctsBill.ThirdDepartureTransportMeansID = "123";
						nctsBill.Validation.ValidateThirdDepartureTransportMeansID();
						AssertNoMessageError("Trailer2 ID is filled but Transport ID, Type, Nationality is empty", nctsBill.ThirdDepartureTransportMeansIDInfo, expectedError);
					}
				}
			});
		}

		public void TestCheckThirdDepartureTransportMeansID_WhenRuleR0474NotActive()
		{
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					deciderTestContext.DisableRule(c => c.IsRuleR0474Active);
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
					{
						nctsBill.ThirdDepartureTransportMeansID = "123";
						nctsBill.Validation.ValidateThirdDepartureTransportMeansID();
						AssertNoNotifications("Trailer2 ID is filled but Transport ID, Type, Nationality is empty", nctsBill.ThirdDepartureTransportMeansIDInfo);
					}
				}
			});
		}

		public void TestCheckTransportTypeAtDeparture_ValidCode()
		{
			ValidationTestHelper.AssertErrorIfInvalidCode(nctsBill.TransportTypeAtDepartureInfo, "00", "10");
		}

		public void TestCheckTransportTypeAtDeparture_WhenRule0078Active()
		{
			var expectedError = $"[TR0078] You have not entered a {nctsBill.TransportTypeAtDepartureInfo.HumanReadableName}.";
			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					deciderTestContext.EnableRule(decider => decider.IsRuleTR0078Active);
					nctsBill.TransportTypeAtDeparture = "10";
					nctsBill.Validation.ValidateTransportTypeAtDeparture();
					AssertNoMessageError("Transport Type is filled", nctsBill.TransportTypeAtDepartureInfo, expectedError);

					nctsBill.TransportTypeAtDeparture = ZString.Empty;
					nctsBill.TransportAtDeparture = "123";
					nctsBill.Validation.ValidateTransportTypeAtDeparture();
					AssertHasMessageErrorContaining("Transport Type is empty but Transport ID is filled", nctsBill.TransportTypeAtDepartureInfo, expectedError);

					nctsBill.TransportAtDeparture = ZString.Empty;
					nctsBill.FirstDepartureTransportMeansNationality = "DE";
					nctsBill.Validation.ValidateTransportTypeAtDeparture();
					AssertHasMessageErrorContaining("Transport Type is empty but Nationaility is filled", nctsBill.TransportTypeAtDepartureInfo, expectedError);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: true))
				{
					nctsBill.TransportTypeAtDeparture = ZString.Empty;
					nctsBill.TransportAtDeparture = "123";
					nctsBill.Validation.ValidateTransportTypeAtDeparture();
					AssertNoMessageError("Transport Type is empty but Transport ID is filled", nctsBill.TransportTypeAtDepartureInfo, expectedError);
				}
			});
		}

		public void TestCheckTransportTypeAtDeparture_WhenRule0078NotActive()
		{
			var expectedError = $"[TR0078] You have not entered a {nctsBill.TransportTypeAtDepartureInfo.HumanReadableName}.";
			CombineAssertions(() =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					deciderTestContext.DisableRule(decider => decider.IsRuleTR0078Active);
					nctsBill.TransportTypeAtDeparture = ZString.Empty;
					nctsBill.TransportAtDeparture = "123";
					nctsBill.Validation.ValidateTransportTypeAtDeparture();
					AssertNoNotifications("Transport Type is empty but Transport ID is filled", nctsBill.TransportTypeAtDepartureInfo);
				}
			});
		}

		public void TestCheckFirstDepartureTransportMeansID_WhenR0474_1ActiveAndNotInTransitionPeriod()
		{
			var expectedMessageError = "[R0474-1] Transport ID is mandatory if at least a Trailer ID is present.";
			var targetInfo = nctsBill.TransportAtDepartureInfo;

			CombineAssertions("When rule R0474_1 is active, TransitionPeriod is false", () =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false))
				{
					deciderTestContext.EnableRule(c => c.IsRuleR0474_1Active);
					AssertEquals("Transport ID, Trailer1 ID and Trailer2 ID are not readonly", expected: false, nctsBill.IsTransportDepartureReadOnly);

					nctsHeader.MovementHeader.BM_InlandTransportMode = "3";
					nctsBill.Trailer1IDAtDeparture = "XYZ";
					nctsBill.Trailer2IDAtDeparture = "DEF";
					nctsBill.TransportAtDeparture = ZString.Empty;
					nctsBill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageError("Inland M.O.T is set 3, trailer1Id and trailer2Id are filled, TransportId is empty", targetInfo, expectedMessageError);

					nctsBill.Trailer2IDAtDeparture = ZString.Empty;
					nctsBill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageError("Inland M.O.T is set 3, trailer1Id is filled and trailer2Id is empty, TransportId is empty", targetInfo, expectedMessageError);

					nctsBill.Trailer2IDAtDeparture = "DEF";
					nctsBill.Trailer1IDAtDeparture = ZString.Empty;
					nctsBill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertHasMessageError("Inland M.O.T is set 3, trailer1Id is empty and trailer2Id is filled, TransportId is empty", targetInfo, expectedMessageError);
				}
			});
		}

		public void TestCheckFirstDepartureTransportMeansID_WhenR0474_1ActiveAndInTransitionPeriod()
		{
			var targetInfo = nctsBill.TransportAtDepartureInfo;

			CombineAssertions("When rule R0474_1 is active, TransitionPeriod is true", () =>
			{
				using (var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					deciderTestContext.EnableRule(c => c.IsRuleR0474_1Active);
					AssertEquals("Transport ID, Trailer1 ID and Trailer2 ID are readonly", expected: true, nctsBill.IsTransportDepartureReadOnly);

					nctsHeader.MovementHeader.BM_InlandTransportMode = "3";
					nctsBill.TransportAtDeparture = ZString.Empty;
					nctsBill.Trailer1IDAtDeparture = "XYZ";
					nctsBill.Trailer2IDAtDeparture = "DEF";
					nctsBill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("Inland M.O.T is set 3, trailer1Id and trailer2Id are filled, TransportId is empty", targetInfo);

					nctsBill.Trailer2IDAtDeparture = ZString.Empty;
					nctsBill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("Inland M.O.T is set 3, trailer1Id is filled and trailer2Id is empty, TransportId is empty", targetInfo);

					nctsBill.Trailer2IDAtDeparture = "DEF";
					nctsBill.Trailer1IDAtDeparture = ZString.Empty;
					nctsBill.Validation.ValidateFirstDepartureTransportMeansID();
					AssertNoNotifications("Inland M.O.T is set 3, trailer1Id is empty and trailer2Id is filled, TransportId is empty", targetInfo);
				}
			});
		}

		public void TestCheckFirstDepartureTransportMeansID_WhenR0474_1NotActive()
		{
			var targetInfo = nctsBill.TransportAtDepartureInfo;

			CombineAssertions("When rule R0474_1 is inactive, TransitionPeriod is false", () =>
			{
				using var deciderTestContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
				deciderTestContext.DisableRule(c => c.IsRuleR0474_1Active);
				nctsHeader.MovementHeader.BM_InlandTransportMode = "3";
				nctsBill.TransportAtDeparture = ZString.Empty;
				nctsBill.Trailer1IDAtDeparture = "XYZ";
				nctsBill.Trailer2IDAtDeparture = "DEF";
				nctsBill.Validation.ValidateFirstDepartureTransportMeansID();
				AssertNoNotifications("Inland M.O.T is set 3, trailer1Id and trailer2Id are filled, TransportId is empty", targetInfo);

				nctsBill.Trailer2IDAtDeparture = ZString.Empty;
				nctsBill.Validation.ValidateFirstDepartureTransportMeansID();
				AssertNoNotifications("Inland M.O.T is set 3, trailer1Id is filled and trailer2Id is empty, TransportId is empty", targetInfo);

				nctsBill.Trailer2IDAtDeparture = "DEF";
				nctsBill.Trailer1IDAtDeparture = ZString.Empty;
				nctsBill.Validation.ValidateFirstDepartureTransportMeansID();
				AssertNoNotifications("Inland M.O.T is set 3, trailer1Id is empty and trailer2Id is filled, TransportId is empty", targetInfo);
			});
		}

		public void TestCheckB0_RX_NKLinePriceCurrency()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsBill.B0_RX_NKLinePriceCurrencyInfo, "XX", CurrencyCodes.Germany);
		}

		public void TestValidateRuleTR0094()
		{
			var errorMessage = "[TR0094] House Consignment must contain at least one Goods Item.";
			var bill = nctsHeader.Bills.AddNew();

			using var ruleContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory);
			CombineAssertions(() =>
			{
				ruleContext.EnableRule(c => c.IsRuleTR0094Active);
				bill.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(bill, errorMessage);

				bill.GoodsItems.AddNew();
				bill.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(bill, errorMessage);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsBill = nctsHeader.Bills.AddNew();
		}
		NctsHeader nctsHeader;
		NctsBill nctsBill;
	}
}
