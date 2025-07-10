
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRInstrumentMessageAdvice : AutoCMRInstrumentMessageAdvice
	{
		public CMRInstrumentMessageAdvice(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRInstrumentMessageAdvice New(BusinessObjectFactory factory)
		{
			return factory.New<CMRInstrumentMessageAdvice>();
		}
	}
}
