using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration;

public partial class ExportJobDeclarationValidation
{
	protected override void CheckJE_SpecificCircumstanceIndicator()
	{
		base.CheckJE_SpecificCircumstanceIndicator();

		if (Parent.JE_TypeOfSecurity == ExportSecurityTypeList.Codes.NotUsed && Parent.JE_SpecificCircumstanceIndicator == NLSpecificCircumstanceIndicatorList.Codes.A20)
		{
			Parent.JE_SpecificCircumstanceIndicatorInfo.AddMessageError(Res.GetString("3FE7CC04-CD43-4725-A111-92D2E074CC4E", "Cannot use Special Circumstances '{0}' when Security is '{1}'", NLSpecificCircumstanceIndicatorList.Codes.A20, ExportSecurityTypeList.Codes.NotUsed));
		}
	}
}
