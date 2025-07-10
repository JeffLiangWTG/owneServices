
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffClassificationMessageAdvice : AutoCMRTariffClassificationMessageAdvice
	{
		public CMRTariffClassificationMessageAdvice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTariffClassificationMessageAdvice New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTariffClassificationMessageAdvice>();
		}
	}
}
