using CargoWise.EntityFramework;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CargoAndUnderbondCusStatus : CalculatedCusStatus
	{
		public CargoAndUnderbondCusStatus(ZPropertyInfo wrappedPropertyInfo, ICalculatedCusStatusCalculator calculator)
			: base(wrappedPropertyInfo, calculator, CMRConsolidatedCargoAndUnderbondStatuses.GetStatuses(wrappedPropertyInfo.BizObj.Factory))
		{
		}
	}
}
