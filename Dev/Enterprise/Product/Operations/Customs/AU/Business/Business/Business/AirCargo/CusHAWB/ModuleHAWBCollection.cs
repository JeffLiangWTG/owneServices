using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business;

public class ModuleHAWBCollection : BusinessObjectCollection<CusHAWB>
{
	public ModuleHAWBCollection(BusinessObjectFactory factory)
		: base(factory)
	{
	}
}
