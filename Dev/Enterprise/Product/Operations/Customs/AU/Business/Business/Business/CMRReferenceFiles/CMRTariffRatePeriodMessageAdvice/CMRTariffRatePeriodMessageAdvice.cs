
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffRatePeriodMessageAdvice : AutoCMRTariffRatePeriodMessageAdvice
	{
		public CMRTariffRatePeriodMessageAdvice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTariffRatePeriodMessageAdvice New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTariffRatePeriodMessageAdvice>();
		}
	}
}
