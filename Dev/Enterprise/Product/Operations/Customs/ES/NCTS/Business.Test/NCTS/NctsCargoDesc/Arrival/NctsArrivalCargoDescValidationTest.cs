using System;
using System.Data;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.Testing;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

public class NctsArrivalCargoDescValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckLiabilityListValidation()
	{
		var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
		var lvTariffTypePK = Universal.Testing.UniversalReferenceTestDataHelper.CreateNewOrGetExistingRefCusTariffType(Factory, Core.Constants.CountryCodes.Spain, Universal.Constants.TariffTypes.Import).PK;
		_ = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Spain, lvTariffTypePK, "1234567890", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
		Factory.Save();

		using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

			var bill = header.Bills.AddNew();
			var arrivalCargoDesc = bill.ArrivalGoodsItems.AddNew();
			arrivalCargoDesc.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, arrivalCargoDesc.Header);
			arrivalCargoDesc.LiabilityTariff = "0987654321";
			var warning = "You have not entered a valid code.";
			CombineAssertions(() =>
			{
				AssertNoMessageErrors("MessageError Tariff is not valid", arrivalCargoDesc.LiabilityTariffInfo);
				AssertHasWarningContaining("Warning Tariff is not valid", arrivalCargoDesc.LiabilityTariffInfo, warning);

				arrivalCargoDesc.LiabilityTariff = "1234567890";
				AssertNoMessageErrors("MessageError Tariff is valid", arrivalCargoDesc.LiabilityTariffInfo);
				AssertNoWarningContaining("Warning Tariff is valid", arrivalCargoDesc.LiabilityTariffInfo, warning);
			});
		}
	}

	public void TestCheckBY_CustomsSecondQuantity_RuleTR0084() => CombineAssertions(() =>
	{
		var messageError = "[TR0084] A unit for the Supplementary Quantity has been defined, perhaps from the tariff chosen, yet no quantity has been given. Supply a non-zero quantity";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		var goodsItemArrival = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
		goodsItemArrival.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
		AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, goodsItemArrival.Header);
		using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(Factory, isLiabilityCalculationForArrivalSupported: true, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
		{
			AssertRuleActive(goodsItemArrival,
				(unit) => goodsItemArrival.BY_CustomsSecondUnitQty = unit,
				(qty) => goodsItemArrival.BY_CustomsSecondQuantity = qty,
				() => goodsItemArrival.Validation.ValidateBY_CustomsSecondQuantity(),
				goodsItemArrival.BY_CustomsSecondQuantityInfo,
				messageError);
		}

		using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(Factory, isLiabilityCalculationForArrivalSupported: true, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
		{
			AssertRuleInactive(goodsItemArrival,
				(unit) => goodsItemArrival.BY_CustomsSecondUnitQty = unit,
				(qty) => goodsItemArrival.BY_CustomsSecondQuantity = qty,
				() => goodsItemArrival.Validation.ValidateBY_CustomsSecondQuantity(),
				goodsItemArrival.BY_CustomsSecondQuantityInfo,
				messageError);
		}
	});

	public void TestCheckBY_CustomsThirdQuantity_RuleTR0084() => CombineAssertions(() =>
	{
		var messageError = "[TR0084] A unit for the [31] Third Qty has been defined, perhaps from the tariff chosen, yet no quantity has been given. Supply a non-zero quantity";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		var goodsItemArrival = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
		goodsItemArrival.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
		AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, goodsItemArrival.Header);
		using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(Factory, isLiabilityCalculationForArrivalSupported: true, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
		{
			AssertRuleActive(goodsItemArrival,
				(unit) => goodsItemArrival.BY_CustomsThirdUnitQty = unit,
				(qty) => goodsItemArrival.BY_CustomsThirdQuantity = qty,
				() => goodsItemArrival.Validation.ValidateBY_CustomsThirdQuantity(),
				goodsItemArrival.BY_CustomsThirdQuantityInfo,
				messageError);
		}

		using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(Factory, isLiabilityCalculationForArrivalSupported: true, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
		{
			AssertRuleInactive(goodsItemArrival,
				(unit) => goodsItemArrival.BY_CustomsThirdUnitQty = unit,
				(qty) => goodsItemArrival.BY_CustomsThirdQuantity = qty,
				() => goodsItemArrival.Validation.ValidateBY_CustomsThirdQuantity(),
				goodsItemArrival.BY_CustomsThirdQuantityInfo,
				messageError);
		}
	});

	public void TestCheckBY_CustomsFourthQuantity_RuleTR0084() => CombineAssertions(() =>
	{
		var messageError = "[TR0084] A unit for the Fourth Quantity has been defined, perhaps from the tariff chosen, yet no quantity has been given. Supply a non-zero quantity";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		var goodsItemArrival = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
		goodsItemArrival.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
		AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, goodsItemArrival.Header);
		using (ValidationRuleConfigurationTestHelper.TemporarilyActivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(Factory, isLiabilityCalculationForArrivalSupported: true, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
		{
			AssertRuleActive(goodsItemArrival,
				(unit) => goodsItemArrival.BY_CustomsFourthUnitQty = unit,
				(qty) => goodsItemArrival.BY_CustomsFourthQuantity = qty,
				() => goodsItemArrival.Validation.ValidateBY_CustomsFourthQuantity(),
				goodsItemArrival.BY_CustomsFourthQuantityInfo,
				messageError);
		}

		using (ValidationRuleConfigurationTestHelper.TemporarilyInactivateValidationConfigurationRuleForArrivalWithLiabilityCalculation(Factory, isLiabilityCalculationForArrivalSupported: true, nameof(ValidationRuleConfiguration.IsRuleTR0084Active)))
		{
			AssertRuleInactive(goodsItemArrival,
				(unit) => goodsItemArrival.BY_CustomsFourthUnitQty = unit,
				(qty) => goodsItemArrival.BY_CustomsFourthQuantity = qty,
				() => goodsItemArrival.Validation.ValidateBY_CustomsFourthQuantity(),
				goodsItemArrival.BY_CustomsFourthQuantityInfo,
				messageError);
		}
	});

	public void TestCheckLiabilityTariffFirstEightNumbers()
	{
		using (NctsConfigurationTestHelper.TemporarilySetGoodsItemConfigurationIsLiabilityCalculationForArrivalSupported(Factory, isLiabilityCalculationForArrivalSupported: true))
		{
			var arrivalCargoDesc = SetUpSimpleNctsArrivalCargoDesc(true);
			arrivalCargoDesc.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.NEW;
			arrivalCargoDesc.BY_HarmonisedTariff = "1234567890";
			const string message = "Commodity Code entered for liability calculation does not match the value used in the Transit declaration. Please note this value will be used to enter goods into Temporary Storage";
			CombineAssertions(() =>
			{
				AssertNoWarning("10 digits, but the first 8 of BY_HarmonisedTariff characters are same to the first 8 characters of LiabilityFormattedTariff", arrivalCargoDesc.LiabilityTariffInfo, message);

				arrivalCargoDesc.LiabilityTariff = "1111111190";
				AssertHasWarning("10 digits, but the first 8 characters of BY_HarmonisedTariff are different to the first 8 characters of LiabilityFormattedTariff", arrivalCargoDesc.LiabilityTariffInfo, message);

				arrivalCargoDesc.BY_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
				arrivalCargoDesc.UnloadedGoodsItem.BY_HarmonisedTariff = "1111111190";

				AssertNoWarning("10 digits, but the first 8 characters of Unloaded Value-BY_HarmonisedTariff characters are same to the first 8 characters of LiabilityFormattedTariff", arrivalCargoDesc.LiabilityTariffInfo, message);

				arrivalCargoDesc.LiabilityTariff = "1234567890";
				AssertHasWarningContaining("10 digits, but the first 8 characters of Unloaded Value-BY_HarmonisedTariff are different to the first 8 characters of LiabilityFormattedTariff", arrivalCargoDesc.LiabilityTariffInfo, message);
			});
		}
	}

	NctsArrivalCargoDesc SetUpSimpleNctsArrivalCargoDesc(bool needTestForLiabilityAmount = false)
	{
		var header = Factory.New<NctsHeader>();

		header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;

		if (needTestForLiabilityAmount)
		{
			NCTSTestHelper.AddNctsArrivalMovementHeaderForLiabilityTestToHeader(Factory, header);
		}

		var bill = header.Bills.AddNew();
		return bill.ArrivalGoodsItems.AddNew();
	}

	void AssertRuleActive(EU.NCTS.Business.NctsCommonCargoDesc goodsItem, Action<string> unitQty, Action<decimal> quantity, Action validation, ZPropertyInfo info, string messageError)
	{
		goodsItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
		unitQty.Invoke(ZString.Empty);
		validation.Invoke();
		AssertNoWarningContaining("Phase 4 and empty UnitQty", info, messageError);

		unitQty.Invoke("KG");
		validation.Invoke();
		AssertNoWarningContaining("Phase 4 and no empty UnitQty", info, messageError);

		goodsItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		validation.Invoke();
		AssertHasWarningContaining("Arrival Phase 5 and no empty UnitQty", info, messageError);
		AssertNoMessageErrorContaining("No Message Error Arrival Phase 5 and no empty UnitQty", info, messageError);

		quantity.Invoke(2);
		AssertNoWarningContaining("Phase 5 and no empty UnitQty no empty Qty", info, messageError);

		unitQty.Invoke(ZString.Empty);
		quantity.Invoke(0);
		AssertNoWarningContaining("Phase 5 and empty UnitQty", info, messageError);
	}

	void AssertRuleInactive(EU.NCTS.Business.NctsCommonCargoDesc goodsItem, Action<string> unitQty, Action<decimal> quantity, Action validation, ZPropertyInfo info, string messageError)
	{
		goodsItem.Header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		unitQty.Invoke("KG");
		quantity.Invoke(0);
		validation.Invoke();
		AssertNoWarningContaining("InactivateValidation - Phase 5 and no empty UnitQty", info, messageError);
	}

	public class NctsArrivalMovementHeaderForLiabilityTest : NctsArrivalMovementHeader
	{
		public NctsArrivalMovementHeaderForLiabilityTest(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		protected override ZBool ShouldGuaranteeForArrivalBeVisibleCore => true;
	}

	public static void AddNctsArrivalMovementHeaderForLiabilityTestToHeader(BusinessObjectFactory factory, NctsHeader header)
	{
		var arrivalMovementHeader = factory.New<NctsArrivalMovementHeaderForLiabilityTest>();

		_ = header.MovementHeaders.RemoveAll(match => true);
		header.MovementHeaders.Add(arrivalMovementHeader);
	}
}
