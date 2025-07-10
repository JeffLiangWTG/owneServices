using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.Universal;
using FunctionalityTypes = Enterprise.Customs.Universal.Constants.FunctionalityTypes;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IE.NCTS.Business.Testing;

sealed class NctsDepartureCargoDescPhase5ValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckConditionC901_TIRMovement()
	{
		var goodsItem = GetFirstTIRGoodsItem();
		goodsItem.Validation.ValidateAll();
		AssertNoRowMessageErrorContaining(goodsItem, "[C901]");
	}

	NctsDepartureCargoDesc GetFirstTIRGoodsItem()
	{
		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var movementHeader = header.MovementHeader;
		movementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
		return header.Bills.AddNew().GoodsItems.AddNew();
	}

	public void TestCheckBY_GrossWeight()
	{
		using var nctsTransitionPeriod = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
			code: FunctionalityTypes.NCTSTransitionPeriod,
			dataGroupingCode: RefDataGroupingCodes.EuropeanUnionEUN,
			effectiveDate: ZDate.Today,
			value: false
		);

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.Validation.ValidateBY_GrossWeight();
		AssertNoMessageErrorContaining("Package Quantity zero, no B2101 message.", goodsItem.BY_GrossWeightInfo, "[B2101]");

		goodsItem.Packages.AddNew().B5_UnitCount = 2;
		goodsItem.Validation.ValidateBY_GrossWeight();
		AssertHasMessageErrorContaining("Package Quantity present, show B2101 message.", goodsItem.BY_GrossWeightInfo, "[B2101]");

		goodsItem.BY_GrossWeight = 200;
		AssertNoMessageErrorContaining("Package Quantity present, BY_GrossWeight present, no B2101 message.", goodsItem.BY_GrossWeightInfo, "[B2101]");

		using var nctsTransitionPeriodYes = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(
			code: FunctionalityTypes.NCTSTransitionPeriod,
			dataGroupingCode: RefDataGroupingCodes.EuropeanUnionEUN,
			effectiveDate: ZDate.Today,
			value: true
		);
		goodsItem.BY_GrossWeight = 0;
		AssertNoMessageErrorContaining("Transition Period, B2101 inactivate.", goodsItem.BY_GrossWeightInfo, "[B2101]");
	}

	public void TestCheckBY_GrossWeightUnit()
	{
		using var nctsTransitionPeriod = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, value: false);

		var header = Factory.New<NctsHeader>();
		header.SetMovementType(NctsMovementType.Codes.Departure);
		var goodsItem = header.Bills.AddNew().GoodsItems.AddNew();
		goodsItem.BY_GrossWeightUnit = string.Empty;
		goodsItem.Validation.ValidateBY_GrossWeightUnit();
		AssertNoMessageErrorContaining("Package Quantity zero, no B2101 message.", goodsItem.BY_GrossWeightUnitInfo, "[B2101]");

		goodsItem.Packages.AddNew().B5_UnitCount = 2;
		goodsItem.Validation.ValidateBY_GrossWeightUnit();
		AssertHasMessageErrorContaining("Package Quantity present, show B2101 message.", goodsItem.BY_GrossWeightUnitInfo, "[B2101]");

		goodsItem.BY_GrossWeight = 200;
		goodsItem.BY_GrossWeightUnit = "KG";
		AssertNoMessageErrorContaining("Package Quantity present, BY_GrossWeight present, no B2101 message.", goodsItem.BY_GrossWeightUnitInfo, "[B2101]");
	}
}
