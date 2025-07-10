using CargoWise.EntityFramework;
using Enterprise.Customs.ExitControlBase.Business;

namespace Enterprise.Customs.CH.DeclarationActivation.Business;

public class DeclarationActivationHeaderValidation(DeclarationActivationHeader parent) : CusExitHeaderValidation(parent)
{
	protected override void CheckCXH_GS_NKCustomsAgent()
	{
		base.CheckCXH_GS_NKCustomsAgent();
		ListValidation.MessageErrorIfInvalidCode(Parent.CXH_GS_NKCustomsAgentInfo);
	}
}
