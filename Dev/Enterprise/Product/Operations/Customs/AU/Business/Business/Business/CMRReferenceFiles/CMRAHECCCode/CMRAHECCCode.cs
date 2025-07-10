
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAHECCCode : AutoCMRAHECCCode
	{
		public CMRAHECCCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRAHECCCode New(BusinessObjectFactory factory)
		{
			return factory.New<CMRAHECCCode>();
		}
	}
}
