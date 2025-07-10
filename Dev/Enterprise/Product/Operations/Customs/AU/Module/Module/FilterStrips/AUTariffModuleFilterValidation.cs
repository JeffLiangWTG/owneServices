using System;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Module;

public class AUTariffModuleFilterValidation : ModuleFilterValidation
{
	public AUTariffModuleFilterValidation(AUTariffModuleFilter parent)
		: base(parent)
	{
	}

	public override void ValidateAll()
	{
	}

	public override Type AutoValidationType
	{
		get { return this.GetType(); }
	}
}
