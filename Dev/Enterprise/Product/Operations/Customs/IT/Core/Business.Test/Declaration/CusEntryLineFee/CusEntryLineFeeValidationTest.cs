using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

abstract class CusEntryLineFeeValidationTest : EU.Business.Declaration.Testing.EUUniversalCusEntryLineFeeValidationTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
{
	public void TestCheckCF_ChargeType_ItalianVatCode()
	{
		const string expectedMessageError = "Do not use the code 405 for VAT. Instead, use the code B00, which is the standard VAT code for the EU.";

		var fee = Factory.New<CusEntryLineFee>();
		CombineAssertions(() =>
		{
			fee.CF_ChargeType = "";
			AssertNoError(fee.CF_ChargeTypeInfo, expectedMessageError);

			fee.CF_ChargeType = "A00";
			AssertNoError(fee.CF_ChargeTypeInfo, expectedMessageError);

			fee.CF_ChargeType = "405";
			AssertHasError(fee.CF_ChargeTypeInfo, expectedMessageError);
		});
	}

	protected override void AssertNotificationsForEmptyMethodOfPayment(EU.Business.Declaration.CusEntryLineFee fee) => AssertHasMessageErrorContaining(fee.CF_MethodOfPaymentInfo, MandatoryValidation.YouHaveNotEntered);

	public void TestCheckCF_MethodOfPaymentWithEmptyValue()
	{
		var fee = Factory.New<CusEntryLineFee>();
		fee.CF_MethodOfPayment = "";
		AssertHasMessageErrorContaining(fee.CF_MethodOfPaymentInfo, MandatoryValidation.YouHaveNotEntered);

		fee.CF_MethodOfPayment = "R";
		AssertNoMessageErrorContaining(fee.CF_MethodOfPaymentInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCF_MethodOfCalculationWithEmptyValue()
	{
		var fee = Factory.New<CusEntryLineFee>();
		fee.CF_MethodOfCalculation = "";
		AssertHasMessageErrorContaining(fee.CF_MethodOfCalculationInfo, MandatoryValidation.YouHaveNotEntered);

		fee.CF_MethodOfCalculation = "ARG";
		AssertNoMessageErrorContaining(fee.CF_MethodOfCalculationInfo, MandatoryValidation.YouHaveNotEntered);
	}

	public void TestCheckCF_Rate_WithRequestedProcedureNotRequiringVAT_ForImportDeclaration()
	{
		const string expectedWarningMessage = "For Procedure 42, Tax Rate should be = '0' or not present.";

		var testDataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		testDataHelper.SetUpRefDataWithVATRequirement();
		var (declaration, entryLine, portTax) = SetEntryLineFeeData();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		var entryInstruction = entryLine.Header.EntryInstruction;

		CombineAssertions("Import declaration", () =>
		{
			entryInstruction.CEI_Procedure = UniversalReferenceConstants.RefCusProcedureCodes.ReleaseForFreeCirculationOfGoodsForConsumption42;

			portTax.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
			portTax.CF_Rate = 20m;
			AssertHasWarning("When procedure = 42, charge type = B00 and tax rate = 20", portTax.CF_RateInfo, expectedWarningMessage);

			portTax.CF_Rate = ZDecimal.Zero;
			AssertNoWarning("When procedure = 42, charge type = B00 and tax rate = 0", portTax.CF_RateInfo, expectedWarningMessage);

			portTax.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
			portTax.CF_Rate = 20m;
			AssertNoWarning("When procedure = 42, charge type = A30 and tax rate = 20", portTax.CF_RateInfo, expectedWarningMessage);
		});
	}

	public void TestCheckCF_Rate_WithRequestedProcedureRequiringVAT_ForImportDeclaration()
	{
		var testDataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		testDataHelper.SetUpRefDataWithVATRequirement();
		var (declaration, entryLine, portTax) = SetEntryLineFeeData();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		var entryInstruction = entryLine.Header.EntryInstruction;

		entryInstruction.CEI_Procedure = UniversalReferenceConstants.RefCusProcedureCodes.IntoTemporaryImport;

		portTax.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		portTax.CF_Rate = 20m;
		AssertNoNotifications(portTax.CF_RateInfo);
	}

	public void TestCheckCF_Rate_WithRequestedProcedureNotRequiringVAT_ForExportDeclaration()
	{
		var testDataHelper = new ITUniversalReferenceTestDataHelper(Factory);
		testDataHelper.SetUpRefDataWithVATRequirement();
		var (declaration, entryLine, portTax) = SetEntryLineFeeData();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		var entryInstruction = entryLine.Header.EntryInstruction;

		entryInstruction.CEI_Procedure = UniversalReferenceConstants.RefCusProcedureCodes.IntoTemporaryExport;

		portTax.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		portTax.CF_Rate = 20m;
		AssertNoNotifications(portTax.CF_RateInfo);
	}

	public void TestCheckCF_ChargeTypeDuplicatePortTax()
	{
		var helper = new ITUniversalReferenceTestDataHelper(Factory);
		helper.SetupHarbourRates();

		var portTaxCanOnlyBeEnteredOnceExpectedMessageError = "Port Tax can only be entered once";
		var portTaxShouldNotBeUsedForSelectedTransportMode = "Port Tax should not be used for the selected transport mode.";

		using (ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
		{
			(var declaration, _, var portTax) = SetEntryLineFeeData();
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";
			declaration.JE_RL_NKPortOfArrival = "ITCEJ";

			var entryLine = portTax.EntryLine;
			portTax.CF_ChargeType = "9AA";
			AssertNoMessageErrorContaining("When only one port tax, no message error expected", portTax.CF_ChargeTypeInfo, portTaxCanOnlyBeEnteredOnceExpectedMessageError);

			var anotherPortTax = entryLine.Fees.AddNew();
			anotherPortTax.CF_ChargeType = "9AB";
			AssertHasMessageErrorContaining("When two or more port taxes, message error expected", anotherPortTax.CF_ChargeTypeInfo, portTaxCanOnlyBeEnteredOnceExpectedMessageError);

			var genericFee = entryLine.Fees.AddNew();
			genericFee.CF_ChargeType = "A00";
			AssertNoMessageErrorContaining("No message error expected", genericFee.CF_ChargeTypeInfo, portTaxCanOnlyBeEnteredOnceExpectedMessageError);

			var anotherGenericFee = entryLine.Fees.AddNew();
			anotherGenericFee.CF_ChargeType = "A00";
			AssertNoMessageErrorContaining("When two or more fee have same Type but they are not Port Tax, duplicated port tax message error expected", anotherGenericFee.CF_ChargeTypeInfo, portTaxCanOnlyBeEnteredOnceExpectedMessageError);

			declaration.JE_TransportMode = "AIR";
			portTax.Validation.ValidateCF_ChargeType();
			AssertNoMessageErrorContaining("When transport mode declaration is not SEA, no message error expected", portTax.CF_ChargeTypeInfo, portTaxCanOnlyBeEnteredOnceExpectedMessageError);
			AssertHasMessageErrorContaining("When transport mode declaration is not SEA but a port tax has been entered", portTax.CF_ChargeTypeInfo, portTaxShouldNotBeUsedForSelectedTransportMode);
		}

		using (ITCustomsDataRegistry.Instance.ITGeneratePortTaxes.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
		{
			(var declaration, _, var portTax) = SetEntryLineFeeData();
			declaration.JE_MessageType = "EXP";
			declaration.JE_TransportMode = "SEA";
			declaration.JE_RL_NKPortOfArrival = "ITCEJ";

			AssertEquals("[PRE-CONDITION] NeedsPortTax", false, declaration.NeedsPortTax);

			var entryLine = portTax.EntryLine;
			portTax.CF_ChargeType = "9AA";
			AssertNoMessageErrorContaining("When transport mode declaration is SEA, no message error expected", portTax.CF_ChargeTypeInfo, portTaxShouldNotBeUsedForSelectedTransportMode);

			var anotherPortTaxFee = entryLine.Fees.AddNew();
			anotherPortTaxFee.CF_ChargeType = "9AB";
			AssertHasMessageErrorContaining("When two or more port taxes, message error expected", anotherPortTaxFee.CF_ChargeTypeInfo, portTaxCanOnlyBeEnteredOnceExpectedMessageError);
		}
	}

	public void TestCheckCF_ChargeType_VatExemptionFeeRequiresDeclarationOfIntentSupportingDocument()
	{
		var messageError = ValidationCaptions.CusEntryLineFee.VatExemptionFeeRequiresDeclarationOfIntentSupportingDocument;
		(_, var entryLine, var fee) = SetEntryLineFeeData();
		var invoiceLine = entryLine.RandomLine;

		fee.CF_ChargeType = "XXX";
		AssertNoMessageErrorContaining("Validation is run only if CF_ChargeType = '406'", fee.CF_ChargeTypeInfo, messageError);

		fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406;
		AssertHasMessageErrorContaining("'01DI' supporting document is missing", fee.CF_ChargeTypeInfo, messageError);

		invoiceLine.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;
		fee.Validation.ValidateCF_ChargeType();
		AssertNoMessageErrorContaining("'01DI' supporting document is added", fee.CF_ChargeTypeInfo, messageError);
	}

	public void TestCheckCF_ChargeType_DeclarationOfIntentSupportingDocumentRequiresVatExemptionFee()
	{
		var messageError = ValidationCaptions.CusEntryLineFee.DeclarationOfIntentSupportingDocumentRequiresVatExemptionFee;
		(_, var entryLine, var vatExemptionFee) = SetEntryLineFeeData();
		var invoiceLine = entryLine.RandomLine;
		invoiceLine.SupportingDocuments.AddNew().CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.DeclarationOfIntent;

		var testFee = entryLine.Fees.AddNew();
		testFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty;
		AssertNoMessageErrorContaining("Validation is run only if CF_ChargeType = 'B00'", testFee.CF_ChargeTypeInfo, messageError);

		testFee.CF_ChargeType = EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat;
		AssertHasMessageErrorContaining("'406' fee is missing", testFee.CF_ChargeTypeInfo, messageError);

		vatExemptionFee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406;
		testFee.Validation.ValidateCF_ChargeType();
		AssertNoMessageErrorContaining("'406' fee is added", testFee.CF_ChargeTypeInfo, messageError);
	}

	public void TestEntryLineFeeTypeVAT()
	{
		var fee = SetEntryLineFeeData().entryLineFee;
		fee.CF_ChargeType = "B00";
		AssertNoMessageErrors(fee.CF_ChargeTypeInfo);
	}

	public void TestCheckCF_BaseValue_NegativeValidationBasedOnChargeType()
	{
		var fee = SetEntryLineFeeData().entryLineFee;
		AssertNegativeAmountValidation(fee.CF_BaseValueInfo);
	}

	public void TestCheckCF_ChargeAmount_NegativeValidationBasedOnChargeType()
	{
		var fee = SetEntryLineFeeData().entryLineFee;
		AssertNegativeAmountValidation(fee.CF_ChargeAmountInfo);
	}

	void AssertNegativeAmountValidation(ZPropertyInfo amountPtyInfo)
	{
		var fee = (CusEntryLineFee)amountPtyInfo.BizObj;

		fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode406;
		fee[amountPtyInfo.Name] = -1;
		AssertNoMessageErrorContaining($"Negative value is allowed when ChargeType is {fee.CF_ChargeType}", amountPtyInfo, MandatoryValidation.ValueCannotBeNegative);

		fee.CF_ChargeType = UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatExemptionCode407;
		fee[amountPtyInfo.Name] = -1;
		AssertNoMessageErrorContaining($"Negative value is allowed when ChargeType is {fee.CF_ChargeType}", amountPtyInfo, MandatoryValidation.ValueCannotBeNegative);

		fee.CF_ChargeType = "A00";
		fee[amountPtyInfo.Name] = -1;
		AssertHasMessageErrorContaining(amountPtyInfo, MandatoryValidation.ValueCannotBeNegative);

		fee.CF_ChargeType = "B00";
		fee[amountPtyInfo.Name] = -1;
		AssertHasMessageErrorContaining(amountPtyInfo, MandatoryValidation.ValueCannotBeNegative);
	}

	public void TestCheckCF_ChargeAmount_TotalAmountIsDifferentFromSystemCalculatedAmount_AdditionalFee()
	{
		var lineFee = SetEntryLineFeeData().entryLineFee;
		lineFee.CF_RateOverrideReasonCode = "ADD";
		lineFee.CF_BaseValue = 10m;
		lineFee.CF_MethodOfCalculation = "";
		lineFee.CF_Rate = 1.1234m;

		var chargeAmountCalculator = lineFee.ChargeAmountRefresher.GetNewChargeAmountCalculator();
		var systemCalculatedAndRoundedValue = lineFee.ChargeAmountRounder.Round(chargeAmountCalculator.Calculate());
		AssertEquals("PRE-CONDITION: System Calculated Value", 11.23m, systemCalculatedAndRoundedValue);

		lineFee.CF_ChargeAmount = 99m;
		AssertHasWarningContaining(lineFee.CF_ChargeAmountInfo, "The Total Amount is different from what is calculated by the system: 11.23");

		lineFee.CF_ChargeAmount = 11.23m;
		AssertNoWarnings(lineFee.CF_ChargeAmountInfo);
	}

	public void TestCheckCF_ChargeAmount_TotalAmountIsDifferentFromSystemCalculatedAmount_OverrideFee()
	{
		var lineFee = SetEntryLineFeeData().entryLineFee;
		lineFee.CF_RateOverrideReasonCode = "OVR";
		lineFee.CF_BaseValue = 10m;
		lineFee.CF_MethodOfCalculation = "";
		lineFee.CF_Rate = 1.1234m;

		var chargeAmountCalculator = lineFee.ChargeAmountRefresher.GetNewChargeAmountCalculator();
		var systemCalculatedAndRoundedValue = lineFee.ChargeAmountRounder.Round(chargeAmountCalculator.Calculate());
		AssertEquals("PRE-CONDITION: System Calculated Value", 11.23m, systemCalculatedAndRoundedValue);

		lineFee.CF_ChargeAmount = 99m;
		AssertHasWarningContaining(lineFee.CF_ChargeAmountInfo, "The Total Amount is different from what is calculated by the system: 11.23");

		lineFee.CF_ChargeAmount = 11.23m;
		AssertNoWarnings(lineFee.CF_ChargeAmountInfo);
	}

	public void TestCheckCF_ChargeAmount_TotalAmountIsDifferentFromSystemCalculatedAmount_SystemAddedFee()
	{
		var lineFee = SetEntryLineFeeData().entryLineFee;
		lineFee.CF_RateOverrideReasonCode = "";
		lineFee.CF_BaseValue = 10m;
		lineFee.CF_MethodOfCalculation = "";
		lineFee.CF_Rate = 1.1234m;

		var chargeAmountCalculator = lineFee.ChargeAmountRefresher.GetNewChargeAmountCalculator();
		var systemCalculatedAndRoundedValue = lineFee.ChargeAmountRounder.Round(chargeAmountCalculator.Calculate());
		AssertEquals("PRE-CONDITION: System Calculated Value", 11.23m, systemCalculatedAndRoundedValue);

		lineFee.CF_ChargeAmount = 99m;
		AssertNoWarnings(lineFee.CF_ChargeAmountInfo);

		lineFee.CF_ChargeAmount = 11.23m;
		AssertNoWarnings(lineFee.CF_ChargeAmountInfo);
	}

	public void TestCheckCF_BaseValue_NoMandatoryValidationWhenRateOverrideCodeIsEXCAndValueIsZero()
	{
		var (declaration, _, lineFee) = SetEntryLineFeeData();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		SetAndAssertBaseValueValidationIsIgnoredForEXCAction("IMP");

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		SetAndAssertBaseValueValidationIsIgnoredForEXCAction("EXP");

		void SetAndAssertBaseValueValidationIsIgnoredForEXCAction(string declarationType)
		{
			CombineAssertions($"With Declaration={declarationType}", () =>
			{
				lineFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Exclude;
				lineFee.CF_BaseValue = 0;
				lineFee.Validation.ValidateCF_BaseValue();
				AssertNoMessageErrorContaining("When Action=EXC, ChargeType not Empty and Base Value=0", lineFee.CF_BaseValueInfo, MandatoryValidation.YouHaveNotEntered);

				lineFee.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
				lineFee.Validation.ValidateCF_BaseValue();
				AssertHasMessageErrorContaining("When Action=ADD, Base Value=0", lineFee.CF_BaseValueInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}
	}

	public void TestCheckBaseValuePrecision()
	{
		const string expectedMessage = "Base Amount allows only 2 decimal places.";
		var entryLineFee = SetEntryLineFeeData().entryLineFee;
		CombineAssertions("When CF_BaseValue has 6 decimals", () =>
		{
			entryLineFee.CF_BaseValue = 1.234567m;
			AssertNoMessageError("When CF_MethodOfCalculation empty, CF_BaseValue", entryLineFee.CF_BaseValueInfo, expectedMessage);

			entryLineFee.CF_MethodOfCalculation = "%";
			entryLineFee.Validation.ValidateCF_BaseValue();
			AssertHasMessageError("When CF_MethodOfCalculation %, CF_BaseValue", entryLineFee.CF_BaseValueInfo, expectedMessage);

			entryLineFee.CF_MethodOfCalculation = "DTN";
			entryLineFee.Validation.ValidateCF_BaseValue();
			AssertNoMessageError("When CF_MethodOfCalculation DTN, CF_BaseValue", entryLineFee.CF_BaseValueInfo, expectedMessage);
		});

		entryLineFee.CF_MethodOfCalculation = "%";
		entryLineFee.CF_BaseValue = 1.23m;
		AssertNoMessageError("When CF_MethodOfCalculation % and CF_BaseValue has 2 decimals", entryLineFee.CF_BaseValueInfo, expectedMessage);
	}
}
