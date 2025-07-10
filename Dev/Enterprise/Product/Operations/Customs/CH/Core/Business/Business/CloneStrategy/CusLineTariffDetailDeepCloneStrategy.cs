using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class CusLineTariffDetailDeepCloneStrategy : CustomsBusinessObjectCloneStrategy
{
	public CusLineTariffDetailDeepCloneStrategy(BusinessObject bizObjToClone, CloneType cloneType) : base(bizObjToClone, cloneType)
	{
	}
}
