using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public static class CheckCreditHelper
{
	public static bool CheckCredit(this JobDeclaration jobDeclaration, out string reasonForNotAllowed)
	{
		reasonForNotAllowed = string.Empty;

		var helper = new MessageManagerCreditCheckWithSecurityHelper(jobDeclaration);
		var shouldContinue = helper.IsCreditCheckOKToSend;
		if (!shouldContinue && !helper.IsCreditCheckDoneOutsideCW1)
		{
			reasonForNotAllowed = helper.ReasonForNotAllowed;
		}

		return shouldContinue;
	}
}
