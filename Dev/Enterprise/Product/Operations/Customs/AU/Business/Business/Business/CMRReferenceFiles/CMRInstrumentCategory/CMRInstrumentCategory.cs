
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCategory : AutoCMRInstrumentCategory
	{
		public CMRInstrumentCategory(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRInstrumentCategory New(BusinessObjectFactory factory)
		{
			return factory.New<CMRInstrumentCategory>();
		}
	}
}
