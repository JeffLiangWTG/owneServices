using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business;

public class PermitCountrySpecificInstruction : Customs.Business.PermitCountrySpecificInstruction
{
	public PermitCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
	{
	}

	public override bool IsQtyValIndicatorMandatory => false;

	public override PermitTypeList GetTypeList()
	{
		var date = ZDateTime.Today;
		return Factory.GetCachedValue("CH.PermitCountrySpecificInstruction.PermitTypeList." + date, () =>
		{
			var codeList = RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Switzerland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PermitAuthority, date);
			var typeList = new PermitTypeList();
			typeList.AddRange(codeList);
			return typeList;
		});
	}
}
