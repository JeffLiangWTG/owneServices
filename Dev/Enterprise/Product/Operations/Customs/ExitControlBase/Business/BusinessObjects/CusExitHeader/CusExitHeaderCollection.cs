using CargoWise.EntityFramework;

namespace Enterprise.Customs.ExitControlBase.Business;

public class CusExitHeaderCollection<T> : ActiveBusinessObjectCollection<T>
	where T : CusExitHeader
{
	public CusExitHeaderCollection(BusinessObjectFactory factory, ZQuery filter)
		: base(factory, filter)
	{
	}

	public CusExitHeaderCollection(BusinessObjectFactory factory)
		: base(factory)
	{
	}
}
