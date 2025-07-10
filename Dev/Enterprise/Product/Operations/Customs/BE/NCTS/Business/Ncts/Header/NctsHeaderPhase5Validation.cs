using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.NCTS.Business;

public class NctsHeaderPhase5Validation : EU.NCTS.Business.NctsHeaderPhase5Validation
{
	public NctsHeaderPhase5Validation(NctsHeader parent) : base(parent)
	{
	}

	protected override void CheckBH_CommunicationLanguage()
	{
		base.CheckBH_CommunicationLanguage();
		ListValidation.ErrorIfInvalidCode(Parent.BH_CommunicationLanguageInfo);
	}
}
