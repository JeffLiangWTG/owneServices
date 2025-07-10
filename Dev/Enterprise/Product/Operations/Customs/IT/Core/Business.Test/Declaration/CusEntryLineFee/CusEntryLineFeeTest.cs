using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(CusEntryLineFee))]
sealed class CusEntryLineFeeTest : EU.Business.Declaration.Testing.CusEntryLineFeeTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
{
	protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		var fee = entryLine.Fees.AddNew();

		return fee;
	}

	protected override int ExpectedCF_BaseValueDecimalPlaces => 6;

	public void TestIncludeForVatCalculation()
	{
		SetupRates();
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";

		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		var entryLineFee = entryLine.Fees.AddNew();

		entryLineFee.CF_ChargeType = "131";
		AssertEquals($"ChargeType: 131 [Excise], {nameof(entryLineFee.IncludeForVatCalculation)}", true, entryLineFee.IncludeForVatCalculation);

		entryLineFee.CF_ChargeType = "SEC";
		AssertEquals($"ChargeType: SEC [Security Deposit], {nameof(entryLineFee.IncludeForVatCalculation)}", true, entryLineFee.IncludeForVatCalculation);

		entryLineFee.CF_ChargeType = "217";
		AssertEquals($"ChargeType: 217 [Miscellaneous Not VATable], {nameof(entryLineFee.IncludeForVatCalculation)}", false, entryLineFee.IncludeForVatCalculation);

		entryLineFee.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR;
		AssertEquals("Fees having Method of Payment R should not be included in VAT calculation", false, entryLineFee.IncludeForVatCalculation);

		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
		{
			AssertEquals("UCC6 Export - Fees having Method of Payment R should not be included in VAT calculation", false, entryLineFee.IncludeForVatCalculation);
		}

		declaration.JE_MessageType = "IMP";
		entryLineFee.CF_ChargeType = "SEC";
		entryLineFee.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR;
		AssertEquals("When declaration is IMP; Fees having Method of Payment R must be included in VAT calculation", true, entryLineFee.IncludeForVatCalculation);
	}

	public void TestIsNationalIndirectTaxationFee()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		var entryLineFee = entryLine.Fees.AddNew();

		AssertEquals(nameof(entryLineFee.IsNationalIndirectTaxationFee), false, entryLineFee.IsNationalIndirectTaxationFee);
	}

	public void TestIsTemporaryAntiDumping()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();

		entryLineFee.CF_ChargeType = "A00";
		AssertEquals("When charge type is: A00, IsTemporaryAntiDumping", false, entryLineFee.IsTemporaryAntiDumping);

		entryLineFee.CF_ChargeType = "A35";
		AssertEquals("When charge type is: A35, IsTemporaryAntiDumping", true, entryLineFee.IsTemporaryAntiDumping);

		entryLineFee.CF_ChargeType = "";
		AssertEquals("When charge type is Empty, IsTemporaryAntiDumping", false, entryLineFee.IsTemporaryAntiDumping);
	}

	public void TestIsTemporaryCountervailing()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();

		entryLineFee.CF_ChargeType = "A00";
		AssertEquals("When charge type is: A00, IsTemporaryCountervailing", false, entryLineFee.IsTemporaryCountervailing);

		entryLineFee.CF_ChargeType = "A45";
		AssertEquals("When charge type is: A45, IsTemporaryCountervailing", true, entryLineFee.IsTemporaryCountervailing);

		entryLineFee.CF_ChargeType = "";
		AssertEquals("When charge type is Empty, IsTemporaryCountervailing", false, entryLineFee.IsTemporaryCountervailing);
	}

	public void TestIsDuty()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();

		entryLineFee.CF_ChargeType = "";
		Assert("When charge type is Empty, IsDuty", !entryLineFee.IsDuty);

		entryLineFee.CF_ChargeType = "A00";
		Assert("When charge type is: A00, IsDuty", entryLineFee.IsDuty);

		entryLineFee.CF_ChargeType = "172";
		Assert("When charge type is: 172, IsDuty", !entryLineFee.IsDuty);

		entryLineFee.CF_ChargeType = "270";
		Assert("When charge type is: 270, IsDuty", !entryLineFee.IsDuty);
	}

	public void TestIsSanMarinoDuty()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();

		entryLineFee.CF_ChargeType = "";
		Assert("When charge type is Empty, IsDuty", !entryLineFee.IsSanMarinoDuty);

		entryLineFee.CF_ChargeType = "A00";
		Assert("When charge type is: A00, IsDuty", !entryLineFee.IsSanMarinoDuty);

		entryLineFee.CF_ChargeType = "172";
		Assert("When charge type is: 172, IsDuty", !entryLineFee.IsSanMarinoDuty);

		entryLineFee.CF_ChargeType = "270";
		Assert("When charge type is: 270, IsDuty", entryLineFee.IsSanMarinoDuty);
	}

	public void TestIsVat()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();

		entryLineFee.CF_ChargeType = "";
		Assert("Empty charge type", !entryLineFee.IsVat);

		entryLineFee.CF_ChargeType = "XXX";
		Assert("XXX charge type", !entryLineFee.IsVat);

		entryLineFee.CF_ChargeType = "B00";
		Assert("B00 charge type", entryLineFee.IsVat);

		entryLineFee.CF_ChargeType = "405";
		Assert("405 charge type", entryLineFee.IsVat);
	}

	public void TestIsVatExemption()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();

		entryLineFee.CF_ChargeType = "";
		Assert("Empty charge type", !entryLineFee.IsVatExemption);

		entryLineFee.CF_ChargeType = "XXX";
		Assert("XXX charge type", !entryLineFee.IsVatExemption);

		entryLineFee.CF_ChargeType = "406";
		Assert("406 charge type", entryLineFee.IsVatExemption);

		entryLineFee.CF_ChargeType = "407";
		Assert("407 charge type", entryLineFee.IsVatExemption);
	}

	public void TestEntryLineFeeTypeVAT()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		var cusEntryLineFee = entryLine.Fees.AddNew();

		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		AssertContains("B00", cusEntryLineFee.Lookups.ChargeTypeList.CodesAsString);
		declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
		AssertContains("B00", cusEntryLineFee.Lookups.ChargeTypeList.CodesAsString);
	}

	public void TestAllowNegativeAmount()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();
		entryLineFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406;
		Assert($"{UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406} charge type should allow negative amounts", entryLineFee.AllowNegativeAmount);
		entryLineFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode407;
		Assert($"{UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode407} charge type should allow negative amounts", entryLineFee.AllowNegativeAmount);
		entryLineFee.CF_ChargeType = ZString.Empty;
		Assert("Empty charge type should not allow negative amounts", !entryLineFee.AllowNegativeAmount);
		entryLineFee.CF_ChargeType = "XXX";
		Assert("Any other charge type should not allow negative amounts", !entryLineFee.AllowNegativeAmount);
	}

	public void TestIFeeMembers()
	{
		var fee = Factory.New<CusEntryLineFee>();
		fee.CF_ChargeType = "A00";
		fee.CF_BaseValue = 1m;
		fee.CF_Rate = 2m;
		fee.CF_MethodOfCalculation = "A";
		fee.CF_ChargeAmount = 123m;
		fee.CF_MethodOfPayment = "X";
		CombineAssertions(() =>
		{
			var iFee = (IFee)fee;
			AssertEquals(nameof(iFee.ChargeType), "A00", iFee.ChargeType);
			AssertEquals(nameof(iFee.BaseValue), 1m, iFee.BaseValue);
			AssertEquals(nameof(iFee.Rate), 2m, iFee.Rate);
			AssertEquals(nameof(iFee.MethodOfCalculation), "A", iFee.MethodOfCalculation);
			AssertEquals(nameof(iFee.Amount), 123m, iFee.Amount);
			AssertEquals(nameof(iFee.MethodOfPayment), "X", iFee.MethodOfPayment);
		});
	}

	public void TestRoundChangeAmount()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();

		entryLineFee.CF_ChargeAmount = 0.6542m;
		AssertEquals("ChargeAmount", 0.65m, entryLineFee.CF_ChargeAmount);

		entryLineFee.CF_ChargeAmount = 0.6555;
		AssertEquals("ChargeAmount", 0.66m, entryLineFee.CF_ChargeAmount);

		entryLineFee.CF_ChargeAmount = -0.6555;
		AssertEquals("ChargeAmount", -0.66m, entryLineFee.CF_ChargeAmount);

		entryLineFee.CF_ChargeAmount = 0.004m;
		AssertEquals("When absolute ChargeAmount value is greater than zero, Minimum value for ChargeAmount", 0.01m, entryLineFee.CF_ChargeAmount);

		entryLineFee.CF_ChargeAmount = -0.004;
		AssertEquals("When absolute ChargeAmount value is greater than zero, Minimum value for ChargeAmount", -0.01m, entryLineFee.CF_ChargeAmount);
	}

	public void TestChargeAmountRounderType()
	{
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		AssertType<TwoDigitCustomChargeAmountRounder>($"{nameof(CusEntryLineFee.ChargeAmountRounder)} type", entryLineFee.ChargeAmountRounder);
	}

	public void TestChargeAmountRefresher()
	{
		var lineFee = Factory.New<CusEntryLineFee>();
		AssertType<ChargeAmountRefresher>("ChargeAmountRefresher Type", lineFee.ChargeAmountRefresher);
	}

	public void TestReadOnlyFields_ForEXCRateOverrideReasonCode()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();
		entryLineFee.CF_ChargeType = "B00";
		entryLineFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
		AssertReadOnly("When CF_RateOverrideReasonCode=ADD", false);

		entryLineFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Exclude;
		entryLineFee.CF_ChargeType = ZString.Empty;
		AssertReadOnly("When CF_RateOverrideReasonCode=EXC and CF_ChargeType is Empty", false);

		entryLineFee.CF_ChargeType = "A00";
		AssertReadOnly("When CF_RateOverrideReasonCode=EXC and CF_ChargeType is Not Empty", true);

		void AssertReadOnly(string groupAssertionMessage, bool expectedIsReadOnly)
		{
			CombineAssertions(groupAssertionMessage, () =>
			{
				AssertEquals("CH_ChargeType", expectedIsReadOnly, entryLineFee.CF_ChargeTypeInfo.ReadOnly);
				AssertEquals("CF_MethodOfCalculation", expectedIsReadOnly, entryLineFee.CF_MethodOfCalculationInfo.ReadOnly);
				AssertEquals("CF_Rate", expectedIsReadOnly, entryLineFee.CF_RateInfo.ReadOnly);
				AssertEquals("CF_ChargeAmount", expectedIsReadOnly, entryLineFee.CF_ChargeAmountInfo.ReadOnly);
				AssertEquals("CF_MethodOfPayment", expectedIsReadOnly, entryLineFee.CF_MethodOfPaymentInfo.ReadOnly);
				AssertEquals("CF_BaseValue", expectedIsReadOnly, entryLineFee.CF_BaseValueInfo.ReadOnly);
			});
		}
	}

	public void TestSetBaseValueToZero_ForEXCRateOverrideReasonCode()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();
		entryLineFee.CF_BaseValue = 10m;

		entryLineFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
		AssertEquals("BaseValue when CF_RateOverrideReasonCode=ADD", 10m, entryLineFee.CF_BaseValue);

		entryLineFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Exclude;
		AssertEquals("BaseValue when CF_RateOverrideReasonCode=EXC", 0m, entryLineFee.CF_BaseValue);

		entryLineFee.CF_BaseValue = 20m;
		entryLineFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Exclude;
		AssertEquals("BaseValue when CF_RateOverrideReasonCode=OVR", 20m, entryLineFee.CF_BaseValue);
	}

	public void TestIsPortTax()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.SetupHarbourRates();

		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
		var entryLineFee = entryLine.Fees.AddNew();

		CombineAssertions(() =>
		{
			entryLineFee.CF_ChargeType = "";
			AssertEquals("When ChargeType is empty, IsPortTax", false, entryLineFee.IsPortTax);

			entryLineFee.CF_ChargeType = "9AA";
			AssertEquals("When ChargeType is a valid port tax rate code, IsPortTax", true, entryLineFee.IsPortTax);

			entryLineFee.CF_ChargeType = "XYZ";
			AssertEquals("When ChargeType is not a valid port tax rate code, IsPortTax", false, entryLineFee.IsPortTax);
		});
	}

	public void TestIsCarTax()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();
		entryLineFee.CF_ChargeType = "";
		AssertEquals("IsCarTax", false, entryLineFee.IsCarTax);

		entryLineFee.CF_ChargeType = "423";
		AssertEquals("IsCarTax", true, entryLineFee.IsCarTax);
	}

	public void TestIsMiscellaneousContingentRevenueConcerningTax()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();
		entryLineFee.CF_ChargeType = "";
		AssertEquals("IsMiscellaneousContingentRevenueConcerningTax", false, entryLineFee.IsMiscellaneousContingentRevenueConcerningTax);

		entryLineFee.CF_ChargeType = "430";
		AssertEquals("IsMiscellaneousContingentRevenueConcerningTax", true, entryLineFee.IsMiscellaneousContingentRevenueConcerningTax);
	}

	public void TestIsRecoveryOfCourtCostsTax()
	{
		var entryLineFee = Factory.New<CusEntryLineFee>();
		entryLineFee.CF_ChargeType = "";
		AssertEquals("IsRecoveryOfCourtCostsTax", false, entryLineFee.IsRecoveryOfCourtCostsTax);

		entryLineFee.CF_ChargeType = "445";
		AssertEquals("IsRecoveryOfCourtCostsTax", true, entryLineFee.IsRecoveryOfCourtCostsTax);
	}

	void SetupRates()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);

		var eunDataGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
		helper.CreateNewOrGetExistingDataGrouping("IT", parent: eunDataGrouping);

		var secRateType = helper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "SEC", description: "Security Deposit");
		var excRateType = helper.CreateCusRateType(dataGroupingCode: "IT", rateType: "EXC", description: "Excise");
		var moeRateType = helper.CreateCusRateType(dataGroupingCode: "EUN", rateType: "MNV", description: "Miscellaneous Not VATable");
		var mieRateType = helper.CreateCusRateType(dataGroupingCode: "IT", rateType: "MIE", description: "Miscellaneous Import Export");

		helper.CreateCusRateCode(Factory, zy1RateCode: "SEC", secRateType.PK);
		helper.CreateCusRateCode(Factory, zy1RateCode: "131", excRateType.PK);
		helper.CreateCusRateCode(Factory, zy1RateCode: "217", moeRateType.PK);
		helper.CreateCusRateCode(Factory, zy1RateCode: "444", mieRateType.PK);
	}
}
