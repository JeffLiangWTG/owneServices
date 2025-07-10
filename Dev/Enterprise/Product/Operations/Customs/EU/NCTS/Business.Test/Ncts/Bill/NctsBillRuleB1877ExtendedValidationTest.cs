using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsBillRuleB1877ExtendedValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNctsBillRuleB1877ExtendedValidationConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NctsBillRuleB1877ExtendedValidation(nctsBill: null));
		}

		public void TestValidateHouseConsignment_AllGoodItemsConsigneesHaveSameEORI()
		{
			var errorMessage = "[B1877-1] At least one Goods item must have a consignee with different EORI/TCU code";
			var goodItems1 = nctsBill.GoodsItems.AddNew();
			var goodItems2 = nctsBill.GoodsItems.AddNew();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader1 = orgHeader1.Addresses.AddNew();

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader2 = orgHeader2.Addresses.AddNew();

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleB1877_1Active));
					CombineAssertions("When Transition Period: ON, RuleB1877: Active", () =>
					{
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("Consignee is not added", nctsBill, errorMessage);

						goodItems1.Consignee.E2_OA_Address = orgAddressHeader1.PK;
						goodItems2.Consignee.E2_OA_Address = orgAddressHeader2.PK;
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("Consignee without EORI, added", nctsBill, errorMessage);

						orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321", Core.Constants.CountryCodes.Germany);
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("Not all GoodItems having Consignee with EORI added", nctsBill, errorMessage);

						orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.Germany);
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("All GoodItems having Consignee with different EORI", nctsBill, errorMessage);

						orgHeader1.CustomsCodes.RemoveAndDeleteAll();
						orgHeader2.CustomsCodes.RemoveAndDeleteAll();
						orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.UnitedStates);
						orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.UnitedStates);
						nctsBill.Validation.ValidateAll();
						AssertHasRowMessageError("All GoodItems having Consignee with same EORI", nctsBill, errorMessage);
					});

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleB1877_1Active));
					nctsBill.Validation.ValidateAll();
					AssertNoRowMessageError("When Transition Period: ON, RuleB1877: OFF and all GoodItems having Consignee with same EORIs", nctsBill, errorMessage);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleB1877_1Active));

					nctsBill.Validation.ValidateAll();
					AssertNoRowMessageError("When Transition Period: OFF, RB1877: Active and all GoodItems having Consignee with same EORIs", nctsBill, errorMessage);
				}
			}
		}

		public void TestValidateHouseConsignment_AllGoodItemsConsigneesHaveSameTCU()
		{
			var errorMessage = "[B1877-1] At least one Goods item must have a consignee with different EORI/TCU code";
			var goodItems1 = nctsBill.GoodsItems.AddNew();
			var goodItems2 = nctsBill.GoodsItems.AddNew();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader1 = orgHeader1.Addresses.AddNew();

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader2 = orgHeader2.Addresses.AddNew();

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleB1877_1Active));
					CombineAssertions("When Transition Period: ON, RuleB1877: Active", () =>
					{
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("Consignee is not added", nctsBill, errorMessage);

						goodItems1.Consignee.E2_OA_Address = orgAddressHeader1.PK;
						goodItems2.Consignee.E2_OA_Address = orgAddressHeader2.PK;
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("Consignee without EORI/TCU, added", nctsBill, errorMessage);

						orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "987654321", Core.Constants.CountryCodes.Germany);
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("Not all GoodItems having Consignee with TCU added", nctsBill, errorMessage);

						orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.Germany);
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("All GoodItems having Consignee with different TCUs", nctsBill, errorMessage);

						orgHeader1.CustomsCodes.RemoveAndDeleteAll();
						orgHeader2.CustomsCodes.RemoveAndDeleteAll();
						orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.UnitedStates);
						orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123454321", Core.Constants.CountryCodes.UnitedStates);
						orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);
						orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("All GoodItems having Consignee with same TCUs, but EROIs also added", nctsBill, errorMessage);

						orgHeader1.CustomsCodes.RemoveAndDeleteAll();
						orgHeader2.CustomsCodes.RemoveAndDeleteAll();
						orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);
						orgHeader2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "123456789", Core.Constants.CountryCodes.UnitedStates);
						nctsBill.Validation.ValidateAll();
						AssertHasRowMessageError("All GoodItems having Consignee with same TCUs", nctsBill, errorMessage);
					});

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleB1877_1Active));
					nctsBill.Validation.ValidateAll();
					AssertNoRowMessageError("When Transition Period: ON, RuleB1877: OFF and all GoodItems having Consignee with same TCUs", nctsBill, errorMessage);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleB1877_1Active));

					nctsBill.Validation.ValidateAll();
					AssertNoRowMessageError("When Transition Period: OFF, RB1877: Active and all GoodItems having Consignee with same TCUs", nctsBill, errorMessage);
				}
			}
		}

		public void TestValidateHouseConsignment_AllGoodItemsConsigneesHaveSameNameAndAddress()
		{
			var errorMessage = "[B1877-1] At least one Goods item must have a consignee with different name and address";
			var goodItems1 = nctsBill.GoodsItems.AddNew();
			var goodItems2 = nctsBill.GoodsItems.AddNew();

			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader1 = orgHeader1.Addresses.AddNew();
			orgAddressHeader1.CompanyName = "WiseTech Global";
			orgAddressHeader1.OA_Address1 = "Unit 3a";
			orgAddressHeader1.OA_Address2 = "1 Epping Road";
			orgAddressHeader1.OA_RL_NKRelatedPortCode = "AUSYD";
			orgAddressHeader1.OA_City = "Tokyo";
			orgAddressHeader1.OA_State = "Tokyo";

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddressHeader2 = orgHeader2.Addresses.AddNew();

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleB1877_1Active));
					CombineAssertions("When Transition Period: ON, RuleB1877: Active", () =>
					{
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("Consignee is not added", nctsBill, errorMessage);

						goodItems1.Consignee.E2_OA_Address = orgAddressHeader1.PK;
						goodItems2.Consignee.E2_OA_Address = orgAddressHeader2.PK;
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("Consignee without EORI/TCU and with different name & address, added", nctsBill, errorMessage);

						goodItems1.Consignee.E2_OA_Address = orgAddressHeader1.PK;
						goodItems2.Consignee.E2_OA_Address = orgAddressHeader1.PK;
						orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU, "987654321", Core.Constants.CountryCodes.Germany);
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("Consignee with same name, address and TCU added", nctsBill, errorMessage);

						orgHeader1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.UnitedStates);
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("Consignee with same name, address and EORI added", nctsBill, errorMessage);

						orgHeader1.CustomsCodes.RemoveAndDeleteAll();
						nctsBill.Validation.ValidateAll();
						AssertHasRowMessageError("Consignee with same name and address and without EORI/TCU added", nctsBill, errorMessage);
					});

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleB1877_1Active));
					nctsBill.Validation.ValidateAll();
					AssertNoRowMessageError("When Transition Period: ON, RB1877: Disabled and Consignee with same name and address and without EORI/TCU added", nctsBill, errorMessage);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleB1877_1Active));

					nctsBill.Validation.ValidateAll();
					AssertNoRowMessageError("When Transition Period: OFF, RB1877: Active and Consignee with same name and address and without EORI/TCU added", nctsBill, errorMessage);
				}
			}
		}

		public void TestValidateHouseConsignment_AllGoodItemsHaveSameTransportChgMop()
		{
			var errorMessage = "[B1877-1] At least one Goods item must have a Trans. Chg. MoP with different value";

			var goodsItem1 = nctsBill.GoodsItems.AddNew();
			var goodsItem2 = nctsBill.GoodsItems.AddNew();

			using (var ruleTestContext = ValidationRuleConfigurationTestHelper.GetValidationRuleConfigurationTestContext(Factory))
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleB1877_1Active));
					CombineAssertions("When Transition Period: ON, RuleB1877: Active", () =>
					{
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("Trans. Chg. MoP empty for all goodsitems", nctsBill, errorMessage);

						goodsItem1.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("Trans. Chg. MoP empty for atleast one goodsitem", nctsBill, errorMessage);

						goodsItem2.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.Cheque;
						nctsBill.Validation.ValidateAll();
						AssertNoRowMessageError("Trans. Chg. MoP is different for atleast one goodsitem", nctsBill, errorMessage);

						goodsItem2.BY_TransportChargesMethodOfPayment = TransportChargesModeOfPayment.Codes.CreditCard;
						nctsBill.Validation.ValidateAll();
						AssertHasRowMessageError("Trans. Chg. MoP is same for all goodsitems", nctsBill, errorMessage);
					});

					ruleTestContext.DisableRule(nameof(ValidationRuleConfiguration.IsRuleB1877_1Active));
					nctsBill.Validation.ValidateAll();
					AssertNoRowMessageError("When Transition Period: ON, RuleB1877: Disabled, Trans. Chg. MoP is same for all goodsitems", nctsBill, errorMessage);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					ruleTestContext.EnableRule(nameof(ValidationRuleConfiguration.IsRuleB1877_1Active));

					nctsBill.Validation.ValidateAll();
					AssertNoRowMessageError("When Transition Period: OFF, RuleB1877: Active, Trans. Chg. MoP is same for all goodsitems", nctsBill, errorMessage);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsBill = nctsHeader.Bills.AddNew();
		}
		NctsBill nctsBill;
	}
}
