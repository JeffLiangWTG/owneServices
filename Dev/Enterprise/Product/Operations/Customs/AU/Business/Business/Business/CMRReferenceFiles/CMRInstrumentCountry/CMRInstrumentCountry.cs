
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCountry : AutoCMRInstrumentCountry
	{
		public CMRInstrumentCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRInstrumentCountry New(BusinessObjectFactory factory)
		{
			return factory.New<CMRInstrumentCountry>();
		}
	}
}
