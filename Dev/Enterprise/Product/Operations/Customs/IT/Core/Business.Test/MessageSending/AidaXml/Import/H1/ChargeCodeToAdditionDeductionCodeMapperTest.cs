using System;
using Enterprise.Customs.EU.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class ChargeCodeToAdditionDeductionCodeMapperTest : TestCase
{
	public void TestMapUccChargeCodeToCodeOfTypeAddition()
	{
		var codeMapper = new ChargeCodeToAdditionDeductionCodeMapper();
		var mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.CommissionExceptBuyingCommissionsCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AB,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.CommissionAndBrokerageCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AB,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.ContainersAndPackingCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AD,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.MaterialsComponentsPartsCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AE,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.ToolsMiesMouldsCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AF,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.MaterialsConsumedCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AG,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.EngineeringDevelopmentArtworkCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AH,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.RoyaltiesLicenseFeeCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AI,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.ProceedsOfAnySubsequentResaleCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AJ,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AK,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.TransportCostsCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.TransportCostsCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AK,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AK,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.OtherNotElsewhereDeclaredCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AK,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.IndirectAndOtherPaymentsCharge,
			expectedCode: AdditionDeductionCodeList.Codes.AL,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeAddition(UCCCustomsChargeTypeList.Codes.Additions71Charge);
		AssertAdditionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.Additions71Charge,
			expectedCode: AdditionDeductionCodeList.Codes.AN,
			actualCode: mappedCode);
	}

	public void TestMapUccChargeCodeToCodeOfTypeDeduction()
	{
		var codeMapper = new ChargeCodeToAdditionDeductionCodeMapper();
		var mappedCode = codeMapper.MapChargeCodeToCodeOfTypeDeduction(UCCCustomsChargeTypeList.Codes.TransportCostsCharge);
		AssertDeductionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.TransportCostsCharge,
			expectedCode: AdditionDeductionCodeList.Codes.BA,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeDeduction(UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge);
		AssertDeductionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.AirTransportCostsCharge,
			expectedCode: AdditionDeductionCodeList.Codes.BA,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeDeduction(UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge);
		AssertDeductionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.InsuranceCostsCharge,
			expectedCode: AdditionDeductionCodeList.Codes.BA,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeDeduction(UCCCustomsChargeTypeList.Codes.AdjustmentCharge);
		AssertDeductionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.AdjustmentCharge,
			expectedCode: AdditionDeductionCodeList.Codes.BA,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeDeduction(UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge);
		AssertDeductionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.ConstructionErectionAssemblyCharge,
			expectedCode: AdditionDeductionCodeList.Codes.BB,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeDeduction(UCCCustomsChargeTypeList.Codes.DeductionsNotElsewhereDeclaredCharge);
		AssertDeductionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.DeductionsNotElsewhereDeclaredCharge,
			expectedCode: AdditionDeductionCodeList.Codes.BB,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeDeduction(UCCCustomsChargeTypeList.Codes.DiscountNotElsewhereDeclaredCharge);
		AssertDeductionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.DeductionsNotElsewhereDeclaredCharge,
			expectedCode: AdditionDeductionCodeList.Codes.BB,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeDeduction(UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge);
		AssertDeductionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.ImportDutiesOrOtherCharge,
			expectedCode: AdditionDeductionCodeList.Codes.BC,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeDeduction(UCCCustomsChargeTypeList.Codes.InterestCharge);
		AssertDeductionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.InterestCharge,
			expectedCode: AdditionDeductionCodeList.Codes.BD,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeDeduction(UCCCustomsChargeTypeList.Codes.RightToReproduceCharge);
		AssertDeductionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.RightToReproduceCharge,
			expectedCode: AdditionDeductionCodeList.Codes.BE,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeDeduction(UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge);
		AssertDeductionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.BuyingCommissionsCharge,
			expectedCode: AdditionDeductionCodeList.Codes.BF,
			actualCode: mappedCode);

		mappedCode = codeMapper.MapChargeCodeToCodeOfTypeDeduction(UCCCustomsChargeTypeList.Codes.Deductions71Charge);
		AssertDeductionCodeType(chargeCode: UCCCustomsChargeTypeList.Codes.Deductions71Charge,
			expectedCode: AdditionDeductionCodeList.Codes.BG,
			actualCode: mappedCode);
	}

	public void TestMapUccChargeCodeToCodeOfTypeAddition_InvalidCode()
	{
		AssertExceptionThrown<InvalidOperationException>("Invalid Operation exception for code XYZ",
			"Charge Code XYZ is not configured for mapping to Addition/Deduction Code of Type Addition",
			() => new ChargeCodeToAdditionDeductionCodeMapper().MapChargeCodeToCodeOfTypeAddition("XYZ"));
	}

	public void TestMapUccChargeCodeToCodeOfTypeDeduction_InvalidCode()
	{
		AssertExceptionThrown<InvalidOperationException>("Invalid Operation exception for code PQR",
			"Charge Code PQR is not configured for mapping to Addition/Deduction Code of Type Deduction",
			() => new ChargeCodeToAdditionDeductionCodeMapper().MapChargeCodeToCodeOfTypeDeduction("PQR"));
	}

	void AssertAdditionCodeType(string chargeCode, string expectedCode, string actualCode)
	{
		AssertEquals($"Mapped Code of Type Addition for {chargeCode}", expectedCode, actualCode);
	}

	void AssertDeductionCodeType(string chargeCode, string expectedCode, string actualCode)
	{
		AssertEquals($"Mapped Code of Type Deduction for {chargeCode}", expectedCode, actualCode);
	}
}
