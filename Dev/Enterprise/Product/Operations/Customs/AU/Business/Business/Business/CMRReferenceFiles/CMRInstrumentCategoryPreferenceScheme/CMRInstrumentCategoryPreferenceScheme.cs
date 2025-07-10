
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCategoryPreferenceScheme : AutoCMRInstrumentCategoryPreferenceScheme
	{
		public CMRInstrumentCategoryPreferenceScheme(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRInstrumentCategoryPreferenceScheme New(BusinessObjectFactory factory)
		{
			return factory.New<CMRInstrumentCategoryPreferenceScheme>();
		}
	}
}
