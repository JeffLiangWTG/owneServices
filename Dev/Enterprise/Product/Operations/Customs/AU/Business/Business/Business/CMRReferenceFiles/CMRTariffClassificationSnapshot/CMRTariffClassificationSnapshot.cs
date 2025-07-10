
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRTariffClassificationSnapshot : AutoCMRTariffClassificationSnapshot
	{
		public CMRTariffClassificationSnapshot(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRTariffClassificationSnapshot New(BusinessObjectFactory factory)
		{
			return factory.New<CMRTariffClassificationSnapshot>();
		}
	}
}
