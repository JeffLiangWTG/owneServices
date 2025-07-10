
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCategoryCountry : AutoCMRInstrumentCategoryCountry
	{
		public CMRInstrumentCategoryCountry(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRInstrumentCategoryCountry New(BusinessObjectFactory factory)
		{
			return factory.New<CMRInstrumentCategoryCountry>();
		}
	}
}
