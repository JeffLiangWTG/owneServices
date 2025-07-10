using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business.Declaration;

public partial class JobDeclarationValidation
{
	protected override void CheckJE_VATDeferType()
	{
		base.CheckJE_VATDeferType();
		ListValidation.MessageErrorIfInvalidCode(Parent.ZG_VATDeferTypeInfo);
	}
}
