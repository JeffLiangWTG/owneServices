
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentPreferenceScheme : AutoCMRInstrumentPreferenceScheme
	{
		public CMRInstrumentPreferenceScheme(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRInstrumentPreferenceScheme New(BusinessObjectFactory factory)
		{
			return factory.New<CMRInstrumentPreferenceScheme>();
		}
	}
}
