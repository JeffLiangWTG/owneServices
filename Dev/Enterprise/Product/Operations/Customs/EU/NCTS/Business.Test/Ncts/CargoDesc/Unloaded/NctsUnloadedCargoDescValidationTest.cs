using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal;
using Moq.Protected;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsUnloadedCargoDescValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBY_HarmonisedTariff()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var lvTariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.Latvia, Constants.TariffTypes.Export).PK;
			helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Latvia, lvTariffTypePK, "01000001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			CombineAssertions(() =>
			{
				var unloadedCargoDesc = Factory.New<NctsUnloadedCargoDesc>();
				unloadedCargoDesc.BY_UnloadedState = "X";
				unloadedCargoDesc.BY_HarmonisedTariff = "";
				AssertHasMessageError(unloadedCargoDesc.BY_HarmonisedTariffInfo, "You have not entered a Commodity Code.");

				unloadedCargoDesc.BY_HarmonisedTariff = "99999999";
				AssertHasMessageError(unloadedCargoDesc.BY_HarmonisedTariffInfo, "The code you have selected is not in the list.");

				unloadedCargoDesc.BY_HarmonisedTariff = "01000001";
				AssertNoMessageErrors(unloadedCargoDesc.BY_HarmonisedTariffInfo);
			});
		}

		public void TestCheckBY_HarmonisedTariff_IsUnloadedCommodityCodeRequired()
		{
			CombineAssertions(() =>
			{
				var unloadedCargoDescMock = Factory.NewMoq<NctsArrivalCargoDesc>();
				unloadedCargoDescMock.CallBase = true;
				unloadedCargoDescMock.Protected().Setup<bool>("IsUnloadedCommodityCodeRequiredCore").Returns(false);
				unloadedCargoDescMock.Object.BY_UnloadedState = "DIF";
				var unloadedCargoDesc = unloadedCargoDescMock.Object.UnloadedGoodsItem;
				unloadedCargoDesc.BY_UnloadedState = "X";
				unloadedCargoDesc.BY_HarmonisedTariff = "";
				AssertNoMessageError("IsUnloadedCommodityCodeRequired is false", unloadedCargoDesc.BY_HarmonisedTariffInfo, "You have not entered a Commodity Code.");

				unloadedCargoDescMock.Protected().Setup<bool>("IsUnloadedCommodityCodeRequiredCore").Returns(true);
				unloadedCargoDesc.BY_HarmonisedTariff = "";

				AssertHasMessageError("IsUnloadedCommodityCodeRequired is true", unloadedCargoDesc.BY_HarmonisedTariffInfo, "You have not entered a Commodity Code.");
			});
		}

		public void TestCheckBY_HarmonisedTariff_NR0004()
		{
			var message = "[NR0004] Commodity Code must be 6 or 8 digits of the full commodity code.(A060)";
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
			using (var ruleTestContext = new CargoDescValidationDeciderTestContext<INctsArrivalCargoDescPhase5ValidationDecider>(Factory))
			{
				ruleTestContext.EnableRule(rule => rule.IsRuleNR0004Active);
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				var harmonisedTariffInfo = arrivalCargoDesc.UnloadedGoodsItem.BY_HarmonisedTariffInfo;
				foreach (var testCase in new string[] { "999" }.SelectMany(GetRuleCase).ToArray())
				{
					testCase.Invoke();
					AssertHasMessageError("Commodity Code is too short", harmonisedTariffInfo, message);
				}
			}

			using (var ruleTestContext = new CargoDescValidationDeciderTestContext<INctsArrivalCargoDescPhase5ValidationDecider>(Factory))
			{
				ruleTestContext.DisableRule(rule => rule.IsRuleNR0004Active);
				header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				bill = header.Bills.AddNew();
				arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				var harmonisedTariffInfo = arrivalCargoDesc.UnloadedGoodsItem.BY_HarmonisedTariffInfo;

				foreach (var testCase in new string[] { "999", "010001", "01000001" }.SelectMany(GetRuleCase).ToArray())
				{
					testCase.Invoke();
					AssertNoMessageError("Commodity Code is valid", harmonisedTariffInfo, message);
				}
			}

			IEnumerable<Action> GetRuleCase(string commodityCode)
			{
				yield return () =>
				{
					arrivalCargoDesc.UnloadedGoodsItem.BY_HarmonisedTariff = commodityCode;
				};
			}
		}

		public void TestCheckBY_HarmonisedTariff_NR0055()
		{
			var message = "[NR0055] Commodity Code must be 8 or 10 digits.";

			CombineAssertions(() =>
			{
				using (var ruleTestContext = new CargoDescValidationDeciderTestContext<INctsArrivalCargoDescPhase5ValidationDecider>(Factory))
				{
					ruleTestContext.DisableRule(v => v.IsRuleNR0055Active);
					var unloadedItem = GetNctsUnloadedGoodItem();
					AssertNoMessageErrorContaining("Rule Disable: Commodity Code has no error if rule is not active", unloadedItem.BY_HarmonisedTariffInfo, message);

					ruleTestContext.EnableRule(v => v.IsRuleNR0055Active);
					unloadedItem = GetNctsUnloadedGoodItem();
					unloadedItem.Validation.ValidateBY_HarmonisedTariff();
					AssertHasMessageErrorContaining("Rule Enable: Commodity Code has error cause is not 8 or 10 chr", unloadedItem.BY_HarmonisedTariffInfo, message);

					unloadedItem.BY_HarmonisedTariff = ZString.Empty;
					AssertNoMessageErrorContaining("Rule Enable: Commodity Code empty has no error cause is empty", unloadedItem.BY_HarmonisedTariffInfo, message);

					unloadedItem.BY_HarmonisedTariff = "1";
					AssertHasMessageErrorContaining("Rule Enable: Commodity Code has error cause is not 8 or 10 chr", unloadedItem.BY_HarmonisedTariffInfo, message);

					unloadedItem.BY_HarmonisedTariff = "12345678";
					AssertNoMessageErrorContaining("Rule Enable: Commodity Code has no error cause is 8", unloadedItem.BY_HarmonisedTariffInfo, message);

					unloadedItem.BY_HarmonisedTariff = "123456";
					AssertHasMessageErrorContaining("Rule Enable: Commodity Code has error cause is not 8 or 10 chr", unloadedItem.BY_HarmonisedTariffInfo, message);

					unloadedItem.BY_HarmonisedTariff = "1234567890";
					AssertNoMessageErrorContaining("Rule Enable: Commodity Code has no error cause is 10", unloadedItem.BY_HarmonisedTariffInfo, message);
				}
			});

			NctsUnloadedCargoDesc GetNctsUnloadedGoodItem()
			{
				var header = Factory.New<NctsHeader>();
				header.SetMovementType(NctsMovementType.Codes.Arrival);
				header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				var bill = header.Bills.AddNew();
				var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				var unloadedItem = arrivalCargoDesc.UnloadedGoodsItem;
				unloadedItem.BY_HarmonisedTariff = "123456789";

				return unloadedItem;
			}
		}

		public void TestCheckBY_CusC4Number()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, "Latvia");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_ECICS, "CUS Codes");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_ECICS, "01000001", "CUSCode 1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "CNCODE", "11000001");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Code_ECICS, "01000002", "CUSCode 2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "CNCODE", "21000002");
			Factory.Save();

			CombineAssertions(() =>
			{
				var unloadedCargoDesc = Factory.New<NctsUnloadedCargoDesc>();
				unloadedCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.NEW;
				unloadedCargoDesc.BY_CusC4Number = "99999999";
				AssertHasMessageError(unloadedCargoDesc.BY_CusC4NumberInfo, "The code you have selected is not in the list.");

				unloadedCargoDesc.BY_HarmonisedTariff = "11000001";
				unloadedCargoDesc.BY_CusC4Number = "01000001";
				AssertNoMessageErrors(unloadedCargoDesc.BY_CusC4NumberInfo);

				unloadedCargoDesc.BY_CusC4Number = "01000002";
				AssertHasMessageError(unloadedCargoDesc.BY_CusC4NumberInfo, "CUS Code does not belong to the commodity code");
			});
		}

		public void TestCheckBY_Description()
		{
			CombineAssertions(() =>
			{
				var unloadedCargoDesc = Factory.New<NctsUnloadedCargoDesc>();
				unloadedCargoDesc.BY_Description = "";
				AssertHasMessageError(unloadedCargoDesc.BY_DescriptionInfo, "You have not entered a Goods Description.");

				unloadedCargoDesc.BY_Description = "TEST";
				AssertNoMessageErrors(unloadedCargoDesc.BY_DescriptionInfo);
			});
		}

		public void TestCheckBY_GrossWeightUnit()
		{
			CombineAssertions(() =>
			{
				var unloadedCargoDesc = Factory.New<NctsUnloadedCargoDesc>();
				unloadedCargoDesc.BY_GrossWeightUnit = "";
				AssertHasMessageError(unloadedCargoDesc.BY_GrossWeightUnitInfo, "You have not entered a Gross Weight Units.");

				unloadedCargoDesc.BY_GrossWeightUnit = "KG";
				AssertNoMessageErrors(unloadedCargoDesc.BY_GrossWeightUnitInfo);
			});
		}

		public void TestCheckBY_NetWeightUnit()
		{
			CombineAssertions(() =>
			{
				var arrivalCargoDesc = Factory.NewWithValidTestData<NctsArrivalCargoDesc>();
				arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;

				var unloadedCargoDesc = arrivalCargoDesc.UnloadedGoodsItem;
				unloadedCargoDesc.BY_NetWeightUnit = "";
				AssertHasMessageError(unloadedCargoDesc.BY_NetWeightUnitInfo, "You have not entered a Net Weight Units.");

				unloadedCargoDesc.BY_NetWeightUnit = "KG";
				AssertNoMessageErrors(unloadedCargoDesc.BY_NetWeightUnitInfo);
			});
		}

		public void TestCheckBY_HarmonisedTariff_6DigitsInWcoGroup()
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, parent: grouping);

			var wcoTariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, Constants.TariffTypes.HarmonizedSystem).PK;
			var tariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Constants.TariffTypes.Export).PK;
			helper.LoadOrCreateNewTariff(Core.Constants.Customs.Universal.RefDataGrouping.Codes.WorldCustomsOrganisationWCO, wcoTariffTypePK, "013456", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.LoadOrCreateNewTariff(MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode, tariffTypePK, "013456", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = header.Bills.AddNew();
			var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
			
			arrivalCargoDesc.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;

			ValidationTestHelper.AssertInvalidCodeMessageError(arrivalCargoDesc.UnloadedGoodsItem.BY_HarmonisedTariffInfo, invalidCode: "123456", validCode: "013456");
		}
	}
}
