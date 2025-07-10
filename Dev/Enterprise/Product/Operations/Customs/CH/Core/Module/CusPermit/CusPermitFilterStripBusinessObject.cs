using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CH.Module;

public class CusPermitFilterStripBusinessObject : Customs.Module.CusPermitFilterStripBusinessObject
{
	protected override Customs.Module.PermitTypeModuleFilter GetPermitTypeModuleFilter(ZString description, Customs.Module.PermitTypeModuleFilter.GetPermitTypeQuery queryDelegate, GetList getCountries)
	{
		return new PermitTypeModuleFilter(description, queryDelegate, getCountries);
	}

	protected override MultilingualString PermitTypeFilterDescription => ResString.GetMultilingualString("CH.CusPermitFilter|PermitType", "Permit Type");
}
