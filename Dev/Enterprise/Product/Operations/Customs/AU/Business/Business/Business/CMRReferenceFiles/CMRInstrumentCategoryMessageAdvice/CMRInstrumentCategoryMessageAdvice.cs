
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentCategoryMessageAdvice : AutoCMRInstrumentCategoryMessageAdvice
	{
		public CMRInstrumentCategoryMessageAdvice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRInstrumentCategoryMessageAdvice New(BusinessObjectFactory factory)
		{
			return factory.New<CMRInstrumentCategoryMessageAdvice>();
		}
	}
}
