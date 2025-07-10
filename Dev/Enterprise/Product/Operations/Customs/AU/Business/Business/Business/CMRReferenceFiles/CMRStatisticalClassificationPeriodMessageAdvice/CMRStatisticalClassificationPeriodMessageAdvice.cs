
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRStatisticalClassificationPeriodMessageAdvice : AutoCMRStatisticalClassificationPeriodMessageAdvice
	{
		public CMRStatisticalClassificationPeriodMessageAdvice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRStatisticalClassificationPeriodMessageAdvice New(BusinessObjectFactory factory)
		{
			return factory.New<CMRStatisticalClassificationPeriodMessageAdvice>();
		}
	}
}
