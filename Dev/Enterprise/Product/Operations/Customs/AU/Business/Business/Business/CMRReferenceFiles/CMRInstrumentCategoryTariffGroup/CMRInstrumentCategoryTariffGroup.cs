
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCategoryTariffGroup : AutoCMRInstrumentCategoryTariffGroup
	{
		public CMRInstrumentCategoryTariffGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRInstrumentCategoryTariffGroup New(BusinessObjectFactory factory)
		{
			return factory.New<CMRInstrumentCategoryTariffGroup>();
		}
	}
}
