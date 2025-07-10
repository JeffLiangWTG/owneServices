using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class CusEntryLineFeeValidationTest : EU.Business.Declaration.Testing.EUUniversalCusEntryLineFeeValidationTest<JobDeclaration, CusEntryLine, CusEntryLineFee>
{
	public void TestCheckCF_MethodOfPayment_BE0104()
	{
		const string errorMessage = "E or P for a duty or tax only allowed if 'Approval Defer. No' is filled in Misc tab. Please fill it there.";
		(JobDeclaration declaration, CusEntryLine entryLine, CusEntryLineFee entryLineFee) = SetEntryLineFeeData();
		declaration.JE_DefermentAccountNumber = ZString.Empty;
		declaration.SetImport();
		CombineAssertions(() =>
		{
			entryLineFee.CF_MethodOfPayment = PaymentMethodList.Codes.Deferral;
			TestCaseWithFactory.AssertHasMessageError(entryLineFee.CF_MethodOfPaymentInfo, errorMessage);

			entryLineFee.CF_MethodOfPayment = PaymentMethodList.Codes.AgentCashAccount;
			TestCaseWithFactory.AssertHasMessageError(entryLineFee.CF_MethodOfPaymentInfo, errorMessage);

			entryLineFee.CF_MethodOfPayment = PaymentMethodList.Codes.Cheque;
			TestCaseWithFactory.AssertNoMessageError(entryLineFee.CF_MethodOfPaymentInfo, errorMessage);

			declaration.JE_DefermentAccountNumber = "123";

			entryLineFee.CF_MethodOfPayment = PaymentMethodList.Codes.Deferral;
			TestCaseWithFactory.AssertNoMessageError(entryLineFee.CF_MethodOfPaymentInfo, errorMessage);

			entryLineFee.CF_MethodOfPayment = PaymentMethodList.Codes.AgentCashAccount;
			TestCaseWithFactory.AssertNoMessageError(entryLineFee.CF_MethodOfPaymentInfo, errorMessage);
		});
	}

	public override void TestCheckCF_ChargeType()
	{
		(JobDeclaration declaration, CusEntryLine entryLine, CusEntryLineFee entryLineFee) = SetEntryLineFeeData();
		entryLineFee.CF_ChargeType = ZString.Empty;
		AssertHasMessageErrorContaining(entryLineFee.CF_ChargeTypeInfo, "You have not entered a [UCC 4/3] Type.");
		entryLineFee.CF_ChargeType = "INV";
		AssertHasMessageErrorContaining(entryLineFee.CF_ChargeTypeInfo, "The code you have selected is not in the list.");
	}
}
