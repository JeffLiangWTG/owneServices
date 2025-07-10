using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Core.Constants.CountryCodes;
using static Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes.Codes;
using Constants = Enterprise.Customs.Universal.Constants;
using RefCusCodeListTypes = Enterprise.Customs.EU.NCTS.Business.UniversalReferenceConstants.RefCusCodeListTypes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsDepartureCargoDescPhase5ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBY_CustomsThirdQuantity()
		{
			NCTSTestHelper.AssertCheckBY_CustomsThirdQuantity_RuleTR0084(Factory, goodsItem);
		}

		public void TestCheckBY_CustomsFourthQuantity()
		{
			NCTSTestHelper.AssertCheckBY_CustomsFourthQuantity_RuleTR0084(Factory, goodsItem);
		}

		public void TestCheckBY_FormattedHarmonisedTariff_MustBe_6_Or_8_Or_10_digits()
		{
			var info = goodsItem.BY_FormattedHarmonisedTariffInfo;
			var message = $"{info.HumanReadableName} must be 6/8/10 digits";
			CombineAssertions(() =>
			{
				goodsItem.BY_HarmonisedTariff = new string('x', 5);
				validation.ValidateBY_HarmonisedTariff();
				AssertHasMessageError($"5 digits", info, message);

				goodsItem.BY_HarmonisedTariff = new string('x', 6);
				validation.ValidateBY_HarmonisedTariff();
				AssertNoMessageError($"6 digits", info, message);

				goodsItem.BY_HarmonisedTariff = new string('x', 7);
				validation.ValidateBY_HarmonisedTariff();
				AssertHasMessageError($"7 digits", info, message);

				goodsItem.BY_HarmonisedTariff = new string('x', 8);
				validation.ValidateBY_HarmonisedTariff();
				AssertNoMessageError($"8 digits", info, message);

				goodsItem.BY_HarmonisedTariff = new string('x', 9);
				validation.ValidateBY_HarmonisedTariff();
				AssertHasMessageError($"9 digits", info, message);

				goodsItem.BY_HarmonisedTariff = new string('x', 10);
				validation.ValidateBY_HarmonisedTariff();
				AssertNoMessageError($"10 digits", info, message);

				goodsItem.BY_HarmonisedTariff = new string('x', 11);
				validation.ValidateBY_HarmonisedTariff();
				AssertHasMessageError($"11 digits", info, message);
			});
		}

		public void TestCheckBY_HarmonisedTariff_6Digit_inWCO() => CheckBY_HarmonisedTariff_InDataGroup("123456", Constants.TariffTypes.HarmonizedSystem, RefDataGroupingCodes.WorldCustomsOrganisationWCO, validCode: "013456");
		public void TestCheckBY_HarmonisedTariff_8Digit_inEUN() => CheckBY_HarmonisedTariff_InDataGroup("12345678", Constants.TariffTypes.Export, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, validCode: "01345678");
		public void TestCheckBY_HarmonisedTariff_10Digit_inEUN() => CheckBY_HarmonisedTariff_InDataGroup("1234567890", Constants.TariffTypes.Import, RefDataGroupingCodes.EuropeanUnionEUN, validCode: "0134567890");
		void CheckBY_HarmonisedTariff_InDataGroup(string tariffCode, string tariffType, string refDataGroupingCode, string validCode)
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);

			if (refDataGroupingCode == RefDataGroupingCodes.EuropeanUnionEUN)
			{
				var grouping = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN);
				helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, parent: grouping);
			}
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var tariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, refDataGroupingCode, tariffType).PK;
			helper.LoadOrCreateNewTariff(refDataGroupingCode, tariffTypePK, validCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			goodsItem.BY_HarmonisedTariff = tariffCode;
			validation.ValidateBY_HarmonisedTariff();
			ValidationTestHelper.AssertInvalidCodeMessageError(goodsItem.BY_HarmonisedTariffInfo, invalidCode: tariffCode, validCode);
		}

		public void TestCheckBY_TransportChargesMethodOfPayment_WhenRuleB2400_1Active()
		{
			const string errorMessage = "[B2400-1] Transport MoP field must be empty";
			CombineAssertions(() =>
			{
				using var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory);
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(x => x.IsRuleB2400_1Active);

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					goodsItem.BY_TransportChargesMethodOfPayment = "A";
					AssertHasMessageError("When Transport MoP is filled and not in transition period", goodsItem.BY_TransportChargesMethodOfPaymentInfo, errorMessage);

					goodsItem.BY_TransportChargesMethodOfPayment = ZString.Empty;
					AssertNoMessageError("When Transport MoP is not filled and not in transition period", goodsItem.BY_TransportChargesMethodOfPaymentInfo, errorMessage);
				}

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					goodsItem.BY_TransportChargesMethodOfPayment = "A";
					AssertNoMessageError("When Transport MoP is filled and in transition period", goodsItem.BY_TransportChargesMethodOfPaymentInfo, errorMessage);

					goodsItem.BY_TransportChargesMethodOfPayment = ZString.Empty;
					AssertNoMessageError("When Transport MoP is not filled and in transition period", goodsItem.BY_TransportChargesMethodOfPaymentInfo, errorMessage);
				}
			});
		}

		public void TestCheckBY_FormattedHarmonisedTariff_NR0024()
		{
			const string messageError = "[NR0024] This field must be filled";

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				goodsItem.BY_HarmonisedTariff = "1234567";

				CombineAssertions(() =>
				{
					deciderTestContext.EnableRule(c => c.IsRuleNR0024Active);
					validation.ValidateBY_HarmonisedTariff();
					AssertNoMessageError("When EnableRule IsRuleNR0024Active and Commodity Code is not empty, no error", goodsItem.BY_FormattedHarmonisedTariffInfo, messageError);

					goodsItem.BY_HarmonisedTariff = ZString.Empty;
					validation.ValidateBY_HarmonisedTariff();
					AssertHasMessageErrorContaining("When EnableRule IsRuleNR0024Active and Commodity Code is empty, error", goodsItem.BY_FormattedHarmonisedTariffInfo, messageError);

					deciderTestContext.DisableRule(c => c.IsRuleNR0024Active);
					validation.ValidateBY_HarmonisedTariff();
					AssertNoMessageError("When DisableRule IsRuleNR0024Active and Commodity Code is empty, no error", goodsItem.BY_FormattedHarmonisedTariffInfo, messageError);
				});
			}
		}

		public void TestCheckBY_RN_NKCountryOfDestination_CheckRuleR0507()
		{
			AssertRuleR0507(x => x.BY_RN_NKCountryOfDestinationInfo);
		}

		public void TestCheckBY_RN_NKCountryOfDispatch_E1301()
		{
			const string message = "[E1301] In transition period, which is now, Country of Dispatch must be empty";
			var targetInfo = goodsItem.BY_RN_NKCountryOfDispatchInfo;

			using (var testContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				testContext.ClearCachedValidationDecider(goodsItem);
				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					testContext.EnableRule(decider => decider.IsRuleE1301Active);

					CombineAssertions(() =>
					{
						goodsItem.BY_RN_NKCountryOfDispatch = string.Empty;
						AssertNoMessageError("Dispatch Country not specified", targetInfo, message);

						goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
						AssertHasMessageError("Dispatch Country specified on goods item level", targetInfo, message);
					});

					testContext.DisableRule(decider => decider.IsRuleE1301Active);

					CombineAssertions(() =>
					{
						goodsItem.BY_RN_NKCountryOfDispatch = string.Empty;
						AssertNoMessageError("Dispatch Country not specified", targetInfo, message);

						goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
						AssertNoMessageError("Dispatch Country specified on goods item level", targetInfo, message);
					});
				}

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					testContext.EnableRule(decider => decider.IsRuleE1301Active);

					CombineAssertions(() =>
					{
						goodsItem.BY_RN_NKCountryOfDispatch = string.Empty;
						AssertNoMessageError("Dispatch Country not specified", targetInfo, message);

						goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
						AssertNoMessageError("Dispatch Country specified on goods item level", targetInfo, message);
					});

					testContext.DisableRule(decider => decider.IsRuleE1301Active);

					CombineAssertions(() =>
					{
						goodsItem.BY_RN_NKCountryOfDispatch = string.Empty;
						AssertNoMessageError("Dispatch Country not specified", targetInfo, message);

						goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
						AssertNoMessageError("Dispatch Country specified on goods item level", targetInfo, message);
					});
				}
			}
		}

		public void TestCheckBY_RN_NKCountryOfDispatch_C0909_1_WhenInTransitionPeriod()
		{
			const string message = "[C0909-1] Dispatch Country/Region must be filled either on Declaration or Goods Item Level.";
			var targetInfo = goodsItem.BY_RN_NKCountryOfDispatchInfo;
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(c => c.IsRuleC0909_1Active);

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					CombineAssertions(() =>
					{
						goodsItem.BY_RN_NKCountryOfDispatch = string.Empty;
						AssertHasMessageError("Dispatch Country not specified", targetInfo, message);

						deciderTestContext.DisableRule(c => c.IsRuleC0909_1Active);

						goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
						AssertNoMessageError("Rule C0909_1 is disabled", targetInfo, message);

						deciderTestContext.EnableRule(c => c.IsRuleC0909_1Active);

						goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
						AssertNoMessageError("Dispatch Country specified on goods item level", targetInfo, message);

						goodsItem.BY_RN_NKCountryOfDispatch = string.Empty;
						movementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
						goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
						AssertNoMessageError("Dispatch Country specified on declaration level", targetInfo, message);
					});
				}
			}
		}

		public void TestCheckBY_RN_NKCountryOfDispatch_C0909_1_WhenOutsideTransitionPeriod()
		{
			const string message = "[C0909-1] Dispatch Country/Region must be filled either on Declaration, House Consignment or Goods Item Level.";
			var targetInfo = goodsItem.BY_RN_NKCountryOfDispatchInfo;
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(c => c.IsRuleC0909_1Active);

				CombineAssertions(() =>
				{
					goodsItem.BY_RN_NKCountryOfDispatch = string.Empty;
					AssertHasMessageError("Dispatch Country not specified", targetInfo, message);

					deciderTestContext.DisableRule(c => c.IsRuleC0909_1Active);

					goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
					AssertNoMessageError("Rule C0909_1 is disabled", targetInfo, message);

					deciderTestContext.EnableRule(c => c.IsRuleC0909_1Active);

					goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
					AssertNoMessageError("Dispatch Country specified on goods item level", targetInfo, message);

					goodsItem.BY_RN_NKCountryOfDispatch = "";
					movementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.China;
					goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
					AssertNoMessageError("Dispatch Country specified on declaration level", targetInfo, message);

					movementHeader.BM_RN_NKCountryOfDispatch = string.Empty;
					goodsItem.Bill.B0_RN_NKCountryOfExport = Core.Constants.CountryCodes.China;
					goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
					AssertNoMessageError("Dispatch Country specified on house consignment level", targetInfo, message);
				});
			}
		}

		public void TestCheckBY_RN_NKCountryOfDestination_C0343_1_WhenInTransitionPeriod()
		{
			const string message = "[C0343-1] Destination Country/Region must be filled either on Declaration or Goods Item Level.";
			var targetInfo = goodsItem.BY_RN_NKCountryOfDestinationInfo;
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(c => c.IsRuleC0343_1Active);

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					CombineAssertions(() =>
					{
						goodsItem.BY_RN_NKCountryOfDestination = string.Empty;
						AssertHasMessageError("Destination Country not specified", targetInfo, message);

						deciderTestContext.DisableRule(c => c.IsRuleC0343_1Active);
						goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();
						AssertNoMessageError("Rule C0343_1 is disabled", targetInfo, message);
						deciderTestContext.EnableRule(c => c.IsRuleC0343_1Active);

						goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
						AssertNoMessageError("Destination Country specified on goods item level", targetInfo, message);

						goodsItem.BY_RN_NKCountryOfDestination = string.Empty;
						movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.China;
						goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();
						AssertNoMessageError("Destination Country specified on declaration level", targetInfo, message);

						movementHeader.BM_RL_NKDestinationPort = ZString.Empty;
						goodsItem.Bill.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
						goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();
						AssertHasMessageError("Destination Country specified on house consignment level is being ignored", targetInfo, message);
					});
				}
			}
		}

		public void TestCheckBY_RN_NKCountryOfDestination_C0343_1_WhenOutsideTransitionPeriod()
		{
			const string message = "[C0343-1] Destination Country/Region must be filled either on Declaration, House Consignment or Goods Item Level.";
			var targetInfo = goodsItem.BY_RN_NKCountryOfDestinationInfo;
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(c => c.IsRuleC0343_1Active);

				CombineAssertions(() =>
				{
					goodsItem.BY_RN_NKCountryOfDestination = string.Empty;
					AssertHasMessageError("Destination Country not specified", targetInfo, message);

					deciderTestContext.DisableRule(c => c.IsRuleC0343_1Active);
					goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();
					AssertNoMessageError("Rule C0343_1 is disabled", targetInfo, message);
					deciderTestContext.EnableRule(c => c.IsRuleC0343_1Active);

					goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
					AssertNoMessageError("Destination Country specified on goods item level", targetInfo, message);

					goodsItem.BY_RN_NKCountryOfDestination = string.Empty;
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.China;
					goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();
					AssertNoMessageError("Destination Country specified on declaration level", targetInfo, message);

					movementHeader.BM_RL_NKDestinationPort = string.Empty;
					goodsItem.Bill.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
					goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();
					AssertNoMessageError("Destination Country specified on house consignment level", targetInfo, message);
				});
			}
		}

		public void TestCheckBY_RN_NKCountryOfDestination_C0343_2()
		{
			const string message = "[C0343-2] Destination Country/Region must be filled either on Declaration, House Consignment or Goods Item Level.";
			var targetInfo = goodsItem.BY_RN_NKCountryOfDestinationInfo;
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(c => c.IsRuleC0343_2Active);

				CombineAssertions(() =>
				{
					goodsItem.BY_RN_NKCountryOfDestination = string.Empty;
					AssertHasMessageError("Destination Country not specified", targetInfo, message);

					deciderTestContext.DisableRule(c => c.IsRuleC0343_2Active);
					goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();
					AssertNoMessageError("Rule C0343_2 is disabled", targetInfo, message);
					deciderTestContext.EnableRule(c => c.IsRuleC0343_2Active);

					goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
					AssertNoMessageError("Destination Country specified on goods item level", targetInfo, message);

					goodsItem.BY_RN_NKCountryOfDestination = string.Empty;
					movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.China;
					goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();
					AssertNoMessageError("Destination Country specified on declaration level", targetInfo, message);

					movementHeader.BM_RL_NKDestinationPort = string.Empty;
					goodsItem.Bill.B0_RN_NKCountryOfDestination = Core.Constants.CountryCodes.China;
					goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();
					AssertNoMessageError("Destination Country specified on house consignment level", targetInfo, message);
				});
			}
		}

		public void TestCheckBY_RN_NKCountryOfDispatch_CheckRuleR0507()
		{
			AssertRuleR0507(x => x.BY_RN_NKCountryOfDispatchInfo);
		}

		public void TestCheckRuleC0909_WhenActive()
		{
			const string message = "[C0909] You have not entered Country Of Dispatch. It is required either on Declaration or House Consignment or House Consignment Item.";
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(c => c.IsRuleC0909Active);
					movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T2;
					movementHeader.BM_RN_NKCountryOfDispatch = CountryCodes.China;
					goodsItem.Bill.B0_RN_NKCountryOfExport = CountryCodes.China;
					goodsItem.BY_RN_NKCountryOfDispatch = CountryCodes.China;
					AssertNoMessageError("MovementHeader's BM_InBondEntryType is not TIR", goodsItem.BY_RN_NKCountryOfDispatchInfo, message);

					movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
					goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
					AssertNoMessageError("MovementHeader's BM_InBondEntryType is TIR", goodsItem.BY_RN_NKCountryOfDispatchInfo, message);

					movementHeader.BM_RN_NKCountryOfDispatch = ZString.Empty;
					goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
					AssertNoMessageError("Dispatch Country haven't specified on declaration level", goodsItem.BY_RN_NKCountryOfDispatchInfo, message);

					movementHeader.BM_RN_NKCountryOfDispatch = CountryCodes.China;
					goodsItem.Bill.B0_RN_NKCountryOfExport = ZString.Empty;
					goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
					AssertNoMessageError("Dispatch Country haven't specified on house consignment level", goodsItem.BY_RN_NKCountryOfDispatchInfo, message);

					goodsItem.Bill.B0_RN_NKCountryOfExport = CountryCodes.China;
					goodsItem.BY_RN_NKCountryOfDispatch = ZString.Empty;
					AssertNoMessageError("Dispatch Country haven't specified on goods items level", goodsItem.BY_RN_NKCountryOfDispatchInfo, message);

					goodsItem.Bill.B0_RN_NKCountryOfExport = ZString.Empty;
					goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
					AssertNoMessageError("Dispatch Country haven't specified on goods items or house consignment level", goodsItem.BY_RN_NKCountryOfDispatchInfo, message);

					goodsItem.Bill.B0_RN_NKCountryOfExport = CountryCodes.China;
					movementHeader.BM_RN_NKCountryOfDispatch = ZString.Empty;
					goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();
					AssertNoMessageError("Dispatch Country haven't specified on goods items or declaration level", goodsItem.BY_RN_NKCountryOfDispatchInfo, message);

					goodsItem.Bill.B0_RN_NKCountryOfExport = ZString.Empty;
					goodsItem.BY_RN_NKCountryOfDispatch = CountryCodes.China;
					AssertNoMessageError("Dispatch Country haven't specified on house consignment or declaration level", goodsItem.BY_RN_NKCountryOfDispatchInfo, message);

					goodsItem.BY_RN_NKCountryOfDispatch = ZString.Empty;
					AssertHasMessageError("Dispatch Country haven't specified on goods items, house consignment or declaration level", goodsItem.BY_RN_NKCountryOfDispatchInfo, message);
				}
			});
		}

		public void TestCheckRuleC0909_WhenInactive()
		{
			const string message = "[C0909] You have not entered Country Of Dispatch. It is required either on Declaration or House Consignment or House Consignment Item.";

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.DisableRule(c => c.IsRuleC0909Active);
				movementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
				goodsItem.Validation.ValidateBY_RN_NKCountryOfDispatch();

				AssertCollectionNotContains($"Expected notifications would not contain {message}", goodsItem.BY_RN_NKCountryOfDispatchInfo.Notifications.Select(e => e.Message), x => x.Contains(message));
			}
		}

		public void TestCheckBY_CusC4Number()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Virtual");
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "CUS Codes");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "01000001", "CUSCode 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "CNCODE", "11000001");
			Factory.Save();

			goodsItem.BY_HarmonisedTariff = "11000001";
			ValidationTestHelper.AssertInvalidCodeMessageError(goodsItem.BY_CusC4NumberInfo, "INVALID", "01000001");
		}

		public void TestCheckBY_RN_NKCountryOfDestination_RuleC0343_WhenActive() => CombineAssertions(() =>
		{
			const string messageError = "[C0343] You have not entered Country of Destination. It is required either on Declaration or House Consignment Item.";

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(c => c.IsRuleC0343Active);

				movementHeader.BM_RL_NKDestinationPort = ZString.Empty;
				goodsItem.BY_RN_NKCountryOfDestination = ZString.Empty;
				AssertHasMessageError("Empty BM_RL_NKDestinationPort and BY_RN_NKCountryOfDestination", goodsItem.BY_RN_NKCountryOfDestinationInfo, messageError);

				movementHeader.BM_RL_NKDestinationPort = ZString.Empty;
				goodsItem.BY_RN_NKCountryOfDestination = CountryCodes.Germany;
				AssertNoMessageError("Empty BM_RL_NKDestinationPort and not empty BY_RN_NKCountryOfDestination", goodsItem.BY_RN_NKCountryOfDestinationInfo, messageError);

				movementHeader.BM_RL_NKDestinationPort = CountryCodes.Germany;
				goodsItem.BY_RN_NKCountryOfDestination = ZString.Empty;
				AssertNoMessageError("Not empty BM_RL_NKDestinationPort and empty BY_RN_NKCountryOfDestination", goodsItem.BY_RN_NKCountryOfDestinationInfo, messageError);
			}
		});

		public void TestCheckBY_RN_NKCountryOfDestination_RuleC0343_WhenNotActive()
		{
			const string messageError = "[C0343] You have not entered Country of Destination. It is required either on Declaration or House Consignment Item.";

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.DisableRule(c => c.IsRuleC0343Active);
				movementHeader.BM_RL_NKDestinationPort = ZString.Empty;
				goodsItem.BY_RN_NKCountryOfDestination = ZString.Empty;
				goodsItem.Validation.ValidateBY_RN_NKCountryOfDestination();

				AssertCollectionNotContains($"Expected notifications would not contain {messageError}", goodsItem.BY_RN_NKCountryOfDestinationInfo.Notifications.Select(e => e.Message), x => x.Contains(messageError));
			}
		}

		public void TestCheckBY_CommercialReferenceNumber_Mandatory()
		{
			CombineAssertions("Preconditions: the field should be mandatory by default.", () =>
			{
				AssertEquals("BY_CommercialReferenceNumber", ZString.Empty, goodsItem.BY_CommercialReferenceNumber);
				AssertEquals("BM_UniqueConsignmentReference", ZString.Empty, movementHeader.BM_UniqueConsignmentReference);
				AssertEquals("B0_ReferenceID", ZString.Empty, goodsItem.Bill.B0_ReferenceID);
				AssertContainsExactElementsInAnyOrder("Additional Documents for Bill", Array.Empty<string>(), goodsItem.Bill.AdditionalDocuments.Select(x => x.CSI_SubType));
				AssertContainsExactElementsInAnyOrder("Additional Documents for Declaration", Array.Empty<string>(), nctsHeader.AdditionalDocuments.Select(x => x.CSI_SubType));

				goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
				AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);
			});

			goodsItem.BY_CommercialReferenceNumber = "A";
			AssertNoMessageErrors("Entering a commercial reference number should clear the message error, obviously.", goodsItem.BY_CommercialReferenceNumberInfo);

			goodsItem.BY_CommercialReferenceNumber = ZString.Empty;
			AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);
		}

		public void TestCheckBY_CommercialReferenceNumber_Mandatory_ReferenceID()
		{
			goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
			AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);

			goodsItem.Bill.B0_ReferenceID = "A";
			AssertNoMessageErrors("Entering a bill reference ID should also clear the message error on the goods item, even if the commercial reference is empty.", goodsItem.BY_CommercialReferenceNumberInfo);

			goodsItem.Bill.B0_ReferenceID = ZString.Empty;
			AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);
		}

		public void TestCheckBY_CommercialReferenceNumber_Mandatory_UniqueConsignmentReference()
		{
			goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
			AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);

			movementHeader.BM_UniqueConsignmentReference = "A";
			AssertNoMessageErrors("Entering a reference number should also clear the message error on the goods item, even if the commercial reference is empty.", goodsItem.BY_CommercialReferenceNumberInfo);

			movementHeader.BM_UniqueConsignmentReference = ZString.Empty;
			AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);
		}

		public void TestCheckBY_CommercialReferenceNumber_Mandatory_BillAdditionalDocument()
		{
			goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
			AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);

			var billAdditionalDocument = goodsItem.Bill.AdditionalDocuments.AddNew();
			goodsItem.Validation.ValidateBY_CommercialReferenceNumber();

			CombineAssertions("Additional documents on the bill only make this field non-mandatory if the document's sub-type is 'TRA'.", () =>
			{
				AssertNotEquals(AdditionalInfoSubTypeList.Codes.TransportDocument, billAdditionalDocument.CSI_SubType);
				AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);
			});

			billAdditionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
			AssertNoMessageErrors("A TRA additional document on the bill should cause this message error to disappear.", goodsItem.BY_CommercialReferenceNumberInfo);

			billAdditionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
			AssertHasMessageError("The bill's additional document's sub type is not TRA so the message error should reappear.", goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);
		}

		public void TestCheckBY_CommercialReferenceNumber_Mandatory_HeaderAdditionalDocument()
		{
			goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
			AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);

			var headerAdditionalDocument = nctsHeader.AdditionalDocuments.AddNew();

			CombineAssertions("Additional documents on the header only make this field non-mandatory if the document's sub-type is 'TRA'.", () =>
			{
				AssertNotEquals(AdditionalInfoSubTypeList.Codes.TransportDocument, headerAdditionalDocument.CSI_SubType);
				AssertHasMessageError(goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);
			});

			headerAdditionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
			AssertNoMessageErrors("A TRA additional document on the header should cause this message error to disappear.", goodsItem.BY_CommercialReferenceNumberInfo);

			headerAdditionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
			goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
			AssertHasMessageError("The header's additional document's sub type is not TRA so the message error should reappear.", goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);
		}

		public void TestCheckBY_CommercialReferenceNumber_Mandatory_RuleC0502()
		{
			goodsItem.BY_CommercialReferenceNumber = ZString.Empty;

			using (var billRuleContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					nctsBill.ClearAllCachedValues();

					billRuleContext.EnableRule(x => x.IsRuleC0502Active);
					billRuleContext.EnableRule(x => x.IsRuleB1895_1Active);
					using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
					{
						goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
						AssertNoMessageError("Rule C0502 is enabled, Rule B1895-1 is enabled", goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);
					}

					billRuleContext.DisableRule(x => x.IsRuleB1895_1Active);
					goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
					AssertHasMessageError("Rule C0502 is enabled, Rule B1895-1 is disabled", goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);

					billRuleContext.DisableRule(x => x.IsRuleC0502Active);
					goodsItem.Validation.ValidateBY_CommercialReferenceNumber();
					AssertNoMessageError("Rule C0502 is disabled", goodsItem.BY_CommercialReferenceNumberInfo, RuleC0502ExpectedMessageError);
				});
			}
		}

		public void TestCheckBY_CommercialReferenceNumber_Unique()
		{
			AssertRuleR0507(x => x.BY_CommercialReferenceNumberInfo);
		}

		public void TestCheckBY_CommercialReferenceNumber_WhenBillReferenceIDChanged_ShouldNotCheckForUniqueness_ForPerformance()
		{
			const string singleItemMessageError = "[R0507] For Declaration Type = ‘T’ (Mixed transit) there must be at least two Consignment Items with different Reference Number / UCR.";
			const string generalMessageError = "[R0507] Reference Number / UCR must be different for at least one of the consignment items.";
			using var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory);

			CombineAssertions(() =>
			{
				deciderTestContext.EnableRule(x => x.IsRuleR0507Active);
				goodsItem.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
				goodsItem.BY_CommercialReferenceNumber = "A";
				AssertHasMessageError("Single item, should trigger error message", goodsItem.BY_CommercialReferenceNumberInfo, singleItemMessageError);

				var goodsItem2 = goodsItem.Bill.GoodsItems.AddNew();
				goodsItem2.BY_CommercialReferenceNumber = "A";
				AssertHasMessageError("Two items, same reference number", goodsItem2.BY_CommercialReferenceNumberInfo, generalMessageError);

				goodsItem.BY_CommercialReferenceNumber = "B";
				AssertNoMessageError("Two items, different reference numbers", goodsItem.BY_CommercialReferenceNumberInfo, generalMessageError);

				goodsItem.Bill.B0_ReferenceID = "A";
				AssertNoMessageError("Changing B0_ReferenceID should only run mandatory validation on all its goods items. If we did the full unique validation on every goods item, it would iterate over " +
					"the whole collection once for each item. And B0_ReferenceID is only relevant for mandatory validation.", goodsItem.BY_CommercialReferenceNumberInfo, generalMessageError);
			});
		}

		public void TestCheckBY_CommercialReferenceNumber_WhenRuleB1895_1Active()
		{
			var expectedErrorMessage = "[B1895-1] This field must be empty if Details > 'Ref.No. / UCR' field is filled.";

			movementHeader.BM_UniqueConsignmentReference = "HBL101010";

			CombineAssertions(() =>
			{
				using (var billRuleContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					nctsBill.ClearAllCachedValues();

					billRuleContext.EnableRule(x => x.IsRuleB1895_1Active);
					using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
					{
						validation.ValidateBY_CommercialReferenceNumber();
						AssertNoMessageError("Details > 'Ref.No. / UCR' is filled and House consignment > goods item > item details > 'Ref.No. / UCR' is NOT filled", goodsItem.BY_CommercialReferenceNumberInfo, expectedErrorMessage);

						goodsItem.BY_CommercialReferenceNumber = "JBL121212";
						AssertHasMessageError("Details > 'Ref.No. / UCR' is filled and House consignment > goods item > item details > 'Ref.No. / UCR' is filled", goodsItem.BY_CommercialReferenceNumberInfo, expectedErrorMessage);

						goodsItem.BY_CommercialReferenceNumber = ZString.Empty;
						AssertNoMessageError("Details > 'Ref.No. / UCR' is filled and House consignment > goods item > item details > 'Ref.No. / UCR' is empty", goodsItem.BY_CommercialReferenceNumberInfo, expectedErrorMessage);

						movementHeader.BM_UniqueConsignmentReference = ZString.Empty;
						goodsItem.BY_CommercialReferenceNumber = "JBL121212";
						AssertNoMessageError("Details > 'Ref.No. / UCR' is NOT filled and House consignment > goods item > item details > 'Ref.No. / UCR' is filled", goodsItem.BY_CommercialReferenceNumberInfo, expectedErrorMessage);
					}
				}
			});
		}

		public void TestCheckBY_CommercialReferenceNumber_WhenRuleB1895_1Inactive()
		{
			var expectedErrorMessage = "[B1895-1] This field must be empty if Details > 'Ref.No. / UCR' field is filled.";

			var propertyInfo = goodsItem.BY_CommercialReferenceNumberInfo;
			movementHeader.BM_UniqueConsignmentReference = "HBL101010";
			CombineAssertions(() =>
			{
				using (var billRuleContext = new NctsBillValidationDeciderTestContext<INctsBillDeparturePhase5ValidationDecider>(Factory))
				{
					billRuleContext.DisableRule(x => x.IsRuleB1895_1Active);

					goodsItem.BY_CommercialReferenceNumber = "JBL121212";
					AssertCollectionNotContains("BM_UniqueConsignmentReference isn't empty", propertyInfo.Notifications.Select(e => e.Message), x => x.Contains(expectedErrorMessage));

					movementHeader.BM_UniqueConsignmentReference = ZString.Empty;
					goodsItem.BY_CommercialReferenceNumber = "JBL121212";
					AssertCollectionNotContains("BM_UniqueConsignmentReference is empty", propertyInfo.Notifications.Select(e => e.Message), x => x.Contains(expectedErrorMessage));
				}
			});
		}

		public void TestCheckBY_NetWeight_B1805_1()
		{
			var expectedError = "[B1805-1] During the transition period, which is now, Net weight must be empty";

			CombineAssertions(() =>
			{
				using var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory);
				deciderTestContext.ClearCachedValidationDecider(goodsItem);

				deciderTestContext.EnableRule(x => x.IsRuleB1805_1Active);
				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					validation.ValidateBY_NetWeight();
					AssertNoMessageError("When Net weight is empty", goodsItem.BY_NetWeightInfo, expectedError);

					goodsItem.BY_NetWeight = 1000.00;
					AssertNoMessageError("When Reduced DatasetIndicator is false but Net weight is filled", goodsItem.BY_NetWeightInfo, expectedError);

					movementHeader.BM_ReducedDatasetIndicator = true;
					validation.ValidateBY_NetWeight();
					AssertHasMessageError("When Reduced DatasetIndicator is true and Net weight is filled", goodsItem.BY_NetWeightInfo, expectedError);
				}

				deciderTestContext.DisableRule(x => x.IsRuleB1805_1Active);
				validation.ValidateBY_NetWeight();
				AssertNoMessageError("When Rule is disabled", goodsItem.BY_NetWeightInfo, expectedError);
			});
		}

		public void TestCheckBY_NetWeight_Mandatory_C0837()
		{
			var previousDocument = goodsItem.Bill.PreviousDocuments.AddNew();
			const string error = "[C0837] You have not entered a Net Weight";

			CombineAssertions(() =>
			{
				using var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory);
				deciderTestContext.ClearCachedValidationDecider(goodsItem);

				deciderTestContext.EnableRule(x => x.IsRuleC0837Active);
				validation.ValidateBY_NetWeight();
				AssertNoMessageError("Bill's previous document type is not N830 and goodsItem's net weight is 0.", goodsItem.BY_NetWeightInfo, error);

				previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
				validation.ValidateBY_NetWeight();
				AssertHasMessageError("Bill's previous document type is N830 and goodsItem's net weight is 0.", goodsItem.BY_NetWeightInfo, error);

				deciderTestContext.DisableRule(x => x.IsRuleC0837Active);
				validation.ValidateBY_NetWeight();
				AssertNoMessageError("Rule is disabled.", goodsItem.BY_NetWeightInfo, error);

				deciderTestContext.EnableRule(x => x.IsRuleC0837Active);

				goodsItem.BY_NetWeight = 1000;
				AssertNoMessageError("Bill's previous document type is N830 and goodsItem's net weight is not 0.", goodsItem.BY_NetWeightInfo, error);

				previousDocument.CSI_Code = ZString.Empty;
				validation.ValidateBY_NetWeight();
				AssertNoMessageError("Bill's previous document type is not N830 and goodsItem's net weight is not 0.", goodsItem.BY_NetWeightInfo, error);
			});
		}

		public void TestCheckBY_NetWeight_E1109()
		{
			const string message = "[E1109] Entered Net Weight exceeding the max value supported Inside Transition Period (Total 11 digits with maximum of 3 decimals).";
			UniversalValidationHelperTest.AssertCheckMaxValueAndMaxDecimalLengthForWeightIfPhase5TransitionPeriod(goodsItem.BY_NetWeightInfo, message);
		}

		public void TestCheckBY_NetWeight_R0223()
		{
			const string expectedError = "[R0223] If Gross Weight is not 0, then Net Weight must be less than or equal to Gross Weight";

			CombineAssertions(() =>
			{
				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					goodsItem.BY_NetWeight = 500;
					goodsItem.BY_GrossWeight = 1000;
					goodsItem.Validation.ValidateBY_NetWeight();
					AssertNoMessageError("When Gross weight is not 0 then Net weight should be less than or equal to Gross weight", goodsItem.BY_NetWeightInfo, expectedError);

					goodsItem.BY_NetWeight = 1000;
					goodsItem.BY_GrossWeight = 500;
					goodsItem.Validation.ValidateBY_NetWeight();
					AssertHasMessageError("When Gross weight is not 0 then Net weight should be less than or equal to Gross weight", goodsItem.BY_NetWeightInfo, expectedError);

					goodsItem.BY_NetWeight = 1000;
					goodsItem.BY_GrossWeight = 0;
					goodsItem.Validation.ValidateBY_NetWeight();
					AssertNoMessageError("When Gross weight is 0 then Net weight should be greater than 0", goodsItem.BY_NetWeightInfo, expectedError);

					goodsItem.BY_NetWeight = 0;
					goodsItem.BY_GrossWeight = 1000;
					goodsItem.Validation.ValidateBY_NetWeight();
					AssertNoMessageError("When Gross weight is not 0 then Net weight should be less than or equal to Gross weight", goodsItem.BY_NetWeightInfo, expectedError);

					goodsItem.BY_NetWeight = 0;
					goodsItem.BY_GrossWeight = 0;
					goodsItem.Validation.ValidateBY_NetWeight();
					AssertNoMessageError("When Gross weight is 0 then Net weight should be greater than 0", goodsItem.BY_NetWeightInfo, expectedError);

					goodsItem.BY_NetWeight = 1000;
					goodsItem.BY_GrossWeight = 1000;
					goodsItem.Validation.ValidateBY_NetWeight();
					AssertNoMessageError("When Gross weight is not 0 then Net weight should be less than or equal to Gross weight", goodsItem.BY_NetWeightInfo, expectedError);
				}

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					goodsItem.BY_NetWeight = 500;
					goodsItem.BY_GrossWeight = 1000;
					goodsItem.Validation.ValidateBY_NetWeight();
					AssertNoMessageError("When Gross weight is not 0 then Net weight should be less than or equal to Gross weight", goodsItem.BY_NetWeightInfo, expectedError);

					goodsItem.BY_NetWeight = 1000;
					goodsItem.BY_GrossWeight = 500;
					goodsItem.Validation.ValidateBY_NetWeight();
					AssertNoMessageError("When Gross weight is not 0 then Net weight should be less than or equal to Gross weight", goodsItem.BY_NetWeightInfo, expectedError);

					goodsItem.BY_NetWeight = 1000;
					goodsItem.BY_GrossWeight = 0;
					goodsItem.Validation.ValidateBY_NetWeight();
					AssertNoMessageError("When Gross weight is 0 then Net weight should be greater than 0", goodsItem.BY_NetWeightInfo, expectedError);

					goodsItem.BY_NetWeight = 0;
					goodsItem.BY_GrossWeight = 1000;
					goodsItem.Validation.ValidateBY_NetWeight();
					AssertNoMessageError("When Gross weight is not 0 then Net weight should be less than or equal to Gross weight", goodsItem.BY_NetWeightInfo, expectedError);

					goodsItem.BY_NetWeight = 0;
					goodsItem.BY_GrossWeight = 0;
					goodsItem.Validation.ValidateBY_NetWeight();
					AssertNoMessageError("When Gross weight is 0 then Net weight should be greater than 0", goodsItem.BY_NetWeightInfo, expectedError);

					goodsItem.BY_NetWeight = 1000;
					goodsItem.BY_GrossWeight = 1000;
					goodsItem.Validation.ValidateBY_NetWeight();
					AssertNoMessageError("When Gross weight is not 0 then Net weight should be less than or equal to Gross weight", goodsItem.BY_NetWeightInfo, expectedError);
				}
			});
		}

		public void TestCheckBY_Type_CheckRuleR0507()
		{
			AssertRuleR0507(x => x.BY_TypeInfo);
		}

		public void TestCheckBY_Type_CheckRuleR0601_1_When_AdditionalInfos_HasExciseGoods()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_CL234, "CusCodeTypeCL234");
			helper.CreateCusCodeList("EUN", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL234, "TE", "Document Type Excise", new ZDateTime(2022, 01, 01), new ZDateTime(2040, 12, 31));
			Factory.Save();

			const string error = "Please enter 'N380' in Previous Document and 'T1' as Declaration type at Consignment item when Additional Reference Type at Consignment item has Excise Codes.";
			var info = goodsItem.BY_TypeInfo;

			var additionalInfos = goodsItem.AdditionalInfos.AddNew();
			var previousDocument = goodsItem.PreviousDocuments.AddNew();

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(x => x.IsRuleR0601_1Active);
				additionalInfos.CSI_SubType = "REF";
				additionalInfos.CSI_Code = "YY";
				goodsItem.BY_Type = "ZZ";
				previousDocument.CSI_Code = "A123";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error in transition period when rule is active and rule does not apply", info, error);

				additionalInfos.CSI_Code = "TE";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error in transition period when rule is active and rule not satisfied", info, error);

				goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T1;
				previousDocument.CSI_Code = "N380";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error in transition period when rule is active and rule is satisfied", info, error);
			}

			var newFactory = new BusinessObjectFactory();
			nctsHeader = newFactory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			validation = goodsItem.Validation as NctsDepartureCargoDescPhase5Validation;
			additionalInfos = goodsItem.AdditionalInfos.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(newFactory))
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(x => x.IsRuleR0601_1Active);
				info = goodsItem.BY_TypeInfo;
				additionalInfos.CSI_SubType = "REF";
				additionalInfos.CSI_Code = "YY";
				goodsItem.BY_Type = "ZZ";
				previousDocument.CSI_Code = "A123";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error outside transition period when rule is active and rule does not apply", info, error);

				additionalInfos.CSI_Code = "TE";
				validation.ValidateBY_Type();
				AssertHasMessageError("Has error outside transition period when rule is active and rule not satisfied", info, error);

				goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T1;
				validation.ValidateBY_Type();
				AssertHasMessageError("Has error outside transition period when rule is active and rule not satisfied (no N380)", info, error);

				goodsItem.BY_Type = "ZZ";
				previousDocument.CSI_Code = "N380";
				validation.ValidateBY_Type();
				AssertHasMessageError("Has error outside transition period when rule is active and rule not satisfied (wrong BM_InBondEntryType)", info, error);

				goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T1;
				previousDocument.CSI_Code = "N380";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error outside transition period when rule is active and rule is satisfied", info, error);

				goodsItem.BY_Type = string.Empty;
				movementHeader.BM_InBondEntryType = "ZZ";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error outside transition period when rule is active and goodsItem.BY_Type is empty because error should be shown at CusInBondMoveHeader.BM_InBondEntryType", info, error);
			}

			newFactory = new BusinessObjectFactory();
			nctsHeader = newFactory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			validation = goodsItem.Validation as NctsDepartureCargoDescPhase5Validation;
			additionalInfos = goodsItem.AdditionalInfos.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(newFactory))
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.DisableRule(x => x.IsRuleR0601_1Active);

				info = goodsItem.BY_TypeInfo;
				additionalInfos.CSI_SubType = "REF";
				additionalInfos.CSI_Code = "YY";
				goodsItem.BY_Type = "ZZ";
				previousDocument.CSI_Code = "A123";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error outside transition period when rule is not active and rule does not apply", info, error);

				additionalInfos.CSI_Code = "TE";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error outside transition period when rule is not active and rule not satisfied", info, error);

				goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T1;
				previousDocument.CSI_Code = "N380";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error outside transition period when rule is not active and rule is satisfied", info, error);
			}
		}

		public void TestCheckBY_Type_CheckRuleR0601_1_When_SupportingDocuments_HasExciseGoods()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", eun);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Code_CL234, "CusCodeTypeCL234");
			helper.CreateCusCodeList("EUN", UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_CL234, "TE", "Document Type Excise", new ZDateTime(2022, 01, 01), new ZDateTime(2040, 12, 31));
			Factory.Save();

			const string error = "Declaration type should be T2 or T2F at Consignment level or Consignment Item level when Supporting Document Type at Consignment item level has Excise Codes.";
			var info = goodsItem.BY_TypeInfo;

			var supportingDocuments = goodsItem.SupportingDocuments.AddNew();

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(x => x.IsRuleR0601_1Active);
				supportingDocuments.CSI_Code = "YY";
				goodsItem.BY_Type = "ZZ";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error in transition period when rule is active and rule does not apply", info, error);

				supportingDocuments.CSI_Code = "TE";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error in transition period when rule is active and rule not satisfied", info, error);

				goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
				validation.ValidateBY_Type();
				AssertNoMessageError("No error in transition period when rule is active and rule is satisfied", info, error);
			}

			var newFactory = new BusinessObjectFactory();
			nctsHeader = newFactory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			validation = goodsItem.Validation as NctsDepartureCargoDescPhase5Validation;
			supportingDocuments = goodsItem.SupportingDocuments.AddNew();

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(newFactory))
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(x => x.IsRuleR0601_1Active);
				info = goodsItem.BY_TypeInfo;
				supportingDocuments.CSI_Code = "YY";
				goodsItem.BY_Type = "ZZ";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error outside transition period when rule is active and rule does not apply", info, error);

				supportingDocuments.CSI_Code = "TE";
				validation.ValidateBY_Type();
				AssertHasMessageError("Has error outside transition period when rule is active and rule not satisfied", info, error);

				goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
				AssertNoMessageError("No error outside transition period when rule is active and rule is satisfied (T2)", info, error);

				goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T2F;
				validation.ValidateBY_Type();
				AssertNoMessageError("No error outside transition period when rule is active and rule is satisfied (T2F)", info, error);

				goodsItem.BY_Type = string.Empty;
				movementHeader.BM_InBondEntryType = "ZZ";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error outside transition period when rule is active and goodsItem.BY_Type is empty because error should be shown at CusInBondMoveHeader.BM_InBondEntryType", info, error);
			}

			newFactory = new BusinessObjectFactory();
			nctsHeader = newFactory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			validation = goodsItem.Validation as NctsDepartureCargoDescPhase5Validation;
			supportingDocuments = goodsItem.SupportingDocuments.AddNew();

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(newFactory))
			using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.DisableRule(x => x.IsRuleR0601_1Active);
				info = goodsItem.BY_TypeInfo;
				supportingDocuments.CSI_Code = "YY";
				goodsItem.BY_Type = "ZZ";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error outside transition period when rule is not active and rule does not apply", info, error);

				supportingDocuments.CSI_Code = "TE";
				validation.ValidateBY_Type();
				AssertNoMessageError("No error outside transition period when rule is not active and rule not satisfied", info, error);

				goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T1;
				validation.ValidateBY_Type();
				AssertNoMessageError("No error outside transition period when rule is not active and rule is satisfied", info, error);
			}
		}

		public void TestCheckPackages_TR0001Rule()
		{
			var rowMessageError = "[TR0001] At least one Package Record is required.";
			CombineAssertions(() =>
			{
				using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0001Active));
					validation.ValidateAll();
					AssertHasRowMessageError("No packages, Rule TR0001 active", goodsItem, rowMessageError);

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0001Active));
					validation.ValidateAll();
					AssertNoRowMessageError("No packages, Rule TR0001 inactive", goodsItem, rowMessageError);

					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleTR0001Active));
					goodsItem.Packages.AddNew();
					validation.ValidateAll();
					AssertNoRowMessageError("Has packages, Rule TR0001 active", goodsItem, rowMessageError);
				}
			});
		}

		[TestDate(2022, 07, 01)]
		public void TestCheckBY_Type_ConditionR0909()
		{
			using (var ruleTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingDataGrouping("EUN");
				helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "CL010 Desc.");
				helper.CreateCusCodeList("EUN", EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL148, "AT", "Austria", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
				helper.CreateCusCodeList("EUN", EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, "CY", "Cyprus", new ZDateTime(2022, 01, 01), new ZDateTime(2022, 12, 31));
				Factory.Save();

				var departureOffice = NCTSTestHelper.CreateCustomsOfficeForTest(movementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "CY123456", ZDateTime.Empty, true);
				NCTSTestHelper.CreateCustomsOfficeForTest(movementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "SM123456", ZDateTime.Empty);

				const string errorText = "R909";

				CombineAssertions(() =>
				{
					ruleTestContext.ClearCachedValidationDecider(goodsItem);

					ruleTestContext.EnableRule(x => x.IsRuleR0909Active);
					goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
					AssertHasMessageErrorContaining("Departure office in CL010 country, type not T2", goodsItem.BY_TypeInfo, errorText);

					ruleTestContext.DisableRule(x => x.IsRuleR0909Active);
					goodsItem.Validation.ValidateBY_Type();
					AssertNoMessageErrorContaining("Departure office in CL010 country, type not T2. Bust Rule R0909 is inactive", goodsItem.BY_TypeInfo, errorText);

					ruleTestContext.EnableRule(x => x.IsRuleR0909Active);
					departureOffice.CY_Data = "AT123456";
					goodsItem.Validation.ValidateBY_Type();
					AssertNoMessageErrorContaining("Departure office not in CL010 country", goodsItem.BY_TypeInfo, errorText);

					departureOffice.CY_Data = "CY123456";
					goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories;
					AssertNoMessageErrorContaining("Departure office in CL010 country, T2 type", goodsItem.BY_TypeInfo, errorText);
				});
			}
		}

		public void TestCheckBY_Type_ConditionC0045()
		{
			var c0045Message = "[C0045] Declaration Type ‘T’ on Header level requires Declaration Type on Item level.";
			var c0045MessageMixedType = "[C0045] Declaration Types for all items must be left empty if Declaration Type at header is not 'T'.";
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;

					deciderTestContext.EnableRule(x => x.IsRuleC0045Active);
					validation.ValidateAll();
					AssertHasMessageError("Message shows when T and BY_Type is empty.", goodsItem.BY_TypeInfo, c0045Message);

					goodsItem.BY_Type = "T1";
					AssertNoMessageError("Validation passes when T and only 1 BY_Type exists.", goodsItem.BY_TypeInfo, c0045Message);

					var item2 = goodsItem.Bill.GoodsItems.AddNew();
					item2.BY_Type = string.Empty;
					validation.ValidateAll();
					AssertNoMessageError("Validation passes when T and another BY_Type is empty", goodsItem.BY_TypeInfo, c0045Message);
					AssertHasMessageError("Message shows when T and BY_Type is empty", item2.BY_TypeInfo, c0045Message);

					item2.BY_Type = "T2";
					validation.ValidateAll();
					AssertNoMessageError("Validation passes when T and multiple BY_Type.", goodsItem.BY_TypeInfo, c0045Message);

					var item3 = goodsItem.Bill.GoodsItems.AddNew();
					item3.BY_Type = "T2";
					validation.ValidateAll();
					AssertNoMessageError("Validation passes when T and 2 BY_Type on 3 items.", goodsItem.BY_TypeInfo, c0045Message);

					movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
					validation.ValidateAll();
					AssertHasMessageError("Message shows when not T and any non-empty BY_Type.", goodsItem.BY_TypeInfo, c0045MessageMixedType);

					item3.BY_Type = ZString.Empty;
					item2.BY_Type = ZString.Empty;
					goodsItem.BY_Type = ZString.Empty;
					AssertNoMessageError("Validation passes when not T and all empty BY_Type.", goodsItem.BY_TypeInfo, c0045MessageMixedType);

					deciderTestContext.DisableRule(x => x.IsRuleC0045Active);
					movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
					goodsItem.BY_Type = "T1";
					item2.BY_Type = "T1";
					item3.BY_Type = "T1";
					validation.ValidateAll();
					AssertNoMessageError("Validation skipped when rule disabled", goodsItem.BY_TypeInfo, c0045MessageMixedType);
				});
			}
		}

		public void TestCheckBY_GrossWeight_RuleRP11()
		{
			AssertRuleRP11(x => x.BY_GrossWeightInfo, x => x.Validation.ValidateBY_GrossWeight());
		}

		public void TestCheckBY_GrossWeight_E1109()
		{
			const string message = "[E1109] Entered Gross Weight exceeding the max value supported Inside Transition Period (Total 11 digits with maximum of 3 decimals).";
			UniversalValidationHelperTest.AssertCheckMaxValueAndMaxDecimalLengthForWeightIfPhase5TransitionPeriod(goodsItem.BY_GrossWeightInfo, message);
		}

		public void TestCheckBY_NetWeight_RuleRP11()
		{
			AssertRuleRP11(x => x.BY_NetWeightInfo, x => x.Validation.ValidateBY_NetWeight());
		}

		public void TestCheckBY_GrossWeight_RuleR0221_3() => CombineAssertions(() =>
		{
			const string errorText = "[R0221-3]";
			var propertyInfo = goodsItem.BY_GrossWeightInfo;
			var bulkType = Factory.SetupBulkCusCode();

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(c => c.IsRuleR0221_3Active);

				goodsItem.Packages.RemoveAll();
				goodsItem.Packages.AddNew().B5_UnitCount = 0;
				goodsItem.BY_GrossWeight = 0;

				AssertNoMessageErrorContaining("BY_GrossWeight = 0; B5_UnitCount = 0;", propertyInfo, errorText);

				var package2 = goodsItem.Packages.AddNew();
				package2.B5_UnitCount = 2;
				goodsItem.BY_GrossWeight = 1;

				AssertNoMessageErrorContaining("BY_GrossWeight > 0; B5_UnitCount > 0;", propertyInfo, errorText);

				package2.B5_UnitCount = 0;

				goodsItem.Validation.ValidateBY_GrossWeight();
				AssertHasMessageErrorContaining("BY_GrossWeight > 0; B5_UnitCount = 0;", propertyInfo, errorText);

				package2.B5_UnitType = bulkType;
				goodsItem.Validation.ValidateBY_GrossWeight();
				AssertNoMessageErrorContaining("BY_GrossWeight > 0; B5_UnitCount = 0; Type Bulk", propertyInfo, errorText);
				deciderTestContext.VerifyRule(e => e.IsRuleR0221_3Active);

				deciderTestContext.DisableRule(e => e.IsRuleR0221_3Active);

				package2.B5_UnitType = "1A";
				goodsItem.Validation.ValidateBY_GrossWeight();
				AssertNoMessageErrorContaining(propertyInfo, errorText);
				deciderTestContext.VerifyRule(e => e.IsRuleR0221_3Active);
			}
		});

		public void TestCheckBY_GrossWeight_RuleR0221_1() => CombineAssertions(() =>
		{
			const string errorText = "[R0221-1]";
			var propertyInfo = goodsItem.BY_GrossWeightInfo;

			using var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory);
			deciderTestContext.ClearCachedValidationDecider(goodsItem);

			deciderTestContext.EnableRule(x => x.IsRuleR0221_1Active);

			goodsItem.Packages.RemoveAll();
			goodsItem.Packages.AddNew().B5_UnitCount = 0;
			goodsItem.BY_GrossWeight = 0;
			AssertNoMessageErrorContaining("BY_GrossWeight = 0; B5_UnitCount = 0;", propertyInfo, errorText);

			goodsItem.Packages.AddNew().B5_UnitCount = 2;
			goodsItem.BY_GrossWeight = 1;
			AssertNoMessageErrorContaining("BY_GrossWeight > 0; B5_UnitCount > 0;", propertyInfo, errorText);

			goodsItem.BY_GrossWeight = 0;
			AssertHasMessageErrorContaining("BY_GrossWeight = 0; B5_UnitCount > 0;", propertyInfo, errorText);

			deciderTestContext.DisableRule(x => x.IsRuleR0221_1Active);

			goodsItem.BY_GrossWeight = 0;
			AssertNoMessageErrorContaining(propertyInfo, errorText);
		});

		public void TestCheckConditionR0020_BYType() => CombineAssertions(() =>
		{
			const string messageError = "[R0020] Previous Document of Type in CL178 is required either at Consignment level or for this Goods Item when Country of Customs Office of Departure is from the CL112 (Country Codes CTC) list and Declaration Type for this Goods Item is T2/T2F.";

			UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(Factory, Code_CL112, (Norway, "Norway"));
			UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(Factory, Code_CL178, ("123", "Something"));
			Factory.Save();

			var officeOfDeparture = movementHeader.CustomsOffices.AddNew();
			officeOfDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			officeOfDeparture.CY_Data = CountryCodes.Latvia;

			using var deciderTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);
			deciderTestContext.ClearCachedValidationDecider(movementHeader);
			deciderTestContext.EnableRule(c => c.IsRuleR0020Active);

			goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
			validation.ValidateAll();
			AssertNoRowMessageError("No Departure office within CL112", goodsItem, messageError);

			officeOfDeparture.CY_Data = CountryCodes.Norway;
			validation.ValidateAll();
			AssertHasRowMessageError("T2 - Office within CL112 without CL178 Previous Document", goodsItem, messageError);

			goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T2F;
			validation.ValidateAll();
			AssertHasRowMessageError("T2F - Office within CL112 without CL178 Previous Document", goodsItem, messageError);

			goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T;
			validation.ValidateAll();
			AssertNoRowMessageError("Not T2 or T2F", goodsItem, messageError);

			goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T2;
			var nctsHeaderPreviousDocument = nctsHeader.PreviousDocuments.AddNew();
			nctsHeaderPreviousDocument.CSI_Code = "123";
			validation.ValidateAll();
			AssertNoRowMessageError("Ncts Header Previous Document in CL178", goodsItem, messageError);

			var goodsItemPreviousDocument = goodsItem.PreviousDocuments.AddNew();
			nctsHeaderPreviousDocument.CSI_Code = ZString.Empty;
			goodsItemPreviousDocument.CSI_Code = "123";
			validation.ValidateAll();
			AssertNoRowMessageError("Goods Item Previous Document in CL178", goodsItem, messageError);

			goodsItemPreviousDocument.CSI_Code = "999";
			validation.ValidateAll();
			AssertHasRowMessageError("Previous document is not in CL178", goodsItem, messageError);

			deciderTestContext.ClearCachedValidationDecider(movementHeader);
			deciderTestContext.DisableRule(c => c.IsRuleR0020Active);
			validation.ValidateAll();
			AssertNoRowMessageError("R0020 inactive", goodsItem, messageError);
		});

		public void TestCheckConditionR0020_BM_InBondEntryType() => CombineAssertions(() =>
		{
			var messageError = "[R0020] Previous Document of Type in CL178 is required either at Consignment level or for all Goods Item when Country of Customs Office of Departure is from the CL112 (Country Codes CTC) list and the Declaration Type is T2/T2F.";

			UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(Factory, Code_CL112, (Norway, "Norway"));
			UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(Factory, Code_CL178, ("123", "Something"));
			Factory.Save();

			var officeOfDeparture = movementHeader.CustomsOffices.AddNew();
			officeOfDeparture.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
			officeOfDeparture.CY_Data = CountryCodes.Latvia;

			using var deciderTestContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);
			deciderTestContext.ClearCachedValidationDecider(movementHeader);
			deciderTestContext.EnableRule(c => c.IsRuleR0020Active);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
			validation.ValidateAll();
			AssertNoRowMessageError("No Departure office within CL112", goodsItem, messageError);

			officeOfDeparture.CY_Data = CountryCodes.Norway;
			validation.ValidateAll();
			AssertHasRowMessageError("T2 - Office within CL112 without CL178 Previous Document", goodsItem, messageError);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2F;
			validation.ValidateAll();
			AssertHasRowMessageError("T2F - Office within CL112 without CL178 Previous Document", goodsItem, messageError);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T;
			validation.ValidateAll();
			AssertNoRowMessageError("Not T2 or T2F", goodsItem, messageError);

			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
			var nctsHeaderPreviousDocument = nctsHeader.PreviousDocuments.AddNew();
			nctsHeaderPreviousDocument.CSI_Code = "123";
			validation.ValidateAll();
			AssertNoRowMessageError("Ncts Header Previous Document in CL178", goodsItem, messageError);

			var goodsItemPreviousDocument = goodsItem.PreviousDocuments.AddNew();
			nctsHeaderPreviousDocument.CSI_Code = ZString.Empty;
			goodsItemPreviousDocument.CSI_Code = "123";
			validation.ValidateAll();
			AssertNoRowMessageError("Goods Item Previous Document in CL178", goodsItem, messageError);

			goodsItemPreviousDocument.CSI_Code = "999";
			validation.ValidateAll();
			AssertHasRowMessageError("Previous document is not in CL178", goodsItem, messageError);

			deciderTestContext.ClearCachedValidationDecider(movementHeader);
			deciderTestContext.DisableRule(x => x.IsRuleR0020Active);
			validation.ValidateAll();
			AssertNoRowMessageError("R0020 inactive", goodsItem, messageError);
		});

		public void TestCheckConditionR0020_1_BYType() => CombineAssertions(() =>
		{
			const string messageError = "[R0020-1] Previous Document of Type in CL178 is required either at Consignment level or for this Goods Item when Declaration Type for this Goods Item is T2/T2F.";

			UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(Factory, Code_CL178, ("123", "Something"));
			Factory.Save();

			var entryTypesRequiringCL178 = new[] { NctsPhase5DeclarationTypeList.Codes.T2, NctsPhase5DeclarationTypeList.Codes.T2F };
			var entryTypesNotRequiringCL178 = new NctsPhase5DeclarationTypeList().GetAllCodes().Except(entryTypesRequiringCL178);

			using var testContext = new MovementHeaderValidationDeciderTestContext<INctsDepartureMovementHeaderPhase5ValidationDecider>(Factory);
			testContext.ClearCachedValidationDecider(movementHeader);
			testContext.EnableRule(x => x.IsRuleR0020_1Active);

			foreach (var entryType in entryTypesRequiringCL178)
			{
				goodsItem.BY_Type = entryType;
				validation.ValidateAll();
				AssertHasRowMessageError($"{goodsItem.BY_Type}: No previous document", goodsItem, messageError);
			}

			foreach (var entryType in entryTypesNotRequiringCL178)
			{
				goodsItem.BY_Type = entryType;
				validation.ValidateAll();
				AssertNoRowMessageError($"{goodsItem.BY_Type}: No previous document", goodsItem, messageError);
			}

			var itemPreviousDocument = goodsItem.PreviousDocuments.AddNew();
			itemPreviousDocument.CSI_Code = "123";

			foreach (var entryType in entryTypesRequiringCL178)
			{
				goodsItem.BY_Type = entryType;
				validation.ValidateAll();
				AssertNoRowMessageError($"{goodsItem.BY_Type}: With CL178 previous document at consignment item level", goodsItem, messageError);
			}

			itemPreviousDocument.CSI_Code = "999";

			foreach (var entryType in entryTypesRequiringCL178)
			{
				goodsItem.BY_Type = entryType;
				validation.ValidateAll();
				AssertHasRowMessageError($"{goodsItem.BY_Type}: With other previous document at consignment item level", goodsItem, messageError);
			}

			var headerPreviousDocument = nctsHeader.PreviousDocuments.AddNew();
			headerPreviousDocument.CSI_Code = "123";

			foreach (var entryType in entryTypesRequiringCL178)
			{
				goodsItem.BY_Type = entryType;
				validation.ValidateAll();
				AssertNoRowMessageError($"{goodsItem.BY_Type}: With CL178 previous document at consignment level", goodsItem, messageError);
			}

			headerPreviousDocument.CSI_Code = "999";

			foreach (var entryType in entryTypesRequiringCL178)
			{
				goodsItem.BY_Type = entryType;
				validation.ValidateAll();
				AssertHasRowMessageError($"{goodsItem.BY_Type}: With other previous document at consignment level", goodsItem, messageError);
			}

			testContext.ClearCachedValidationDecider(movementHeader);
			testContext.DisableRule(x => x.IsRuleR0020_1Active);

			validation.ValidateAll();
			AssertNoRowMessageError("R0020-1 inactive", goodsItem, messageError);
		});

		public void TestCheckBY_GrossWeight_NR0020() => CombineAssertions(() =>
		{
			const string expectedErrorMessage = "[NR0020] Gross Weight must be greater than 0 if the Goods Item has a Package with Quantity greater than 0 or the Package Type Bulk.";
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);

				deciderTestContext.EnableRule(x => x.IsRuleNR0020Active);
				var bulkType = Factory.SetupBulkCusCode();

				goodsItem.BY_GrossWeight = ZDecimal.Zero;
				goodsItem.Validation.ValidateBY_GrossWeight();
				AssertNoMessageErrorContaining("Package list empty", goodsItem.BY_GrossWeightInfo, expectedErrorMessage);

				var package = goodsItem.Packages.AddNew();
				package.B5_UnitCount = 1;
				goodsItem.Validation.ValidateBY_GrossWeight();
				AssertHasMessageErrorContaining("B5_UnitCount > 0", goodsItem.BY_GrossWeightInfo, expectedErrorMessage);

				package.B5_UnitCount = 0;
				package.B5_UnitType = bulkType;
				goodsItem.Validation.ValidateBY_GrossWeight();
				AssertHasMessageErrorContaining($"B5_UnitType is Bulk ({package.B5_UnitType})", goodsItem.BY_GrossWeightInfo, expectedErrorMessage);

				deciderTestContext.DisableRule(x => x.IsRuleNR0020Active);
				goodsItem.Validation.ValidateBY_GrossWeight();
				AssertNoMessageErrorContaining($"Rule is disabled", goodsItem.BY_GrossWeightInfo, expectedErrorMessage);

				deciderTestContext.EnableRule(x => x.IsRuleNR0020Active);

				goodsItem.BY_GrossWeight = 1;
				goodsItem.Validation.ValidateBY_GrossWeight();
				AssertNoMessageErrorContaining($"BY_GrossWeight > 0 and B5_UnitType is Bulk ({package.B5_UnitType})", goodsItem.BY_GrossWeightInfo, expectedErrorMessage);

				package.B5_UnitCount = 1;
				package.B5_UnitType = ZString.Empty;
				goodsItem.Validation.ValidateBY_GrossWeight();
				AssertNoMessageErrorContaining("BY_GrossWeight > 0 and B5_UnitCount > 0", goodsItem.BY_GrossWeightInfo, expectedErrorMessage);
			}
		});

		public void TestCheckBY_GrossWeight_B2101()
		{
			const string expectedErrorMessage = "[B2101] Gross Weight must be greater than 0";

			var grossWeightInfo = goodsItem.BY_GrossWeightInfo;
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					deciderTestContext.DisableRule(x => x.IsRuleB2101Active);
					goodsItem.BY_GrossWeight = ZDecimal.Zero;
					AssertNoMessageError("When rule B2101 is disabled", grossWeightInfo, expectedErrorMessage);

					CombineAssertions("When rule B2101 is enabled and not in TP", () =>
					{
						deciderTestContext.EnableRule(x => x.IsRuleB2101Active);
						goodsItem.Validation.ValidateBY_GrossWeight();
						AssertHasMessageError("Gross Weight is set zero", grossWeightInfo, expectedErrorMessage);

						goodsItem.BY_GrossWeight = 15;
						AssertNoMessageError("Gross Weight is filled", grossWeightInfo, expectedErrorMessage);
					});
				}

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					goodsItem.BY_GrossWeight = ZDecimal.Zero;
					AssertNoMessageError("When in TP", grossWeightInfo, expectedErrorMessage);
				}
			}
		}

		public void TestCheckBY_GrossWeightUnit_B2101()
		{
			const string expectedErrorMessage = "[B2101] You have not entered a Gross Weight Unit";

			var grossWeightUnitInfo = goodsItem.BY_GrossWeightUnitInfo;
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					deciderTestContext.DisableRule(x => x.IsRuleB2101Active);
					goodsItem.BY_GrossWeightUnit = ZString.Empty;
					AssertNoMessageError("When rule B2101 is disabled", grossWeightUnitInfo, expectedErrorMessage);

					CombineAssertions("When rule B2101 is enabled and not in TP", () =>
					{
						deciderTestContext.EnableRule(x => x.IsRuleB2101Active);
						goodsItem.Validation.ValidateBY_GrossWeightUnit();
						AssertHasMessageError("Gross Weight Unit is set zero", grossWeightUnitInfo, expectedErrorMessage);

						goodsItem.BY_GrossWeightUnit = Weight.Pounds;
						AssertNoMessageError("Gross Weight Unit is filled", grossWeightUnitInfo, expectedErrorMessage);
					});
				}

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					goodsItem.BY_GrossWeightUnit = ZString.Empty;
					AssertNoMessageError("When in TP", grossWeightUnitInfo, expectedErrorMessage);
				}
			}
		}

		public void TestCheckBY_Description_RuleNR0021()
		{
			const string expectedErrorMessage = "[NR0021] You have not entered a Goods Description.";
			var descriptionInfo = goodsItem.BY_DescriptionInfo;
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(x => x.IsRuleNR0021Active);
				CombineAssertions("When NR0021 active", () =>
				{
					goodsItem.BY_Description = ZString.Empty;
					AssertHasMessageError("'Goods Description' is empty", descriptionInfo, expectedErrorMessage);

					goodsItem.BY_Description = "Goods Description";
					AssertNoMessageError("'Goods Description' is not empty", descriptionInfo, expectedErrorMessage);
				});

				deciderTestContext.DisableRule(x => x.IsRuleNR0021Active);
				CombineAssertions("When NR0021 disable", () =>
				{
					goodsItem.BY_Description = ZString.Empty;
					AssertNoMessageError("'Goods Description' is empty", descriptionInfo, expectedErrorMessage);
				});
			}
		}

		public void TestCheckBY_Description_RuleE1107() => CombineAssertions(() =>
		{
			const string expectedErrorMessage = "[E1107] Length of Goods Description must not exceed 280 characters.";

			var descriptionInfo = goodsItem.BY_DescriptionInfo;
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.EnableRule(x => x.IsRuleE1107Active);

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					goodsItem.BY_Description = new string('X', 280);
					AssertNoMessageError("(E1107 active) TransitionPeriod ON and 'Goods Description' length it is 280 Chars", descriptionInfo, expectedErrorMessage);
					goodsItem.BY_Description += "X";
					AssertHasMessageError("(E1107 active) TransitionPeriod ON and 'Goods Description' length it is greater than 280 Chars", descriptionInfo, expectedErrorMessage);
				}

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					goodsItem.Validation.ValidateBY_Description();
					AssertNoMessageError("(E1107 active) TransitionPeriod OFF and 'Goods Description' length it is greater than 280 Chars", descriptionInfo, expectedErrorMessage);
				}

				deciderTestContext.DisableRule(x => x.IsRuleE1107Active);

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					goodsItem.BY_GrossWeightUnit = ZString.Empty;
					goodsItem.Validation.ValidateBY_GrossWeightUnit();
					AssertNoMessageError("(E1107 inactive) TransitionPeriod OFF and 'Goods Description' length it is greater than 280 Chars", descriptionInfo, expectedErrorMessage);
				}
			}
		});

		public void TestCheckBY_Description_RuleE1107_1() => CombineAssertions(() =>
		{
			const string expectedErrorMessage = "[E1107] In transition period, which is now, the maximum number of characters allowed is 280; excess characters will be truncated in the Message";

			var descriptionInfo = goodsItem.BY_DescriptionInfo;
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(x => x.IsRuleE1107_1Active);
				{
					using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
					{
						goodsItem.BY_Description = new string('X', 280);
						AssertNoWarning("(rule active) TransitionPeriod ON and 'Goods Description' length it is 280 Chars", descriptionInfo, expectedErrorMessage);
						goodsItem.BY_Description += "X";
						AssertHasWarning("(rule active) TransitionPeriod ON and 'Goods Description' length it is greater than 280 Chars", descriptionInfo, expectedErrorMessage);
					}

					using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
					{
						goodsItem.Validation.ValidateBY_Description();
						AssertNoWarning("(rule active) TransitionPeriod OFF and 'Goods Description' length it is greater than 280 Chars", descriptionInfo, expectedErrorMessage);
					}
				}

				deciderTestContext.DisableRule(x => x.IsRuleE1107_1Active);
				{
					using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
					{
						goodsItem.BY_GrossWeightUnit = ZString.Empty;
						goodsItem.Validation.ValidateBY_GrossWeightUnit();
						AssertNoWarning("(rule inactive) TransitionPeriod OFF and 'Goods Description' length it is greater than 280 Chars", descriptionInfo, expectedErrorMessage);
					}
				}
			}
		});

		public void TestCheckBY_Type_B1922_DeclarationTypeNotT1()
		{
			const string errorMessage = "[B1922] Declaration Type must be T1.";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(Code_CL234, "CusCodeTypeCL234");
			helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, Code_CL234, code: "XYZ", description: "Code Description", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var additionalInfo = goodsItem.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = "XYZ";
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

			var previousDocument = goodsItem.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;

			var targetInfo = goodsItem.BY_TypeInfo;
			using (var ruleTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				CombineAssertions(() =>
				{
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
					{
						ruleTestContext.ClearCachedValidationDecider(goodsItem);
						ruleTestContext.EnableRule(x => x.IsRuleB1922Active);

						goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
						AssertHasMessageError("Rule active, in Phase5TransitionPeriod, Declaration Type is not T1", targetInfo, errorMessage);

						goodsItem.BY_Type = ZString.Empty;
						AssertNoMessageError("Declaration type is empty", targetInfo, errorMessage);

						goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
						AssertNoMessageError("Declaration type is T1", targetInfo, errorMessage);

						additionalInfo.CSI_Code = "ABC";
						goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
						AssertNoMessageError("No AdditionalInfo with CL234 Type", targetInfo, errorMessage);

						additionalInfo.CSI_Code = "XYZ";
						additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
						goodsItem.Validation.ValidateBY_Type();
						AssertNoMessageError("Additional Info with CL234 Type does not have SubType REF", targetInfo, errorMessage);

						additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.C658;
						goodsItem.Validation.ValidateBY_Type();
						AssertNoMessageError("No PreviousDocument with Type N830", targetInfo, errorMessage);

						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
						ruleTestContext.DisableRule(x => x.IsRuleB1922Active);
						goodsItem.Validation.ValidateBY_Type();
						AssertNoMessageError("No error message as rule is disabled", targetInfo, errorMessage);
					}

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
					{
						ruleTestContext.EnableRule(x => x.IsRuleB1922Active);
						goodsItem.Validation.ValidateBY_Type();
						AssertNoMessageError("Rule active, outside Phase5TransitionPeriod", targetInfo, errorMessage);
					}
				});
			}
		}

		public void TestCheckBY_Type_B1922_DeclarationTypeNotT2OrT2F()
		{
			const string errorMessage = "[B1922] Declaration Type must be T2 or T2F.";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia", eun);
			helper.CreateNewOrGetExistingCusCodeType(Code_CL234, "CusCodeTypeCL234");
			helper.CreateCusCodeList(RefDataGroupingCodes.EuropeanUnionEUN, Code_CL234, code: "XYZ", description: "Code Description", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
			Factory.Save();

			var additionalInfo = goodsItem.AdditionalInfos.AddNew();
			additionalInfo.CSI_Code = "XYZ";
			additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

			var targetInfo = goodsItem.BY_TypeInfo;
			using (var ruleTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				ruleTestContext.ClearCachedValidationDecider(goodsItem);
				ruleTestContext.EnableRule(x => x.IsRuleB1922Active);

				CombineAssertions(() =>
				{
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
					{
						goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
						AssertHasMessageError("Rule active, in Phase5TransitionPeriod, Declaration Type is not T2 or T2F", targetInfo, errorMessage);

						goodsItem.BY_Type = ZString.Empty;
						AssertNoMessageError("Declaration type is empty", targetInfo, errorMessage);

						goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
						AssertNoMessageError("Declaration type is T2", targetInfo, errorMessage);

						goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedureBetweenDifferentFiscalTerritories;
						AssertNoMessageError("Declaration type is T2F", targetInfo, errorMessage);

						additionalInfo.CSI_Code = "ABC";
						goodsItem.BY_Type = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderInternalCommunityTransitProcedure;
						AssertNoMessageError("No AdditionalInfo with CL234 Type", targetInfo, errorMessage);

						additionalInfo.CSI_Code = "XYZ";
						additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
						goodsItem.Validation.ValidateBY_Type();
						AssertNoMessageError("Additional Info with CL234 Type does not have SubType REF", targetInfo, errorMessage);

						additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
						ruleTestContext.DisableRule(x => x.IsRuleB1922Active);
						goodsItem.Validation.ValidateBY_Type();
						AssertNoMessageError("No error message as rule is disabled", targetInfo, errorMessage);
					}

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
					{
						ruleTestContext.EnableRule(x => x.IsRuleB1922Active);
						goodsItem.Validation.ValidateBY_Type();
						AssertNoMessageError("Rule active, outside Phase5TransitionPeriod", targetInfo, errorMessage);
					}
				});
			}
		}

		public void TestCheckBY_RN_NKCountryOfDispatch_R0507_1Rule()
		{
			const string expectedErrorMessage = "[R0507-1] Country of Dispatch must be different for at least one of the consignment items.";

			CombineAssertions(() =>
			{
				using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(x => x.IsRuleR0507_1Active);
					deciderTestContext.DisableRule(x => x.IsRuleR0507Active);

					var bill = nctsHeader.Bills.AddNew();
					var goodsItem1 = bill.GoodsItems.AddNew();
					var goodsItem2 = bill.GoodsItems.AddNew();
					var countryOfDispatchInfo = goodsItem1.BY_RN_NKCountryOfDispatchInfo;

					goodsItem1.Validation.ValidateBY_RN_NKCountryOfDispatch();
					AssertNoMessageError("All CoD empty", countryOfDispatchInfo, expectedErrorMessage);

					goodsItem1.BY_RN_NKCountryOfDispatch = CountryCodes.Switzerland;
					AssertNoMessageError("One GoodsItem with same CoD", countryOfDispatchInfo, expectedErrorMessage);

					goodsItem2.BY_RN_NKCountryOfDispatch = CountryCodes.Switzerland;
					goodsItem1.Validation.ValidateBY_RN_NKCountryOfDispatch();
					AssertHasMessageError("Multiple GoodsItems with same CoD", countryOfDispatchInfo, expectedErrorMessage);

					bill.GoodsItems.RemoveFromRelationship(goodsItem2);
					goodsItem1.Validation.ValidateBY_RN_NKCountryOfDispatch();
					AssertNoMessageError("Only one GoodsItem with CoD", countryOfDispatchInfo, expectedErrorMessage);
				}
			});
		}

		public void TestCheckBY_CommercialReferenceNumber_R0507_1Rule()
		{
			const string expectedErrorMessage = "[R0507-1] Reference Number (UCR) must be different for at least one of the consignment items.";
			CombineAssertions(() =>
			{
				using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
				{
					deciderTestContext.EnableRule(x => x.IsRuleR0507_1Active);
					deciderTestContext.DisableRule(x => x.IsRuleR0507Active);

					var bill = nctsHeader.Bills.AddNew();
					var goodsItem1 = bill.GoodsItems.AddNew();
					var goodsItem2 = bill.GoodsItems.AddNew();
					var commercialReferenceNumberInfo = goodsItem1.BY_CommercialReferenceNumberInfo;

					goodsItem1.Validation.ValidateBY_CommercialReferenceNumber();
					AssertNoMessageError("All RefNr empty", commercialReferenceNumberInfo, expectedErrorMessage);

					goodsItem1.BY_CommercialReferenceNumber = "XX";
					AssertNoMessageError("One GoodsItem with RefNr", commercialReferenceNumberInfo, expectedErrorMessage);

					goodsItem2.BY_CommercialReferenceNumber = "XX";
					goodsItem1.Validation.ValidateBY_CommercialReferenceNumber();
					AssertHasMessageError("Multiple GoodsItems with same RefNr", commercialReferenceNumberInfo, expectedErrorMessage);

					bill.GoodsItems.RemoveFromRelationship(goodsItem2);
					goodsItem1.Validation.ValidateBY_CommercialReferenceNumber();
					AssertNoMessageError("Only one GoodsItem with RefNr", commercialReferenceNumberInfo, expectedErrorMessage);
				}
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			(nctsHeader, nctsBill, goodsItem) = GetBusinessObjectsForTest();
			movementHeader = nctsHeader.MovementHeader;
			validation = goodsItem.Validation as NctsDepartureCargoDescPhase5Validation;
		}

		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
		NctsDepartureCargoDescPhase5Validation validation;
		NctsDepartureMovementHeader movementHeader;
		NctsBill nctsBill;

		void AssertRuleR0507(Func<NctsDepartureCargoDesc, ZPropertyInfo> getTargetInfo)
		{
			var targetInfo = getTargetInfo(goodsItem);
			var humanReadableName = targetInfo.HumanReadableName;
			goodsItem.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
			var singleItemMessageError = $"[R0507] For Declaration Type = ‘T’ (Mixed transit) there must be at least two Consignment Items with different {humanReadableName}.";
			var generalMessageError = $"[R0507] {humanReadableName} must be different for at least one of the consignment items.";
			using var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory);

			CombineAssertions(() =>
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);

				deciderTestContext.EnableRule(x => x.IsRuleR0507Active);
				targetInfo.SetValueFromString("A");
				AssertHasMessageError($"Single consignment item, should trigger error message", targetInfo, singleItemMessageError);

				var goodsItem2 = goodsItem.Bill.GoodsItems.AddNew();
				var siblingInfo = getTargetInfo(goodsItem2);
				deciderTestContext.ClearCachedValidationDecider(goodsItem2);
				siblingInfo.SetValueFromString("A");
				AssertHasMessageError($"Consignment items with same {humanReadableName}", siblingInfo, generalMessageError);

				targetInfo.SetValueFromString("B");
				AssertNoMessageError($"Consignment items with different {humanReadableName}", targetInfo, generalMessageError);

				siblingInfo.SetValueFromString(ZString.Empty);
				targetInfo.SetValueFromString(ZString.Empty);
				AssertNoMessageError($"No error for empty {humanReadableName} despite both are equal", targetInfo, generalMessageError);

				siblingInfo.SetValueFromString("A");
				targetInfo.SetValueFromString("A");
				var goodsItemOfOtherBill = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
				var targetInfoOfOtherBill = getTargetInfo(goodsItemOfOtherBill);
				targetInfoOfOtherBill.SetValueFromString("A");
				AssertHasMessageError($"Consignment items with same {humanReadableName} but in different Bill but the new bill has single consignment item", targetInfoOfOtherBill, singleItemMessageError);

				goodsItem.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				targetInfoOfOtherBill.SetValueFromString("B");
				targetInfoOfOtherBill.SetValueFromString("A");
				AssertNoMessageError("Not mixed consignment", targetInfoOfOtherBill, singleItemMessageError);

				goodsItem.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;
				deciderTestContext.DisableRule(x => x.IsRuleR0507Active);
				targetInfo.SetValueFromString("B");
				targetInfo.SetValueFromString("A");
				AssertNoMessageError($"Consignment items with same {humanReadableName} but RuleR0507 deactivated", targetInfo, generalMessageError);
			});
		}

		void AssertRuleRP11(Func<NctsDepartureCargoDesc, ZPropertyInfo> targetWeightInfoProvider, Action<NctsDepartureCargoDesc> targetWeightValidationRunner)
		{
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				var (header, bill, item) = GetBusinessObjectsForTest();

				var itemPreviousDocument = item.PreviousDocuments.AddNew();
				var billPreviousDocument = bill.PreviousDocuments.AddNew();
				var headerPreviousDocument = header.PreviousDocuments.AddNew();

				var weightInfo = targetWeightInfoProvider(item);
				var errorMessage = $"[RP11] {MandatoryValidation.YouHaveNotEntered}";

				CombineAssertions(() =>
				{
					deciderTestContext.EnableRule(c => c.IsRuleRP11Active);
					AssertEquals("Target Weight is empty", true, weightInfo.Value.IsEmpty);
					targetWeightValidationRunner.Invoke(item);
					AssertNoMessageErrorContaining("No previous document of type N830", weightInfo, errorMessage);

					itemPreviousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
					targetWeightValidationRunner.Invoke(item);
					AssertHasMessageErrorContaining("Item has previous document of type N830", weightInfo, errorMessage);

					itemPreviousDocument.CSI_Code = ZString.Empty;
					billPreviousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
					targetWeightValidationRunner.Invoke(item);
					AssertHasMessageErrorContaining("Consignment has previous document of type N830", weightInfo, errorMessage);

					billPreviousDocument.CSI_Code = ZString.Empty;
					headerPreviousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
					targetWeightValidationRunner.Invoke(item);
					AssertHasMessageErrorContaining("Header has previous document of type N830", weightInfo, errorMessage);

					deciderTestContext.DisableRule(c => c.IsRuleRP11Active);
					targetWeightValidationRunner.Invoke(item);
					AssertNoMessageErrorContaining("Header has previous document of type N830, Rule RP11 is inactive", weightInfo, errorMessage);

					deciderTestContext.EnableRule(c => c.IsRuleRP11Active);
					weightInfo.SetValueFromString("2000.0");
					AssertNoMessageError("Target weight is set", weightInfo, errorMessage);
				});
			}
		}

		public void TestCheckSupportingAndAdditionalDocumentsCountValidateRuleE1407()
		{
			var (_, nctsBill, goodsItems) = GetBusinessObjectsForTest();
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItems);
				deciderTestContext.EnableRule(c => c.IsRuleE1407Active);

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					CombineAssertions("When Transition Period is ON", () =>
					{
						AddSupportingDocument(goodsItems, 30);
						AddAdditionalDocuments_GoodsItems(goodsItems, 30, "REF");
						AddAdditionalDocuments_Bills(nctsBill, 30, "TRA");
						goodsItems.Validation.ValidateAll();
						AssertNoRowMessageError("When Transition Period is ON, sum of number of documents < 99 and additional infos of type REF AND TRA, no error is expected", goodsItems, ExpectedE1407MessageError);

						RemoveAndDeleteAllDocuments(goodsItems, nctsBill);
						AddSupportingDocument(goodsItems, 50);
						AddAdditionalDocuments_GoodsItems(goodsItems, 30, "REF");
						AddAdditionalDocuments_Bills(nctsBill, 20, "TRA");
						goodsItems.Validation.ValidateAll();
						AssertHasRowMessageError("When Transition Period is ON, sum of number of documents > 99 and additional infos of type REF AND TRA, error is expected", goodsItems, ExpectedE1407MessageError);

						RemoveAndDeleteAllDocuments(goodsItems, nctsBill);
						AddSupportingDocument(goodsItems, 50);
						AddAdditionalDocuments_GoodsItems(goodsItems, 30, "REF");
						AddAdditionalDocuments_Bills(nctsBill, 20, "REF");
						goodsItems.Validation.ValidateAll();
						AssertNoRowMessageError("When Transition Period is ON, sum of number of documents < 99 and additional infos of type REF only, no error is expected", goodsItems, ExpectedE1407MessageError);

						RemoveAndDeleteAllDocuments(goodsItems, nctsBill);
						AddSupportingDocument(goodsItems, 50);
						AddAdditionalDocuments_Bills(nctsBill, 50, "TRA");
						goodsItems.Validation.ValidateAll();
						AssertHasRowMessageError("When Transition Period is ON, sum of number of documents > 99 and additional infos of type TRA only, error is expected", goodsItems, ExpectedE1407MessageError);

						RemoveAndDeleteAllDocuments(goodsItems, nctsBill);
						AddSupportingDocument(goodsItems, 50);
						AddAdditionalDocuments_GoodsItems(goodsItems, 60, "ABC");
						goodsItems.Validation.ValidateAll();
						AssertNoRowMessageError("When Transition Period is ON, sum of number of documents > 99 and additional infos not of type REF or TRA, no error is expected", goodsItems, ExpectedE1407MessageError);

						RemoveAndDeleteAllDocuments(goodsItems, nctsBill);
						AddSupportingDocument(goodsItems, 30);
						AddAdditionalDocuments_GoodsItems(goodsItems, 60, "ABC");
						goodsItems.Validation.ValidateAll();
						AssertNoRowMessageError("When Transition Period is ON, sum of number of documents < 99 and additional infos not of type REF or TRA, no error is expected", goodsItems, ExpectedE1407MessageError);
					});
				}

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					CombineAssertions("When Transition Period is OFF", () =>
					{
						RemoveAndDeleteAllDocuments(goodsItems, nctsBill);
						AddSupportingDocument(goodsItems, 10);
						AddAdditionalDocuments_GoodsItems(goodsItems, 60, "REF");
						AddAdditionalDocuments_Bills(nctsBill, 20, "TRA");
						goodsItems.Validation.ValidateAll();
						AssertNoRowMessageError("When Transition Period is OFF, sum of number of documents < 99, no error is expected", goodsItems, ExpectedE1407MessageError);

						RemoveAndDeleteAllDocuments(goodsItems, nctsBill);
						AddSupportingDocument(goodsItems, 50);
						AddAdditionalDocuments_GoodsItems(goodsItems, 30, "REF");
						AddAdditionalDocuments_Bills(nctsBill, 20, "TRA");
						goodsItems.Validation.ValidateAll();
						AssertNoRowMessageError("When Transition Period is OFF, even with exceeding sum of number of documents > 99, no error is expected", goodsItems, ExpectedE1407MessageError);
					});
				}

				deciderTestContext.DisableRule(c => c.IsRuleE1407Active);

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					CombineAssertions("When Transition Period is ON", () =>
					{
						RemoveAndDeleteAllDocuments(goodsItems, nctsBill);
						AddSupportingDocument(goodsItems, 50);
						AddAdditionalDocuments_GoodsItems(goodsItems, 30, "REF");
						AddAdditionalDocuments_Bills(nctsBill, 20, "TRA");
						goodsItems.Validation.ValidateAll();
						AssertNoRowMessageError("When RuleE1407Active is disabled, Transition Period is ON, sum of number of documents > 99 and additional infos of type REF AND TRA, error is expected", goodsItems, ExpectedE1407MessageError);

						RemoveAndDeleteAllDocuments(goodsItems, nctsBill);
						AddSupportingDocument(goodsItems, 50);
						AddAdditionalDocuments_Bills(nctsBill, 50, "TRA");
						goodsItems.Validation.ValidateAll();
						AssertNoRowMessageError("When RuleE1407Active is disabled, Transition Period is ON, sum of number of documents > 99 and additional infos of type TRA only, error is expected", goodsItems, ExpectedE1407MessageError);
					});
				}

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					CombineAssertions("When Transition Period is OFF", () =>
					{
						RemoveAndDeleteAllDocuments(goodsItems, nctsBill);
						AddSupportingDocument(goodsItems, 10);
						AddAdditionalDocuments_GoodsItems(goodsItems, 60, "REF");
						AddAdditionalDocuments_Bills(nctsBill, 20, "TRA");
						goodsItems.Validation.ValidateAll();
						AssertNoRowMessageError("When RuleE1407Active is disabled, Transition Period is OFF, sum of number of documents < 99, no error is expected", goodsItems, ExpectedE1407MessageError);

						RemoveAndDeleteAllDocuments(goodsItems, nctsBill);
						AddSupportingDocument(goodsItems, 50);
						AddAdditionalDocuments_GoodsItems(goodsItems, 30, "REF");
						AddAdditionalDocuments_Bills(nctsBill, 20, "TRA");
						goodsItems.Validation.ValidateAll();
						AssertNoRowMessageError("When RuleE1407Active is disabled, Transition Period is OFF, even with exceeding sum of number of documents > 99, no error is expected", goodsItems, ExpectedE1407MessageError);
					});
				}
			}
		}

		public void TestCheckBY_TransportChargesMethodOfPayment_RuleB1875_1()
		{
			const string expectedErrorMessage = "[B1875-1] Transport Charges MoP must be empty.";
			using var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory);
			deciderTestContext.ClearCachedValidationDecider(goodsItem);
			deciderTestContext.EnableRule(x => x.IsRuleB1875_1Active);
			CombineAssertions("When rule is enabled and Transition period is ON", () =>
			{
				using (TemporarilySetTransitionPeriod(true))
				{
					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
					movementHeader.BM_MethodOfPayment = ZString.Empty;
					goodsItem.BY_TransportChargesMethodOfPayment = ZString.Empty;
					goodsItem.Validation.ValidateBY_TransportChargesMethodOfPayment();
					AssertNoMessageError("security = NON, header transport MoP is empty, TransportChargesMethodOfPayment is empty", goodsItem.BY_TransportChargesMethodOfPaymentInfo, expectedErrorMessage);

					goodsItem.BY_TransportChargesMethodOfPayment = "M";
					goodsItem.Validation.ValidateBY_TransportChargesMethodOfPayment();
					AssertHasMessageError("security = NON, header transport MoP is empty, TransportChargesMethodOfPayment is not empty", goodsItem.BY_TransportChargesMethodOfPaymentInfo, expectedErrorMessage);

					movementHeader.BM_MethodOfPayment = "M";
					goodsItem.Validation.ValidateBY_TransportChargesMethodOfPayment();
					AssertHasMessageError("security = NON, header transport MoP is not empty, TransportChargesMethodOfPayment is not empty", goodsItem.BY_TransportChargesMethodOfPaymentInfo, expectedErrorMessage);

					movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.BTH;
					goodsItem.Validation.ValidateBY_TransportChargesMethodOfPayment();
					AssertHasMessageError("security = not NON, header transport MoP is not empty, TransportChargesMethodOfPayment is not empty", goodsItem.BY_TransportChargesMethodOfPaymentInfo, expectedErrorMessage);

					movementHeader.BM_MethodOfPayment = ZString.Empty;
					goodsItem.Validation.ValidateBY_TransportChargesMethodOfPayment();
					AssertNoMessageError("security = not NON, header transport MoP is empty, TransportChargesMethodOfPayment is not empty", goodsItem.BY_TransportChargesMethodOfPaymentInfo, expectedErrorMessage);
				}
			});

			using (TemporarilySetTransitionPeriod(false))
			{
				movementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
				movementHeader.BM_MethodOfPayment = "M";
				goodsItem.BY_TransportChargesMethodOfPayment = "M";
				goodsItem.Validation.ValidateBY_TransportChargesMethodOfPayment();
				AssertNoMessageError("Rule is enabled but transition period is OFF", goodsItem.BY_TransportChargesMethodOfPaymentInfo, expectedErrorMessage);
			}

			deciderTestContext.DisableRule(x => x.IsRuleB1875_1Active);
			using (TemporarilySetTransitionPeriod(true))
			{
				AssertNoMessageError("transition period is ON but rule is disabled", goodsItem.BY_TransportChargesMethodOfPaymentInfo, expectedErrorMessage);
			}

			using (TemporarilySetTransitionPeriod(false))
			{
				AssertNoMessageError("transition period is OFF and rule is disabled", goodsItem.BY_TransportChargesMethodOfPaymentInfo, expectedErrorMessage);
			}
		}

		public void TestCheckBY_HarmonisedTariff_SingleErrorMessageForEmptyCommodityCode()
		{
			var expectedMessage = "The code you have selected is not in the list.";

			var (header, bill, goodsItem) = GetBusinessObjectsForTest();

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(x => x.IsRuleC0153_1Active);

				goodsItem.BY_HarmonisedTariff = ZString.Empty;
				goodsItem.Validation.ValidateBY_HarmonisedTariff();
				AssertNoMessageError("Commodity Code: empty", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedMessage);

				goodsItem.BY_HarmonisedTariff = "123456";
				goodsItem.Validation.ValidateBY_HarmonisedTariff();
				AssertHasMessageError("Commodity Code: invalid", goodsItem.BY_HarmonisedTariffInfo, expectedMessage);
			}
		}

		public void TestCheckBY_HarmonisedTariff_RuleC0153_1()
		{
			var expectedErrorMessage = "[C0153-1] You have not entered a Commodity Code.";

			var (header, bill, goodsItem) = GetBusinessObjectsForTest();
			var previousDocument = Factory.New<CommonPreviousDocument>();
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(x => x.IsRuleC0153_1Active);
				using (TemporarilySetTransitionPeriod(false))
				{
					CombineAssertions("When RuleC0153-1 is enabled and Transition period is OFF", () =>
					{
						var movementHeader = header.MovementHeader;
						movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
						goodsItem.Validation.ValidateBY_HarmonisedTariff();
						AssertHasMessageError("PreviousDocument Not Added, BM_InBondEntryType: Not TIR", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);

						movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
						goodsItem.Validation.ValidateBY_HarmonisedTariff();
						AssertNoMessageError("PreviousDocument Not Added, BM_InBondEntryType: TIR", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);

						bill.PreviousDocuments.Add(previousDocument);
						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.C658;
						movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
						goodsItem.Validation.ValidateBY_HarmonisedTariff();
						AssertNoMessageError("CSI_Code: Not N830, BM_InBondEntryType: TIR", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);

						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.C658;
						movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
						goodsItem.Validation.ValidateBY_HarmonisedTariff();
						AssertHasMessageError("CSI_Code: Not N830, BM_InBondEntryType: Not TIR", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);

						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
						movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
						goodsItem.Validation.ValidateBY_HarmonisedTariff();
						AssertHasMessageError("CSI_Code: N830, BM_InBondEntryType: TIR", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);

						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
						movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
						goodsItem.Validation.ValidateBY_HarmonisedTariff();
						AssertHasMessageError("CSI_Code: N830, BM_InBondEntryType: Not TIR", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);

						goodsItem.BY_FormattedHarmonisedTariff = "110000001";
						goodsItem.Validation.ValidateBY_HarmonisedTariff();
						AssertNoMessageError("CSI_Code: N830, BM_InBondEntryType: Not TIR, BY_FormattedHarmonisedTariff: Not Empty", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);
						goodsItem.BY_FormattedHarmonisedTariff = string.Empty;
					});

					deciderTestContext.DisableRule(x => x.IsRuleC0153_1Active);
					previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
					movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
					goodsItem.Validation.ValidateBY_HarmonisedTariff();
					AssertNoMessageError("RuleC0153-1: False, TransitionPeriod: OFF, CSI_Code: N830, BM_InBondEntryType: TIR", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);
				}

				using (TemporarilySetTransitionPeriod(true))
				{
					deciderTestContext.EnableRule(x => x.IsRuleC0153_1Active);
					CombineAssertions("When RuleC0153-1 is enabled and Transition period is ON", () =>
					{
						goodsItem.Validation.ValidateBY_HarmonisedTariff();
						AssertNoMessageError("CSI_Code: N830, BM_InBondEntryType: TIR", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);

						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
						movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
						goodsItem.Validation.ValidateBY_HarmonisedTariff();
						AssertNoMessageError("CSI_Code: N830, BM_InBondEntryType: Not TIR", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);

						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.C658;
						movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.T2PlusSanMarino;
						goodsItem.Validation.ValidateBY_HarmonisedTariff();
						AssertNoMessageError("CSI_Code: Not N830, BM_InBondEntryType: Not TIR", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);
					});
				}
			}
		}

		public void TestCheckBY_NetWeight_RuleC0837_1()
		{
			var errorMessage = "[C0837-1] Net weight must be empty";
			var nctsBill = nctsHeader.Bills.AddNew();
			var billGoodsItem = nctsBill.GoodsItems.AddNew();

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(billGoodsItem);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					deciderTestContext.EnableRule(x => x.IsRuleC0837_1Active);

					CombineAssertions("When RuleC0837_1: Active, Transition Period: OFF", () =>
					{
						movementHeader.BM_ReducedDatasetIndicator = true;
						billGoodsItem.BY_NetWeight = 10m;
						AssertHasMessageError("PreviousDocument: Not Added, BM_ReducedDatasetIndicator: True, BY_NetWeight: 10m", billGoodsItem.BY_NetWeightInfo, errorMessage);

						var previousDocument = nctsBill.PreviousDocuments.AddNew();
						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.N830;
						movementHeader.BM_ReducedDatasetIndicator = true;
						billGoodsItem.BY_NetWeight = 10m;
						AssertNoMessageError("PreviousDocument.CSI_Code: N830, BM_ReducedDatasetIndicator: True, BY_NetWeight: 10m", billGoodsItem.BY_NetWeightInfo, errorMessage);

						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.C658;
						movementHeader.BM_ReducedDatasetIndicator = false;
						billGoodsItem.Validation.ValidateBY_NetWeight();
						AssertNoMessageError("PreviousDocument.CSI_Code: C658, BM_ReducedDatasetIndicator: false, BY_NetWeight: 10m", billGoodsItem.BY_NetWeightInfo, errorMessage);

						billGoodsItem.BY_NetWeight = 0.0m;
						movementHeader.BM_ReducedDatasetIndicator = true;
						billGoodsItem.Validation.ValidateBY_NetWeight();
						AssertNoMessageError("PreviousDocument.CSI_Code: C658, BM_ReducedDatasetIndicator: true, BY_NetWeight: 0m", billGoodsItem.BY_NetWeightInfo, errorMessage);

						billGoodsItem.BY_NetWeight = 10m;
						AssertHasMessageError("PreviousDocument.CSI_Code: C658, BM_ReducedDatasetIndicator: true, BY_NetWeight: 10m", billGoodsItem.BY_NetWeightInfo, errorMessage);

						previousDocument.CSI_Code = NctsConstants.NctsTypeOfPreviousDocument.Codes.C651;
						billGoodsItem.Validation.ValidateBY_NetWeight();
						AssertHasMessageError("PreviousDocument.CSI_Code: C651, BM_ReducedDatasetIndicator: true, BY_NetWeight: 10m", billGoodsItem.BY_NetWeightInfo, errorMessage);
					});

					deciderTestContext.DisableRule(x => x.IsRuleC0837_1Active);
					billGoodsItem.Validation.ValidateBY_NetWeight();
					AssertNoMessageError("When RuleC0837_1: Disabled, Transition Period: OFF, PreviousDocument.CSI_Code: C658, BM_ReducedDatasetIndicator: true, BY_NetWeight: 10m",
						billGoodsItem.BY_NetWeightInfo, errorMessage);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					deciderTestContext.EnableRule(x => x.IsRuleC0837_1Active);
					billGoodsItem.Validation.ValidateBY_NetWeight();
					AssertNoMessageError("When RuleC0837_1: Active, Transition Period: ON, PreviousDocument.CSI_Code: C658, BM_ReducedDatasetIndicator: true, BY_NetWeight: 10m",
						billGoodsItem.BY_NetWeightInfo, errorMessage);
				}
			}
		}

		public void TestCheckBM_InBondEntryType_ConditionC901()
		{
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.DisableRule(x => x.IsRuleC901Active);
				goodsItem.MoveHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				goodsItem.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(goodsItem, "C901");

				deciderTestContext.EnableRule(x => x.IsRuleC901Active);
				goodsItem.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(goodsItem, "C901");
				var supportingDoc = goodsItem.SupportingDocuments.AddNew();
				supportingDoc.CSI_Code = NctsHeaderValidationHelper.TirCarnetDocumentCode;
				goodsItem.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(goodsItem, "C901");
			}
		}

		public void TestCheckBY_HarmonisedTariff_RuleNR0025()
		{
			var expectedErrorMessage = "[NR0025] You have not entered a Commodity Code.";

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: true))
				{
					deciderTestContext.EnableRule(x => x.IsRuleNR0025Active);
					CombineAssertions("When NR0025 is enabled and Transition period is ON", () =>
					{
						AssertNoMessageError("Guarantee empty, CommodityCode not mandatory", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);

						var guarantee = movementHeader.Guarantees.AddNew();
						goodsItem.Validation.ValidateBY_HarmonisedTariff();
						AssertNoMessageError("Exists Guarantee with PW_BondType != 2, CommodityCode not mandatory", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);

						guarantee.PW_BondType = "2";
						goodsItem.Validation.ValidateBY_HarmonisedTariff();
						AssertHasMessageError("Exists Guarantee with PW_BondType == 2, CommodityCode mandatory", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);

						goodsItem.BY_HarmonisedTariff = "XXX";
						AssertNoMessageError("Exists Guarantee with PW_BondType == 2, CommodityCode not empty", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);
					});
				}

				using (TemporarilySetTransitionPeriod(isTransitionPeriodActive: false))
				{
					deciderTestContext.EnableRule(x => x.IsRuleNR0025Active);
					var guarantee = movementHeader.Guarantees.AddNew();
					guarantee.PW_BondType = "2";
					goodsItem.Validation.ValidateBY_HarmonisedTariff();
					AssertNoMessageError("Exists Guarantee with PW_BondType != 2, CommodityCode not mandatory", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedErrorMessage);
				}
			}
		}

		public void TestCheckBY_BondedWhsQuantity_Mandatory_RuleNR0040()
		{
			var expectedErrorMessage = "[NR0040] You have not entered a Warehouse Quantity.";

			var (_, _, goodsItem) = GetBusinessObjectsForTest();

			using var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory);
			deciderTestContext.ClearCachedValidationDecider(goodsItem);

			deciderTestContext.EnableRule(x => x.IsRuleNR0040Active);
			AssertNullOrEmpty("Precondition", goodsItem.BY_BondedWhsUnitQty);
			ValidationTestHelper.AssertFieldIsNotMandatory(goodsItem.BY_BondedWhsQuantityInfo, expectedErrorMessage);

			goodsItem.BY_BondedWhsUnitQty = "KG";
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(goodsItem.BY_BondedWhsQuantityInfo, expectedErrorMessage);

			deciderTestContext.DisableRule(x => x.IsRuleNR0040Active);
			goodsItem.Validation.ValidateBY_BondedWhsQuantity();
			ValidationTestHelper.AssertFieldIsNotMandatory(goodsItem.BY_BondedWhsQuantityInfo, expectedErrorMessage);
		}

		public void TestCheckBY_BondedWhsQuantity_Integer_RuleNR0041()
		{
			const string integerRequiredMessage = "[NR0041] Unit of quantity 'NAR', 'NARB', 'NCL' or 'NPR' requires an Integer value.";

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				var (_, _, goodsItem) = GetBusinessObjectsForTest();

				deciderTestContext.EnableRule(x => x.IsRuleNR0041Active);
				goodsItem.BY_BondedWhsUnitQty = "KG";
				goodsItem.BY_BondedWhsQuantity = 1.2;
				AssertNoMessageError(goodsItem.BY_BondedWhsQuantityInfo, integerRequiredMessage);

				goodsItem.BY_BondedWhsUnitQty = "NAR";
				goodsItem.BY_BondedWhsQuantity = 1.2;
				AssertHasMessageError(goodsItem.BY_BondedWhsQuantityInfo, integerRequiredMessage);

				deciderTestContext.DisableRule(x => x.IsRuleNR0041Active);
				goodsItem.Validation.ValidateBY_BondedWhsQuantity();
				AssertNoMessageError(goodsItem.BY_BondedWhsQuantityInfo, integerRequiredMessage);
				deciderTestContext.EnableRule(x => x.IsRuleNR0041Active);

				goodsItem.BY_BondedWhsUnitQty = "NAR";
				goodsItem.BY_BondedWhsQuantity = 1;
				AssertNoMessageError(goodsItem.BY_BondedWhsQuantityInfo, integerRequiredMessage);
			}
		}

		public void TestCheckBY_BondedWhsUnitQty_Mandatory_RuleNR0042()
		{
			var expectedErrorMessage = "[NR0042] You have not entered a Warehouse Unit of Quantity.";

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				var (_, _, goodsItem) = GetBusinessObjectsForTest();

				AssertEquals("Precondition", (decimal)0, goodsItem.BY_BondedWhsQuantity);

				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(x => x.IsRuleNR0042Active);
				ValidationTestHelper.AssertFieldIsNotMandatory(goodsItem.BY_BondedWhsUnitQtyInfo, expectedErrorMessage);

				goodsItem.BY_BondedWhsQuantity = 1;
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(goodsItem.BY_BondedWhsUnitQtyInfo, expectedErrorMessage);

				deciderTestContext.DisableRule(x => x.IsRuleNR0042Active);
				goodsItem.Validation.ValidateBY_BondedWhsUnitQty();
				ValidationTestHelper.AssertFieldIsNotMandatory(goodsItem.BY_BondedWhsUnitQtyInfo, expectedErrorMessage);
			}
		}

		public void TestCheckBY_BondedWhsUnitQty_InvalidCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Germany))
			{
				var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
				var eun = helper.CreateNewOrGetExistingDataGrouping(RefDataGroupingCodes.EuropeanUnionEUN);
				helper.CreateNewOrGetExistingDataGrouping(Germany, parent: eun);
				helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "CustomsUQ");
				helper.CreateCusCodeList(Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UQ1", "UQ 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
				Factory.Save();

				var (_, _, goodsItem) = GetBusinessObjectsForTest();

				ValidationTestHelper.AssertInvalidCodeMessageError(goodsItem.BY_BondedWhsUnitQtyInfo, "UQ2", "UQ1");
			}
		}

		public void TestCheckBY_WarehouseEntryLineNo_Mandatory_RuleNR0043()
		{
			var expectedErrorMessage = "[NR0043] You have not entered a Previous Entry Line.";

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				var (_, _, goodsItem) = GetBusinessObjectsForTest();

				AssertNullOrEmpty("Precondition", goodsItem.BY_WarehouseEntryNumber);

				deciderTestContext.EnableRule(x => x.IsRuleNR0043Active);
				ValidationTestHelper.AssertFieldIsNotMandatory(goodsItem.BY_WarehouseEntryLineNoInfo, expectedErrorMessage);

				goodsItem.BY_WarehouseEntryNumber = "123";
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(goodsItem.BY_WarehouseEntryLineNoInfo, expectedErrorMessage);

				deciderTestContext.DisableRule(x => x.IsRuleNR0043Active);
				goodsItem.Validation.ValidateBY_WarehouseEntryLineNo();
				ValidationTestHelper.AssertFieldIsNotMandatory(goodsItem.BY_WarehouseEntryLineNoInfo, expectedErrorMessage);
			}
		}

		public void TestCheckBY_BondedWHSOrderLineNumber_Mandatory_RuleNR0044()
		{
			var expectedErrorMessage = "[NR0044] You have not entered a Warehouse Order Line.";

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				var (_, _, goodsItem) = GetBusinessObjectsForTest();

				AssertNullOrEmpty("Precondition", goodsItem.BY_BondedWHSOrderNumber);

				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(x => x.IsRuleNR0044Active);
				ValidationTestHelper.AssertFieldIsNotMandatory(goodsItem.BY_BondedWHSOrderLineNumberInfo, expectedErrorMessage);

				goodsItem.BY_BondedWHSOrderNumber = "123";
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(goodsItem.BY_BondedWHSOrderLineNumberInfo, expectedErrorMessage);

				deciderTestContext.DisableRule(x => x.IsRuleNR0044Active);
				goodsItem.Validation.ValidateBY_BondedWHSOrderNumber();
				ValidationTestHelper.AssertFieldIsNotMandatory(goodsItem.BY_BondedWHSOrderLineNumberInfo, expectedErrorMessage);
			}
		}

		public void TestCheckBY_OP_PartIsValidZGuid_RuleNR0045()
		{
			var expectedErrorMessage = "[NR0045] Please enter a valid Product which is required for Inventory Management integration.";

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.FillWithValidTestData();
			var validPart = Factory.New<OrgSupplierPart>();
			validPart.OP_PartNum = "T1";
			validPart.RelatedOrganisations.AddSupplier(orgHeader);
			var invalidPart = Factory.New<OrgSupplierPart>();
			invalidPart.OP_PartNum = "T2";
			Factory.Save();

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				var (header, bill, goodsItem) = GetBusinessObjectsForTest();

				AssertNullOrEmpty("Precondition", goodsItem.BY_BondedWHSOrderNumber);

				deciderTestContext.EnableRule(x => x.IsRuleNR0045Active);

				bill.Consignor.OrganisationPK = orgHeader.PK;
				ValidationTestHelper.AssertErrorIfInvalidPK(goodsItem.BY_OP_PartInfo, invalidPart.PK, validPart.PK, expectedErrorMessage);

				header.Consignor.OrganisationPK = orgHeader.PK;
				ValidationTestHelper.AssertErrorIfInvalidPK(goodsItem.BY_OP_PartInfo, invalidPart.PK, validPart.PK, expectedErrorMessage);

				deciderTestContext.DisableRule(x => x.IsRuleNR0045Active);
				goodsItem.BY_OP_Part = invalidPart.PK;
				AssertNoMessageError(goodsItem.BY_OP_PartInfo, expectedErrorMessage);
			}
		}

		public void TestCheckBY_HarmonisedTariff_RuleC0821_1()
		{
			var expectedWarning = "[C0821-1] For the 'Departure Office' that has been used, only the first 6 characters of the 'Commodity Code' will be entered in the Message.";
			var factory = Factory;
			UniversalReferenceTestDataHelper.EnsureCountriesAreInRefCusCodeList(factory, Code_CL112, (Italy, "Italy"));
			factory.Save();

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);

				deciderTestContext.EnableRule(x => x.IsRuleC0821_1Active);
				movementHeader.CustomsOffices.RemoveAndDeleteAll();
				var nctsEuOfficeCode = movementHeader.CustomsOffices.AddNew();
				nctsEuOfficeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				nctsEuOfficeCode.CY_Data = "IT123456";

				CombineAssertions("When rule active", () =>
				{
					goodsItem.BY_FormattedHarmonisedTariff = "8247149";
					AssertHasWarning("When in set CL112, code is departure and tarrif code length > 6", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedWarning);

					goodsItem.BY_FormattedHarmonisedTariff = "824756";
					AssertNoWarning("When in set CL112, code is departure and tarrif code length = 6", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedWarning);

					goodsItem.BY_FormattedHarmonisedTariff = "82475";
					AssertNoWarning("When in set CL112, code is departure and tarrif code length < 6", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedWarning);

					goodsItem.BY_FormattedHarmonisedTariff = ZString.Empty;
					AssertNoWarning("When in set CL112, code is departure and tarrif code is Empty", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedWarning);

					nctsEuOfficeCode.CY_Data = "PL123456";
					goodsItem.BY_FormattedHarmonisedTariff = "8247149";
					AssertNoWarning("When not in set CL112, code is departure and tarrif code length > 6", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedWarning);

					nctsEuOfficeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSActualOfficeOfDestination;
					nctsEuOfficeCode.CY_Data = "IT123456";
					goodsItem.Validation.ValidateBY_HarmonisedTariff();
					AssertNoWarning("When in set CL112, code is not departure and tarrif code length > 6", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedWarning);
				});

				deciderTestContext.DisableRule(x => x.IsRuleC0821_1Active);
				nctsEuOfficeCode.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture;
				nctsEuOfficeCode.CY_Data = "IT123456";
				goodsItem.BY_FormattedHarmonisedTariff = "82471495";
				AssertNoWarning("When rule inactive", goodsItem.BY_FormattedHarmonisedTariffInfo, expectedWarning);
			}
		}

		public void TestCheckUNDGs_ThereAreErrorsWithinDangerousGoods()
		{
			using var temporaryFunctionality = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, ZDate.Today, true);
			using var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory);
			ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleE1406Active));
			ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleTR0027Active));

			var substance = Factory.New<UNDGSubstance>();
			substance.DG_Code = "1234a";

			CombineAssertions(() =>
			{
				goodsItem.Validation.ValidateUNDGsAsString();
				AssertNoNotifications("When UNDGs Count = 0", goodsItem.UNDGsAsStringInfo);

				var undg1 = goodsItem.UNDGs.AddNew();
				undg1.SubstancePK = substance.PK;
				goodsItem.Validation.ValidateUNDGsAsString();
				AssertNoNotifications("When UNDGs Count = 1", goodsItem.UNDGsAsStringInfo);

				var undg2 = goodsItem.UNDGs.AddNew();
				undg2.SubstancePK = substance.PK;
				goodsItem.Validation.ValidateUNDGsAsString();
				AssertHasMessageErrorContaining("When UNDGs Count > 1", goodsItem.UNDGsAsStringInfo, "There are errors within the 'Dangerous Goods', please click on 'More' to view the error information");
			});
		}

		public void TestCheckBY_RN_NKCountryOfOriginNR0058()
		{
			NCTSTestHelper.AssertCheckBY_RN_NKCountryOfOriginNR0058<INctsDepartureCargoDescPhase5ValidationDecider>(Factory, goodsItem, validationRuleConfiguration);
		}

		public void TestCheckBY_MonetaryValueNR0059()
		{
			NCTSTestHelper.AssertCheckBY_MonetaryValueNR0059<INctsDepartureCargoDescPhase5ValidationDecider>(Factory, goodsItem, validationRuleConfiguration);
		}

		public void TestCheckBY_SupplementsNR0060()
		{
			NCTSTestHelper.AssertCheckBY_SupplementsNR0060<INctsDepartureCargoDescPhase5ValidationDecider>(Factory, goodsItem, validationRuleConfiguration);
		}

		public void TestCheckBY_Supplements()
		{
			NCTSTestHelper.AssertCheckBY_Supplements_HasChildValidationNotification(goodsItem);
		}

		public void TestCheckBY_LinePrice_TR0076()
		{
			const string messageError = "[TR0076] The Line Price for Liability Amount must be captured in all Items.";

			var goodsItem1 = goodsItem;
			var goodsItem2 = nctsBill.Header.Bills.AddNew().GoodsItems.AddNew();
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem1);
				deciderTestContext.ClearCachedValidationDecider(goodsItem2);
				deciderTestContext.EnableRule(dtc => dtc.IsRuleTR0076Active);

				goodsItem1.BY_LinePrice = 0;
				goodsItem2.BY_LinePrice = 0;
				goodsItem1.Validation.ValidateBY_LinePrice();
				goodsItem2.Validation.ValidateBY_LinePrice();
				CombineAssertions("All LinePrices are 0", () =>
				{
					AssertNoMessageError(goodsItem1.BY_LinePriceInfo, messageError);
					AssertNoMessageError(goodsItem2.BY_LinePriceInfo, messageError);
				});

				goodsItem1.BY_LinePrice = 1;
				goodsItem2.BY_LinePrice = 1;
				goodsItem1.Validation.ValidateBY_LinePrice();
				goodsItem2.Validation.ValidateBY_LinePrice();
				CombineAssertions("All LinePrices are not 0", () =>
				{
					AssertNoMessageError(goodsItem1.BY_LinePriceInfo, messageError);
					AssertNoMessageError(goodsItem2.BY_LinePriceInfo, messageError);
				});

				goodsItem1.BY_LinePrice = 1;
				goodsItem2.BY_LinePrice = 0;
				goodsItem1.Validation.ValidateBY_LinePrice();
				goodsItem2.Validation.ValidateBY_LinePrice();
				CombineAssertions("LinePrice1 is 1", () =>
				{
					AssertNoMessageError(goodsItem1.BY_LinePriceInfo, messageError);
					AssertHasMessageError(goodsItem2.BY_LinePriceInfo, messageError);
				});

				goodsItem1.BY_LinePrice = 0;
				goodsItem2.BY_LinePrice = 1;
				goodsItem1.Validation.ValidateBY_LinePrice();
				goodsItem2.Validation.ValidateBY_LinePrice();
				CombineAssertions("LinePrice2 is 1", () =>
				{
					AssertHasMessageError(goodsItem1.BY_LinePriceInfo, messageError);
					AssertNoMessageError(goodsItem2.BY_LinePriceInfo, messageError);
				});

				deciderTestContext.DisableRule(dtc => dtc.IsRuleTR0076Active);
				goodsItem1.Validation.ValidateBY_LinePrice();
				goodsItem2.Validation.ValidateBY_LinePrice();
				CombineAssertions("RuleTR0076 disabled", () =>
				{
					AssertNoMessageError(goodsItem1.BY_LinePriceInfo, messageError);
					AssertNoMessageError(goodsItem2.BY_LinePriceInfo, messageError);
				});
			}
		}

		public void TestCheckBY_LinePrice_TR0100()
		{
			const string messageError = "[TR0100] Only values >=0 must be entered.";

			var goodsItem1 = goodsItem;
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem1);
				deciderTestContext.EnableRule(dtc => dtc.IsRuleTR0100Active);

				goodsItem1.BY_LinePrice = 0;
				goodsItem1.Validation.ValidateBY_LinePrice();
				AssertNoError(goodsItem1.BY_LinePriceInfo, messageError);

				goodsItem1.BY_LinePrice = -1;
				goodsItem1.Validation.ValidateBY_LinePrice();
				AssertHasError(goodsItem1.BY_LinePriceInfo, messageError);

				deciderTestContext.DisableRule(dtc => dtc.IsRuleTR0100Active);
				goodsItem1.Validation.ValidateBY_LinePrice();
				AssertNoError("RuleTR0100 disabled", goodsItem1.BY_LinePriceInfo, messageError);
			}
		}

		public void TestCheckBY_RX_NKLinePriceCurrency_TR0068()
		{
			const string messageError = "[TR0068] You have not entered a Line Price Currency.";

			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(dtc => dtc.IsRuleTR0068Active);

				goodsItem.BY_LinePrice = 0;
				goodsItem.BY_RX_NKLinePriceCurrency = string.Empty;
				goodsItem.Validation.ValidateBY_RX_NKLinePriceCurrency();
				AssertNoError("BY_LinePrice is 0", goodsItem.BY_RX_NKLinePriceCurrencyInfo, messageError);

				goodsItem.BY_LinePrice = 1;
				goodsItem.BY_RX_NKLinePriceCurrency = string.Empty;
				goodsItem.Validation.ValidateBY_RX_NKLinePriceCurrency();
				AssertHasError("BY_LinePrice is not 0", goodsItem.BY_RX_NKLinePriceCurrencyInfo, messageError);

				deciderTestContext.DisableRule(dtc => dtc.IsRuleTR0068Active);
				goodsItem.BY_LinePrice = 1;
				goodsItem.BY_RX_NKLinePriceCurrency = string.Empty;
				goodsItem.Validation.ValidateBY_RX_NKLinePriceCurrency();
				AssertNoError("RuleTR0068 disabled", goodsItem.BY_RX_NKLinePriceCurrencyInfo, messageError);
			}
		}

		public void TestGoodsItem_TR0096RuleEnbaled() => CombineAssertions(() =>
		{
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.EnableRule(c => c.IsRuleTR0096Active);
				var errorMessage = nctsHeader.Configuration.ValidationRuleConfiguration.Messages.TR0096Message;

				goodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("expected no error, because no Guarantee exists yet", goodsItem, errorMessage);

				var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
				goodsItem.Validation.ValidateAll();
				AssertHasRowMessageError("expected error, because no DTY record exists for the Goods Item and guarantee is not overriden", goodsItem, errorMessage);

				guarantee.PW_Override = true;
				goodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("expected no error, because the guarantee calculation is overriden now", goodsItem, errorMessage);

				guarantee.PW_Override = false;
				var fee = goodsItem.Fees.AddNew();
				fee.BFE_ChargeType = NctsCommonCargoDesc.ChargeType.Duty;
				goodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("expected no error, because a DTY record exists in the Goods Item Fees", goodsItem, errorMessage);
			}
		});

		public void TestGoodsItem_TR0096RuleDisabled()
		{
			using (var deciderTestContext = new CargoDescValidationDeciderTestContext<INctsDepartureCargoDescPhase5ValidationDecider>(Factory))
			{
				deciderTestContext.ClearCachedValidationDecider(goodsItem);
				deciderTestContext.DisableRule(c => c.IsRuleTR0096Active);
				var errorMessage = nctsHeader.Configuration.ValidationRuleConfiguration.Messages.TR0096Message;

				var guarantee = nctsHeader.GetEffectiveGuarantees().AddNew();
				goodsItem.Validation.ValidateAll();
				AssertNoRowMessageError("expected no error, because the RuleTR0096 is disabled", goodsItem, errorMessage);
			}
		}

		void AddSupportingDocument(NctsDepartureCargoDesc goodsItems, int numberOfDocuments)
		{
			for (var i = 0; i < numberOfDocuments; i++)
			{
				goodsItems.SupportingDocuments.AddNew();
			}
		}

		void AddAdditionalDocuments_GoodsItems(NctsDepartureCargoDesc goodsItems, int numberOfDocuments, string subType)
		{
			for (var i = 0; i < numberOfDocuments; i++)
			{
				var additionalInfo = goodsItems.AdditionalInfos.AddNew();
				additionalInfo.CSI_SubType = subType;
			}
		}

		void AddAdditionalDocuments_Bills(NctsBill nctsBill, int numberOfDocuments, string subType)
		{
			for (var i = 0; i < numberOfDocuments; i++)
			{
				var additionalInfo = nctsBill.AdditionalDocuments.AddNew();
				additionalInfo.CSI_SubType = subType;
			}
		}

		void RemoveAndDeleteAllDocuments(NctsDepartureCargoDesc goodsItems, NctsBill nctsBill)
		{
			goodsItems.SupportingDocuments.RemoveAndDeleteAll();
			goodsItems.AdditionalInfos.RemoveAndDeleteAll();
			nctsBill.AdditionalDocuments.RemoveAndDeleteAll();
		}

		(NctsHeader, NctsBill, NctsDepartureCargoDesc) GetBusinessObjectsForTest()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var nctsBill = nctsHeader.Bills.AddNew();
			var goodsItem = nctsBill.GoodsItems.AddNew();
			return (nctsHeader, nctsBill, goodsItem);
		}

		IDisposable TemporarilySetTransitionPeriod(bool isTransitionPeriodActive) => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, isTransitionPeriodActive);

		const string RuleC0502ExpectedMessageError = "[C0502] You have not entered a Reference Number / UCR. A Reference Number / UCR at Goods Items level is required if there is no Reference Number / UCR at Header or House Consignments level and there is no Transport Document at Header or House Consignments level.";

		const string ExpectedE1407MessageError = "[E1407] In transition period, which is now, the maximum cumulative number of Supporting Documents, Transport Document and Additional Reference must not exceed 99";

		readonly ValidationRuleConfiguration validationRuleConfiguration = new ValidationRuleConfiguration();
	}
}
