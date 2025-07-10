
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffClassificationConcordance : AutoCMRTariffClassificationConcordance
	{
		public CMRTariffClassificationConcordance(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTariffClassificationConcordance New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTariffClassificationConcordance>();
		}
	}
}
