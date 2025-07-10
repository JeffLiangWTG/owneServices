using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Module
{
	public class CusPermitFilterStripBusinessObject : Customs.Module.CusPermitFilterStripBusinessObject
	{
		protected override Customs.Module.PermitTypeModuleFilter GetPermitTypeModuleFilter(ZString description, Customs.Module.PermitTypeModuleFilter.GetPermitTypeQuery queryDelegate, GetList getCountries)
		{
			return new PermitTypeModuleFilter(description, queryDelegate, getCountries);
		}
	}
}
