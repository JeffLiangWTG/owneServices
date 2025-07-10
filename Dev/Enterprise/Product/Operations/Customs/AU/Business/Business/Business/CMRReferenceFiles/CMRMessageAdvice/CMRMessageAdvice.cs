
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRMessageAdvice : AutoCMRMessageAdvice
	{
		public CMRMessageAdvice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRMessageAdvice New(BusinessObjectFactory factory)
		{
			return factory.New<CMRMessageAdvice>();
		}
	}
}
