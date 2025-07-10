using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLineFee))]
sealed class CusEntryLineFeeTest : Customs.Business.Testing.CusEntryLineFeeTest
{
	public void TestLookups()
	{
		AssertType(typeof(CusEntryLineFeeLookups), fee.Lookups);
	}

	public void TestNationalFeeTypeDescriptionForDisplay()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");
		var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusRateTypes.Duty);
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.SecuritiesForEndUse, rateType.PK, description: "Securities For End Use");
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.DeferredVAT, rateType.PK, description: "Deferred VAT");
		helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.IERefCusRateCodes.SpecialArrangementsVAT, rateType.PK, description: "Special Arrangements VAT");
		Factory.Save();
		CombineAssertions("NationalFeeTypeDescriptionForDisplay", () =>
		{
			AssertEquals("Empty for default.", ZString.Empty, fee.NationalFeeTypeDescriptionForDisplay);
			fee.NationalFeeTypeCode = "XXX";
			AssertEquals("Runnable for unrecognized CF_ChargeType.", ZString.Empty, fee.NationalFeeTypeDescriptionForDisplay);

			fee.NationalFeeTypeCode = UniversalReferenceConstants.IERefCusRateCodes.SecuritiesForEndUse;
			AssertEquals("Get description by CF_ChargeType", "Securities For End Use", fee.NationalFeeTypeDescriptionForDisplay);
		});
	}

	public void TestNationalFeeTypeDescriptionForDisplay_Caption()
	{
		var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(fee.NationalFeeTypeDescriptionForDisplayInfo);
		AssertEquals("Caption", "Description", resourceStringDataAttribute.Caption);
		AssertEquals("FullDescription", "Tax Type Description", resourceStringDataAttribute.FullDescription);
	}

	public void TestNationalFeeTypeCode()
	{
		var instruction = fee.EntryLine.RandomLine.EntryInstruction;
		instruction.CEI_Style = ImportDeclarationTypeList.Codes.H1;
		fee.EntryLine.RandomLine.JI_Procedure = "44";
		instruction.AdditionalInfos.AddNew("00100", "").CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;

		fee.CF_ChargeType = UniversalReferenceConstants.IERefCusRateCodes.DeferredVAT;
		CombineAssertions("NationalFeeTypeCode", () =>
		{
			AssertEquals("Get CF_ChargeType", UniversalReferenceConstants.IERefCusRateCodes.DeferredVAT, fee.NationalFeeTypeCode);

			fee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			fee.NationalFeeTypeCode = UniversalReferenceConstants.IERefCusRateCodes.SecuritiesForEndUse;
			AssertEquals(
				"NationalFeeTypeCode and CF_ChargeType not keep synchronized when: IsSecuritiesForEndUse,A00/B00 and 1D6",
				EU.Business.UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts,
				fee.CF_ChargeType
			);

			fee.NationalFeeTypeCode = UniversalReferenceConstants.IERefCusRateCodes.SpecialArrangementsVAT;
			AssertEquals("Should set CF_ChargeType when not 1D6", UniversalReferenceConstants.IERefCusRateCodes.SpecialArrangementsVAT, fee.CF_ChargeType);
		});
	}

	public void TestNationalFeeTypeCode_Caption()
	{
		var resourceStringDataAttribute = DataBoundResourceStrings.GetDataForProperty(fee.NationalFeeTypeCodeInfo);
		AssertEquals("Caption", "Tax Type", resourceStringDataAttribute.Caption);
	}

	public void TestNationalFeeTypeCode_Readonly()
	{
		fee.CF_RateOverrideReasonCode = "A";
		AssertEquals("NationalFeeTypeCode writable when Action not empty.", false, fee.NationalFeeTypeCodeInfo.ReadOnly);
		fee.CF_RateOverrideReasonCode = string.Empty;
		AssertEquals("NationalFeeTypeCode readonly when Action has value.", true, fee.NationalFeeTypeCodeInfo.ReadOnly);
	}

	protected override void SetUp()
	{
		fee = NewTestObjects(Factory).fee;
	}
	CusEntryLineFee fee;

	public static (JobDeclaration declaration, CusEntryLineFee fee) NewTestObjects(BusinessObjectFactory factory, string messageType = "IMP")
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;
		declaration.JE_PaymentMethod = "A";
		var instruction = declaration.CustomsEntryInstructions.FirstOrAddNew();
		var invoiceHeader = declaration.Invoices.AddNew();
		var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		invoiceLine.JI_CEI = instruction.PK;
		var fee = entryLine.Fees.AddNew();
		return (declaration, (CusEntryLineFee)fee);
	}
}
