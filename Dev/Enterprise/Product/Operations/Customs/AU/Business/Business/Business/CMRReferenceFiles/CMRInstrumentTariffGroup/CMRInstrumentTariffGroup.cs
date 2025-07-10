
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentTariffGroup : AutoCMRInstrumentTariffGroup
	{
		public CMRInstrumentTariffGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRInstrumentTariffGroup New(BusinessObjectFactory factory)
		{
			return factory.New<CMRInstrumentTariffGroup>();
		}
	}
}
