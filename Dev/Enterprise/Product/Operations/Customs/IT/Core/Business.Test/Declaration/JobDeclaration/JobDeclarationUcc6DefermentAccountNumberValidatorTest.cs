using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class JobDeclarationUcc6DefermentAccountNumberValidatorTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When declaration is null", () => new JobDeclarationGoodsLocationAddressValidation(jobDeclaration: null));
		AssertNoExceptionThrown("When declaration is not null", () => new JobDeclarationGoodsLocationAddressValidation(declaration));
	}

	public void TestCheckJE_DefermentAccountNumber_ForMethodOfPaymentIfRequiredExport()
	{
		const string expectedMessage = "[CN0558] The selected method of payment in Entries > Entry Lines > Tax or Fee requires to enter an Approval Defer No.";

		declaration.JE_MessageType = MessageTypeList.Codes.Export;

		var defermentAccountNumberInfo = declaration.JE_DefermentAccountNumberInfo;
		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(false))
		{
			AssertDefermentAccountNumberUCC5(UniversalReferenceConstants.DutyMethodOfPayment.OthersD, defermentAccountNumberInfo, expectedMessage);
			AssertDefermentAccountNumberUCC5(UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentE, defermentAccountNumberInfo, expectedMessage);
			AssertDefermentAccountNumberUCC5(UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentVatProcedureG, defermentAccountNumberInfo, expectedMessage);
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(true))
		{
			AssertDefermentAccountNumberUCC6(UniversalReferenceConstants.DutyMethodOfPayment.OthersD, defermentAccountNumberInfo, expectedMessage);
			AssertDefermentAccountNumberUCC6(UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentE, defermentAccountNumberInfo, expectedMessage);
			AssertDefermentAccountNumberUCC6(UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentVatProcedureG, defermentAccountNumberInfo, expectedMessage);

			feeLine.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.AgentGeneralGuaranteeAccountT;
			declaration.JE_DefermentAccountNumber = ZString.Empty;
			AssertNoWarning("When JE_DefermentAccountNumber is Empty and MoP is Not in [D, E, G]", defermentAccountNumberInfo, expectedMessage);
		}
	}

	public void TestCheckJE_DefermentAccountNumber_ForMethodOfPaymentIfRequiredImport()
	{
		const string expectedMessage = "The selected method of payment in Entries > Entry Lines > Tax or Fee requires to enter an Approval Defer No.";
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var defermentAccountNumberInfo = declaration.JE_DefermentAccountNumberInfo;

		AssertDefermentAccountNumberUCC6(UniversalReferenceConstants.DutyMethodOfPayment.OthersD, defermentAccountNumberInfo, expectedMessage);
		AssertDefermentAccountNumberUCC6(UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentE, defermentAccountNumberInfo, expectedMessage);
		AssertDefermentAccountNumberUCC6(UniversalReferenceConstants.DutyMethodOfPayment.DeferredPaymentVatProcedureG, defermentAccountNumberInfo, expectedMessage);

		feeLine.CF_MethodOfPayment = UniversalReferenceConstants.DutyMethodOfPayment.AgentGeneralGuaranteeAccountT;
		declaration.JE_DefermentAccountNumber = ZString.Empty;
		AssertNoMessageError("When JE_DefermentAccountNumber is Empty and MoP is Not in [D, E, G]", defermentAccountNumberInfo, expectedMessage);
	}

	public void TestCheckJE_DefermentAccountNumberFillingWithMethodOfPayment_WhenMustBeEmpty()
	{
		var expectedMessage = "This field must be empty if no 'Method of Payment' is [D], [E] or [G]";
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		var entryLine = entryHeader.MergedLines.AddNew();
		var feeLine = entryLine.Fees.AddNew();

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: false))
		{
			CombineAssertions("For Non Ucc6 Export", () =>
			{
				feeLine.CF_MethodOfPayment = "A";
				declaration.JE_DefermentAccountNumber = "123456A";
				AssertNoMessageErrorContaining("For CF_MethodOfPayment = A, JE_DefermentAccountNumber filled", declaration.JE_DefermentAccountNumberInfo, expectedMessage);
			});
		}

		using (TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(isActive: true))
		{
			CombineAssertions("For Ucc6 Export", () =>
			{
				feeLine.CF_MethodOfPayment = "A";
				declaration.Validation.ValidateJE_DefermentAccountNumber();
				AssertHasMessageErrorContaining("For CF_MethodOfPayment not In [D E G], JE_DefermentAccountNumber filled", declaration.JE_DefermentAccountNumberInfo, expectedMessage);

				feeLine.CF_MethodOfPayment = "D";
				declaration.Validation.ValidateJE_DefermentAccountNumber();
				AssertNoMessageErrorContaining("For CF_MethodOfPayment = D, JE_DefermentAccountNumber filled", declaration.JE_DefermentAccountNumberInfo, expectedMessage);

				feeLine.CF_MethodOfPayment = "E";
				declaration.Validation.ValidateJE_DefermentAccountNumber();
				AssertNoMessageErrorContaining("For CF_MethodOfPayment = E, JE_DefermentAccountNumber filled", declaration.JE_DefermentAccountNumberInfo, expectedMessage);

				feeLine.CF_MethodOfPayment = "G";
				declaration.Validation.ValidateJE_DefermentAccountNumber();
				AssertNoMessageErrorContaining("For CF_MethodOfPayment = G, JE_DefermentAccountNumber filled", declaration.JE_DefermentAccountNumberInfo, expectedMessage);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		feeLine = (CusEntryLineFee)declaration.ActiveEntryHeaders.AddNew().MergedLines.AddNew().Fees.AddNew();
	}

	void AssertDefermentAccountNumberUCC6(string methodOfPayment, ZPropertyInfo defermentAccountNumberInfo, string expectedMessage)
	{
		feeLine.CF_MethodOfPayment = methodOfPayment;
		declaration.JE_DefermentAccountNumber = ZString.Empty;
		AssertHasMessageError($"When UCC6 JE_DefermentAccountNumber is Empty and MoP is {methodOfPayment}", defermentAccountNumberInfo, expectedMessage);
		declaration.JE_DefermentAccountNumber = "DAT123";
		AssertNoMessageError($"When UCC6 JE_DefermentAccountNumber is NOT Empty and MoP is {methodOfPayment}", defermentAccountNumberInfo, expectedMessage);
	}

	void AssertDefermentAccountNumberUCC5(string methodOfPayment, ZPropertyInfo defermentAccountNumberInfo, string expectedMessage)
	{
		feeLine.CF_MethodOfPayment = methodOfPayment;
		declaration.JE_DefermentAccountNumber = ZString.Empty;
		AssertNoMessageError($"When NOT UCC6 JE_DefermentAccountNumber is Empty and MoP is {feeLine.CF_MethodOfPayment}", defermentAccountNumberInfo, expectedMessage);
	}

	IDisposable TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(bool isActive)
		=> ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, isActive);

	JobDeclaration declaration;

	CusEntryLineFee feeLine;
}
