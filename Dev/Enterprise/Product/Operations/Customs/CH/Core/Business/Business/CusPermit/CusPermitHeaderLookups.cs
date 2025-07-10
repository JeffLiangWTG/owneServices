using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Business;

public class CusPermitHeaderLookups : Customs.Business.BaseCusPermitHeaderLookups
{
	public CusPermitHeaderLookups(CusPermitHeader parent)
		: base(parent)
	{
	}

	public CodeDescriptionPairList UnitOfQuantityList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today);
}
