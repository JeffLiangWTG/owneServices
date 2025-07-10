
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTreatmentRatePeriodMessageAdvice : AutoCMRTreatmentRatePeriodMessageAdvice
	{
		public CMRTreatmentRatePeriodMessageAdvice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTreatmentRatePeriodMessageAdvice New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTreatmentRatePeriodMessageAdvice>();
		}
	}
}
