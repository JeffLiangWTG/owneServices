using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class UnderbondCusStatus : CalculatedCusStatus
	{
		public UnderbondCusStatus(ZPropertyInfo wrappedPropertyInfo, ICalculatedCusStatusCalculator calculator)
			: base(wrappedPropertyInfo, calculator, wrappedPropertyInfo.BizObj.Factory.GetCachedValue<CMRUnderbondStatuses>())
		{
		}
	}
}
