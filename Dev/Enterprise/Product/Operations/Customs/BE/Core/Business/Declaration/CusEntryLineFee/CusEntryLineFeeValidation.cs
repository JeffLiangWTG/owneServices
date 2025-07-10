using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.BE.Business.Declaration;

public class CusEntryLineFeeValidation : EUUniversalCusEntryLineFeeValidation
{
	public CusEntryLineFeeValidation(AutoCusEntryLineFee parent)
		: base(parent)
	{
	}

	protected override void CheckCF_MethodOfPayment()
	{
		base.CheckCF_MethodOfPayment();
		var methodOfPayment = Parent.CF_MethodOfPayment;
		if (Parent.EntryLine.Declaration is JobDeclaration declaration && declaration.IsImport && declaration.JE_DefermentAccountNumber.IsEmpty
			&& (methodOfPayment == PaymentMethodList.Codes.Deferral || methodOfPayment == PaymentMethodList.Codes.AgentCashAccount))
		{
			Parent.CF_MethodOfPaymentInfo.AddMessageError(Res.GetString("CA38CF19-7065-400A-8F18-E318873A465B", "E or P for a duty or tax only allowed if 'Approval Defer. No' is filled in Misc tab. Please fill it there."));
		}
	}
}
