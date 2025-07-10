using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import;

sealed class ChargeCodeToAdditionDeductionCodeMapper
{
	public string MapChargeCodeToCodeOfTypeAddition(string chargeCode)
	{
		Argument.NotNullOrEmpty(chargeCode, nameof(chargeCode));
		return MapChargeCodeInsideDictionary(chargeCode, chargeCodeToAdditionTypeCodeMap, (NoResString)"Addition");
	}

	public string MapChargeCodeToCodeOfTypeDeduction(string chargeCode)
	{
		Argument.NotNullOrEmpty(chargeCode, nameof(chargeCode));
		return MapChargeCodeInsideDictionary(chargeCode, chargeCodeToDeductionTypeCodeMap, (NoResString)"Deduction");
	}

	string MapChargeCodeInsideDictionary(string chargeCode, IDictionary<string, string> map, string mapType)
	{
		if (!map.ContainsKey(chargeCode))
		{
			var notSupportedCodeMessage = ResString.GetMultilingualString("69E08E5B-4E22-4DCF-8DFA-212510C85D89", "Charge Code {0} is not configured for mapping to Addition/Deduction Code of Type {1}", chargeCode, mapType);
			throw new InvalidOperationException(notSupportedCodeMessage);
		}

		return map[chargeCode];
	}

	readonly ImmutableDictionary<string, string> chargeCodeToAdditionTypeCodeMap = new Dictionary<string, string>()
	{
		{ UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge,  AdditionDeductionCodeList.Codes.AB },
		{ UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge,  AdditionDeductionCodeList.Codes.AB },
		{ UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge,  AdditionDeductionCodeList.Codes.AD },
		{ UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge,  AdditionDeductionCodeList.Codes.AE },
		{ UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge,  AdditionDeductionCodeList.Codes.AF },
		{ UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge,  AdditionDeductionCodeList.Codes.AG },
		{ UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge,  AdditionDeductionCodeList.Codes.AH },
		{ UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge,  AdditionDeductionCodeList.Codes.AI },
		{ UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge,  AdditionDeductionCodeList.Codes.AJ },
		{ UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge,  AdditionDeductionCodeList.Codes.AK },
		{ UCCCustomsChargeTypeList.Codes.TransportCostsCharge,  AdditionDeductionCodeList.Codes.AK },
		{ UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge,  AdditionDeductionCodeList.Codes.AK },
		{ UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge,  AdditionDeductionCodeList.Codes.AK },
		{ UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge,  AdditionDeductionCodeList.Codes.AL },
		{ UCCCustomsChargeTypeList.Codes.Additions71Charge,  AdditionDeductionCodeList.Codes.AN },
	}.ToImmutableDictionary();

	readonly ImmutableDictionary<string, string> chargeCodeToDeductionTypeCodeMap = new Dictionary<string, string>
	{
		{ UCCCustomsChargeTypeList.Codes.TransportCostsCharge, AdditionDeductionCodeList.Codes.BA },
		{ UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge, AdditionDeductionCodeList.Codes.BA },
		{ UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge, AdditionDeductionCodeList.Codes.BA },
		{ UCCCustomsChargeTypeList.Codes.AdjustmentCharge, AdditionDeductionCodeList.Codes.BA },
		{ UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge, AdditionDeductionCodeList.Codes.BB },
		{ UCCCustomsChargeTypeList.Codes.DeductionsNotElsewhereDeclaredCharge, AdditionDeductionCodeList.Codes.BB },
		{ UCCCustomsChargeTypeList.Codes.DiscountNotElsewhereDeclaredCharge, AdditionDeductionCodeList.Codes.BB },
		{ UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge, AdditionDeductionCodeList.Codes.BC },
		{ UCCCustomsChargeTypeList.Codes.InterestCharge, AdditionDeductionCodeList.Codes.BD },
		{ UCCCustomsChargeTypeList.Codes.RightToReproduceCharge, AdditionDeductionCodeList.Codes.BE },
		{ UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge, AdditionDeductionCodeList.Codes.BF },
		{ UCCCustomsChargeTypeList.Codes.Deductions71Charge, AdditionDeductionCodeList.Codes.BG },
	}.ToImmutableDictionary();
}
