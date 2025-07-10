using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CH.GUI;

public static class CheckCreditGUIHelper
{
	public static bool CheckCredit(this JobDeclaration jobDeclaration)
	{
		var shouldContinue = jobDeclaration.CheckCredit(out var reasonForNotAllowed);
		if (!shouldContinue)
		{
			Globals.Message.ShowError(reasonForNotAllowed);
		}
		return shouldContinue;
	}
}
