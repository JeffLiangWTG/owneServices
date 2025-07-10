using System;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using RefCusRateCodes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class CusEntryLineFeeMopValidatorTest : CusEntryLineFeeValidationTest
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when cusEntryLine parameter is null", () => new CusEntryLineFeeMopValidator(null));
	}

	public void TestCheckCF_MethodOfPaymentWithDifferentApprovalDeferNo()
	{
		(var declaration, _, var fee) = SetEntryLineFeeData();
		declaration.JE_DefermentAccountNumber = "";
		fee.CF_MethodOfPayment = "E";
		AssertHasWarningContaining("When JE_DefermentAccountNumber is Empty and MoP is E", fee.CF_MethodOfPaymentInfo, NotEnteredApprovalDeferNoExpectedWarningMessage);

		fee.CF_MethodOfPayment = "A";
		AssertNoWarningContaining("When JE_DefermentAccountNumber is Empty and MoP is A", fee.CF_MethodOfPaymentInfo, NotEnteredApprovalDeferNoExpectedWarningMessage);

		declaration.JE_DefermentAccountNumber = "123456A";
		fee.CF_MethodOfPayment = "G";
		AssertNoWarningContaining("When JE_DefermentAccountNumber is entered and MoP is G", fee.CF_MethodOfPaymentInfo, NotEnteredApprovalDeferNoExpectedWarningMessage);
	}

	public void TestCheckCF_MethodOfPaymentForMopA()
	{
		var approvalDefermentAccountHasBeenSetButNotUsedWarningMessage = "Approval Defer No. has been set but not used, consider to change the Method of Payment";

		(var declaration, _, var fee) = SetEntryLineFeeData();
		declaration.JE_DefermentAccountNumber = "123456A";
		fee.CF_MethodOfPayment = "A";
		AssertHasWarningContaining("When JE_DefermentAccountNumber is entered and MoP is A", fee.CF_MethodOfPaymentInfo, approvalDefermentAccountHasBeenSetButNotUsedWarningMessage);

		fee.CF_MethodOfPayment = "F";
		AssertNoWarningContaining("When JE_DefermentAccountNumber is entered and MoP is not A", fee.CF_MethodOfPaymentInfo, approvalDefermentAccountHasBeenSetButNotUsedWarningMessage);
	}

	public void TestMOPIsRWhenChargeTypeIsA35OrA45()
	{
		(_, _, var fee) = SetEntryLineFeeData();
		var mopShouldBeRWhenChargeTypeIsA35orA45WarningMessage = "A35 and A45 are provisional duties and should be guaranteed with Method of Payment R";

		fee.CF_ChargeType = "A45";
		fee.CF_MethodOfPayment = "A";
		AssertHasWarningContaining("When CF_ChargeType is A35 or A45 and MoP is not R", fee.CF_MethodOfPaymentInfo, mopShouldBeRWhenChargeTypeIsA35orA45WarningMessage);

		fee.CF_MethodOfPayment = "R";
		AssertNoWarningContaining("When CF_ChargeType is A35 or A45 and MoP is not R", fee.CF_MethodOfPaymentInfo, mopShouldBeRWhenChargeTypeIsA35orA45WarningMessage);
	}

	public void TestApprovalDefermentIsAppropriate()
	{
		const string approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage = "Method of Payment for duties when Approval Defer No. is set should be F or T";
		(var declaration, _, var fee) = SetEntryLineFeeData();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		fee.CF_ChargeType = "A00";
		declaration.JE_DefermentAccountNumber = "123456A";

		fee.CF_MethodOfPayment = "R";
		fee.Validation.ValidateCF_MethodOfPayment();
		AssertHasWarningContaining("When CF_ChargeType is not A35 or A45 and MoP is R", fee.CF_MethodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage);

		fee.CF_MethodOfPayment = "T";
		fee.Validation.ValidateCF_MethodOfPayment();
		AssertNoWarningContaining("When CF_ChargeType is not A35 or A45 and MoP is T", fee.CF_MethodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage);
	}

	public void TestCheckCF_MethodOfPaymentWhenFeeIsDuty()
	{
		const string approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage = "Method of Payment for duties when Approval Defer No. is set should be F or T";

		(var declaration, _, var fee) = SetEntryLineFeeData();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.JE_DefermentAccountNumber = "123456A";
		fee.CF_ChargeType = "A00";
		fee.CF_MethodOfPayment = "R";
		AssertHasWarningContaining("When Fee is duty (starts with A or 27), Approval Defer No has been set and MoP is not F or T", fee.CF_MethodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage);

		declaration.JE_DefermentAccountNumber = "123456A";
		fee.CF_ChargeType = "270";
		fee.CF_MethodOfPayment = "T";
		AssertNoWarningContaining("When Fee is duty (starts with A or 27), Approval Defer No has been set and MoP is not F or T", fee.CF_MethodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage);

		declaration.JE_DefermentAccountNumber = "";
		fee.CF_ChargeType = "A00";
		fee.CF_MethodOfPayment = "R";
		AssertNoWarningContaining(fee.CF_MethodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage);

		declaration.JE_DefermentAccountNumber = "123456A";
		fee.CF_ChargeType = "172";
		fee.CF_MethodOfPayment = "R";
		AssertNoWarningContaining(fee.CF_MethodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage);

		fee.CF_ChargeType = "A35";
		fee.CF_MethodOfPayment = "T";
		AssertNoWarningContaining(fee.CF_MethodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage);
	}

	public void TestCheckCF_MethodOfPaymentWhenFeeIsDutyForImpUCC6AndApprovalHasNoDAT()
	{
		SetUpProcedureWithAttribute();

		var expectedWarningNoDAT = "Method of Payment should be E.";

		(var declaration, var entryLine, var fee) = SetEntryLineFeeData();
		declaration.JE_MessageType = "IMP";
		declaration.JE_DefermentAccountNumber = "123456A";
		fee.CF_ChargeType = "A00";
		fee.CF_MethodOfPayment = "R";
		AssertHasWarningContaining("When Fee is duty (starts with A or 27), Approval Defer No has been set and MoP is not E", fee.CF_MethodOfPaymentInfo, expectedWarningNoDAT);

		declaration.JE_DefermentAccountNumber = "123456A";
		fee.CF_ChargeType = "270";
		fee.CF_MethodOfPayment = "E";
		AssertNoWarningContaining("When Fee is duty (starts with A or 27), Approval Defer No has been set and MoP is not E", fee.CF_MethodOfPaymentInfo, expectedWarningNoDAT);

		declaration.JE_DefermentAccountNumber = "";
		fee.CF_ChargeType = "A00";
		fee.CF_MethodOfPayment = "R";
		AssertNoWarningContaining(fee.CF_MethodOfPaymentInfo, expectedWarningNoDAT);

		declaration.JE_DefermentAccountNumber = "123456A";
		fee.CF_ChargeType = "172";
		fee.CF_MethodOfPayment = "R";
		AssertNoWarningContaining(fee.CF_MethodOfPaymentInfo, expectedWarningNoDAT);

		fee.CF_ChargeType = "A35";
		fee.CF_MethodOfPayment = "E";
		AssertNoWarningContaining(fee.CF_MethodOfPaymentInfo, expectedWarningNoDAT);

		var invoiceLine = entryLine.RandomLine;
		invoiceLine.JI_Procedure = "0000";
		fee.CF_ChargeType = "A00";
		fee.CF_MethodOfPayment = "X";
		AssertEquals("RefCusProcedureAttribute Rule applies", "X", ProcedureAttributeBasedMethodOfPaymentCalculator.GetNew(fee).GetDefaultValue());
		AssertNoWarningContaining("When Fee is duty (starts with A or 27), Approval Defer No has been set, MoP is not E, but RefCusProcedureAttribute Rule applies", fee.CF_MethodOfPaymentInfo, expectedWarningNoDAT);
	}

	public void TestCheckCF_MethodOfPaymentWhenFeeIsDutyForImpUCC6AndApprovalHasDAT()
	{
		var expectedWarningDAT = "Method of Payment should be D.";

		(var declaration, _, var fee) = SetEntryLineFeeData();
		declaration.JE_MessageType = "IMP";

		declaration.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;
		var importer = Factory.NewWithValidTestData<OrgHeader>();
		importer.OH_Code = "IMPORTER";
		importer.CustomsCodes.AddNew(ItalyOrgCusCodeInfo.OrgCusCodes.DefermentApprovaNumberForTrieste, "DAT123", declaration.CountryCode);
		declaration.JE_OH_Importer = importer.PK;

		declaration.JE_DefermentAccountNumber = "DAT123";
		fee.CF_ChargeType = "A00";
		fee.CF_MethodOfPayment = "R";
		AssertHasWarningContaining("When Fee is duty (starts with A or 27), Approval Defer No has been set and MoP is not D", fee.CF_MethodOfPaymentInfo, expectedWarningDAT);

		declaration.JE_DefermentAccountNumber = "DAT123";
		fee.CF_ChargeType = "270";
		fee.CF_MethodOfPayment = "D";
		AssertNoWarningContaining("When Fee is duty (starts with A or 27), Approval Defer No has been set and MoP is not D", fee.CF_MethodOfPaymentInfo, expectedWarningDAT);

		declaration.JE_DefermentAccountNumber = "";
		fee.CF_ChargeType = "A00";
		fee.CF_MethodOfPayment = "R";
		AssertNoWarningContaining(fee.CF_MethodOfPaymentInfo, expectedWarningDAT);

		declaration.JE_DefermentAccountNumber = "DAT123";
		fee.CF_ChargeType = "172";
		fee.CF_MethodOfPayment = "R";
		AssertNoWarningContaining(fee.CF_MethodOfPaymentInfo, expectedWarningDAT);

		fee.CF_ChargeType = "A35";
		fee.CF_MethodOfPayment = "D";
		AssertNoWarningContaining(fee.CF_MethodOfPaymentInfo, expectedWarningDAT);
	}

	public void TestCheckCF_MethodOfPayment_WhenFeeIsDutyForExportUCC6()
	{
		const string approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage = "Method of payment for duties, when Approval Defer No. is set, should be E or D";
		(var declaration, _, var lineFee) = SetEntryLineFeeData();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		var methodOfPaymentInfo = lineFee.CF_MethodOfPaymentInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: true))
		{
			declaration.JE_DefermentAccountNumber = "123456A";
			CombineAssertions("When Export UCC6 with Deferral account", () =>
			{
				lineFee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
				lineFee.CF_MethodOfPayment = "A";
				AssertHasWarningContaining($"And Fee is duty Without(A35,A45): {lineFee.CF_ChargeType} But MoP: '{lineFee.CF_MethodOfPayment}'", methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);

				lineFee.CF_ChargeType = RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts;
				lineFee.CF_MethodOfPayment = "F";
				AssertHasWarningContaining($"And Fee is duty Without(A35,A45): {lineFee.CF_ChargeType} But MoP: '{lineFee.CF_MethodOfPayment}'", methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);

				lineFee.CF_ChargeType = RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge;
				lineFee.CF_MethodOfPayment = "C";
				AssertHasWarningContaining($"And Fee is duty Without(A35,A45): {lineFee.CF_ChargeType} But MoP: '{lineFee.CF_MethodOfPayment}'", methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);

				lineFee.CF_ChargeType = RefCusRateCodes.DefinitiveAntiDumpingDuty;
				lineFee.CF_MethodOfPayment = "T";
				AssertHasWarningContaining($"And Fee is duty Without(A35,A45): {lineFee.CF_ChargeType} But MoP: '{lineFee.CF_MethodOfPayment}'", methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);

				lineFee.CF_ChargeType = RefCusRateCodes.DefinitiveCountervailingDuty;
				lineFee.CF_MethodOfPayment = "H";
				AssertHasWarningContaining($"And Fee is duty Without(A35,A45): {lineFee.CF_ChargeType} But MoP: '{lineFee.CF_MethodOfPayment}'", methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);

				lineFee.CF_ChargeType = RefCusRateCodes.DefinitiveCountervailingDuty;
				lineFee.CF_MethodOfPayment = "A";
				AssertHasWarningContaining($"And Fee is duty Without(A35,A45): {lineFee.CF_ChargeType} But MoP: '{lineFee.CF_MethodOfPayment}'", methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);

				lineFee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
				lineFee.CF_MethodOfPayment = ZString.Empty;
				AssertHasWarningContaining($"And Fee is duty Without(A35,A45): {lineFee.CF_ChargeType} And MoP: '{lineFee.CF_MethodOfPayment}'", methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);

				lineFee.CF_ChargeType = RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge;
				lineFee.CF_MethodOfPayment = "D";
				AssertNoWarningContaining($"And Fee is duty Without(A35,A45): {lineFee.CF_ChargeType} And MoP: '{lineFee.CF_MethodOfPayment}'", methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);

				lineFee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
				lineFee.CF_MethodOfPayment = "E";
				AssertNoWarningContaining($"And Fee is duty Without(A35,A45): {lineFee.CF_ChargeType} And MoP: '{lineFee.CF_MethodOfPayment}'", methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);

				lineFee.CF_ChargeType = RefCusRateCodes.ProvisionalAntiDumpingDuty;
				lineFee.CF_MethodOfPayment = "A";
				AssertNoWarningContaining($"And Fee is A35: {lineFee.CF_ChargeType} And MoP: '{lineFee.CF_MethodOfPayment}'", methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);

				lineFee.CF_ChargeType = RefCusRateCodes.ProvisionalCountervailingDuty;
				lineFee.CF_MethodOfPayment = "A";
				AssertNoWarningContaining($"And Fee is A45: {lineFee.CF_ChargeType} And MoP: '{lineFee.CF_MethodOfPayment}'", methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);
			});

			declaration.JE_DefermentAccountNumber = ZString.Empty;

			lineFee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
			lineFee.CF_MethodOfPayment = "A";
			AssertNoWarningContaining($"When Export UCC6 withOUT Deferral account And Fee is duty Without(A35,A45): {lineFee.CF_ChargeType} But MoP: '{lineFee.CF_MethodOfPayment}'", methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);
		}
	}

	public void TestCheckCF_MethodOfPayment_CheckAvoidFallback()
	{
		const string approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage = "Method of payment for duties, when Approval Defer No. is set, should be E or D";
		const string approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage = "Method of Payment for duties when Approval Defer No. is set should be F or T";
		(var declaration, _, var lineFee) = SetEntryLineFeeData();
		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.JE_DefermentAccountNumber = "123456A";
		lineFee.CF_ChargeType = RefCusRateCodes.CustomsDutyOnIndustrialProducts;
		var methodOfPaymentInfo = lineFee.CF_MethodOfPaymentInfo;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: true))
		{
			CombineAssertions("When Export UCC6 with Deferral account and MoP is E", () =>
			{
				lineFee.CF_MethodOfPayment = "E";
				AssertNoWarningContaining(methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);
				AssertNoWarningContaining(methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage);
			});

			CombineAssertions("When Export UCC6 with Deferral account and MoP is R", () =>
			{
				lineFee.CF_MethodOfPayment = "R";
				AssertHasWarningContaining(methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);
				AssertNoWarningContaining(methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: false))
		{
			CombineAssertions("When Export Not UCC6 with Deferral account and Mop Is R", () =>
			{
				lineFee.CF_MethodOfPayment = "R";
				AssertHasWarningContaining("When CF_ChargeType is not A35 or A45 and MoP is R", lineFee.CF_MethodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage);
				AssertNoWarningContaining(methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: false))
		{
			CombineAssertions("When Export Not UCC6 with Deferral account and Mop Is T", () =>
			{
				lineFee.CF_MethodOfPayment = "T";
				AssertNoWarningContaining("When CF_ChargeType is not A35 or A45 and MoP is T", lineFee.CF_MethodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongWarningMessage);
				AssertNoWarningContaining(methodOfPaymentInfo, approvalDefermentAccountHasBeenSetAndFeeIsDutyButMopIsWrongExportWarningMessage);
			});
		}
	}

	public void TestCheckCF_MethodOfPaymentWithDifferentApprovalDeferNo_WhenOthersD_CheckDefermentAccountNumber()
	{
		(var declaration, _, var fee) = SetEntryLineFeeData();
		var methodOfPaymentInfo = fee.CF_MethodOfPaymentInfo;
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: true))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			fee.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.OthersD;
			AssertHasWarning("When Export UCC6 JE_DefermentAccountNumber is Empty and MoP is D", methodOfPaymentInfo, NotEnteredApprovalDeferNoExpectedWarningMessage);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			fee.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.SecurityDepositDeferredPaymentR;
			AssertNoWarning("When Export UCC6 JE_DefermentAccountNumber is Empty and MoP is NOT D", methodOfPaymentInfo, NotEnteredApprovalDeferNoExpectedWarningMessage);

			declaration.JE_DefermentAccountNumber = "DAT123";
			fee.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.OthersD;
			AssertNoWarning("When Export UCC6 JE_DefermentAccountNumber is NOT Empty and MoP is D", methodOfPaymentInfo, NotEnteredApprovalDeferNoExpectedWarningMessage);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			fee.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.OthersD;
			AssertHasWarning("When import UCC6 JE_DefermentAccountNumber is Empty and MoP is D", methodOfPaymentInfo, NotEnteredApprovalDeferNoExpectedWarningMessage);
		}
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: false))
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			fee.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.OthersD;
			AssertNoWarning("When Export UCC5 JE_DefermentAccountNumber is Empty and MoP is D", methodOfPaymentInfo, NotEnteredApprovalDeferNoExpectedWarningMessage);
		}
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(JobDeclaration declaration, bool isUCC6)
			=> ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	void SetUpProcedureWithAttribute()
	{
		var testDataHelper = new UniversalReferenceTestDataHelper(Factory);
		var procedure = testDataHelper.CreateRefCusProcedure("IT", "IM", "00", "00", "", "Procedure with attribute", "IMP");
		testDataHelper.CreateRefCusProcedureAttribute(procedure.PK, "DTYPaymentMethod", "X");
	}

	const string NotEnteredApprovalDeferNoExpectedWarningMessage = "You have not entered an Approval Defer No. for this method of payment";
}
