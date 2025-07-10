
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRAqisCommodity : AutoCMRAqisCommodity
	{
		public CMRAqisCommodity(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static CMRAqisCommodity New(BusinessObjectFactory factory)
		{
			return factory.New<CMRAqisCommodity>();
		}
	}
}
