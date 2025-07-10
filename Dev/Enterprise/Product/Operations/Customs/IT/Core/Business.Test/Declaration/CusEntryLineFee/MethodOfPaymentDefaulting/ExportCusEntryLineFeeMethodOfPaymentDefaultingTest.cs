using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using EUJobMessageTypeList = Enterprise.Customs.Common.EU.EUJobMessageTypeList;
using RefCusRateCodes = Enterprise.Customs.EU.Business.UniversalReferenceConstants.RefCusRateCodes;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ExportCusEntryLineFeeMethodOfPaymentDefaultingTest : TestCaseWithFactory
{
	public void TestDefaulting_WhenChargeTypeIsTemporaryAntiDumping()
	{
		lineFee.CF_ChargeType = "A35";
		AssertEquals("Method of payment", "R", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenChargeTypeIsTemporaryCountervailing()
	{
		lineFee.CF_ChargeType = "A45";
		AssertEquals("Method of payment", "R", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenDefermentAccountNumberIsEmpty()
	{
		declaration.JE_DefermentAccountNumber = "";
		lineFee.CF_ChargeType = "XXX";
		AssertEquals("Method of payment", "A", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenDefermentAccountNumberIsEqualToDatCode()
	{
		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		orgHeader.CustomsCodes.AddNew("DAT", "DAT123", "IT");
		declaration.JE_OH_Importer = orgHeader.PK;
		declaration.JE_DefermentAccountNumber = "DAT123";
		lineFee.CF_ChargeType = "XXX";
		AssertEquals("Method of payment", "T", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenDefermentAccountNumberIsNotEmptyAndChargeTypeIsDuty()
	{
		declaration.JE_DefermentAccountNumber = "ABCDEF";
		lineFee.CF_ChargeType = "ANN";
		AssertEquals("Method of payment", "F", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenDefermentAccountNumberIsNotEmptyAndChargeTypeIsSanMarinoDuty()
	{
		declaration.JE_DefermentAccountNumber = "ABCDEF";
		lineFee.CF_ChargeType = "27N";
		AssertEquals("Method of payment", "F", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_WhenDefermentAccountNumberIsNotEmpty()
	{
		declaration.JE_DefermentAccountNumber = "ABCDEF";
		lineFee.CF_ChargeType = "XXX";
		AssertEquals("Method of payment", "G", lineFee.CF_MethodOfPayment);
	}

	public void TestDefaulting_SuggestionA_WhenUCC6ExportWithoutDefermentAccountNumberAndIsDutyButNotProvisional()
	{
		const string expectedImmediatePaymentInCashA = "A";

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.JE_DefermentAccountNumber = ZString.Empty;

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: true))
		{
			foreach (var chargeType in chargeTypeDutyListWithoutProvisional)
			{
				lineFee.CF_ChargeType = chargeType;
				AssertEquals($"When Export UCC6 Without Deferral account And ChargeType is {chargeType}", expectedImmediatePaymentInCashA, lineFee.CF_MethodOfPayment);
			}
		}
	}

	public void TestDefaulting_SuggestionE_WhenUCC6ExportWithDefermentAccountNumberAndIsDutyButNotProvisional()
	{
		const string expectedDeferredPaymentE = "E";

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.JE_DefermentAccountNumber = "123456A";

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: true))
		{
			foreach (var chargeType in chargeTypeDutyListWithoutProvisional)
			{
				lineFee.CF_ChargeType = chargeType;
				AssertEquals($"When Export UCC6 with Deferral account And ChargeType is {chargeType}", expectedDeferredPaymentE, lineFee.CF_MethodOfPayment);
			}
		}
	}

	public void TestDefaulting_SuggestionR_WhenUCC6ExportWithDefermentAccountNumberAndIsDutyProvisional()
	{
		const string expectedSecurityDepositDeferredPaymentR = "R";

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.JE_DefermentAccountNumber = "123456A";
		var chargeTypeDutyProvisionalList = new List<string>() {
				RefCusRateCodes.ProvisionalAntiDumpingDuty,
				RefCusRateCodes.ProvisionalCountervailingDuty,
		};

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: true))
		{
			foreach (var chargeType in chargeTypeDutyProvisionalList)
			{
				lineFee.CF_ChargeType = chargeType;
				AssertEquals($"When Export UCC6 with Deferral account And ChargeType is {chargeType}", expectedSecurityDepositDeferredPaymentR, lineFee.CF_MethodOfPayment);
			}
		}
	}

	public void TestDefaulting_SuggestionF_WhenNotUCC6ExportWithDefermentAccountNumberAndIsDutyButNotProvisional()
	{
		const string expectedDeferredPaymentCustomsProcedureF = "F";

		declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		declaration.JE_DefermentAccountNumber = "123456A";

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6: false))
		{
			foreach (var chargeType in chargeTypeDutyListWithoutProvisional)
			{
				lineFee.CF_ChargeType = chargeType;
				AssertEquals($"When Export Not UCC6 with Deferral account And ChargeType is {chargeType}", expectedDeferredPaymentCustomsProcedureF, lineFee.CF_MethodOfPayment);
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		var entryLine = declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		lineFee = entryLine.Fees.AddNew();
		invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		invoiceLine.JI_CL = entryLine.PK;
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(JobDeclaration declaration, bool isUCC6) => ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isUCC6);

	static readonly ImmutableHashSet<string> chargeTypeDutyListWithoutProvisional = ImmutableHashSet.Create(
		RefCusRateCodes.CustomsDutyOnIndustrialProducts,
		RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts,
		RefCusRateCodes.AdditionalDutyCountervailingSafeguardChargeVariableCharge,
		RefCusRateCodes.DefinitiveAntiDumpingDuty,
		RefCusRateCodes.DefinitiveCountervailingDuty
	);

	JobDeclaration declaration;
	JobComInvoiceLine invoiceLine;
	CusEntryLineFee lineFee;
}
